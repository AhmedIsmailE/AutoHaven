using AutoHaven.Models;
using System.ComponentModel.DataAnnotations;
using static AutoHaven.Models.CarListingModel;


namespace AutoHaven.ViewModel
{

    public class CreateCarListingViewModel
    {
        // ===== CAR INFORMATION (From CarModel) =====

        [Required(ErrorMessage = "Manufacturer is required.")]
        [StringLength(100)]
        [Display(Name = "Make")]
        public string Manufacturer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required.")]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required.")]
        [Range(1900, 2100, ErrorMessage = "Please enter a valid year.")]
        [Display(Name = "Model Year")]
        public int ModelYear { get; set; }

        [StringLength(50)]
        [Display(Name = "Body Style")]
        public string? BodyStyle { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [Required(ErrorMessage = "Transmission is required.")]
        [Display(Name = "Transmission")]
        public CarModel.Transmission Transmission { get; set; }

        [Required(ErrorMessage = "Fuel type is required.")]
        [Display(Name = "Fuel Type")]
        public CarModel.FuelType Fuel { get; set; }

        [Required(ErrorMessage = "Power (horsepower) is required.")]
        [Range(1, 10000, ErrorMessage = "Please enter a valid power value.")]
        [Display(Name = "Power (HP)")]
        public int Power { get; set; }

        [Required(ErrorMessage = "Number of doors is required.")]
        [Range(1, 10, ErrorMessage = "Please enter a valid number of doors.")]
        public int Doors { get; set; }

        // ===== LISTING INFORMATION (From CarListingModel) =====

        [Required(ErrorMessage = "Please select a listing type.")]
        [Display(Name = "Listing Type")]
        public ListingType ListingType { get; set; }

        [Display(Name = "Sale Price")]
        [DataType(DataType.Currency)]
        [Range(0, 999999999, ErrorMessage = "Please enter a valid price.")]
        public decimal? NewPrice { get; set; }

        [Display(Name = "Rental Price (Per Day)")]
        [DataType(DataType.Currency)]
        [Range(0, 999999, ErrorMessage = "Please enter a valid rental price.")]
        public decimal? RentPrice { get; set; }

        [StringLength(4000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // ===== OPTIONAL: IMAGES =====

        [Display(Name = "Vehicle Images")]
        public List<IFormFile>? ImageFiles { get; set; }

        // ===== VALIDATION HELPER PROPERTY =====
        // This property will be checked in the controller to ensure the correct price is filled.
        public bool IsValid()
        {

            if (ListingType == ListingType.ForSelling)
            {
                if (!NewPrice.HasValue || NewPrice <= 0)
                {
                    return false;
                }
            }

          
            if (ListingType == ListingType.ForRenting)
            {
                if (!RentPrice.HasValue || RentPrice <= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}