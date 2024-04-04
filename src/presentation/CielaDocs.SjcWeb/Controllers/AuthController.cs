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

        public AuthController(Saml2Configuration config, SignInManager<IdentityUser> signInManager,
               UserManager<IdentityUser> userManager,
               IHttpContextAccessor httpContextAccessor,  ISjcBudgetRepository sjcRepo, IConfiguration configuration)
        {
            this.config = config;
            _userManager = userManager;
            _signInManager = signInManager;
          
            _httpContextAccessor = httpContextAccessor;
            _sjcRepo = sjcRepo;
            _configuration = configuration;
        }
       
        [Route("Login")]
        public IActionResult Login(string returnUrl = null)
        {
            //var binding=new Saml2PostBinding();//added signature to auth request but "egov eAuth' throw error 
            var binding = new Saml2RedirectBinding();//do not added signature

            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });
            var zr = binding.Bind(new Saml2AuthnRequest(config));

            var ret = binding.Bind(new Saml2AuthnRequest(config)
            {

                ForceAuthn = true,
                IsPassive = false,
                ProtocolBinding = new Uri("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"),
                AssertionConsumerServiceUrl = new Uri(_configuration.GetValue<string>("Saml2:Issuer")),
                IssueInstant = DateTime.Now,
                NameIdPolicy = new NameIdPolicy { AllowCreate = true, Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent" },
                Extensions = new Egov2Extensions(_configuration),

            });
            var z = ret;
            return ret.ToActionResult();
        }

        [Route("AssertionConsumerService")]
        public async Task<IActionResult> AssertionConsumerService()
        {
            var binding = new Saml2PostBinding();
            var saml2AuthnResponse = new Saml2AuthnResponse(config);

            binding.ReadSamlResponse(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");
            }
            binding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            ClaimsIdentity identity = saml2AuthnResponse.ClaimsIdentity;
            var personalIdentifier = GetClaimValue(identity, "urn:egov:bg:eauth:2.0:attributes:personIdentifier");
            if (!string.IsNullOrWhiteSpace(personalIdentifier)) {
                personalIdentifier=personalIdentifier.Split(new char[] { '-', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
            }
            if (string.IsNullOrWhiteSpace(personalIdentifier)) {
                return RedirectToAction(nameof(UserNotFound));
            }
            try {
                var dbuser = await _sjcRepo.GetUserByIdentifierAsync(personalIdentifier);
                if (dbuser == null)
                {
                    return RedirectToAction("UserNotFound", "Auth");
                }
                else {
                    List<Claim> claims = new List<Claim>();

                    claims.Add(new Claim(AccountClaimTypes.UserIdClaimType, dbuser?.AspNetUserId??string.Empty));
                    claims.Add(new Claim("EmplId", dbuser?.Id.ToString() ));
                    claims.Add(new Claim(AccountClaimTypes.UserTypeIdClaimType, dbuser?.UserTypeId?.ToString()));

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties  { };
                 
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
            // create auth session for authenticated user
           // await saml2AuthnResponse.CreateSession(HttpContext, claimsTransform: (claimsPrincipal) => ClaimsTransform.Transform(claimsPrincipal));

           // var relayStateQuery = binding.GetRelayStateQuery();
          //  var returnUrl = relayStateQuery.ContainsKey(relayStateReturnUrl) ? relayStateQuery[relayStateReturnUrl] : Url.Content("~/");
           
            return Redirect(Url.Content("~/"));
        }
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
