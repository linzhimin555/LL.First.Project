using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LL.FirstCore.Extensions
{
    /// <summary>
    /// AutoFac 模块注入
    /// </summary>
    public class AutofacModuleRegister : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //获取程序根目录
            var basePath = AppContext.BaseDirectory;
            var repository = Assembly.Load("LL.FirstCore.Repository");
            var service = Assembly.LoadFrom(Path.Combine(basePath, "LL.FirstCore.Services.dll"));
            builder.RegisterAssemblyTypes(service).AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(repository).AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
