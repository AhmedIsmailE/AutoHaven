using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace AutoHaven.Models
{
    public class ApplicationUser : IdentityUser
    {
        
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
}