using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoHaven.Repository
{
    public class ReviewModelRepository : IReviewModelRepository
    {
        ProjectDbContext projectDbcontext;
        public ReviewModelRepository(ProjectDbContext _projectDbcontext)
        {
            projectDbcontext = _projectDbcontext;
        }
        public List<ReviewModel> Get()
        {
            List<ReviewModel> reviews = projectDbcontext.Reviews.AsNoTracking().ToList();
            return reviews;
        }
        public ReviewModel GetById(int id)
        {
            ReviewModel review = projectDbcontext.Reviews.FirstOrDefault(s => s.ReviewId == id);
            return review;
        }
        public List<ReviewModel> GetByListingId(int listingId)
        {
            List<ReviewModel> review = projectDbcontext.Reviews.AsNoTracking().Where(r => r.ListingId == listingId).OrderByDescending(r => r.CreatedAt).ToList();
            return review;
        }

        public List<ReviewModel> GetByUserId(int userId)
        {
            List<ReviewModel> review = projectDbcontext.Reviews.AsNoTracking().Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedAt).ToList();
            return review;
        }
        public void Insert(ReviewModel review)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));
            if (review.Rating < 1 || review.Rating > 5) // Rating is out of 5 only
                throw new ArgumentOutOfRangeException(nameof(review.Rating), "Rating must be between 1 and 5.");

            review.CreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            projectDbcontext.Reviews.Add(review);
            projectDbcontext.SaveChanges();
        }
        public void Update(ReviewModel review)
        {
            if (review == null) throw new ArgumentNullException(nameof(review)); ;
            var existing = projectDbcontext.Reviews.Find(review.ReviewId);
            if (existing == null) throw new ArgumentNullException(nameof(existing));

            if (review.Rating < 1 || review.Rating > 5)
                throw new ArgumentOutOfRangeException(nameof(review.Rating), "Rating must be between 1 and 5.");

            existing.Rating = review.Rating;
            existing.Comment = review.Comment;
            existing.UpdatedAt = DateTime.UtcNow;

            projectDbcontext.SaveChanges();
        }

        // Delete
        public void Delete(int reviewId)
        {
            var existing = projectDbcontext.Reviews.Find(reviewId);
            if (existing == null) throw new ArgumentNullException(nameof(existing));

            projectDbcontext.Reviews.Remove(existing);
            projectDbcontext.SaveChanges();
        }

        // Helpers
        public double GetAverageRating(int listingId)
        {
            var ratings = projectDbcontext.Reviews.Where(r => r.ListingId == listingId).Select(r => (int?)r.Rating).ToList();
            if (!ratings.Any()) return 0;
            return ratings.Average() ?? 0;
        }

        public int GetCount(int listingId)
        {
            return projectDbcontext.Reviews.Count(r => r.ListingId == listingId);
        }
    }
}
