using AutoHaven.Models;

namespace AutoHaven.IRepository
{
    public interface IUserSubscriptionModelRepository
    {
        public List<UserSubscriptionModel> Get();
        public UserSubscriptionModel GetById(int id);
        public UserSubscriptionModel GetActiveForUser(int id);
        public void Insert(UserSubscriptionModel usersub);
        public void Create(int userId, int planId);
        public void Delete(int id);
        public void Update(UserSubscriptionModel subscription);
    }
}
