using LL.FirstCore.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LL.FirstCore.Common.Logger
{
    public class ContentFormat : ILogFormat
    {
        public string Format(LogContent content)
        {
            return FormatStr(content);
        }

        private string FormatStr(LogContent content)
        {
            int line = 1;
            var result = new StringBuilder();
            Line1(result, content, ref line);
            Line2(result, content, ref line);
            Line9(result, content, ref line);
            Line10(result, content, ref line);
            Line11(result, content, ref line);
            Line12(result, content, ref line);
            Line13(result, content, ref line);
            Line14(result, content, ref line);
            Finish(result);

            return result.ToString();
        }

        #region 日志记录
        /// <summary>
        /// 第1行
        /// </summary>
        protected void Line1(StringBuilder result, LogContent content, ref int line)
        {
            AppendLine(result, content, (r, c) =>
            {
                r.AppendFormat("{0}: {1} >> ", c.Level, c.LogName);
                r.AppendFormat("{0}: {1}   ", "操作时间", c.OperationTime);
            }, ref line);
        }

        /// <summary>
        /// 第2行
        /// </summary>
        protected void Line2(StringBuilder result, LogContent content, ref int line)
        {
            AppendLine(result, content, (r, c) =>
            {
                Append(r, "线程号", c.ThreadId);
            }, ref line);
        }

        /// <summary>
        /// 第9行
        /// </summary>
        protected void Line9(StringBuilder result, LogContent content, ref int line)
        {
            if (string.IsNullOrWhiteSpace(content.Caption))
                return;
            AppendLine(result, content, (r, c) =>
            {
                r.AppendFormat("{0}: {1}", "标题", c.Caption);
            }, ref line);
        }

        /// <summary>
        /// 第10行
        /// </summary>
        protected void Line10(StringBuilder result, LogContent content, ref int line)
        {
            if (content.Content == null || content.Content.Length == 0)
                return;
            AppendLine(result, content, (r, c) =>
            {
                r.AppendLine($"内容:");
                r.Append(c.Content);
            }, ref line);
        }

        /// <summary>
        /// 第11行
        /// </summary>
        protected void Line11(StringBuilder result, LogContent content, ref int line)
        {
            if (content.Sql == null || content.Sql.Length == 0)
                return;
            AppendLine(result, content, (r, c) =>
            {
                r.AppendLine($"Sql语句:");
                r.Append(c.Sql);
            }, ref line);
        }

        /// <summary>
        /// 第12行
        /// </summary>
        protected void Line12(StringBuilder result, LogContent content, ref int line)
        {
            if (content.SqlParams == null || content.SqlParams.Length == 0)
                return;
            AppendLine(result, content, (r, c) =>
            {
                r.AppendLine($"Sql参数:");
                r.Append(c.SqlParams);
            }, ref line);
        }

        /// <summary>
        /// 第13行
        /// </summary>
        protected void Line13(StringBuilder result, LogContent content, ref int line)
        {
            if (content.Exception == null)
                return;
            AppendLine(result, content, (r, c) =>
            {
                r.AppendLine($"异常: {GetExceptionTypes(c.Exception)} { GetErrorCode(c.ErrorCode) }");
                r.Append($"   { Warning.GetMessage(c.Exception) }");
            }, ref line);
        }

        /// <summary>
        /// 第14行
        /// </summary>
        protected void Line14(StringBuilder result, LogContent content, ref int line)
        {
            if (content.Exception == null)
                return;
            AppendLine(result, content, (r, c) =>
            {
                r.AppendLine($"堆栈跟踪:");
                r.Append(c.Exception.StackTrace);
            }, ref line);
        }

        /// <summary>
        /// 结束
        /// </summary>
        protected void Finish(StringBuilder result)
        {
            for (int i = 0; i < 125; i++)
                result.Append("-");
        }
        #endregion

        /// <summary>
        /// 添加行
        /// </summary>
        protected void AppendLine(StringBuilder result, LogContent content, Action<StringBuilder, LogContent> action, ref int line)
        {
            Append(result, content, action, ref line);
            result.AppendLine();
        }

        /// <summary>
        /// 添加行
        /// </summary>
        protected void Append(StringBuilder result, LogContent content, Action<StringBuilder, LogContent> action, ref int line)
        {
            result.AppendFormat("{0}. ", line++);
            action(result, content);
        }

        /// <summary>
        /// 添加日志内容
        /// </summary>
        /// <param name="result">日志结果</param>
        /// <param name="caption">标题</param>
        protected void Append(StringBuilder result, string caption, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            result.AppendFormat("{0}: {1}   ", caption, value);
        }

        /// <summary>
        /// 获取异常类型列表
        /// </summary>
        /// <param name="exception">异常</param>
        private string GetExceptionTypes(Exception exception)
        {
            return string.Join(",", Warning.GetExceptions(exception).Select(t => t.GetType().ToString()));
        }

        /// <summary>
        /// 获取错误码
        /// </summary>
        /// <param name="errorCode">错误码</param>
        private string GetErrorCode(string errorCode)
        {
            if (string.IsNullOrWhiteSpace(errorCode))
                return string.Empty;
            return $"-- 错误码: {errorCode}";
        }
    }
}
