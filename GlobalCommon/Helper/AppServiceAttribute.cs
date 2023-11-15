using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalCommon
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AppServiceAttribute : Attribute
    {
        /// <summary>
        /// 生命周期
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }


        public AppServiceAttribute(ServiceLifetime serviceLifetime)
        {
            Lifetime = serviceLifetime;
        }
    }
}
