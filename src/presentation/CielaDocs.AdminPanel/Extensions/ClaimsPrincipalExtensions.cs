using CielaDocs.Application;
using CielaDocs.AdminPanel.Extensions;

using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;

namespace CielaDocs.AdminPanel.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string FindFirstValue(this ClaimsPrincipal principal, string claimType, bool throwIfNotFound = false)
        {
            Guard.ArgumentNotNull(principal, nameof(principal));

            var value = principal.FindFirst(claimType)?.Value;
            if (throwIfNotFound && string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture, "The supplied principal does not contain a claim of type {0}", claimType));
            }

            return value;
        }

       

        /// <summary>
        /// Extension method on <see cref="System.Security.Claims.ClaimsPrincipal"/> which returns the AAD Tenant ID, if it exists.
        /// </summary>
        /// <param name="principal">A <see cref="System.Security.Claims.ClaimsPrincipal"/> representing the currently signed in ASP.NET user.</param>
        /// <returns>The AAD Tenant ID if it exists, otherwise, an exception is thrown.</returns>
       
        public static int GetUserAccountTypeValue(this ClaimsPrincipal principal)
        {
            return (int)Convert.ChangeType(principal.FindFirstValue(AccountClaimTypes.UserAccountClaimType, false), typeof(int));
        }
        public static string GetUserIdValue(this ClaimsPrincipal principal)
        {
            return (string)Convert.ChangeType(principal.FindFirstValue("UserId", true), typeof(string));
        }

        public static int GetEmplIdValue(this ClaimsPrincipal principal)
        {
            return (int)Convert.ChangeType(principal.FindFirstValue("EmplId", true), typeof(int));
        }



        public static string GetEmailValue(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Email, true);
        }

        public static string GetUserName(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);
        }

        public static bool IsSignedInToApplication(this ClaimsPrincipal principal)
        {
            Guard.ArgumentNotNull(principal, nameof(principal));
            return principal.Identity != null && principal.Identity.IsAuthenticated;
        }

    }
}

