namespace CNEMC
{
    using System;
    using System.Reflection;
    using System.ServiceModel;
    using OpenRiaServices.DomainServices.Client;

    public static class WcfTimeoutUtility
    {
        public static void ChangeWcfTimeout(DomainContext context, TimeSpan sendTimeout, TimeSpan receiveTimeout)
        {
            PropertyInfo property = context.GetType().GetProperty("ChannelFactory");
            if (property == null)
            {
                throw new InvalidOperationException("There is no 'ChannelFactory' property on the DomainClient.");
            }
            System.ServiceModel.ChannelFactory factory = (System.ServiceModel.ChannelFactory) property.GetValue(context, null);
            factory.Endpoint.Binding.ReceiveTimeout = receiveTimeout;
            factory.Endpoint.Binding.SendTimeout = sendTimeout;
        }
    }
}

