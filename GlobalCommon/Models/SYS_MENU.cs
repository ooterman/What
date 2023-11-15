using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace GlobalCommon.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("SYS_MENU")]
    public partial class SYS_MENU
    {
           public SYS_MENU(){


           }
           /// <summary>
           /// Desc:菜单标识
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public decimal menu_id {get;set;}

           /// <summary>
           /// Desc:菜单名称
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string menu_name {get;set;}

           /// <summary>
           /// Desc:父级标识
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal parent_menuid {get;set;}

           /// <summary>
           /// Desc:图标
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string icon {get;set;}

           /// <summary>
           /// Desc:地址
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string menu_url {get;set;}

           /// <summary>
           /// Desc:排序
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? show_order {get;set;}

           /// <summary>
           /// Desc:状态0：禁用，1：可用
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? is_enable {get;set;}

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

           public string remark { get; set; }

    }
}
