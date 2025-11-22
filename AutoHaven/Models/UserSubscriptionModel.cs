//using System;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using static AutoHaven.Models.CarListingModel;
//namespace AutoHaven.Models
//{
//    public class UserSubscriptionModel
//    {
//        [Key]
//        public int UserSubscriptionId { get; set; }

//        [ForeignKey("User")]
//        public int UserId { get; set; }

//        [ForeignKey("SubscriptionPlan")]
//        public int SubscriptionPlanId { get; set; }

//        public DateTime StartDate { get; set; }
//        public DateTime EndDate { get; set; }

//        public enum BillingCycle { Monthly = 0, QuarterYearl = 1, HalfYear = 2, Yearly = 3 }
//        public enum Status { Active = 0, Cancelled = 1, Expired = 2 }

//        public BillingCycle BillingCycle { get; set; }
//        public Status Status { get; set; }

//        public ApplicationUser User { get; set; } = null!;
//        public SubscriptionPlanModel SubscriptionPlan { get; set; } = null!;
//    }
//}

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

        // ✅ Define enums at top
        public enum BillingCycle { Monthly = 0, HalfYear = 1, Yearly = 2 }  // ✅ FIXED: QuarterYearl -> QuarterYearly
        public enum Status { Active = 0, Cancelled = 1, Expired = 2 }

        // ✅ ADD THESE PROPERTIES
        public BillingCycle BillingCycle { get; set; }
        public Status Status { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public SubscriptionPlanModel SubscriptionPlan { get; set; } = null!;
    }
}