using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace GlobalCommon.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("MENU_BUTTON")]
    public partial class MENU_BUTTON
    {
           public MENU_BUTTON(){


           }
           /// <summary>
           /// Desc:按钮标识
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public decimal button_id {get;set;}

           /// <summary>
           /// Desc:菜单标识
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? menu_id {get;set;}

           /// <summary>
           /// Desc:按钮名称
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string button_name {get;set;}

           /// <summary>
           /// Desc:权限点
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string authority {get;set;}

    }
}
