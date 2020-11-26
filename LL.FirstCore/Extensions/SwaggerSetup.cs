using LL.FirstCore.SwaggerFilter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LL.FirstCore.Extensions
{
    /// <summary>
    /// swagger服务注册
    /// </summary>
    public static class SwaggerSetup
    {

        public static void AddSwaggerService(this IServiceCollection services, IApiVersionDescriptionProvider provider)
        {
            if (services == null)
                throw new ArgumentException(nameof(services));

            services.AddSwaggerGen(option =>
            {
                foreach (var descriptiopn in provider.ApiVersionDescriptions)
                {
                    option.SwaggerDoc(descriptiopn.GroupName, new OpenApiInfo()
                    {
                        Version = descriptiopn.ApiVersion.ToString(),
                        Title = $"LL.FirstCore 接口文档--{RuntimeInformation.FrameworkDescription}",    //版本控制信息
                        Description = "LL.FirstCore Api",
                        Contact = new OpenApiContact { Name = "LL.FirstCore", Email = "1137020867@qq.com", Url = new Uri("https://github.com/linzhimin555/LL.First.Project") },
                        License = new OpenApiLicense
                        {
                            Name = $"LL.FirstCore 官方文档",
                            Url = new Uri("https://github.com/linzhimin555/LL.First.Project/blob/master/README.md")
                        }
                    });
                    //Action排序
                    option.OrderActionsBy(v => v.HttpMethod);
                }

                option.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var versions = apiDesc.CustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v.ToString()}" == docName);
                });

                option.OperationFilter<RemoveVersionParameterOperationFilter>();
                option.DocumentFilter<SetVersionInPathDocumentFilter>();
                // 开启加权小锁
                option.OperationFilter<AddResponseHeadersFilter>();
                option.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                // 在header中添加token，传递到后台
                option.OperationFilter<SecurityRequirementsOperationFilter>();

                //以下两种写法等效
                var temPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Startup).Assembly.GetName().Name}.xml");
                //var temPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");                
                option.IncludeXmlComments(temPath, true);
                //加入控制器注释描述信息
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "LL.FirstCore.xml");//这个就是刚刚配置的xml文件名
                option.IncludeXmlComments(xmlPath, true);//默认的第二个参数是false，这个是controller的注释，记得修改

                // Jwt Bearer 认证，必须是 oauth2
                option.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey
                });
            });
        }
    }
}
