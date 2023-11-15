using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace GlobalCommon.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("USER_DEVICE")]
    public partial class USER_DEVICE
    {
        public USER_DEVICE()
        {


        }
        /// <summary>
        /// Desc:用户标识
        /// Default:
        /// Nullable:False
        /// </summary>           
        public decimal user_id { get; set; }

        /// <summary>
        /// Desc:设备标识
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string device_number { get; set; }


    }
}
