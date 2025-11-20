using AutoHaven.Models;
using AutoHaven.IRepository;
using Microsoft.EntityFrameworkCore;
namespace AutoHaven.Repository
{
    public class CarModelRepository : ICarModelRepository
    {
        ProjectDbContext projectDbcontext;
        public CarModelRepository(ProjectDbContext _projectDbcontext)
        {
            projectDbcontext = _projectDbcontext;
        }
        public List<CarModel> Get()
        {
            List<CarModel> cars = projectDbcontext.Cars.AsNoTracking().ToList();
            return cars;
        }

        public CarModel GetById(int id)
        {
            CarModel car = projectDbcontext.Cars.FirstOrDefault(s => s.CarId == id);
            return car;
        }
        public void Insert(CarModel car)
        {
            projectDbcontext.Cars.Add(car);
            projectDbcontext.SaveChanges();
        }
        public void Delete(int id)
        {
            CarModel car = GetById(id);
            projectDbcontext.Cars.Remove(car);
            projectDbcontext.SaveChanges();
        }
        public void Update(CarModel car)
        {
            CarModel cr = GetById(car.CarId);
            // Update allowed fields
            cr.Manufacturer = car.Manufacturer;
            cr.Model = car.Model;
            cr.ModelYear = car.ModelYear;
            cr.BodyStyle = car.BodyStyle;
            cr.Power = car.Power;
            cr.Doors = car.Doors;
            cr.fuel = car.fuel;
            cr.trans = car.trans;
            projectDbcontext.SaveChanges();
        }
    }
}
