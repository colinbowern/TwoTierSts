using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.IdentityModel.Claims;

namespace Services
{
    public class EchoClaims : IEchoClaims
    {
        public string Echo()
        {
            var principal = (IClaimsPrincipal)Thread.CurrentPrincipal;
            var identity = (IClaimsIdentity)principal.Identity;
            var sb = new StringBuilder();

            // Print details about the caller's identity.
            sb.AppendFormat("<div><p>Caller's identity name: {0}</p>\n",
                identity.Name != null ? identity.Name : "Identity does not have a name");
            sb.Append("Caller's claims:<br>\n");
            PrintClaimsTable(sb, identity);

            // Print all the actors associated with the caller's identity.
            identity = identity.Actor;
            while (identity != null)
            {
                sb.AppendFormat("<p>Calling via identity: {0}</p>\n",
                    identity.Name != null ? identity.Name : "Identity does not have a name");
                sb.Append("With claims:<br>\n");
                PrintClaimsTable(sb, identity);
                identity = identity.Actor;
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private static void PrintClaimsTable(StringBuilder sb, IClaimsIdentity identity)
        {
            sb.Append("<table style='width:100%;'><tr><th>Claim Type</th>" +
                "<th>Claim Value</th><th>Claim Issuer</th></tr>\n");
            foreach (Claim claim in identity.Claims)
            {
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>\n",
                    claim.ClaimType, claim.Value, claim.Issuer);
            }
            sb.Append("</table>\n");
        }
    }
}
