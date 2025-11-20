using AutoHaven.Models;

namespace AutoHaven.IRepository
{
    public interface IUserModelRepository
    {
        public List<UserModel> Get();

        public UserModel GetById(int id);
        public UserModel GetByEmail(string email);
        public void Insert(UserModel user);
        public void Delete(int id);
        public void Update(UserModel user);
    }
}
