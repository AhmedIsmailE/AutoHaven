
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
        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }  
        public enum Status { Active = 0, Cancelled = 1, Expired = 2 }
        public Status CurrentStatus { get; set; }
        public virtual ApplicationUserModel User { get; set; } = null!;
        public virtual SubscriptionPlanModel SubscriptionPlan { get; set; } = null!;
    }
}
