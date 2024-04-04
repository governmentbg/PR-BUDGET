using SendGrid;

using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Services
{
    public interface ISendGridMailer
    {
        Task<Response> SendEmailAsync(List<string> emails, string subject, string message);
        Task<Response> SendEmailAsync(string emails, string subject, string message);
        Task<Response> SendEmailAsync(MailMessage mM);
        Task<Response> SendEmailAsync(MailMessage mM, string sTitel);
        Task<Response> SendFeedbackEmailAsync( string subject, string message);
    }
}
