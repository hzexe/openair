namespace CNEMC.Repository
{
    using Env.CnemcPublish.RiaServices;
    using System;
    using OpenRiaServices.DomainServices.Client;
    using Env.CnemcPublish.DAL;
    using System.Collections.Generic;

    public class AQIDataPublishLiveRepository : RepositoryBase<AQIDataPublishLive>
    {
        public AQIDataPublishLiveRepository(EnvCnemcPublishDomainContext service) : base(service)
        {
        }

        public virtual void GetAllAQIPublishLive(Action<byte[]> completedAction)
        {
            base.Service.GetAllAQIPublishLive().Completed += (delegate (object sender, EventArgs e) {
                InvokeOperation<byte[]> operation = sender as InvokeOperation<byte[]>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }

        public virtual void GetAQIDataPublishLives(Action<IEnumerable<AQIDataPublishLive>> completedAction)
        {
            EntityQuery<AQIDataPublishLive> aQIDataPublishLivesQuery = base.Service.GetAQIDataPublishLivesQuery();
            base.ProcessCollection(completedAction, aQIDataPublishLivesQuery);
        }

        public virtual void GetAreaAQIPublishLive(Action<byte[]> completedAction, string area)
        {
            base.Service.GetAreaAQIPublishLive(area).Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<byte[]> operation = sender as InvokeOperation<byte[]>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }

        public virtual void GetProvinceAQIPublishLive(Action<byte[]> completedAction, int pid)
        {
            base.Service.GetProvincePublishLives(pid).Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<byte[]> operation = sender as InvokeOperation<byte[]>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }
    }
}

