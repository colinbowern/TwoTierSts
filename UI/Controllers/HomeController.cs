using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Web.Mvc;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.Web;

namespace UI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DoStuff()
        {
            var channelFactory = new ChannelFactory<Services.IEchoClaimsChannel>("WS2007FederationHttpBinding_IEchoClaims");
            channelFactory.ConfigureChannelFactory();
            channelFactory.Credentials.SupportInteractive = false;
            var claimsPrincipal = Thread.CurrentPrincipal as IClaimsPrincipal;
            var channel = channelFactory.CreateChannelActingAs(claimsPrincipal.Identities.First().BootstrapToken);
            var success = false;
            try
            {
                var result = channel.Echo();
                if (channel.State != CommunicationState.Faulted)
                {
                    channel.Close();
                    success = true;
                }
                return View(model: result);
            }
            finally
            {
                if (!success)
                {
                    channel.Abort();
                }
            }
        }

        public ActionResult SignOut()
        {
            FederatedAuthentication.WSFederationAuthenticationModule.SignOut(false);

            var signOutRequest = new SignOutRequestMessage(new Uri(FederatedAuthentication.WSFederationAuthenticationModule.Issuer), FederatedAuthentication.WSFederationAuthenticationModule.Realm);
            return new RedirectResult(signOutRequest.WriteQueryString());
        }
    }
}
