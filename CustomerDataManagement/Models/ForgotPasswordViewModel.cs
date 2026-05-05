using System.ComponentModel.DataAnnotations;

namespace CustomerDataManagement.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "User Name")]
        [RegularExpression("[A-Za-z0-9-._@+]*")]
        public string? Name { get; set; }
    }
}
