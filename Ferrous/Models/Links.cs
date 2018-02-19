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
        overridden = -1,
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

        public LinkHelper SetOptions(ClaimsPrincipal user, Type type, IUrlHelper urlHelper)
        {
            this.user = user;
            this.type = type;
            this.urlHelper = urlHelper;
            return this;
        }

        public LinkHelper AddLinks(string[] routes)
        {
            foreach (var route in routes) AddLink(route);
            return this;
        }

        public LinkHelper AddLinks(Tuple<string, object>[] routes)
        {
            foreach (var route in routes) AddLink(route.Item1, route.Item2);
            return this;
        }

        public LinkHelper AddLinks(Tuple<string, object, string>[] routes)
        {
            foreach (var route in routes) AddLink(route.Item1, route.Item2, route.Item3);
            return this;
        }

        public LinkHelper AddLink(string route, object routeParams = null, string overrideWithRel = "")
        {
            MethodInfo controllerMethod = type.GetMethod(route);
            Authorization attr = (Authorization)controllerMethod.GetCustomAttributes(typeof(Authorization), true)[0];

            var getAtt = (HttpGetAttribute) controllerMethod.GetCustomAttribute(typeof(HttpGetAttribute));
            var postAtt = (HttpPostAttribute) controllerMethod.GetCustomAttribute(typeof(HttpPostAttribute));
            var putAtt = (HttpPutAttribute) controllerMethod.GetCustomAttribute(typeof(HttpPutAttribute));
            var deleteAtt = (HttpDeleteAttribute) controllerMethod.GetCustomAttribute(typeof(HttpDeleteAttribute));
            var relAtt = (HTTPrel)controllerMethod.GetCustomAttribute(typeof(HTTPrel));
            
            string httpMethod = HTTPMethod.GET;

            object routingAttribute = null;

            if (getAtt != null) { httpMethod = HTTPMethod.GET; routingAttribute = getAtt; }
            else if (postAtt != null) { httpMethod = HTTPMethod.POST; routingAttribute = postAtt; }
            else if (putAtt != null) { httpMethod = HTTPMethod.PUT; routingAttribute = putAtt; }
            else if (deleteAtt != null) { httpMethod = HTTPMethod.DELETE; routingAttribute = deleteAtt; }

            string routeTemplate = "";
            if (routingAttribute != null)
                routeTemplate = (string) routingAttribute.GetType().GetProperty(nameof(getAtt.Template)).GetValue(routingAttribute);

            if (relAtt == null) throw new Exception("HTTPrel attribute not set for creating link");

            if (hasPrivilege(user.Identity.Name, attr._elevationLevel, attr._privilege))
            {
                if (routeParams == null)
                    Links.Add(new Link(relAtt.rel, httpMethod, urlHelper.Action(route)));
                else if (overrideWithRel == String.Empty)
                    Links.Add(new Link(relAtt.rel, httpMethod, urlHelper.Action(route, routeParams)));
                else
                {
                    var cRouteAtt = (RouteAttribute)type.GetCustomAttribute(typeof(RouteAttribute));
                    foreach (var propInfo in routeParams.GetType().GetProperties())
                    {
                        string prop = propInfo.GetValue(routeParams).ToString();
                        routeTemplate = routeTemplate.Replace("{" + propInfo.Name + "}", prop);
                    }
                    Links.Add(new Link(overrideWithRel != "no" ? overrideWithRel : relAtt.rel, 
                        httpMethod, "/" + cRouteAtt.Template + "/" + routeTemplate));
                }   
            }
            return this;
        }

        public List<Link> GetLinks() => Links;
    }

    public class Link
    {
        public Link() { }

        public Link(string _rel, string _method, string _href)
        {
            rel = _rel;
            method = _method;
            href = _href;
        }
        public string rel;
        public string method;
        public string href;
    }

    public class EnumContainer {
        public List<Link> Links;
        public object Data;

        public EnumContainer(object data, List<Link>links)
        {
            this.Links = links;
            this.Data = data;
        }
    }
}
