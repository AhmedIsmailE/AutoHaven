using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoHaven.Models
{
    public class CarImageModel
    {
        [Key]
        public int CarImageId { get; set; }

        [ForeignKey("CarListing")]
        public int ListingId { get; set; }

       public string Path { get; set; }
        public string AltText { get; set; }
        public bool IsPrimary { get; set; }

        public virtual CarListingModel CarListing { get; set; }
    }
}