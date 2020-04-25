using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LL.FirstCore.SwaggerFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LL.FirstCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// Api版本信息
        /// </summary>
        private IApiVersionDescriptionProvider provider;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            #region using Api version(eg:参考链接:https://www.cnblogs.com/jjg0519/p/7253594.html,https://www.quarkbook.com/?p=793)
            //动态整合版:https://www.cnblogs.com/jixiaosa/p/10817143.html

            services.AddApiVersioning(option =>
            {
                // 可选，为true时API返回支持的版本信息
                option.ReportApiVersions = true;
                // 不提供版本时，默认为1.0
                option.AssumeDefaultVersionWhenUnspecified = true;
                // 请求中未指定版本时默认为1.0
                option.DefaultApiVersion = new ApiVersion(1, 0);
            }).AddVersionedApiExplorer(option =>
            {
                // 版本名的格式：v+版本号
                option.GroupNameFormat = "'v'V";
                option.AssumeDefaultVersionWhenUnspecified = true;
            });
            #endregion

            #region using Swagger
            provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
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

                var temPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Startup).Assembly.GetName().Name}.xml");
                option.IncludeXmlComments(temPath, true);
                //就是这里
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "LL.FirstCore.xml");//这个就是刚刚配置的xml文件名
                option.IncludeXmlComments(xmlPath, true);//默认的第二个参数是false，这个是controller的注释，记得修改
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region swagger
            app.UseSwagger();
            app.UseSwaggerUI(option =>
            {
                foreach (var item in provider.ApiVersionDescriptions)
                {
                    option.SwaggerEndpoint($"/swagger/{item.GroupName}/swagger.json", "LL.First.Core V" + item.ApiVersion);
                }
                option.RoutePrefix = string.Empty;
            });
            #endregion

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
