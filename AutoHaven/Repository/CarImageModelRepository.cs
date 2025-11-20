using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace AutoHaven.Repository
{
    public class CarImageModelRepository : ICarImageModelRepository
    {
        ProjectDbContext projectDbcontext;
        IFileStorage? _fileStorage; // it can be null
        const string ImagesFolder = "Uploads/Listings";
        public CarImageModelRepository(ProjectDbContext _projectDbcontext, IFileStorage? fileStorage = null)
        {
            projectDbcontext = _projectDbcontext;
            _fileStorage = fileStorage;
        }
        public List<CarImageModel> Get()
        {
            List<CarImageModel> carimages = projectDbcontext.CarImages.AsNoTracking().ToList();
            return carimages;
        }
        public CarImageModel GetById(int id)
        {
            CarImageModel carimage = projectDbcontext.CarImages.FirstOrDefault(s => s.CarImageId == id);
            return carimage;
        }
        public void Insert(CarImageModel carimage)
        {
            if (carimage.IsPrimary)
            {
                var existingPrimary = projectDbcontext.CarImages.FirstOrDefault(i => i.ListingId == carimage.ListingId && i.IsPrimary);
                if (existingPrimary != null)
                {
                    existingPrimary.IsPrimary = false;
                }
            }
            projectDbcontext.CarImages.Add(carimage);
            projectDbcontext.SaveChanges();
        }
        public List<CarImageModel> AddRange(IEnumerable<CarImageModel> images)
        {
            if (images == null) return new List<CarImageModel>();
            var list = images.ToList();
            if (!list.Any()) return new List<CarImageModel>();

            // If multiple primaries in the batch, keep only the first as primary
            var firstPrimary = list.FirstOrDefault(i => i.IsPrimary);
            if (firstPrimary != null)
            {
                foreach (var other in list.Where(i => i.IsPrimary && i != firstPrimary))
                    other.IsPrimary = false;

                // clear DB primary for that listing (if any)
                var dbPrimaries = projectDbcontext.CarImages.Where(i => i.ListingId == firstPrimary.ListingId && i.IsPrimary).ToList();
                foreach (var dp in dbPrimaries) dp.IsPrimary = false;
            }

            projectDbcontext.CarImages.AddRange(list);
            projectDbcontext.SaveChanges();
            return list;
        }
        public void Update(CarImageModel image)
        {
            var existing = projectDbcontext.CarImages.FirstOrDefault(i => i.CarImageId == image.CarImageId);
            // if switching to primary, clear previous primary
            if (image.IsPrimary && !existing.IsPrimary)
            {
                var prior = projectDbcontext.CarImages.FirstOrDefault(i => i.ListingId == existing.ListingId && i.IsPrimary);
                if (prior != null) prior.IsPrimary = false;
            }

            // update fields
            existing.Path = image.Path;
            existing.AltText = image.AltText;
            existing.IsPrimary = image.IsPrimary;
            existing.ListingId = image.ListingId;
            projectDbcontext.SaveChanges();
        }
        public void Delete(int id)
        {
            var existing = projectDbcontext.CarImages.Find(id);

            // attempt file deletion (best-effort)
            if (!string.IsNullOrWhiteSpace(existing.Path) && _fileStorage != null)
            {
                try { _fileStorage.DeleteFile(existing.Path); } catch { /* log if desired */ }
            }

            projectDbcontext.CarImages.Remove(existing);
            projectDbcontext.SaveChanges();
        }
        public void DeleteByListingId(int listingId)
        {
            var images = projectDbcontext.CarImages.Where(i => i.ListingId == listingId).ToList();

            if (_fileStorage != null)
            {
                foreach (var img in images)
                {
                    if (!string.IsNullOrWhiteSpace(img.Path))
                    {
                        try { _fileStorage.DeleteFile(img.Path); } catch { }
                    }
                }
            }
            projectDbcontext.CarImages.RemoveRange(images);
            projectDbcontext.SaveChanges();
        }
        public CarImageModel GetPrimaryImage(int listingId)
        {
            return projectDbcontext.CarImages.AsNoTracking().FirstOrDefault(i => i.ListingId == listingId && i.IsPrimary);
        }

        public void SetPrimary(int listingId, int imageId)
        {
            var target = projectDbcontext.CarImages.FirstOrDefault(i => i.CarImageId == imageId && i.ListingId == listingId);

            var others = projectDbcontext.CarImages.Where(i => i.ListingId == listingId && i.IsPrimary && i.CarImageId != imageId).ToList();
            foreach (var o in others) o.IsPrimary = false;

            target.IsPrimary = true;
            projectDbcontext.SaveChanges();
        }
    }
}
