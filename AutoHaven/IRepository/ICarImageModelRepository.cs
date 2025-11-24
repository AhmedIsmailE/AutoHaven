using AutoHaven.Models;

namespace AutoHaven.IRepository
{
    public interface ICarImageModelRepository
    {
        public List<CarImageModel> Get();
        public CarImageModel GetById(int id);
        public void Insert(CarImageModel carimage);
        public List<CarImageModel> AddRange(IEnumerable<CarImageModel> images);
        public void Update(CarImageModel image);
        public void Delete(int id);
        public void DeleteByListingId(int listingId);
        public CarImageModel GetPrimaryImage(int listingId);
        public void SetPrimary(int listingId, int imageId);
    }
}
