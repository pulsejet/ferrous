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

        private void setOptions(ClaimsPrincipal user, Type type, IUrlHelper urlHelper)
        {
            this.user = user;
            this.type = type;
            this.urlHelper = urlHelper;
        }

        public LinkHelper(ClaimsPrincipal user, Type type, IUrlHelper urlHelper)
        {
            setOptions(user, type, urlHelper);
        }

        public LinkHelper(ClaimsPrincipal user, Type type, IUrlHelper urlHelper, string[] routes)
        {
            setOptions(user, type, urlHelper);
            foreach (var route in routes) AddLink(route);
        }

        public LinkHelper(ClaimsPrincipal user, Type type, IUrlHelper urlHelper, Tuple<string, object>[] routes)
        {
            setOptions(user, type, urlHelper);
            foreach (var route in routes) AddLink(route.Item1, route.Item2);
        }

        public bool AddLink(string route, object routeParams = null)
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

            if (relAtt == null) throw new Exception("HTTPrel attribute not set for creating link");

            if (hasPrivilege(user.Identity.Name, attr._elevationLevel, attr._privilege))
            {
                if (routeParams == null)
                    Links.Add(new Link(relAtt.rel, httpMethod, urlHelper.Action(route)));
                else
                    Links.Add(new Link(relAtt.rel, httpMethod, urlHelper.Action(route, routeParams)));
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
