using System;
using System.Collections.Generic;
using System.Linq;
using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoHaven.Repository
{
    public class FavouriteRepository : IFavouriteModelRepository
    {
        ProjectDbContext projectDbcontext;
        public FavouriteRepository(ProjectDbContext _projectDbcontext)
        {
            projectDbcontext = _projectDbcontext;
        }

        // --- queries ---

        public List<FavouriteModel> Get()
        {
            List<FavouriteModel> fav = projectDbcontext.Favourites.AsNoTracking().ToList();
            return fav;
        }

        public FavouriteModel GetById(int id)
        {
            FavouriteModel fav = projectDbcontext.Favourites.FirstOrDefault(s => s.FavouriteId == id);
            return fav;
        }

        public List<FavouriteModel> GetByUserId(int userId)
        {
            List<FavouriteModel> fav = projectDbcontext.Favourites.AsNoTracking().Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedAt).ToList();
            return fav;
        }

        public List<FavouriteModel> GetByListingId(int listingId)
        {
            List<FavouriteModel> fav = projectDbcontext.Favourites.AsNoTracking().Where(r => r.ListingId == listingId).OrderByDescending(r => r.CreatedAt).ToList();
            return fav;
        }


        public void Insert(int listingId, int userId)
        {
            // prevent duplicates
            var exists = projectDbcontext.Favourites.Any(f => f.ListingId == listingId && f.UserId == userId);
            if (exists) return; // already present, treat as success

            var fav = new FavouriteModel
            {
                ListingId = listingId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            projectDbcontext.Favourites.Add(fav);
            projectDbcontext.SaveChanges();
        }

        public bool RemoveFavourite(int listingId, int userId)
        {
            var fav = projectDbcontext.Favourites.FirstOrDefault(f => f.ListingId == listingId && f.UserId == userId);
            if (fav == null) return false;

            projectDbcontext.Favourites.Remove(fav);
            projectDbcontext.SaveChanges();
            return true;
        }

        public void Delete(int id)
        {
            var fav = projectDbcontext.Favourites.Find(id);
            if (fav == null) return;

            projectDbcontext.Favourites.Remove(fav);
            projectDbcontext.SaveChanges();
        }
        public bool Exists(int listingId, int userId)
        {
            return projectDbcontext.Favourites.Any(f => f.ListingId == listingId && f.UserId == userId);
        }

        public int CountForListing(int listingId)
        {
            return projectDbcontext.Favourites.Count(f => f.ListingId == listingId);
        }
    }
}
