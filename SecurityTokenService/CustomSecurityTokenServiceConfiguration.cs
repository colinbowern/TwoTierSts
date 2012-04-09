using System;
using System.IdentityModel.Selectors;
using System.Linq;
using System.ServiceModel.Security;
using Microsoft.IdentityModel.Configuration;
using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml11;
using Microsoft.IdentityModel.Tokens.Saml2;

namespace SecurityTokenService
{
    public class CustomSecurityTokenServiceConfiguration : SecurityTokenServiceConfiguration
    {
        public CustomSecurityTokenServiceConfiguration()
        {
            AudienceRestriction.AudienceMode = AudienceUriMode.Never;
            CertificateValidationMode = X509CertificateValidationMode.None;
            IssuerNameRegistry = new FakeIssuerNameRegistry();
            SecurityTokenService = typeof(CustomSecurityTokenService);
            DefaultTokenLifetime = Configuration.PersistentSessionLength;
            MaximumTokenLifetime = Configuration.PersistentSessionLength;
            TokenIssuerName = Configuration.IssuerName;
            SigningCredentials = new X509SigningCredentials(Configuration.TokenSigningCertificate);

            var actAsHandlers = new SecurityTokenHandlerCollection(new SecurityTokenHandler[] { new Saml11SecurityTokenHandler(), new Saml2SecurityTokenHandler() });
            actAsHandlers.Configuration.AudienceRestriction.AudienceMode = AudienceUriMode.Never;
            actAsHandlers.Configuration.CertificateValidator = X509CertificateValidator.None;
            actAsHandlers.Configuration.IssuerNameRegistry = new FakeIssuerNameRegistry();
            SecurityTokenHandlerCollectionManager[SecurityTokenHandlerCollectionManager.Usage.ActAs] = actAsHandlers;
        }
    }
}
