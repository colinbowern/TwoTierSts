using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Web;
using Thinktecture.IdentityModel.Claims;

namespace SecurityTokenService.Controllers
{
    /// <remarks>
    /// Relying Party (RP) applications will initially direct themselves to
    /// the issuing endpoint (i.e. issue/wsfederation) which will be redirected
    /// to thie account controller for authentication if the user does not have
    /// an STS session already.
    /// </remarks>
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(string returnUrl)
        {
            // Build claims collection based on valid username
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "Chuck Norris"),
                        new Claim(ClaimTypes.Email, "chuck.norris@contoso.com"),
                        new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.Password),
                        AuthenticationInstantClaim.Now
                    };

            // Create and authenticate principal
            var claimsPrincipal = ClaimsPrincipal.CreateFromIdentity(new ClaimsIdentity(claims));
            var principal = FederatedAuthentication.ServiceConfiguration.ClaimsAuthenticationManager.Authenticate(string.Empty, claimsPrincipal);
            var transformedPrincipal = FederatedAuthentication.ServiceConfiguration.ClaimsAuthenticationManager.Authenticate(HttpContext.Request.Url.AbsoluteUri, principal);

            // Create session token
            var sessionToken = new SessionSecurityToken(transformedPrincipal, Configuration.PersistentSessionLength)
            {
                IsPersistent = false
            };
            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionToken);

            // Redirect
            var result = !String.IsNullOrWhiteSpace(returnUrl) ?
                Redirect(returnUrl) :
                Redirect(Configuration.DefaultApplicationUrl);

            return result;
        }

        public ActionResult SignOut()
        {
            if (Request.IsAuthenticated)
            {
                FederatedAuthentication.SessionAuthenticationModule.DeleteSessionTokenCookie();
            }
            return View();
        }
    }
}
