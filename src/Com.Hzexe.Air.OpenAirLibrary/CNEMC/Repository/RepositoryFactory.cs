namespace CNEMC.Repository
{
    using Env.CnemcPublish.RiaServices;
    using System;
    using System.Runtime.CompilerServices;

    [Obsolete("请不要使用本类实现业务,因为RepositoryBase派生的类业务实现没有回调,过程完全不可控,仅做实现业务而作参考才保留的这些类")]
    public class RepositoryFactory : IRepositoryFactory
    {
        private static IRepositoryFactory _instance;

        public AQIDataPublishHistoryRepository CreateAQIDataPublishHistoryRepository()
        {
            return new AQIDataPublishHistoryRepository(this.Service);
        }

        public AQIDataPublishLiveRepository CreateAQIDataPublishLiveRepository()
        {
            return new AQIDataPublishLiveRepository(this.Service);
        }

        public CityAQIPublishHistoryRepository CreateCityAQIPublishHistoryRepository()
        {
            return new CityAQIPublishHistoryRepository(this.Service);
        }

        public CityAQIPublishLiveRepository CreateCityAQIPublishLiveRepository()
        {
            return new CityAQIPublishLiveRepository(this.Service);
        }

        public CityDayAQIPublishHistoryRepository CreateCityDayAQIPublishHistoryRepository()
        {
            return new CityDayAQIPublishHistoryRepository(this.Service);
        }

        public CityDayAQIPublishLiveRepository CreateCityDayAQIPublishLiveRepository()
        {
            return new CityDayAQIPublishLiveRepository(this.Service);
        }

        public CityRepository CreateCityRepository()
        {
            return new CityRepository(this.Service);
        }

        public IAQIDataPublishHistoryRepository CreateIAQIDataPublishHistoryRepository()
        {
            return new IAQIDataPublishHistoryRepository(this.Service);
        }

        public IAQIDataPublishLiveRepository CreateIAQIDataPublishLiveRepository()
        {
            return new IAQIDataPublishLiveRepository(this.Service);
        }

        public ProvinceRepository CreateProvinceRepository()
        {
            return new ProvinceRepository(this.Service);
        }

        public SystemConfigRepository CreateSystemConfigRepository()
        {
            return new SystemConfigRepository(this.Service);
        }

        public static IRepositoryFactory Instance
        {
            get
            {
                return (_instance ?? (_instance = new RepositoryFactory()));
            }
            set
            {
                _instance = value;
            }
        }

        public EnvCnemcPublishDomainContext Service { get; set; }
    }
}

