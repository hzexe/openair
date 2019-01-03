namespace CNEMC.Repository
{
    using Env.CnemcPublish.RiaServices;
    using System;
    using OpenRiaServices.DomainServices.Client;
    using Env.CnemcPublish.DAL;
    using System.Collections.Generic;

    public class CityAQIPublishLiveRepository : RepositoryBase<CityAQIPublishLive>
    {
        public CityAQIPublishLiveRepository(EnvCnemcPublishDomainContext service) : base(service)
        {
        }

        public virtual void GetAllCityAqIs(Action<byte[]> completedAction)
        {
            base.Service.GetAllCityRealTimeAQIModels().Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<byte[]> operation = sender as InvokeOperation<byte[]>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }

        public virtual void GetCityAQIBycityCode(int cityCode, Action<IEnumerable<CityAQIPublishLive>> completedAction)
        {
            EntityQuery<CityAQIPublishLive> cityRealTimeAQIModelByCitycodeQuery = base.Service.GetCityRealTimeAQIModelByCitycodeQuery(cityCode);
            base.ProcessCollection(completedAction, cityRealTimeAQIModelByCitycodeQuery);
        }
    }
}

