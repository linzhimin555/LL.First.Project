using LL.FirstCore.SwaggerFilter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                        Title = $"LL.FirstCore 接口文档--NetCore 3.1",
                        Description = "LL.FirstCore Api",
                        Contact = new OpenApiContact { Name = "LL.FirstCore", Email = "1137020867@qq.com", Url = new Uri("https://github.com/linzhimin555/LL.First.Project") },
                        License = new OpenApiLicense
                        {
                            Name = $"LL.FirstCore 官方文档",
                            Url = new Uri("https://github.com/linzhimin555/LL.First.Project/blob/master/README.md")
                        }
                    });
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

                //以下两种写法等效
                var temPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Startup).Assembly.GetName().Name}.xml");
                //var temPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");                
                option.IncludeXmlComments(temPath, true);
                //加入控制器注释描述信息
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "LL.FirstCore.xml");//这个就是刚刚配置的xml文件名
                option.IncludeXmlComments(xmlPath, true);//默认的第二个参数是false，这个是controller的注释，记得修改

                //option.AddSecurityDefinition("JwtBearer", new OpenApiSecurityScheme
                //{
                //    Description = "JWT授权Bearer {token}",
                //    Name = "Authorization",//jwt默认的参数名称
                //    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                //    Type = SecuritySchemeType.ApiKey
                //});
            });
        }
    }
}
