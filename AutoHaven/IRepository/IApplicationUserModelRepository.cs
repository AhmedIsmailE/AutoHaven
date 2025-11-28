using AutoHaven.Models;

namespace AutoHaven.IRepository
{
    public interface IApplicationUserModelRepository
    {
        public List<ApplicationUserModel> Get();

        public ApplicationUserModel GetById(int id);
        public ApplicationUserModel GetByEmail(string email);
        public void Insert(ApplicationUserModel user);
        public void Delete(int id);
        public void Update(ApplicationUserModel user);
    }
}
