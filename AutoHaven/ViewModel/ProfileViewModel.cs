using AutoHaven.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace AutoHaven.ViewModel
{
    public class ProfileViewModel
    {
            public const string DevFallbackLocalPath = "/images/Default/default.jpg";
            public int Id { get; set; }
            public string? AvatarUrl { get; set; }
            public string? Name { get; set; }
            public string? UserName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [Phone(ErrorMessage = "Invalid phone number")]
            public string? PhoneNumber { get; set; }
            public string? CompanyName { get; set; }
            public string? Street { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public ApplicationUserModel.RoleEnum Role { get; set; }
            public DateTime CreatedAt { get; set; }
            
            public DateTime UpdatedAt { get; set; }

            public string? SubscriptionTier { get; set; }
            public DateTime? SubscriptionExpiresAt { get; set; }

        public static ProfileViewModel MapToModel(ApplicationUserModel user)
        {
            var avatarUrl = string.IsNullOrWhiteSpace(user.AvatarUrl)
                ? DevFallbackLocalPath
                : user.AvatarUrl;

            var vm = new ProfileViewModel
            {
                Id = user.Id,
                AvatarUrl = avatarUrl,
                Name = user.Name,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CompanyName = user.CompanyName,
                Street = user.Street,
                City = user.City,
                State = user.State,
                Role = user.Role,
                UpdatedAt = user.UpdatedAt,
                CreatedAt = user.CreatedAt,
            };

            // ----- Subscription selection logic -----
            // Prefer an active subscription that hasn't ended; otherwise choose the subscription with the latest EndDate.
            // ----- Subscription selection logic -----
            try
            {
                var subs = user.UserSubscriptions ?? new List<UserSubscriptionModel>();
                var now = DateTime.Now;

                // choose active sub first; otherwise choose the most recent one
                var active = subs
                    .Where(s => s.CurrentStatus == UserSubscriptionModel.Status.Active && s.EndDate > now)
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefault();

                var chosen = active ?? subs.OrderByDescending(s => s.EndDate).FirstOrDefault();

                if (chosen != null)
                {
                    // Tier
                    vm.SubscriptionTier = chosen.SubscriptionPlan?.tier.ToString();

                    // Expiration date
                    vm.SubscriptionExpiresAt = chosen.EndDate;
                }
                else
                {
                    vm.SubscriptionTier = null;
                    vm.SubscriptionExpiresAt = null;
                }
            }
            catch
            {
                vm.SubscriptionTier = null;
                vm.SubscriptionExpiresAt = null;
            }


            return vm;
        }

    }
}
