using System.ComponentModel.DataAnnotations;

namespace Authentication.Models.Users
{
    public class UserRegisterRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
