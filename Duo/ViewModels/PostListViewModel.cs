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

        public PostListViewModel(IPostService postService, ICategoryService? categoryService = null)
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
            try
            {
                _allHashtags.Clear();
                _allHashtags.Add(ALL_HASHTAGS_FILTER);


                foreach (var hashtag in _postService.GetHashtags(_categoryID))
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
            var (posts, totalCount) = _postService.GetFilteredAndFormattedPosts(
                _categoryID,
                _selectedHashtags.ToList(),
                _filterText,
                _currentPage,
                ItemsPerPage);

            Posts.Clear();
            foreach (var post in posts)
            {
                Posts.Add(post);
            }

            _totalPostCount = totalCount;
            TotalPages = Math.Max(DEFAULT_TOTAL_PAGES, (int)Math.Ceiling(_totalPostCount / (double)ItemsPerPage));
            OnPropertyChanged(nameof(TotalPages));
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
            _selectedHashtags = _postService.ToggleHashtagSelection(_selectedHashtags, hashtag, ALL_HASHTAGS_FILTER);
            CurrentPage = DEFAULT_PAGE_NUMBER;
            OnPropertyChanged(nameof(SelectedHashtags));
            LoadPosts();
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
