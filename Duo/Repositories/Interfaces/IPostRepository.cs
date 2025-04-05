using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duo.Models;

namespace Duo.Repositories.Interfaces
{
    internal interface IPostRepository
    {
        public int CreatePost(Post post);

        public void DeletePost(int id);

        public void UpdatePost(Post post);

        public Post? GetPostById(int id);

        public Collection<Post> GetPostsByCategoryId(int categoryId, int page, int pageSize);

        public List<string> GetAllPostTitles();

        public List<Post> GetPostsByTitle(string title);

        public int? GetUserIdByPostId(int postId);

        public List<Post> GetPostsByUserId(int userId, int page, int pageSize);

        public List<Post> GetPostsByHashtags(List<string> hashtags, int page, int pageSize);

        public bool IncrementPostLikeCount(int postId);

        public List<Post> GetPaginatedPosts(int page, int pageSize);

        public int GetTotalPostCount();

        public int GetPostCountByCategory(int categoryId);

        public int GetPostCountByHashtags(List<string> hashtags);
    }
}
