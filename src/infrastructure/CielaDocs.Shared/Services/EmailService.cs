using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Application.Dtos.Email;
using CielaDocs.Domain.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
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
            try
            {
                // create message
                var email = new MimeMessage { Sender = MailboxAddress.Parse(request.From ?? MailSettings.EmailFrom) };
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;
                var builder = new BodyBuilder { HtmlBody = request.Body };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (System.Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                throw new ApiException(ex.Message);
            }
        }

        public Task SendEmailConfirmationAsync(string email, string message, string link)
        {
            throw new NotImplementedException();
        }
    }
}