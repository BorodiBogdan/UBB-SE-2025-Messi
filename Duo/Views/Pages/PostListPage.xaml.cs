using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq; // Ensure this is included for LINQ operations
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;

namespace Duo.Views.Pages
{
    // MockPost class to represent a post until we have a real Post class
    public class MockPost
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string> Hashtags { get; set; } = new List<string>();
        public string Username { get; set; } = "u/anonymous"; // Added property for username
        public string Date { get; set; } = "1 year ago"; // Added property for display date
        public int LikeCount { get; set; } = 0; // Added property for like count

        public MockPost()
        {
        }

        public MockPost(string title, string content, params string[] hashtags)
        {
            Title = title;
            Content = content;
            if (hashtags != null)
            {
                Hashtags.AddRange(hashtags);
            }
            
            // Generate random mock data for demo purposes
            Random random = new Random();
            Username = $"u/user{random.Next(100, 999)}";
            
            string[] timeOptions = { "2 hours ago", "5 days ago", "2 weeks ago", "1 month ago", "3 months ago" };
            Date = timeOptions[random.Next(timeOptions.Length)];
            
            LikeCount = random.Next(0, 500);
        }
    }

    public sealed partial class PostListPage : Page
    {
        private ObservableCollection<MockPost> allPosts = new ObservableCollection<MockPost>();
        public ObservableCollection<MockPost> postsFiltered { get; private set; } = new ObservableCollection<MockPost>();
        private HashSet<string> selectedHashtags = new HashSet<string>();
        private Dictionary<string, Button> hashtagButtons = new Dictionary<string, Button>();
        
        // Pagination properties
        private const int ItemsPerPage = 5;
        private List<MockPost> currentFilteredPosts = new List<MockPost>();
        private int totalPages = 1;
        
        // Property to store the category name
        private string category = "Posts";

        public PostListPage()
        {
            this.InitializeComponent();

            // Populate the allPosts collection with some data and hashtags
            allPosts.Add(new MockPost("Technology Trends 2023", "This post discusses the latest technology trends.", "technology", "news"));
            allPosts.Add(new MockPost("Programming Best Practices", "Learn about programming best practices.", "programming", "education"));
            allPosts.Add(new MockPost("UI/UX Design Principles", "A guide to modern UI/UX design principles.", "design", "art"));
            allPosts.Add(new MockPost("Latest Science Discoveries", "Recent breakthroughs in science.", "science", "news"));
            allPosts.Add(new MockPost("Healthy Eating Habits", "Tips for maintaining a healthy diet.", "health", "food"));
            allPosts.Add(new MockPost("Travel Destinations 2025", "Explore the top travel destinations for 2023.", "travel", "vacation"));
            allPosts.Add(new MockPost("Photography Tips", "Improve your photography skills with these tips.", "photography", "art"));
            allPosts.Add(new MockPost("Music Production Techniques", "Learn advanced music production techniques.", "music", "audio"));
            allPosts.Add(new MockPost("Fitness Workouts", "Effective workouts for staying in shape.", "fitness", "health"));
            allPosts.Add(new MockPost("Movie Reviews", "Reviews of the latest movies.", "movies", "entertainment"));
            allPosts.Add(new MockPost("Book Recommendations", "Discover new books to read.", "books", "reading"));
            allPosts.Add(new MockPost("Productivity Hacks", "Boost your productivity with these tips.", "productivity", "work"));
            allPosts.Add(new MockPost("Gaming News", "The latest updates in the gaming world.", "gaming", "entertainment"));
            allPosts.Add(new MockPost("Home Decor Ideas", "Creative ideas for home decor.", "home", "decor"));
            
            // Initialize filtered posts with all posts
            foreach (var post in allPosts)
            {
                postsFiltered.Add(post);
            }
            
            // Load hashtags dynamically from posts
            LoadHashtags();
            
            // Calculate initial pagination
            UpdatePagination(allPosts.ToList());
            
            // Set up pagination
            PostsPager.SelectedIndexChanged += PostsPager_SelectedIndexChanged;
        }
        
        // Override OnNavigatedTo to receive the category name parameter
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Check if we received a category name parameter
            if (e.Parameter is string category && !string.IsNullOrEmpty(category))
            {
                this.category = category;
                
                // Update the page title with the category name
                PageTitle.Text = this.category;
            }
        }
        
        private void LoadHashtags()
        {
            // Clear existing items
            HashtagsContainer.Items.Clear();
            hashtagButtons.Clear();
            selectedHashtags.Clear();

            // Get all distinct hashtags from posts
            HashSet<string> distinctHashtags = new HashSet<string>();
            foreach (var post in allPosts)
            {
                foreach (var hashtag in post.Hashtags)
                {
                    distinctHashtags.Add(hashtag);
                }
            }
            
            // Sort hashtags alphabetically
            var sortedHashtags = distinctHashtags.OrderBy(h => h).ToList();
            
            // Add "All" hashtag at the beginning
            sortedHashtags.Insert(0, "All");
            
            // Create a button for each hashtag
            foreach (var hashtag in sortedHashtags)
            {
                Button button = new Button
                {
                    Content = hashtag == "All" ? "All" : $"#{hashtag}",
                    Tag = hashtag,
                    Style = hashtag == "All" ? 
                        Resources["SelectedHashtagButtonStyle"] as Style : 
                        Resources["HashtagButtonStyle"] as Style
                };
                
                button.Click += Hashtag_Click;
                HashtagsContainer.Items.Add(button);
                hashtagButtons[hashtag] = button;
            }
            
            // Set "All" as the default selected hashtag
            selectedHashtags.Add("All");
        }
        
        private void PostsPager_SelectedIndexChanged(PipsPager sender, PipsPagerSelectedIndexChangedEventArgs args)
        {
            // Load the appropriate page of items
            LoadPageItems(sender.SelectedPageIndex);
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            ApplyFilters();
        }

        private void Hashtag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string hashtag)
            {
                // Special handling for "All" hashtag
                if (hashtag == "All")
                {
                    // If "All" is clicked, deactivate all other hashtags
                    foreach (var tag in selectedHashtags.ToList())
                    {
                        if (tag != "All" && hashtagButtons.TryGetValue(tag, out Button tagButton))
                        {
                            tagButton.Style = Resources["HashtagButtonStyle"] as Style;
                        }
                    }
                    
                    // Clear selected hashtags
                    selectedHashtags.Clear();
                    
                    // Activate "All" hashtag
                    selectedHashtags.Add("All");
                    button.Style = Resources["SelectedHashtagButtonStyle"] as Style;
                }
                else
                {
                    // For other hashtags, toggle their selection
                    if (selectedHashtags.Contains(hashtag))
                    {
                        // Deactivate this hashtag
                        selectedHashtags.Remove(hashtag);
                        button.Style = Resources["HashtagButtonStyle"] as Style;
                        
                        // If no hashtags are selected, activate "All"
                        if (selectedHashtags.Count == 0 && hashtagButtons.TryGetValue("All", out Button allButton))
                        {
                            selectedHashtags.Add("All");
                            allButton.Style = Resources["SelectedHashtagButtonStyle"] as Style;
                        }
                    }
                    else
                    {
                        // Activate this hashtag
                        selectedHashtags.Add(hashtag);
                        button.Style = Resources["SelectedHashtagButtonStyle"] as Style;
                        
                        // Deactivate "All" hashtag
                        if (selectedHashtags.Contains("All") && hashtagButtons.TryGetValue("All", out Button allButton))
                        {
                            selectedHashtags.Remove("All");
                            allButton.Style = Resources["HashtagButtonStyle"] as Style;
                        }
                    }
                }
                
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            var filtered = allPosts.Where(Filter).ToList();
            
            // Update pagination based on filtered results
            UpdatePagination(filtered);
            
            // Load the first page of filtered results
            LoadPageItems(0);
        }

        private bool Filter(MockPost post)
        {
            bool titleMatch = post.Title.Contains(FilterByTitle.Text ?? string.Empty, StringComparison.InvariantCultureIgnoreCase);
            
            bool hashtagMatch = true;
            if (selectedHashtags.Count > 0)
            {
                // If "All" is selected, show all posts (no hashtag filtering)
                if (selectedHashtags.Contains("All"))
                {
                    hashtagMatch = true;
                }
                else
                {
                    // Ensure all selected hashtags are present in the post
                    hashtagMatch = selectedHashtags.All(tag => post.Hashtags.Contains(tag));
                }
            }
            
            return titleMatch && hashtagMatch;
        }

        private void UpdatePagination(List<MockPost> filteredData)
        {
            // Store the current filtered posts
            currentFilteredPosts = filteredData;
            
            // Calculate total pages
            totalPages = (int)Math.Ceiling(filteredData.Count / (double)ItemsPerPage);
            
            // Ensure at least one page
            if (totalPages == 0)
                totalPages = 1;
            
            // Update PipsPager
            PostsPager.NumberOfPages = totalPages;
            
            // Reset to first page
            PostsPager.SelectedPageIndex = 0;
        }

        private void LoadPageItems(int pageIndex)
        {
            // Clear the current items
            postsFiltered.Clear();
            
            // Calculate start and end indices
            int startIndex = pageIndex * ItemsPerPage;
            int endIndex = Math.Min(startIndex + ItemsPerPage, currentFilteredPosts.Count);
            
            // Add items for the current page
            for (int i = startIndex; i < endIndex; i++)
            {
                postsFiltered.Add(currentFilteredPosts[i]);
            }
        }

        private void ClearHashtags_Click(object sender, RoutedEventArgs e)
        {
            // Reset all selected hashtags
            foreach (var tag in selectedHashtags.ToList())
            {
                if (hashtagButtons.TryGetValue(tag, out Button button))
                {
                    button.Style = Resources["HashtagButtonStyle"] as Style;
                }
            }
            
            // Clear the selected hashtags collection
            selectedHashtags.Clear();
            
            // Apply filters to update the list
            ApplyFilters();
        }
    }
}
