using LL.FirstCore.Common.Config;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LL.FirstCore.Authorization
{
    /// <summary>
    /// Jwt帮助类
    /// </summary>
    public class JwtHelper
    {
        /// <summary>
        /// 颁发Jwt字符串
        /// </summary>
        /// <param name="tokenModle"></param>
        /// <returns></returns>
        public static string IssueJwtStr(JwtTokenModel tokenModle)
        {
            if (ConfigHelper.IsExistNode("JwtSetting"))
            {
                var issure = ConfigHelper.GetDefaultJsonValue("JwtSetting:Issure");
                var audience = ConfigHelper.GetDefaultJsonValue("JwtSetting:Audience");
                //秘钥要足够的长
                var secret = ConfigHelper.GetDefaultJsonValue("JwtSetting:Secret");

                var claims = new List<Claim>()
                {
                    new Claim(JwtRegisteredClaimNames.Iss,issure),  //签发人   
                    new Claim(JwtRegisteredClaimNames.Aud,audience),    //受众
                    new Claim(JwtRegisteredClaimNames.Jti,tokenModle.UserId.ToString()),  //编号
                    new Claim(JwtRegisteredClaimNames.Iat,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()}"),  //Jwt的颁发时间,采用标准的unix时间,用于验证过期
                    new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()}"),  //生效时间
                    new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddSeconds(1000)).ToUnixTimeSeconds()}"), //过期时间
                };

                claims.AddRange(tokenModle.Role.Split(',').Select(v => new Claim(ClaimTypes.Role, v)));

                //秘钥 (SymmetricSecurityKey 对安全性的要求，密钥的长度太短会报出异常,至少16位)
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                //数字签名
                var signature = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var jwt = new JwtSecurityToken(issuer: issure, claims: claims, signingCredentials: signature);
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtStr = jwtHandler.WriteToken(jwt);

                return jwtStr;
            }

            return string.Empty;
        }

        /// <summary>
        /// 解析Jwttoken信息
        /// </summary>
        /// <param name="jwtStr">token信息</param>
        /// <returns></returns>
        public static JwtTokenModel SerializeJwt(string jwtStr)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);
            object role;
            try
            {
                jwtToken.Payload.TryGetValue(ClaimTypes.Role, out role);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            var entity = new JwtTokenModel
            {
                UserId = int.Parse(jwtToken.Id),
                Role = role?.ToString() ?? string.Empty,
            };

            return entity;
        }
    }

    /// <summary>
    /// Jwt令牌实体类
    /// </summary>
    public class JwtTokenModel
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 职能
        /// </summary>
        public string Work { get; set; }
    }
}
