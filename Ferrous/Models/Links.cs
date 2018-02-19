using Ferrous.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using static Ferrous.Controllers.Authorization;

namespace Ferrous.Models
{
    public class HTTPrel : Attribute
    {
        public string rel;
        public HTTPrel(HTTPrelList rel)
        {
            this.rel = rel.ToString();
        }
    }

    public enum HTTPrelList {
        self = 0,
        update = 1,
        delete = 2,
        create = 3
    }

    public static class HTTPMethod
    {
        public static string GET = "GET";
        public static string POST = "POST";
        public static string PUT = "PUT";
        public static string DELETE = "DELETE";
    }    

    public class LinkHelper
    {
        private ClaimsPrincipal user;
        private Type type;
        private List<Link> Links = new List<Link>();
        private IUrlHelper urlHelper;

        public LinkHelper(ClaimsPrincipal user, Type type, IUrlHelper urlHelper)
        {
            this.user = user;
            this.type = type;
            this.urlHelper = urlHelper;
        }

        public LinkHelper(ClaimsPrincipal user, Type type, IUrlHelper urlHelper, string[] routes)
        {
            this.user = user;
            this.type = type;
            this.urlHelper = urlHelper;

            foreach(var route in routes)
            {
                AddLink(route);
            }
        }

        public bool AddLink(string route)
        {
            MethodInfo controllerMethod = type.GetMethod(route);
            Authorization attr = (Authorization)controllerMethod.GetCustomAttributes(typeof(Authorization), true)[0];

            var getAtt = controllerMethod.GetCustomAttribute(typeof(HttpGetAttribute));
            var postAtt = controllerMethod.GetCustomAttribute(typeof(HttpPostAttribute));
            var putAtt = controllerMethod.GetCustomAttribute(typeof(HttpPutAttribute));
            var deleteAtt = controllerMethod.GetCustomAttribute(typeof(HttpDeleteAttribute));
            var relAtt = (HTTPrel)controllerMethod.GetCustomAttribute(typeof(HTTPrel));

            string httpMethod = HTTPMethod.GET;

            if (getAtt != null) httpMethod = HTTPMethod.GET;
            else if (postAtt != null) httpMethod = HTTPMethod.POST;
            else if (putAtt != null) httpMethod = HTTPMethod.PUT;
            else if (deleteAtt != null) httpMethod = HTTPMethod.DELETE;

            if (hasPrivilege(user.Identity.Name, attr._elevationLevel, attr._privilege))
            {
                Links.Add(new Link(relAtt.rel, httpMethod, urlHelper.Action(route)));
                return true;
            }
            else return false;
        }

        public List<Link> GetLinks() => Links;
    }

    public class Link
    {
        public Link() { }

        public Link(string _rel, string _method, string _url)
        {
            rel = _rel;
            method = _method;
            url = _url;
        }
        public string rel;
        public string method;
        public string url;
    }
}
