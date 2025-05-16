using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.MvcCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CielaDocs.SjcWeb.Utils;
using System.Security.Authentication;
using System.Security.Claims;
using System.Xml;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.SjcWeb.Models;
using Microsoft.AspNetCore.Http;
using CielaDocs.Application.Models;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.IO.Compression;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Xml.Serialization;
using System.ServiceModel.Channels;
using Newtonsoft.Json;
using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Helpers;

namespace CielaDocs.SjcWeb.Controllers
{
    [AllowAnonymous]
    [Route("Auth")]
    public class AuthController : Controller
    {
        const string relayStateReturnUrl = "returnUrl";
        private readonly Saml2Configuration config;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

      
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        public AuthController(Saml2Configuration config, SignInManager<IdentityUser> signInManager,
               UserManager<IdentityUser> userManager,
               IHttpContextAccessor httpContextAccessor,  ISjcBudgetRepository sjcRepo, IConfiguration configuration, ILogger<AuthController> logger)
        {
            this.config = config;
            _userManager = userManager;
            _signInManager = signInManager;
          
            _httpContextAccessor = httpContextAccessor;
            _sjcRepo = sjcRepo;
            _configuration = configuration;
            _logger = logger;
        }
       
        [Route("Login")]
        public IActionResult Login(string returnUrl = null)
        {
            var thumbprint = _configuration.GetValue<string>("Saml2:SigningCertificateThumbprint");
            var samlCert = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            samlCert.Open(OpenFlags.ReadOnly);
            var cert = samlCert.Certificates
                .Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false)[0];
            config.SigningCertificate = cert;
             config.SignatureAlgorithm = Saml2SecurityAlgorithms.RsaSha256Signature;
            //config.SignatureAlgorithm = Saml2SecurityAlgorithms.RsaSha1Signature;

