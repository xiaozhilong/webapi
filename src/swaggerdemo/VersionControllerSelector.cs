using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace WebApiVersion
{
    /// <summary>
    /// 版本控制
    /// </summary>
    public class VersionControllerSelector : DefaultHttpControllerSelector
    {
        private HttpConfiguration config;
        private IDictionary<string, HttpControllerDescriptor> controllrerMapping;

        public VersionControllerSelector(HttpConfiguration config):base(config)
        {
            this.config = config;
        }
        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            Dictionary<string, HttpControllerDescriptor> dict = new Dictionary<string, HttpControllerDescriptor>();
            foreach (var item in config.Services.GetAssembliesResolver().GetAssemblies())
            {
                var controllerTypes = item.GetTypes().Where(t => t.IsAbstract == false && typeof(ApiController).IsAssignableFrom(t));
                foreach (var ctrlType in controllerTypes)
                {
                    string ctrlTypeNameSpace = ctrlType.Namespace;
                    var match = Regex.Match(ctrlTypeNameSpace, @".v(\d)");
                    if (!match.Success)
                    {
                        continue;
                    }
                    string versionNum = match.Groups[1].Value;//把版本号提取出来
                    string ctrlTypeName = ctrlType.Name;//DefaultControl
                    var matchControler = Regex.Match(ctrlTypeName, "^(.+)Controller$");//把控制器的名字提取出来
                    if (!matchControler.Success)
                    {
                        continue;
                    }
                    string ctrlName = matchControler.Groups[1].Value;

                    string key = ctrlName + "v" + versionNum;
                    dict[key] = new HttpControllerDescriptor(config, ctrlName, ctrlType);
                }
            }
            controllrerMapping = dict;
            return dict;
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            string controller = (string)request.GetRouteData().Values["controller"];

            if (controllrerMapping == null)
            {
                var ctrlMapping = GetControllerMapping();
            }
            var matchVersion = Regex.Match(request.RequestUri.PathAndQuery, @"/v(\d+)/");
            if (!matchVersion.Success)
            {
                return base.SelectController(request);
            }
            string versionNum = matchVersion.Groups[1].Value;
            string key = controller + "v" + versionNum;
            if (controllrerMapping.ContainsKey(key))
            {
                return controllrerMapping[key];
            }
            else
            {
                return base.SelectController(request);
            }

        }
    }
}