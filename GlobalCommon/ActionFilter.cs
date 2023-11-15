using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GlobalCommon
{
    public class GlobalActionFilter: Attribute, IActionFilter
    {
        private ILog _iLogger = LogManager.GetLogger(CommonHelper.repository.Name, typeof(GlobalActionFilter));

        public GlobalActionFilter()
        {
            
        }

        //方法执行前
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(item => item.GetType() == typeof(AllowAnonymousAttribute))) //如果标记的有特殊的记号，就避开检查；
            {
                return;
            }
     
            //string token = context.HttpContext.Request.Cookies["Token"];
            string token = context.HttpContext.Request.Headers["Authorization"];
            CommResponse<object> resp = new CommResponse<object>();
            if (string.IsNullOrEmpty(token)||token.Equals("null") || token.Equals("undefined"))
            {
                //context.HttpContext.Response.Body.Write();
                resp.Code = 117;
                resp.Data = "";
                resp.Message = "token令牌验证失败,请重新登录";
                context.Result = new JsonResult(resp); 
                
                return;
            }
            UserInfo user =  JwtHelp.GetJwtDecode(token);
            if (user == null)
            {
                resp.Code = 117;
                resp.Data = "";
                resp.Message = "请重新登录";
                context.Result = new JsonResult(resp);
                return;
            }
            CommonHelper.LoginUser = user;

            if (CommonHelper.online!=null && CommonHelper.online.Count>0 && string.Concat(CommonHelper.online[user.user_id]) != context.HttpContext.Session.Id)//在其他地方登录
            {
                resp.Code = 117;
                resp.Data = "";
                resp.Message = "用户在其他地方登录，强制下线";
                context.Result = new JsonResult(resp);
                return;
            }
            
        }

        //方法执行后
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(item => item.GetType() == typeof(AllowAnonymousAttribute))) //如果标记的有特殊的记号，就避开检查；
            {
                return;
            }
            //_iLogger.Info(Newtonsoft.Json.JsonConvert.SerializeObject(context.Result));
            string token = context.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrWhiteSpace(token)&& !token.Equals("null")&& !token.Equals("undefined"))
            {
                context.HttpContext.Response.Cookies.Append("token", token, new CookieOptions() { Expires = DateTime.Now.AddMinutes(30) });
                context.HttpContext.Response.Headers.Append("Authorization ", token);
            }  
        }
    }

    public class AllowAnonymousAttribute : Attribute
    {

    }
}
