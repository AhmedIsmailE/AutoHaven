using AutoHaven.IRepository;
using AutoHaven.Models;
using Microsoft.EntityFrameworkCore;
namespace AutoHaven.Repository
{
    public class UserModelRepository : IUserModelRepository
    {
        ProjectDbContext projectDbcontext;
        public UserModelRepository(ProjectDbContext _projectDbcontext)
        {
            projectDbcontext = _projectDbcontext;
        }
        public List<UserModel> Get()
        {
            List<UserModel> users = projectDbcontext.Users.AsNoTracking().ToList();
            return users;
        }

        public UserModel GetById(int id)
        {
            UserModel user = projectDbcontext.Users.FirstOrDefault(s => s.UserId == id);
            return user;
        }
        public UserModel GetByEmail (string email)
        {
            UserModel user = projectDbcontext.Users.FirstOrDefault(s => s.Email == email);
            return user;
        }
        public void Insert(UserModel user)
        {
            projectDbcontext.Users.Add(user);
            projectDbcontext.SaveChanges();
        }
        public void Delete(int id)
        {
            UserModel user = GetById(id);
            projectDbcontext.Users.Remove(user);
            projectDbcontext.SaveChanges();
        }
        public void Update(UserModel user)
        {
            UserModel usr = GetById(user.UserId);
            // Update allowed fields
            usr.UserName = user.UserName;
            usr.Email = user.Email;
            usr.Name = user.Name;
            usr.CompanyName = user.CompanyName;
            usr.Phone = user.Phone;
            usr.Street = user.Street;
            usr.City = user.City;
            usr.State = user.State;
            usr.role = user.role; //For roles update
            usr.UpdatedAt = DateTime.UtcNow;
            // Update password only if a new non-empty password is provided.
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                usr.Password = user.Password;
            }
            projectDbcontext.SaveChanges();
        }
    }
}
