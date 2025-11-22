using AutoHaven.Models;
using System.ComponentModel.DataAnnotations;

namespace AutoHaven.ViewModel
{
    public class RegisterUserViewModel
    {
        [Required]
        
        public string UserName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Phone]
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public ApplicationUser.RoleEnum Role { get; set; } = ApplicationUser.RoleEnum.Customer;

        // OPTIONAL: CreatedAt and UpdatedAt handled automatically in IdentityUser child
    }
}
