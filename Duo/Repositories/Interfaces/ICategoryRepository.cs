using System.Collections.Generic;
using Duo.Models;

namespace Duo.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        /// <summary>
        /// Gets all available categories
        /// </summary>
        /// <returns>A list of all categories</returns>
        List<Category> GetCategories();

        /// <summary>
        /// Gets a category by its name
        /// </summary>
        /// <param name="name">The name of the category to find</param>
        /// <returns>The category with the specified name</returns>
        /// <exception cref="System.Exception">Thrown when the category is not found or an error occurs</exception>
        Category GetCategoryByName(string name);
    }
} 