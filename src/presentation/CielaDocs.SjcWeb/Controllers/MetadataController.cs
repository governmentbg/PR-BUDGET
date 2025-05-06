using CielaDocs.SjcWeb.Models;

using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.TermStore;

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace CielaDocs.SjcWeb.Controllers
{
    [AllowAnonymous]
    [Route("Metadata")]
    public class MetadataController : Controller
    {
        private readonly Saml2Configuration _saml2Config;
        private readonly IConfiguration _conf;

        public MetadataController(Saml2Configuration config, IConfiguration conf)
        {
            this._saml2Config = config;
            this._conf=conf;
        }

        public IActionResult Index()
        {
            var defaultSite = new Uri($"{Request.Scheme}://{Request.Host.ToUriComponent()}/");
            var thumbprint = _conf.GetValue<string>("Saml2:SigningCertificateThumbprint");
            var samlCert = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            samlCert.Open(OpenFlags.ReadOnly);
            var cert = samlCert.Certificates
                .Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false)[0];

            var entityDescriptor = new EntityDescriptor(_saml2Config);
            entityDescriptor.ValidUntil = 365;

            entityDescriptor.SPSsoDescriptor = new SPSsoDescriptor
            {
                AuthnRequestsSigned = _saml2Config.SignAuthnRequest,
                WantAssertionsSigned = true,

                SigningCertificates = new X509Certificate2[]
                {
                   // _saml2Config.SigningCertificate
                   cert
                },
                EncryptionCertificates = new X509Certificate2[]
                {
                   // _saml2Config.SigningCertificate
                   cert
                },
                SingleLogoutServices = new SingleLogoutService[]
                {
                    new SingleLogoutService { Binding = ProtocolBindings.HttpPost, Location = new Uri(defaultSite, "Auth/SingleLogout"), ResponseLocation = new Uri(defaultSite, "Auth/LoggedOut") }
                },
                NameIDFormats = new Uri[] { NameIdentifierFormats.X509SubjectName },
                AssertionConsumerServices = new AssertionConsumerService[]
                {
                    new AssertionConsumerService { Binding = ProtocolBindings.HttpPost, Location = new Uri(defaultSite, "Auth/AssertionConsumerService") },
                },
                AttributeConsumingServices = new AttributeConsumingService[]
                {
                  new AttributeConsumingService { ServiceNames = new[] { new LocalizedNameType("Some SP", "en") }, // Target-typed new expression
                        RequestedAttributes = CreateRequestedAttributes()
                  }
                },
            };
            entityDescriptor.ContactPersons = new[] {
                new ContactPerson(ContactTypes.Administrative)
                {
                    Company = "Ciela AD",
                    GivenName = "Atanas",
                    SurName = "Tinkin",
                    EmailAddress = "a.tinkin@ciela.com",
                    TelephoneNumber = "0894747007",
                },
            };
            return new Saml2Metadata(entityDescriptor).CreateMetadata().ToActionResult();
        }

        private IEnumerable<RequestedAttribute> CreateRequestedAttributes()
        {
            var configSection = _conf.GetSection("eAuth:RequestedAttributes");
            var rawList = configSection.Get<List<RequestedAttributeDto>>();

            return rawList?.Select(x => new RequestedAttribute(x.Name, x.IsRequired, x.NameFormat)).ToList()
                ?? Enumerable.Empty<RequestedAttribute>();
        }
    }

}