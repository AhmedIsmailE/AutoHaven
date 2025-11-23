using AutoHaven.Models;

namespace AutoHaven.IRepository
{
    public interface IUserModelRepository
    {
        public List<ApplicationUser> Get();

        public ApplicationUser GetById(int id);
        public ApplicationUser GetByEmail(string email);
        public void Insert(ApplicationUser user);
        public void Delete(int id);
        public void Update(ApplicationUser user);
    }
}
