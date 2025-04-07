using System;
using System.Collections.Generic;
using Duo.Models;

namespace Duo.Services.Interfaces
{
    public interface ICategoryService
    {
        List<Category> GetAllCategories();
        Category GetCategoryByName(string name);
    }
}
