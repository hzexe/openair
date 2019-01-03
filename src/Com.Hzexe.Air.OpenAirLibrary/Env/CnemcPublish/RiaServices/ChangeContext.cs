namespace Env.CnemcPublish.RiaServices
{
    using Env.CnemcPublish.DAL.Models;
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using OpenRiaServices.DomainServices.Client;

    public sealed class ChangeContext : DomainContext
    {
        public ChangeContext() : this(CreateDomainClient(typeof(IChangeServiceContract),new Uri("Env-CnemcPublish-RiaServices-ChangeService.svc", UriKind.Relative),false))
        {
        }

        public ChangeContext(DomainClient domainClient) : base(domainClient)
        {
        }

        public ChangeContext(Uri serviceUri) : this(CreateDomainClient(typeof(IChangeServiceContract), serviceUri,false))
        {
        }

        protected override EntityContainer CreateEntityContainer()
        {
            return new ChangeContextEntityContainer();
        }

        public InvokeOperation<byte[]> GetAqi24H()
        {
            base.ValidateMethod("GetAqi24H", null);
            return (InvokeOperation<byte[]>) this.InvokeOperation("GetAqi24H", typeof(byte[]), null, true, null, null);
        }

        public InvokeOperation<byte[]> GetAqi24H(Action<InvokeOperation<byte[]>> callback, object userState)
        {
            base.ValidateMethod("GetAqi24H", null);
            return this.InvokeOperation<byte[]>("GetAqi24H", typeof(byte[]), null, true, callback, userState);
        }

        public InvokeOperation<byte[]> GetAqi24HByInt(int searchIndex, int count)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("searchIndex", searchIndex);
            dictionary.Add("count", count);
            base.ValidateMethod("GetAqi24HByInt", dictionary);
            return (InvokeOperation<byte[]>) this.InvokeOperation("GetAqi24HByInt", typeof(byte[]), dictionary, true, null, null);
        }

        public InvokeOperation<byte[]> GetAqi24HByInt(int searchIndex, int count, Action<InvokeOperation<byte[]>> callback, object userState)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("searchIndex", searchIndex);
            dictionary.Add("count", count);
            base.ValidateMethod("GetAqi24HByInt", dictionary);
            return this.InvokeOperation<byte[]>("GetAqi24HByInt", typeof(byte[]), dictionary, true, callback, userState);
        }

        public InvokeOperation<Change24Hours> GetAqi24HByNameCount(string poName, int searchIndex, int count)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("poName", poName);
            dictionary.Add("searchIndex", searchIndex);
            dictionary.Add("count", count);
            base.ValidateMethod("GetAqi24HByNameCount", dictionary);
            return (InvokeOperation<Change24Hours>) this.InvokeOperation("GetAqi24HByNameCount", typeof(Change24Hours), dictionary, true, null, null);
        }

        public InvokeOperation<Change24Hours> GetAqi24HByNameCount(string poName, int searchIndex, int count, Action<InvokeOperation<Change24Hours>> callback, object userState)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("poName", poName);
            dictionary.Add("searchIndex", searchIndex);
            dictionary.Add("count", count);
            base.ValidateMethod("GetAqi24HByNameCount", dictionary);
            return this.InvokeOperation<Change24Hours>("GetAqi24HByNameCount", typeof(Change24Hours), dictionary, true, callback, userState);
        }

        public InvokeOperation<byte[]> GetAqi24HByTime(DateTime startTime, DateTime endTime)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("startTime", startTime);
            dictionary.Add("endTime", endTime);
            base.ValidateMethod("GetAqi24HByTime", dictionary);
            return (InvokeOperation<byte[]>) this.InvokeOperation("GetAqi24HByTime", typeof(byte[]), dictionary, true, null, null);
        }

        public InvokeOperation<byte[]> GetAqi24HByTime(DateTime startTime, DateTime endTime, Action<InvokeOperation<byte[]>> callback, object userState)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("startTime", startTime);
            dictionary.Add("endTime", endTime);
            base.ValidateMethod("GetAqi24HByTime", dictionary);
            return this.InvokeOperation<byte[]>("GetAqi24HByTime", typeof(byte[]), dictionary, true, callback, userState);
        }

        public InvokeOperation<Change24Hours> GetEntity()
        {
            base.ValidateMethod("GetEntity", null);
            return (InvokeOperation<Change24Hours>) this.InvokeOperation("GetEntity", typeof(Change24Hours), null, true, null, null);
        }

        public InvokeOperation<Change24Hours> GetEntity(Action<InvokeOperation<Change24Hours>> callback, object userState)
        {
            base.ValidateMethod("GetEntity", null);
            return this.InvokeOperation<Change24Hours>("GetEntity", typeof(Change24Hours), null, true, callback, userState);
        }

        public InvokeOperation<string> GetEntityPm2_5()
        {
            base.ValidateMethod("GetEntityPm2_5", null);
            return (InvokeOperation<string>) this.InvokeOperation("GetEntityPm2_5", typeof(string), null, true, null, null);
        }

        public InvokeOperation<string> GetEntityPm2_5(Action<InvokeOperation<string>> callback, object userState)
        {
            base.ValidateMethod("GetEntityPm2_5", null);
            return this.InvokeOperation<string>("GetEntityPm2_5", typeof(string), null, true, callback, userState);
        }

        internal sealed class ChangeContextEntityContainer : EntityContainer
        {
        }

        [System.ServiceModel.ServiceContract]
        public interface IChangeServiceContract
        {
            [System.ServiceModel.OperationContract(AsyncPattern=true, Action="http://tempuri.org/ChangeService/GetAqi24H", ReplyAction="http://tempuri.org/ChangeService/GetAqi24HResponse")]
            IAsyncResult BeginGetAqi24H(AsyncCallback callback, object asyncState);
            [ System.ServiceModel.OperationContract(AsyncPattern=true, Action="http://tempuri.org/ChangeService/GetAqi24HByInt", ReplyAction="http://tempuri.org/ChangeService/GetAqi24HByIntResponse")]
            IAsyncResult BeginGetAqi24HByInt(int searchIndex, int count, AsyncCallback callback, object asyncState);
            [ System.ServiceModel.OperationContract(AsyncPattern=true, Action="http://tempuri.org/ChangeService/GetAqi24HByNameCount", ReplyAction="http://tempuri.org/ChangeService/GetAqi24HByNameCountResponse")]
            IAsyncResult BeginGetAqi24HByNameCount(string poName, int searchIndex, int count, AsyncCallback callback, object asyncState);
            [System.ServiceModel.OperationContract(AsyncPattern=true, Action="http://tempuri.org/ChangeService/GetAqi24HByTime", ReplyAction="http://tempuri.org/ChangeService/GetAqi24HByTimeResponse")]
            IAsyncResult BeginGetAqi24HByTime(DateTime startTime, DateTime endTime, AsyncCallback callback, object asyncState);
            [System.ServiceModel.OperationContract(AsyncPattern=true, Action="http://tempuri.org/ChangeService/GetEntity", ReplyAction="http://tempuri.org/ChangeService/GetEntityResponse")]
            IAsyncResult BeginGetEntity(AsyncCallback callback, object asyncState);
            [ System.ServiceModel.OperationContract(AsyncPattern=true, Action="http://tempuri.org/ChangeService/GetEntityPm2_5", ReplyAction="http://tempuri.org/ChangeService/GetEntityPm2_5Response")]
            IAsyncResult BeginGetEntityPm2_5(AsyncCallback callback, object asyncState);
            byte[] EndGetAqi24H(IAsyncResult result);
            byte[] EndGetAqi24HByInt(IAsyncResult result);
            Change24Hours EndGetAqi24HByNameCount(IAsyncResult result);
            byte[] EndGetAqi24HByTime(IAsyncResult result);
            Change24Hours EndGetEntity(IAsyncResult result);
            string EndGetEntityPm2_5(IAsyncResult result);
        }
    }
}

