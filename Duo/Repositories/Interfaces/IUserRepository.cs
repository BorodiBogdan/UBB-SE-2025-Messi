using Duo.Models;

namespace Duo.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public int CreateUser(User user);

        public User GetUserById(int id);

        public User GetUserByUsername(string username);
    }
} 