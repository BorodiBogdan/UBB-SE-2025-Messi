using Duo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
