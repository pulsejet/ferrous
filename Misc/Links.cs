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
        /// Adds an absolute Link to the Helper with protocol and hostname
        /// </summary>
        /// <param name="action">Routing object's name; may be passed using nameof(object)</param>
        /// <param name="routeParams">Params for route passed to UrlHelper</param>
        /// <param name="overrideWithRel">Override rel attribute</param>
        /// <param name="hasQueryParams">Set to true to include query params in href</param>
        /// <returns>LinkHelper</returns>
        public LinkHelper AddLink(string action, object routeParams = null, string overrideWithRel = "", bool hasQueryParams = false)
        {
            /* Get the method */
            MethodInfo controllerMethod = type.GetMethod(action);

            /* Check privileges */
            Authorization AuthAttr = (Authorization)controllerMethod.GetCustomAttribute(typeof(Authorization));
            if (AuthAttr != null && 
                !hasPrivilege(user.Identity.Name, AuthAttr._elevationLevel, AuthAttr._privilege)) {
                return this;
            }

            /* Get rel and auth attributes */
            var relAtt = (HTTPrel)controllerMethod.GetCustomAttribute(typeof(HTTPrel));
            if (relAtt == null && overrideWithRel == String.Empty) {
                throw new Exception("HTTPrel attribute not set for creating link");
            }   

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

            /* Add query parameters */
            string url_parameters = String.Empty;
            if (hasQueryParams && controllerMethod.GetParameters().Count() > 0) {
                url_parameters = "{?";
                foreach (var param in controllerMethod.GetParameters()){
                   if (param.GetCustomAttribute(typeof(FromQueryAttribute)) != null) {
                       url_parameters += param.Name + ",";
                   }
                }
                url_parameters = url_parameters.TrimEnd(',');
                url_parameters += "}";
            }

            /* Add the link */
            Links.Add(new Link(
                (overrideWithRel != String.Empty) ? overrideWithRel : relAtt.rel,
                http_verb,
                urlHelper.Action(action, controller, routeParams, "https") + url_parameters
            ));
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
