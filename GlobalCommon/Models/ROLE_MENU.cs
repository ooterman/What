using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace GlobalCommon.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("ROLE_MENU")]
    public partial class ROLE_MENU
    {
           public ROLE_MENU(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal role_id {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal menu_id {get;set;}

    }
}
