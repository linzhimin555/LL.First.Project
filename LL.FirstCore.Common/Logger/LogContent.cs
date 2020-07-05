using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Common.Logger
{
    public class LogContent
    {
        /// <summary>
        /// 日志名称
        /// </summary>
        public string LogName { get; set; }
        /// <summary>
        /// 日志级别
        /// </summary>
        public string Level { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public string OperationTime { get; set; }
        /// <summary>
        /// 线程号
        /// </summary>
        public string ThreadId { get; set; }
        /// <summary>
        /// 日志标题
        /// </summary>
        public string Caption { get; set; }
        /// <summary>
        /// 日志内容
        /// </summary>
        public StringBuilder Content { get; set; }
        /// <summary>
        /// Sql语句
        /// </summary>
        public StringBuilder Sql { get; set; }
        /// <summary>
        /// Sql参数
        /// </summary>
        public StringBuilder SqlParams { get; set; }
        /// <summary>
        /// 错误码
        /// </summary>
        public string ErrorCode { get; set; }
        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; set; }

        public void SetContent(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            if (Content == null)
                Content = new StringBuilder();
            Content.Append(value);
        }

        public void SetSql(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            if (Sql == null)
                Sql = new StringBuilder();
            Sql.Append(value);
        }

        public void SetSqlParams(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            if (SqlParams == null)
                SqlParams = new StringBuilder();
            SqlParams.Append(value);
        }
    }
}
