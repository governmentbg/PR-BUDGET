using System.ComponentModel.DataAnnotations;

namespace CielaDocs.SjcWeb.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
