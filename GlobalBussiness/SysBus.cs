using GlobalCommon;
using GlobalCommon.Models;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalBussiness
{
    [AppService(ServiceLifetime.Transient)]
    public class SysBus
    {

        private ILog logger = LogManager.GetLogger(CommonHelper.repository.Name, typeof(SysBus));
        /// <summary>
        /// 用户拥有的角色
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public IList<SYS_ROLE> GetRoleByUserId(decimal user_id)
        {
            IList<USER_ROLE> urlist = SqlHelper.scope.Queryable<USER_ROLE>().Where(i => i.user_id == user_id).ToList();
            decimal[] roleid = urlist.Select(u => u.role_id).ToArray();
            IList<SYS_ROLE> rolelist = SqlHelper.scope.Queryable<SYS_ROLE>().Where(i => roleid.Contains(i.role_id)).ToList();//用户拥有的角色
            return rolelist;
        }

        /// <summary>
        /// 用户拥有的菜单
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public IList<SYS_MENU> GetMenuList(decimal user_id)
        {
            IList<SYS_ROLE> rolelist = GetRoleByUserId(user_id);
            IList<SYS_MENU> menulist = GetMenuList(rolelist);//用户拥有的菜单

            return menulist;
        }

        /// <summary>
        /// //角色拥有的菜单
        /// </summary>
        /// <param name="rolelist"></param>
        /// <returns></returns>
        public IList<SYS_MENU> GetMenuList(IList<SYS_ROLE> rolelist)
        {
            decimal[] roleid = rolelist.Select(u => u.role_id).ToArray();
            IList<ROLE_MENU> rmlist = SqlHelper.scope.Queryable<ROLE_MENU>().Where(i => roleid.Contains(i.role_id)).ToList();
            decimal[] menuid = rmlist.Select(r => r.menu_id).ToArray();
            IList<SYS_MENU> menulist = SqlHelper.scope.Queryable<SYS_MENU>().Where(i => menuid.Contains(i.menu_id) && i.is_enable == 1).OrderBy("show_order asc").ToList();//用户拥有的菜单

            return menulist;
        }

        /// <summary>
        /// //角色拥有的按钮
        /// </summary>
        public IList<MENU_BUTTON> GetButtonList(IList<SYS_ROLE> rolelist)
        {
            decimal[] roleid = rolelist.Select(u => u.role_id).ToArray();
            IList<ROLE_BUTTON> rblist = SqlHelper.scope.Queryable<ROLE_BUTTON>().Where(i => roleid.Contains(i.role_id)).ToList();
            decimal[] btnid = rblist.Select(r => r.button_id).ToArray();
            IList<MENU_BUTTON> btnlist = SqlHelper.scope.Queryable<MENU_BUTTON>().Where(i => btnid.Contains(i.button_id)).ToList();//用户拥有的按钮
            return btnlist;
        }

        /// <summary>
        /// 用户拥有的按钮
        /// </summary>
        public IList<MENU_BUTTON> GetButtonList(decimal user_id)
        {
            IList<SYS_ROLE> rolelist = GetRoleByUserId(user_id);
            IList<MENU_BUTTON> btnlist = GetButtonList(rolelist);

            return btnlist;
        }


        public SYS_FILE UploadFile(IFormFile file,string bussiness_id, int bussiness_type,int file_type,string url)
        {
            SYS_FILE sfile = new SYS_FILE();
            try
            {
   
                sfile.file_name = file.FileName;
                sfile.file_type = file_type;
                //sfile.file_url = url;
                sfile.bussiness_id = bussiness_id;
                sfile.bussiness_type = bussiness_type;
                sfile.create_time = DateTime.Now;
                string ext = Path.GetExtension(file.FileName);
                string filename = string.Format("{0}_{1}{2}", Guid.NewGuid().ToString().Replace("-", "").Substring(0, 32), DateTime.Now.ToString("yyyyMMdd"), ext);
                sfile.file_url = string.Format("{0}\\{1}",url, filename);
                if (!Directory.Exists(url))
                {
                    Directory.CreateDirectory(url);
                }
                using (var stream = new FileStream(sfile.file_url, FileMode.Create))      //创建特定名称的文件流
                {
                    file.CopyToAsync(stream);     //上传文件
                }
                sfile.file_id = SqlHelper.scope.Insertable(sfile).ExecuteReturnIdentity();
                return sfile;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return sfile;
            }
       
        }
    }
}
