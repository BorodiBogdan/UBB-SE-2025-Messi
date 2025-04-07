using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Duo.Commands;
using Duo.Models;
using Duo.Services;
using Duo.ViewModels.Base;
using Duo.Views.Components;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using static Duo.App;

namespace Duo.ViewModels
{
    public class PostDetailViewModel : ViewModelBase
    {
        private readonly PostService _postService;
        private readonly CommentService _commentService;
        private readonly UserService _userService;
        private Models.Post _post;
        private ObservableCollection<CommentViewModel> _commentViewModels;
        private ObservableCollection<Models.Comment> _comments;
        private CommentCreationViewModel _commentCreationViewModel;
        private bool _isLoading;
        private bool _hasComments;
        private string _errorMessage;
        private object _commentsPanel;
        private string _lastProcessedReply;

        public static Dictionary<int, bool> CollapsedComments { get; } = new Dictionary<int, bool>();

        public event EventHandler CommentsLoaded;

        public object CommentsPanel
        {
            get => _commentsPanel;
            set => SetProperty(ref _commentsPanel, value);
        }

        public PostDetailViewModel()
        {
            _postService = _postService ?? App._postService;
            _commentService = _commentService ?? new CommentService(_commentRepository, _postRepository, userService);
            _userService = _userService ?? App.userService;

            _post = new Models.Post { 
                Title = "",
                Description = "",
                Hashtags = new List<string>()
            };
            _comments = new ObservableCollection<Models.Comment>();
            _commentViewModels = new ObservableCollection<CommentViewModel>();
            _commentCreationViewModel = new CommentCreationViewModel();
            _commentCreationViewModel.CommentSubmitted += CommentCreationViewModel_CommentSubmitted;

            LoadPostDetailsCommand = new RelayCommandWithParameter<int>(LoadPostDetails);
            AddCommentCommand = new RelayCommandWithParameter<string>(AddComment);
            AddReplyCommand = new RelayCommandWithParameter<Tuple<int, string>>(AddReply);
            BackCommand = new RelayCommand(GoBack);
        }

        public Models.Post Post
        {
            get => _post;
            set => SetProperty(ref _post, value);
        }

        public ObservableCollection<Models.Comment> Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        public ObservableCollection<CommentViewModel> CommentViewModels
        {
            get => _commentViewModels;
            set => SetProperty(ref _commentViewModels, value);
        }

        public CommentCreationViewModel CommentCreationViewModel
        {
            get => _commentCreationViewModel;
            set => SetProperty(ref _commentCreationViewModel, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasComments
        {
            get => _hasComments;
            set => SetProperty(ref _hasComments, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoadPostDetailsCommand { get; private set; }
        public ICommand AddCommentCommand { get; private set; }
        public ICommand AddReplyCommand { get; private set; }
        public ICommand BackCommand { get; private set; }

        private void GoBack()
        {
            // This is a placeholder - actual navigation will be handled in the view
        }

        private void CommentCreationViewModel_CommentSubmitted(object sender, EventArgs e)
        {
            if (sender is CommentCreationViewModel viewModel && !string.IsNullOrWhiteSpace(viewModel.CommentText))
            {
                AddComment(viewModel.CommentText);
                viewModel.ClearComment();
            }
        }

        public void LoadPostDetails(int postId)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                if (postId <= 0)
                {
                    throw new ArgumentException("Invalid post ID", nameof(postId));
                }

                if (Post == null)
                {
                    Post = new Models.Post { 
                        Title = "",
                        Description = "",
                        Hashtags = new List<string>()
                    };
                }

                var post = _postService.GetPostById(postId);
                if (post != null)
                {
                    if (post.Id <= 0)
                    {
                        post.Id = postId;
                    }

                    if (post.Hashtags == null)
                    {
                        post.Hashtags = new List<string>();
                    }

                    try 
                    {
                        var user = _userService.GetUserById(post.UserID);
                        post.Username = $"{user?.Username ?? "Unknown User"}";
                    }
                    catch (Exception userEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting user: {userEx.Message}");
                        post.Username = "Unknown User";
                    }

                    try
                    {
                        if (string.IsNullOrEmpty(post.Date) && post.CreatedAt != default)
                        {
                            DateTime localCreatedAt = Helpers.DateTimeHelper.ConvertUtcToLocal(post.CreatedAt);
                            post.Date = FormatDate(localCreatedAt);
                        }
                    }
                    catch (Exception dateEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error formatting date: {dateEx.Message}");
                        post.Date = "Unknown date";
                    }

                    try 
                    {
                        var hashtags = _postService.GetHashtagsByPostId(post.Id);
                        if (hashtags != null && hashtags.Any())
                        {
                            post.Hashtags = hashtags.Select(h => h.Name ?? h.Tag).ToList();
                        }
                    }
                    catch (Exception hashtagEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading hashtags: {hashtagEx.Message}");
                        post.Hashtags = new List<string>();
                    }

                    try
                    {
                        Post = post;
                        LoadComments(post.Id);
                    }
                    catch (Exception uiEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating UI: {uiEx.Message}");
                        ErrorMessage = "Error displaying post details";
                    }
                }
                else
                {
                    ErrorMessage = "Post not found";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading post details: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadPostDetails error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void LoadComments(int postId)
        {
            try
            {
                if (postId <= 0)
                {
                    throw new ArgumentException("Invalid post ID", nameof(postId));
                }

                var comments = _commentService.GetCommentsByPostId(postId);

                Comments.Clear();
                CommentViewModels.Clear();

                if (comments != null && comments.Any())
                {
                    HasComments = true;

                    foreach (var comment in comments)
                    {
                        Comments.Add(comment);
                    }

                    var topLevelComments = comments.Where(c => c.ParentCommentId == null).ToList();
                    
                    var repliesByParentId = comments
                                        .Where(c => c.ParentCommentId.HasValue)
                                        .GroupBy(c => c.ParentCommentId.Value)
                                        .ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var comment in topLevelComments)
                    {
                        comment.Level = 1;
                    }

                    foreach (var parentId in repliesByParentId.Keys)
                    {
                        var parentComment = comments.FirstOrDefault(c => c.Id == parentId);
                        if (parentComment != null)
                        {
                            foreach (var reply in repliesByParentId[parentId])
                            {
                                reply.Level = parentComment.Level + 1;
                            }
                        }
                    }

                    foreach (var comment in topLevelComments)
                    {
                        var commentViewModel = new CommentViewModel(comment, repliesByParentId);
                        
                        if (CollapsedComments.TryGetValue(comment.Id, out bool isCollapsed))
                        {
                            commentViewModel.IsExpanded = !isCollapsed;
                        }
                        
                        CommentViewModels.Add(commentViewModel);
                    }

                    CommentsLoaded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    HasComments = false;
                }
            }
            catch (Exception ex)
            {
                HasComments = false;
                ErrorMessage = $"Error loading comments: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadComments error: {ex.Message}");
            }
        }

        private void AddComment(string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText) || Post == null || Post.Id <= 0)
                return;

            try
            {
                _commentService.CreateComment(commentText, Post.Id, null);
                LoadComments(Post.Id);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding comment: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"AddComment error: {ex.Message}");
            }
        }

