using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoHaven.Models
{
    public class ApplicationUserModel : IdentityUser<int>
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string? CompanyName { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string? Name { get; set; }

        [MaxLength(150)]
        [Column(TypeName = "nvarchar(150)")]
        public string? Street { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string? City { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string? State { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? AvatarUrl { get; set; }

        [StringLength(14, MinimumLength = 14)]
        [Column(TypeName = "nvarchar(14)")]
        public string? NationalId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? IdImagePath { get; set; }

        [NotMapped]
        public IFormFile? IdImage { get; set; }

        public enum RoleEnum { Customer = 0, Provider = 1, Admin = 2 }
        public RoleEnum Role { get; set; } = RoleEnum.Customer;

        public virtual List<FavouriteModel> Favourites { get; set; } = new();
        public virtual List<ReviewModel> Reviews { get; set; } = new();
        public virtual List<CarListingModel> CarListings { get; set; } = new();
        public virtual List<UserSubscriptionModel> UserSubscriptions { get; set; } = new();
    }
}