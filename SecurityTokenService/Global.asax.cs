using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentSecurity;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.Practices.ServiceLocation;
using SecurityTokenService.Controllers;
using SecurityTokenService.ModelBinders;

namespace SecurityTokenService
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            ModelBinderProviders.BinderProviders.Add(new ConventionModelBinderProvider());
            RegisterSecurityPolicy();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private static void RegisterSecurityPolicy()
        {
            SecurityConfigurator.Configure(configuration =>
            {
                configuration.ResolveServicesUsing(type => ServiceLocator.Current.GetAllInstances(type));
                configuration.GetAuthenticationStatusFrom(() => HttpContext.Current.User.Identity.IsAuthenticated);
                configuration.ForAllControllers().DenyAnonymousAccess();
                configuration.For<AccountController>().Ignore();
                configuration.For<FederationMetadataController>(x => x.Generate()).Ignore();
            });
        }

        #region Security Policy Violation Handlers
        public class DenyAnonymousAccessPolicyViolationHandler : IPolicyViolationHandler
        {
            public ActionResult Handle(PolicyViolationException exception)
            {
                var result = new HttpStatusCodeResult((int) HttpStatusCode.Unauthorized) as ActionResult;
                var noRedirect = HttpContext.Current.Items["NoRedirect"];
                if (noRedirect == null)
                {
                    result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "SignIn",
                        returnUrl = HttpContext.Current.Request.RawUrl
                    }));
                }
                return result;
            }
        }
        #endregion

        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleSecurityAttribute(), 0);
            filters.Add(new HandleErrorAttribute());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "FederationMetadata",
                "FederationMetadata/2007-06/FederationMetadata.xml",
                new { controller = "FederationMetadata", action = "Generate" });

            routes.MapRoute(
                "WSFederationSignIn",
                "Issue/WSFederation",
                new { controller = "WSFederation", action = "SignIn" },
                new { wa = new QueryStringActionConstraint(WSFederationConstants.Actions.SignIn) });

            routes.MapRoute(
                "WSFederationSignOut",
                "Issue/WSFederation",
                new { controller = "WSFederation", action = "SignOut" },
                new { wa = new QueryStringActionConstraint(WSFederationConstants.Actions.SignOut) });

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new { controller = ".*$(?<!Issue)$" }
            );

            routes.Add(new ServiceRoute(
                "Issue/WSTrust",
                    new CustomSecurityTokenServiceHostFactory(),
                    typeof(CustomSecurityTokenServiceConfiguration)));
        }

        #region Routing Constraints
        class QueryStringActionConstraint : IRouteConstraint
        {
            private readonly string[] values;

            public QueryStringActionConstraint(params string[] values)
            {
                this.values = values ?? new string[0];
            }

            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                var result =
                    routeDirection == RouteDirection.IncomingRequest &&
                    httpContext.Request.QueryString.AllKeys.Contains(parameterName) &&
                    this.values.Any(x => String.Equals(x, httpContext.Request.QueryString[parameterName], StringComparison.OrdinalIgnoreCase));
                return result;
            }
        }
        #endregion
    }
}