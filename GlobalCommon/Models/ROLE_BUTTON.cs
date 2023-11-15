using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace GlobalCommon.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("ROLE_BUTTON")]
    public partial class ROLE_BUTTON
    {
           public ROLE_BUTTON(){


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
           public decimal button_id {get;set;}

    }
}
