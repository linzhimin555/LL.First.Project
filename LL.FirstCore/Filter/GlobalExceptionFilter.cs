using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LL.FirstCore.Filter
{
    /// <summary>
    /// 全局异常处理
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostEnvironment _env;
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public const string GlobalErrorLogName = "GlobalErrorLog";

        public GlobalExceptionFilter(IWebHostEnvironment env, ILogger<GlobalExceptionFilter> logger)
        {
            _env = env;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var trace = new StackTrace(context.Exception, true);
            var fileName = trace.GetFrame(0).GetFileName();
            var methodName = trace.GetFrame(0).GetMethod().ReflectedType.FullName;
            var lineNum = trace.GetFrame(0).GetFileLineNumber();
            ContentResult result = new ContentResult
            {
                StatusCode = 500,
                ContentType = "text/json;charset=utf-8;"
            };

            string error = string.Empty;
            void ReadException(Exception ex)
            {
                error += $@"<br>【异常信息】：{context.Exception.Message}<br>【异常类型】：{ex.GetType()}<br>【堆栈调用】:{ex.StackTrace}";
                error = error.Replace("<br>", "\r\n");
                error = error.Replace("位置", "<strong style=\"color:red\">位置</strong>");
                if (ex.InnerException != null)
                {
                    ReadException(ex.InnerException);
                }
            }
            ReadException(context.Exception);

            _logger.LogError(error);

            if (_env.IsDevelopment())
            {
                var json = new { message = context.Exception.Message, detail = error };
                result.Content = JsonConvert.SerializeObject(json);
            }
            else
            {
                result.Content = "抱歉，服务端出错了";
            }

            //可以做一些扩展，比如加入短信通知，邮箱通知功能

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }

    /// <summary>
    /// 定义服务端错误信息
    /// </summary>
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object value) : base(value)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }

    //返回错误信息
    public class JsonErrorResponse
    {
        /// <summary>
        /// 生产环境的消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 开发环境的消息
        /// </summary>
        public string DevelopmentMessage { get; set; }
    }
}
