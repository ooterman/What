using GlobalApi.Models;
using GlobalBussiness;
using GlobalCommon;
using GlobalCommon.Models;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTaste;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalApi.Controllers
{
    public class LoginController : Controller
    {
        private ILog logger = LogManager.GetLogger(CommonHelper.repository.Name, typeof(LoginController));

        [Autowired]
        private SysBus sysBus;

        public LoginController(AutowiredService autowiredService)
        {
            autowiredService.Autowired(this);
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult Login(string loginname, string password, string code)
        {
           
            code = code.ToLower();
            string sesionCode = base.HttpContext.Session.GetString(code);
            if (string.IsNullOrEmpty(sesionCode))
                return new JsonResult(new { Code = 201, Message = "验证码过期或不存在" });
            
            if (sesionCode != code)
            {
                HttpContext.Session.Remove(code);
                return new JsonResult(new { Code = 201, Message = "验证码错误" });
            }
            HttpContext.Session.Remove(code);

            SYS_USER user = SqlHelper.scope.Queryable<SYS_USER>().First(it => it.user_account == loginname);
            if (user == null)
                return new JsonResult(new { Code = 201, Message = "账号不存在" });

            if ((password != user.password.ToLower()))
                return new JsonResult(new { Code = 201, Message = "密码错误" });

            IList<SYS_ROLE> rolelist = sysBus.GetRoleByUserId(user.user_id);
            IList<SYS_MENU> menulist = sysBus.GetMenuList(rolelist); //用户拥有的菜单

            string menuTree = CommonHelper.jsonTree(menulist);
            //menuTree = (String)JsonConvert.DeserializeObject(menuTree);

            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("userMenu", JsonConvert.DeserializeObject(menuTree));
            dic.Add("userInfo", user);
            dic.Add("userResource", "");
            dic.Add("userRole", "");
            dic.Add("sliderbar", menulist);
  
            user.login_time = DateTime.Now;
            user.access_ip= HttpContext.Connection.RemoteIpAddress.ToString();
            int rCount = SqlHelper.scope.Updateable(user).UpdateColumns(it => new { it.login_time, it.access_ip }).ExecuteCommand();
            string token = CommonHelper.createToken(loginname, password, user.user_id.ToString(), DateTime.Now);
            HttpContext.Response.Cookies.Append("token", token, new CookieOptions() { Expires = DateTime.Now.AddMinutes(30) });
            HttpContext.Response.Headers.Append("Authorization ", token);
            return new JsonResult(new { Code = 200, Message = "登录成功", Data = dic, Token = token });
        }

        [AllowAnonymous]
        [HttpPost]
        public string Logout()
        {
            HttpContext.Response.Cookies.Delete("token");
            HttpContext.Response.Cookies.Delete("Authorization");
            base.HttpContext.Session.Clear();
            return CommonHelper.ToJSON(new { code = 200, message = "ok" });
        }

        [AllowAnonymous]
        [HttpGet]
        public string GetVerifyCode()
        {
            int codeW = 80;
            int codeH = 40;
            int fontSize = 18;
            string chkCode = string.Empty;
            Color[] color = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.Brown, Color.DarkBlue };
            //字体列表，用于验证码 
            string[] font = { "Times New Roman" };
            char[] character = { '2', '3', '4', '5', '6', '8', '9', 'a', 'b', 'd', 'e', 'f', 'h', 'k', 'm', 'n', 'r', 'x', 'y', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'X', 'Y' };
            Random rnd = new Random();
            //生成验证码字符串 
            for (int i = 0; i < 4; i++)
            {
                chkCode += character[rnd.Next(character.Length)];
            }
            //HttpRuntime.Cache.Insert(chkCode.ToUpper(), chkCode, null, DateTime.Now.AddMinutes(3), System.Web.Caching.Cache.NoSlidingExpiration);
            base.HttpContext.Session.SetString(chkCode.ToLower(), chkCode.ToLower());
            string ss = base.HttpContext.Session.GetString(chkCode.ToLower());
            //创建画布
            Bitmap bmp = new Bitmap(codeW, codeH);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            //画噪线 
            for (int i = 0; i < 1; i++)
            {
                int x1 = rnd.Next(codeW);
                int y1 = rnd.Next(codeH);
                int x2 = rnd.Next(codeW);
                int y2 = rnd.Next(codeH);
                Color clr = color[rnd.Next(color.Length)];
                g.DrawLine(new Pen(clr), x1, y1, x2, y2);
            }
            //画验证码字符串 
            for (int i = 0; i < chkCode.Length; i++)
            {
                string fnt = font[rnd.Next(font.Length)];
                Font ft = new Font(fnt, fontSize);
                Color clr = color[rnd.Next(color.Length)];
                g.DrawString(chkCode[i].ToString(), ft, new SolidBrush(clr), (float)i * 18, (float)0);
            }
            //将验证码图片写入内存流，并将其以 "image/Png" 格式输出 
            MemoryStream ms = new MemoryStream();
            try
            {
                CommResponseMessage<object> mes = new CommResponseMessage<object>();
                bmp.Save(ms, ImageFormat.Png);
                //HttpContext.Response.ContentType = "image/png";
                //HttpContext.Response.BodyWriter.WriteAsync(ms.ToArray());
                //HttpContext.Response.End();

            }
            catch (Exception ex)
            {

            }
            finally
            {
                g.Dispose();
                bmp.Dispose();
            }
            return JsonConvert.SerializeObject(ms.ToArray());
        }


        public IActionResult Privacy()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
