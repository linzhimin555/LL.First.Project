using LL.FirstCore.Common.Extensions;
using LL.FirstCore.Model.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using LL.FirstCore.IServices.Base;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace LL.FirstCore.Middleware
{
    public class RequestLogMilddleware
    {
        /// <summary>
        /// 请求委托
        /// </summary>
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RequestLogMilddleware> _logger;
        private Stopwatch _stopwatch;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="serviceProvider"></param>
        public RequestLogMilddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<RequestLogMilddleware> logger)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _stopwatch = new Stopwatch();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _stopwatch.Restart();
            var request = context.Request;
            var entity = new RequestLogEntity()
            {
                TranceId = Guid.NewGuid().ToString(),
                ClientIp = GetClientIP(context),
                RequestMethod = request.Method,
                RequestHeaders = JsonSerializer.Serialize(request.Headers.ToDictionary(x => x.Key, v => string.Join(";", v.Value.ToList()))),
                Url = request.Path,
                ExecutedTime = DateTime.Now,
            };
            //注意：文件上传接口可能需要单独处理
            //miniprofiler一直请求接口结果,所以此处过滤该请求信息
            if (entity.Url != "/profiler/results")
            {
                switch (request.Method.ToLower())
                {
                    case "get":
                        entity.RequestParamters = request.QueryString.Value;
                        break;
                    case "post":
                        //确保请求体信息可被多次读取
                        request.EnableBuffering();
                        var reader = new StreamReader(request.Body, Encoding.UTF8);
                        entity.RequestParamters = await reader.ReadToEndAsync();
                        //流位置重置为0
                        request.Body.Position = 0;
                        break;
                    case "put":
                        //确保请求体信息可被多次读取
                        request.EnableBuffering();
                        var readers = new StreamReader(request.Body, Encoding.UTF8);
                        entity.RequestParamters = await readers.ReadToEndAsync();
                        //流位置重置为0
                        request.Body.Position = 0;
                        break;
                    case "delete":
                        entity.RequestParamters = request.QueryString.Value;
                        break;
                    case "options":
                        entity.RequestParamters = string.Empty;
                        break;
                }

                // 获取Response.Body内容
                var originalBodyStream = context.Response.Body;
                //引用类型，共享内存地址，所以memory也会被赋值
                using (var memory = new MemoryStream())
                {
                    context.Response.Body = memory;
                    await _next(context);
                    _logger.LogInformation("开始处理请求结果。。。。。。。。。");
                    ResponseDataLog(memory);
                    memory.Position = 0;
                    await memory.CopyToAsync(originalBodyStream);
                }

                context.Response.OnCompleted(() =>
                {
                    _stopwatch.Stop();
                    entity.ElaspedTime = $"{_stopwatch.ElapsedMilliseconds}ms";
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var _services = _serviceProvider.GetRequiredService<IRequestLogServices>();
                        _services.Insert(entity, true);
                    }
                    _logger.LogInformation("请求结果处理结束。。。。。。。。。");
                    return Task.CompletedTask;
                });
            }
            else
            {
                await _next(context);
            }
        }

        /// <summary>
        /// 获取客户端请求的ip地址信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string GetClientIP(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].SafeString();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.SafeString();
            }

            return ip;
        }

        /// <summary>
        /// 返回结果输出到控制台
        /// </summary>
        /// <param name="stream"></param>
        private void ResponseDataLog(MemoryStream stream)
        {
            stream.Position = 0;
            var responseBody = new StreamReader(stream).ReadToEnd();

            if (!string.IsNullOrEmpty(responseBody))
            {
                _logger.LogInformation("请求结果:" + responseBody);
            }
        }
    }
}
