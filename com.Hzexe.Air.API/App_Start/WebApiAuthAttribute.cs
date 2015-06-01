using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using com.Hzexe.Air.API.Models;

namespace com.Hzexe.Air.API
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class WebApiAuthAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {

        // OnActionExecuted 在执行操作方法后由 ASP.NET MVC 框架调用。
        // OnActionExecuting 在执行操作方法之前由 ASP.NET MVC 框架调用。
        // OnResultExecuted 在执行操作结果后由 ASP.NET MVC 框架调用。
        // OnResultExecuting 在执行操作结果之前由 ASP.NET MVC 框架调用。

        /// <summary>
        /// 在执行操作方法之前由 ASP.NET MVC 框架调用。
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            string token = System.Web.HttpContext.Current.Request.QueryString["token"];

            if (!System.Web.HttpContext.Current.Request.IsSecureConnection)
            {
                throw new WebApiHandleException(ResponseModelCode.SSLRequired, "请使用https协议请求");
            }
            else if (string.IsNullOrEmpty(token))
            {
                throw new WebApiHandleException(ResponseModelCode.NoTokenArgument, "却少参数token");
            }
            else
            {
                MyContext db = new MyContext();
                var q = db.users.FirstOrDefault(u => u.token.Equals(token));
                db.Dispose();
                if (null == q)
                    throw new WebApiHandleException(ResponseModelCode.Unauthorized, "token错误");
                else if (!q.isok.Value)
                    throw new WebApiHandleException(ResponseModelCode.Unauthorized, "token已被停用");
            }
            base.OnActionExecuting(filterContext);


            //var fcinfo = new filterContextInfo(filterContext);
            //fcinfo.actionName;//获取域名
            //fcinfo.controllerName;获取 controllerName 名称

            bool isstate = true;
            //islogin = false;
            if (isstate)//如果满足
            {
                //逻辑代码
                // filterContext.Result = new HttpUnauthorizedResult();//直接URL输入的页面地址跳转到登陆页  
                // filterContext.Result = new RedirectResult("http://www.baidu.com");//也可以跳到别的站点
                //filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { Controller = "product", action = "Default" }));
            }
            else
            {
                // filterContext
            }

        }
    }
}