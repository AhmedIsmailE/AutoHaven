
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace AutoHaven.Models
{
    public class CarModel
    {
        [Key]
        public int CarId { get; set; }

        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public int ModelYear { get; set; }
        public string BodyStyle { get; set; }
        public enum FuelType { Fuel = 0, Electric = 1, Diesel = 2, Hybrid = 3 }
        public int Power { get; set; }
        public int Doors { get; set; }
        public enum Transmission { Manual = 0, Automatic = 1 }
        public Transmission trans { get; set; } = Transmission.Manual;
        public FuelType fuel { get; set; } = FuelType.Fuel;
        public virtual List<CarListingModel> CarListings { get; set; }
    }
}
