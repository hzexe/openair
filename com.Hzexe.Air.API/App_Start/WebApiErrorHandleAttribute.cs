using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;

namespace com.Hzexe.Air.API
{
    public class WebApiErrorHandleAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            //Logger.Error(actionExecutedContext.Exception);
            //actionExecutedContext.ActionContext.ModelState.
           
            ResponseModel<string> rm;
            if (actionExecutedContext.Exception is WebApiHandleException)
            {
                WebApiHandleException ex=actionExecutedContext.Exception as WebApiHandleException;
                rm = new ResponseModel<string>(ex.statusCode, ex.Message);
            }
            else
            {
          rm = new ResponseModel<string>(ResponseModelCode.InternalServerError,actionExecutedContext.Exception.Message);
              
            }

            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                HttpStatusCode.OK,
               rm);
            //json序列化
            new JsonCallbackAttribute().OnActionExecuted(actionExecutedContext);
            //actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse<string>(HttpStatusCode.OK, null);
        }
    }

    public class WebApiHandleException : Exception
    {
        

        public ResponseModelCode statusCode { get; set; }

        public WebApiHandleException(ResponseModelCode statusCode, Exception ex)
            : base(ex.Message, ex)
        {
            this.statusCode = statusCode;
        }
        public WebApiHandleException(ResponseModelCode statusCode, string msg)
            : base(msg)
        {
            this.statusCode = statusCode;
        }
    }

}