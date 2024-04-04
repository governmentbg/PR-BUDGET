using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace CielaDocs.SjcWeb.Controllers
{
    [AllowAnonymous]
    [Route("Metadata")]
    public class MetadataController : Controller
    {
        private readonly Saml2Configuration config;

        public MetadataController(Saml2Configuration config)
        {
            this.config = config;
        }

        public IActionResult Index()
        {
            var defaultSite = new Uri($"{Request.Scheme}://{Request.Host.ToUriComponent()}/");

            var entityDescriptor = new EntityDescriptor(config);
            entityDescriptor.ValidUntil = 365;
            entityDescriptor.SPSsoDescriptor = new SPSsoDescriptor
            {
                AuthnRequestsSigned = config.SignAuthnRequest,
                WantAssertionsSigned = true,

                SigningCertificates = new X509Certificate2[]
                {
                    config.SigningCertificate
                },
                //EncryptionCertificates = new X509Certificate2[]
                //{
                //    config.DecryptionCertificate
                //},
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
                    new AttributeConsumingService { ServiceName = new ServiceName("Some SP", "en"), RequestedAttributes = CreateRequestedAttributes() }
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
            yield return new RequestedAttribute("urn:oid:2.16.100.1.1.7");
            yield return new RequestedAttribute("urn:oid:2.16.100.1.1.7.1.11.1.1", false);
        }
    }
}