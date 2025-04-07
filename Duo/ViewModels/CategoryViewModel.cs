using System;
using System.Windows.Input;
using Duo.Models;
using Duo.Services;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Duo.Commands;
using System.Collections.Generic;
using Duo.Services.Interfaces;

namespace Duo.ViewModels
{
    public class CategoryViewModel : INotifyPropertyChanged
    {
        private readonly ICategoryService _categoryService;
        private string _categoryName = string.Empty;
        private List<Category> _categories = new List<Category>();

        public event PropertyChangedEventHandler PropertyChanged;

        public List<Category> Categories
        {
            get => _categories;
            set
            {
                if (_categories != value)
                {
                    _categories = value;
                    OnPropertyChanged();
                }
            }
        }

        public CategoryViewModel(ICategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            LoadCategories();
        }

        public void LoadCategories()
        {
                Categories = _categoryService.GetAllCategories();
        }

        public List<string> GetCategoryNames()
        {
            List<string> categoryNames = new List<string>();
            foreach (var category in Categories)
            {
                categoryNames.Add(category.Name);
            }
            return categoryNames;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
