using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace AutoHaven.Models
{
    public class UserModel
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string UserName { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; }

        [Required, MaxLength(255)]
        public string Password { get; set; }
        [Required, MaxLength(255)]
        [Compare(nameof(Password))] // Added to compare between password and confirm password
        public string ConfirmPassword { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string CompanyName { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(150)]
        public string Street { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(100)]
        public string State { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public enum Role { Customer = 0, Provider = 1, Admin = 2 }

        // For Saving Role in Database
        public Role role { get; set; } = Role.Customer;
        public List<FavouriteModel> Favourites { get; set; } = new();
        public List<ReviewModel> Reviews { get; set; } = new();
        public List<CarListingModel> CarListings { get; set; } = new();
        public List<UserSubscriptionModel> UserSubscriptions { get; set; } = new();
    }
}
