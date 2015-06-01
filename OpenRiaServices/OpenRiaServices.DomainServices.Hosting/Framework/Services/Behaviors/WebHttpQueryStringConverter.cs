using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Dispatcher;
using System.Text;
#if SILVERLIGHT
using System.Windows.Browser;
#else
using System.Web;
#endif

#if SERVERFX
namespace OpenRiaServices.DomainServices.Hosting
#else
namespace OpenRiaServices.DomainServices.Client
#endif
{
    internal class WebHttpQueryStringConverter : QueryStringConverter
    {
        public override bool CanConvert(Type type)
        {
            // Allow everything.
            return true;
        }

#if !SILVERLIGHT // server-side path
        public override object ConvertStringToValue(string parameter, Type parameterType)
        {
            if (parameter == null)
            {
                return null;
            }

            if (base.CanConvert(parameterType))
            {
                return base.ConvertStringToValue(parameter, parameterType);
            }

            parameter = HttpUtility.UrlDecode(parameter);
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(parameter)))
            {
                return new DataContractJsonSerializer(parameterType).ReadObject(ms);
            }
        }
#endif // !SILVERLIGHT

        public override string ConvertValueToString(object parameter, Type parameterType)
        {
            if (base.CanConvert(parameterType))
            {
                return base.ConvertValueToString(parameter, parameterType);
            }
            using (MemoryStream ms = new MemoryStream())
            {
                new DataContractJsonSerializer(parameterType).WriteObject(ms, parameter);
                byte[] result = ms.ToArray();
                string value = Encoding.UTF8.GetString(result, 0, result.Length);
                return HttpUtility.UrlEncode(value);
            }
        }
    }
}
