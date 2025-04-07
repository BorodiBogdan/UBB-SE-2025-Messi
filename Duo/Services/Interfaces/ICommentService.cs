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
        public int CreateComment(string content, int postId, int? parentCommentId = null);

        public bool DeleteComment(int commentId, int userId);

        public bool LikeComment(int commentId);
    }
}