           // var binding = new Saml2PostBinding();//added signature to auth request but "egov eAuth' throw error 
           var binding = new Saml2RedirectBinding();//do not added signature

            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });

            var ret = binding.Bind(new Saml2AuthnRequest(config)
            {
                Subject = new Subject { NameID = new NameID { ID = "abcd" } },
                ForceAuthn = true,
                IsPassive = false,
                ProtocolBinding = new Uri("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"),
                AssertionConsumerServiceUrl = new Uri(_configuration.GetValue<string>("Saml2:AssertionConsumerServiceUrl")),
                IssueInstant = DateTime.Now,
                NameIdPolicy = new NameIdPolicy { AllowCreate = true, Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent" },
                Extensions = new Egov2Extensions(_configuration),
                RequestedAuthnContext = new RequestedAuthnContext
                {
                    Comparison = AuthnContextComparisonTypes.Minimum,
                    AuthnContextClassRef = new string[] { AuthnContextClassTypes.PasswordProtectedTransport.OriginalString },
                },


            });
          //  var z = ret.ToActionResult();
            return ret.ToActionResult();



        }
        [Route("LoginKep")]
        public IActionResult LoginKep(string returnUrl = null)
        {

            var samlLoginUrl = _configuration.GetValue<string>("RemoteAuth:RemoteAuthUrl"); // remote SAML entry point
            var loginCallbackUrl= _configuration.GetValue<string>("RemoteAuth:LoginCallbackUrl"); // remote SAML entry point
            var returnUrls = Url.Action(loginCallbackUrl, "Auth", null, Request.Scheme); //  handler endpoint in .NET Core
            var fullRedirect = $"{samlLoginUrl}?returnUrl={Uri.EscapeDataString(returnUrls)}";

            return Redirect(fullRedirect);
        }
        public string SignXml(XmlDocument xmlDoc, X509Certificate2 cert)
        {
            // Step 1: Create a SignedXml object
            var signedXml = new SignedXml(xmlDoc)
            {
                SigningKey = cert.GetRSAPrivateKey() // Use the private key from the certificate
            };

            // Step 2: Create a reference to the document
            var reference = new Reference
            {
                Uri = "" // An empty URI means we're signing the entire document
            };

            // Add a transform to ensure the canonicalization
            reference.AddTransform(new XmlDsigExcC14NTransform()); // Exclusive C14N

            // Add the reference to the SignedXml object
            signedXml.AddReference(reference);

            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;
            // Step 3: Create the XML signature
            signedXml.ComputeSignature();

            // Step 4: Get the XML signature (this is a <Signature> element)
            XmlElement signatureElement = signedXml.GetXml();

            // Step 5: Append the signature to the original XML document
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(signatureElement, true));

            // Return the signed XML as a string
            return xmlDoc.OuterXml;
        }
        private static void SignXml2(XmlDocument xmlDoc, X509Certificate2 cert)
        {
            var signedXml = new SignedXml(xmlDoc)
            {
                SigningKey = cert.GetRSAPrivateKey()
            };

            var reference = new Reference();
            reference.Uri = "";

            //var env = new XmlDsigEnvelopedSignatureTransform();
            //reference.AddTransform(env);
            reference.AddTransform(new XmlDsigExcC14NTransform());

            signedXml.AddReference(reference);
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();

            var xmlDigitalSignature = signedXml.GetXml();
            xmlDoc.DocumentElement?.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        }
        public static string SignString(string input, X509Certificate2 cert, string sigAlg)
        {
            var csp = cert.GetRSAPrivateKey();
            byte[] data = Encoding.UTF8.GetBytes(input);
            byte[] signature = csp.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }
        public static string SignRequest(string samlRequest, string sigAlg, X509Certificate2 cert)
        {
            var rsa = cert.GetRSAPrivateKey();

            if (rsa == null)
                throw new InvalidOperationException("Certificate does not have a private key.");

            byte[] data = Encoding.UTF8.GetBytes(samlRequest);

            // Determine algorithm based on sigAlg
            HashAlgorithmName hashAlgorithm = sigAlg switch
            {
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256" => HashAlgorithmName.SHA256,
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384" => HashAlgorithmName.SHA384,
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512" => HashAlgorithmName.SHA512,
                _ => throw new NotSupportedException($"Unsupported signature algorithm: {sigAlg}")
            };

            byte[] signedBytes = rsa.SignData(data, hashAlgorithm, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signedBytes);
        }
        public static string DeflateBase64(string input)
        {
            using (var memoryStream = new MemoryStream())
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
            using (var writer = new StreamWriter(deflateStream))
            {
                writer.Write(input);
                writer.Close();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
        [Route("AssertionConsumerService")]
        public async Task<IActionResult> AssertionConsumerService()
        {

            _logger.LogInformation("SAML AssertionConsumerService in");
            // var binding = new Saml2PostBinding();
          
                var binding = new Saml2PostBinding();
                var saml2AuthnResponse = new Saml2AuthnResponse(config);
            try
            {
                binding.ReadSamlResponse(Request.ToGenericHttpRequest(), saml2AuthnResponse);
                if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
                {
                    _logger.LogError($"SAML Response status: {saml2AuthnResponse.Status}");
                    throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");
                }
                binding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            }
            catch (Exception ex) {
                _logger.LogError($"SAML AssertionConsumerService error {ex?.Message}");
               // do not throw exeption when response signature is invalid throw new AuthenticationException($"SAML AssertionConsumerService error {ex?.Message}");
            }
            ClaimsIdentity identity = saml2AuthnResponse.ClaimsIdentity;
            if (identity != null)
            {
                var personalIdentifier = GetClaimValue(identity, "urn:egov:bg:eauth:2.0:attributes:personIdentifier");
                if (!string.IsNullOrWhiteSpace(personalIdentifier))
                {
                    personalIdentifier = personalIdentifier.Split(new char[] { '-', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
                }
                if (string.IsNullOrWhiteSpace(personalIdentifier))
                {
                    return RedirectToAction(nameof(UserNotFound));
                }
                try
                {
                    var dbuser = await _sjcRepo.GetUserByIdentifierAsync(personalIdentifier);
                    if (dbuser == null)
                    {
                        _logger.LogError("UserNotFound");
                        return RedirectToAction("UserNotFound", "Auth");
                    }
                    else
                    {
                        List<Claim> claims = new List<Claim>();

                        claims.Add(new Claim(AccountClaimTypes.UserIdClaimType, dbuser?.AspNetUserId ?? string.Empty));
                        claims.Add(new Claim("EmplId", dbuser?.Id.ToString()));
                        claims.Add(new Claim(AccountClaimTypes.UserTypeIdClaimType, dbuser?.UserTypeId?.ToString()));

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties { };

                        switch (dbuser?.UserTypeId)
                        {
                            case 1:
                                claimsIdentity.AddClaim(new Claim("Admin", "Admin"));
                                break;
                            case 2:
                                claimsIdentity.AddClaim(new Claim("LocalAdmin", "LocalAdmin"));
                                break;
                            case 3:
                                claimsIdentity.AddClaim(new Claim("CourtUser", "CourtUser"));
                                break;
                            case 5:
                                claimsIdentity.AddClaim(new Claim("ProsecutorAdmin", "ProsecutorAdmin"));
                                break;
                            default: break;
                        }

                        SjcUserSess sjcUser = new SjcUserSess
                        {


                            Name = dbuser?.UserName,
                            CourtId = dbuser?.CourtId ?? 0,
                            AspNetUserId = dbuser?.AspNetUserId ?? string.Empty,
                            UserId = dbuser?.Id ?? 0,
                        };
                        HttpContext.Session.Set<SjcUserSess>("SjcUserSess", sjcUser);
                        if (dbuser?.LoginEnabled == false)
                        {
                            claimsIdentity.AddClaim(new Claim("Disabled", "Disabled"));

                        }
                        var user = await _userManager.FindByIdAsync(dbuser?.AspNetUserId);
                        if (user != null)
                        {
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity),
                                authProperties);
                        }

                    }
                }
                catch { return Redirect(Url.Content("~/")); }

                return Redirect(Url.Content("~/"));
            }
            return RedirectToAction("UserNotFound", "Auth");
        }
        public AuthResult DecryptAuthResult(string encryptedBase64)
        {
            string base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012")); // 32 bytes
            string base64IV = Convert.ToBase64String(Encoding.UTF8.GetBytes("1234567890123456")); // 16 bytes
            string decryptedJson = AesEncryptionHelper.Decrypt(encryptedBase64, base64Key, base64IV);
            AuthResult decryptedResult = JsonConvert.DeserializeObject<AuthResult>(decryptedJson);
            return decryptedResult;
        }
        [Route("RemoteLoginCallback")]
        public async Task<IActionResult> RemoteLoginCallback(string id)
        {

            var authResult = DecryptAuthResult(id);
            if (!authResult.IsAuthenticated)
            {
                ViewBag.ErrorMessage = authResult.ErrorDescription;
                return View();
            }
            try
                {
                    var dbuser = await _sjcRepo.GetUserByIdentifierAsync(authResult.UserIdentifier);
                    if (dbuser == null)
                    {
                        _logger.LogError("UserNotFound");
                        return RedirectToAction("UserNotFound", "Auth");
                    }
                    else
                    {
                        List<Claim> claims = new List<Claim>();

                        claims.Add(new Claim(AccountClaimTypes.UserIdClaimType, dbuser?.AspNetUserId ?? string.Empty));
                        claims.Add(new Claim("EmplId", dbuser?.Id.ToString()));
                        claims.Add(new Claim(AccountClaimTypes.UserTypeIdClaimType, dbuser?.UserTypeId?.ToString()));

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties { };

                        switch (dbuser?.UserTypeId)
                        {
                            case 1:
                                claimsIdentity.AddClaim(new Claim("Admin", "Admin"));
                                break;
                            case 2:
                                claimsIdentity.AddClaim(new Claim("LocalAdmin", "LocalAdmin"));
                                break;
                            case 3:
                                claimsIdentity.AddClaim(new Claim("CourtUser", "CourtUser"));
                                break;
                            case 5:
                                claimsIdentity.AddClaim(new Claim("ProsecutorAdmin", "ProsecutorAdmin"));
                                break;
                            default: break;
                        }

                        SjcUserSess sjcUser = new SjcUserSess
                        {

                            Name = dbuser?.UserName,
                            CourtId = dbuser?.CourtId ?? 0,
                            AspNetUserId = dbuser?.AspNetUserId ?? string.Empty,
                            UserId = dbuser?.Id ?? 0,
                        };
                        HttpContext.Session.Set<SjcUserSess>("SjcUserSess", sjcUser);
                        if (dbuser?.LoginEnabled == false)
                        {
                            claimsIdentity.AddClaim(new Claim("Disabled", "Disabled"));

                        }
                        var user = await _userManager.FindByIdAsync(dbuser?.AspNetUserId);
                        if (user != null)
                        {
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity),
                                authProperties);
                        return RedirectToAction("Index", "Home");
                    }

                    }
                }
                catch { return Redirect(Url.Content("~/")); }

            return RedirectToAction("UserNotFound", "Auth");
        }
        [Route("auth/user-not-found")]
        public IActionResult UserNotFound() { 
            return View();
        }
        private Claim GetClaim(ClaimsIdentity principal, string claimType)
        {
            return ((ClaimsIdentity)principal).Claims.Where(c => c.Type == claimType).FirstOrDefault();
        }

        private string GetClaimValue(ClaimsIdentity principal, string claimType)
        {
            var claim = GetClaim(principal, claimType);
            return claim != null ? claim.Value : null;
        }
        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Content("~/"));
            }
          
            var binding = new Saml2PostBinding();
            var saml2LogoutRequest = await new Saml2LogoutRequest(config, User).DeleteSession(HttpContext);
            return binding.Bind(saml2LogoutRequest).ToActionResult();
        }

        [Route("LoggedOut")]
        public IActionResult LoggedOut()
        {
            var binding = new Saml2PostBinding();
            binding.Unbind(Request.ToGenericHttpRequest(), new Saml2LogoutResponse(config));

            return Redirect(Url.Content("~/"));
        }

        [Route("SingleLogout")]
        public async Task<IActionResult> SingleLogout()
        {
            Saml2StatusCodes status;
            var requestBinding = new Saml2PostBinding();
            var logoutRequest = new Saml2LogoutRequest(config, User);
            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), logoutRequest);
                status = Saml2StatusCodes.Success;
                await logoutRequest.DeleteSession(HttpContext);
            }
            catch (Exception exc)
            {
                // log exception
                Debug.WriteLine("SingleLogout error: " + exc.ToString());
                status = Saml2StatusCodes.RequestDenied;
            }

            var responsebinding = new Saml2PostBinding();
            responsebinding.RelayState = requestBinding.RelayState;
            var saml2LogoutResponse = new Saml2LogoutResponse(config)
            {
                InResponseToAsString = logoutRequest.IdAsString,
                Status = status,
            };
            return responsebinding.Bind(saml2LogoutResponse).ToActionResult();
        }
    }
}
