using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IdentityModel.Tokens;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Xml;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Protocols.WSFederation.Metadata;
using Microsoft.IdentityModel.Protocols.WSIdentity;
using Microsoft.IdentityModel.SecurityTokenService;
using SecurityTokenTypes = Microsoft.IdentityModel.Tokens.SecurityTokenTypes;

namespace SecurityTokenService.Controllers
{
    public class FederationMetadataController : Controller
    {
        [OutputCache(Duration = 3600, VaryByParam = "none", Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult Generate()
        {
            var wsFederationEndpoint = new EndpointAddress(Request.ToApplicationUri() + "Issue/WSFederation");
            var wsTrustEndpoint = new EndpointAddress(new Uri(Request.ToApplicationUri() + "Issue/WSTrust/Message/Windows"), null, null, CreateMetadataReader(new Uri(Request.ToApplicationUri() + "Issue/WSTrust/mex")), null);
            var metadata = new EntityDescriptor
            {
                EntityId = new EntityId(Request.ToApplicationUri()),
                RoleDescriptors =
                {
                    new SecurityTokenServiceDescriptor {
                        ServiceDisplayName = Configuration.IssuerName,
                        Contacts = {
                            new ContactPerson {
                                 Type = ContactType.Support,
                                 Company = "Contoso Bank",
                                 EmailAddresses = { "help@contoso.com" },
                                 TelephoneNumbers = { "+1 (416) 555-1212" }
                            }
                        },
                        ClaimTypesOffered = 
                        {
                            DisplayClaim.CreateDisplayClaimFromClaimType(ClaimTypes.Name),
                            DisplayClaim.CreateDisplayClaimFromClaimType(ClaimTypes.Email),
                        },
                        ProtocolsSupported = { new Uri(WSFederationMetadataConstants.Namespace) },
                        TokenTypesOffered = { new Uri(SecurityTokenTypes.OasisWssSaml11TokenProfile11), new Uri(SecurityTokenTypes.OasisWssSaml2TokenProfile11) },
                        PassiveRequestorEndpoints = { wsFederationEndpoint },
                        SecurityTokenServiceEndpoints = { wsTrustEndpoint },
                        Keys = { new KeyDescriptor(
                                    new SecurityKeyIdentifier(
                                        new X509SecurityToken(Configuration.TokenSigningCertificate).CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>())) { Use = KeyType.Signing } }
                    },
                },
                SigningCredentials = new X509SigningCredentials(Configuration.TokenSigningCertificate)
            };

            using (var stream = new MemoryStream())
            using (var streamReader = new StreamReader(stream))
            {
                var serializer = new MetadataSerializer();
                serializer.WriteMetadata(stream, metadata);
                stream.Position = 0;
                var result = new ContentResult
                {
                    Content = streamReader.ReadToEnd(),
                    ContentEncoding = Encoding.Unicode,
                    ContentType = System.Net.Mime.MediaTypeNames.Text.Xml
                };
                return result;
            }
        }

        private static XmlDictionaryReader CreateMetadataReader(Uri mexAddress)
        {
            var metadataSet = new MetadataSet();
            var metadataReference = new MetadataReference(new EndpointAddress(mexAddress), AddressingVersion.WSAddressing10);
            var metadataSection = new MetadataSection(MetadataSection.MetadataExchangeDialect, null, metadataReference);
            metadataSet.MetadataSections.Add(metadataSection);

            var stringBuilder = new StringBuilder();
            using(var stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
            {
                var xmlWriter = XmlWriter.Create(stringWriter);
                metadataSet.WriteTo(xmlWriter);
                xmlWriter.Flush();
                stringWriter.Flush();

                var stringReader = new StringReader(stringBuilder.ToString());
                var xmlTextReader = new XmlTextReader(stringReader);
                var result = XmlDictionaryReader.CreateDictionaryReader(xmlTextReader);
                return result;
            }
        }
    }
}
