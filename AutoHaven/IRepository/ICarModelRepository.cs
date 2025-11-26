using AutoHaven.Models;

namespace AutoHaven.IRepository
{
    public interface ICarModelRepository
    {
        public List<CarModel> Get();
        public CarModel GetById(int id);
        public void Insert(CarModel car);
        public void Delete(int id);
        public void Update(CarModel car);
    }
}
