using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using SqlSugar;

namespace GlobalCommon
{
    public class SqlHelper
    {

        public static SqlSugarScope scope = GetConnect();


        public SqlHelper()
        {

        }

        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <returns></returns>
        private static SqlSugarScope GetConnect()
        {
            if (scope == null)
            {
                var Configuration = new ConfigurationBuilder()
              .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
              .Build();

                string ConnectionString = Configuration["ConnectionStrings:OraCon"];
                scope = new SqlSugarScope(new ConnectionConfig()
                {
                    ConnectionString = ConnectionString,//连接符字串
                    DbType = DbType.Oracle,//数据库类型
                    IsAutoCloseConnection = true //不设成true要手动close
                },
                db =>
                {
                    //（A）这里配置参数：所有上下文都生效 
                    // db.Aop.xxx=xxxx..
                });
            }
          
            return scope;
        }
    }
}
