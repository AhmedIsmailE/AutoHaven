using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.EntityFrameworkCore;
namespace AutoHaven.Repository
{
    public class SubscriptionPlanModelRepository : ISubscriptionPlanModelRepository
    {
        ProjectDbContext projectDbcontext;
        public SubscriptionPlanModelRepository(ProjectDbContext _projectDbcontext)
        {
            projectDbcontext = _projectDbcontext;
        }
        public List<SubscriptionPlanModel> Get()
        {
            List<SubscriptionPlanModel> sublists = projectDbcontext.SubscriptionPlans.AsNoTracking().ToList();
            return sublists;
        }

        public SubscriptionPlanModel GetById(int id)
        {
            SubscriptionPlanModel model = projectDbcontext.SubscriptionPlans.FirstOrDefault(s => s.SubscriptionPlanId == id);
            return model;
        }

        public void Insert(SubscriptionPlanModel sublist)
        {
            projectDbcontext.SubscriptionPlans.Add(sublist);
            projectDbcontext.SaveChanges();
        }
        public void Delete(int id)
        {
            SubscriptionPlanModel sublist = GetById(id);
            projectDbcontext.SubscriptionPlans.Remove(sublist);
            projectDbcontext.SaveChanges();
        }
        public void Update(SubscriptionPlanModel sublist)
        {
            SubscriptionPlanModel sub = GetById(sublist.SubscriptionPlanId);
            // Update allowed fields
            sub.SubscriptionName = sublist.SubscriptionName;
            sub.MaxCarListing = sublist.MaxCarListing;
            sub.FeatureSlots = sublist.FeatureSlots;
            sub.PricePerMonth = sublist.PricePerMonth;
            sub.tier = sublist.tier;
            projectDbcontext.SaveChanges();
        }
    }
}
