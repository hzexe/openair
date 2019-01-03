namespace CNEMC.Repository
{
    using Env.CnemcPublish.DAL;
    using Env.CnemcPublish.RiaServices;
    using System;
    using OpenRiaServices.DomainServices.Client;
    
    public class AQIDataPublishHistoryRepository : RepositoryBase<AQIDataPublishHistory>
    {
        public AQIDataPublishHistoryRepository(EnvCnemcPublishDomainContext service) : base(service)
        {
        }

        public virtual void GetAqi24H(Action<byte[]> completedAction)
        {
            base.Service.GetAqi24H().Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<byte[]> operation = sender as InvokeOperation<byte[]>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }

        public virtual void GetAqi24HByInt(int startIndex, int count, Action<byte[]> completedAction)
        {
            base.Service.GetAqi24HByInt(startIndex, count).Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<byte[]> operation = sender as InvokeOperation<byte[]>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }

        public virtual void GetAqi24HByTimes(DateTime sTime, DateTime eTime, Action<byte[]> completedAction)
        {
            base.Service.GetAqi24HByTime(sTime, eTime).Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<byte[]> operation = sender as InvokeOperation<byte[]>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }

        public virtual void GetAqiDataPublishHistoriesByCondition(string stationCode, Action<byte[]> completedAction)
        {
            base.Service.GetAqiHistoryByCondition(stationCode).Completed+=(delegate (object sender, EventArgs e) {
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

