using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoHaven.Models
{
    public class CarListingModel
    {
        [Key]
        public int ListingId { get; set; }

        [ForeignKey("Car")]
        public int CarId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public int Views { get; set; } = 0;

        public enum ListingType { ForRenting = 0, ForSelling = 1 }
        public ListingType Type { get; set; }
        public decimal RentPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public string Description { get; set; }

        public bool IsFeatured { get; set; }
        public int Discount { get; set; }
        public string Color { get; set; }
        public enum State { Sold = 0, Rented = 1, Unavaliable = 2, Available = 3 }
        public State CurrentState { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public CarModel Car { get; set; }
        public ApplicationUserModel User { get; set; } = null!;
        public virtual List<UserSubscriptionModel> UserSubscriptions { get; set; } = new();
        public List<CarImageModel> CarImages { get; set; }
        public List<ReviewModel> Reviews { get; set; }
        public List<FavouriteModel> Favourites { get; set; }
    }
}
