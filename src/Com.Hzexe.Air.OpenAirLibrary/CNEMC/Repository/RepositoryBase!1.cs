namespace CNEMC.Repository
{
    using Env.CnemcPublish.RiaServices;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.InteropServices;
    using OpenRiaServices.DomainServices.Client;

    public abstract class RepositoryBase<T> : IDisposable where T: Entity
    {
        private readonly EnvCnemcPublishDomainContext _service;

        protected RepositoryBase(EnvCnemcPublishDomainContext service)
        {
            this._service = service;
        }

        public void Dispose()
        {
        }

        private void LogErrorToDebug(SubmitOperation so)
        {
            foreach (Entity entity in so.EntitiesInError)
            {
                foreach (ValidationResult result in entity.ValidationErrors)
                {
                    using (IEnumerator<string> enumerator3 = result.MemberNames.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            string current = enumerator3.Current;
                        }
                    }
                }
                if (entity.EntityConflict != null)
                {
                    using (IEnumerator<string> enumerator4 = entity.EntityConflict.PropertyNames.GetEnumerator())
                    {
                        while (enumerator4.MoveNext())
                        {
                            string local2 = enumerator4.Current;
                        }
                    }
                }
            }
        }

        protected void ProcessCollection(Action<IEnumerable<T>> completedAction, EntityQuery<T> query)
        {
            EventHandler handler = null;
            try
            {
                if (handler == null)
                {
                    handler = delegate (object sender, EventArgs e) {
                        LoadOperation<T> operation = sender as LoadOperation<T>;
                        if (operation != null)
                        {
                            completedAction(operation.Entities);
                        }
                    };
                }
                this.Service.Load<T>(query).Completed+=(handler);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void ProcessSingle(Action<T> completedAction, EntityQuery<T> query)
        {
            this.Service.Load<T>(query).Completed+=(delegate (object sender, EventArgs e) {
                LoadOperation<T> operation = sender as LoadOperation<T>;
                if (operation != null)
                {
                    completedAction(operation.Entities.FirstOrDefault<T>());
                }
            });
        }

        public void SaveOrUpdateEntities(Action callback = null)
        {
            SubmitOperation so;
            EventHandler submitOperationCompleted;
            if (!this.Service.IsSubmitting)
            {
                so = this.Service.SubmitChanges();
                submitOperationCompleted = null;
                submitOperationCompleted = delegate (object s, EventArgs e) {
                    so.Completed-=(submitOperationCompleted);
                    if (so.HasError)
                    {
                        ((RepositoryBase<T>) this).LogErrorToDebug(so);
                        so.MarkErrorAsHandled();
                    }
                    if (callback != null)
                    {
                        callback();
                    }
                };
                so.Completed+=(submitOperationCompleted);
            }
        }

        public EnvCnemcPublishDomainContext Service
        {
            get
            {
                return this._service;
            }
        }
    }
}

