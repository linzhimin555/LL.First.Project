using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using LL.FirstCore.Common.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LL.FirstCore.Controllers.v1
{
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private readonly ILogger<ValuesController> _logger;

        private readonly IJwtProvider _jwtProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="jwtProvider"></param>
        /// <param name="userService"></param>
        public ValuesController(ILogger<ValuesController> logger, IJwtProvider jwtProvider)
        {
            _logger = logger;
            _jwtProvider = jwtProvider;
        }

        /// <summary>
        /// v1版本
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get() => Ok(new string[] { HttpContext.GetRequestedApiVersion().ToString() });

        /// <summary>
        /// v2版本
        /// </summary>
        /// <returns></returns>
        [HttpPost, MapToApiVersion("2")]
        public IActionResult Post() => Ok(new string[] { "version2.0" });


        /// <summary>
        ///  登陆 获取token
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login(string userName ,string Password)
        {
            //var result = await _userService.UserLoginAsync(userLogin.UserName, userLogin.Password);
            //if (result.success)
            //{
                var token = _jwtProvider.CreateJwtToken(new TokenModel { Uid ="123" , UserName = userName });
                //return new JsonResult("");
            //}
            return new JsonResult("");
        }

      
        /// <summary>
        /// 这是个测试
        /// </summary>
        /// <returns></returns>
        [HttpGet("SetToken")]
        public ActionResult GetToken()
        {
            var id = _jwtProvider.GetUserId();
            //_categoryService.Test();
            var uid = "admin";
            var token = _jwtProvider.CreateJwtToken(new TokenModel { Uid = uid, UserName = "admin" });

            SerializeJwt(token);
            return new JsonResult(token);
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        [Authorize]
        private void SerializeJwt(string jwtStr)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);
            object role;
            try
            {
                //jwtToken.Payload.TryGetValue(ClaimTypes.Role, out role);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            //var tm = new TokenModelJwt
            //{
            //    Uid = (jwtToken.Id).ObjToInt(),
            //    Role = role != null ? role.ObjToString() : "",
            //};
            //return tm;
        }

        /// <summary>
        /// post请求
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetId")]
        [Authorize]
        public ActionResult GetId()
        {
            var id = _jwtProvider.GetUserId();
            return new JsonResult(id);
        }
    }
}