using Microsoft.VisualStudio.TestTools.UnitTesting;
using Env.CnemcPublish.RiaServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Env.CnemcPublish.RiaServices.EnvCnemcPublishDomainContext;
using Com.Hzexe.Air.OpenAirLibrary;
using Env.CnemcPublish.DAL;
using OpenAirLibraryUnitTest;

namespace Env.CnemcPublish.RiaServices.Tests
{
    /// <summary>
    /// 此接口远程调用时有三种方式的返回值,分别测试调用和解析
    /// </summary>
    [TestClass()]
    public class EnvCnemcPublishDomainContextTests
    {
        [TestMethod(),Description("Query方式调用测试")]
        public async Task QueryTest()
        {
            //获取省
            var provinces = await Config.PublishCtx.Load(Config.PublishCtx.GetProvincesQuery()).ResultAsync();
            Assert.IsNotNull(provinces);
            Assert.IsTrue(provinces.Count() > 0);
        }

        [TestMethod(), Description("Api方式调用测试")]
        public async Task ApiTest()
        {
            //获取服务端时间
            DateTime serverTime = await Config.PublishCtx.GetServerTime().ResultAsync();
            Assert.IsTrue(serverTime>DateTime.Today);
        }

        [TestMethod(), Description("Api方式返回值被压缩形式的测试")]
        public async Task Api_Decompress_Test()
        {
            //GetAllAQIPublishLive
            CityAQIModel[] compressedData = await Config.PublishCtx.GetAllCityDayAQIModels().ResultAsync<CityAQIModel[]>();
            Assert.IsNotNull(compressedData);
            Assert.IsTrue(compressedData.Length>0);
        }

        [TestMethod(), Description("Api方式返回值被压缩形式的测试")]
        public async Task GetAQIChartValuesTest() {
            var AQIChartValues = await Config.PublishCtx.Load(Config.PublishCtx.GetIAQIDataPublishLivesQuery()).ResultAsync();
            Assert.IsNotNull(AQIChartValues);
            Assert.IsTrue(AQIChartValues.Count() > 0);
        }
    }
}