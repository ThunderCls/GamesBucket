using System.ComponentModel.DataAnnotations;

namespace GamesBucket.App.Models
{
    public class ProfileDetailsViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
    }
}