using Ferrous.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using static Ferrous.Misc.Authorization;

namespace Ferrous.Misc
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
        private string controller;

        /// <summary>
        /// Set options for the LinkHelper
        /// </summary>
        /// <param name="user">User object for authorization</param>
        /// <param name="type">Type of the container of the method</param>
        /// <param name="urlHelper">Url object for Url generation</param>
        /// <returns></returns>
        public LinkHelper SetOptions(ClaimsPrincipal user, Type type, IUrlHelper urlHelper)
        {
            this.user = user;
            this.type = type;
            this.urlHelper = urlHelper;
            this.controller = type.Name.Replace("Controller", "");
            return this;
        }

        /// <summary>
        /// Adds a relative Link to the Helper without protocol and hostname
        /// </summary>
        /// <param name="route">Routing object's name, may be passed using nameof(object)</param>
        /// <param name="routeParams">Params for route. Each {param} will be replaced</param>
        /// <param name="overrideWithRel">Override rel attribute</param>
        /// <param name="noParentTemplate">Do not use parent template</param>
        /// <returns>LinkHelper</returns>
        public LinkHelper AddLinkRelative(string route, object routeParams = null, string overrideWithRel = "", bool noParentTemplate = false)
        {
            /* Get the method */
            MethodInfo controllerMethod = type.GetMethod(route);
            Authorization attr = (Authorization)controllerMethod.GetCustomAttribute(typeof(Authorization));

            /* Get the relevant attributes */
            var getAtt = (HttpGetAttribute) controllerMethod.GetCustomAttribute(typeof(HttpGetAttribute));
            var postAtt = (HttpPostAttribute) controllerMethod.GetCustomAttribute(typeof(HttpPostAttribute));
            var putAtt = (HttpPutAttribute) controllerMethod.GetCustomAttribute(typeof(HttpPutAttribute));
            var deleteAtt = (HttpDeleteAttribute) controllerMethod.GetCustomAttribute(typeof(HttpDeleteAttribute));
            var relAtt = (HTTPrel)controllerMethod.GetCustomAttribute(typeof(HTTPrel));
            
            string httpMethod = HTTPMethod.GET;

            /** Gets the routeing attribute that is being used */
            object routingAttribute = null;

            if (getAtt != null) { httpMethod = HTTPMethod.GET; routingAttribute = getAtt; }
            else if (postAtt != null) { httpMethod = HTTPMethod.POST; routingAttribute = postAtt; }
            else if (putAtt != null) { httpMethod = HTTPMethod.PUT; routingAttribute = putAtt; }
            else if (deleteAtt != null) { httpMethod = HTTPMethod.DELETE; routingAttribute = deleteAtt; }

            /* Gets the route template for the controller method 
             * that is being used to route the call
             * TODO: One method may have multiple. Throw an error for this */
            string routeTemplate = "";
            if (routingAttribute != null)
                routeTemplate = (string) routingAttribute.GetType().GetProperty(nameof(getAtt.Template)).GetValue(routingAttribute);

            if (relAtt == null && overrideWithRel == String.Empty) throw new Exception("HTTPrel attribute not set for creating link");

            /* Check for priveleges and add the link*/
            if (attr == null || hasPrivilege(user.Identity.Name, attr._elevationLevel, attr._privilege))
            {
                var cRouteAtt = (RouteAttribute)type.GetCustomAttribute(typeof(RouteAttribute));
                if (routeParams != null)
                    foreach (var propInfo in routeParams.GetType().GetProperties())
                    {
                        string prop = propInfo.GetValue(routeParams).ToString();
                        routeTemplate = routeTemplate.Replace("{" + propInfo.Name + "}", prop);
                    }
                Links.Add(new Link(
                    (overrideWithRel != String.Empty && overrideWithRel != "no") ? overrideWithRel : relAtt.rel,
                    httpMethod,
                    (!noParentTemplate?"/" + cRouteAtt.Template : String.Empty) + "/" + routeTemplate
                ));
            }
            return this;
        }

        /// <summary>
        /// Adds an absolute Link to the Helper with protocol and hostname
        /// </summary>
        /// <param name="action">Routing object's name; may be passed using nameof(object)</param>
        /// <param name="routeParams">Params for route passed to UrlHelper</param>
        /// <param name="overrideWithRel">Override rel attribute</param>
        /// <returns>LinkHelper</returns>
        public LinkHelper AddLink(string action, object routeParams = null, string overrideWithRel = "")
        {
            /* Get the method */
            MethodInfo controllerMethod = type.GetMethod(action);

            /* Get rel and auth attributes */
            var relAtt = (HTTPrel)controllerMethod.GetCustomAttribute(typeof(HTTPrel));
            if (relAtt == null && overrideWithRel == String.Empty) {
                throw new Exception("HTTPrel attribute not set for creating link");
            }

            Authorization AuthAttr = (Authorization)controllerMethod.GetCustomAttribute(typeof(Authorization));

            /* List of verb attributes */
            var Attributes = new List<(Type, string)> {
                (typeof(HttpGetAttribute), HTTPMethod.GET),
                (typeof(HttpPostAttribute), HTTPMethod.POST),
                (typeof(HttpPutAttribute), HTTPMethod.PUT),
                (typeof(HttpDeleteAttribute), HTTPMethod.DELETE)
            };
            
            /* Get the HTTP verb from the routing attribute */
            string http_verb = HTTPMethod.GET;

            foreach (var rAtt in Attributes) {
                if (controllerMethod.GetCustomAttribute(rAtt.Item1) != null) {
                    http_verb = rAtt.Item2;
                    break;
                }
            }

            /* Check for priveleges and add the link */
            if (AuthAttr == null || 
                hasPrivilege(user.Identity.Name, AuthAttr._elevationLevel, AuthAttr._privilege))
            {
                Links.Add(new Link(
                    (overrideWithRel != String.Empty) ? overrideWithRel : relAtt.rel,
                    http_verb,
                    urlHelper.Action(action, controller, routeParams, "https")
                ));
            }
            return this;
        }

        /// <summary>
        /// Adds a fully qualified URL Link to the specified content by using the specified content path.
        /// </summary>
        /// <param name="contentPath">The content path.</param>
        /// <param name="httpRel">relation to object (rel).</param>
        /// <param name="httpVerb">HTTP Verb to use</param>
        /// <returns>LinkHelper</returns>
        public LinkHelper AddAbsoluteContentLink(string contentPath, string httpRel, string httpVerb = "GET")
        {
            HttpRequest request = this.urlHelper.ActionContext.HttpContext.Request;
            string url = new Uri(new Uri("https://" + request.Host.Value), this.urlHelper.Content(contentPath)).ToString();
            Links.Add(new Link(httpRel, httpVerb, url));
            return this;
        }

        /// <summary>
        /// Adds a dummy link used to pass string content.
        /// </summary>
        /// <param name="contentPath">String to pass.</param>
        /// <param name="httpRel">relation to object (rel).</param>
        /// <param name="httpVerb">HTTP Verb to use</param>
        /// <returns>LinkHelper</returns>
        public LinkHelper AddStringLink(string content, string httpRel, string httpVerb = "GET") {
            Links.Add(new Link(httpRel, httpVerb, content));
            return this;
        }

        public List<Link> GetLinks() => Links;
    }

    /// <summary>
    /// A link with a rel, method and href
    /// </summary>
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

    /// <summary>
    /// Container for sending links with top level data
    /// </summary>
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
