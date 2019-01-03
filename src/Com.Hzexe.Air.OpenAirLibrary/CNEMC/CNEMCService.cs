namespace CNEMC
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Xml.Linq;

    public class CNEMCService
    {
        public static Dictionary<string, string> ServiceConfigs = new Dictionary<string, string>();

        static CNEMCService()
        {

            /*
                WebClient client = new WebClient();
                var str = client.DownloadString(new Uri("http://106.37.208.233:20035//Content/Map.xml"));
                client.Dispose();
                foreach (XElement element2 in XElement.Parse(str).Elements())
                {
                    CNEMCService.ServiceConfigs.Add(element2.Attribute("key").Value.Trim(), element2.Attribute("value").Value.Trim());
                }
          */
        }
    }
}

