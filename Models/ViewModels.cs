using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = null!;

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
