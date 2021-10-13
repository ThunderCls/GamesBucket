using System.ComponentModel.DataAnnotations;

namespace GamesBucket.App.Models
{
    public class UserLoginViewModel
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        
        [Required]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool Remember { get; set; }
    }
}