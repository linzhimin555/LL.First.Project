using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LL.FirstCore.SwaggerFilter
{
    /// <summary>
    /// 设置版本信息
    /// </summary>
    public class SetVersionInPathDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var updatedPaths = new OpenApiPaths();

            //将请求路径中的v{version}信息替换为对应的版本号信息
            foreach (var entry in swaggerDoc.Paths)
            {
                updatedPaths.Add(
                    entry.Key.Replace("{version}", swaggerDoc.Info.Version),
                    entry.Value);
            }

            swaggerDoc.Paths = updatedPaths;
        }
    }
}
