using Env.CnemcPublish.RiaServices;
using OpenRiaServices.DomainServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Env.CnemcPublish.RiaServices.ChangeContext;

namespace OpenAirLibraryUnitTest
{
    class Config
    {
        public const string XAP_URL = "http://106.37.208.233:20035/ClientBin/cnemc.xap";
        public static EnvCnemcPublishDomainContext PublishCtx { get; private set; }
        public static ChangeContext ChangeCtx { get; private set; }

        static Config()
        {
            PublishCtx = createPublishDomainContext();
            ChangeCtx = createChangeContext();
        }

        private static EnvCnemcPublishDomainContext createPublishDomainContext()
        {
            Uri xsp = new Uri(XAP_URL);
            return new EnvCnemcPublishDomainContext(XAP_URL);
        }

        private static ChangeContext createChangeContext()
        {
            Uri xsp = new Uri(XAP_URL);
            Uri serviceUri = new Uri(xsp, "Env-CnemcPublish-RiaServices-ChangeService.svc");
            var domain = DomainContext.DomainClientFactory.CreateDomainClient(typeof(IChangeServiceContract), new Uri(xsp, "Env-CnemcPublish-RiaServices-ChangeService.svc"), false);
            return new ChangeContext(domain);
        }


    }
}
