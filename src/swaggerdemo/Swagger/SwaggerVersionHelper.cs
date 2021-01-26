﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace WebApiVersion.Swagger
{
    public class SwaggerVersionHelper
    {
        public static bool ResolveVersionSupportByRouteConstraint(ApiDescription apiDesc, string targetApiVersion)
        {
            var attr = apiDesc.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<VersionedRoute>().FirstOrDefault();
            return attr != null && attr.Version == Convert.ToInt32(targetApiVersion.TrimStart('v'));
        }
    }
}