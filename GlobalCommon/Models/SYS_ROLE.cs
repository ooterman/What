using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace GlobalCommon.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("SYS_ROLE")]
    public partial class SYS_ROLE
    {
           public SYS_ROLE(){


           }
           /// <summary>
           /// Desc:角色标识
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public decimal role_id {get;set;}

           /// <summary>
           /// Desc:角色名称
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string role_name {get;set;}

           /// <summary>
           /// Desc:父级角色
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal parent_roleid {get;set;}

           /// <summary>
           /// Desc:备注
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string remark {get;set;}

           /// <summary>
           /// Desc:状态0:禁用，1：启用
           /// Default:
           /// Nullable:False
           /// </summary>           
           public short is_enable {get;set;}

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
           /// Desc:更新人
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string update_user {get;set;}

           /// <summary>
           /// Desc:更新时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? update_time {get;set;}

           /// <summary>
           /// Desc:账号类型1:业务员，2：平台管理员，3：承办机关管理员
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? account_type {get;set;}

    }
}
