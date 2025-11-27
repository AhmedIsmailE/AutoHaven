using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoHaven.Models
{
    public class CarViewHistoryModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CarListing")]
        public int ListingId { get; set; }
        public CarListingModel CarListing { get; set; }

        public int? UserId { get; set; }

        public string? AnonymousId { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }
}
