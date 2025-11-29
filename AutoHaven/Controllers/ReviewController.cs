using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoHaven.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewModelRepository _reviewRepo;
        private readonly ICarListingModelRepository _carListingRepo;

        public ReviewController(
            IReviewModelRepository reviewRepo,
            ICarListingModelRepository carListingRepo)
        {
            _reviewRepo = reviewRepo;
            _carListingRepo = carListingRepo;
        }

        // ==================== POST: Add Review ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(int listingId, int rating, string comment)
        {
            try
            {
                if (rating < 1 || rating > 5)
                {
                    TempData["Error"] = "Rating must be between 1 and 5.";
                    return RedirectToAction("Details", "Car", new { id = listingId });
                }

                if (string.IsNullOrWhiteSpace(comment))
                {
                    TempData["Error"] = "Comment is required.";
                    return RedirectToAction("Details", "Car", new { id = listingId });
                }

                int userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized();

                var listing = _carListingRepo.GetById(listingId);
                if (listing == null)
                    return NotFound("Listing not found.");

                // ✅ CHECK IF USER IS OWNER - PREVENT REVIEW
                if (listing.UserId == userId)
                {
                    TempData["Error"] = "You cannot review your own listing.";
                    return RedirectToAction("Details", "Car", new { id = listingId });
                }

                var review = new ReviewModel
                {
                    UserId = userId,
                    ListingId = listingId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _reviewRepo.Insert(review);

                TempData["Success"] = "Review added successfully!";
                return RedirectToAction("Details", "Car", new { id = listingId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding review: {ex.Message}";
                return RedirectToAction("Details", "Car", new { id = listingId });
            }
        }

        // ==================== POST: Delete Review ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteReview(int reviewId, int listingId)
        {
            try
            {
                var review = _reviewRepo.GetById(reviewId);
                if (review == null)
                    return NotFound("Review not found.");

                int userId = GetCurrentUserId();
                if (review.UserId != userId)
                    return Forbid("You don't have permission to delete this review.");

                _reviewRepo.Delete(reviewId);

                TempData["Success"] = "Review deleted successfully!";
                return RedirectToAction("Details", "Car", new { id = listingId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting review: {ex.Message}";
                return RedirectToAction("Details", "Car", new { id = listingId });
            }
        }

        // ==================== POST: Edit Review ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditReview(int reviewId, int listingId, int rating, string comment)
        {
            try
            {
                if (rating < 1 || rating > 5)
                {
                    TempData["Error"] = "Rating must be between 1 and 5.";
                    return RedirectToAction("Details", "Car", new { id = listingId });
                }

                if (string.IsNullOrWhiteSpace(comment))
                {
                    TempData["Error"] = "Comment is required.";
                    return RedirectToAction("Details", "Car", new { id = listingId });
                }

                var review = _reviewRepo.GetById(reviewId);
                if (review == null)
                    return NotFound("Review not found.");

                int userId = GetCurrentUserId();
                if (review.UserId != userId)
                    return Forbid("You don't have permission to edit this review.");

                review.Rating = rating;
                review.Comment = comment;
                review.UpdatedAt = DateTime.Now;

                _reviewRepo.Update(review);

                TempData["Success"] = "Review updated successfully!";
                return RedirectToAction("Details", "Car", new { id = listingId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating review: {ex.Message}";
                return RedirectToAction("Details", "Car", new { id = listingId });
            }
        }

        // ==================== HELPER: Get Current User ID ====================
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(nameIdentifier) && int.TryParse(nameIdentifier, out int id))
                return id;

            return 0;
        }
    }
}