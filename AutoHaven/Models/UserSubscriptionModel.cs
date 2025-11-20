using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static AutoHaven.Models.UserModel;
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

        public enum BillingCycle {Inactive = 0 ,Monthly = 1, QuarterYearl = 2, HalfYear = 3, Yearly = 4 }
        public enum Status {Inactive = 0 ,Active = 1, Cancelled = 2, Expired = 3 }
        public Status status { get; set; } = Status.Inactive;
        public BillingCycle billingcycle { get; set; } = BillingCycle.Inactive;
        public UserModel User { get; set; }
        public SubscriptionPlanModel SubscriptionPlan { get; set; }
    }
}
