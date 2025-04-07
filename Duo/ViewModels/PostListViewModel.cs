using System;
using System.Windows.Input;
using Duo.Models;
using Duo.Services;
using Duo.Commands;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Duo.Services.Interfaces;

namespace Duo.ViewModels
{
    public class PostListViewModel : INotifyPropertyChanged
    {
        private readonly IPostService _postService;
        
        // Constants for validation and defaults
        private const int INVALID_ID = 0;
        private const int DEFAULT_ITEMS_PER_PAGE = 5;
        private const int DEFAULT_PAGE_NUMBER = 1;
        private const int DEFAULT_TOTAL_PAGES = 1;
        private const int DEFAULT_COUNT = 0;
        private const string ALL_HASHTAGS_FILTER = "All";

        
        private int? _categoryID;
        private string _categoryName;
        private string _filterText;
        private ObservableCollection<Post> _posts;
        private int _currentPage;
        private HashSet<string> _selectedHashtags = new HashSet<string>();
        private const int ItemsPerPage = 5;
        private int _totalPages = 1;
        private List<string> _allHashtags = new List<string>();
        private int _totalPostCount = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public PostListViewModel(IPostService? postService = null, ICategoryService? categoryService = null)
        {
            _postService = postService ?? App._postService;
            _posts = new ObservableCollection<Post>();
            _currentPage = DEFAULT_PAGE_NUMBER;
            _selectedHashtags.Add(ALL_HASHTAGS_FILTER);

            LoadPostsCommand = new RelayCommand(LoadPosts);
            NextPageCommand = new RelayCommand(NextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage);
            FilterPostsCommand = new RelayCommand(FilterPosts);
            ClearFiltersCommand = new RelayCommand(ClearFilters);

            LoadAllHashtags();
        }

        public ObservableCollection<Post> Posts
        {
            get => _posts;
            set
            {
                _posts = value;
                OnPropertyChanged();
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged();
                FilterPosts();
            }
        }

