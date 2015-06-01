using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Filters;
using System.Net.Http;
using Newtonsoft.Json.Linq;


namespace com.Hzexe.Air.API
{
    public class GlobalStasusAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
                //System.Net.Http.ObjectContent
                object a;
                if (null == actionExecutedContext.Response.Content)
                {
                    ResponseModel<string> rm = new ResponseModel<string>(null);
                    actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                        HttpStatusCode.OK,
                       rm);
                }
                else if (!actionExecutedContext.Response.IsSuccessStatusCode)
                {
                    JToken j = actionExecutedContext.Response.Content.ReadAsAsync<JToken>().Result;

                    ResponseModel<JToken> rm = new ResponseModel<JToken>(j);
                    rm.statusCode = ResponseModelCode.Unknown;
                    rm.message = actionExecutedContext.Response.ReasonPhrase ;
                    actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                        HttpStatusCode.OK,
                       rm);
                
                }
                else if (actionExecutedContext.Response.TryGetContentValue<object>(out a))
                {
                    ResponseModel<object> rm = new ResponseModel<object>(a);
                    actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                        HttpStatusCode.OK,
                       rm);
                }
                else
                {
                    JToken j = actionExecutedContext.Response.Content.ReadAsAsync<JToken>().Result;

                    ResponseModel<JToken> rm = new ResponseModel<JToken>(j);
                    actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                        HttpStatusCode.OK,
                       rm);
                }


                //json序列化
                new JsonCallbackAttribute().OnActionExecuted(actionExecutedContext);

         



        }
    }
}