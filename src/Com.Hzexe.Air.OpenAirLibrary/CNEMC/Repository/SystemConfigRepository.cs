namespace CNEMC.Repository
{
    using Env.CnemcPublish.RiaServices;
    using System;
    using OpenRiaServices.DomainServices.Client;
    using Env.CnemcPublish.DAL;
    using System.Collections.Generic;

    public class SystemConfigRepository : RepositoryBase<SystemConfig>
    {
        public SystemConfigRepository(EnvCnemcPublishDomainContext service) : base(service)
        {
        }

        public virtual void GetSystemConfig(Action<IEnumerable<SystemConfig>> completedAction)
        {
            EntityQuery<SystemConfig> systemConfigsQuery = base.Service.GetSystemConfigsQuery();
            base.ProcessCollection(completedAction, systemConfigsQuery);
        }
    }
}

