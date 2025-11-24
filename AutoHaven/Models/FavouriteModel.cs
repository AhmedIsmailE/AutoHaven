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
<<<<<<< HEAD
        public int? UserId { get; set; }

        [ForeignKey("CarListing")]
        public int? ListingId { get; set; }
=======
        public int UserId { get; set; }

        [ForeignKey("CarListing")]
        public int ListingId { get; set; }
>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd

        public DateTime CreatedAt { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual CarListingModel CarListing { get; set; }
    }
}