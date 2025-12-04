using Microsoft.AspNetCore.Mvc;

namespace AutoHaven.ViewModel
{
    public class MyListingViewModel
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "/images/default-car.png";
        public decimal Price { get; set; }
        public string PriceLabel { get; set; } = string.Empty; // e.g. "/day" or ""
        public bool IsForRent { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TypeLabel => IsForRent ? "For Rent" : "For Sale";
    }
}
