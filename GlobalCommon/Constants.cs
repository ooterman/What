using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalCommon
{
    public class Constants
    {

        public struct SysParamType
        {
            /// <summary>
            /// 用户类型。
            /// </summary>
            public static readonly string AccountType = "account_type";
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        public enum IsEnable
        {
            禁用 = 0,
            启用 = 1
        }

        /// <summary>
        /// 审核状态
        /// </summary>
        public enum audit_state
        {
            未审核 = 0,
            审核通过 = 1,
            已驳回 = 2
        }

        public enum upload_state
        {
            未上传 = 0,
            上传成功 = 1,
            上传失败 = 2
        }

        /// <summary>
        /// 性别
        /// </summary>
        public enum sex
        {
            男 = 0,
            女 = 1
        }

        /// <summary>
        /// 
        /// </summary>
        public enum BussinessType
        {
            用户信息 = 99
        }

        /// <summary>
        /// 
        /// </summary>
        public enum FileType
        {
            签名 = 99
        }

    }
}
