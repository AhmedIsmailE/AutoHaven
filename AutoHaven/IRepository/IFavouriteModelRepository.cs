//using AutoHaven.Models;

//namespace AutoHaven.IRepository
//{
//    public interface IFavouriteModelRepository
//    {
//        public List<FavouriteModel> Get();
//        public FavouriteModel GetById(int id);
//        public List<FavouriteModel> GetByUserId(int userId);
//        public List<FavouriteModel> GetByListingId(int listingId);
//        public void Insert(int listingId, int userId);
//        public void Insert(FavouriteModel favourite);
//        public bool RemoveFavourite(int listingId, int userId);
//        public void Delete(int id);
//        public void DeleteByUserAndListing(int userId, int listingId);
//        public bool Exists(int listingId, int userId);
//        public int CountForListing(int listingId);

//    }
//}

using AutoHaven.Models;

namespace AutoHaven.IRepository
{
    public interface IFavouriteModelRepository
    {
        List<FavouriteModel> Get();
        FavouriteModel GetById(int id);
        List<FavouriteModel> GetByListingId(int listingId);
        List<FavouriteModel> GetByUserId(int userId);
        void Insert(FavouriteModel favourite);
        void Update(FavouriteModel favourite);
        void Delete(int id);
        void DeleteByUserAndListing(int userId, int listingId);
        void RemoveFavourite(int userId, int listingId);
        int CountForListing(int listingId);
        bool Exists(int userId, int listingId);
    }
}