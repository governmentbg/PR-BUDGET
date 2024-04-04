using CielaDocs.Application.Dtos.Email;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailDto emailRequest);
        Task SendEmailConfirmationAsync(string email, string message, string link);
    }
}
