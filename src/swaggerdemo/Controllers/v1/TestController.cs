using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebApiVersion.Swagger;

namespace WebApiVersion.Controllers.v1
{
    [VersionedRoute("api/version", 1)]
    public class TestController : ApiController
    {
        // GET: Test
         [HttpGet]
        public string Index()
        {
            return "this is v1 ";
        }
    }
}