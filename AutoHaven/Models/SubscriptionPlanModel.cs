using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace AutoHaven.Models
{
    public class SubscriptionPlanModel
    {
        [Key]
        public int SubscriptionPlanId { get; set; }

        [Required]
        public string SubscriptionName { get; set; }

        public int MaxCarListing { get; set; }
        public int FeatureSlots { get; set; }
        public decimal PricePerMonth { get; set; }
        public enum Tiers { Free = 0, Starter = 1, Pro = 2, Elite = 3 }

        public Tiers tier { get; set; } = Tiers.Free;
        public virtual List<UserSubscriptionModel> UserSubscriptions { get; set; }
    }
}