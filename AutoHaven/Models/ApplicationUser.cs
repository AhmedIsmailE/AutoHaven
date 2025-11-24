using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.ComponentModel.DataAnnotations;

namespace AutoHaven.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? CompanyName { get; set; }

        public string? Name { get; set; } 

        [MaxLength(150)]
        public string? Street { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        public enum RoleEnum { Customer = 0, Provider = 1, Admin = 2 }
        public RoleEnum Role { get; set; } = RoleEnum.Customer;

        // Navigation properties - Initialized to avoid null references
        public virtual List<FavouriteModel> Favourites { get; set; } = new();
        public virtual List<ReviewModel> Reviews { get; set; } = new();
        public virtual List<CarListingModel> CarListings { get; set; } = new();
        public virtual List<UserSubscriptionModel> UserSubscriptions { get; set; } = new();
    }

=======

namespace AutoHaven.Models
{
    public class ApplicationUser : IdentityUser
    {
        // ✅ Define the enum
        public enum RoleEnum
        {
            Customer = 0,
            Provider = 1,
            Admin = 2
        }

        public int UserId { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public string Role { get; set; } = "Customer"; // Customer, Provider, Admin

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public List<CarListingModel> CarListings { get; set; } = new();
        public List<FavouriteModel> Favourites { get; set; } = new();
        public List<ReviewModel> Reviews { get; set; } = new();
        public List<UserSubscriptionModel> UserSubscriptions { get; set; } = new();
    }
>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd
}