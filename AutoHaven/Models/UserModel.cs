////using System.Collections.Generic;
////using System.ComponentModel.DataAnnotations;
////using System.ComponentModel.DataAnnotations.Schema;
////namespace AutoHaven.Models
////{
////    public class UserModel
////    {
////        [Key]
////        public int UserId { get; set; }

////        [Required, MaxLength(100)]
////        public string UserName { get; set; }

////        [Required, MaxLength(255)]
////        public string Email { get; set; }

////        [Required, MaxLength(255)]
////        public string Password { get; set; }
////        [Required, MaxLength(255)]
////        public string ConfirmPassword { get; set; }

////        [MaxLength(100)]
////        public string Name { get; set; }

////        [MaxLength(100)]
////        public string CompanyName { get; set; }

////        [MaxLength(20)]
////        public string Phone { get; set; }

////        [MaxLength(150)]
////        public string Street { get; set; }

////        [MaxLength(100)]
////        public string City { get; set; }

////        [MaxLength(100)]
////        public string State { get; set; }

////        public DateTime CreatedAt { get; set; }
////        public DateTime UpdatedAt { get; set; }

////        public enum Role { Customer = 0, Provider = 1, Admin = 2 }

////        public List<FavouriteModel> Favourites { get; set; } = new();
////        public List<ReviewModel> Reviews { get; set; } = new();
////        public List<CarListingModel> CarListings { get; set; } = new();
////        public List<UserSubscriptionModel> UserSubscriptions { get; set; } = new();
////    }
////}
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace AutoHaven.Models
//{
//    public class UserModel
//    {
//        [Key]
//        public int UserId { get; set; }

//        [Required, MaxLength(100)]
//        public string UserName { get; set; }

//        [Required, MaxLength(255)]
//        public string Email { get; set; }

//        [MaxLength(100)]
//        public string Name { get; set; }

//        [MaxLength(100)]
//        public string CompanyName { get; set; }

//        [MaxLength(20)]
//        public string PhoneNumber { get; set; }

//        [MaxLength(150)]
//        public string Street { get; set; }

//        [MaxLength(100)]
//        public string City { get; set; }

//        [MaxLength(100)]
//        public string State { get; set; }

//        [Required]
//        public string Role { get; set; } = "Customer";

//        // ✅ Link back to ApplicationUser
//        public string ApplicationUserId { get; set; }
//        [ForeignKey("ApplicationUserId")]
//        public ApplicationUser ApplicationUser { get; set; }

//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

//        // Navigation Properties
//        public List<CarListingModel> CarListings { get; set; } = new();
//        public List<FavouriteModel> Favourites { get; set; } = new();
//        public List<ReviewModel> Reviews { get; set; } = new();
//        public List<UserSubscriptionModel> UserSubscriptions { get; set; } = new();
//    }
//}