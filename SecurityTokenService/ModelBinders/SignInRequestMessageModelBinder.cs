using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using Microsoft.IdentityModel.Protocols.WSFederation;

namespace SecurityTokenService.ModelBinders
{
    public class SignInRequestMessageModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            Condition.Requires(controllerContext).IsNotNull();
            Condition.Requires(bindingContext).IsNotNull();

            WSFederationMessage message = null;
            if (WSFederationMessage.TryCreateFromUri(controllerContext.HttpContext.Request.Url, out message) &&
                message is SignInRequestMessage)
            {
                return message;
            }
            return null;
        }
    }
}