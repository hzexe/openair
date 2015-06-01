using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace OpenRiaServices.DomainServices.Client
{
    /// <summary>
    /// Utility class for methods related to object state management.
    /// </summary>
    internal static class ObjectStateUtility
    {
        /// <summary>
        /// Extracts the state of the specified object into a dictionary of name/value
        /// pairs.
        /// </summary>
        /// <param name="o">The object to extract state for.</param>
        /// <returns>The state dictionary.</returns>
        internal static IDictionary<string, object> ExtractState(object o)
        {
            return ExtractState(o, new HashSet<object>());
        }

        private static IDictionary<string, object> ExtractState(object o, HashSet<object> visited)
        {
            MetaType metaType = MetaType.GetMetaType(o.GetType());
            Dictionary<string, object> extractedState = new Dictionary<string, object>();

            if (visited.Contains(o))
            {
                throw new InvalidOperationException(Resource.CyclicReferenceError);
            }
            else
            {
                visited.Add(o);
            }

            foreach (MetaMember metaMember in metaType.DataMembers)
            {
                object value = metaMember.GetValue(o);

                if (value != null && metaMember.IsComplex && !metaMember.IsCollection)
                {
                    value = ExtractState(value, visited);
                }

                extractedState[metaMember.Member.Name] = value;
            }

            return extractedState;
        }

        /// <summary>
        /// Applies the specified state to the specified object.
        /// </summary>
        /// <param name="o">The object to apply state to.</param>
        /// <param name="stateToApply">The state dictionary.</param>
        internal static void ApplyState(object o, IDictionary<string, object> stateToApply)
        {
            ObjectStateUtility.ApplyState(o, stateToApply, null, LoadBehavior.RefreshCurrent);
        }

        /// <summary>
        /// Applies the specified state to the specified object taking the specified LoadBehavior into account.
        /// </summary>
        /// <param name="o">The object to apply state to.</param>
        /// <param name="stateToApply">The state dictionary.</param>
        /// <param name="originalState">The original state map for the modified object.</param>
        /// <param name="loadBehavior">The LoadBehavior to govern property merge behavior.</param>
        internal static void ApplyState(object o, IDictionary<string, object> stateToApply, IDictionary<string, object> originalState, LoadBehavior loadBehavior)
        {
            if (loadBehavior == LoadBehavior.KeepCurrent)
            {
                return;
            }

            MetaType metaType = MetaType.GetMetaType(o.GetType());

            bool isMerging = (o as Entity) == null ? (o as ComplexObject) != null && (o as ComplexObject).IsMergingState : (o as Entity).IsMergingState; 

            foreach (MetaMember metaMember in metaType.DataMembers)
            {
                PropertyInfo propertyInfo = metaMember.Member;
                object newValue;

                if ((isMerging && metaMember.IsMergable || !isMerging) && stateToApply.TryGetValue(propertyInfo.Name, out newValue))
                {
                    if (newValue != null && metaMember.IsComplex && !metaMember.IsCollection)
                    {
                        object currValue = propertyInfo.GetValue(o, null);
                        IDictionary<string, object> newValueState = (IDictionary<string, object>)newValue;
                        if (currValue != null)
                        {
                            // if the current and new values are both non-null, we have to do a merge
                            object complexTypeOriginalValues = null;
                            if (originalState != null)
                            {
                                originalState.TryGetValue(propertyInfo.Name, out complexTypeOriginalValues);
                            }
                            ApplyState(currValue, newValueState, (IDictionary<string, object>)complexTypeOriginalValues, loadBehavior);
                        }
                        else
                        {
                            // We do this lazy to avoid rehydrating the instance if we don't have to
                            Lazy<object> lazy = new Lazy<object>(
                                delegate
                                {
                                    // Rehydrate an instance from the state dictionary.
                                    object newInstance = Activator.CreateInstance(propertyInfo.PropertyType);
                                    ApplyState(newInstance, newValueState);
                                    return newInstance;
                                });
                            ApplyValue(o, lazy, propertyInfo, originalState, loadBehavior);
                        }
                    }
                    else
                    {
                        ApplyValue(o, newValue, propertyInfo, originalState, loadBehavior);
                    }
                }
            }  // end foreach 
        }

        /// <summary>
        /// Method used to apply a value taking the specified LoadBehavior into account.
        /// </summary>
        /// <param name="o">The target object</param>
        /// <param name="value">The value to apply</param>
        /// <param name="propertyInfo">The property to apply the value to</param>
        /// <param name="originalState">The original state map for the object</param>
        /// <param name="loadBehavior">The LoadBehavior to govern property merge behavior.</param>
        private static void ApplyValue(object o, object value, PropertyInfo propertyInfo, IDictionary<string, object> originalState, LoadBehavior loadBehavior)
        {
            if (loadBehavior == LoadBehavior.KeepCurrent)
            {
                return;
            }

            Lazy<object> lazyValue = value as Lazy<object>;
            if (loadBehavior == LoadBehavior.RefreshCurrent)
            {
                // overwrite value with the new value
                if (lazyValue != null)
                {
                    value = lazyValue.Value;
                }
                propertyInfo.SetValue(o, value, null);
            }
            else if (loadBehavior == LoadBehavior.MergeIntoCurrent)
            {
                if (!PropertyHasChanged(o, originalState, propertyInfo))
                {
                    // set the value only if our value hasn't been modified
                    if (lazyValue != null)
                    {
                        value = lazyValue.Value;
                    }
                    propertyInfo.SetValue(o, value, null);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified property has changed value.
        /// </summary>
        /// <param name="o">The parent of the property.</param>
        /// <param name="originalValues">The original values for the modified parent instance.</param>
        /// <param name="prop">The property to check.</param>
        /// <returns>True if the property has changed, false otherwise.</returns>
        internal static bool PropertyHasChanged(object o, IDictionary<string, object> originalValues, PropertyInfo prop)
        {
            if (originalValues == null)
            {
                return false;
            }

            object currentValue = prop.GetValue(o, null);
            object originalValue;

            if (!originalValues.TryGetValue(prop.Name, out originalValue))
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, Resource.Entity_Property_NotChangeTracked, prop.Name, o.GetType()));
            }

            if (!object.Equals(currentValue, originalValue))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// For the specified Type and original state map, this method returns a state map containing
        /// only values that should be roundtripped (based on RoundtripOriginal member attribution).
        /// </summary>
        /// <param name="type">The Type the state map is for.</param>
        /// <param name="state">The original state map.</param>
        /// <returns>The state map containing only values that should be rountripped.</returns>
        internal static IDictionary<string, object> ExtractRoundtripState(Type type, IDictionary<string, object> state)
        {
            MetaType metaType = MetaType.GetMetaType(type);
            Dictionary<string, object> resultRoundtripState = new Dictionary<string, object>();

            foreach (MetaMember metaMember in metaType.DataMembers)
            {
                if (!metaMember.IsRoundtripMember)
                {
                    // only copy RTO state
                    continue;
                }

                if (!metaMember.IsComplex)
                {
                    // for simple members, just copy the state over directly
                    resultRoundtripState.Add(metaMember.Member.Name, state[metaMember.Member.Name]);
                }
                else
                {
                    // if the member is complex we need to preprocess and apply values recursively
                    if (!metaMember.IsCollection)
                    {
                        IDictionary<string, object> originalState = (IDictionary<string, object>)state[metaMember.Member.Name];
                        if (originalState != null)
                        {
                            IDictionary<string, object> roundtripState = ExtractRoundtripState(metaMember.Member.PropertyType, originalState);
                            resultRoundtripState.Add(metaMember.Member.Name, roundtripState);
                        }
                    }
                    else
                    {
                        IEnumerable originalCollection = (IEnumerable)state[metaMember.Member.Name];
                        if (originalCollection != null)
                        {
                            Type elementType = TypeUtility.GetElementType(metaMember.Member.PropertyType);
                            IList newCollection = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

                            // Create a copy collection and copy elements recursively. Since Entity Extract/Apply state isn't
                            // deep through complex type collections, we have to recursively do the RTO filtering here.
                            foreach (object element in originalCollection)
                            {
                                IDictionary<string, object> originalState = ObjectStateUtility.ExtractState(element);
                                if (originalState != null)
                                {
                                    IDictionary<string, object> roundtripState = ExtractRoundtripState(elementType, originalState);
                                    object newInstance = Activator.CreateInstance(elementType);
                                    ObjectStateUtility.ApplyState(newInstance, roundtripState);
                                    newCollection.Add(newInstance);
                                }
                            }

                            resultRoundtripState.Add(metaMember.Member.Name, newCollection);
                        }
                    }
                }
            }

            return resultRoundtripState;
        }
    }
}
