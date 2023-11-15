using GlobalCommon;
using log4net;
using MarryRegistCommon.Models;
using Microsoft.Extensions.DependencyInjection;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace MarryRegistBussiness
{
    [AppService(ServiceLifetime.Transient)]
    public class RegistBus
    {
        private ILog logger = LogManager.GetLogger(CommonHelper.repository.Name, typeof(RegistBus));

        public List<DEVICE_INFO> GetDevListByOrganId(decimal organ_id)
        {
            List<DEVICE_INFO> devlist = SqlHelper.scope.Queryable<DEVICE_INFO>().Where(d => d.organ_id == organ_id).ToList();
            return devlist;
        }


        public string saveAioFile(MemoryStream stream, string file_name)
        {

            var requestUri = CommonHelper.GetConfiguration()["UploadFileUrl"];

            var postContent = new MultipartFormDataContent();
            var fileSteamConten = new StreamContent(stream);
            fileSteamConten.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            postContent.Add(fileSteamConten, "file", file_name);
            postContent.Add(new StringContent("1"), "marryId");
            postContent.Add(new StringContent("dss"), "name");
            postContent.Add(new StringContent("1"), "state");
            postContent.Add(new StringContent("0"), "type");

            HttpClient _httpclient = new HttpClient();
            var response = _httpclient.PostAsync(requestUri, postContent);
            if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string str = response.Result.Content.ReadAsStringAsync().Result;
                return str;
            }
            else
            {
                string str = response.Result.Content.ReadAsStringAsync().Result;
                return str;
            }

        }

        public string DownloadState(int personId)
        {
            REGIST_PERSON MyPerson = SqlHelper.scope.Queryable<REGIST_PERSON>().Where(r => r.regist_personid == personId).First();
            REGIST_INFO regist = SqlHelper.scope.Queryable<REGIST_INFO>().Where(r => r.accept_code == MyPerson.accept_code).First();
            int sex = 0;
            if (MyPerson.sex == 0)
                sex = 1;
            REGIST_PERSON OtherPerson = SqlHelper.scope.Queryable<REGIST_PERSON>().Where(r => r.accept_code == MyPerson.accept_code && r.sex == sex).First();

            string wordPath = Directory.GetCurrentDirectory()+"\\Template\\申请结婚登记声明书.docx";
            string temPath = string.Format("{0}\\{1}.{2}", CommonHelper.GetPath("temporary"), Guid.NewGuid().ToString(), "docx");
            using (FileStream stream = new FileStream(wordPath, FileMode.Open, FileAccess.Read))
            {
                XWPFDocument doc = new XWPFDocument(stream);
                
                Dictionary<string, string> DicWord = new Dictionary<string, string>();
                DicWord.Add("$MyName$", MyPerson.name);
                DicWord.Add("$MySex$", Enum.GetName(typeof(Constants.sex),MyPerson.sex));
                DicWord.Add("$MyNationality$", MyPerson.nationality);
                DicWord.Add("$MyBirthday$", string.Format("{0:yyyy年MM月dd日}",MyPerson.birthday));
                DicWord.Add("$MyNation$", MyPerson.nation);
                DicWord.Add("$MyOccupation$", CommonHelper.GetParamItemName(MarryRegistCommon.Constants.SysParamType.occupation,MyPerson.occupation));
                DicWord.Add("$MyDegree$", CommonHelper.GetParamItemName(MarryRegistCommon.Constants.SysParamType.DegreeType, MyPerson.education_degree));
                DicWord.Add("$MyIdcard$", MyPerson.idcard_number);
                DicWord.Add("$MyHomeAdress$", MyPerson.home_adress);
                DicWord.Add("$MyMarryState$", CommonHelper.GetParamItemName(MarryRegistCommon.Constants.SysParamType.MarryState, MyPerson.marry_state));
                DicWord.Add("$MyHasill$", MyPerson.has_ill);
                DicWord.Add("$MyApplyDate$", string.Format("{0:yyyy年MM月dd日}", regist.apply_time));
                DicWord.Add("$OtherName$", OtherPerson.name);
                DicWord.Add("$OtherSex$", Enum.GetName(typeof(Constants.sex), OtherPerson.sex));
                DicWord.Add("$OtherNationality$", OtherPerson.nationality);
                DicWord.Add("$OtherBirthday$", string.Format("{0:yyyy年MM月dd日}", OtherPerson.birthday));
                DicWord.Add("$OtherNation$", OtherPerson.nation);
                DicWord.Add("$OtherOccupation$", CommonHelper.GetParamItemName(MarryRegistCommon.Constants.SysParamType.occupation, OtherPerson.occupation));
                DicWord.Add("$OtherDegree$", CommonHelper.GetParamItemName(MarryRegistCommon.Constants.SysParamType.DegreeType, OtherPerson.education_degree));
                DicWord.Add("$OtherIdcard$", OtherPerson.idcard_number);
                DicWord.Add("$OtherHomeAdress$", OtherPerson.home_adress);
                DicWord.Add("$OtherMarryState$", CommonHelper.GetParamItemName(MarryRegistCommon.Constants.SysParamType.MarryState, OtherPerson.marry_state));
                DicWord.Add("$OtherHasill$", OtherPerson.has_ill);
                DicWord.Add("$OtherApplyDate$", string.Format("{0:yyyy年MM月dd日}", regist.apply_time));

                //遍历段落                  
                foreach (var para in doc.Paragraphs)
                {
                    string oldText = para.ParagraphText;
                    if (oldText != "" && oldText != string.Empty && oldText != null)
                    {
                        string tempText = para.ParagraphText;

                        foreach (KeyValuePair<string, string> kvp in DicWord)
                        {
                            if (tempText.Contains(kvp.Key))
                            {
                                para.ReplaceText(kvp.Key, kvp.Value);
                            }
                        }

                    }
                }
               
                using (FileStream fs = new FileStream(temPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    doc.Write(fs);
                    doc.Close();
                }


            }

            return temPath;
        }




    }
}
