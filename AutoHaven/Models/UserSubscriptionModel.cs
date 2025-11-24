<<<<<<< HEAD
﻿
=======
﻿//using System;
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

>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd
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

<<<<<<< HEAD
       
        public enum BillingCycle { Monthly = 0, HalfYear = 1, Yearly = 2 }
        public enum Status { Active = 0, Cancelled = 1, Expired = 2 }

       // ADDED THESE PROPERTIES
        public BillingCycle CurrentBillingCycle { get; set; }
        public Status CurrentStatus { get; set; }
=======
        // ✅ Define enums at top
        public enum BillingCycle { Monthly = 0, HalfYear = 1, Yearly = 2 }  // ✅ FIXED: QuarterYearl -> QuarterYearly
        public enum Status { Active = 0, Cancelled = 1, Expired = 2 }

        // ✅ ADD THESE PROPERTIES
        public BillingCycle BillingCycle { get; set; }
        public Status Status { get; set; }
>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd

        public ApplicationUser User { get; set; } = null!;
        public SubscriptionPlanModel SubscriptionPlan { get; set; } = null!;
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> 5d3eb87504c0b7f615a3a91f6a8bc6860a2ccccd
