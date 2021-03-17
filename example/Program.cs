using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Env.CnemcPublish.DAL;
using Env.CnemcPublish.RiaServices;
using Com.Hzexe.Air.OpenAirLibrary;
using OpenRiaServices.DomainServices.Client;

namespace Example
{
    /// <summary>
    /// 部分功能演示
    /// </summary>
    /// <remarks>
    /// 更多功能见契约IEnvCnemcPublishDomainServiceContract和IChangeServiceContract以及IRenderDomainServiceContract
    /// 地图的api没去实现可以参考ServiceUtil<T>类和CNEMCService
    /// 项目地址:https://github.com/hzexe/openair
    /// </remarks>
    class Program
    {
        /// <summary>
        /// 国家空气检测总站上的silverlight文件的绝对地址
        /// </summary>
        /// <remarks>网站址址可能会变化，注意时效性</remarks>
        const string XAP_URL = "http://106.37.208.233:20035/ClientBin/cnemc.xap";

        static void Main(string[] args)
        {
            example().Wait();
            Console.WriteLine("Exit:");
            Console.ReadLine();
        }


        static async Task example()
        {
            Console.Title = "国家空气质量获取库的使用演示";

            //创建domain客户端
            EnvCnemcPublishDomainContext publishCtx = new EnvCnemcPublishDomainContext(XAP_URL);

            /*-----获取基础数据 ：省，市，检测站，然后才能愉快地玩耍,这些只要获取一次保存下来就不用再次获取 */
            //获取数据涉及到的省份
            IEnumerable<Province> provinces = await publishCtx.Load(publishCtx.GetProvincesQuery()).ResultAsync();
            //获取所有城市,业务上City的ProvinceId属性与省份对应
            IEnumerable<City> cities = await publishCtx.Load(publishCtx.GetCitiesQuery()).ResultAsync();
            //获取所有检测站，业务上通过citycode与城市对应
            IEnumerable<StationConfig> stations = await publishCtx.Load(publishCtx.GetStationConfigsQuery()).ResultAsync();
            /*-----基础数据获取完毕*/

            //预览：获取全国所有城市的实时空气质量,因没加查询条件和数量约束数据量比较大
            IEnumerable<CityAQIPublishLive> citylives = await publishCtx.Load(publishCtx.GetCityAQIPublishLivesQuery()).ResultAsync();
            Console.WriteLine($"获取了{citylives.Count()}个城市的数据，限于篇章问题，黑红榜分别显示前5名");
            Console.WriteLine("{0,10}|{1,5}|{2,8}|{3,14}|{4,12}", "地区", "AQI", "等级", "主要污染物", "更新时间");
            Console.WriteLine("黑榜前3");
            citylives.Where(x => int.TryParse(x.AQI, out var i))
                .OrderByDescending(x => int.Parse(x.AQI))
                .Take(5).ToList()
                .ForEach(showCityLive);
            Console.WriteLine("红榜前3");
            citylives.Where(x => int.TryParse(x.AQI, out var i))
                .OrderBy(x => int.Parse(x.AQI))
                .Take(5).ToList()
                .ForEach(showCityLive);

            Console.Write("\r\n进行下一阶段演示:"); Console.ReadLine(); Console.Clear();


            //以下以浙江省和杭州市为示例
            City hangzhou = cities.First(x => x.CityName.Contains("杭州"));
            Province zhejiang = provinces.First(x => x.Id.Equals(hangzhou.ProvinceId));

            Console.WriteLine($"获取单个省份{zhejiang.ProvinceName}的所有检测站实时空气状况");
            AQIDataPublishLive[] zhengjiang_stations_air = await publishCtx.GetProvincePublishLives(zhejiang.Id).ResultAsync<AQIDataPublishLive[]>();
            Console.WriteLine($"获取了{zhejiang.ProvinceName}所有检测站数据，限于篇章问题，黑红榜分别显示前5名");
            Console.WriteLine("{0,10}|{1,5}|{2,8}|{3,14}|{4,12}", "检测站", "AQI", "等级", "主要污染物", "更新时间");
            Console.WriteLine("黑榜前3");
            zhengjiang_stations_air.Where(x => int.TryParse(x.AQI, out var i))
                .OrderByDescending(x => int.Parse(x.AQI))
                .Take(5).ToList()
                .ForEach(showStationLive);
            Console.WriteLine("红榜前3");
            zhengjiang_stations_air.Where(x => int.TryParse(x.AQI, out var i))
                .OrderBy(x => int.Parse(x.AQI))
                .Take(5).ToList()
                .ForEach(showStationLive);

            Console.Write("进行下一阶段演示:"); Console.ReadLine(); Console.Clear();

            ////获取所有城市的一天内空气质量状况，这个太暴力，不建议使用，服务器趴了大家都别玩
            //CityAQIModel[] allCityDayAqi = await PublishCtx.GetAllCityDayAQIModels().ResultAsync<CityAQIModel[]>();

            //根据城市code获取所有检测站的空气数据
            Console.WriteLine($"使用API 获取单个城市{hangzhou.CityName}的所有检测站实时空气状况");
            AQIDataPublishLive[] hangzhou_stations1 = await publishCtx.GetAirByCity(hangzhou.CityCode).ResultAsync<AQIDataPublishLive[]>();
            Console.WriteLine("{0,10}|{1,5}|{2,8}|{3,14}|{4,12}", "检测站", "AQI", "等级", "主要污染物", "更新时间");
            hangzhou_stations1.ToList().ForEach(showStationLive);

           
            Console.Write("\r\n进行下一阶段演示:"); Console.ReadLine(); Console.Clear();


            Console.WriteLine($"获取指定城市{hangzhou.CityName}指定检测站前天的空气数据,并按时间升序显示");
            //创建查询杭州的其中一个检测站
            var station_qiandaohu = stations
                .Where(x => hangzhou.CityCode == x.CityCode)  //杭州的筛选
                //.Where(x => x.PositionName == "千岛湖")         //名叫千岛湖   千岛湖检测站名称已找不到,所以按名字排序随便找一个检测站
                .First();

            DateTime startTime = DateTime.Today.AddDays(-2);
            DateTime endTime = DateTime.Today.AddDays(-1);
            var stationHistory_query = publishCtx.GetIAQIDataPublishHistoriesQuery()
                .Where(x => x.StationCode == station_qiandaohu.StationCode)
                .Where(x => x.TimePoint >= startTime && x.TimePoint<endTime)
                .OrderBy(x => x.TimePoint);
            var station_qiandaohu_history = await publishCtx.Load(stationHistory_query).ResultAsync();

            Console.WriteLine("{0,13}|{1,5}|{2,8}|{3,14}|{4,12}", "检测站", "PM2.5", "PM10", "SO2", "更新时间");
            station_qiandaohu_history.ToList().ForEach(showStationHistory);
            Console.WriteLine("\r\n依赖DomainServer框架的应用查询功能就像使用本地数据库一样,是不是见到了熟悉的Linq to sql,是的使用本库想你所想");
            Console.WriteLine("演示结束");
        }


        private static void showCityLive(CityAQIPublishLive cl)
        {
            var msg = String.Format("{0,10}|{1,5}|{2,8}|{3,14}|{4,12:g}", cl.Area, cl.AQI, cl.Quality, cl.PrimaryPollutant, cl.TimePoint);
            Console.WriteLine(msg);
        }

        private static void showStationLive(AQIDataPublishLive cl)
        {
            var msg = String.Format("{0,10}|{1,5}|{2,8}|{3,14}|{4,12:g}", cl.Area + "-" + cl.PositionName, cl.AQI, cl.Quality, cl.PrimaryPollutant, cl.TimePoint);
            Console.WriteLine(msg);
        }

        private static void showStationHistory(IAQIDataPublishHistory cl)
        {
            var msg = String.Format("{0,10}|{1,5}|{2,8}|{3,14}|{4,12:g}", cl.Area + "-" + cl.PositionName, cl.IPM2_5, cl.IPM10, cl.ISO2, cl.TimePoint);
            Console.WriteLine(msg);
        }
    }
}
