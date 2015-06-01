using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace com.Hzexe.Air.API.Models
{
    /// <summary>
    /// API访问用户授权
    /// </summary>
    [DataContract]
    public class user
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember, Key, Required]
        [Column(Order = 0)]
        public string token { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember, Key, Required]
        [Column(Order = 1)]
        public bool isok { get; set; }

        [DataMember, Required]
        public DateTime createtime { get; set; }

        [DataMember, StringLength(50)]
        public string note { get; set; }
    }
}