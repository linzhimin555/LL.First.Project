using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LL.FirstCore.Controllers.v1
{
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
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
    }
}