using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LL.FirstCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LL.FirstCore.Controllers.v1
{
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        /// <summary>
        /// 用户登录并获取Jwt
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        [HttpGet]
        [Route("JwtTokenInfo")]
        public IActionResult JwtTokenInfo(string username, string password)
        {
            var isSuccess = false;
            //此处会从数据库获取对应角色信息
            var userRole = "Admin";
            var jwtStr = "login fail!!!"; ;
            if (!string.IsNullOrEmpty(userRole))
            {
                var tokenModel = new JwtTokenModel() { UserId = 1, Role = userRole };
                jwtStr = JwtHelper.IssueJwtStr(tokenModel); //用户登录,分发具有一定规则的token令牌
                isSuccess = true;
            }

            return Ok(new
            {
                Success = isSuccess,
                token = jwtStr
            });
        }

        /// <summary>
        /// 获取用户个人信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("UserInfo")]
        [Authorize]
        //[Authorize(Roles = "admin", AuthenticationSchemes = "Bearer")]
        public IActionResult UserInfo()
        {
            var obj = new
            {
                UserName="阿甘",
                Age=25
            };

            return Ok(obj);
        }
    }
}