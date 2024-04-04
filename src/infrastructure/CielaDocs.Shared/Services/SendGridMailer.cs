
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SendGrid;
using SendGrid.Helpers.Mail;
using SendGrid.Helpers.Mail.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Services
{
    public class SendGridMailer : ISendGridMailer
    {
        private SendGridMailerSettings _sendGridSettings;
        private readonly ILogger<SendGridMailer> _logger;

        public SendGridMailer(IOptions<SendGridMailerSettings> sendGridSettings, ILogger<SendGridMailer> logger)
        {
            _sendGridSettings = sendGridSettings.Value;
            _logger = logger;
        }
        public async Task<Response> SendEmailAsync(List<string> emails, string subject, string message)
        {

            return await Execute(_sendGridSettings.SendGridApiKey, subject, message, emails);
        }
        public async Task<Response> SendEmailAsync(string emails, string subject, string message)
        {

            return await Execute(_sendGridSettings.SendGridApiKey, subject, message, emails);
        }
        public async Task<Response> Execute(string apiKey, string subject, string message, List<string> emails)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_sendGridSettings.OutgoingEmail, "Сигнал за нередност"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
               
        };
          
            foreach (var email in emails)
            {
                msg.AddTo(new EmailAddress(email));
            }

            var response = await client.SendEmailAsync(msg);
            return response;
        }
        public async Task<Response> Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_sendGridSettings.OutgoingEmail, "Сигнал за нередност"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));
            var response = await client.SendEmailAsync(msg);
            return response;
        }

        public async Task<Response> SendFeedbackEmailAsync(string subject, string message)
        {
            return await Execute(_sendGridSettings.SendGridApiKey, subject, message,_sendGridSettings.FeedbackEmail);
        }

        public async Task<Response> SendEmailAsync(MailMessage mM)
        {
            var client = new SendGridClient(_sendGridSettings.SendGridApiKey);
            string recipientEmailAddress = mM.To.FirstOrDefault().Address;
            var from = new EmailAddress(_sendGridSettings.OutgoingEmail, "Сигнал за нередност");
            var subject = mM.Subject;
            var to = new EmailAddress(recipientEmailAddress, "");
            var plainTextContent = mM.Body;
            var htmlContent = "<strong>" + mM.Body + "</strong>";
            var msg = CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            if (mM.Attachments.Count > 0)
            {
                foreach (var mailAttachment in mM.Attachments)
                {
                    using (var stream = new MemoryStream())
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        mailAttachment.ContentStream.CopyTo(stream);
                        msg.AddAttachment(mailAttachment.Name, Convert.ToBase64String(stream.ToArray()));
                    }
                }
            }

            var response = await client.SendEmailAsync(msg);
            return response;
        }
        public async Task<Response> SendEmailAsync(MailMessage mM, string sTitle)
        {
            var client = new SendGridClient(_sendGridSettings.SendGridApiKey);
            string recipientEmailAddress = mM.To.FirstOrDefault().Address;
            var from = new EmailAddress(_sendGridSettings.OutgoingEmail, sTitle);
            var subject = mM.Subject;
            var to = new EmailAddress(recipientEmailAddress, "");
            var plainTextContent = mM.Body;
            var htmlContent = "<strong>" + mM.Body + "</strong>";
            var msg = CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            if (mM.Attachments.Count > 0)
            {
                foreach (var mailAttachment in mM.Attachments)
                {
                    using (var stream = new MemoryStream())
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        mailAttachment.ContentStream.CopyTo(stream);
                        msg.AddAttachment(mailAttachment.Name, Convert.ToBase64String(stream.ToArray()));
                    }
                }
            }

            var response = await client.SendEmailAsync(msg);
            return response;
        }
        public static SendGridMessage CreateSingleEmail(EmailAddress from, EmailAddress to, string subject, string plainTextContent, string htmlContent)
        {
            SendGridMessage sendGridMessage = new SendGridMessage();
            sendGridMessage.SetFrom(from);
            sendGridMessage.SetSubject(subject);
            if (!string.IsNullOrEmpty(plainTextContent))
            {
                sendGridMessage.AddContent(MimeType.Text, plainTextContent);
            }

            if (!string.IsNullOrEmpty(htmlContent))
            {
                sendGridMessage.AddContent(MimeType.Html, htmlContent);
            }

            sendGridMessage.AddTo(to);
            return sendGridMessage;
        }
    }
}

