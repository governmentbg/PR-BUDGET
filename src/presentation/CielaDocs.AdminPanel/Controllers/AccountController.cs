﻿using CielaDocs.Application;
using CielaDocs.Data.Contexts;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using CielaDocs.AdminPanel.Extensions;
using CielaDocs.AdminPanel.Models.AccountViewModels;


using MediatR;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CielaDocs.Domain.Entities;
using Microsoft.Graph;

namespace CielaDocs.AdminPanel.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly string AdminCode = "Admin1!";
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly ISendGridMailer _emailSender;
        private readonly ILogger _logger;
        private readonly ILogRepository _logRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;
        private readonly ISjcBudgetRepository _sjcRepo;

        public AccountController(SignInManager<IdentityUser> signInManager,
             ILogger<AccountController> logger,
             UserManager<IdentityUser> userManager,
            ISendGridMailer emailSender,
            ILogRepository logRepo, IMediator mediator, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ISjcBudgetRepository sjcRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _logRepo = logRepo;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
            _sjcRepo = sjcRepo;


        }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        public IActionResult SignIn()
        {
            return new ChallengeResult(
                OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    RedirectUri = Url.Action("SignInCallback", "Account")
                });
        }
        [AllowAnonymous]


        public IActionResult SignOutCallback()
        {
            HttpContext.Session.Remove(".SjcBudget.Session");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// Callback method used when a user has been successfully authenticated.  This can be used for any sign in post-processing.
        /// </summary>
        /// <remarks>Any modifications to the user's <see cref="System.Security.Claims.ClaimsPrincipal"/> within this callback will not be persisted across requests.</remarks>
        /// <returns>A <see cref="RedirectToActionResult"/> representing where to redirect the user after authentication has completed.</returns>

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "Не сте потвърдили все още вашата регистрация! Изпратен ви е имейл за потвърждение на регистрацията!");
                        return View(model);
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    
                    List<Claim> claims = new List<Claim>();

                    claims.Add(new Claim(AccountClaimTypes.UserIdClaimType, user?.Id));
                    claims.Add(new Claim(ClaimTypes.Name, user?.Email??string.Empty));
                  
                    claims.Add(new Claim(AccountClaimTypes.UserAccountClaimType, "1"));

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                    };
                    var db = HttpContext.RequestServices.GetService<ApplicationDbContext>();
                    var empl = await _sjcRepo.GetUserByASpNetUserIdAsync(user?.Id??string.Empty);
                    claimsIdentity.AddClaim(new Claim(AccountClaimTypes.UserTypeIdClaimType, empl?.UserTypeId?.ToString()));
                    switch (empl?.UserTypeId) {
                        case 1:
                            claimsIdentity.AddClaim(new Claim("Admin", "Admin"));
                            break;
                        case 2:
                            claimsIdentity.AddClaim(new Claim("LocalAdmin", "LocalAdmin"));
                            break;
                        case 3:
                            claimsIdentity.AddClaim(new Claim("User", "User"));
                            break;
                        default:break;
                    }
                    claimsIdentity.AddClaim(new Claim("EmplId", (empl?.Id??0).ToString()));

                    if (empl?.LoginEnabled == false)
                    {
                        claimsIdentity.AddClaim(new Claim("Disabled", "Disabled"));
                      
                    }
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                    //----------------
                   

                    var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    string logmsg = $"Вход в системата на {empl?.UserName}";
                    await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId =  empl?.Id??0, MsgId = 0, Msg = logmsg, IP = ip });
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction("Enable", "TwoFactorAuthentication");
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Невалиден потребител.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpPost]
        public ActionResult LoginWith2fa(string inputCode)
        {
            var user = User.Identity.Name;
            //TwoFactorAuthenticator twoFactor = new TwoFactorAuthenticator();
            //bool isValid = twoFactor.ValidateTwoFactorPIN($"myverysecretkey +{user}", inputCode);
            //if (!isValid)
            //{
            //    return RedirectToAction(nameof(LoginWith2fa), new { user, string.Empty });
            //}

            //user.TwoFactorEnabled = true;
            return Redirect("/");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginWith2fa(string email, bool rememberMe, string returnUrl = null)
        {
            //var user = _dbContext.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            //// Ensure the user has gone through the username & password screen first
            //var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            //if (user == null)
            //{
            //    throw new ApplicationException($"Не може да се зареди потребител с двуфакторно удостоверяване.");
            //}

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;


            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Не може да се зареди потребител с '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);


                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                if (model.AdminCode != AdminCode)
                {
                    ModelState.AddModelError(string.Empty, "Невалиден код за регистрация.");
                    return View(model);
                }
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);
                    await _emailSender.SendEmailAsync(model.Email, "Потвърдете своята регистрация",
                        $"Моля, потвърдете акаунта си като <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>кликнете тук</a>.");


                    await _signInManager.SignInAsync(user, isPersistent: false);
                    bool bIsRegistered = await _mediator.Send(new CheckUserByAspNetUserIdQuery { AspNetUserId = user?.Id });
                    if (!bIsRegistered)
                    {
                        _ = await _mediator.Send(new AddUserCommand { AspNetUserId = user?.Id, Email=model?.Email });
                    }
                    return RedirectToAction("SendActivationEmail");
                

                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //[HttpPost]
        public async Task<IActionResult> Logout()
        {
            _ = int.TryParse(User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value?.ToString(), out int UserId);
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Изход от системата на {User.Identity.Name}";
            await _logRepo.AddToAppUserLogAsync(new CielaDocs.Domain.Entities.AppUserLog { AppUserId = UserId, MsgId = 0, Msg = logmsg, IP = ip });
            // Clear the existing external cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //HttpContext.Session.Remove(".CielaDocs.Session");
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "Нулиране на парола",
                    $"Моля, променете своята парола като <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>кликнете тук</a>.");
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SendActivationEmail()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }




        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
