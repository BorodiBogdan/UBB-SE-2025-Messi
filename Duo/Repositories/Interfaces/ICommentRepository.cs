using Duo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        public Comment GetCommentById(int commentId);

        public List<Comment> GetCommentsByPostId(int postId);
        public int CreateComment(Comment comment);
        public bool DeleteComment(int id);
        public List<Comment> GetRepliesByCommentId(int parentCommentId);
        public bool IncrementLikeCount(int commentId);
        public int GetCommentsCountForPost(int postId);
    }
}
