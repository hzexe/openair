using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.Hzexe.Air.API
{
    interface ITopBag
    {

        ResponseModelCode statusCode { get; set; }

        string message { get; set; }
    }
}
