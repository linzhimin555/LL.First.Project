using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Common.Logger
{
    /// <summary>
    /// 日志格式器
    /// </summary>
    public interface ILogFormat
    {
        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="content">日志内容</param>
        string Format(LogContent content);
    }
}
