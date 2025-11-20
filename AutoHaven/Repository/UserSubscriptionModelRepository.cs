using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace AutoHaven.Repository
{
    public class UserSubscriptionModelRepository : IUserSubscriptionModelRepository
    {
        ProjectDbContext projectDbcontext;
        public UserSubscriptionModelRepository(ProjectDbContext _projectDbcontext)
        {
            projectDbcontext = _projectDbcontext;
        }
        public List<UserSubscriptionModel> Get()
        {
            List<UserSubscriptionModel> userssub = projectDbcontext.UserSubscriptions.AsNoTracking().ToList();
            return userssub;
        }

        public UserSubscriptionModel GetById(int id)
        {
            UserSubscriptionModel usersub = projectDbcontext.UserSubscriptions.FirstOrDefault(s => s.UserId == id);
            return usersub;
        }

        public UserSubscriptionModel GetActiveForUser(int id)
        {
            var now = DateTime.UtcNow;
            UserSubscriptionModel useractive= projectDbcontext.UserSubscriptions.AsNoTracking().FirstOrDefault(us => us.UserId == id && us.StartDate <= now && us.EndDate >= now);
            return useractive;
        }
        public void Insert(UserSubscriptionModel usersub)
        {
            projectDbcontext.UserSubscriptions.Add(usersub);
            projectDbcontext.SaveChanges();
        }
        public void Delete(int id)
        {
            UserSubscriptionModel usersub = GetById(id);
            projectDbcontext.UserSubscriptions.Remove(usersub);
            projectDbcontext.SaveChanges();
        }
        public void Update(UserSubscriptionModel subscription)
        {
            var sub = projectDbcontext.UserSubscriptions.FirstOrDefault(us => us.UserSubscriptionId == subscription.UserSubscriptionId);
            // Update allowed fields only
            sub.SubscriptionPlanId = subscription.SubscriptionPlanId;
            sub.StartDate = subscription.StartDate;
            sub.EndDate = subscription.EndDate;
            sub.status = subscription.status;
            sub.billingcycle = subscription.billingcycle;

            projectDbcontext.SaveChanges();
        }
    }
}
