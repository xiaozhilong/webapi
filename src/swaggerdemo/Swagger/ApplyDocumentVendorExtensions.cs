using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Description;

namespace WebApiVersion.Swagger
{
    internal class ApplyDocumentVendorExtensions : IDocumentFilter
    {
        /// <summary>
        /// //swagger版本控制过滤
        /// </summary>
        /// <param name="swaggerDoc">文档</param>
        /// <param name="schemaRegistry">schema注册</param>
        /// <param name="apiExplorer">api概览</param>
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            //缓存目标路由api
            IDictionary<string, PathItem> match = new Dictionary<string, PathItem>();
            //取版本
            var version = swaggerDoc.info.version;
            foreach (var path in swaggerDoc.paths)
            {
                //过滤命名空间 按名称空间区分版本
                if (path.Key.Contains(string.Format("/{0}/", version)))
                {
                    //匹配controller descript中的版本信息
                    Regex r = new Regex("/\\w+" + version, RegexOptions.IgnoreCase);
                    string newKey = path.Key;
                    if (r.IsMatch(path.Key))
                    {
                        var routeinfo = r.Match(path.Key).Value;
                        //修正controller别名路由符合RoutePrefix配置的路由 如api/v2/ValuesV2 修正为 api/v2/Values
                        newKey = path.Key.Replace(routeinfo, routeinfo.Replace(version.ToLower(), "")).Replace(
                            routeinfo, routeinfo.Replace(version.ToUpper(), ""));
                    }
                    //保存修正的path
                    match.Add(newKey, path.Value);
                }
            }
            //当前版本的swagger document
            swaggerDoc.paths = match;
        }
    }
}