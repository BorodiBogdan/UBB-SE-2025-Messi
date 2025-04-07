using System;
using System.Windows.Input;
using Duo.Models;
using Duo.Services;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Duo.Commands;
using System.Collections.Generic;
using System.Diagnostics;
using static Duo.App;
using System.Collections.ObjectModel;
using Duo.Views.Components;
using System.Threading.Tasks;
using Duo.Helpers;
using Duo.Services.Interfaces;
using System.Linq;

namespace Duo.ViewModels
{
    /// <summary>
    /// The PostCreationViewModel is responsible for managing the creation and editing of posts.
    /// It provides properties and methods for interacting with the post creation UI,
    /// and handles the communication with the database through the PostService.
    /// 
    /// Features:
    /// - Managing post title and content
    /// - Handling hashtags (add, remove)
    /// - Community selection
    /// - Post creation with validation
    /// - Error handling
    /// </summary>
    public class PostCreationViewModel : INotifyPropertyChanged
    {
        // Constants for validation and defaults
        private const int INVALID_ID = 0;
        private const int DEFAULT_COUNT = 0;
        private const string EMPTY_STRING = "";
        
        // Services
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;

        // Properties
        private string _postTitle = string.Empty;
        private string _postContent = string.Empty;
        private int _selectedCategoryId;
        private ObservableCollection<string> _postHashtags = new ObservableCollection<string>();
        private ObservableCollection<CommunityItem> _postCommunities = new ObservableCollection<CommunityItem>();
        private string _lastError = string.Empty;
        private bool _isLoading;
        private bool _isSuccess;

        // Commands
        public ICommand CreatePostCommand { get; private set; }
        public ICommand AddHashtagCommand { get; private set; }
        public ICommand RemoveHashtagCommand { get; private set; }
        public ICommand SelectCommunityCommand { get; private set; }

        // Property changed event
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler PostCreationSuccessful;


        public string Title
        {
            get => _postTitle;
            set
            {
                _postTitle = value;
                var (isValid, errorMessage) = ValidationHelper.ValidatePostTitle(value);
                LastError = ValidationHelper.UpdateLastErrorInfo(isValid, errorMessage);
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get => _postContent;
            set
            {
                _postContent = value;
                var (isValid, errorMessage) = ValidationHelper.ValidatePostContent(value);
                LastError = ValidationHelper.UpdateLastErrorInfo(isValid, errorMessage);
                OnPropertyChanged();
            }
        }

        public int SelectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                _selectedCategoryId = value;
                OnPropertyChanged();
                UpdateSelectedCommunity();
            }
        }

        public ObservableCollection<string> Hashtags => _postHashtags;

        public ObservableCollection<CommunityItem> Communities => _postCommunities;

        public string LastError
        {
            get => _lastError;
            set
            {
                _lastError = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                _isSuccess = value;
                OnPropertyChanged();
            }
        }


        public PostCreationViewModel()
        {
            // Get services from App
            _postService = _postService ?? App._postService;
            _categoryService = _categoryService ?? App._categoryService;
            _userService = _userService ?? App.userService;

            // Initialize commands
            CreatePostCommand = new RelayCommand(CreatePost);
            AddHashtagCommand = new RelayCommandWithParameter<string>(AddHashtag);
            RemoveHashtagCommand = new RelayCommandWithParameter<string>(RemoveHashtag);
            SelectCommunityCommand = new RelayCommandWithParameter<int>(SelectCommunity);

            // Load initial data
            LoadCommunities();
        }

        #region Public Methods

