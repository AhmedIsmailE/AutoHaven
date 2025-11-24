
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
        public FuelType CurrentFuel { get; set; }
        public int Power { get; set; }
        public int Doors { get; set; }
        public enum Transmission { Manual = 0, Automatic = 1 }
        public Transmission CurrentTransmission { get; set; }

        public List<CarListingModel> CarListings { get; set; }
    }
}
