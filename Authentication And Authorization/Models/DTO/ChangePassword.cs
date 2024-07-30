using System.ComponentModel.DataAnnotations;

namespace Authentication_And_Authorization.Models.DTO
{
    public class ChangePassword
    {

        [Required]
        public string Username { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string ConfirmNewPassword { get; set; }
    }
}
