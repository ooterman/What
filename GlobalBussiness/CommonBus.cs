using GlobalCommon;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GlobalBussiness
{
    [AppService(ServiceLifetime.Transient)]
    public class CommonBus
    {
        private ILog logger = LogManager.GetLogger(CommonHelper.repository.Name, typeof(CommonBus));
        public IConfigurationRoot GetConfiguration()
        {
            var config = new ConfigurationBuilder()
             .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
             .Build();
            return config;
        }


        public string CreateWord(IEnumerable<string> imgPathList)
        {
            try
            {
                string wordPath = string.Format("{0}\\{1}.{2}", CommonHelper.GetPath("temporary"), Guid.NewGuid().ToString(), "docx");

                XWPFDocument doc = new XWPFDocument();
                XWPFParagraph p2 = doc.CreateParagraph();
                p2.Alignment = ParagraphAlignment.CENTER;//水平居中
                XWPFRun r2 = p2.CreateRun();

                var widthEmus = (int)(400.0 * 9525);//图片的宽度
                var heightEmus = (int)(300.0 * 9525);//图片的高度

                foreach (string imgPath in imgPathList)
                {
                    using (FileStream picData = new FileStream(imgPath, FileMode.Open, FileAccess.Read))
                    {
                        //图片的文件流   图片类型   图片名称   设置的宽度以及高度
                        r2.AddPicture(picData, (int)PictureType.PNG, "", widthEmus, heightEmus);
                        r2.AddCarriageReturn(); r2.AddCarriageReturn();
                    }
                }
                using (FileStream fs = new FileStream(wordPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    doc.Write(fs);
                    doc.Close();
                }

                //string pdfFilePath = string.Format("{0}\\{1}.{2}", CommonHelper.GetPath("temporary"), Guid.NewGuid().ToString(), "pdf");
                //Aspose.Words.Document adoc = new Aspose.Words.Document(wordPath);
                //adoc.Save(pdfFilePath, Aspose.Words.SaveFormat.Pdf);
                //string pngFilePath = string.Format("{0}\\{1}.{2}", CommonHelper.GetPath("temporary"), Guid.NewGuid().ToString(), "png");
                //adoc.Save(pngFilePath, Aspose.Words.SaveFormat.Png);

                return wordPath;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return "";
            }
        }


        public string sendPost(string url, IDictionary<string, string> parameters, string method)
        {
            if (method.ToLower() == "post")
            {
                HttpWebRequest req = null;
                HttpWebResponse rsp = null;
                System.IO.Stream reqStream = null;
                try
                {
                    req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = method;
                    req.KeepAlive = false;
                    req.ProtocolVersion = HttpVersion.Version10;
                    req.Timeout = 5000;
                    req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    byte[] postData = Encoding.UTF8.GetBytes(BuildQuery(parameters, "utf8"));
                    reqStream = req.GetRequestStream();
                    reqStream.Write(postData, 0, postData.Length);
                    rsp = (HttpWebResponse)req.GetResponse();
                    Encoding encoding = Encoding.GetEncoding(rsp.CharacterSet);
                    return GetResponseAsString(rsp, encoding);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                finally
                {
                    if (reqStream != null) reqStream.Close();
                    if (rsp != null) rsp.Close();
                }
            }
            else
            {
                //创建请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + "?" + BuildQuery(parameters, "utf8"));

                //GET请求
                request.Method = "GET";
                request.ReadWriteTimeout = 5000;
                request.ContentType = "text/html;charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

                //返回内容
                string retString = myStreamReader.ReadToEnd();
                return retString;
            }
        }

        public string BuildQuery(IDictionary<string, string> parameters, string encode)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;
            IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name))//&& !string.IsNullOrEmpty(value)
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }
                    postData.Append(name);
                    postData.Append("=");
                    if (encode == "gb2312")
                    {
                        postData.Append(HttpUtility.UrlEncode(value, Encoding.GetEncoding("gb2312")));
                    }
                    else if (encode == "utf8")
                    {
                        postData.Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
                    }
                    else
                    {
                        postData.Append(value);
                    }
                    hasParam = true;
                }
            }
            return postData.ToString();
        }

        /// <summary>
        /// 把响应流转换为文本。
        /// </summary>
        /// <param name="rsp">响应流对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>响应文本</returns>
        public string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            Stream stream = null;
            StreamReader reader = null;
            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
        }


    }
}
