namespace CNEMC.Repository
{
    using Env.CnemcPublish.RiaServices;
    using System;
    using OpenRiaServices.DomainServices.Client;
    using Env.CnemcPublish.DAL;
    using System.Collections.Generic;

    public class ProvinceRepository : RepositoryBase<Province>
    {
        public ProvinceRepository(EnvCnemcPublishDomainContext service) : base(service)
        {
        }

        public virtual void GetProvinces(Action<IEnumerable<Province>> completedAction)
        {
            EntityQuery<Province> provincesQuery = base.Service.GetProvincesQuery();
            base.ProcessCollection(completedAction, provincesQuery);
        }
    }
}

