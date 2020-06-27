using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Common.Logger
{
    /// <summary>
    /// EF日志提供类
    /// </summary>
    public class EFLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new EFLogger(categoryName);
        }

        public void Dispose()
        {
            
        }
    }
}
