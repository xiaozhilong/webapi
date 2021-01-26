using Swashbuckle.Swagger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace WebApiVersion.Swagger
{
    public class CachingSwaggerProvider : ISwaggerProvider
    {
        private static ConcurrentDictionary<string, SwaggerDocument> _cache =
            new ConcurrentDictionary<string, SwaggerDocument>();

        private readonly ISwaggerProvider _swaggerProvider;

        public CachingSwaggerProvider(ISwaggerProvider swaggerProvider)
        {
            _swaggerProvider = swaggerProvider;
        }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            var cacheKey = String.Format("{0}_{1}", rootUrl, apiVersion);
            SwaggerDocument srcDoc;
            //只读取一次
            if (!_cache.TryGetValue(cacheKey, out srcDoc))
            {
                srcDoc = _swaggerProvider.GetSwagger(rootUrl, apiVersion);
                var patht = new Dictionary<string, PathItem>();
                foreach (var item in srcDoc.paths)
                {
                    var arr = item.Key.Split('/');
                    var i = arr[3].LastIndexOf('.') + 1;
                    if (i != -1)
                    {
                        arr[3] = arr[3].Substring(i);
                    }
                    patht.Add(string.Join("/", arr), item.Value);
                }
                srcDoc.paths = patht;
                HashSet<string> moduleList = new HashSet<string>();
                srcDoc.vendorExtensions = new Dictionary<string, object>
                {
                    {"ControllerDesc", GetControllerDesc(moduleList)},
                    {"AreaDescription", moduleList}
                };
                _cache.TryAdd(cacheKey, srcDoc);
            }
            return srcDoc;
        }

        /// <summary>
        /// 从API文档中读取控制器描述
        /// </summary>
        /// <returns>所有控制器描述</returns>
        public static ConcurrentDictionary<string, string> GetControllerDesc(HashSet<string> moduleList)
        {
            string xmlpath = String.Format("{0}/bin/swagger.XML", AppDomain.CurrentDomain.BaseDirectory);
            ConcurrentDictionary<string, string> controllerDescDict = new ConcurrentDictionary<string, string>();
            if (File.Exists(xmlpath))
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(xmlpath);
                int cCount = "Controller".Length;
                foreach (XmlNode node in xmldoc.SelectNodes("//member"))
                {
                    var type = node.Attributes["name"].Value;
                    if (type.StartsWith("T:"))
                    {
                        //控制器
                        var arrPath = type.Split('.');
                        var length = arrPath.Length;
                        var controllerName = arrPath[length - 1];
                        if (controllerName.EndsWith("Controller"))
                        {
                            //模块信息
                            var moduleName = arrPath[length - 2];
                            moduleList.Add(moduleName);
                            //获取控制器注释
                            var summaryNode = node.SelectSingleNode("summary");
                            string key = controllerName.Remove(controllerName.Length - cCount, cCount);
                            if (summaryNode != null && !String.IsNullOrEmpty(summaryNode.InnerText) && !controllerDescDict.ContainsKey(key))
                            {

                                controllerDescDict.TryAdd(key, summaryNode.InnerText.Trim());
                            }
                        }
                    }
                }
            }
            return controllerDescDict;
        }
    }
}