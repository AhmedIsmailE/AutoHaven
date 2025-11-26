using AutoHaven.IRepository;
using AutoHaven.Models;
using AutoHaven.Storage;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace AutoHaven.Repository
{
    public class CarListingModelRepository : ICarListingModelRepository
    {
        ProjectDbContext projectDbcontext;
        IFileStorage? _fileStorage; // it can be null
        const string ImagesFolder = "Uploads/Listings";
        public CarListingModelRepository(ProjectDbContext _projectDbcontext, IFileStorage? fileStorage = null)
        {
            projectDbcontext = _projectDbcontext;
            _fileStorage = fileStorage;
        }
        public List<CarListingModel> Get()
        {
            List<CarListingModel> carslist = projectDbcontext.CarListings.AsNoTracking().ToList();
            return carslist;
        }

        public CarListingModel GetById(int id)
        {
            CarListingModel car = projectDbcontext.CarListings.FirstOrDefault(s => s.ListingId == id);
            return car;
        }
        public void Insert(CarListingModel car, IEnumerable<IFormFile>? images = null)
        {
            car.CreatedAt = DateTime.UtcNow;
            car.UpdatedAt = DateTime.UtcNow;
            car.CarImages = car.CarImages ?? new List<CarImageModel>();
            car.Reviews = car.Reviews ?? new List<ReviewModel>();
            car.Favourites = car.Favourites ?? new List<FavouriteModel>();
            using (var tx = projectDbcontext.Database.BeginTransaction())
            {
                try
                {
                    projectDbcontext.CarListings.Add(car);
                    projectDbcontext.SaveChanges();

                    if (images != null && images.Any())
                    {
                        var first = true;
                        foreach (var f in images)
                        {
                            string path;
                            if (_fileStorage != null) path = _fileStorage.SaveFile(f, ImagesFolder);
                            else path = f.FileName; // fallback

                            var img = new CarImageModel
                            {
                                ListingId = car.ListingId,
                                Path = path,
                                AltText = f.FileName,
                                IsPrimary = first
                            };
                            first = false;
                            projectDbcontext.CarImages.Add(img);
                        }
                        projectDbcontext.SaveChanges();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            projectDbcontext.CarListings.Add(car);
            projectDbcontext.SaveChanges();
        }
        public void Delete(int id)
        {
            CarListingModel car = GetById(id);
            using (var tx = projectDbcontext.Database.BeginTransaction())
            {
                try
                {
                    // delete images files first (best-effort)
                    if (car.CarImages != null)
                    {
                        foreach (var img in car.CarImages)
                        {
                            if (!string.IsNullOrWhiteSpace(img.Path) && _fileStorage != null)
                            {
                                try { _fileStorage.DeleteFile(img.Path); } catch { }
                            }
                        }
                    }

                    // remove children then listing
                    if (car.CarImages != null && car.CarImages.Any()) projectDbcontext.CarImages.RemoveRange(car.CarImages);
                    if (car.Reviews != null && car.Reviews.Any()) projectDbcontext.Reviews.RemoveRange(car.Reviews);
                    if (car.Favourites != null && car.Favourites.Any()) projectDbcontext.Favourites.RemoveRange(car.Favourites);
                    projectDbcontext.CarListings.Remove(car);
                    projectDbcontext.SaveChanges();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
        public void Update(CarListingModel car, int[] imageIdsToKeep, IEnumerable<IFormFile>? newImages = null)
        {
            CarListingModel cr = GetById(car.ListingId);
            // Update allowed fields
            cr.RentPrice = car.RentPrice;
            cr.NewPrice = car.NewPrice;
            cr.OldPrice = car.OldPrice;
            cr.Description = car.Description;
            cr.IsFeatured = car.IsFeatured;
            cr.Discount = car.Discount;
            cr.Color = car.Color;
            cr.Type = car.Type;
            cr.CurrentState = car.CurrentState;
            cr.UpdatedAt = DateTime.UtcNow;
            using (var tx = projectDbcontext.Database.BeginTransaction())
            {
                try
                {
                    // --- handle image diff: remove images not in imageIdsToKeep ---
                    var keepSet = (imageIdsToKeep ?? new int[0]).ToHashSet();
                    var toRemove = cr.CarImages?.Where(i => !keepSet.Contains(i.CarImageId)).ToList() ?? new List<CarImageModel>();

                    foreach (var rem in toRemove)
                    {
                        // delete file best-effort
                        if (!string.IsNullOrWhiteSpace(rem.Path) && _fileStorage != null)
                        {
                            try { _fileStorage.DeleteFile(rem.Path); } catch { }
                        }
                        projectDbcontext.CarImages.Remove(rem);
                    }
                    projectDbcontext.SaveChanges();

                    // --- add new uploaded images ---
                    if (newImages != null && newImages.Any())
                    {
                        var hadPrimary = cr.CarImages != null && cr.CarImages.Any(i => i.IsPrimary);
                        foreach (var f in newImages)
                        {
                            string path = _fileStorage != null ? _fileStorage.SaveFile(f, ImagesFolder) : f.FileName;
                            var img = new CarImageModel
                            {
                                ListingId = cr.ListingId,
                                Path = path,
                                AltText = f.FileName,
                                IsPrimary = !hadPrimary // make first uploaded primary only if none exists
                            };
                            hadPrimary = true;
                            projectDbcontext.CarImages.Add(img);
                        }
                        projectDbcontext.SaveChanges();
                    }

                    // ensure single primary per listing: keep last primary if multiple
                    var primaries = projectDbcontext.CarImages.Where(i => i.ListingId == cr.ListingId && i.IsPrimary).ToList();
                    if (primaries.Count > 1)
                    {
                        var last = primaries.OrderBy(p => p.CarImageId).Last();
                        foreach (var p in primaries.Where(p => p.CarImageId != last.CarImageId)) p.IsPrimary = false;
                    }

                    projectDbcontext.SaveChanges();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

        }
        // ✅ NEW METHOD: Increment views when user visits listing
        public void IncrementViews(int listingId)
        {
            try
            {
                var listing = projectDbcontext.CarListings.FirstOrDefault(cl => cl.ListingId == listingId);
                if (listing != null)
                {
                    listing.Views++;
                    listing.UpdatedAt = DateTime.UtcNow;
                    projectDbcontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error incrementing views: {ex.Message}");
                
            }
        }
    }
}
