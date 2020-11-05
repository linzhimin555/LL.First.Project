using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LL.FirstCore.Controllers.v2
{
    [ApiVersion("2")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// v2版本
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get() => Ok(new string[] { "version2.0" });
    }
}