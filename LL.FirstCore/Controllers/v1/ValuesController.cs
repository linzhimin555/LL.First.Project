using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LL.FirstCore.Common.Jwt;
using LL.FirstCore.HttpHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using LL.FirstCore.Common.Images;
using AutoMapper;
using LL.FirstCore.Model.Dto;
using LL.FirstCore.Model.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace LL.FirstCore.Controllers.v1
{
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;
        private readonly IJwtProvider _jwtProvider;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMapper _mapper;
        private readonly ISwaggerProvider _swaggerProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="jwtProvider"></param>
        /// <param name="clientFactory"></param>
        /// <param name="mapper"></param>
        public ValuesController(ILogger<ValuesController> logger, IJwtProvider jwtProvider, IHttpClientFactory clientFactory, IMapper mapper, ISwaggerProvider swaggerProvider)
        {
            _logger = logger;
            _jwtProvider = jwtProvider;
            _clientFactory = clientFactory;
            _mapper = mapper;
            _swaggerProvider = swaggerProvider;
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
        /// <param name="user">用户信息</param>
        /// <returns></returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserDto user)
        {
            //var result = await _userService.UserLoginAsync(userLogin.UserName, userLogin.Password);
            //if (result.success)
            //{
            var token = _jwtProvider.CreateJwtToken(new TokenModel { Uid = "123", UserName = user.UserName });
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
        }

        /// <summary>
        /// 获取swagger相关信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetSwaggerInfo()
        {
            var swagger = _swaggerProvider.GetSwagger("v1");
            var compoents = swagger.Components;
            //所有的标题
            var tags = swagger.Tags;
            var sercers = swagger.Servers;
            var security = swagger.SecurityRequirements;
            var paths = swagger.Paths;
            var info = swagger.Info;

            return Ok(paths);
        }

        /// <summary>
        /// post请求
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetId")]
        public ActionResult GetId()
        {
            var id = _jwtProvider.GetUserId();
            return new JsonResult(id);
        }

        /// <summary>
        /// 文件测试接口
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("UploadTest")]
        public IActionResult UploadTest(IFormFile formFile)
        {
            return Ok("上传文件成功!!!");
        }

        /// <summary>
        /// 测试http请求方法
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestHttpMethod")]
        public async Task<IActionResult> TestHttpMethod()
        {
            //var client = _clientFactory.CreateClient();
            //var parameters = new Dictionary<string, string>() { ["pageIndex"] = "1", ["pageSize"] = "10" };
            //var result = await CustomClient.GetData(client, "http://www.tzaqwl.com:5001/api/Story", parameters);
            //return Ok(result);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://www.tzaqwl.com:5001/api/Story?pageIndex=1&pageSize=10");
            using (var client = _clientFactory.CreateClient())
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    responseStream.Position = 0;
                    var result = await JsonSerializer.DeserializeAsync<Rootobject>(responseStream);
                    return Ok(result);
                }
            }

            return BadRequest();
        }

        /// <summary>
        /// 测试通过HttpClient上传文件
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestFileHttpMethod")]
        public async Task<IActionResult> TestFileHttpMethod()
        {
            Stream imageStream;
            var request = new HttpRequestMessage(HttpMethod.Get, "http://dsj.shui00.com/shuiliju/service/file.jsp?fileid=116104");
            using (var client = _clientFactory.CreateClient())
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    imageStream = await response.Content.ReadAsStreamAsync();
                    imageStream.Position = 0;
                    var postContent = new MultipartFormDataContent();
                    string boundary = string.Format("--{0}", DateTime.Now.Ticks.ToString("x"));
                    postContent.Headers.Add("ContentType", $"multipart/form-data, boundary={boundary}");
                    var requestUri = "https://web.dcyun.com:48119/zjsdz-api/api/file/upload/syn";
                    //一定要带上文件名称，不然就是500
                    postContent.Add(new StreamContent(imageStream, (int)imageStream.Length), "file", "test.png");
                    postContent.Add(new StringContent("taiZhouShi"), string.Format("\"{0}\"", "userCode"));
                    postContent.Add(new StringContent("2e049b9c258d7f3edb53cf4d997bf93d"), string.Format("\"{0}\"", "passWord"));
                    using (var fileClient = _clientFactory.CreateClient())
                    {
                        var uploadResponse = await fileClient.PostAsync(requestUri, postContent);
                        if (uploadResponse.IsSuccessStatusCode)
                        {
                            var responseStr = await uploadResponse.Content.ReadAsStringAsync();

                            return Ok(responseStr);
                        }
                    }
                }
            }

            return Ok("保存失败");
        }

        /// <summary>
        /// 图片合并到gif
        /// </summary>
        /// <returns></returns>
        [HttpGet("MergeImageToGif")]
        public IActionResult MergeImageToGif()
        {
            List<(string path, int duration)> images = new List<(string path, int duration)>()
            {
                (@"C:\Users\Administrator\Desktop\mergeImage\2020_07_13_13_50_13.png",100),
                (@"C:\Users\Administrator\Desktop\mergeImage\2020_07_13_13_50_17.png",100),
                (@"C:\Users\Administrator\Desktop\mergeImage\2020_07_13_13_50_19.png",100),
                (@"C:\Users\Administrator\Desktop\mergeImage\2020_07_13_13_50_25.png",100)
            };

            ImageHelper.RegularImageToGif(images, @"C:\Users\Administrator\Desktop\mergeImage\result1.gif");
            return Ok("合并成功");
        }

        /// <summary>
        /// 测试Automapper
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        [HttpPost("UpdateUserInfo")]
        public IActionResult UpdateUserInfo([FromBody] UserInfoDto userInfo)
        {
            var users = _mapper.Map<BaseUserInfo>(userInfo);

            return Ok(users);
        }

        /// <summary>
        /// 用户输入实体类
        /// </summary>
        public class UserDto
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Rootobject
        {
            public bool success { get; set; }
            public string msg { get; set; }
            public Response response { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Response
        {
            public int page { get; set; }
            public int pageCount { get; set; }
            public int dataCount { get; set; }
            public int PageSize { get; set; }
            public Datum[] data { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Datum
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string Img { get; set; }
            public int ReadNum { get; set; }
            public bool IsDel { get; set; }
            public DateTime CreateTime { get; set; }
            public int OrderId { get; set; }
        }


    }
}