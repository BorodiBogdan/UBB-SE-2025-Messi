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
using Duo.Services.Interfaces;

namespace Duo.ViewModels
{
    public class PostDetailViewModel : ViewModelBase
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;
        private Models.Post _post;
        private ObservableCollection<CommentViewModel> _commentViewModels;
        private ObservableCollection<Models.Comment> _comments;
        private CommentCreationViewModel _commentCreationViewModel;
        private bool _isLoading;
        private bool _hasComments;
        private string _errorMessage;
        private object _commentsPanel;
        private string _lastProcessedReply;

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

                // Use the service method that encapsulates all the business logic
                var post = _postService.GetPostDetailsWithMetadata(postId);
                
                if (post != null)
                {
                    Post = post;
                    LoadComments(post.Id);
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

                // Use service method to get pre-processed comments
                var (allComments, topLevelComments, repliesByParentId) = _commentService.GetProcessedCommentsByPostId(postId);

                Comments.Clear();
                CommentViewModels.Clear();

                if (allComments != null && allComments.Any())
                {
                    HasComments = true;

                    // Add all comments to the Comments collection
                    foreach (var comment in allComments)
                    {
                        Comments.Add(comment);
                    }

                    // Create view models for top-level comments only
                    foreach (var comment in topLevelComments)
                    {
                        var commentViewModel = new CommentViewModel(comment, repliesByParentId);
                       
                        
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

             try
             {
                User currentUser = userService.GetCurrentUser();

                bool success = _commentService.DeleteComment(commentId, currentUser.UserId);
                if (success)
                {
                    LoadComments(Post.Id);
                }                
             }
             catch (Exception ex)
             {
                 System.Diagnostics.Debug.WriteLine($"Error deleting comment: {ex.Message}");
             }
         }

        public void AddReplyToComment(int parentCommentId, string replyText)
        {
            try
            {
                var (success, newReplySignature) = _commentService.CreateReplyWithDuplicateCheck(
                    replyText,
                    Post?.Id ?? 0,
                    parentCommentId,
                    Comments,
                    _lastProcessedReply);
                    _lastProcessedReply = newReplySignature;
                    LoadComments(Post.Id);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding reply: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"AddReply error: {ex.Message}");
            }
        }

        public CommentViewModel FindCommentById(int commentId)
        {
            return _commentService.FindCommentInHierarchy<CommentViewModel>(
                commentId,
                CommentViewModels,
                c => c.Replies,
                c => c.Id);
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
    }
} 