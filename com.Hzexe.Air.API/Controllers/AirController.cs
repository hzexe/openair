using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using com.Hzexe.Air.API.Models;

namespace com.Hzexe.Air.API.Controllers
{
    /// <summary>
    /// 空气数据相关
    /// </summary>
    //[ApiExplorerSettings(IgnoreApi = true)]
    [RoutePrefix("api/air")]
    public class AirController : ApiController
    {
        /// <summary>
        /// 获取指定城市的空气即时质量
        /// </summary>
        /// <param name="cityid">城市编号</param>
        /// <returns></returns>
        [Route("getCityAir")]
        [ResponseTypeAttribute(typeof(ResponseModel<CityAQIPublishLive>))]
        public CityAQIPublishLive getCityAir(int cityid)
        {
            CityAQIPublishLive r = null;
            MyContext db = new MyContext();
            r = db.CityAQIPublishLives.FirstOrDefault(a => a.CityCode == cityid);

            db.Dispose();
            return r;
        }

        /// <summary>
        /// 获取指定城市的所有检测点的空气即时质量
        /// </summary>
        /// <param name="cityid">城市编号</param>
        /// <returns></returns>
        [Route("getCityStationAir")]
        [ResponseTypeAttribute(typeof(ResponseModel<IEnumerable<AQIDataPublishLive>>))]
        public IEnumerable< AQIDataPublishLive> getCityStationAir(int cityid)
        {
            IEnumerable<AQIDataPublishLive> r = new AQIDataPublishLive[0];
            MyContext db = new MyContext();
            var q = db.Cities.Where(c => c.CityCode == cityid).FirstOrDefault();
            if (null != q)
            {
                string cityname = q.CityName;
                r = db.AQIDataPublishLives.Where(a => a.Area.Equals(cityname)).ToArray();
            }
            db.Dispose();
            return r;
        }
    }
}
