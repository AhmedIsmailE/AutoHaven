using AutoHaven.Models;

namespace AutoHaven.IRepository
{
    public interface IReviewModelRepository
    {
        public List<ReviewModel> Get();
        public ReviewModel GetById(int id);
        public List<ReviewModel> GetByListingId(int listingId);
        public List<ReviewModel> GetByUserId(int userId);
        public void Insert(ReviewModel review);
        public void Update(ReviewModel review);
        public void Delete(int reviewId);
        public double GetAverageRating(int listingId);
        public int GetCount(int listingId);
    }
}
