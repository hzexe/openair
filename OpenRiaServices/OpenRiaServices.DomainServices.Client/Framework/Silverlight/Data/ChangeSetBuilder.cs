using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Takes a <see cref="EntityChangeSet"/> and builds the corresponding set of 
    /// <see cref="ChangeSetEntry"/> instances that should be sent to the server.
    /// </summary>
    internal class ChangeSetBuilder
    {
        /// <summary>
        /// Builds an operation list for the specified <see cref="EntityChangeSet"/>.
        /// </summary>
        /// <param name="changeSet">The <see cref="EntityChangeSet"/>.</param>
        /// <returns>The list of <see cref="ChangeSetEntry"/> for the specified <see cref="EntityChangeSet"/>.</returns>
        public static List<ChangeSetEntry> Build(EntityChangeSet changeSet)
        {
            CheckForInvalidUpdates(changeSet);

            // translate to an operation list
            List<ChangeSetEntry> operations = BuildOperations(changeSet);

            // recursively visit all composition relationships in the
            // changeset and add operations for unmodified children.
            UnmodifiedOperationAdder.Add(operations);

            // set the association maps for all operations in the changeset
            AssociationMapBuilder.Build(operations);

            return operations;
        }

        /// <summary>
        /// Builds the list of submit operations from the current <see cref="EntityChangeSet"/>.
        /// </summary>
        /// <param name="changeSet">The <see cref="EntityChangeSet"/> to process.</param>
        /// <returns>The list of <see cref="ChangeSetEntry"/> for the specified <see cref="EntityChangeSet"/>.</returns>
        private static List<ChangeSetEntry> BuildOperations(EntityChangeSet changeSet)
        {
            List<ChangeSetEntry> operations = new List<ChangeSetEntry>();
            int clientID = 0;
            EntityOperationType operationType = EntityOperationType.None;
            foreach (Entity entity in changeSet)
            {
                switch (entity.EntityState)
                {
                    case EntityState.New:
                        operationType = EntityOperationType.Insert;
                        break;
                    case EntityState.Modified:
                        operationType = EntityOperationType.Update;
                        break;
                    case EntityState.Deleted:
                        operationType = EntityOperationType.Delete;
                        break;
                    default:
                        continue;
                }

                // create the operation and apply any original values
                ChangeSetEntry changeSetEntry = new ChangeSetEntry(entity, clientID++, operationType);

                if (entity.OriginalValues != null)
                {
                    if (entity.MetaType.ShouldRoundtripOriginal && entity.OriginalValues != null)
                    {
                        changeSetEntry.OriginalEntity = GetRoundtripEntity(entity);
                    }
                    else
                    {
                        // In cases where the entity is modified but we're not sending
                        // an original we need to flag the entity as having changes.
                        // For example, this happens in Timestamp scenarios.
                        changeSetEntry.HasMemberChanges = true;
                    }
                }

                // add any custom method invocations
                var entityActions = (ICollection<EntityAction>)entity.EntityActions;
                foreach (EntityAction customInvokation in entityActions)
                {
                    if (string.IsNullOrEmpty(customInvokation.Name))
                    {
                        throw new ArgumentException(Resource.DomainClient_InvocationNameCannotBeNullOrEmpty);
                    }

                    if (changeSetEntry.EntityActions == null)
                    {
                        changeSetEntry.EntityActions = new List<Serialization.KeyValue<string, object[]>>();
                    }
                    changeSetEntry.EntityActions.Add(
                        new Serialization.KeyValue<string, object[]>(customInvokation.Name, customInvokation.Parameters.ToArray()));
                }

                operations.Add(changeSetEntry);
            }

            return operations;
        }

        internal static Entity GetRoundtripEntity(Entity entity)
        {
            Entity roundtripEntity = (Entity)Activator.CreateInstance(entity.GetType());
            IDictionary<string, object> roundtripState = ObjectStateUtility.ExtractRoundtripState(entity.GetType(), entity.OriginalValues);
            roundtripEntity.ApplyState(roundtripState);

            return roundtripEntity;
        }

        /// <summary>
        /// Verify that all update operations in the specified <see cref="EntityChangeSet"/> are permitted.
        /// </summary>
        /// <param name="changeSet">The <see cref="EntityChangeSet"/> to check.</param>
        internal static void CheckForInvalidUpdates(EntityChangeSet changeSet)
        {
            AssociationUpdateChecker associationChecker = new AssociationUpdateChecker();

            foreach (Entity entity in changeSet)
            {
                if (entity.EntityState == EntityState.Modified)
                {
                    foreach (PropertyInfo modifiedProperty in entity.ModifiedProperties)
                    {
                        if (modifiedProperty.GetCustomAttributes(typeof(KeyAttribute), false).Any())
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.Entity_KeyMembersCannotBeChanged, modifiedProperty.Name, entity.GetType().Name));
                        }
                    }
                }

                // search associated entities for any invalid changes
                associationChecker.Visit(entity);
            }
        }

        #region Nested Types

        /// <summary>
        /// This visitor recursively visits all composed entities in a
        /// changeset and adds ChangeSetEntries for Unmodified entities.
        /// Note that this visitor forces all compositional associations
        /// to load.
        /// </summary>
        internal class UnmodifiedOperationAdder : EntityVisitor
        {
            private Dictionary<object, bool> _visited = new Dictionary<object, bool>();
            private List<ChangeSetEntry> _changeSetEntries;
            private int _id;
            private bool _isChild;

            public UnmodifiedOperationAdder(List<ChangeSetEntry> changeSetEntries)
            {
                this._changeSetEntries = changeSetEntries;
                if (this._changeSetEntries.Any())
                {
                    this._id = this._changeSetEntries.Max(p => p.Id) + 1;
                }
            }

            public static void Add(List<ChangeSetEntry> changeSetEntries)
            {
                Entity[] entities = changeSetEntries.Where(p => p.Entity.MetaType.HasComposition).Select(p => p.Entity).ToArray();
                if (entities.Length == 0)
                {
                    return;
                }

                UnmodifiedOperationAdder visitor = new UnmodifiedOperationAdder(changeSetEntries);
                foreach (Entity entity in entities)
                {
                    visitor.Visit(entity);
                }
            }

            public override void Visit(Entity entity)
            {
                if (this._visited.ContainsKey(entity))
                {
                    return;
                }

                // if the entity is unmodified, add an ChangeSetEntry for it
                if (this._isChild && entity.EntityState == EntityState.Unmodified)
                {
                    ChangeSetEntry op = new ChangeSetEntry(entity, this._id++, EntityOperationType.None);
                    this._changeSetEntries.Add(op);
                }

                this._visited.Add(entity, true);

                base.Visit(entity);
            }

            protected override void VisitEntityCollection(IEntityCollection entityCollection, PropertyInfo propertyInfo)
            {
                if (propertyInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Any())
                {
                    bool lastIsChild = this._isChild;
                    this._isChild = true;
                    IEnumerable<Entity> children = entityCollection.Entities;
                    foreach (Entity child in children)
                    {
                        this.Visit(child);
                    }
                    this._isChild = lastIsChild;
                }
            }

            protected override void VisitEntityRef(IEntityRef entityRef, Entity parent, PropertyInfo propertyInfo)
            {
                if (propertyInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Any())
                {
                    Entity child = null;
                    if (entityRef == null)
                    {
                        // If the EntityRef hasn't been accesssed before, it might
                        // not be initialized yet. In this case we need to access
                        // the property directly to force it to load.
                        child = (Entity)propertyInfo.GetValue(parent, null);
                    }
                    else
                    {
                        child = entityRef.Entity;
                    }

                    if (child != null)
                    {
                        bool lastIsChild = this._isChild;
                        this._isChild = true;

                        this.Visit(child);

                        this._isChild = lastIsChild;
                    }
                }
            }
        }

        /// <summary>
        /// Visits all association members of entities in a changeset, setting the
        /// association maps for each. Association info is populated only in the
        /// following instances:
        /// - The association is a composition
        /// - The association is loaded and the referenced entities are New and the parent is not deleted
        /// </summary>
        internal class AssociationMapBuilder : EntityVisitor
        {
            private Dictionary<Entity, int> _entityIdMap;
            private ChangeSetEntry _currentChangeSetEntry;
            private List<ChangeSetEntry> _changeSetEntries;

            /// <summary>
            /// Build the association maps for the specified change set entries.
            /// </summary>
            /// <param name="changeSetEntries">The changeset operations</param>
            public static void Build(List<ChangeSetEntry> changeSetEntries)
            {
                new AssociationMapBuilder(changeSetEntries).Build();
            }

            /// <summary>
            /// Private constructor
            /// </summary>
            /// <param name="changeSetEntries">The changeset operations</param>
            private AssociationMapBuilder(List<ChangeSetEntry> changeSetEntries)
            {
                this._entityIdMap = changeSetEntries.ToDictionary(p => p.Entity, p => p.Id);
                this._changeSetEntries = changeSetEntries;
            }

            /// <summary>
            /// Build and set the association maps for all operations in the specified changeset
            /// </summary>
            private void Build()
            {
                foreach (var entityGroup in this._changeSetEntries.GroupBy(p => p.Entity.GetType()))
                {
                    foreach (ChangeSetEntry changeSetEntry in entityGroup)
                    {
                        this._currentChangeSetEntry = changeSetEntry;
                        this.Visit(changeSetEntry.Entity);
                    }
                }
            }

            protected override void VisitEntityCollection(IEntityCollection entityCollection, PropertyInfo propertyInfo)
            {
                // Check for [ExternalReference] properties, if found we can skip visiting these
                // external entities so they will not be included in our change set.
                if (propertyInfo.GetCustomAttributes(typeof(ExternalReferenceAttribute), true).Any())
                {
                    return;
                }

                bool isComposition = propertyInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Any();
                List<int> currentIds = new List<int>();
                List<int> originalIds = new List<int>();

                // Set associations for any new entities that have been explicitly added to the
                // entity reference (e.g. Order/OrderDetail scenario) and are part of this changeset,
                // or all entities if this is a composition. Note that the UnmodifiedOperationAdder
                // has already caused compositional associations to load, so they will have values.
                if (entityCollection.HasValues)
                {
                    foreach (Entity entity in entityCollection.Entities)
                    {
                        int id;
                        if (this._entityIdMap.TryGetValue(entity, out id))
                        {
                            // add the entity to the current associations if this is a composition
                            // or if the child is New and the parent is not Deleted.
                            bool shouldIncludeNewAssociation = this._currentChangeSetEntry.Entity.EntityState != EntityState.Deleted && entity.EntityState == EntityState.New;
                            if (isComposition || shouldIncludeNewAssociation)
                            {
                                currentIds.Add(id);
                            }

                            if (isComposition && (entity.EntityState != EntityState.New))
                            {
                                // for compositions, we need to add all non-new entities to
                                // the original associations map
                                originalIds.Add(id);
                            }
                        }
                    }
                }

                // For compositions, set original associations for any removed entities that are part of this changeset
                if (isComposition)
                {
                    int[] associatedIds = this.FindOriginalChildren(entityCollection.Association).Select(p => p.Id).ToArray();
                    if (associatedIds.Length > 0)
                    {
                        originalIds.AddRange(associatedIds.ToArray());
                    }
                }

                if (currentIds.Count > 0)
                {
                    Dictionary<string, int[]> associatedEntities = (Dictionary<string, int[]>)this._currentChangeSetEntry.Associations;
                    if (associatedEntities == null)
                    {
                        associatedEntities = new Dictionary<string, int[]>();
                        this._currentChangeSetEntry.Associations = associatedEntities;
                    }
                    associatedEntities.Add(propertyInfo.Name, currentIds.ToArray());
                }

                if (originalIds.Count > 0)
                {
                    Dictionary<string, int[]> associatedEntities = (Dictionary<string, int[]>)this._currentChangeSetEntry.OriginalAssociations;
                    if (associatedEntities == null)
                    {
                        associatedEntities = new Dictionary<string, int[]>();
                        this._currentChangeSetEntry.OriginalAssociations = associatedEntities;
                    }
                    associatedEntities.Add(propertyInfo.Name, originalIds.ToArray());
                }
            }

            /// <summary>
            /// For the current ChangeSetEntry, this method returns all other entries in the changeset
            /// whose entity was previously a child in the specified composition association.
            /// </summary>
            /// <param name="association">The association to check.</param>
            /// <returns>The resulting collection of entries.</returns>
            private IEnumerable<ChangeSetEntry> FindOriginalChildren(AssociationAttribute association)
            {
                foreach (ChangeSetEntry entry in this._changeSetEntries.Where(p => p.Entity.EntityState == EntityState.Deleted))
                {
                    Entity parent = entry.Entity.Parent;
                    if (parent == null)
                    {
                        // not a child entity
                        continue;
                    }

                    if (parent == this._currentChangeSetEntry.Entity &&
                        entry.Entity.ParentAssociation.Name == association.Name)
                    {
                        // if the current entity is the original parent and the association
                        // matches, return the entry
                        yield return entry;
                    }
                }
            }

            protected override void VisitEntityRef(IEntityRef entityRef, Entity parent, PropertyInfo propertyInfo)
            {
                // Check for [ExternalReference] properties, if found we can skip visiting these
                // external entities so they will not be included in our change set.
                if (propertyInfo.GetCustomAttributes(typeof(ExternalReferenceAttribute), true).Any())
                {
                    return;
                }

                // We don't want to cause any deferred loading of the EntityRef in non-compositional
                // cases. Note that the UnmodifiedOperationAdder has already caused compositional
                // associations to load, so they will have values.
                Entity referenced = null;
                if (entityRef != null && entityRef.HasValue)
                {
                    referenced = entityRef.Entity;
                }

                // Now determine the originally referenced entity if this association is a composition
                // and the child has been removed
                Entity prevReferenced = null;
                bool isComposition = propertyInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Any();
                if (isComposition && parent.EntityState != EntityState.New)
                {
                    AssociationAttribute assocAttrib = (AssociationAttribute)propertyInfo.GetCustomAttributes(typeof(AssociationAttribute), false).Single();
                    ChangeSetEntry entry = this.FindOriginalChildren(assocAttrib).SingleOrDefault();
                    if (entry != null)
                    {
                        prevReferenced = entry.Entity;
                    }
                }

                if (isComposition && prevReferenced == null)
                {
                    // if this is an unmodified composition, set the previously referenced entity
                    // to the currently referenced entity
                    prevReferenced = referenced;
                }

                // If the referenced entity is New and the parent is not Deleted or the referenced entity is a composed child and the target
                // is part of the changeset, set the association
                int refId = -1;
                bool shouldIncludeNewAssociation = referenced != null && (this._currentChangeSetEntry.Entity.EntityState != EntityState.Deleted && referenced.EntityState == EntityState.New);
                if (referenced != null && (shouldIncludeNewAssociation || isComposition) && this._entityIdMap.TryGetValue(referenced, out refId))
                {
                    // set the current reference
                    Dictionary<string, int[]> associatedEntities = (Dictionary<string, int[]>)this._currentChangeSetEntry.Associations;
                    if (associatedEntities == null)
                    {
                        associatedEntities = new Dictionary<string, int[]>();
                        this._currentChangeSetEntry.Associations = associatedEntities;
                    }
                    associatedEntities.Add(propertyInfo.Name, new int[] { refId });
                }

                // If the association is a composition, set the original reference
                if (prevReferenced != null && isComposition && this._entityIdMap.TryGetValue(prevReferenced, out refId))
                {
                    Dictionary<string, int[]> associatedEntities = (Dictionary<string, int[]>)this._currentChangeSetEntry.OriginalAssociations;
                    if (associatedEntities == null)
                    {
                        associatedEntities = new Dictionary<string, int[]>();
                        this._currentChangeSetEntry.OriginalAssociations = associatedEntities;
                    }
                    associatedEntities.Add(propertyInfo.Name, new int[] { refId });
                }
            }
        }

        /// <summary>
        /// Class used to perform update validation on association members.
        /// </summary>
        internal class AssociationUpdateChecker : EntityVisitor
        {
            protected override void VisitEntityCollection(IEntityCollection entityCollection, PropertyInfo propertyInfo)
            {
                // look for any invalid updates made to composed children
                if (propertyInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Length == 1 && entityCollection.HasValues)
                {
                    AssociationAttribute assoc = (AssociationAttribute)propertyInfo.GetCustomAttributes(typeof(AssociationAttribute), false).SingleOrDefault();
                    foreach (Entity childEntity in entityCollection.Entities)
                    {
                        CheckInvalidChildUpdates(childEntity, assoc);
                    }
                }
            }

            protected override void VisitEntityRef(IEntityRef entityRef, Entity parent, PropertyInfo propertyInfo)
            {
                // we don't want to cause any deferred loading of the EntityRef
                // in non-compositional cases
                Entity entity = null;
                if (entityRef != null && entityRef.HasValue)
                {
                    entity = entityRef.Entity;
                }

                // look for any invalid updates made to composed children
                if (entity != null && propertyInfo.GetCustomAttributes(typeof(CompositionAttribute), false).Length == 1)
                {
                    AssociationAttribute assoc = (AssociationAttribute)propertyInfo.GetCustomAttributes(typeof(AssociationAttribute), false).SingleOrDefault();
                    CheckInvalidChildUpdates(entity, assoc);
                }
            }

            /// <summary>
            /// Verify that the specified entity does not have any modified FK members for the
            /// specified composition.
            /// </summary>
            /// <param name="entity">The child entity to check.</param>
            /// <param name="compositionAttribute">The composition attribute.</param>
            private static void CheckInvalidChildUpdates(Entity entity, AssociationAttribute compositionAttribute)
            {
                if (compositionAttribute == null)
                {
                    return;
                }

                if (entity.EntityState == EntityState.Modified &&
                    entity.ModifiedProperties.Select(p => p.Name).Intersect(compositionAttribute.OtherKeyMembers).Any())
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resource.Entity_CantReparentComposedChild, entity));
                }
            }
        }

        #endregion // Nested Types
    }
}
