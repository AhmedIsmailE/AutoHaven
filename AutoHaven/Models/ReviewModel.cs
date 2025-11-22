using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoHaven.Models
{
    public class ReviewModel
    {
        [Key]
        public int ReviewId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("CarListing")]
        public int ListingId { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ApplicationUser User { get; set; }

        public CarListingModel CarListing { get; set; }
    }
}