        public void CreatePost()
        {

            var (isTitleValid, titleError) = ValidationHelper.ValidatePostTitle(Title);
            LastError = ValidationHelper.UpdateLastErrorInfo(isTitleValid, titleError);
            IsLoading = true;
        

            try
            {
                // Get current user ID
                var currentUser = _userService.GetCurrentUser();
                
                // Create a new Post object
                var newPost = new Duo.Models.Post
                {
                    Title = Title,
                    Description = Content,
                    UserID = currentUser.UserId,
                    CategoryID = SelectedCategoryId,
                    CreatedAt = DateTimeHelper.EnsureUtcKind(DateTime.UtcNow),
                    UpdatedAt = DateTimeHelper.EnsureUtcKind(DateTime.UtcNow)
                };
                
                // Create post in database using the original CreatePost method
                int createdPostId = _postService.CreatePost(newPost);
                
                // Add hashtags if any
                if (Hashtags.Count > DEFAULT_COUNT)
                {
                    _postService.AddAllHashtagsToPost(createdPostId,Hashtags.ToArray(), currentUser.UserId);
                }

                // Handle success
                IsSuccess = true;
                PostCreationSuccessful?.Invoke(this, EventArgs.Empty);

                // Clear form
                ClearForm();
            }
            catch (Exception postCreationException)
            {
                Debug.WriteLine($"Error creating post: {postCreationException.Message}");
                LastError = $"Failed to create post: {postCreationException.Message}";
                IsSuccess = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> CreatePostAsync(string title, string content, int categoryId, List<string> hashtags = null)
        {
            // Set properties
            Title = title;
            Content = content;
            SelectedCategoryId = categoryId;
            
            // Additional detailed debugging
            System.Diagnostics.Debug.WriteLine($"CreatePostAsync - START - Title: '{title}', Content length: {content?.Length ?? 0}, CategoryID: {categoryId}");
            System.Diagnostics.Debug.WriteLine($"CreatePostAsync - Received hashtags: {hashtags?.Count ?? 0}");
            if (hashtags != null && hashtags.Count > DEFAULT_COUNT)
            {
                System.Diagnostics.Debug.WriteLine($"CreatePostAsync - Hashtags to add: {string.Join(", ", hashtags)}");
            }
            
            // Process hashtags
            List<string> processedHashtags = new List<string>();
            if (hashtags != null && hashtags.Count > DEFAULT_COUNT)
            {
                foreach (var hashtagText in hashtags)
                {
                    if (!string.IsNullOrWhiteSpace(hashtagText))
                    {
                        processedHashtags.Add(hashtagText.Trim());
                    }
                }
            }
            
            // Create the post
            IsLoading = true;
            LastError = EMPTY_STRING;

            try
            {
                // Get current user ID
                var currentUser = _userService.GetCurrentUser();
                
                // Create a new Post object
                var newPost = new Duo.Models.Post
                {
                    Title = Title,
                    Description = Content,
                    UserID = currentUser.UserId,
                    CategoryID = SelectedCategoryId,
                    CreatedAt = DateTimeHelper.EnsureUtcKind(DateTime.UtcNow),
                    UpdatedAt = DateTimeHelper.EnsureUtcKind(DateTime.UtcNow)
                };
                
                // Create post and add hashtags in a single operation
                int createdPostId = _postService.CreatePostWithHashtags(newPost, processedHashtags, currentUser.UserId);
                
                // Now that we have a valid post ID, update our hashtags collection
                Hashtags.Clear();
                foreach (var hashtagText in processedHashtags)
                {
                    _postHashtags.Add(hashtagText);
                }
                
                // Handle success
                IsSuccess = true;
                PostCreationSuccessful?.Invoke(this, EventArgs.Empty);
                
                return true;
            }
            catch (Exception postCreationException)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating post: {postCreationException.Message}");
                LastError = $"Failed to create post: {postCreationException.Message}";
                IsSuccess = false;
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void AddHashtag(string hashtag)
        {
            if (string.IsNullOrWhiteSpace(hashtag))
            {
                System.Diagnostics.Debug.WriteLine("AddHashtag - Empty hashtag provided, ignoring");
                return;
            }

            string trimmedHashtag = hashtag.Trim();
            
            // Add the hashtag if it doesn't already exist
            if (!Hashtags.Contains(trimmedHashtag))
            {
                // Add directly to the collection
                _postHashtags.Add(trimmedHashtag);
                
                // Debug output
                System.Diagnostics.Debug.WriteLine($"Added hashtag to ViewModel: {trimmedHashtag}, Count now: {Hashtags.Count}");
                
                // Explicitly notify that the Hashtags collection has changed
                OnPropertyChanged(nameof(Hashtags));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Hashtag '{trimmedHashtag}' already exists in collection, not adding duplicate");
            }
        }

        public void RemoveHashtag(string hashtag)
        {
            if (!string.IsNullOrWhiteSpace(hashtag) && Hashtags.Contains(hashtag))
            {
                Hashtags.Remove(hashtag);
                
                // Debug output
                System.Diagnostics.Debug.WriteLine($"Removed hashtag from ViewModel: {hashtag}, Count now: {Hashtags.Count}");
                
                // Explicitly notify that the Hashtags collection has changed
                OnPropertyChanged(nameof(Hashtags));
            }
        }

        public void SelectCommunity(int communityId)
        {
            SelectedCategoryId = communityId;
        }

        public void ClearForm()
        {
            Title = EMPTY_STRING;
            Content = EMPTY_STRING;
            SelectedCategoryId = INVALID_ID;
            Hashtags.Clear();
            UpdateSelectedCommunity();
            LastError = EMPTY_STRING;
            IsSuccess = false;
        }

        #endregion


        private void LoadCommunities()
        {
            try
            {
                var allCategories = _categoryService.GetAllCategories();
                
                Communities.Clear();
                foreach (var category in allCategories)
                {
                    Communities.Add(new CommunityItem
                    {
                        Id = category.Id,
                        Name = category.Name,
                        IsSelected = (category.Id == SelectedCategoryId)
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading communities: {ex.Message}");
                LastError = $"Failed to load communities: {ex.Message}";
            }
        }

        private void UpdateSelectedCommunity()
        {
            foreach (var community in Communities)
            {
                community.IsSelected = (community.Id == SelectedCategoryId);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
