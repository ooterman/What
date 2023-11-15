using GlobalCommon.Models;
using log4net;
using log4net.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace GlobalCommon
{
    public static class CommonHelper
    {
        public static Hashtable online=new Hashtable();

        public static ILoggerRepository repository = LogManager.CreateRepository("MarryRegist");

        public static UserInfo LoginUser {get ;set;}
        public static IConfigurationRoot GetConfiguration()
        {
            var config = new ConfigurationBuilder()
             .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
             .Build();
            return config;
        }

        public static DataTable Convert2DataTable(List<Hashtable> list)
        {
            DataTable dt = new DataTable();
            if (list.Count == 0)
                return dt;

            ArrayList akeys = new ArrayList(list[0].Keys);
            foreach (string name in akeys)
                dt.Columns.Add(name);

            for (int i = 0; i < list.Count; i++)
            {
                DataRow dr = dt.NewRow();
                Hashtable ht = (Hashtable)list[i];
                ArrayList keys = new ArrayList(ht.Keys);
                foreach (string skey in keys)
                {
                    if (ht[skey] == null || (ht[skey] != null && string.IsNullOrEmpty(ht[skey].ToString())))
                    {
                        dr[skey] = null;
                    }
                    else
                    {

                        dr[skey] = ht[skey].ToString().Equals("System.Collections.Hashtable") ? ((Hashtable)ht[skey])["value"] : ht[skey];

                    }

                }
                dt.Rows.Add(dr);
            }
            //foreach (Hashtable item in list)
            //{
            //    dt.Rows.
            //}
            //    dt.Rows.Add(new ArrayList(item.Values).ToArray());

            return dt;
        }

        public static string GetEncryptResult(string data, string key)
        {
            HMACMD5 source = new HMACMD5(Encoding.UTF8.GetBytes(key));
            byte[] buff = source.ComputeHash(Encoding.UTF8.GetBytes(data));
            string result = string.Empty;
            for (int i = 0; i < buff.Length; i++)
            {
                result += buff[i].ToString("X2"); // hex format
            }
            return result;
        }

        public static string GetBase64FromImage(string imagefile)
        {
            string strbaser64 = "";
            if (!string.IsNullOrWhiteSpace(imagefile))
            {
                try
                {
                    Bitmap bmp = new Bitmap(imagefile);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] arr = new byte[ms.Length];
                        ms.Position = 0;
                        ms.Read(arr, 0, (int)ms.Length);
                        ms.Close();

                        strbaser64 = Convert.ToBase64String(arr);
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
            return strbaser64;
        }

        public static Bitmap CreateVerifyCode(out string code)
        {
            //建立Bitmap对象，绘图
            Bitmap bitmap = new Bitmap(200, 60);
            Graphics graph = Graphics.FromImage(bitmap);
            graph.FillRectangle(new SolidBrush(Color.White), 0, 0, 200, 60);
            Font font = new Font(FontFamily.GenericSerif, 48, FontStyle.Bold, GraphicsUnit.Pixel);
            Random r = new Random();
            string letters = "ABCDEFGHIJKLMNPQRSTUVWXYZ0123456789";

            StringBuilder sb = new StringBuilder();

            //添加随机的五个字母
            for (int x = 0; x < 4; x++)
            {
                string letter = letters.Substring(r.Next(0, letters.Length - 1), 1);
                sb.Append(letter);
                graph.DrawString(letter, font, new SolidBrush(Color.Black), x * 38, r.Next(0, 15));
            }
            code = sb.ToString();

            //混淆背景
            Pen linePen = new Pen(new SolidBrush(Color.Black), 2);
            for (int x = 0; x < 6; x++)
                graph.DrawLine(linePen, new Point(r.Next(0, 199), r.Next(0, 59)), new Point(r.Next(0, 199), r.Next(0, 59)));
            return bitmap;
        }

 
        public static string ToJson(DataTable dt)
        {
            ArrayList arrayList = new ArrayList();
            foreach (DataRow dataRow in dt.Rows)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    dictionary.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToString());
                }
                arrayList.Add(dictionary); //ArrayList集合中添加键值
            }
            return JsonConvert.SerializeObject(arrayList);  //返回一个json字符串
        }

        public static string StringToJson(string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '/':
                        sb.Append("\\/");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        public static string createToken(string username, string pwd,string user_id, DateTime now)
        {
            string result = string.Empty;

            var payload = new Dictionary<string, object>
                {
                    { "username",username },
                    { "create_time",now },
                    { "user_id",user_id }
                };

            result = JwtHelp.SetJwtEncode(payload);

            return result;
            //get请求需要修改成这样
            //return Json(result,JsonRequestBehavior.AllowGet);
        }



        public static string jsonTree(IList<SYS_MENU> mlist)
        {
            string _menu = string.Empty;
            StringBuilder sb = new StringBuilder();
            IList<SYS_MENU> parent = mlist.Where(d => d.parent_menuid == 0).ToList();
            sb.Append("[");
            bool isFist = false;
            foreach (SYS_MENU dr in parent)
            {
                if (isFist)
                {
                    sb.Append(",");
                }
                isFist = true;
                sb.Append("{");
                sb.AppendFormat("\"id\":\"{0}\",", dr.menu_id);
                sb.AppendFormat("\"name\":\"{0}\",", dr.menu_name);
                sb.AppendFormat("\"pid\":\"{0}\",", dr.parent_menuid);
                sb.AppendFormat("\"iconCls\":\"{0}\",", dr.icon);
                sb.AppendFormat("\"sOrder\":\"{0}\",", dr.show_order);
                sb.AppendFormat("\"is_enable\":\"{0}\",", dr.is_enable);
                sb.AppendFormat("\"remark\":\"{0}\",", dr.remark);
                sb.AppendFormat("\"url\":\"{0}\"", dr.menu_url);
                sb.Append(",\"children\":[");
                sb.Append(GetSubMenu(dr.menu_id, mlist));
                sb.Append("]");
                sb.Append("}");
            }
            sb.Append("]");
            _menu = sb.ToString();
            return JsonConvert.SerializeObject(_menu);
        }

        public static Dictionary<TKey, TValue> DeserializeStringToDictionary<TKey, TValue>(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return new Dictionary<TKey, TValue>();

            Dictionary<TKey, TValue> jsonDict = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(jsonStr);

            return jsonDict;

        }

        /// <summary>
        /// 将字典类型序列化为json字符串
        /// </summary>
        /// <typeparam name="TKey">字典key</typeparam>
        /// <typeparam name="TValue">字典value</typeparam>
        /// <param name="dict">要序列化的字典数据</param>
        /// <returns>json字符串</returns>
        public static string SerializeDictionaryToJsonString<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            if (dict.Count == 0)
                return "";

            string jsonStr = JsonConvert.SerializeObject(dict);
            return jsonStr;
        }
        /// <summary>
        /// 递归调用生成无限级别
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static string GetSubMenu(decimal pid, IList<SYS_MENU> dt)
        {
            StringBuilder sb = new StringBuilder();
            IList<SYS_MENU> rows = dt.Where(d=>d.parent_menuid==pid).ToList();
            if (rows.Count > 0)
            {
                bool isFist = false;
                foreach (SYS_MENU dr in rows)
                {
                    if (isFist)
                        sb.Append(",");
                    isFist = true;
                    sb.Append("{");
                    sb.AppendFormat("\"id\":\"{0}\",", dr.menu_id);
                    sb.AppendFormat("\"name\":\"{0}\",", dr.menu_name);
                    sb.AppendFormat("\"pid\":\"{0}\",", dr.parent_menuid);
                    sb.AppendFormat("\"iconCls\":\"{0}\",", dr.icon);
                    sb.AppendFormat("\"is_enable\":\"{0}\",", dr.is_enable);
                    sb.AppendFormat("\"remark\":\"{0}\",", dr.remark);
                    sb.AppendFormat("\"sOrder\":\"{0}\",", dr.show_order);
                    sb.AppendFormat("\"url\":\"{0}\"", dr.menu_url);
         
                    sb.Append(",\"children\":[");
                    sb.Append(GetSubMenu(dr.menu_id, dt));
                    sb.Append("]");
                    sb.Append("}");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        ///  把本地图片以文件流保存
        ///  byte[] by = Common.SaveImage("D:\\safemon\\111.jpg");
        ///  int a = ExamAuditApi.DAL.TRA_USERDAL.UpateTest(by);
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns></returns>
        public static byte[] SaveImage(String path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read); //将图片以文件流的形式进行保存
            BinaryReader br = new BinaryReader(fs);
            byte[] imgBytesIn = br.ReadBytes((int)fs.Length);  //将流读入到字节数组中
            return imgBytesIn;
        }
        /// <summary>
        /// 将字符串分割为list数组
        /// </summary>
        /// <param name="strip">string like "[\r\n  \"1\",\r\n  \"2\",\r\n  \"3\",\r\n  \"4\"\r\n]"</param>
        /// <returns></returns>
        public static List<string> SplitToList(string strip)
        {
            List<string> list = new List<string>();
            int j = 0;
            foreach (var item in strip.Split(',').ToList())
            {
                list.Add(item.Split(new string[] { "\r\n", "[", "]", "\"" }, StringSplitOptions.None).Select(a => a).Where(a => a.Trim() != "").ToList()[0]);
                j++;
            }

            return list;
        }

        public static string UrlEncode(string param, bool isUpper = false)
        {
            return UrlEncode(param, Encoding.UTF8, isUpper);
        }

        /// <summary>
        /// 对Url进行编码
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="encoding">字符编码</param>
        /// <param name="isUpper">编码字符是否转成大写,范例,"http://"转成"http%3A%2F%2F"</param>
        public static string UrlEncode(string param, Encoding encoding, bool isUpper = false)
        {
            var result = HttpUtility.UrlEncode(param, encoding);
            if (!isUpper)
                return result;
            return GetUpperEncode(result);
        }

        /// <summary>
        /// 获取大写编码字符串
        /// </summary>
        private static string GetUpperEncode(string encode)
        {
            var result = new StringBuilder();
            int index = int.MinValue;
            for (int i = 0; i < encode.Length; i++)
            {
                string character = encode[i].ToString();
                if (character == "%")
                    index = i;
                if (i - index == 1 || i - index == 2)
                    character = character.ToUpper();
                result.Append(character);
            }
            return result.ToString();
        }

  
        public static string Encrypt(string msg)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            byte[] inArray = Encrypt(bytes);
            string val = Convert.ToBase64String(inArray);
            val = ToBase64(val);

            //去掉最后的“=”字符
            if (val.EndsWith("=")) val = val.Replace("=", "");
            return val;
        }

        public static byte[] Encrypt(byte[] inputByte)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                TripleDES mydes = new TripleDESCryptoServiceProvider();
               
                ICryptoTransform transform = mydes.CreateEncryptor();
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(inputByte, 0, inputByte.Length);
                    cryptoStream.FlushFinalBlock();
                }

                return memoryStream.ToArray();
            }
        }


        public static string ToBase64(string inputStr)
        {
            string result = string.Empty;
            if (!string.IsNullOrWhiteSpace(inputStr))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(inputStr);
                if (bytes != null)
                {
                    result = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
                }

                bytes = null;
            }

            return result;
        }


        public static string Decrypt(string msg)
        {
            msg = FromBase64(msg);
            string empty = string.Empty;
            if (string.IsNullOrWhiteSpace(msg))
            {
                return empty;
            }

            byte[] array = Convert.FromBase64String(msg);
            if (array == null || array.Length == 0)
            {
                return empty;
            }

            byte[] array2 = Decrypt(array);
            if (array2 == null && array2.Length == 0)
            {
                return empty;
            }

            return Encoding.UTF8.GetString(array2);
        }

        //
        // 摘要:
        //     解密方法byte[] to byte[]
        //
        // 参数:
        //   inputByte:
        //     待解密的byte数组
        //
        // 返回结果:
        //     经过解密的byte数组
        public static byte[] Decrypt(byte[] inputByte)
        {
            List<byte> list = new List<byte>();
            if (inputByte == null || inputByte.Length == 0)
            {
                return list.ToArray();
            }

            using (MemoryStream stream = new MemoryStream(inputByte, 0, inputByte.Length))
            {
                TripleDES mydes = new TripleDESCryptoServiceProvider();
                ICryptoTransform transform = mydes.CreateDecryptor();
                using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
                {
                    int num = -1;
                    do
                    {
                        num = cryptoStream.ReadByte();
                        if (num != -1)
                        {
                            list.Add((byte)num);
                        }
                    }
                    while (num != -1);
                }
            }

            return list.ToArray();
        }

        //
        // 摘要:
        //     将Base64编码的字符串还原为编码前的字符串，自动检测Base64的合法性
        //
        // 参数:
        //   inputBase64Str:
        //     Base64编码的字符串
        //
        // 返回结果:
        //     错误的Base64编码将返回空字符串
        public static string FromBase64(string inputBase64Str)
        {
            string result = string.Empty;
            string text = inputBase64Str;
            if (!string.IsNullOrWhiteSpace(inputBase64Str))
            {
                try
                {
                    switch (text.Length % 4)
                    {
                        case 1:
                            text = $"{inputBase64Str}===";
                            break;
                        case 2:
                            text = $"{inputBase64Str}==";
                            break;
                        case 3:
                            text = $"{inputBase64Str}=";
                            break;
                    }

                    byte[] bytes = Convert.FromBase64String(text);
                    result = Encoding.UTF8.GetString(bytes);
                    bytes = null;
                }
                catch (Exception ex)
                {
                    
                }
            }

            return result;
        }



        public static IList<ParamItem> GetParamItemList(string param)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("param.xml");
            XmlNode xnparam = doc.SelectSingleNode("param");
            XmlNode xn = xnparam.SelectSingleNode(param);
            XmlNodeList child = xn.ChildNodes;

            IList<ParamItem> itemlist = new List<ParamItem>();
            foreach (XmlNode xn1 in child)
            {
                ParamItem item = new ParamItem();
                XmlElement xe = (XmlElement)xn1;
                item.name = xe.GetAttribute("name").ToString();
                item.value = xe.GetAttribute("value").ToString();
                itemlist.Add(item);
            }
            return itemlist;
        }

        public static string GetParamItemName(string param,string value)
        {
            IList<ParamItem> itemlist = GetParamItemList(param);
            string name = "";
            if(itemlist!=null && itemlist.Count > 0)
            {
                ParamItem item = itemlist.FirstOrDefault(i => i.value == value);
                name = item?.name;
            }
            return name;
        }
        public static string GetParamItemValue(string param, string name)
        {
            IList<ParamItem> itemlist = GetParamItemList(param);
            string value = "";
            if (itemlist != null && itemlist.Count > 0)
            {
                ParamItem item = itemlist.FirstOrDefault(i => i.name == name);
                if(item==null)
                    item = itemlist.FirstOrDefault(i => i.name.Contains(name));
                value = item?.value;
            }
            return value;
        }


        public static string GetMd5(string str)
        {
            try
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] bytValue, bytHash;
                bytValue = System.Text.Encoding.UTF8.GetBytes(str);
                bytHash = md5.ComputeHash(bytValue);
                md5.Clear();
                string sTemp = "";
                for (int i = 0; i < bytHash.Length; i++)
                {
                    sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
                }
                str = sTemp.ToLower();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return str;
        }


        public static string ToJSON(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public static string GetImageBase64(string path)
        {
            try
            {
                if(string.IsNullOrEmpty(path))
                    return null;
                Bitmap bmp = new Bitmap(path);
                MemoryStream ms = new MemoryStream();
                var suffix = path.Substring(path.LastIndexOf('.') + 1,path.Length - path.LastIndexOf('.') - 1).ToLower();
                ImageFormat suffixName = null;
                switch (suffix)
                {
                    case "png":
                        suffixName = ImageFormat.Png;
                        break;
                    case "jpg":
                    case "jpeg":
                        suffixName = ImageFormat.Jpeg;
                        break;
                    case "bmp":
                        suffixName = ImageFormat.Bmp;
                        break;
                    case "gif":
                        suffixName = ImageFormat.Gif;
                        break;
                }
                if (suffixName == null)
                    return null;
                bmp.Save(ms, suffixName);
                byte[] arr = new byte[ms.Length]; ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length); ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string ReplaceSQLChar(string str)
        {
            if (string.IsNullOrEmpty(str))
                return String.Empty;
            str = str.Replace("'", "");
            str = str.Replace(";", "");
            str = str.Replace(",", "");
            str = str.Replace("?", "");
            str = str.Replace("<", "");
            str = str.Replace(">", "");
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            str = str.Replace("@", "");
            str = str.Replace("=", "");
            str = str.Replace("+", "");
            str = str.Replace("*", "");
            str = str.Replace("&", "");
            str = str.Replace("#", "");
            str = str.Replace("%", "");
            str = str.Replace("$", "");

            //删除与数据库相关的词
            str = Regex.Replace(str, "select", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "insert", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "delete from", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "count", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "drop table", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "truncate", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "asc", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "mid", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "char", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "xp_cmdshell", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "exec master", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "net localgroup administrators", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "and", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "net user", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "or", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "net", "", RegexOptions.IgnoreCase);
            //str = Regex.Replace(str, "-", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "delete", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "drop", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "script", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "update", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "and", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "chr", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "master", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "truncate", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "declare", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "mid", "", RegexOptions.IgnoreCase);

            return str;
        }

        public static string GetPath(string path)
        {
            string ss = Directory.GetCurrentDirectory() + $"\\{path}";
            if (!Directory.Exists(ss))
                Directory.CreateDirectory(ss);
            return ss;

        }
    }

    public class ParamItem
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    
    
}
