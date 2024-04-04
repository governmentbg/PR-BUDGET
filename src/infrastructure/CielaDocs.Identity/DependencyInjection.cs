using CielaDocs.Identity.Helpers;
using CielaDocs.Identity.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CielaDocs.Identity
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureIdentity(this IServiceCollection services, IConfiguration config)
        {
            //services.Configure<AuthUserSettings>(config.GetSection(nameof(AuthUserSettings)));
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
