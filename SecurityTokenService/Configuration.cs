using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Hosting;

namespace SecurityTokenService
{
    public static class Configuration
    {
        public static readonly TimeSpan PersistentSessionLength = TimeSpan.FromDays(14);
        public static readonly string IssuerName = "Security Token Service";
        public static readonly X509Certificate2 TokenSigningCertificate = 
            new X509Certificate2(HostingEnvironment.MapPath("~/App_Data/SecurityTokenService.pfx"));
        public static readonly string DefaultApplicationUrl = "http://localhost:51000";

    }
}