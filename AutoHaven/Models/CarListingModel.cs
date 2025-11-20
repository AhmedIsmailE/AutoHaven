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

        public enum ListingType { ForRenting = 0, ForSelling = 1 }
        public decimal RentPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public string Description { get; set; }

        public bool IsFeatured { get; set; }
        public int Discount { get; set; }
        public string Color { get; set; }
        public enum State { Available = 0, Unavaliable = 1, Rented = 2, Sold = 3 }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public State state { get; set; } = State.Available;
        public ListingType listingType { get; set; } = ListingType.ForSelling;
        public CarModel Car { get; set; }
        public UserModel User { get; set; }
        public List<CarImageModel> CarImages { get; set; }
        public List<ReviewModel> Reviews { get; set; }
        public List<FavouriteModel> Favourites { get; set; }
    }
}
