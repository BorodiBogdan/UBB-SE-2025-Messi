using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duo.Models;

namespace Duo.Services.Interfaces
{
    public interface ICommentService
    {
        public List<Comment> GetCommentsByPostId(int postId);
        public (List<Comment> AllComments, List<Comment> TopLevelComments, Dictionary<int, List<Comment>> RepliesByParentId) GetProcessedCommentsByPostId(int postId);
        public int CreateComment(string content, int postId, int? parentCommentId = null);
        public (bool Success, string ReplySignature) CreateReplyWithDuplicateCheck(
            string replyText, 
            int postId, 
            int parentCommentId, 
            IEnumerable<Comment> existingComments,
            string lastProcessedReplySignature = null);
        public bool DeleteComment(int commentId, int userId);
        public bool LikeComment(int commentId);
        public T FindCommentInHierarchy<T>(
            int commentId, 
            IEnumerable<T> topLevelComments,
            Func<T, IEnumerable<T>> getRepliesFunc,
            Func<T, int> getIdFunc) where T : class;
    }
}
