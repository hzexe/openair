using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace com.Hzexe.Air.API
{
    /// <summary>
    /// 响应数据体
    /// </summary>
    /// <typeparam name="T"响应有效类型</typeparam>
    [DataContract]
    public class ResponseModel<T> : ITopBag
    {
        /// <summary>
        /// 响应代码0为正常,其它请查看ResponseModelCode类型
        /// </summary>
        [DataMember]
        public ResponseModelCode statusCode { get; set; }
        /// <summary>
        /// 如果为异常，可能会带有异常描述
        /// </summary>
        [DataMember]
        public string message { get; set; }
        /// <summary>
        /// 响应代码为0时的有效果荷载，否则为null
        /// </summary>
        [DataMember]
        public T data { get; set; }

        [Obsolete("仅用来支持序列化", true)]
        public ResponseModel()
        {

        }


        public ResponseModel(T data)
        {
            this.statusCode = ResponseModelCode.Ok;
            this.data = data;
        }

        public ResponseModel(ResponseModelCode rmc, string message = null)
        {
            this.statusCode = rmc;
            this.message = message;
        }

    }

    /// <summary>
    /// 响应代码
    /// </summary>
    [DataContract]
    public enum ResponseModelCode
    {
        /// <summary>
        /// 正常响应
        /// </summary>
        [EnumMember]
        Ok = 0,
        /// <summary>
        /// 服务器发生错误
        /// </summary>
        [EnumMember]
        InternalServerError = 1,
        /// <summary>
        /// 未认证的访问，可能token无效
        /// </summary>
        [EnumMember]
        Unauthorized = 2,
        /// <summary>
        /// 却少token参数
        /// </summary>
        [EnumMember]
        NoTokenArgument = 3,
        /// <summary>
        /// 必须使用https的安全请求
        /// </summary>
        [EnumMember]
        SSLRequired=4,

        /// <summary>
        /// 未知的异常
        /// </summary>
        [EnumMember]
        Unknown = 999
    }


}