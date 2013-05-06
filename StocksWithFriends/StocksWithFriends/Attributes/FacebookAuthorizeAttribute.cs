using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace StocksWithFriends.Attributes
{
    public class FacebookAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (System.Web.HttpContext.Current.Session["userId"] != null)
                return true;
            else
                return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (System.Web.HttpContext.Current.Session["userId"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                                new RouteValueDictionary 
                        {
                            { "action", "Welcome" },
                            { "controller", "Home" }
                        });
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}