        public int CategoryID
        {
            get => _categoryID ?? INVALID_ID;
            set
            {
                _categoryID = value;
                OnPropertyChanged();

                LoadAllHashtags();
            }
        }

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged();
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged();
            }
        }

        public HashSet<string> SelectedHashtags => _selectedHashtags;

        public List<string> AllHashtags 
        {
            get => _allHashtags;
            set
            {
                _allHashtags = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadPostsCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand FilterPostsCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        private void LoadAllHashtags()
        {
            if (_postService == null) return;

            try
            {
                _allHashtags.Clear();
                _allHashtags.Add(ALL_HASHTAGS_FILTER);

                List<Hashtag> hashtags;

                if (_categoryID.HasValue && _categoryID.Value > INVALID_ID)
                {
                    hashtags = _postService.GetHashtagsByCategory(_categoryID.Value);
                }
                else
                {
                    hashtags = _postService.GetAllHashtags();
                }

                foreach (var hashtag in hashtags)
                {
                    if (!_allHashtags.Contains(hashtag.Name))
                    {
                        _allHashtags.Add(hashtag.Name);
                    }
                }

                OnPropertyChanged(nameof(AllHashtags));
            }
            catch (Exception ex)
            {

            }
        }

        public void LoadPosts()
        {
            if (_postService == null) return;

            try
            {
                IEnumerable<Post> filteredPosts;

                int pageOffset = (CurrentPage - DEFAULT_PAGE_NUMBER) * ItemsPerPage;

                if (_selectedHashtags.Count > DEFAULT_COUNT && !_selectedHashtags.Contains(ALL_HASHTAGS_FILTER))
                {
                    filteredPosts = _postService.GetPostsByHashtags(_selectedHashtags.ToList(), CurrentPage, ItemsPerPage);
                    _totalPostCount = _postService.GetPostCountByHashtags(_selectedHashtags.ToList());
                }
                else if (_categoryID.HasValue && _categoryID.Value > INVALID_ID)
                {
                    if (!string.IsNullOrEmpty(FilterText))
                    {
                        filteredPosts = _postService.GetPostsByCategory(CategoryID, DEFAULT_PAGE_NUMBER, int.MaxValue);
                    }
                    else
                    {
                        filteredPosts = _postService.GetPostsByCategory(CategoryID, CurrentPage, ItemsPerPage);
                    }

                    _totalPostCount = _postService.GetPostCountByCategoryId(CategoryID);

                }
                else
                {
                    if (!string.IsNullOrEmpty(FilterText))
                    {
                        filteredPosts = _postService.GetPaginatedPosts(DEFAULT_PAGE_NUMBER, int.MaxValue);
                    }
                    else
                    {
                        filteredPosts = _postService.GetPaginatedPosts(CurrentPage, ItemsPerPage);
                    }

                    _totalPostCount = _postService.GetTotalPostCount();
                }

                if (!string.IsNullOrEmpty(FilterText))
                {
                    var searchResults = new List<Post>();
                    foreach (var post in filteredPosts)
                    {

                        if (App._searchService.FindFuzzySearchMatches(FilterText, new[] { post.Title }).Any())
                        {
                            searchResults.Add(post);
                        }
                    }

                    _totalPostCount = searchResults.Count;

                    filteredPosts = searchResults
                        .Skip((CurrentPage - DEFAULT_PAGE_NUMBER) * ItemsPerPage)
                        .Take(ItemsPerPage);
                }

                Posts.Clear();

                foreach (var post in filteredPosts)
                {

                    if (string.IsNullOrEmpty(post.Username))
                    {
                        var postAuthor = App.userService.GetUserById(post.UserID);
                        post.Username = postAuthor?.Username ?? "Unknown User";
                    }
                    
                    DateTime localCreatedAt = Helpers.DateTimeHelper.ConvertUtcToLocal(post.CreatedAt);
                    post.Date = Helpers.DateTimeHelper.GetRelativeTime(localCreatedAt);
                    
                    post.Hashtags.Clear();
                    try 
                    {
                        var postHashtags = _postService.GetHashtagsByPostId(post.Id);
                        foreach (var hashtag in postHashtags)
                        {
                            post.Hashtags.Add(hashtag.Name);
                        }
                    }
                    catch
                    {
                    }

                    Posts.Add(post);
                }

                TotalPages = Math.Max(DEFAULT_TOTAL_PAGES, (int)Math.Ceiling(_totalPostCount / (double)ItemsPerPage));

                OnPropertyChanged(nameof(TotalPages));
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                LoadPosts();
            }
        }

        private void PreviousPage()
        {
            if (CurrentPage > DEFAULT_PAGE_NUMBER)
            {
                CurrentPage--;
                LoadPosts();
            }
        }

        public void ToggleHashtag(string hashtag)
        {
            if (string.IsNullOrEmpty(hashtag)) return;

            try
            {
                if (hashtag == ALL_HASHTAGS_FILTER)
                {
                    _selectedHashtags.Clear();
                    _selectedHashtags.Add(ALL_HASHTAGS_FILTER);
                }
                else
                {
                    if (_selectedHashtags.Contains(hashtag))
                    {
                        _selectedHashtags.Remove(hashtag);

                        if (_selectedHashtags.Count == DEFAULT_COUNT)
                        {
                            _selectedHashtags.Add(ALL_HASHTAGS_FILTER);
                        }
                    }
                    else
                    {
                        _selectedHashtags.Add(hashtag);

                        if (_selectedHashtags.Contains(ALL_HASHTAGS_FILTER))
                        {
                            _selectedHashtags.Remove(ALL_HASHTAGS_FILTER);
                        }
                    }
                }

                CurrentPage = DEFAULT_PAGE_NUMBER;

                OnPropertyChanged(nameof(SelectedHashtags));
                LoadPosts();
            }
            catch (Exception ex)
            {
            }
        }

        public void FilterPosts()
        {
            CurrentPage = DEFAULT_PAGE_NUMBER;
            LoadPosts();
        }

        public void ClearFilters()
        {
            FilterText = string.Empty;
            _selectedHashtags.Clear();
            _selectedHashtags.Add(ALL_HASHTAGS_FILTER);
            CurrentPage = DEFAULT_PAGE_NUMBER;
            LoadPosts();
            OnPropertyChanged(nameof(SelectedHashtags));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
