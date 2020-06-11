using LL.FirstCore.Common.Config;
using LL.FirstCore.Common.Jwt;
using LL.FirstCore.Extensions;
using LL.FirstCore.Repository.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

            services.Configure<JwtSetting>(Configuration.GetSection("JwtSetting"));

            #region 官方JWT认证方式
            //开启bearer认证，注入jwtbearer认证服务
            /*AddAuthentication("Bearer")==AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })*/
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    //token认证对象
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //3+2模式 (发行人+订阅人+秘钥)+(生命周期+过期时间)

                        ValidateIssuer = true,      //是否验证Issuer
                        ValidIssuer = Configuration["JwtSetting:Issure"],   //验证与配置文件中设置的是否一致
                        ValidateAudience = true,    //是否验证Audience
                        ValidAudience = Configuration["JwtSetting:Audience"],   //验证与配置文件中设置的是否一致
                        ValidateIssuerSigningKey = true,    //是否验证SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSetting:Secret"])),

                        ValidateLifetime = true,    //是否验证失效时间,使用当前时间与token中的Claim中声明的Notbefore和expires对比
                        RequireExpirationTime = true,    //是否要求token的claim中必须包含Expires
                        //允许的服务器时间偏移量  
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                    //Jwt事件(JwtBearer认证中，默认是通过Http的Authorization头来获取的，这也是最推荐的做法，但是在某些场景下，我们可能会使用Url或者是Cookie来传递Token)
                    options.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Query["access_token"];
                            return Task.CompletedTask;
                        }
                    };
                });
            #endregion

            #region 配置跨域请求
            services.AddCors(options =>
            {
                var origins = Configuration.GetSection("AllowOrigins").Get<string[]>();
                options.AddDefaultPolicy(builder => builder.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
            });
            #endregion

            #region using Swagger
            provider = BuildServiceProvider(services).GetRequiredService<IApiVersionDescriptionProvider>();
            services.AddSwaggerService(provider);
            #endregion

            #region 添加EF Core服务
            services.AddDbContext<BaseDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            #endregion

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IJwtProvider, JwtProvider>();

            //测试读取配置信息帮助类
            var str = ConfigHelper.GetDefaultJsonValue("AllowedHosts"); //默认配置文件
            var otherStr = ConfigHelper.GetAppSetting("test.json", "AllowedHosts");   //其他json文件信息
            //下面写法将配置信息以对象形式来表达，并以单例方式注册到服务容器中
            services.AddOptions().Configure<string>(Configuration.GetSection("AllowedHosts"));
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
            app.UseCors();
            //app.UseHttpsRedirection();

            app.UseRouting();
            // 先开启认证
            app.UseAuthentication();
            // 然后是授权中间件
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private ServiceProvider BuildServiceProvider(IServiceCollection services)
        {
            return services.BuildServiceProvider();
        }
    }
}
