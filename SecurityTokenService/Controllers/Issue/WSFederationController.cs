using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Web;
using SecurityTokenService.ActionResults;

namespace SecurityTokenService.Controllers.Issue
{
    public class WSFederationController : Controller
    {
        public ActionResult SignIn(SignInRequestMessage message)
        {
            var response = FederatedPassiveSecurityTokenServiceOperations.ProcessSignInRequest(
                                message,
                                HttpContext.User,
                                new CustomSecurityTokenService(new CustomSecurityTokenServiceConfiguration()));
            return new WSFederationResult(response);
        }

        public ActionResult SignOut(SignOutRequestMessage message)
        {
            FederatedAuthentication.SessionAuthenticationModule.SignOut();
            return RedirectToAction("SignOut", "Account");
        }
    }
}
