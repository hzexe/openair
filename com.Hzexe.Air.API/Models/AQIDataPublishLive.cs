using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace com.Hzexe.Air.API.Models
{
    /// <summary>
    /// 检测点即时数据
    /// </summary>
    [DataContract]
    public class AQIDataPublishLive
    {
        /// <summary>
        /// 即时aqi
        /// </summary>
        [ StringLength(5), ConcurrencyCheck]
        public string AQI { get; set; }
        /// <summary>
        /// 城市名
        /// </summary>
        [ConcurrencyCheck,  StringLength(10)]
        public string Area { get; set; }

        /// <summary>
        /// 一氧化碳
        /// </summary>
        [StringLength(8), ConcurrencyCheck, DataMember]
        public string CO { get; set; }
        /// <summary>
        /// 一氧化碳24小时平均值
        /// </summary>
        [ConcurrencyCheck, DataMember,  StringLength(8)]
        public string CO_24h { get; set; }

        /// <summary>
        /// 当前检测点所在的纬度
        /// </summary>
        [ConcurrencyCheck,  StringLength(15), DataMember]
        public string Latitude { get; set; }
        /// <summary>
        /// 当前检测点所在的经度
        /// </summary>
        [ConcurrencyCheck,  StringLength(15), DataMember]
        public string Longitude { get; set; }
        /// <summary>
        /// 生活建议
        /// </summary>
        [StringLength(0xff), ConcurrencyCheck, DataMember]
        public string Measure { get; set; }
        /// <summary>
        /// 即时二氧化氮浓度
        /// </summary>
        [ DataMember, StringLength(5), ConcurrencyCheck]
        public string NO2 { get; set; }
        /// <summary>
        /// 二氧化氮24小时平均值
        /// </summary>
        [ConcurrencyCheck, StringLength(5),  DataMember]
        public string NO2_24h { get; set; }
        /// <summary>
        /// 即时臭氧浓度
        /// </summary>
        [StringLength(5), DataMember, ConcurrencyCheck]
        public string O3 { get; set; }
        /// <summary>
        /// 臭氧24小时平均浓度
        /// </summary>
        [DataMember, ConcurrencyCheck,  StringLength(5)]
        public string O3_24h { get; set; }
        /// <summary>
        /// 臭氧8小时平均浓度
        /// </summary>
        [ ConcurrencyCheck, StringLength(5), DataMember]
        public string O3_8h { get; set; }
        /// <summary>
        /// 这系麻意思？
        /// </summary>
        [StringLength(5), DataMember, ConcurrencyCheck]
        public string O3_8h_24h { get; set; }
        /// <summary>
        /// 即时PM10浓度
        /// </summary>
        public string PM10 { get; set; }
        /// <summary>
        /// PM10 24小时内平均浓度
        /// </summary>
        [StringLength(5), DataMember,  ConcurrencyCheck]
        public string PM10_24h { get; set; }
        /// <summary>
        /// 即时PM2.5浓度
        /// </summary>
        [StringLength(5), ConcurrencyCheck, DataMember]
        public string PM2_5 { get; set; }
        /// <summary>
        /// 麻意思
        /// </summary>
        [ ConcurrencyCheck, StringLength(5), DataMember]
        public string PM2_5_24h { get; set; }
        [Key]
        public string StationCode { get; set; }


        /// <summary>
        /// 测试点名称
        /// </summary>
        [DataMember, StringLength(40), ConcurrencyCheck]
        public string PositionName { get; set; }
        /// <summary>
        /// 首要污染物
        /// </summary>
        [ConcurrencyCheck, StringLength(0xff),  DataMember]
        public string PrimaryPollutant { get; set; }

        /// <summary>
        /// 质量描述
        /// </summary>
        [ConcurrencyCheck, DataMember, StringLength(0xff)]
        public string Quality { get; set; }
        /// <summary>
        /// 即时二氧化硫 浓度
        /// </summary>
        [StringLength(5), DataMember, ConcurrencyCheck]
        public string SO2 { get; set; }
        /// <summary>
        /// 二氧化硫24小时平均浓度
        /// </summary>
        [ ConcurrencyCheck, DataMember, StringLength(5)]
        public string SO2_24h { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [ DataMember]
        public DateTime  TimePoint { get; set; }
        /// <summary>
        /// 危害程度描述
        /// </summary>
        [ StringLength(0xff), ConcurrencyCheck, DataMember]
        public string Unheathful { get; set; }
    }
}