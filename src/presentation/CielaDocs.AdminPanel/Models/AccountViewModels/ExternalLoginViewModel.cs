using System.ComponentModel.DataAnnotations;

namespace CielaDocs.AdminPanel.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
