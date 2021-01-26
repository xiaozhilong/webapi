using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiVersion.Swagger
{
    [AttributeUsage(AttributeTargets.All)]
    public class VersionedRoute : Attribute
    {
        public VersionedRoute(string name, int version)
        {
            Name = name;
            Version = version;
        }
        public string Name { get; set; }
        public int Version { get; set; }
    }
}