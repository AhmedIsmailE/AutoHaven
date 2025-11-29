using AutoHaven.IRepository;
using AutoHaven.Models;

namespace AutoHaven.Services
{
    public class CarListingValidationService
    {
        private readonly IUserSubscriptionModelRepository _userSubRepo;
        private readonly ICarListingModelRepository _carListingRepo;

        public CarListingValidationService(
            IUserSubscriptionModelRepository userSubRepo,
            ICarListingModelRepository carListingRepo)
        {
            _userSubRepo = userSubRepo;
            _carListingRepo = carListingRepo;
        }

        /// <summary>
        /// Validates if user can create a new car listing
        /// </summary>
        public (bool isValid, string message) ValidateCarCreation(int userId, bool wantsFeatured = false)
        {
            // 1. Get highest active subscription
            var subscription = _userSubRepo.GetActiveForUser(userId);

            // NO SUBSCRIPTION
            if (subscription == null)
            {
                return (false, "❌ You need an active subscription to list cars. Please purchase a plan.");
            }

            // 2. Check car listing limit
            var carCount = _carListingRepo.CountActiveListingsByUserId(userId);
            if (carCount >= subscription.SubscriptionPlan.MaxCarListing)
            {
                return (false,
                    $"❌ You've reached your car listing limit ({carCount}/{subscription.SubscriptionPlan.MaxCarListing}). " +
                    $"Upgrade to {GetNextTier(subscription.SubscriptionPlan.tier)} for more listings.");
            }

            // 3. Check featured slots if user wants to feature
            if (wantsFeatured)
            {
                var featuredCount = _carListingRepo.CountFeaturedByUserId(userId);
                if (featuredCount >= subscription.SubscriptionPlan.FeatureSlots)
                {
                    return (false,
                        $"❌ You've used all featured slots ({featuredCount}/{subscription.SubscriptionPlan.FeatureSlots}). " +
                        $"Unfeature a car or upgrade your subscription.");
                }
            }

            return (true, "");
        }

        /// <summary>
        /// Gets subscription info for display
        /// </summary>
        public (int currentCount, int maxCount, int featuredCount, int maxFeatured) GetSubscriptionStatus(int userId)
        {
            var subscription = _userSubRepo.GetActiveForUser(userId);

            if (subscription == null)
                return (0, 0, 0, 0);

            var carCount = _carListingRepo.CountActiveListingsByUserId(userId);
            var featuredCount = _carListingRepo.CountFeaturedByUserId(userId);

            return (carCount, subscription.SubscriptionPlan.MaxCarListing,
                    featuredCount, subscription.SubscriptionPlan.FeatureSlots);
        }

        private string GetNextTier(SubscriptionPlanModel.Tiers currentTier)
        {
            return currentTier switch
            {
                SubscriptionPlanModel.Tiers.Free => "Starter",
                SubscriptionPlanModel.Tiers.Starter => "Pro",
                SubscriptionPlanModel.Tiers.Pro => "Elite",
                _ => "Elite"
            };
        }
    }
}