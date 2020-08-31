# LL.First.Core

.Net Core 官方文档：https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/startup?view=aspnetcore-2.2

## Swagger+ApiVersion进行版本控制

> 参考链接:
>
> 1. [版本控制][https://www.talkingdotnet.com/support-multiple-versions-of-asp-net-core-web-api/]
> 2. [Swagger+版本控制的使用][https://www.cnblogs.com/jixiaosa/p/10817143.html]
> 3. [swagger官方文档][https://docs.microsoft.com/zh-cn/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-3.1&tabs=visual-studio]

> <font  color='red'> 注意点:版本控制中apiversion=2.0与apiversion=2会自动划分为同一个版本号</font>

## T4模板根据数据表生成框架基本代码

> MySqlHelper.ttinclude主要用于対生成的命名空间以及数据库连接信息进行配置处理

## IConfiguration

> 官方介绍：https://docs.microsoft.com/zh-cn/dotnet/api/microsoft.extensions.configuration.iconfiguration?view=dotnet-plat-ext-3.1
>
> 参考博文：https://www.cnblogs.com/nimorl/p/12570823.html，https://www.cnblogs.com/smartsmile/p/11721544.html

## HealthCheck健康检查

> 官方介绍：https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/tree/netcore-3.0
> 
> <font  color='red'> 注意点:HealthyUI页面报表结果需要引用HealthChecks.UI.Client包</font>