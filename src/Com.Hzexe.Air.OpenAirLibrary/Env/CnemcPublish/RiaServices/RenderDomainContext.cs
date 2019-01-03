namespace Env.CnemcPublish.RiaServices
{
    using Env.CnemcPublish.DAL;
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using OpenRiaServices.DomainServices.Client;

    public sealed class RenderDomainContext : DomainContext
    {
        public RenderDomainContext() : this(CreateDomainClient(typeof(IRenderDomainServiceContract),new Uri("Env-CnemcPublish-RiaServices-RenderDomainService.svc", UriKind.Relative),false))
        {
        }

        public RenderDomainContext(DomainClient domainClient) : base(domainClient)
        {
        }

        public RenderDomainContext(Uri serviceUri) : this(CreateDomainClient(typeof(IRenderDomainServiceContract),serviceUri,false))
        {
        }

        protected override EntityContainer CreateEntityContainer()
        {
            return new RenderDomainContextEntityContainer();
        }

        public InvokeOperation<List<AQIDataPublishHistory_2014>> GetAqiHistoryFromEnvDataChina(DateTime time)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("time", time);
            base.ValidateMethod("GetAqiHistoryFromEnvDataChina", dictionary);
            return (InvokeOperation<List<AQIDataPublishHistory_2014>>)this.InvokeOperation("GetAqiHistoryFromEnvDataChina", typeof(List<AQIDataPublishHistory_2014>), dictionary, true, null, null);
        }

        public InvokeOperation<List<AQIDataPublishHistory_2014>> GetAqiHistoryFromEnvDataChina(DateTime time, Action<InvokeOperation<List<AQIDataPublishHistory_2014>>> callback, object userState)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("time", time);
            base.ValidateMethod("GetAqiHistoryFromEnvDataChina", dictionary);
            return this.InvokeOperation<List<AQIDataPublishHistory_2014>>("GetAqiHistoryFromEnvDataChina", typeof(List<AQIDataPublishHistory_2014>), dictionary, true, callback, userState);
        }

        public InvokeOperation<List<CityDayAQIPublishHistory_2014>> GetCityAqiHistoryFrom(DateTime time)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("time", time);
            base.ValidateMethod("GetCityAqiHistoryFrom", dictionary);
            return (InvokeOperation<List<CityDayAQIPublishHistory_2014>>)this.InvokeOperation("GetCityAqiHistoryFrom", typeof(List<CityDayAQIPublishHistory_2014>), dictionary, true, null, null);
        }

        public InvokeOperation<List<CityDayAQIPublishHistory_2014>> GetCityAqiHistoryFrom(DateTime time, Action<InvokeOperation<List<CityDayAQIPublishHistory_2014>>> callback, object userState)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("time", time);
            base.ValidateMethod("GetCityAqiHistoryFrom", dictionary);
            return this.InvokeOperation<List<CityDayAQIPublishHistory_2014>>("GetCityAqiHistoryFrom", typeof(List<CityDayAQIPublishHistory_2014>), dictionary, true, callback, userState);
        }

        public InvokeOperation<CityDayAQIPublishHistory_2014> GetCityDayAqiDataEntity()
        {
            base.ValidateMethod("GetCityDayAqiDataEntity", null);
            return (InvokeOperation<CityDayAQIPublishHistory_2014>)this.InvokeOperation("GetCityDayAqiDataEntity", typeof(CityDayAQIPublishHistory_2014), null, true, null, null);
        }

        public InvokeOperation<CityDayAQIPublishHistory_2014> GetCityDayAqiDataEntity(Action<InvokeOperation<CityDayAQIPublishHistory_2014>> callback, object userState)
        {
            base.ValidateMethod("GetCityDayAqiDataEntity", null);
            return this.InvokeOperation<CityDayAQIPublishHistory_2014>("GetCityDayAqiDataEntity", typeof(CityDayAQIPublishHistory_2014), null, true, callback, userState);
        }

        public InvokeOperation<List<CityAQIPublishHistory_2014>> GetCityRealAqiHistoryFrom(DateTime time)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("time", time);
            base.ValidateMethod("GetCityRealAqiHistoryFrom", dictionary);
            return (InvokeOperation<List<CityAQIPublishHistory_2014>>)this.InvokeOperation("GetCityRealAqiHistoryFrom", typeof(List<CityAQIPublishHistory_2014>), dictionary, true, null, null);
        }

        public InvokeOperation<List<CityAQIPublishHistory_2014>> GetCityRealAqiHistoryFrom(DateTime time, Action<InvokeOperation<List<CityAQIPublishHistory_2014>>> callback, object userState)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("time", time);
            base.ValidateMethod("GetCityRealAqiHistoryFrom", dictionary);
            return this.InvokeOperation<List<CityAQIPublishHistory_2014>>("GetCityRealAqiHistoryFrom", typeof(List<CityAQIPublishHistory_2014>), dictionary, true, callback, userState);
        }

        public InvokeOperation<AQIDataPublishHistory_2014> GetEntity()
        {
            base.ValidateMethod("GetEntity", null);
            return (InvokeOperation<AQIDataPublishHistory_2014>)this.InvokeOperation("GetEntity", typeof(AQIDataPublishHistory_2014), null, true, null, null);
        }

        public InvokeOperation<AQIDataPublishHistory_2014> GetEntity(Action<InvokeOperation<AQIDataPublishHistory_2014>> callback, object userState)
        {
            base.ValidateMethod("GetEntity", null);
            return this.InvokeOperation<AQIDataPublishHistory_2014>("GetEntity", typeof(AQIDataPublishHistory_2014), null, true, callback, userState);
        }

        [System.ServiceModel.ServiceContract]
        public interface IRenderDomainServiceContract
        {
            [System.ServiceModel.OperationContract(AsyncPattern = true, Action = "http://tempuri.org/RenderDomainService/GetAqiHistoryFromEnvDataChina", ReplyAction = "http://tempuri.org/RenderDomainService/GetAqiHistoryFromEnvDataChinaResponse")]
            IAsyncResult BeginGetAqiHistoryFromEnvDataChina(DateTime time, AsyncCallback callback, object asyncState);
            [System.ServiceModel.OperationContract(AsyncPattern = true, Action = "http://tempuri.org/RenderDomainService/GetCityAqiHistoryFrom", ReplyAction = "http://tempuri.org/RenderDomainService/GetCityAqiHistoryFromResponse")]
            IAsyncResult BeginGetCityAqiHistoryFrom(DateTime time, AsyncCallback callback, object asyncState);
            [System.ServiceModel.OperationContract(AsyncPattern = true, Action = "http://tempuri.org/RenderDomainService/GetCityDayAqiDataEntity", ReplyAction = "http://tempuri.org/RenderDomainService/GetCityDayAqiDataEntityResponse")]
            IAsyncResult BeginGetCityDayAqiDataEntity(AsyncCallback callback, object asyncState);
            [System.ServiceModel.OperationContract(AsyncPattern = true, Action = "http://tempuri.org/RenderDomainService/GetCityRealAqiHistoryFrom", ReplyAction = "http://tempuri.org/RenderDomainService/GetCityRealAqiHistoryFromResponse")]
            IAsyncResult BeginGetCityRealAqiHistoryFrom(DateTime time, AsyncCallback callback, object asyncState);
            [System.ServiceModel.OperationContract(AsyncPattern = true, Action = "http://tempuri.org/RenderDomainService/GetEntity", ReplyAction = "http://tempuri.org/RenderDomainService/GetEntityResponse")]
            IAsyncResult BeginGetEntity(AsyncCallback callback, object asyncState);
            List<AQIDataPublishHistory_2014> EndGetAqiHistoryFromEnvDataChina(IAsyncResult result);
            List<CityDayAQIPublishHistory_2014> EndGetCityAqiHistoryFrom(IAsyncResult result);
            CityDayAQIPublishHistory_2014 EndGetCityDayAqiDataEntity(IAsyncResult result);
            List<CityAQIPublishHistory_2014> EndGetCityRealAqiHistoryFrom(IAsyncResult result);
            AQIDataPublishHistory_2014 EndGetEntity(IAsyncResult result);
        }

        internal sealed class RenderDomainContextEntityContainer : EntityContainer
        {
        }
    }
}

