using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace UI
{
    public class ComputerCertificateValidator : X509CertificateValidator
    {
        private readonly X509CertificateValidator chain;

        public ComputerCertificateValidator()
        {
            chain = ChainTrust;
        }

        public ComputerCertificateValidator(bool useMachineContext, X509ChainPolicy chainPolicy)
        {
            chain = CreateChainTrustValidator(useMachineContext, chainPolicy);
        }

        public override void Validate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }

            Exception exception;
            if (!TryValidate(certificate, out exception))
            {
                try
                {
                    chain.Validate(certificate);
                }
                catch (SecurityTokenValidationException exception2)
                {
                    throw new SecurityTokenValidationException((exception.Message + " " + exception2.Message).Trim());
                }
            }
        }

        private bool TryValidate(X509Certificate2 certificate, out Exception exception)
        {
            var now = DateTime.Now;
            if ((now > certificate.NotAfter) || (now < certificate.NotBefore))
            {
                exception =
                    new SecurityTokenValidationException(
                        String.Format("The X.509 certificate ({0}) usage time is invalid.  The usage time '{1}' does not fall between NotBefore time '{2}' and NotAfter time '{3}'.",
                            !String.IsNullOrWhiteSpace(certificate.SubjectName.Name) ? certificate.SubjectName.Name : certificate.Thumbprint,
                            now,
                            certificate.NotBefore,
                            certificate.NotAfter));
                return false;
            }
            if (!StoreContainsCertificate(StoreName.My, certificate) &&
                !StoreContainsCertificate(StoreName.TrustedPeople, certificate) &&
                !StoreContainsCertificate(StoreName.TrustedPublisher, certificate) &&
                !StoreContainsCertificate("Enterprise Trust", certificate))
            {
                exception =
                    new SecurityTokenValidationException(
                        String.Format("The X.509 certificate {0} is not in the personal, trusted people, publisher or enterprise trust stores",
                            !String.IsNullOrWhiteSpace(certificate.SubjectName.Name) ? certificate.SubjectName.Name : certificate.Thumbprint));
                return false;
            }
            if (StoreContainsCertificate(StoreName.Disallowed, certificate))
            {
                exception =
                    new SecurityTokenValidationException(
                        String.Format("The X.509 certificate {0} is in an untrusted certificate store",
                            !String.IsNullOrWhiteSpace(certificate.SubjectName.Name) ? certificate.SubjectName.Name : certificate.Thumbprint));
                return false;
            }
            exception = null;
            return true;
        }

        private static bool StoreContainsCertificate(StoreName storeName, X509Certificate2 certificate)
        {
            var store = new X509Store(storeName, StoreLocation.LocalMachine);
            
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var result =
                    store.Certificates.OfType<X509Certificate2>().Any(x => String.Equals(x.Thumbprint, certificate.Thumbprint, StringComparison.OrdinalIgnoreCase));
                return result;
            }
            finally
            {
                store.Close();
            }
        }

        private static bool StoreContainsCertificate(string storeName, X509Certificate2 certificate)
        {
            var store = new X509Store(storeName, StoreLocation.LocalMachine);
            
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var result =
                    store.Certificates.OfType<X509Certificate2>().Any(x => x.GetCertHash() == certificate.GetCertHash());
                return result;
            }
            finally
            {
                store.Close();
            }
        }
    }
}