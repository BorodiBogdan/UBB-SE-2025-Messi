using System.Collections.Generic;
using Duo.Models;
using Microsoft.Data.SqlClient;

namespace Duo.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        public List<Category> GetCategories(SqlParameter[] parameters = null);
        public Category GetCategoryByName(string name);
    }
} 