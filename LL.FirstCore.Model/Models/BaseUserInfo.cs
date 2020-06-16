using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LL.FirstCore.Model.Models
{
    /// <summary>
    /// 
    /// </summary>
    [Table("BaseUserInfo")]
    public class BaseUserInfo : BaseEntity
    {
        [Column("UserId")]
        public override int Id { get; set; }
        /// <summary>
        /// 登录账户
        /// </summary>
        [Required]
        public string Account { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 密码加密秘钥
        /// </summary>
        public string Secretkey { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string HeadIcon { get; set; }
        /// <summary>
        /// 性别(0:女;1:男)
        /// </summary>
        public int Gender { get; set; } = 0;
        /// <summary>
        /// 生日
        /// </summary>
        public DateTime Birthday { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 电子邮件
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// QQ号
        /// </summary>
        public string OICQ { get; set;}
        /// <summary>
        /// 微信号
        /// </summary>
        public string WeChat { get; set; }
    }
}
