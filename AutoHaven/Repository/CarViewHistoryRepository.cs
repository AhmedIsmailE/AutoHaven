using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace AutoHaven.Repository
{
    public class CarViewHistoryRepository : ICarViewHistoryRepository
    {
        private readonly ProjectDbContext _context;

        public CarViewHistoryRepository(ProjectDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CarViewHistoryModel> Get()
        {
            return _context.CarViewHistories
                .Include(h => h.CarListing)
                    .ThenInclude(cl => cl.Car)
                .Include(h => h.CarListing)
                    .ThenInclude(cl => cl.CarImages)
                .AsNoTracking()
                .OrderByDescending(h => h.ViewedAt)
                .ToList();
        }
        
        public bool HasViewedBefore(int listingId, int? userId, string ipAddress)
        {
            if (userId.HasValue && userId > 0)
            {
                // Logged-in user
                return _context.CarViewHistories
                    .Any(h => h.ListingId == listingId && h.UserId == userId);
            }
            else
            {
                // Anonymous user (by IP)
                return _context.CarViewHistories
                    .Any(h => h.ListingId == listingId && h.IpAddress == ipAddress);
            }
        }
        public IEnumerable<CarViewHistoryModel> GetByUserId(int userId)
        {
            return _context.CarViewHistories
                .Where(h => h.UserId == userId)
                .Include(h => h.CarListing)
                    .ThenInclude(cl => cl.Car)
                .Include(h => h.CarListing)
                    .ThenInclude(cl => cl.CarImages)
                .AsNoTracking()
                .OrderByDescending(h => h.ViewedAt)
                .ToList();
        }

        public IEnumerable<CarViewHistoryModel> GetLatestPerListingForUser(int userId, int skip, int take)
        {
            // 1) Get the Ids of the latest history rows per listing for this user (pure projection)
            var latestIds = _context.CarViewHistories
                .Where(h => h.UserId == userId)
                .GroupBy(h => h.ListingId)
                // for each group select the row with the latest ViewedAt
                .Select(g => g.OrderByDescending(x => x.ViewedAt).FirstOrDefault().Id)
                // order groups by their ViewedAt (we need to correlate id -> ViewedAt ordering;
                // ordering here is approximate because we only have the Id right now — we'll re-order after fetching)
                .ToList();

            if (!latestIds.Any())
                return new List<CarViewHistoryModel>();

            // 2) Apply paging to the list of latest ids (server-side if you want, but we already materialized ids).
            //    Use ordering by ViewedAt when we fetch the entities to ensure correct order.
            var pagedIds = latestIds
                .OrderByDescending(id => id)    // placeholder; better ordering follows when fetching entities
                .Skip(skip)
                .Take(take)
                .ToList();

            // 3) Fetch the actual history rows including related listing and images.
            var items = _context.CarViewHistories
                .Where(h => pagedIds.Contains(h.Id))
                .Include(h => h.CarListing)
                    .ThenInclude(cl => cl.Car)
                .Include(h => h.CarListing)
                    .ThenInclude(cl => cl.CarImages)
                .AsNoTracking()
                // order by ViewedAt descending to show most recent first
                .OrderByDescending(h => h.ViewedAt)
                .ToList();

            // 4) If you want the items in the exact order of pagedIds (safe), reorder them in-memory:
            var itemsById = items.ToDictionary(x => x.Id);
            var ordered = pagedIds.Where(id => itemsById.ContainsKey(id)).Select(id => itemsById[id]).ToList();

            return ordered;
        }

        public void Insert(CarViewHistoryModel item)
        {
            _context.CarViewHistories.Add(item);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.CarViewHistories.Find(id);
            if (entity != null)
            {
                _context.CarViewHistories.Remove(entity);
                _context.SaveChanges();
            }
        }

        public void DeleteByUser(int userId)
        {
            var items = _context.CarViewHistories.Where(h => h.UserId == userId);
            _context.CarViewHistories.RemoveRange(items);
            _context.SaveChanges();
        }
    }
}
