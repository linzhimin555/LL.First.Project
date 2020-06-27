using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Common.Logger
{
    public class EFLogger : ILogger
    {
        private readonly string _CategoryName;
        public EFLogger(string categoryName)
        {
            this._CategoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            //ef core执行数据库查询时的categoryName为Microsoft.EntityFrameworkCore.Database.Command,日志级别为Information
            if (_CategoryName == "Microsoft.EntityFrameworkCore.Database.Command" && logLevel == LogLevel.Information)
            {
                var logContent = formatter(state, exception);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(logContent);
                Console.ResetColor();
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }
    }
}
