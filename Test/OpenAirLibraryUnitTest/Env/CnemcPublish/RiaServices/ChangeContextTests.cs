using Microsoft.VisualStudio.TestTools.UnitTesting;
using Env.CnemcPublish.RiaServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAirLibraryUnitTest;
using Com.Hzexe.Air.OpenAirLibrary;
using Env.CnemcPublish.DAL;

namespace Env.CnemcPublish.RiaServices.Tests
{
    [TestClass()]
    public class ChangeContextTests
    {

        [TestMethod()]
        public async Task GetEntityPm2_5Test()
        {
            string Pm2_5 = await Config.ChangeCtx.GetEntityPm2_5().ResultAsync();
            Assert.IsTrue(DateTime.TryParse(Pm2_5, out var date));
        }

        [TestMethod()]
        public async Task GetAqi24HTest()
        {
            var Change24Hours = await Config.ChangeCtx.GetAqi24H().ResultAsync<AQIDataPublishHistory[]>();
            Assert.IsNotNull(Change24Hours?.Length>0);
        }

        [TestMethod()]
        public async Task GetAqi24HByTimeTest()
        {
            var AQIDataPublishHistorys = await Config.ChangeCtx.GetAqi24HByTime(DateTime.Now.AddDays(-1),DateTime.Now).ResultAsync<AQIDataPublishHistory[]>();
            Assert.IsNotNull(AQIDataPublishHistorys?.Length > 0);
        }

        [TestMethod()]
        public async Task GetAqi24HByIntTest()
        {
            var AQIDataPublishHistorys = await Config.ChangeCtx.GetAqi24HByInt(0,100).ResultAsync<AQIDataPublishHistory[]>();
            Assert.IsNotNull(AQIDataPublishHistorys?.Length > 0);
        }
    }
}