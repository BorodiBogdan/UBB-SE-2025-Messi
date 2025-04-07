using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Duo.Commands;
using Duo.Helpers;
using Duo.ViewModels.Base;
using Duo.Services;
using static Duo.App;
using Duo.Data;
using Duo.Models;

namespace Duo.ViewModels
{
    public class CommentViewModel : ViewModelBase
    {
        private CommentService _commentService;
        private Models.Comment _comment;
        private ObservableCollection<CommentViewModel> _replies;
        private bool _isExpanded = true;
        private string _replyText;
        private bool _isReplyVisible;
        private int _likeCount;
        private bool _isDeleteButtonVisible;
        private bool _isReplyButtonVisible;
        private bool _isToggleButtonVisible;
        private string _toggleIconGlyph = "\uE109"; // Plus icon by default
        private const int MAX_NESTING_LEVEL = 3;

        public CommentViewModel(Comment comment, Dictionary<int, List<Comment>> repliesByParentId)
        {
            _comment = comment ?? throw new ArgumentNullException(nameof(comment));
            _replies = new ObservableCollection<CommentViewModel>();
            _likeCount = comment.LikeCount;
            
            // Load any child comments/replies
            if (repliesByParentId != null && repliesByParentId.TryGetValue(comment.Id, out var childComments))
            {
                foreach (var reply in childComments)
                {
                    _replies.Add(new CommentViewModel(reply, repliesByParentId));
                }
            }

            // Initialize visibility states
            UpdateVisibilityStates();
            
            ToggleRepliesCommand = new RelayCommand(ToggleReplies);
            ShowReplyFormCommand = new RelayCommand(ShowReplyForm);
            CancelReplyCommand = new RelayCommand(CancelReply);
            SubmitReplyCommand = new RelayCommand(SubmitReply);
            LikeCommentCommand = new RelayCommand(OnLikeComment);
            DeleteCommentCommand = new RelayCommand(DeleteComment);
        }

        public int Id => _comment.Id;
        public int UserId => _comment.UserId;
        public int? ParentCommentId => _comment.ParentCommentId;
        public string Content => _comment.Content;
        public string Username => _comment.Username;
        public string Date => DateTimeHelper.FormatDate(_comment.CreatedAt);
        public int Level => _comment.Level;
        
        public int LikeCount
        {
            get => _likeCount;
            set => SetProperty(ref _likeCount, value);
        }

        public ObservableCollection<CommentViewModel> Replies
        {
            get => _replies;
            set => SetProperty(ref _replies, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value))
                {
                    ToggleIconGlyph = value ? "\uE108" : "\uE109";
                    PostDetailViewModel.CollapsedComments[Id] = !value;
                }
            }
        }

        public string ReplyText
        {
            get => _replyText;
            set => SetProperty(ref _replyText, value);
        }

        public bool IsReplyVisible
        {
            get => _isReplyVisible;
            set => SetProperty(ref _isReplyVisible, value);
        }

        public bool IsDeleteButtonVisible
        {
            get => _isDeleteButtonVisible;
            private set => SetProperty(ref _isDeleteButtonVisible, value);
        }

        public bool IsReplyButtonVisible
        {
            get => _isReplyButtonVisible;
            private set => SetProperty(ref _isReplyButtonVisible, value);
        }

        public bool IsToggleButtonVisible
        {
            get => _isToggleButtonVisible;
            private set => SetProperty(ref _isToggleButtonVisible, value);
        }

        public string ToggleIconGlyph
        {
            get => _toggleIconGlyph;
            private set => SetProperty(ref _toggleIconGlyph, value);
        }
        public void LikeComment()
        {
            _comment.IncrementLikeCount();
            LikeCount = _comment.LikeCount;
        }

        public ICommand ToggleRepliesCommand { get; }
        public ICommand ShowReplyFormCommand { get; }
        public ICommand CancelReplyCommand { get; }
        public ICommand SubmitReplyCommand { get; }
        public ICommand LikeCommentCommand { get; }
        public ICommand DeleteCommentCommand { get; }

        // Events
        public event EventHandler<Tuple<int, string>> ReplySubmitted;
        public event EventHandler<int> CommentLiked;
        public event EventHandler<int> CommentDeleted;

        private void UpdateVisibilityStates()
        {
            // Update reply button visibility based on nesting level
            IsReplyButtonVisible = Level < MAX_NESTING_LEVEL;

            // Update toggle button visibility based on replies
            IsToggleButtonVisible = Replies != null && Replies.Count > 0;

            // Update delete button visibility based on user ownership
            try
            {
                var currentUser = userService.GetCurrentUser();
                IsDeleteButtonVisible = currentUser != null && currentUser.UserId == UserId;
            }
            catch (Exception)
            {
                IsDeleteButtonVisible = false;
            }
        }

        private void ToggleReplies()
        {
            IsExpanded = !IsExpanded;
        }

        private void ShowReplyForm()
        {
            IsReplyVisible = true;
            ReplyText = string.Empty;
        }

        private void CancelReply()
        {
            IsReplyVisible = false;
            ReplyText = string.Empty;
        }

        private void SubmitReply()
        {
            if (!string.IsNullOrWhiteSpace(ReplyText))
            {
                ReplySubmitted?.Invoke(this, new Tuple<int, string>(Id, ReplyText));
                IsReplyVisible = false;
                ReplyText = string.Empty;
            }
        }

        private void OnLikeComment()
        {
            _comment.IncrementLikeCount();
        }

        private void DeleteComment()
        {
            CommentDeleted?.Invoke(this, Id);
        }
    }
}
