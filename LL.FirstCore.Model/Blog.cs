using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LL.FirstCore.Model
{
    [Table("Blog")]
    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }
    }
}
