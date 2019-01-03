namespace CNEMC.Repository
{
    using Env.CnemcPublish.RiaServices;
    using System;
    using OpenRiaServices.DomainServices.Client;
    using Env.CnemcPublish.DAL;

    public class IAQIDataPublishHistoryRepository : RepositoryBase<IAQIDataPublishHistory>
    {
        public IAQIDataPublishHistoryRepository(EnvCnemcPublishDomainContext service) : base(service)
        {
        }

        public virtual void GetIaqiDataPublishHistoriesByCondition(string stationCode, Action<byte[]> completedAction)
        {
            base.Service.GetIaqiHistoryByCondition(stationCode).Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<byte[]> operation = sender as InvokeOperation<byte[]>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }

        public virtual void GetServerTime(Action<DateTime> completedAction)
        {
            base.Service.GetServerTime().Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<DateTime> operation = sender as InvokeOperation<DateTime>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }
    }
}

