using Duo.Models;
using Duo.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using Duo.ViewModels;
using static Duo.App;

namespace Duo.Views.Components
{
    public sealed partial class Comment : UserControl
    {
        public event EventHandler<CommentReplyEventArgs> ReplySubmitted;
        public event EventHandler<CommentLikedEventArgs> CommentLiked;
        public event EventHandler<CommentDeletedEventArgs> CommentDeleted;

        public CommentViewModel ViewModel => DataContext as CommentViewModel;

        public Comment()
        {
            this.InitializeComponent();

            CommentReplyButton.Click += CommentReplyButton_Click;
            LikeButton.LikeClicked += LikeButton_LikeClicked;
            ReplyInputControl.CommentSubmitted += ReplyInput_CommentSubmitted;
            DeleteButton.Click += DeleteButton_Click;
            ToggleChildrenButton.Click += ToggleChildrenButton_Click;
            
            this.DataContextChanged += Comment_DataContextChanged;
            
            // Set up the element factory for the ChildCommentsRepeater
            ChildCommentsRepeater.ElementPrepared += ChildCommentsRepeater_ElementPrepared;
        }

        private void ChildCommentsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is ContentPresenter presenter)
            {
                var index = args.Index;
                if (ViewModel?.Replies != null && index < ViewModel.Replies.Count)
                {
                    var childViewModel = ViewModel.Replies[index];
                    
                    // Create a Comment control for this child
                    var childComment = new Comment
                    {
                        DataContext = childViewModel,
                        Margin = new Thickness(0, 4, 0, 0)
                    };
                    
                    // Wire up events
                    childComment.ReplySubmitted += ChildComment_ReplySubmitted;
                    childComment.CommentLiked += ChildComment_CommentLiked;
                    childComment.CommentDeleted += ChildComment_CommentDeleted;

                    // Set the content of the presenter
                    presenter.Content = childComment;
                }
            }
        }

        private void Comment_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (ViewModel != null)
            {
                // Set up level lines for indentation
                var indentationLevels = new List<int>();
                for (int i = 1; i <= ViewModel.Level; i++)
                {
                    indentationLevels.Add(i);
                }
                LevelLinesRepeater.ItemsSource = indentationLevels;

                // Update UI based on ViewModel state
                CommentReplyButton.Visibility = ViewModel.IsReplyButtonVisible ? Visibility.Visible : Visibility.Collapsed;
                DeleteButton.Visibility = ViewModel.IsDeleteButtonVisible ? Visibility.Visible : Visibility.Collapsed;
                ToggleChildrenButton.Visibility = ViewModel.IsToggleButtonVisible ? Visibility.Visible : Visibility.Collapsed;
                ToggleIcon.Glyph = ViewModel.ToggleIconGlyph;
            }
        }

        private void ToggleChildrenButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ToggleRepliesCommand.Execute(null);
        }

        private void LikeButton_LikeClicked(object sender, LikeButtonClickedEventArgs e)
        {
            if (ViewModel != null && e.TargetType == LikeTargetType.Comment && e.TargetId == ViewModel.Id)
            {
                CommentLiked?.Invoke(this, new CommentLikedEventArgs(ViewModel.Id));
            }
        }

        private void ChildComment_CommentLiked(object sender, CommentLikedEventArgs e)
        {
            CommentLiked?.Invoke(this, e);
        }

        private void CommentReplyButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ShowReplyFormCommand.Execute(null);
        }

        private void ReplyInput_CommentSubmitted(object sender, EventArgs e)
        {
            if (ViewModel == null) return;
            
            if (sender is CommentInput commentInput && !string.IsNullOrWhiteSpace(commentInput.CommentText))
            {
                ReplySubmitted?.Invoke(this, new CommentReplyEventArgs(ViewModel.Id, commentInput.CommentText));
                commentInput.ClearComment();
            }
        }

        private void ChildComment_ReplySubmitted(object sender, CommentReplyEventArgs e)
        {
            ReplySubmitted?.Invoke(this, e);
        }

        private void ChildComment_CommentDeleted(object sender, CommentDeletedEventArgs e)
        {
            CommentDeleted?.Invoke(this, e);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.DeleteCommentCommand.Execute(null);
        }
    }

    public class CommentReplyEventArgs : EventArgs
    {
        public int ParentCommentId { get; private set; }
        public string ReplyText { get; private set; }

        public CommentReplyEventArgs(int parentCommentId, string replyText)
        {
            ParentCommentId = parentCommentId;
            ReplyText = replyText;
        }
    }

    public class CommentLikedEventArgs : EventArgs
    {
        public int CommentId { get; private set; }

        public CommentLikedEventArgs(int commentId)
        {
            CommentId = commentId;
        }
    }

    public class CommentDeletedEventArgs : EventArgs
    {
        public int CommentId { get; private set; }

        public CommentDeletedEventArgs(int commentId)
        {
            CommentId = commentId;
        }
    }
} 