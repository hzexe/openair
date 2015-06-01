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
    /// 地域相关
    /// </summary>
    //[ApiExplorerSettings(IgnoreApi = true)]
    [RoutePrefix("api/basic")]
    public class AreaController : ApiController
    {
        /// <summary>
        /// 获取所有提供空气服务的省
        /// </summary>
        /// <returns></returns>
        [Route("getAllProvinces")]
        [ResponseTypeAttribute(typeof(ResponseModel<IEnumerable<Province>>))]
        public IEnumerable<Province> getAllProvinces()
        {
            MyContext db = new MyContext();
            var q = db.Provinces.ToArray();
            db.Dispose();
            return q;
        }

        /// <summary>
        /// 根据省编号获取其下城市
        /// </summary>
        /// <param name="ProvinceId">省编号</param>
        /// <returns></returns>
        [Route("getCitysByProvinceId")]
        [ResponseTypeAttribute(typeof(ResponseModel<IEnumerable<City>>))]
        public IEnumerable<City> getCitysByProvinceId(int ProvinceId)
        {
            MyContext db = new MyContext();
            var q = db.Cities.Where(c => c.ProvinceId == ProvinceId).ToArray();
            db.Dispose();
            return q;
        }



        /// <summary>
        /// 获取所有提供空气数据的城市
        /// </summary>
        /// <returns></returns>
        [Route("getAllCitys")]
        [ResponseTypeAttribute(typeof(ResponseModel<IEnumerable<City>>))]
        public IEnumerable<City> getAllCitys()
        {
            MyContext db = new MyContext();
            var q = db.Cities.ToArray();
            db.Dispose();
            return q;
        }

    }
}