using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LL.FirstCore.Middleware
{
    /// <summary>
    /// 中间件注册帮助类
    /// </summary>
    public static class MiddlewareHelpers
    {
        /// <summary>
        /// 请求日志记录中间件
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequestLog(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLogMilddleware>();
        }
    }
}
