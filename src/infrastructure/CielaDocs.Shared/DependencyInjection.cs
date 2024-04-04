using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Domain.Settings;
using CielaDocs.Shared.DataAccess;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;




using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;

namespace CielaDocs.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureShared(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            services.AddTransient<IDateTime, DateTimeService>();
            services.AddTransient<IRandomGenerator, RandomGenerator>();
            services.AddTransient<IEmailService, EmailService>();
            services.Configure<SendGridMailerSettings>(config.GetSection("SendGridMailerSettings"));
            services.AddTransient<ISendGridMailer, SendGridMailer>();
            services.AddSingleton<LogContext>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddSingleton<SjcBudgetContext>();
            services.AddScoped<ISjcBudgetRepository, SjcBudgetRepository>();

            return services;
        }
    }
}
