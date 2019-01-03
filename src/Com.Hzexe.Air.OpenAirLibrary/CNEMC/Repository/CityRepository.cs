namespace CNEMC.Repository
{
    using Env.CnemcPublish.DAL;
    using Env.CnemcPublish.RiaServices;
    using System;
    using System.Collections.Generic;
    using OpenRiaServices.DomainServices.Client;

    public class CityRepository : RepositoryBase<City>
    {
        public CityRepository(EnvCnemcPublishDomainContext service) : base(service)
        {
        }

        public virtual void GetCities(Action<IEnumerable<City>> completedAction)
        {
            EntityQuery<City> citiesQuery = base.Service.GetCitiesQuery();
            base.ProcessCollection(completedAction, citiesQuery);
        }

        public virtual void GetCitiesByPid(Action<IEnumerable<City>> completedAction, int pid)
        {
            EntityQuery<City> citiesByPidQuery = base.Service.GetCitiesByPidQuery(pid);
            base.ProcessCollection(completedAction, citiesByPidQuery);
        }

        public virtual void GetPid(Action<int> completedAction, string CityName)
        {
            base.Service.GetPid(CityName).Completed+=(delegate (object sender, EventArgs e) {
                InvokeOperation<int> operation = sender as InvokeOperation<int>;
                if (operation != null)
                {
                    completedAction(operation.Value);
                }
            });
        }
    }
}

