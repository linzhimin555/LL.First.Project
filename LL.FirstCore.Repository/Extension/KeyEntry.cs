using System;
using System.Collections.Generic;
using System.Text;

namespace LL.FirstCore.Repository.Extension
{
    public class KeyEntry
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }
    }
}
