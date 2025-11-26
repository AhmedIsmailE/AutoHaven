using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoHaven.Repository
{
    public class FavouriteRepository : IFavouriteModelRepository
    {
        private readonly ProjectDbContext _projectDbContext;

        public FavouriteRepository(ProjectDbContext projectDbContext)
        {
            _projectDbContext = projectDbContext;
        }

        public List<FavouriteModel> Get()
        {
            return _projectDbContext.Favourites
                .Include(f => f.User)
                .Include(f => f.CarListing)
                    .ThenInclude(cl => cl.Car)
                .Include(f => f.CarListing.CarImages)
                .AsNoTracking()
                .ToList();
        }

        public FavouriteModel GetById(int id)
        {
            return _projectDbContext.Favourites
                .Include(f => f.User)
                .Include(f => f.CarListing)
                .FirstOrDefault(f => f.FavouriteId == id);
        }

        // ✅ ADD THIS
        public List<FavouriteModel> GetByListingId(int listingId)
        {
            return _projectDbContext.Favourites
                .Include(f => f.User)
                .Include(f => f.CarListing)
                .Where(f => f.ListingId == listingId)
                .AsNoTracking()
                .ToList();
        }

        // ✅ ADD THIS
        public List<FavouriteModel> GetByUserId(int userId)
        {
            return _projectDbContext.Favourites
                .Include(f => f.CarListing)
                    .ThenInclude(cl => cl.Car)
                .Include(f => f.CarListing.CarImages)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .AsNoTracking()
                .ToList();
        }

        public void Insert(FavouriteModel favourite)
        {
            if (favourite == null) throw new ArgumentNullException(nameof(favourite));

            var existing = _projectDbContext.Favourites
                .FirstOrDefault(f => f.UserId == favourite.UserId && f.ListingId == favourite.ListingId);

            if (existing != null)
                throw new InvalidOperationException("This listing is already in your favorites.");

            favourite.CreatedAt = DateTime.UtcNow;
            _projectDbContext.Favourites.Add(favourite);
            _projectDbContext.SaveChanges();
        }

        public void Update(FavouriteModel favourite)
        {
            if (favourite == null) throw new ArgumentNullException(nameof(favourite));

            var existing = GetById(favourite.FavouriteId);
            if (existing == null) throw new InvalidOperationException($"Favourite with ID {favourite.FavouriteId} not found.");

            _projectDbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var favourite = GetById(id);
            if (favourite != null)
            {
                _projectDbContext.Favourites.Remove(favourite);
                _projectDbContext.SaveChanges();
            }
        }

        public void DeleteByUserAndListing(int userId, int listingId)
        {
            var favourite = _projectDbContext.Favourites
                .FirstOrDefault(f => f.UserId == userId && f.ListingId == listingId);

            if (favourite != null)
            {
                _projectDbContext.Favourites.Remove(favourite);
                _projectDbContext.SaveChanges();
            }
        }

        // ✅ ADD THIS
        public int CountForListing(int listingId)
        {
            return _projectDbContext.Favourites
                .Count(f => f.ListingId == listingId);
        }

        // ✅ ADD THIS
        public bool Exists(int userId, int listingId)
        {
            return _projectDbContext.Favourites
                .Any(f => f.UserId == userId && f.ListingId == listingId);
        }

        // ✅ ADD THIS
        public void RemoveFavourite(int userId, int listingId)
        {
            var favourite = _projectDbContext.Favourites
                .FirstOrDefault(f => f.UserId == userId && f.ListingId == listingId);

            if (favourite != null)
            {
                _projectDbContext.Favourites.Remove(favourite);
                _projectDbContext.SaveChanges();
            }
        }
    }
}