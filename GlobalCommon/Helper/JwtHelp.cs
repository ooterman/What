using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace GlobalCommon
{
    public class JwtHelp
    {

        //私钥  web.config中配置
        //"GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
        private static IConfigurationRoot Configuration = CommonHelper.GetConfiguration();
        private static string Enkey ="jdh@sd*we$";
        /// <summary>
        /// 生成JwtToken
        /// </summary>
        /// <param name="payload">不敏感的用户数据</param>
        /// <returns></returns>
        public static string SetJwtEncode(Dictionary<string, object> payload)
        {

            //格式如下
            //var payload = new Dictionary<string, object>
            //{
            //    { "username","admin" },
            //    { "pwd", "claim2-value" }
            //};

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, Enkey);
            return token;
        }

        /// <summary>
        /// 根据jwtToken  获取实体
        /// </summary>
        /// <param name="token">jwtToken</param>
        /// <returns></returns>
        public static UserInfo GetJwtDecode(string token)
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);
            var userInfo = decoder.DecodeToObject<UserInfo>(token, Enkey, verify: true);//token为之前生成的字符串
            return userInfo;
        }
    }
    public class UserInfo
    {
        public string user_id { get; set; }
        public string username { get; set; }

        public string create_time { get; set; }
        public string Sign { get; set; }
        /// <summary>
        /// the key
        /// </summary>
        const string encryptKey = "c1a2t3c4h5e6r.";
        /// <summary>
        /// validate the parameters
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            return this.Sign == CommonHelper.GetEncryptResult((username), encryptKey);
        }
    }

}
