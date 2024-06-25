using System.ComponentModel.DataAnnotations;

namespace Authentication.Models.Users
{
    public class ResetPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

		[Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters")]
		public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
