using System.ComponentModel.DataAnnotations;

namespace CielaDocs.AdminPanel.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
