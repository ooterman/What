using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace GlobalCommon
{
    [Serializable]
    public sealed class CommResponse<T>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public CommResponse()
        {
            this.Code = 1;
            this.Message = "";
            this.Data = default( T );
        }

        /// <summary>
        /// 状态代码
        /// </summary>
        [XmlElement( "code" ), JsonProperty( "code" )]
        public UInt16 Code { get; set; }

        /// <summary>
        /// 附加消息
        /// </summary>
        [XmlElement( "message" ), JsonProperty( "message" )]
        public String Message { get; set; }

        /// <summary>
        /// 返回结果
        /// </summary>
        [XmlElement( "data" ), JsonProperty( "data" )]
        public T Data { get; set; }
        public int TotalNum { get; set; }
        public T UserTable { get; set; }
        public string Token { get; set; }
        public bool Success { get; set; }

        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            this.Message = "";
            this.Code = 1;
        }
    }

    public sealed class CommResponseMessage<T> {

        /// <summary>
        /// 构造函数
        /// </summary>
        public CommResponseMessage()
        {
            this.Code = 1;
            this.Message = "";
            this.Data = default(T);
        }

        /// <summary>
        /// 状态代码
        /// </summary>
        [XmlElement("code"), JsonProperty("code")]
        public UInt16 Code { get; set; }

        /// <summary>
        /// 附加消息
        /// </summary>
        [XmlElement("message"), JsonProperty("message")]
        public String Message { get; set; }
        /// <summary>
        /// 返回结果
        /// </summary>
        [XmlElement("data"), JsonProperty("data")]
        public T Data { get; set; }


        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            this.Message = "";
            this.Code = 1;
        }
    }
}
