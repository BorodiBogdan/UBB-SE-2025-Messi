using System;
using System.Collections.Generic;
using System.Windows.Input;
using Duo.Commands;
using Duo.Models;
using Duo.Services;
using Duo.ViewModels.Base;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Duo.Services.Interfaces;

namespace Duo.ViewModels
{
    public class CategoryPageViewModel : ViewModelBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;
        private readonly CategoryViewModel _categoryViewModel;
        
        private int _currentCategoryId = 0;
        private string _currentCategoryName = string.Empty;
        private string _username = "Guest";

        public event EventHandler<Type> NavigationRequested;
        public event EventHandler<string> CategoryNavigationRequested;
        public event EventHandler<bool> PostCreationSucceeded;

        public CategoryPageViewModel()
        {
            _categoryService = App._categoryService;
            _userService = App.userService;
            _categoryViewModel = new CategoryViewModel(_categoryService);
            
            GetUserInfo();
            
            SelectCategoryCommand = new RelayCommandWithParameter<string>(SelectCategory);
            CreatePostCommand = new RelayCommand(CreatePost);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public int CurrentCategoryId
        {
            get => _currentCategoryId;
            set => SetProperty(ref _currentCategoryId, value);
        }

        public string CurrentCategoryName
        {
            get => _currentCategoryName;
            set => SetProperty(ref _currentCategoryName, value);
        }

        public List<string> CategoryNames => _categoryViewModel.GetCategoryNames();

        public ICommand SelectCategoryCommand { get; }
        public ICommand CreatePostCommand { get; }

        private void GetUserInfo()
        {
            try
            {
                User currentUser = _userService.GetCurrentUser();
                Username = currentUser.Username;
            }
            catch (Exception ex)
            {
                Username = "Guest";
                Debug.WriteLine($"Failed to get username: {ex.Message}");
            }
        }

        public void HandleNavigationSelectionChanged(NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (args.SelectedItem is NavigationViewItem selectedItem)
                {
                    var tag = selectedItem.Tag?.ToString();

                    if (string.IsNullOrEmpty(tag))
                        return;

                    if (selectedItem.MenuItems.Count > 0)
                    {
                        return;
                    }

                    if (IsCategoryTag(tag))
                    {
                        // Save the current category name
                        CurrentCategoryName = tag;
                        
                        // Get category ID from name
                        if (!string.IsNullOrEmpty(CurrentCategoryName))
                        {
                            try
                            {
                                var category = _categoryService.GetCategoryByName(CurrentCategoryName);
                                if (category != null)
                                {
                                    CurrentCategoryId = category.Id;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error getting category ID: {ex.Message}");
                            }
                        }
                        
                        CategoryNavigationRequested?.Invoke(this, tag);
                    }
                    else
                    {
                        // Reset the current category when navigating to a non-category page
                        CurrentCategoryName = string.Empty;
                        CurrentCategoryId = 0;
                        
                        Type? pageType = null;
                        switch (tag)
                        {
                            case "MainPage":
                                pageType = typeof(Views.Pages.MainPage);
                                break;
                            default:
                                Debug.WriteLine($"Unknown page tag: {tag}");
                                return;
                        }
                        
                        NavigationRequested?.Invoke(this, pageType);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in navigation selection: {ex.Message}");
            }
        }

        private void SelectCategory(string category)
        {
            if (IsCategoryTag(category))
            {
                CurrentCategoryName = category;
                try
                {
                    var categoryObj = _categoryService.GetCategoryByName(category);
                    if (categoryObj != null)
                    {
                        CurrentCategoryId = categoryObj.Id;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting category ID: {ex.Message}");
                }
                
                CategoryNavigationRequested?.Invoke(this, category);
            }
        }

        private bool IsCategoryTag(string tag)
        {
            return CategoryNames.Contains(tag);
        }

        private void CreatePost()
        {
            // This is just a placeholder as the actual post creation is handled via the dialog
            // The view will handle showing the dialog when this command is executed
        }

        public async void HandlePostCreation(bool success)
        {
            if (success && !string.IsNullOrEmpty(CurrentCategoryName))
            {
                // Notify the view to refresh the content
                PostCreationSucceeded?.Invoke(this, true);
                
                // Navigate back to the category to refresh the posts
                CategoryNavigationRequested?.Invoke(this, CurrentCategoryName);
            }
        }
    }
} 