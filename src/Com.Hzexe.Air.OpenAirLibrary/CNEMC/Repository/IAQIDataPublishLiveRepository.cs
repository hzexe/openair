namespace CNEMC.Repository
{
    using Env.CnemcPublish.RiaServices;
    using System;
    using OpenRiaServices.DomainServices.Client;
    using Env.CnemcPublish.DAL;

    public class IAQIDataPublishLiveRepository : RepositoryBase<IAQIDataPublishLive>
    {
        public IAQIDataPublishLiveRepository(EnvCnemcPublishDomainContext service) : base(service)
        {
        }

        public virtual void GetAllIaqiPublishLive(Action<byte[]> completedAction)
        {
            base.Service.GetAllIaqiPublishLive().Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<byte[]> operation = sender as InvokeOperation<byte[]>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }

        public virtual void GetAreaIaqiPublishLive(Action<byte[]> completedAction, string area)
        {
            base.Service.GetAreaIaqiPublishLive(area).Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<byte[]> operation = sender as InvokeOperation<byte[]>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }
    }
}

