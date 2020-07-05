using LL.FirstCore.Common.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LL.FirstCore.Common.Logger
{
    public class EFLogger : ILogger
    {
        /// <summary>
        /// Ef跟踪日志名
        /// </summary>
        public const string TraceLogName = "EfTraceLog";

        private readonly string _CategoryName;
        public EFLogger(string categoryName)
        {
            this._CategoryName = categoryName;
        }

        /// <summary>
        /// 日志记录
        /// </summary>
        /// <typeparam name="TState">状态类型</typeparam>
        /// <param name="logLevel">日志级别</param>
        /// <param name="eventId">事件编号</param>
        /// <param name="state">状态</param>
        /// <param name="exception">异常</param>
        /// <param name="formatter">日志内容</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var _logger = NLog.LogManager.GetLogger(TraceLogName);
            //ef core执行数据库查询时的categoryName为Microsoft.EntityFrameworkCore.Database.Command,日志级别为Information
            if (_CategoryName == "Microsoft.EntityFrameworkCore.Database.Command" && logLevel == LogLevel.Information)
            {
                //var logContent = formatter(state, exception);
                //log.Log(ConvertTo(logLevel), logContent);
                var content = new LogContent
                {
                    LogName = _logger.Name,
                    Level = Enum.GetName(typeof(LogLevel), logLevel),
                    OperationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Caption = $"执行Ef操作：",
                    ThreadId = Thread.CurrentThread.ManagedThreadId.ToString()
                };
                content.SetContent($"事件Id: {eventId.Id}{Environment.NewLine}");
                content.SetContent($"事件名称: {eventId.Name}{Environment.NewLine}");
                content.SetContent($"事件内容：{state.SafeString()}{Environment.NewLine}");
                if (!(state is IEnumerable list))
                    return;
                var dictionary = new Dictionary<string, string>();
                foreach (KeyValuePair<string, object> item in list)
                    dictionary.Add(item.Key, item.Value.SafeString());
                AddDictionary(dictionary, content);
                content.Exception = exception;

                var provider = new FormatProvider(new ContentFormat());
                _logger.Log(ConvertTo(logLevel), provider, content);
            }
        }

        /// <summary>
        /// 添加字典内容
        /// </summary>
        private void AddDictionary(IDictionary<string, string> dictionary, LogContent log)
        {
            AddElapsed(GetValue(dictionary, "elapsed"), log);
            var sqlParams = GetValue(dictionary, "parameters");
            AddSql(GetValue(dictionary, "commandText"), log);
            AddSqlParams(sqlParams, log);
        }

        /// <summary>
        /// 添加执行时间
        /// </summary>
        private void AddElapsed(string value, LogContent log)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            log.SetContent($"执行时间: {value} 毫秒{Environment.NewLine}");
        }

        /// <summary>
        /// 添加Sql执行语句
        /// </summary>
        private void AddSql(string sql, LogContent log)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return;
            log.SetSql($"{sql}{Environment.NewLine}");
        }

        /// <summary>
        /// 添加Sql参数
        /// </summary>
        private void AddSqlParams(string value, LogContent log)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            log.SetSqlParams(value);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        private string GetValue(IDictionary<string, string> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];
            return string.Empty;
        }

        /// <summary>
        /// 转换日志等级
        /// </summary>
        private NLog.LogLevel ConvertTo(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return NLog.LogLevel.Trace;
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case LogLevel.Information:
                    return NLog.LogLevel.Info;
                case LogLevel.Warning:
                    return NLog.LogLevel.Warn;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
                case LogLevel.Critical:
                    return NLog.LogLevel.Fatal;
                default:
                    return NLog.LogLevel.Off;
            }
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <summary>
        /// 起始范围
        /// </summary>
        public IDisposable BeginScope<TState>(TState state)
        {
            throw null;
        }
    }
}
