
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoHaven.Models
{
    public class UserSubscriptionModel
    {
        [Key]
        public int UserSubscriptionId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("SubscriptionPlan")]
        public int SubscriptionPlanId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

       
        public enum BillingCycle { Monthly = 0, HalfYear = 1, Yearly = 2 }
        public enum Status { Active = 0, Cancelled = 1, Expired = 2 }

       // ADDED THESE PROPERTIES
        public BillingCycle CurrentBillingCycle { get; set; }
        public Status CurrentStatus { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public SubscriptionPlanModel SubscriptionPlan { get; set; } = null!;
    }
}
