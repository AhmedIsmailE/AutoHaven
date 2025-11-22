using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AutoHaven.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? CompanyName { get; set; }

        [MaxLength(150)]
        public string? Street { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }
        public enum RoleEnum { Customer = 0, Provider = 1, Admin = 2 }
        public RoleEnum Role { get; set; } = RoleEnum.Customer;

        public virtual List<FavouriteModel> Favourites { get; set; }
        public virtual List<ReviewModel> Reviews { get; set; }
        public virtual List<CarListingModel> CarListings { get; set; }
        public virtual List<UserSubscriptionModel> UserSubscriptions { get; set; }
    }
}
