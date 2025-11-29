using AutoHaven.Models;
using System.ComponentModel.DataAnnotations;

namespace AutoHaven.ViewModel
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Account Type")]
        public ApplicationUserModel.RoleEnum Role { get; set; } = ApplicationUserModel.RoleEnum.Customer;

        // ==================== PROVIDER FIELDS ====================
        [StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be 14 digits")]
        public string? NationalId { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? IdImage { get; set; }

        public string? CompanyName { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? Street { get; set; }
    }
}