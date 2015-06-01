using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace com.Hzexe.Air.API.Models
{
    /// <summary>
    /// 城市即时空气质量数据
    /// </summary>
    [DataContract]
    public class CityAQIPublishLive
    {
        /// <summary>
        /// AQI
        /// </summary>
        [ConcurrencyCheck, DataMember,  StringLength(0xff)]
        public string AQI { get; set; }
        /// <summary>
        /// 一氧化碳浓度
        /// </summary>
        [DataMember,  ConcurrencyCheck, StringLength(0xff)]
        public string CO
        { get; set; }

        /// <summary>
        /// 生活建议
        /// </summary>
        [StringLength(0xff), ConcurrencyCheck, DataMember ]
        public string Measure { get; set; }
        /// <summary>
        /// 二氧化氮浓度
        /// </summary>
        [ ConcurrencyCheck, StringLength(0xff), DataMember]
        public string NO2 { get; set; }
        /// <summary>
        /// 臭氧浓度
        /// </summary>
        [ConcurrencyCheck,  StringLength(0xff), DataMember]
        public string O3 { get; set; }
        /// <summary>
        /// PM10浓度
        /// </summary>
        [ConcurrencyCheck, DataMember,  StringLength(0xff)]
        public string PM10 { get; set; }
        /// <summary>
        /// pm2.5浓度
        /// </summary>
        [StringLength(0xff),  ConcurrencyCheck, DataMember]
        public string PM2_5 { get; set; }
        /// <summary>
        /// 首要污染物
        /// </summary>
        [DataMember, StringLength(0xff), ConcurrencyCheck]
        public string PrimaryPollutant { get; set; }
        /// <summary>
        /// 质量描述
        /// </summary>
        [DataMember, StringLength(0xff), ConcurrencyCheck]
        public string Quality { get; set; }
        /// <summary>
        /// 二氧化硫 浓度
        /// </summary>
        [ ConcurrencyCheck, StringLength(0xff), DataMember]
        public string SO2 { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [ConcurrencyCheck, DataMember]
        public DateTime TimePoint { get; set; }
        /// <summary>
        /// 危害程度描述
        /// </summary>
        [DataMember, ConcurrencyCheck, StringLength(0xff)]
        public string Unheathful { get; set; }





        [DataMember(Name = "cityid"), ConcurrencyCheck, Key]
        public int CityCode { get; set; }

        
        [DataMember]
        public string Area { get; set; }
    }
}