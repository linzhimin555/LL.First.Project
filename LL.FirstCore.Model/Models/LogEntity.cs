using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Model.Models
{
    /// <summary>
    /// Http请求日志记录实体
    /// </summary>
    public class LogEntity : BaseEntity
    {
        /// <summary>
        /// 追踪编号
        /// </summary>
        public string TranceId { get; set; }
        /// <summary>
        /// 客户端请求地址
        /// </summary>
        public string ClientIp { get; set; }
        /// <summary>
        /// 请求方式
        /// </summary>
        public string RequestMethod { get; set; }
        /// <summary>
        /// 请求头
        /// </summary>
        public string RequestHeaders { get; set; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 请求时间
        /// </summary>
        public DateTime? ExecutedTime { get; set; }
        /// <summary>
        /// 请求参数信息
        /// </summary>
        public string RequestParamters { get; set; }
    }
}
