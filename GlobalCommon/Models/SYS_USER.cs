using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace GlobalCommon.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("SYS_USER")]
    public partial class SYS_USER
    {
           public SYS_USER(){


           }
           /// <summary>
           /// Desc:用户标识
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true, OracleSequenceName = "SEQUENCE_USERID")]
           public decimal user_id {get;set;}

           /// <summary>
           /// Desc:姓名
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string user_name {get;set;}

           /// <summary>
           /// Desc:账号
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string user_account {get;set;}

           /// <summary>
           /// Desc:密码
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string password {get;set;}

           /// <summary>
           /// Desc:手机号码
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string mobile {get;set;}

           /// <summary>
           /// Desc:状态0:禁用，1：启用
           /// Default:
           /// Nullable:True
           /// </summary>           
           public short? is_enable {get;set;}

           /// <summary>
           /// Desc:账号类型1:业务员，2：平台管理员，3：承办机关管理员
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? account_type {get;set;}

           /// <summary>
           /// Desc:创建人
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string create_user {get;set;}

           /// <summary>
           /// Desc:创建时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? create_time {get;set;}

            /// <summary>
           /// Desc:是否超级管理员
           /// Default:
           /// Nullable:True
           /// </summary>           
           public short? is_administrator { get;set;}
           
           /// <summary>
           /// Desc:登录时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? login_time { get;set;}

        /// <summary>
        /// Desc:访问ip
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string access_ip { get; set; }

        /// <summary>
        /// 承办机关标识
        /// </summary>
        public decimal organ_id { get; set; }

        /// <summary>
        /// 承办机关名称
        /// </summary>
        public string organ_name { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public decimal signfile_id { get; set; }

    }
}
