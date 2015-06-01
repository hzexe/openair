using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace com.Hzexe.Air.API.Models
{
    [DataContract]
    public class Province
    {
        [Key, DataMember(Name="id")]
        public int Id { get; set; }

       
        public int? ProvinceCode { get; set; }

        
        public string ProvinceJC { get; set; }

        [StringLength(50), DataMember(Name = "name")]
        public string ProvinceName { get; set; }
        
        ICollection<City> citys { get; set; }
    }
}