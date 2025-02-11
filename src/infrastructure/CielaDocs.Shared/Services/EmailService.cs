using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Application.Dtos.Email;
using CielaDocs.Domain.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CielaDocs.Application.Common.Exceptions;

namespace CielaDocs.Shared.Services
{
    public class EmailService : IEmailService
    {
        private MailSettings MailSettings { get; }
        private ILogger<EmailService> Logger { get; }

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            MailSettings = mailSettings.Value;
            Logger = logger;
        }

        public async Task SendAsync(EmailDto request)
        {
            //
        }

        public Task SendEmailConfirmationAsync(string email, string message, string link)
        {
            throw new NotImplementedException();
        }
    }
}