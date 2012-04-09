using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.IdentityModel.Protocols.WSFederation;

namespace SecurityTokenService.ActionResults
{
    public class WSFederationResult : ContentResult
    {
        public WSFederationResult(WSFederationMessage message)
        {
            Content = message.WriteFormPost();
        }
    }
}