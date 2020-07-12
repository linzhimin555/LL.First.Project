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

namespace LL.FirstCore.Middleware
{
    public class RequestLogMilddleware
    {
        /// <summary>
        /// 请求委托
        /// </summary>
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="serviceProvider"></param>
        public RequestLogMilddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
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

            var _services = _serviceProvider.GetRequiredService<IRequestLogServices>();
            _services.Insert(entity, true);
            await _next(context);
        }

        /// <summary>
        /// 获取客户端请求的ip地址信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetClientIP(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].SafeString();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.SafeString();
            }

            return ip;
        }
    }
}
