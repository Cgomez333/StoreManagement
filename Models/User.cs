using System;
using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "Empleado";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
