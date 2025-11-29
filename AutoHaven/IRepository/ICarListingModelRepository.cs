using AutoHaven.Models;

namespace AutoHaven.IRepository
{
    public interface ICarListingModelRepository
    {
        public List<CarListingModel> Get();
        public CarListingModel GetById(int id);
        public void Insert(CarListingModel car, IEnumerable<IFormFile>? images = null);
        public void Delete(int id);
        public void Update(CarListingModel car, int[] imageIdsToKeep, IEnumerable<IFormFile>? newImages = null);
        void IncrementViews(int listingId);
        // Count active listings for a user
        int CountActiveListingsByUserId(int userId);

        // Count featured listings for a user
        int CountFeaturedByUserId(int userId);

        // Hide cars when subscription expires
        void HideListingsByUserId(int userId);
    }
}
