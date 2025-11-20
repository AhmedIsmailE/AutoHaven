using AutoHaven.Models;

namespace AutoHaven.IRepository
{
    public interface IFavouriteModelRepository
    {
        public List<FavouriteModel> Get();
        public FavouriteModel GetById(int id);
        public List<FavouriteModel> GetByUserId(int userId);
        public List<FavouriteModel> GetByListingId(int listingId);
        public void Insert(int listingId, int userId);
        public bool RemoveFavourite(int listingId, int userId);
        public void Delete(int id);
        public bool Exists(int listingId, int userId);
        public int CountForListing(int listingId);
    }
}