        private void AddReply(Tuple<int, string> data)
        {
            if (data == null)
                return;
                
            AddReplyToComment(data.Item1, data.Item2);
        }

        public void DeleteComment(int commentId)
         {
             if (commentId <= 0 || Post == null || Post.Id <= 0)
                 return;

             try
             {
                 User currentUser = userService.GetCurrentUser();
                 if (currentUser != null)
                 {
                     bool success = _commentService.DeleteComment(commentId, currentUser.UserId);
                     if (success)
                     {
                         if (CollapsedComments.ContainsKey(commentId))
                         {
                             CollapsedComments.Remove(commentId);
                         }

                         LoadComments(Post.Id);
                     }
                 }
             }
             catch (Exception ex)
             {
                 System.Diagnostics.Debug.WriteLine($"Error deleting comment: {ex.Message}");
             }
         }

        public void AddReplyToComment(int parentCommentId, string replyText)
        {
            if (string.IsNullOrWhiteSpace(replyText) || Post == null || Post.Id <= 0 || parentCommentId <= 0)
                return;

            try
            {
                string replySignature = $"{parentCommentId}_{replyText}";

                // Check for duplicate comments
                bool isDuplicate = false;
                foreach (var comment in Comments)
                {
                    if (comment.ParentCommentId == parentCommentId && 
                        comment.Content.Equals(replyText, StringComparison.OrdinalIgnoreCase))
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (_lastProcessedReply == replySignature)
                {
                    isDuplicate = true;
                }

                if (isDuplicate)
                {
                    return;
                }

                _lastProcessedReply = replySignature;

                _commentService.CreateComment(replyText, Post.Id, parentCommentId);
                LoadComments(Post.Id);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding reply: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"AddReply error: {ex.Message}");
            }
        }

        private string FormatDate(DateTime date)
        {
            // This method assumes date is already in local time
            return date.ToString("MMM dd, yyyy HH:mm");
        }

        public CommentViewModel FindCommentById(int commentId)
        {
            // First, check top-level comments
            var comment = CommentViewModels.FirstOrDefault(c => c.Id == commentId);
            if (comment != null)
            {
                return comment;
            }
            
            // If not found in top-level, recursively search through replies
            foreach (var topLevelComment in CommentViewModels)
            {
                var foundInReplies = FindCommentInReplies(topLevelComment.Replies, commentId);
                if (foundInReplies != null)
                {
                    return foundInReplies;
                }
            }
            
            return null;
        }

        public void LikeCommentById(int commentId)
        {
            try
            {
                // Call the service to persist the like to the database
                bool success = _commentService.LikeComment(commentId);
                
                if (success)
                {
                    // Update the UI via the view model
                    var commentViewModel = FindCommentById(commentId);
                    if (commentViewModel != null)
                    {
                        commentViewModel.LikeComment();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to like comment: {ex.Message}";
            }
        }

        private CommentViewModel FindCommentInReplies(IEnumerable<CommentViewModel> replies, int commentId)
        {
            foreach (var reply in replies)
            {
                if (reply.Id == commentId)
                    return reply;
                    
                var foundInNestedReplies = FindCommentInReplies(reply.Replies, commentId);
                if (foundInNestedReplies != null)
                    return foundInNestedReplies;
            }
            
            return null;
        }
    }
} 