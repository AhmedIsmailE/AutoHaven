using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoHaven.Models
{
    public class FavouriteModel
    {
        [Key]
        public int FavouriteId { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }

        [ForeignKey("CarListing")]
        public int? ListingId { get; set; }

        public DateTime CreatedAt { get; set; }
        public virtual ApplicationUserModel User { get; set; }

        public virtual CarListingModel CarListing { get; set; }
    }
}