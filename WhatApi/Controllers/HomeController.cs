using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SugarTest.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using GlobalCommon;
using GlobalCommon.Models;
using log4net;
using System.Threading;

namespace MarryRegistApi.Controllers
{
    public class HomeController : Controller
    {
        private ILog logger = LogManager.GetLogger(CommonHelper.repository.Name, typeof(HomeController));

        public HomeController()
        {
           
        }

        [AllowAnonymous]
        public string Index()
        {
      
            return "Started";
        }

        public async Task<string> TestAsync2()
        {
            await Task.Run(() => { Thread.Sleep(3000); });
            return  "成功" ;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
