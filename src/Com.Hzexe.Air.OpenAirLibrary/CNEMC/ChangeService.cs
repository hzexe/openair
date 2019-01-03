namespace CNEMC
{
    using Env.CnemcPublish.RiaServices;
    using OpenRiaServices.DomainServices.Client;
    using System;

    public class ChangeService
    {
        public ChangeContext changeService = new ChangeContext();
        public string time = DateTime.Now.ToString();

        public ChangeService()
        {
            TimeSpan sendTimeout = new TimeSpan(0, 0, 15);
            WcfTimeoutUtility.ChangeWcfTimeout(this.changeService, sendTimeout, sendTimeout);
        }

        public override string ToString()
        {
            throw new NotImplementedException();
            /*
            this.changeService.GetEntityPm2_5(delegate (InvokeOperation<string> data) {
                this.time = data.();
            }, null);
            return this.time.ToString();
            */
        }
    }
}

