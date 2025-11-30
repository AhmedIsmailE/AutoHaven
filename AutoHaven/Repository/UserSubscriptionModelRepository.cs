using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace AutoHaven.Repository
{
    public class UserSubscriptionModelRepository : IUserSubscriptionModelRepository
    {
        private readonly ProjectDbContext _projectDbContext;

        public UserSubscriptionModelRepository(ProjectDbContext projectDbContext)
        {
            _projectDbContext = projectDbContext;
        }

        public List<UserSubscriptionModel> Get()
        {
            return _projectDbContext.UserSubscriptions
                .Include(us => us.User)
                .AsNoTracking()
                .ToList();
        }

        public UserSubscriptionModel GetById(int id)
        {
            return _projectDbContext.UserSubscriptions
                .Include(us => us.User)
                .FirstOrDefault(us => us.UserSubscriptionId == id);  // ✅ CHANGED: s.UserId to UserSubscriptionId
        }
        public UserSubscriptionModel GetActiveForUser(int userId)
        {
            var now = DateTime.Now;
            return _projectDbContext.UserSubscriptions
                .AsNoTracking()
                .Where(us => us.UserId == userId &&
                             us.StartDate <= now &&
                             us.EndDate >= now &&  // ✅ Check not expired
                             us.CurrentStatus == UserSubscriptionModel.Status.Active)
                .Include(us => us.SubscriptionPlan)
                .OrderByDescending(us => us.SubscriptionPlan.tier) // ✅ Get highest tier
                .FirstOrDefault();
        }

        public void Insert(UserSubscriptionModel subscription)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            subscription.StartDate = DateTime.Now;
            _projectDbContext.UserSubscriptions.Add(subscription);
            _projectDbContext.SaveChanges();
        }
        public void Create(int userId, int planId)
        {
            UserSubscriptionModel subscription = new UserSubscriptionModel();
            subscription.StartDate = DateTime.UtcNow;
            subscription.EndDate = subscription.StartDate.AddDays(30);
            subscription.CurrentStatus = 0;
            subscription.PlanId = planId;
            subscription.SubscriptionPlan = _projectDbContext.SubscriptionPlans.FirstOrDefault(s => s.SubscriptionPlanId == planId);
            subscription.UserId = userId;
            subscription.User = _projectDbContext.Users.FirstOrDefault(u => u.Id == userId);
            _projectDbContext.UserSubscriptions.Add(subscription);
            _projectDbContext.SaveChanges();
        }

        public void Update(UserSubscriptionModel subscription)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            var existing = GetById(subscription.UserSubscriptionId);
            if (existing == null)
                throw new InvalidOperationException($"Subscription with ID {subscription.UserSubscriptionId} not found.");

            existing.SubscriptionPlan.SubscriptionPlanId = subscription.SubscriptionPlan.SubscriptionPlanId;
            existing.StartDate = subscription.StartDate;
            existing.EndDate = subscription.EndDate;
            existing.CurrentStatus = subscription.CurrentStatus;


            _projectDbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var subscription = GetById(id);
            if (subscription != null)
            {
                _projectDbContext.UserSubscriptions.Remove(subscription);
                _projectDbContext.SaveChanges();
            }
        }
    }
}