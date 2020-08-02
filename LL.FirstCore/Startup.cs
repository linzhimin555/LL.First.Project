using Autofac;
using LL.FirstCore.Common.Config;
using LL.FirstCore.Common.Jwt;
using LL.FirstCore.Common.Logger;
using LL.FirstCore.Extensions;
using LL.FirstCore.Middleware;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Profiling.Storage;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Unicode;
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
            //services.AddCors(options =>
            //{
            //    var origins = Configuration.GetSection("AllowOrigins").Get<string[]>();
            //    options.AddDefaultPolicy(builder => builder.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
            //});
            #endregion

            #region 添加miniProfile(https://miniprofiler.com/dotnet/AspDotNetCore)
            services.AddMemoryCache();
            services.AddMiniProfiler(options =>
            {
                // 设定访问分析结果URL的路由基地址
                options.RouteBasePath = "/profiler";
                //（可选）控制存储
                //（在MemoryCacheStorage中默认为30分钟）
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(30);
                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                // 设定弹出窗口的位置是左下角
                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.Left;
                // 设定在弹出的明细窗口里会显示Time With Children这列
                options.PopupShowTimeWithChildren = true;
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
            }).AddEntityFramework();
            #endregion

            #region using Swagger
            provider = BuildServiceProvider(services).GetRequiredService<IApiVersionDescriptionProvider>();
            services.AddSwaggerService(provider);
            #endregion

            #region 添加EF Core服务
            services.AddDbContext<BaseDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), builder => builder.EnableRetryOnFailure());
            });
            services.AddScoped<BaseDbContext>();
            #endregion

            #region 添加HttpClient请求
            services.AddHttpClient();
            #endregion

            //注入类似于HttpContext的上下文
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IJwtProvider, JwtProvider>();

            //测试读取配置信息帮助类
            var str = ConfigHelper.GetDefaultJsonValue("AllowedHosts"); //默认配置文件
            var otherStr = ConfigHelper.GetAppSetting("test.json", "AllowedHosts");   //其他json文件信息
            //下面写法将配置信息以对象形式来表达，并以单例方式注册到服务容器中
            services.AddOptions().Configure<string>(Configuration.GetSection("AllowedHosts"));
            services.AddSingleton<ILogFormat, ContentFormat>();

            #region 设置编码格式
            //services.Configure<WebEncoderOptions>(options =>
            //{
            //    options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(UnicodeRanges.All);
            //});
            #endregion
        }

        // 注意在Program.CreateHostBuilder，添加Autofac服务工厂(3.0语法)
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModuleRegister());
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
                // 将swagger首页，设置成我们自定义的页面，记得这个字符串的写法,记得这个字符串的写法,记得这个字符串的写法：程序集名.index.html
                option.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("LL.FirstCore.index.html");
            });
            #endregion

            //请求日志中间件
            app.UseRequestLog();

            // ↓↓↓↓↓↓ 注意下边这些中间件的顺序，很重要 ↓↓↓↓↓↓
            // CORS跨域
            //app.UseCors();
            // 跳转https
            //app.UseHttpsRedirection();
            // 使用静态文件
            app.UseStaticFiles();
            //
            app.UseRouting();
            // 先开启认证
            app.UseAuthentication();
            // 然后是授权中间件
            app.UseAuthorization();
            // 性能分析
            app.UseMiniProfiler();
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
