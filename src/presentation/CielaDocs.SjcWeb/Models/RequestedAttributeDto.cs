namespace CielaDocs.SjcWeb.Models
{
   public class RequestedAttributeDto
    {
        public string Name { get; set; }
        public bool IsRequired { get; set; } = true;
        public string NameFormat { get; set; } = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic";
    }
}
