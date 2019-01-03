namespace CNEMC
{
    using System;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class ServiceUtil<T> where T: class, new()
    {
        public static T GetInstance()
        {
            T local = default(T);
            Type type = typeof(T);
            string serviceAddress = GetServiceAddress(type.Name.Replace("Client", ""));
            if (string.IsNullOrEmpty(serviceAddress))
            {
                return default(T);
            }
            object[] parameters = new object[2];
            System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None) {
                MaxBufferSize = 0x7fffffff,
                MaxReceivedMessageSize = 0x7fffffffL,
                SendTimeout = new TimeSpan(0, 5, 0),
                ReceiveTimeout = new TimeSpan(0, 5, 0),
                OpenTimeout = new TimeSpan(0, 1, 0),
                CloseTimeout = new TimeSpan(0, 1, 0)
            };
            System.ServiceModel.EndpointAddress address = new System.ServiceModel.EndpointAddress(new Uri(serviceAddress, UriKind.Absolute), new System.ServiceModel.Channels.AddressHeader[0]);
            parameters[0] = binding;
            parameters[1] = address;
            ConstructorInfo constructor = null;
            try
            {
                Type[] types = new Type[] { typeof(System.ServiceModel.Channels.Binding), typeof(System.ServiceModel.EndpointAddress) };
                constructor = typeof(T).GetConstructor(types);
            }
            catch (Exception)
            {
                return default(T);
            }
            if (constructor != null)
            {
                local = (T) constructor.Invoke(parameters);
            }
            return local;
        }

        public static string GetServiceAddress(string serviceName)
        {
            return CNEMCService.ServiceConfigs[serviceName];
        }
        /*
        public enum ServiceType
        {
            public const ServiceUtil<T>.ServiceType Service1svc = ServiceUtil<T>.ServiceType.Service1svc;
        }
        */
    }
}

