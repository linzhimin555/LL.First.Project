using Autofac;
using AutoMapper;
using HealthChecks.UI.Client;
using LL.FirstCore.Common.Config;
using LL.FirstCore.Common.Jwt;
using LL.FirstCore.Common.Logger;
using LL.FirstCore.Extensions;
using LL.FirstCore.Filter;
using LL.FirstCore.Filter.GlobalConvention;
using LL.FirstCore.Middleware;
using LL.FirstCore.Repository.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling.Storage;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
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
        /// Api�汾��Ϣ
        /// </summary>
        private IApiVersionDescriptionProvider provider;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(option =>
            {
                option.Filters.Add(typeof(GlobalExceptionFilter));
                //ȫ��·��ǰ׺��Լ
                option.UseCentralRoutePrefix(new RouteAttribute("api"));
            }).AddControllersAsServices();  //�����е�controller��Ϊserviceע��
            //ʹ��System.Text.Json�������ı�������
            //.AddJsonOptions(option =>
            //{
            //    //Ĭ�����������Ļᱻ����
            //    option.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            //});
            #region using Api version(eg:�ο�����:https://www.cnblogs.com/jjg0519/p/7253594.html,https://www.quarkbook.com/?p=793)
            //��̬���ϰ�:https://blog.csdn.net/ma524654165/article/details/77880106

            services.AddApiVersioning(option =>
            {
                // ��ѡ��ΪtrueʱAPI����֧�ֵİ汾��Ϣ
                option.ReportApiVersions = true;
                // ���ṩ�汾ʱ��Ĭ��Ϊ1.0
                option.AssumeDefaultVersionWhenUnspecified = true;
                // ������δָ���汾ʱĬ��Ϊ1.0
                option.DefaultApiVersion = new ApiVersion(1, 0);
            }).AddVersionedApiExplorer(option =>
            {
                // �汾���ĸ�ʽ��v+�汾��
                option.GroupNameFormat = "'v'V";
                option.AssumeDefaultVersionWhenUnspecified = true;
            });
            #endregion

            services.Configure<JwtSetting>(Configuration.GetSection("JwtSetting"));

            #region �ٷ�JWT��֤��ʽ
            //����bearer��֤��ע��jwtbearer��֤����
            /*AddAuthentication("Bearer")==AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })*/
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    //token��֤����
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //3+2ģʽ (������+������+��Կ)+(��������+����ʱ��)

                        ValidateIssuer = true,      //�Ƿ���֤Issuer
                        ValidIssuer = Configuration["JwtSetting:Issure"],   //��֤�������ļ������õ��Ƿ�һ��
                        ValidateAudience = true,    //�Ƿ���֤Audience
                        ValidAudience = Configuration["JwtSetting:Audience"],   //��֤�������ļ������õ��Ƿ�һ��
                        ValidateIssuerSigningKey = true,    //�Ƿ���֤SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSetting:Secret"])),

                        ValidateLifetime = true,    //�Ƿ���֤ʧЧʱ��,ʹ�õ�ǰʱ����token�е�Claim��������Notbefore��expires�Ա�
                        RequireExpirationTime = true,    //�Ƿ�Ҫ��token��claim�б������Expires
                        //����ķ�����ʱ��ƫ����  
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                    //Jwt�¼�(JwtBearer��֤�У�Ĭ����ͨ��Http��Authorizationͷ����ȡ�ģ���Ҳ�����Ƽ���������������ĳЩ�����£����ǿ��ܻ�ʹ��Url������Cookie������Token)
                    options.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context =>
                        {
                            //SignalR����query����token��Ϣ
                            context.Token = context.Request.Query["access_token"];
                            return Task.CompletedTask;
                        },
                        //��֤ʧ��
                        OnAuthenticationFailed = c =>
                        {
                            c.NoResult();
                            c.Response.StatusCode = 500;
                            c.Response.ContentType = "text/plain";
                            c.Response.WriteAsync(c.Exception.ToString()).Wait();
                            return Task.CompletedTask;
                        },
                        //401
                        OnChallenge = context =>
                        {
                            // Skip the default logic.
                            context.HandleResponse();

                            var payload = new JObject
                            {
                                ["error"] = context.Error,
                                ["error_description"] = context.ErrorDescription,
                                ["error_uri"] = context.ErrorUri
                            };

                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = 401;

                            return context.Response.WriteAsync(payload.ToString());
                        }
                    };
                });
            #endregion

            #region ���ÿ�������
            //services.AddCors(options =>
            //{
            //    var origins = Configuration.GetSection("AllowOrigins").Get<string[]>();
            //    options.AddDefaultPolicy(builder => builder.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
            //});
            #endregion

            #region ���miniProfile(https://miniprofiler.com/dotnet/AspDotNetCore)
            services.AddMemoryCache();
            services.AddMiniProfiler(options =>
            {
                // �趨���ʷ������URL��·�ɻ���ַ
                options.RouteBasePath = "/profiler";
                //����ѡ�����ƴ洢
                //����MemoryCacheStorage��Ĭ��Ϊ30���ӣ�
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(30);
                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                // �趨�������ڵ�λ�������½�
                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.Left;
                // �趨�ڵ�������ϸ���������ʾTime With Children����
                options.PopupShowTimeWithChildren = true;
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
            }).AddEntityFramework();
            #endregion

            #region using Swagger
            provider = BuildServiceProvider(services).GetRequiredService<IApiVersionDescriptionProvider>();
            services.AddSwaggerService(provider);
            #endregion

            #region ���EF Core����
            services.AddDbContext<BaseDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), builder => builder.EnableRetryOnFailure());
            });
            services.AddScoped<BaseDbContext>();
            #endregion

            #region ���HttpClient����
            services.AddHttpClient();
            #endregion

            #region ���ͳһģ����֤,����ʵ��IActionFilter
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // ʹ���Զ���ģ����֤
                options.InvalidModelStateResponseFactory = (context) =>
                {
                    StringBuilder errorMessage = new StringBuilder();
                    foreach (var item in context.ModelState.Values)
                    {
                        foreach (var error in item.Errors)
                        {
                            errorMessage.Append(error.ErrorMessage + "|");
                        }
                    }

                    return new JsonResult(errorMessage);
                };
            });
            #endregion

            //ע��������HttpContext��������
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IJwtProvider, JwtProvider>();

            //���Զ�ȡ������Ϣ������
            var str = ConfigHelper.GetDefaultJsonValue("AllowedHosts"); //Ĭ�������ļ�
            var otherStr = ConfigHelper.GetAppSetting("test.json", "AllowedHosts");   //����json�ļ���Ϣ
            //����д����������Ϣ�Զ�����ʽ�������Ե�����ʽע�ᵽ����������
            services.AddOptions().Configure<string>(Configuration.GetSection("AllowedHosts"));
            services.AddSingleton<ILogFormat, ContentFormat>();

            #region ���ñ����ʽ
            //services.Configure<WebEncoderOptions>(options =>
            //{
            //    options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(UnicodeRanges.All);
            //});
            #endregion

            #region ��ӽ������
            //��Ӷ����ݿ�ļ��
            services.AddHealthChecks().AddSqlServer(
                 Configuration.GetConnectionString("DefaultConnection"),
                 healthQuery: "SELECT 1;",
                 name: "sql server",
                 failureStatus: HealthStatus.Degraded,
                 tags: new[] { "db", "sql", "sqlserver" }
                );
            //���AspNetCore.HealthChecks.UI�Լ�HealthChecks.UI.InMemory.Storage��
            services.AddHealthChecksUI(setupSettings: setup =>
            {
                setup.AddHealthCheckEndpoint("sqlserver", "/health");
            }).AddInMemoryStorage();
            #endregion

            #region AutoMapper �Զ�ӳ��
            //Ѱ�ұ������м̳�Profile�������ʵ��
            services.AddAutoMapper(typeof(Startup));
            #endregion
        }

        // ע����Program.CreateHostBuilder�����Autofac���񹤳�(3.0�﷨)
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
                // ��swagger��ҳ�����ó������Զ����ҳ�棬�ǵ�����ַ�����д��,�ǵ�����ַ�����д��,�ǵ�����ַ�����д����������.index.html
                option.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("LL.FirstCore.index.html");
                option.DocExpansion(DocExpansion.List);//�۵�Api
                //�ر�ҳ��Schemeչʾ(��֪�����ߵ�����)
                option.DefaultModelsExpandDepth(-1);
                //���������������û��Ч��
                option.DefaultModelExpandDepth(-1);
            });
            #endregion
            //��ȡ��ǰ���еĽ�������
            var process = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            //������־�м��
            app.UseRequestLog();

            // ������������ ע���±���Щ�м����˳�򣬺���Ҫ ������������
            // CORS����
            //app.UseCors();
            // ��תhttps
            //app.UseHttpsRedirection();
            // ʹ�þ�̬�ļ�
            app.UseStaticFiles();
            //
            app.UseRouting();
            // �ȿ�����֤
            app.UseAuthentication();
            // Ȼ������Ȩ�м��
            app.UseAuthorization();
            // ���ܷ���
            app.UseMiniProfiler();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                //����"/healthchecks-ui"���ɿ������ӻ�ҳ��
                endpoints.MapHealthChecksUI();
                endpoints.MapControllers();
            });
        }

        private ServiceProvider BuildServiceProvider(IServiceCollection services)
        {
            return services.BuildServiceProvider();
        }

        //ָ�����ظ�ʽ
        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json";

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(pair =>
                    new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("data", new JObject(pair.Value.Data.Select(
                            p => new JProperty(p.Key, p.Value))))))))));

            return context.Response.WriteAsync(
                json.ToString());
        }
    }
}
