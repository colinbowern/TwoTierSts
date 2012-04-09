using System;
using System.Linq;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Configuration;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.SecurityTokenService;

namespace SecurityTokenService
{
    public class CustomSecurityTokenService : Microsoft.IdentityModel.SecurityTokenService.SecurityTokenService
    {
        public CustomSecurityTokenService(SecurityTokenServiceConfiguration configuration) : base(configuration) { }

        protected override Scope GetScope(IClaimsPrincipal principal, RequestSecurityToken request)
        {
            // TODO: Analyze and validate request details against policy
            // - Validate the request.AppliesTo URL is one of the accepted applications and if not thorw an new InvalidRequestException(String.Format("The 'appliesTo' address '{0}' is not valid.", request.AppliesTo.Uri.OriginalString));

            var result = new Scope(request.AppliesTo.Uri.OriginalString, SecurityTokenServiceConfiguration.SigningCredentials)
                            {
                                SymmetricKeyEncryptionRequired = false,
                                TokenEncryptionRequired = false
                            };
            result.ReplyToAddress = result.AppliesToAddress;
            return result;
        }

        protected override IClaimsIdentity GetOutputClaimsIdentity(IClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
        {
            var callerIdentity = (IClaimsIdentity)principal.Identity;

            // Create new identity and copy content of the caller's identity into it (including the existing delegate chain)
            var result = new ClaimsIdentity();
            CopyClaims(callerIdentity, result);

            // If there is an ActAs token in the RST, add and return the claims from it as the top-most identity
            // and put the caller's identity into the Delegate property of this identity.
            if (request.ActAs != null)
            {
                var actAsIdentity = new ClaimsIdentity();
                var actAsSubject = request.ActAs.GetSubject()[0];
                CopyClaims(actAsSubject, actAsIdentity);

                // Find the last delegate in the actAs identity
                var lastActingVia = actAsIdentity as IClaimsIdentity;
                while (lastActingVia.Actor != null)
                {
                    lastActingVia = lastActingVia.Actor;
                }

                // Put the caller's identity as the last delegate to the ActAs identity
                lastActingVia.Actor = result;

                // Return the actAsIdentity instead of the caller's identity in this case
                result = actAsIdentity;
            }

            return result;
        }

        private static void CopyClaims(IClaimsIdentity source, IClaimsIdentity target)
        {
            foreach (var claim in source.Claims)
            {
                // We don't copy the issuer because it is not needed in this case. The STS always issues claims
                // using its own identity.
                var newClaim = new Claim(claim.ClaimType, claim.Value, claim.ValueType);

                // copy all claim properties
                foreach (var key in claim.Properties.Keys)
                {
                    newClaim.Properties.Add(key, claim.Properties[key]);
                }

                // add claim to the destination identity
                target.Claims.Add(newClaim);
            }

            // Recursively copy claims from the source identity delegates
            if (source.Actor != null)
            {
                target.Actor = new ClaimsIdentity();
                CopyClaims(source.Actor, target.Actor);
            }
        }
    }
}
