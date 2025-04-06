using System;
using Duo.Models;

namespace Duo.Services.Interfaces
{
    public interface IUserService
    {
        public void setUser(string username);
        public User GetCurrentUser();
        public User GetUserById(int id);
        public User GetUserByUsername(string username);
    }
} 