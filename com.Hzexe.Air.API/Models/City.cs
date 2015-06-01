using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace com.Hzexe.Air.API.Models
{
    /// <summary>
    /// 城市对象
    /// </summary>
    [DataContract]
    public class City
    {
        /// <summary>
        /// 城市编号
        /// </summary>
        [Required]
        [DataMember(Name = "cityid"), ConcurrencyCheck, Key]
        public int CityCode { get; set; }

        [ StringLength(10), ConcurrencyCheck]
        public string CityJC { get; set; }
        /// <summary>
        /// 城市名称
        /// </summary>
        [StringLength(50), DataMember, ConcurrencyCheck, Required]
        public string CityName { get; set; }


        [ConcurrencyCheck]
        public int Id { get; set; }
        /// <summary>
        /// 对应的省编号
        /// </summary>
        [DataMember(Name = "provinceId"), ConcurrencyCheck]
        public int ProvinceId { get; set; }
    }
}