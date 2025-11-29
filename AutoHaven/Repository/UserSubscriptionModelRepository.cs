//using AutoHaven.IRepository;
//using AutoHaven.Models;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Identity.Client;

//namespace AutoHaven.Repository
//{
//    public class UserSubscriptionModelRepository : IUserSubscriptionModelRepository
//    {
//        ProjectDbContext projectDbcontext;
//        public UserSubscriptionModelRepository(ProjectDbContext _projectDbcontext)
//        {
//            projectDbcontext = _projectDbcontext;
//        }
//        public List<UserSubscriptionModel> Get()
//        {
//            List<UserSubscriptionModel> userssub = projectDbcontext.UserSubscriptions.AsNoTracking().ToList();
//            return userssub;
//        }

//        public UserSubscriptionModel GetById(int id)
//        {
//            UserSubscriptionModel usersub = projectDbcontext.UserSubscriptions.FirstOrDefault(s => s.UserId == id);
//            return usersub;
//        }

//        public UserSubscriptionModel GetActiveForUser(int id)
//        {
//            var now = DateTime.Now;
//            UserSubscriptionModel useractive= projectDbcontext.UserSubscriptions.AsNoTracking().FirstOrDefault(us => us.UserId == id && us.StartDate <= now && us.EndDate >= now);
//            return useractive;
//        }
//        public void Insert(UserSubscriptionModel usersub)
//        {
//            projectDbcontext.UserSubscriptions.Add(usersub);
//            projectDbcontext.SaveChanges();
//        }
//        public void Delete(int id)
//        {
//            UserSubscriptionModel usersub = GetById(id);
//            projectDbcontext.UserSubscriptions.Remove(usersub);
//            projectDbcontext.SaveChanges();
//        }
//        public void Update(UserSubscriptionModel subscription)
//        {
//            var sub = projectDbcontext.UserSubscriptions.FirstOrDefault(us => us.UserSubscriptionId == subscription.UserSubscriptionId);
//            // Update allowed fields only
//            sub.SubscriptionPlanId = subscription.SubscriptionPlanId;
//            sub.StartDate = subscription.StartDate;
//            sub.EndDate = subscription.EndDate;
//            sub.status = subscription.status;
//            sub.billingcycle = subscription.billingcycle;

//            projectDbcontext.SaveChanges();
//        }
//    }
//}
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

        public UserSubscriptionModel GetActiveForUser(int userId)  // ✅ CHANGED: parameter name for clarity
        {
            var now = DateTime.Now;
            return _projectDbContext.UserSubscriptions
                .AsNoTracking()
                .FirstOrDefault(us =>
                    us.UserId == userId &&
                    us.StartDate <= now &&
                    us.EndDate >= now &&
                    us.CurrentStatus == UserSubscriptionModel.Status.Active);
        }

        public void Insert(UserSubscriptionModel subscription)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            subscription.StartDate = DateTime.Now;
            _projectDbContext.UserSubscriptions.Add(subscription);
            _projectDbContext.SaveChanges();
        }
        public void Create(int userId,int planId)
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