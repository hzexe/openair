namespace CNEMC.Repository
{
    using Env.CnemcPublish.RiaServices;
    using System;

    public interface IRepositoryFactory
    {
        AQIDataPublishHistoryRepository CreateAQIDataPublishHistoryRepository();
        AQIDataPublishLiveRepository CreateAQIDataPublishLiveRepository();
        CityAQIPublishHistoryRepository CreateCityAQIPublishHistoryRepository();
        CityAQIPublishLiveRepository CreateCityAQIPublishLiveRepository();
        CityDayAQIPublishHistoryRepository CreateCityDayAQIPublishHistoryRepository();
        CityDayAQIPublishLiveRepository CreateCityDayAQIPublishLiveRepository();
        CityRepository CreateCityRepository();
        IAQIDataPublishHistoryRepository CreateIAQIDataPublishHistoryRepository();
        IAQIDataPublishLiveRepository CreateIAQIDataPublishLiveRepository();
        ProvinceRepository CreateProvinceRepository();
        SystemConfigRepository CreateSystemConfigRepository();

        EnvCnemcPublishDomainContext Service { get; set; }
    }
}

