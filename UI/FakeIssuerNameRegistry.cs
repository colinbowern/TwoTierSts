using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using Microsoft.IdentityModel.Tokens;

namespace UI
{
    public class FakeIssuerNameRegistry : IssuerNameRegistry
    {
        public override string GetIssuerName(SecurityToken securityToken)
        {
            var token = securityToken as X509SecurityToken;
            if (token != null)
            {
                return token.Certificate.SubjectName.Name;
            }
            return null;
        }
    }
}