﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GlobalCommon
{
    [AppService(ServiceLifetime.Singleton)]
    public class AutowiredService
    {
        IServiceProvider serviceProvider;

        public AutowiredService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private ConcurrentDictionary<Type, Action<object, IServiceProvider>> autowiredActions =
                    new ConcurrentDictionary<Type, Action<object, IServiceProvider>>();

        public void Autowired(object service)
        {
            Autowired(service, serviceProvider);
        }

        public void Autowired(object service, IServiceProvider serviceProvider)
        {
            var serviceType = service.GetType();
            if (autowiredActions.TryGetValue(serviceType, out Action<object, IServiceProvider> act))
            {
                act(service, serviceProvider);
            }
            else
            {
          
                //参数
                var objParam = Expression.Parameter(typeof(object), "obj");
                var spParam = Expression.Parameter(typeof(IServiceProvider), "sp");

                var obj = Expression.Convert(objParam, serviceType);
                var GetService = typeof(AutowiredService).GetMethod("GetService", BindingFlags.Static | BindingFlags.NonPublic);
                List<Expression> setList = new List<Expression>();

                //字段赋值
                foreach (FieldInfo field in serviceType.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var autowiredAttr = field.GetCustomAttribute<AutowiredAttribute>();
                    if (autowiredAttr != null)
                    {
                        var fieldExp = Expression.Field(obj, field);
                        var createService = Expression.Call(null, GetService, spParam,
                            Expression.Constant(field.FieldType), Expression.Constant(autowiredAttr));

                        var setExp = Expression.Assign(fieldExp, Expression.Convert(createService, field.FieldType));
                        setList.Add(setExp);
                    }
                }

                //属性赋值
                foreach (PropertyInfo property in serviceType.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var autowiredAttr = property.GetCustomAttribute<AutowiredAttribute>();
                    if (autowiredAttr != null)
                    {
                        var propExp = Expression.Property(obj, property);
                        var createService = Expression.Call(null, GetService, spParam,
                            Expression.Constant(property.PropertyType), Expression.Constant(autowiredAttr));

                        var setExp = Expression.Assign(propExp,
                            Expression.Convert(createService, property.PropertyType));
                        setList.Add(setExp);
                    }
                }

                var bodyExp = Expression.Block(setList);
                var setAction = Expression.Lambda<Action<object, IServiceProvider>>(bodyExp, objParam, spParam)
                    .Compile();
                autowiredActions[serviceType] = setAction;
                setAction(service, serviceProvider);
            }
        }

        /// <summary>
        /// 根据不同的Identifier获取不同的服务实现
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="serviceType"></param>
        /// <param name="autowiredAttribute"></param>
        /// <returns></returns>
        private static object GetService(IServiceProvider serviceProvider, Type serviceType,
            AutowiredAttribute autowiredAttribute)
        {
            var list = serviceProvider.GetServices(serviceType).ToList();
            if (list.Count == 0)
            {
                return null;
            }
            else if (list.Count == 1 && string.IsNullOrEmpty(autowiredAttribute.Identifier))
            {
                return list[0];
            }
            else
            {
                if (string.IsNullOrEmpty(autowiredAttribute.Identifier))
                {
                    return list.Last();
                }
               
            }

            return null;
        }

    }
}
