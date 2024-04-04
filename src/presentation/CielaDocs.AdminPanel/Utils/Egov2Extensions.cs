using Schemas = ITfoxtec.Identity.Saml2.Schemas;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CielaDocs.AdminPanel.Utils
{
    public class Egov2Extensions : Schemas.Extensions
    {
        static readonly XName egovNamespaceNameX = XNamespace.Xmlns + "egovbga";
        static readonly Uri egovNamespace = new Uri("urn:bg:egov:eauth:2.0:saml:ext");
        static readonly XNamespace egovNamespaceX = XNamespace.Get(egovNamespace.OriginalString);
        private readonly IConfiguration _configuration;
      

        public Egov2Extensions(IConfiguration configuration)
        {
            _configuration = configuration;
            Element.Add(GetXContent());
        }

        protected IEnumerable<XObject> GetXContent()
        {
            yield return new XAttribute(egovNamespaceNameX, egovNamespace.OriginalString);

            yield return new XElement(egovNamespaceX + "RequestedService",
               new XElement(egovNamespaceX + "Service", _configuration.GetValue<string>("eAuth:ServiceOid")),
               new XElement(egovNamespaceX + "Provider", _configuration.GetValue<string>("eAuth:ServiceOid")),
               new XElement(egovNamespaceX + "LevelOfAssurance", _configuration.GetValue<string>("eAuth:LevelOfAssurance")));

            yield return new XElement(egovNamespaceX + "RequestedAttributes",
                GetRequestedAttribute("urn:egov:bg:eauth:2.0:attributes:latinName", true),
                GetRequestedAttribute("urn:egov:bg:eauth:2.0:attributes:birthName", true),
                GetRequestedAttribute("urn:egov:bg:eauth:2.0:attributes:email", true),
                GetRequestedAttribute("urn:egov:bg:eauth:2.0:attributes:phone", true),
                GetRequestedAttribute("urn:egov:bg:eauth:2.0:attributes:dateOfBirth", false));
        }

        private static XElement GetRequestedAttribute(string name, bool isRequired = false, string value = null)
        {
            var element = new XElement(egovNamespaceX + "RequestedAttribute",
                                 new XAttribute("Name", name),
                                 new XAttribute("NameFormat", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri"),
                                 new XAttribute("isRequired", isRequired));

            if (!string.IsNullOrWhiteSpace(value))
            {
                element.Add(new XElement(egovNamespaceX + "AttributeValue", value));
            }
            return element;
        }
    }
}

