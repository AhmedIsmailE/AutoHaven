using Microsoft.AspNetCore.Mvc;

namespace AutoHaven.ViewModel
{
   
        public class AdminPostsViewModel
        {
            // ===== Rows =====
            public List<PostRow> Posts { get; set; } = new();

            // ===== Filters =====
            public string Search { get; set; } = string.Empty;
            public string Status { get; set; } = "All";
            public string Type { get; set; } = "All";
            public DateTime? DateFrom { get; set; }
            public DateTime? DateTo { get; set; }

            // ===== Paging =====
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int TotalCount { get; set; } = 0;

            // Convenience computed property
            public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

            // Computed display range
            public int ShowingFrom => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;
            public int ShowingTo => Math.Min(Page * PageSize, TotalCount);

            // ===== Nested row model (keeps everything in a single class file) =====
            public record PostRow
            {
                public int ListingId { get; init; }
                public string Title { get; init; } = string.Empty;
            public string ThumbnailUrl { get; set; } = "/images/default-car.png";
            public string SellerDisplay { get; init; } = string.Empty;
                public string SellerType { get; init; } = string.Empty;
                public decimal Price { get; init; }
                public string PriceLabel { get; init; } = string.Empty;
                public string Status { get; init; } = string.Empty;
                public bool IsFeatured { get; init; }
                public DateTime CreatedAt { get; init; }
            }
        }
}
