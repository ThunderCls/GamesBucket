using System.ComponentModel.DataAnnotations;

namespace GamesBucket.App.Models
{
    public class UserRegisterViewModel
    {
        [Required]
        [Display(Name = "first name")]
        public string FirstName { get; set; }
        
        [Required]
        [Display(Name = "last name")]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        
        [Required]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}