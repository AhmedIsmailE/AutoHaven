using AutoHaven.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace AutoHaven.ViewModel
{
    public class ProfileViewModel
    {
            public const string DevFallbackLocalPath = "/images/Default/default.jpg";
            public int Id { get; set; }
            public string? AvatarUrl { get; set; }
            public string? Name { get; set; }
            public string? UserName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [Phone(ErrorMessage = "Invalid phone number")]
            public string? PhoneNumber { get; set; }
            public string? CompanyName { get; set; }
            public string? Street { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public ApplicationUserModel.RoleEnum Role { get; set; }
            public DateTime CreatedAt { get; set; }
            
            public DateTime UpdatedAt { get; set; }
            public static ProfileViewModel MapToModel(ApplicationUserModel user)
            {
            var avatarUrl = string.IsNullOrWhiteSpace(user.AvatarUrl)
                ? DevFallbackLocalPath
                : user.AvatarUrl;

            return new ProfileViewModel
                {
                    Id = user.Id,
                    AvatarUrl = avatarUrl,
                    Name = user.Name,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    CompanyName = user.CompanyName,
                    Street = user.Street,
                    City = user.City,
                    State = user.State,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };
            }

    }
}
