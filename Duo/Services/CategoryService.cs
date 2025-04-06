using System;
using System.Collections.Generic;
using Duo.Models;
using Duo.Repositories;
using Duo.Repositories.Interfaces;
using Duo.Services.Interfaces;

namespace Duo.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public List<Category> GetAllCategories()
        {
            try
            {
                return _categoryRepository.GetCategories();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching categories: {ex.Message}");
                return new List<Category>(); 
            }
        }

        public Category GetCategoryByName(string name)
        {
            try
            {
                return _categoryRepository.GetCategoryByName(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
