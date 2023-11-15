using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace GlobalCommon.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("SYS_FILE")]
    public partial class SYS_FILE
    {
           public SYS_FILE(){


           }
           /// <summary>
           /// Desc:文件标识
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true, OracleSequenceName = "SEQUENCE_FILE_ID")]
            public decimal file_id {get;set;}

           /// <summary>
           /// Desc:文件名称
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string file_name {get;set;}

           /// <summary>
           /// Desc:业务标识
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string bussiness_id {get;set;}

           /// <summary>
           /// Desc:业务类型1：结婚登记资料
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int bussiness_type {get;set;}

           /// <summary>
           /// Desc:文件类型1：身份证正面，2：身份证反面
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? file_type {get;set;}

           /// <summary>
           /// Desc:文件路径
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string file_url {get;set;}

           /// <summary>
           /// Desc:创建时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? create_time {get;set;}

    }
}
