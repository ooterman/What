using GlobalBussiness;
using GlobalCommon;
using GlobalCommon.Models;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace GlobalApi.Controllers
{
    public class SysController : Controller
    {
        [Autowired]
        private CommonBus commonBus;

        [Autowired]
        private SysBus SysBus;

        private ILog logger = LogManager.GetLogger(CommonHelper.repository.Name, typeof(SysController));

        public SysController(AutowiredService autowiredService)
        {
            autowiredService.Autowired(this);
        }

       

        //获取用户类型
        [HttpGet]
        public string GetAccountType()
        {
            IList<ParamItem> list = CommonHelper.GetParamItemList(Constants.SysParamType.AccountType);
            return JsonConvert.SerializeObject(list);
        }

        //获取角色列表
        [HttpGet]
        public string GetRoleSelect(decimal roleid)
        {
            IList<SYS_ROLE> rolelist= SqlHelper.scope.Queryable<SYS_ROLE>().ToList();
            if (roleid != 0)
                rolelist = rolelist.Where(r => r.role_id != roleid).ToList();
            return JsonConvert.SerializeObject(rolelist.Select(r => new { name = r.role_name, id = string.Concat(r.role_id) }));
        }

        //菜单管理
        [HttpGet]
        public string GetMenuList()
        {
            IList<SYS_MENU> menulist = SqlHelper.scope.Queryable<SYS_MENU>().OrderBy("show_order asc").ToList();
            string menuTree = CommonHelper.jsonTree(menulist);
            var mt = JsonConvert.DeserializeObject(menuTree);
            return CommonHelper.ToJSON(new { data = mt, code = 200, message = "成功" });
        }

        [HttpGet]
        public string GetUserMenu()
        {
            IList<SYS_MENU> menulist = SysBus.GetMenuList(Convert.ToDecimal(CommonHelper.LoginUser.user_id));
            string menuTree = CommonHelper.jsonTree(menulist);
            var mt = JsonConvert.DeserializeObject(menuTree);
            return CommonHelper.ToJSON(new { data = mt, code = 200, message = "成功" });
        }

        [HttpPost]
        public string SaveMenu(SYS_MENU menu)
        {
            if (menu.menu_id != 0)
            {
                var result = SqlHelper.scope.Updateable(menu).ExecuteCommand();
                if(result==0)
                    return CommonHelper.ToJSON(new { code = 201, message = "更新失败" });
            }
            else if (menu.menu_id == 0)
            {
                menu.create_time = DateTime.Now;
                menu.create_user = CommonHelper.LoginUser.username;
                var result = SqlHelper.scope.Insertable(menu).ExecuteCommand();
                if (result == 0)
                    return CommonHelper.ToJSON(new { code = 201, message = "新增失败" });
            }
            return CommonHelper.ToJSON(new { code = 200, message = "成功" });
        }

        [HttpGet]
        public string GetMenuBtn()
        {
            int menuId = Convert.ToInt32(Request.Query["menuId"]);
            List<MENU_BUTTON> menuBtnlist = SqlHelper.scope.Queryable<MENU_BUTTON>().Where(m => m.menu_id == menuId).ToList();

            return CommonHelper.ToJSON(new { code = 200, data = menuBtnlist });
        }

        [HttpPost]
        public string DeleteMenu()
        {
            int menuId = Convert.ToInt32(Request.Form["menuId"]);
            SqlHelper.scope.BeginTran();
            SqlHelper.scope.Deleteable<ROLE_MENU>().Where(m=>m.menu_id== menuId).ExecuteCommand();
            SqlHelper.scope.Deleteable<SYS_MENU>().Where(m => m.menu_id == menuId).ExecuteCommand();
            SqlHelper.scope.CommitTran();
            return CommonHelper.ToJSON(new { code = 200 });
        }

        //角色管理
        [HttpGet]
        public string GetRoleList()
        {
            int totalCount = 0;
            DataTable table = new DataTable();
            List<Hashtable> htlist = new List<Hashtable>();
            try
            {
                int pageIndex = Convert.ToInt32(Request.Query["pageIndex"]);
                int pageSize = Convert.ToInt32(Request.Query["pageSize"]);
                string where = "parent_roleid = 0";
                string role_name = CommonHelper.ReplaceSQLChar(Request.Query["role_name"]);
                if (!string.IsNullOrWhiteSpace(role_name))
                    where = where + $" and role_name like '%{role_name}%'";
                string is_enable = CommonHelper.ReplaceSQLChar(Request.Query["is_enable"]);
                if (!string.IsNullOrWhiteSpace(is_enable))
                    where = where + $" and is_enable = {is_enable}";
              
                //List<SYS_ROLE> page = SqlHelper.scope.SqlQueryable<SYS_ROLE>($"select * from sys_role where {where}").OrderBy("role_id desc").ToPageList(pageIndex, pageSize, ref totalCount);
                List<SYS_ROLE> page = SqlHelper.scope.Queryable<SYS_ROLE>().Where(where).OrderBy("role_id desc").ToPageList(pageIndex, pageSize, ref totalCount);
                List<USER_ROLE> urAll = SqlHelper.scope.Queryable<USER_ROLE>().ToList();
                List<SYS_ROLE> childAll = SqlHelper.scope.Queryable<SYS_ROLE>().Where(p => p.parent_roleid != 0).ToList();
 
                foreach (SYS_ROLE item in page)
                {
                    Hashtable row = new Hashtable();
                    var userlist = urAll.Where(u => u.role_id == item.role_id).Select(i => i.user_id).Distinct();
                    row["role_id"] = string.Concat(item.role_id);
                    row["role_name"] = item.role_name;
                    row["parent_roleid"] = string.Concat(item.parent_roleid);
                    row["is_enable"] = item.is_enable;// Enum.GetName(typeof(Constants.IsEnable), item.is_enable);
                    row["account_type"] = CommonHelper.GetParamItemName(Constants.SysParamType.AccountType, string.Concat(item.account_type));
                    row["account_count"] = userlist.Count();
                    row["create_user"] = item.create_user;
                    row["create_time"] = item.create_time;
                    row["update_user"] = item.update_user;
                    row["update_time"] = item.update_time;
                    row["remark"] = item.remark;
                    row["has_children"] = "false";
                    var jsonchild = GetChild(urAll, item.role_id, childAll);
                    if (jsonchild.Count > 0)
                        row["has_children"] = "true";
                   
                    row["children"] = jsonchild;
                    htlist.Add(row);
                }

                return CommonHelper.ToJSON(new { code = 200, data = (htlist), count = totalCount });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { code = 201, data = (table), count = totalCount });
            }
        }

        private List<Hashtable> GetChild( List<USER_ROLE> urAll, decimal pid, List<SYS_ROLE> child)
        {
            List<SYS_ROLE> rows = child.Where(d => d.parent_roleid == pid).ToList();
            //string jsonchild = "";
            List<Hashtable> htlist = new List<Hashtable>();
            if (rows.Count > 0)
            {
                //DataTable childTable = table.Clone();
         
                foreach (SYS_ROLE chitem in rows)
                {
                    //var chrow = childTable.NewRow();
                    Hashtable chrow = new Hashtable();
                    var chuserlist = urAll.Where(u => u.role_id == chitem.role_id).Select(i => i.user_id).Distinct();
                    chrow["role_id"] = chitem.role_id;
                    chrow["role_name"] = chitem.role_name;
                    chrow["parent_roleid"] = chitem.parent_roleid;
                    chrow["is_enable"] = chitem.is_enable; // Enum.GetName(typeof(Constants.IsEnable), item.is_enable);
                    chrow["account_type"] = CommonHelper.GetParamItemName(Constants.SysParamType.AccountType, string.Concat(chitem.account_type));
                    chrow["account_count"] = chuserlist.Count();
                    chrow["create_user"] = chitem.create_user;
                    chrow["create_time"] = chitem.create_time;
                    chrow["update_user"] = chitem.update_user;
                    chrow["update_time"] = chitem.update_time;
                    chrow["remark"] = chitem.remark;
                    chrow["has_children"] = "false";
                    List<Hashtable> childjs = GetChild(urAll, chitem.role_id, rows);
                    if (childjs.Count > 0)
                        chrow["has_children"] = "true";
                    chrow["children"] = childjs;
                    htlist.Add(chrow);
                }
                //jsonchild = JsonConvert.SerializeObject(childTable);
            }
            return htlist;
        }



        [HttpPost]
        public string SaveRole(SYS_ROLE role)
        {
            try
            {
                if (role.role_id != 0)
                {
                    role.update_user = CommonHelper.LoginUser.username;
                    role.update_time = DateTime.Now;
                    var result = SqlHelper.scope.Updateable(role).IgnoreColumns(i=>new { i.create_time, i.create_user }).ExecuteCommand();
                    if (result == 0)
                        return CommonHelper.ToJSON(new { Code = 201, Message = "更新失败" });
                }
                else if (role.role_id == 0)
                {
                    role.create_user = CommonHelper.LoginUser.username;
                    role.create_time = DateTime.Now;
                    var result = SqlHelper.scope.Insertable(role).ExecuteCommand();
                    if (result == 0)
                        return CommonHelper.ToJSON(new { Code = 201, Message = "新增失败" });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { Code = 201, Message = "保存失败" });
            }
            return CommonHelper.ToJSON(new { code = 200, msg = "OK" });
        }

        [HttpPost]
        public string DeleteRole()
        {
            try
            {
                string role_id = Request.Form["role_id"];
                SqlHelper.scope.BeginTran();
                SqlHelper.scope.Deleteable<ROLE_MENU>().Where(it => it.role_id == Convert.ToDecimal(role_id)).ExecuteCommand();
                SqlHelper.scope.Deleteable<ROLE_BUTTON>().Where(it => it.role_id == Convert.ToDecimal(role_id)).ExecuteCommand();
                SqlHelper.scope.Deleteable<USER_ROLE>().Where(it => it.role_id == Convert.ToDecimal(role_id)).ExecuteCommand();
                SqlHelper.scope.Deleteable<SYS_ROLE>().Where(it => it.role_id == Convert.ToDecimal(role_id)).ExecuteCommand();
      
                SqlHelper.scope.CommitTran();
                return CommonHelper.ToJSON(new { code = 200, message = "删除成功" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { code = 201, message = "删除失败" });
            }
        }

        [HttpGet]
        public string GetRoleMoule()
        {
            string roleid = Request.Query["checkroleid"];
            List<SYS_MENU> menulist = SqlHelper.scope.Queryable<SYS_MENU>().Where(m => m.is_enable == 1).ToList();
            List<SYS_ROLE> rolelist = SqlHelper.scope.Queryable<SYS_ROLE>().Where(r => r.role_id == Convert.ToDecimal(roleid)).ToList();
            IList <SYS_MENU> userMenu = SysBus.GetMenuList(rolelist);//角色拥有的菜单
            List<MENU_BUTTON> menuBtn = SqlHelper.scope.Queryable<MENU_BUTTON>().ToList();

            IList<MENU_BUTTON> userBtn = SysBus.GetButtonList(rolelist);//角色拥有的按钮

            List<SYS_MENU> parentlist = menulist.Where(m => m.parent_menuid == 0).ToList();
            List<Hashtable> htlist = new List<Hashtable>();
            foreach (SYS_MENU menu in parentlist)
            {
                List<MENU_BUTTON> thisBtn = menuBtn.Where(m => m.menu_id == menu.menu_id).ToList();
                Hashtable row = new Hashtable();
                List<Hashtable> blist = new List<Hashtable>();
                foreach(MENU_BUTTON btn in thisBtn)
                {
                    Hashtable bht = new Hashtable();
                    bht["bname"] = btn.button_name;
                    bht["id"] = btn.button_id;
                    bht["check"] = "0";
                    int btnc = userBtn.Where(u => u.button_id == btn.button_id).Count();
                    if (btnc > 0)
                        bht["check"] = "1";
                    blist.Add(bht);
                }

                row["btnlist"] = blist;
                row["check"] = "0";
                int c = userMenu.Where(u => u.menu_id == menu.menu_id).Count();
                if (c > 0)
                    row["check"] = "1";
                row["icon"] = menu.icon;
                row["mid"] = menu.menu_id;
                row["mname"] = menu.menu_name;
                row["children"] = GetChild(menuBtn, menu.menu_id, menulist, userMenu, userBtn);
                htlist.Add(row);
            }

            return CommonHelper.ToJSON(new { code = 200, data = htlist });
        }

        [HttpPost]
        public string SaveRoleMoule()
        {
            try
            {
                string mids = Request.Form["mids"];
                string btnids = Request.Form["btnids"];
                string roleid = Request.Form["roleid"];

                List<string> midArr = new List<string>();
                List<string> btnidArr = new List<string>();
                if (!string.IsNullOrWhiteSpace(mids))
                    midArr = mids.Split(",").ToList();
                if (!string.IsNullOrWhiteSpace(btnids))
                    btnidArr = btnids.Split(",").ToList();
                List<ROLE_MENU> rmlist = new List<ROLE_MENU>();
                List<ROLE_BUTTON> rblist = new List<ROLE_BUTTON>();
                for (int i = 0; i < midArr.Count; i++)
                {
                    ROLE_MENU rm = new ROLE_MENU();
                    rm.menu_id = Convert.ToDecimal(midArr[i]);
                    rm.role_id = Convert.ToDecimal(roleid);
                    rmlist.Add(rm);
                }
                for (int i = 0; i < btnidArr.Count; i++)
                {
                    ROLE_BUTTON rb = new ROLE_BUTTON();
                    rb.button_id = Convert.ToDecimal(btnidArr[i]);
                    rb.role_id = Convert.ToDecimal(roleid);
                    rblist.Add(rb);
                }
                SqlHelper.scope.BeginTran();
                SqlHelper.scope.Deleteable<ROLE_MENU>().Where(it => it.role_id ==Convert.ToDecimal(roleid)).ExecuteCommand();
                SqlHelper.scope.Deleteable<ROLE_BUTTON>().Where(it => it.role_id == Convert.ToDecimal(roleid)).ExecuteCommand();
                SqlHelper.scope.Insertable(rmlist).ExecuteCommand();
                SqlHelper.scope.Insertable(rblist).ExecuteCommand();
                SqlHelper.scope.CommitTran();

                return CommonHelper.ToJSON(new { code = 200, message = "保存成功" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { code = 201, message = "保存失败" });
            }
        }

        [HttpPost]
        public string EnableRole()
        {
            try
            {
                SYS_ROLE role = new SYS_ROLE();
                role.is_enable = Convert.ToInt16(Request.Form["status"]);
                role.role_id = Convert.ToDecimal(Request.Form["id"]);
                //string status = Request.Form["status"];
                //string id = Request.Form["id"];
                //SqlHelper.scope.Updateable<SYS_ROLE>()
                SqlHelper.scope.Updateable(role).UpdateColumns(i => new { i.is_enable }).ExecuteCommand();
                return CommonHelper.ToJSON(new { code = 200, message = "保存成功" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { code = 201, message = "保存失败" });
            }
        }

        private List<Hashtable> GetChild(List<MENU_BUTTON> menuBtn, decimal pid, List<SYS_MENU> child, IList<SYS_MENU> userMenu, IList<MENU_BUTTON> userBtn)
        {
            List<SYS_MENU> rows = child.Where(d => d.parent_menuid == pid).ToList();
            List<Hashtable> htlist = new List<Hashtable>();

            foreach (SYS_MENU chitem in rows)
            {
                Hashtable chrow = new Hashtable();
                List<MENU_BUTTON> thisBtn = menuBtn.Where(m => m.menu_id == chitem.menu_id).ToList();
                List<Hashtable> blist = new List<Hashtable>();
                foreach (MENU_BUTTON btn in thisBtn)
                {
                    Hashtable bht = new Hashtable();
                    bht["bname"] = btn.button_name;
                    bht["id"] = btn.button_id;
                    bht["check"] = "0";
                    int btnc = userBtn.Where(u => u.button_id == btn.button_id).Count();
                    if (btnc > 0)
                        bht["check"] = "1";
                    blist.Add(bht);
                }
                chrow["btnlist"] = blist;
                chrow["check"] = "0";
                int c = userMenu.Where(u => u.menu_id == chitem.menu_id).Count();
                if (c > 0)
                    chrow["check"] = "1";
                chrow["icon"] = chitem.icon;
                chrow["mid"] = chitem.menu_id;
                chrow["mname"] = chitem.menu_name;
                chrow["children"] = GetChild(menuBtn, chitem.menu_id, rows, userMenu, userBtn);

                htlist.Add(chrow);
            }
            return htlist;
        }


        [HttpGet]
        public string GetUserlist()
        {
            int totalCount = 0;
            List<Hashtable> htlist = new List<Hashtable>();

            try
            {
                int pageIndex = string.IsNullOrWhiteSpace(Request.Query["pageIndex"]) ? 1 : Convert.ToInt32(Request.Query["pageIndex"]);
                int pageSize = string.IsNullOrWhiteSpace(Request.Query["pageSize"]) ? 10 : Convert.ToInt32(Request.Query["pageSize"]);

                string where = "1=1";
                string user_account = Request.Query["user_account"];
                if (!string.IsNullOrWhiteSpace(user_account))
                    where = where + $" and user_account like '%{user_account}%'";
                string user_name = Request.Query["user_name"];
                if (!string.IsNullOrWhiteSpace(user_name))
                    where = where + $" and user_name like '%{user_name}%'";
                string roleid = Request.Query["roleid"];
                if (!string.IsNullOrWhiteSpace(roleid))
                    where = where + $" and user_id in (select user_id from user_role where role_id = {roleid})";
                string is_enable = Request.Query["is_enable"];
                if (!string.IsNullOrWhiteSpace(is_enable))
                    where = where + $" and is_enable = {is_enable}";
                string organ_id = Request.Query["organ_id"];
                if (!string.IsNullOrWhiteSpace(organ_id))
                    where = where + $" and organ_id = {organ_id}";
                string account_type = Request.Query["account_type"];
                if (!string.IsNullOrWhiteSpace(account_type))
                    where = where + $" and account_type = {account_type}";

                List<SYS_USER> page = SqlHelper.scope.Queryable<SYS_USER>().Where(where).OrderBy("user_id desc").ToPageList(pageIndex, pageSize, ref totalCount);
                List<SYS_ROLE> role = SqlHelper.scope.Queryable<SYS_ROLE>().ToList();
                List<USER_ROLE> urlist = SqlHelper.scope.Queryable<USER_ROLE>().ToList();

                List<USER_DEVICE> udlist = SqlHelper.scope.Queryable<USER_DEVICE>().ToList();

                foreach (SYS_USER item in page)
                {
                    Hashtable row = new Hashtable();
                    List<USER_DEVICE> thisudlist = udlist.Where(u => u.user_id == item.user_id).ToList();
                    row["user_account"] = item.user_account;
                    row["user_id"] = item.user_id;
                    row["user_name"] = item.user_name;
                    row["mobile"] = item.mobile;
                    row["account_type"] = string.Concat(item.account_type);
                    row["account_typename"] = CommonHelper.GetParamItemName(Constants.SysParamType.AccountType, string.Concat(item.account_type));
                    List<USER_ROLE> thisur = urlist.Where(u => u.user_id == item.user_id).ToList();
                    var ids = thisur.Select(t => t.role_id).ToList();
                    List<SYS_ROLE> thisrole = role.Where(r => ids.Contains(r.role_id)).ToList();
                    string rolename = string.Join(",", thisrole.Select(t => t.role_name));
                    row["role"] = rolename;
                    row["roleid"] = string.Join(",", thisrole.Select(t => t.role_id));
                    row["is_enable"] = item.is_enable;
                    row["create_user"] = item.create_user;
                    row["create_time"] = string.Format("{0:g}", item.create_time);
                    int dev_count = thisudlist.Select(t => t.device_number).Distinct().Count();
                    row["dev_count"] = dev_count;
                    row["organ_id"] = item.organ_id;
                    row["organ_name"] = item.organ_name;
                    htlist.Add(row);
                }

                return CommonHelper.ToJSON(new { code = 200, data = htlist, count = totalCount });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { code = 201, data = htlist, count = totalCount });
            }
        }

        [HttpPost]
        public string SaveUser(SYS_USER user)
        {
            try
            {
                if (user.user_id != 0)
                {
                    var result = SqlHelper.scope.Updateable(user).IgnoreColumns(i => new { i.create_time, i.create_user, i.access_ip, i.password }).ExecuteCommand();
                    if (result == 0)
                        return CommonHelper.ToJSON(new { code = 201, message = "更新失败" });
                }
                else if (user.user_id == 0)
                {
                    user.create_time = DateTime.Now;
                    user.create_user = CommonHelper.LoginUser.username;
                    user.password = CommonHelper.GetMd5("123456" + "ma$r@r*y");//默认密码123456
                    user.user_id = SqlHelper.scope.Insertable(user).ExecuteReturnIdentity();
                    if (user.user_id == 0)
                        return CommonHelper.ToJSON(new { code = 201, message = "新增失败" });
                }

                if (!string.IsNullOrWhiteSpace(Request.Form["roleid"]))
                {
                    var roleid = Convert.ToDecimal(Request.Form["roleid"]);
                    USER_ROLE ur = new USER_ROLE();
                    ur.role_id = roleid;
                    ur.user_id = user.user_id;
                    SqlHelper.scope.Deleteable<USER_ROLE>().Where(it => it.user_id == user.user_id).ExecuteCommand();
                    var r = SqlHelper.scope.Insertable(ur).ExecuteCommand();
                    if (r == 0)
                        return CommonHelper.ToJSON(new { code = 201, message = "分配角色失败" });
                }

                var files = Request.Form.Files.GetFiles("signfile");
                string url = CommonHelper.GetConfiguration()["UserSignFile"];
                foreach (IFormFile file in files)
                {
                    SYS_FILE sfile = SysBus.UploadFile(file, string.Concat(user.user_id), Constants.BussinessType.用户信息.GetHashCode(),
                        Constants.FileType.签名.GetHashCode(), url);
                    user.signfile_id = sfile.file_id;
                    SqlHelper.scope.Updateable(user).UpdateColumns(i => new { i.signfile_id }).ExecuteCommand();
                    
                }

                return CommonHelper.ToJSON(new { code = 200, message = "保存成功", user_id = user.user_id });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { code = 201, message = "保存失败" });
            }
         
        }

        [HttpPost]
        public string EnableUser()
        {
            try
            {
                SYS_USER user = new SYS_USER();
                user.is_enable = Convert.ToInt16(Request.Form["status"]);
                user.user_id = Convert.ToDecimal(Request.Form["id"]);
                //string status = Request.Form["status"];
                //string id = Request.Form["id"];
                //SqlHelper.scope.Updateable<SYS_ROLE>()
                SqlHelper.scope.Updateable(user).UpdateColumns(i => new { i.is_enable }).ExecuteCommand();
                return CommonHelper.ToJSON(new { code = 200, message = "保存成功" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { code = 201, message = "保存失败" });
            }
        }


        [HttpPost]
        public string DeleteUser()
        {
            try
            {
                string user_id = Request.Form["id"];
                SqlHelper.scope.BeginTran();
                SqlHelper.scope.Deleteable<USER_ROLE>().Where(it => it.user_id == Convert.ToDecimal(user_id)).ExecuteCommand();
                SqlHelper.scope.Deleteable<SYS_USER>().Where(it => it.user_id == Convert.ToDecimal(user_id)).ExecuteCommand();
                SqlHelper.scope.CommitTran();
                return CommonHelper.ToJSON(new { code = 200, message = "删除成功" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { code = 201, message = "删除失败" });
            }
        }

        [HttpPost]
        public string ReSetPassword()
        {
            try
            {
                SYS_USER user = new SYS_USER();
                user.user_id =Convert.ToDecimal(Request.Form["id"]);
                user.password = CommonHelper.GetMd5("123456" + "ma$r@r*y");
                SqlHelper.scope.Updateable(user).UpdateColumns(i => new { i.password }).ExecuteCommand();
                return CommonHelper.ToJSON(new { code = 200, message = "操作成功" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { code = 201, message = "操作失败" });
            }
        }

        [HttpPost]
        public ActionResult DownloadFile()
        {
            try
            {
                string id = Request.Form["id"];
                if (string.IsNullOrEmpty(id))
                    return null;
                SYS_FILE file = SqlHelper.scope.Queryable<SYS_FILE>().First(f => f.file_id == Convert.ToDecimal(id));
                FileStream fileStream = new FileStream(file.file_url, FileMode.Open);
                string fileExt = Path.GetExtension(file.file_url);
                //获取文件的ContentType
                var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

                var memi = provider.Mappings[fileExt];
                return File(fileStream, memi, file.file_name);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        public  string GetFileContentType(string fileExtension)
        {
            string text = "";
            if (fileExtension == null)
            {
                throw new ArgumentException(fileExtension + "不能为空。");
            }

            if (fileExtension.Substring(0, 1) != ".")
            {
                fileExtension = "." + fileExtension;
            }

            switch (fileExtension.ToLower())
            {
                case ".asf":
                    return "video/x-ms-asf";
                case ".avi":
                    return "video/avi";
                case ".doc":
                    return "application/ms-word";
                case ".zip":
                    return "application/zip";
                case ".xls":
                    return "application/ms-excel";
                case ".gif":
                    return "image/gif";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".wav":
                    return "audio/wav";
                case ".mp3":
                    return "audio/mpeg3";
                case ".mpg":
                case ".mpeg":
                    return "video/mpeg";
                case ".rtf":
                    return "application/rtf";
                case ".htm":
                case ".html":
                    return "text/html";
                case ".txt":
                    return "text/plain";
                case ".apk":
                    return "application/vnd.android.package-archive";
                case ".ipa":
                    return "application/vnd.iphone";
                case ".svg":
                    return "text/xml";
                default:
                    return "application/octet-stream";
            }
        }

       
        [HttpPost]
        public ActionResult DownloadFile2()
        {

            string id = Request.Form["id"];
            if (string.IsNullOrEmpty(id))
                return null;
            SYS_FILE file = SqlHelper.scope.Queryable<SYS_FILE>().First(f => f.file_id == Convert.ToDecimal(id));
            FileStream fileStream = new FileStream(file.file_url, FileMode.Open);
            string ext = Path.GetExtension(file.file_url);
            string contentType = GetFileContentType(ext);
            var actionresult = new FileStreamResult(fileStream, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(contentType));
            actionresult.FileDownloadName = file.file_name;
            //Response.ContentLength = res.Length;
            return actionresult;
            //return File(fileStream, contentType, Path.GetFileName(file.file_url));

        }

        [HttpPost]
        public string ChangePassword()
        {
            try
            {
                string oldPas = Request.Form["oldPas"];
                string newPas = Request.Form["newPas"];

                string userId = Request.Form["user_id"];
                SYS_USER user = SqlHelper.scope.Queryable<SYS_USER>().First(u => u.user_id == Convert.ToDecimal(userId));
                if(!user.password.Equals(oldPas))
                    return CommonHelper.ToJSON(new { code = 201, message = "原密码不正确" });
                SqlHelper.scope.Updateable<SYS_USER>().SetColumns(u => u.password == newPas).Where(u => u.user_id == Convert.ToDecimal(userId)).ExecuteCommand();
                return CommonHelper.ToJSON(new { code = 200, message = "修改成功" });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return CommonHelper.ToJSON(new { code = 201, message = "修改失败" });
            }
        }
    }
}
