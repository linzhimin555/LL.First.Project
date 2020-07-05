using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Common.Logger
{
    /// <summary>
    /// Ef日志提供器
    /// </summary>
    public class EFLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// 初始化Ef日志提供器
        /// </summary>
        /// <param name="category">日志分类</param>
        public ILogger CreateLogger(string categoryName)
        {
            return categoryName.StartsWith("Microsoft.EntityFrameworkCore") ? new EFLogger(categoryName) : NullLogger.Instance;
        }

        public void Dispose()
        {

        }
    }
}
