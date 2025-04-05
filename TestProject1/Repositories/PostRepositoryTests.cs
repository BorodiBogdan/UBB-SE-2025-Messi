using Duo.Data;
using Duo.Models;
using Duo.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Xunit;

namespace TestMessi.Repositories
{
    public class PostRepositoryTests
    {
        private MockDatabaseConnectionPostRepository _dataLinkMock;

        public PostRepositoryTests()
        {
            _dataLinkMock = new MockDatabaseConnectionPostRepository();
        }

        [Fact]
        public void GetPostRepository_CorrectlyInstanciated_ReturnsInstance()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.NotNull(postRepository);
        }

        [Fact]
        public void GetPostById_ReturnsPost()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var post = postRepository.GetPostById(1);
            Assert.NotNull(post);
            Assert.Equal(1, post.Id);
            Assert.IsType<Post>(post);
        }

        [Fact]
        public void GetPostById_NoResults_ReturnsNull()
        {
            _dataLinkMock.ReturnEmptyResultSet = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var post = postRepository.GetPostById(999);
            Assert.Null(post);
        }

        [Fact]
        public void GetPostById_ThrowsException_PropagatesException()
        {
            _dataLinkMock.ThrowExceptionOnExecuteReader = true;
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => postRepository.GetPostById(1));
        }

        [Fact]
        public void CreatePost_ReturnsInt()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test post",
                Description = "Test post description",
                UserID = 1,
                CategoryID = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                LikeCount = 0
            };
            var result = postRepository.CreatePost(post);
            Assert.IsType<int>(result);
            Assert.Equal(1, result);
        }

        [Fact]
        public void CreatePost_WithDatabaseError_ThrowsException()
        {
            _dataLinkMock.ThrowExceptionOnExecuteScalar = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test post",
                Description = "Test post description",
                UserID = 1,
                CategoryID = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                LikeCount = 0
            };
            Assert.Throws<Exception>(() => postRepository.CreatePost(post));
        }

        [Fact]
        public void DeletePost_DoesNotThrowException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var exception = Record.Exception(() => postRepository.DeletePost(1));
            Assert.Null(exception);
        }

        [Fact]
        public void DeletePost_WithDatabaseError_ThrowsException()
        {
            _dataLinkMock.ThrowSqlException = true;
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => postRepository.DeletePost(1));
        }

        [Fact]
        public void UpdatePost_DoesNotThrowException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Id = 1,
                Title = "Updated Test post",
                Description = "Updated Test post description",
                UserID = 1,
                CategoryID = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                LikeCount = 0
            };
            var exception = Record.Exception(() => postRepository.UpdatePost(post));
            Assert.Null(exception);
        }

        [Fact]
        public void GetPostsByCategoryId_ReturnsCollection()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByCategoryId(1, 1, 10);
            Assert.NotNull(posts);
            Assert.IsType<Collection<Post>>(posts);
            Assert.True(posts.Count > 0);
            
            foreach (var post in posts)
            {
                Assert.Equal(1, post.CategoryID);
            }
        }

        [Fact]
        public void GetPostsByCategoryId_EmptyResults_ReturnsEmptyCollection()
        {
            _dataLinkMock.ReturnEmptyResultSet = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByCategoryId(999, 1, 10);
            Assert.NotNull(posts);
            Assert.IsType<Collection<Post>>(posts);
            Assert.Equal(0, posts.Count);
        }

        [Fact]
        public void GetPostsByCategoryId_WithDatabaseError_ThrowsException()
        {
            _dataLinkMock.ThrowExceptionOnExecuteReader = true;
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => postRepository.GetPostsByCategoryId(1, 1, 10));
        }

        [Fact]
        public void GetAllPostTitles_ReturnsList()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var titles = postRepository.GetAllPostTitles();
            Assert.NotNull(titles);
            Assert.IsType<List<string>>(titles);
            Assert.True(titles.Count > 0);
            
            Assert.Contains(titles, title => title == "First Post");
            Assert.Contains(titles, title => title == "Searchable Post");
        }

        [Fact]
        public void GetAllPostTitles_WithException_ReturnsEmptyList()
        {
            _dataLinkMock.ThrowExceptionOnExecuteReader = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var titles = postRepository.GetAllPostTitles();
            Assert.NotNull(titles);
            Assert.IsType<List<string>>(titles);
            Assert.Empty(titles);
        }

        [Fact]
        public void GetPostsByTitle_ReturnsList()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByTitle("Searchable");
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
            Assert.True(posts.Count > 0);
            
            var post = Assert.Single(posts.Where(p => p.Title == "Searchable Post"));
            Assert.NotNull(post);
        }

        [Fact]
        public void GetPostsByTitle_WithNoMatches_ReturnsEmptyList()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByTitle("NonExistentTitle");
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
        }

        [Fact]
        public void GetPostsByTitle_WithException_ReturnsEmptyList()
        {
            _dataLinkMock.ThrowExceptionOnExecuteReader = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByTitle("Post");
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
            Assert.Empty(posts);
        }

        [Fact]
        public void GetUserIdByPostId_ReturnsInt()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var userId = postRepository.GetUserIdByPostId(1);
            Assert.NotNull(userId);
            Assert.IsType<int>(userId.Value);
            Assert.Equal(1, userId.Value);
        }

        [Fact]
        public void GetUserIdByPostId_NoResults_ReturnsNull()
        {
            _dataLinkMock.ReturnEmptyResultSet = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var userId = postRepository.GetUserIdByPostId(999);
            Assert.Null(userId);
        }

        [Fact]
        public void GetUserIdByPostId_WithException_ThrowsException()
        {
            _dataLinkMock.ThrowExceptionOnExecuteReader = true;
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => postRepository.GetUserIdByPostId(1));
        }

        [Fact]
        public void GetPostsByUserId_ReturnsList()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByUserId(1, 1, 10);
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
            Assert.True(posts.Count > 0);
            
            foreach (var post in posts)
            {
                Assert.Equal(1, post.UserID);
            }
        }

        [Fact]
        public void GetPostsByUserId_NoResults_ReturnsEmptyList()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByUserId(999, 1, 10);
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
        }

        [Fact]
        public void GetPostsByUserId_EmptyResults_ReturnsEmptyList()
        {
            _dataLinkMock.ReturnEmptyResultSet = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByUserId(999, 1, 10);
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
            Assert.Empty(posts);
        }

        [Fact]
        public void GetPostsByUserId_WithException_ThrowsException()
        {
            _dataLinkMock.ThrowExceptionOnExecuteReader = true;
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => postRepository.GetPostsByUserId(1, 1, 10));
        }

        [Fact]
        public void GetPostsByHashtags_ReturnsList()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var hashtags = new List<string> { "test" };
            var posts = postRepository.GetPostsByHashtags(hashtags, 1, 10);
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
        }

        [Fact]
        public void GetPostsByHashtags_NullHashtags_CallsGetPaginatedPosts()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByHashtags(null, 1, 10);
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
            Assert.True(posts.Count > 0);
        }

        [Fact]
        public void GetPostsByHashtags_EmptyHashtags_CallsGetPaginatedPosts()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByHashtags(new List<string>(), 1, 10);
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
            Assert.True(posts.Count > 0);
        }

        [Fact]
        public void GetPostsByHashtags_NullOrWhitespaceHashtags_CallsGetPaginatedPosts()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByHashtags(new List<string> { null, string.Empty, "   " }, 1, 10);
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
            Assert.True(posts.Count > 0);
        }

        [Fact]
        public void GetPostsByHashtags_WithException_ThrowsException()
        {
            _dataLinkMock.ThrowExceptionOnExecuteReader = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var hashtags = new List<string> { "test" };
            Assert.Throws<Exception>(() => postRepository.GetPostsByHashtags(hashtags, 1, 10));
        }

        [Fact]
        public void IncrementPostLikeCount_ReturnsBoolean()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var result = postRepository.IncrementPostLikeCount(1);
            Assert.True(result);
        }

        [Fact]
        public void IncrementPostLikeCount_WithDatabaseError_ThrowsException()
        {
            _dataLinkMock.ThrowSqlException = true;
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => postRepository.IncrementPostLikeCount(1));
        }

        [Fact]
        public void GetPaginatedPosts_ReturnsList()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPaginatedPosts(1, 10);
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
            Assert.True(posts.Count > 0);
            
            Assert.Contains(posts, post => post.Id == 1);
            Assert.Contains(posts, post => post.Id == 2);
        }

        [Fact]
        public void GetPaginatedPosts_WithPagination_ReturnsCorrectPage()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            
            var firstPage = postRepository.GetPaginatedPosts(1, 2);
            Assert.Equal(2, firstPage.Count);
            
            var secondPage = postRepository.GetPaginatedPosts(2, 2);
            Assert.True(secondPage.Count > 0);
            
            Assert.NotEqual(firstPage[0].Id, secondPage[0].Id);
        }

        [Fact]
        public void GetPaginatedPosts_EmptyResults_ReturnsEmptyList()
        {
            _dataLinkMock.ReturnEmptyResultSet = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPaginatedPosts(100, 10);
            Assert.NotNull(posts);
            Assert.IsType<List<Post>>(posts);
            Assert.Empty(posts);
        }

        [Fact]
        public void GetPaginatedPosts_WithException_ThrowsException()
        {
            _dataLinkMock.ThrowExceptionOnExecuteReader = true;
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => postRepository.GetPaginatedPosts(1, 10));
        }

        [Fact]
        public void GetTotalPostCount_ReturnsInt()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var count = postRepository.GetTotalPostCount();
            Assert.IsType<int>(count);
            Assert.True(count > 0);
            Assert.Equal(9, count);
        }

        [Fact]
        public void GetTotalPostCount_WithException_ThrowsException()
        {
            _dataLinkMock.ThrowExceptionOnExecuteScalar = true;
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => postRepository.GetTotalPostCount());
        }

        [Fact]
        public void GetPostCountByCategory_ReturnsInt()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var count = postRepository.GetPostCountByCategory(1);
            Assert.IsType<int>(count);
            Assert.True(count > 0);
        }

        [Fact]
        public void GetPostCountByCategory_WithException_ThrowsException()
        {
            _dataLinkMock.ThrowExceptionOnExecuteScalar = true;
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => postRepository.GetPostCountByCategory(1));
        }

        [Fact]
        public void GetPostCountByHashtags_ReturnsInt()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var hashtags = new List<string> { "test" };
            var count = postRepository.GetPostCountByHashtags(hashtags);
            Assert.IsType<int>(count);
            Assert.Equal(3, count);
        }

        [Fact]
        public void GetPostCountByHashtags_NullHashtags_CallsGetTotalPostCount()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var count = postRepository.GetPostCountByHashtags(null);
            Assert.IsType<int>(count);
            Assert.Equal(9, count);
        }

        [Fact]
        public void GetPostCountByHashtags_EmptyHashtags_CallsGetTotalPostCount()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var count = postRepository.GetPostCountByHashtags(new List<string>());
            Assert.IsType<int>(count);
            Assert.Equal(9, count);
        }

        [Fact]
        public void GetPostCountByHashtags_NullOrWhitespaceHashtags_CallsGetTotalPostCount()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var count = postRepository.GetPostCountByHashtags(new List<string> { null, string.Empty, "   " });
            Assert.IsType<int>(count);
            Assert.Equal(9, count);
        }

        [Fact]
        public void GetPostCountByHashtags_WithException_ThrowsException()
        {
            _dataLinkMock.ThrowExceptionOnExecuteScalar = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var hashtags = new List<string> { "test" };
            Assert.Throws<Exception>(() => postRepository.GetPostCountByHashtags(hashtags));
        }

        [Fact]
        public void CreatePost_WithNullPost_ThrowsArgumentNullException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentNullException>(() => postRepository.CreatePost(null));
        }

        [Fact]
        public void CreatePost_WithEmptyTitle_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "",
                Description = "Test description",
                UserID = 1,
                CategoryID = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            Assert.Throws<ArgumentException>(() => postRepository.CreatePost(post));
        }

        [Fact]
        public void CreatePost_WithEmptyDescription_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test Title",
                Description = "",
                UserID = 1,
                CategoryID = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            Assert.Throws<ArgumentException>(() => postRepository.CreatePost(post));
        }

        [Fact]
        public void CreatePost_WithInvalidUserID_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test Title",
                Description = "Test description",
                UserID = 0,
                CategoryID = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            Assert.Throws<ArgumentException>(() => postRepository.CreatePost(post));
        }

        [Fact]
        public void CreatePost_WithInvalidCategoryID_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test Title",
                Description = "Test description",
                UserID = 1,
                CategoryID = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            Assert.Throws<ArgumentException>(() => postRepository.CreatePost(post));
        }

        [Fact]
        public void DeletePost_WithInvalidId_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentException>(() => postRepository.DeletePost(0));
        }

        [Fact]
        public void UpdatePost_WithNullPost_ThrowsArgumentNullException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentNullException>(() => postRepository.UpdatePost(null));
        }

        [Fact]
        public void UpdatePost_WithInvalidId_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Id = 0,
                Title = "Test Title",
                Description = "Test description",
                UserID = 1,
                CategoryID = 1
            };
            Assert.Throws<ArgumentException>(() => postRepository.UpdatePost(post));
        }

        [Fact]
        public void UpdatePost_WithEmptyTitle_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Id = 1,
                Title = "",
                Description = "Test description",
                UserID = 1,
                CategoryID = 1
            };
            Assert.Throws<ArgumentException>(() => postRepository.UpdatePost(post));
        }

        [Fact]
        public void UpdatePost_WithEmptyDescription_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Id = 1,
                Title = "Test Title",
                Description = "",
                UserID = 1,
                CategoryID = 1
            };
            Assert.Throws<ArgumentException>(() => postRepository.UpdatePost(post));
        }

        [Fact]
        public void GetPostById_WithInvalidId_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentException>(() => postRepository.GetPostById(0));
        }

        [Fact]
        public void GetPostsByCategoryId_WithInvalidCategoryId_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentException>(() => postRepository.GetPostsByCategoryId(0, 1, 10));
        }

        [Fact]
        public void GetPostsByCategoryId_WithInvalidPage_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentException>(() => postRepository.GetPostsByCategoryId(1, 0, 10));
        }

        [Fact]
        public void GetPostsByCategoryId_WithInvalidPageSize_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentException>(() => postRepository.GetPostsByCategoryId(1, 1, 0));
        }

        [Fact]
        public void GetPaginatedPosts_WithInvalidPage_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentException>(() => postRepository.GetPaginatedPosts(0, 10));
        }

        [Fact]
        public void GetPaginatedPosts_WithInvalidPageSize_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentException>(() => postRepository.GetPaginatedPosts(1, 0));
        }

        [Fact]
        public void GetPostCountByCategory_WithInvalidCategoryId_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentException>(() => postRepository.GetPostCountByCategory(0));
        }

        [Fact]
        public void IncrementPostLikeCount_WithInvalidPostId_ThrowsArgumentException()
        {
            var postRepository = new PostRepository(_dataLinkMock);
            Assert.Throws<ArgumentException>(() => postRepository.IncrementPostLikeCount(0));
        }

        [Fact]
        public void GetPostById_DataTableHasZeroRows_ReturnsNull()
        {
            _dataLinkMock.ReturnZeroRowsButNonEmpty = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var post = postRepository.GetPostById(1);
            Assert.Null(post);
        }

        [Fact]
        public void GetPostsByTitle_DataTableHasZeroRows_ReturnsEmptyList()
        {
            _dataLinkMock.ReturnZeroRowsButNonEmpty = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByTitle("any title");
            Assert.NotNull(posts);
            Assert.Empty(posts);
        }

        [Fact]
        public void GetPostsByUserId_DataTableHasZeroRows_ReturnsEmptyList()
        {
            _dataLinkMock.ReturnZeroRowsButNonEmpty = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByUserId(1, 1, 10);
            Assert.NotNull(posts);
            Assert.Empty(posts);
        }

        [Fact]
        public void GetPostsByCategoryId_DataTableHasZeroRows_ReturnsEmptyCollection()
        {
            _dataLinkMock.ReturnZeroRowsButNonEmpty = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByCategoryId(1, 1, 10);
            Assert.NotNull(posts);
            Assert.Equal(0, posts.Count);
        }

        [Fact]
        public void GetPaginatedPosts_DataTableHasZeroRows_ReturnsEmptyList()
        {
            _dataLinkMock.ReturnZeroRowsButNonEmpty = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPaginatedPosts(1, 10);
            Assert.NotNull(posts);
            Assert.Empty(posts);
        }

        [Fact]
        public void GetPostsByHashtags_DataTableHasZeroRows_ReturnsEmptyList()
        {
            _dataLinkMock.ReturnZeroRowsButNonEmpty = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByHashtags(new List<string> { "test" }, 1, 10);
            Assert.NotNull(posts);
            Assert.Empty(posts);
        }

        [Fact]
        public void GetUserIdByPostId_DataTableHasZeroRows_ReturnsNull()
        {
            _dataLinkMock.ReturnZeroRowsButNonEmpty = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var userId = postRepository.GetUserIdByPostId(1);
            Assert.Null(userId);
        }

        [Fact]
        public void GetAllPostTitles_DataTableHasZeroRows_ReturnsEmptyList()
        {
            _dataLinkMock.ReturnZeroRowsButNonEmpty = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var titles = postRepository.GetAllPostTitles();
            Assert.NotNull(titles);
            Assert.Empty(titles);
        }

        [Fact]
        public void GetPostsByTitle_WrongColumnNames_HandlesException()
        {
            _dataLinkMock.ReturnTableWithWrongColumns = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var posts = postRepository.GetPostsByTitle("any title");
            Assert.NotNull(posts);
            Assert.Empty(posts);
        }

        [Fact]
        public void GetPostById_WrongColumnNames_HandlesException()
        {
            _dataLinkMock.ReturnTableWithWrongColumns = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var exception = Record.Exception(() => postRepository.GetPostById(1));
            Assert.NotNull(exception);
        }

        [Fact]
        public void GetUserIdByPostId_WrongColumnNames_HandlesException()
        {
            _dataLinkMock.ReturnTableWithWrongColumns = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var exception = Record.Exception(() => postRepository.GetUserIdByPostId(1));
            Assert.NotNull(exception);
        }

        [Fact]
        public void IncrementPostLikeCount_ExceptionCatchBlock_ThrowsException()
        {
            _dataLinkMock.ThrowSqlException = true;
            var postRepository = new PostRepository(_dataLinkMock);
            
            var exception = Assert.Throws<Exception>(() => postRepository.IncrementPostLikeCount(1));
            Assert.Contains("Exception", exception.Message);
        }

        [Fact]
        public void IncrementPostLikeCount_WithSqlException_ThrowsSpecificException()
        {
            _dataLinkMock.ThrowSqlException = true;
            var postRepository = new PostRepository(_dataLinkMock);
            
            var exception = Assert.Throws<Exception>(() => postRepository.IncrementPostLikeCount(1));
            Assert.Contains("Exception", exception.Message);
        }

        [Fact]
        public void CreatePost_WithSqlException_ThrowsDatabaseError()
        {
            _dataLinkMock.ThrowExceptionOnExecuteScalar = true;
            _dataLinkMock.ThrowSqlException = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test Post",
                Description = "Test Description",
                UserID = 1,
                CategoryID = 1
            };
            
            var exception = Assert.Throws<Exception>(() => postRepository.CreatePost(post));
            Assert.Contains("Test SQL exception", exception.Message);
        }

        [Fact]
        public void CreatePost_WithGeneralException_ThrowsGeneralError()
        {
            _dataLinkMock.ThrowExceptionOnExecuteScalar = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test title",
                Description = "Test description",
                UserID = 1,
                CategoryID = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            
            var exception = Assert.Throws<Exception>(() => postRepository.CreatePost(post));
            Assert.Contains("Create post general error", exception.Message);
        }

        [Fact]
        public void CreatePost_ReturnNull_ReturnsZero()
        {
            _dataLinkMock.ReturnNullForExecuteScalar = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test title",
                Description = "Test description",
                UserID = 1,
                CategoryID = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            
            var result = postRepository.CreatePost(post);
            
            Assert.Equal(0, result);
            
            _dataLinkMock.ReturnNullForExecuteScalar = true;
            result = postRepository.CreatePost(post);
            Assert.Equal(0, result);
        }

        [Fact]
        public void CreatePost_WithGeneralException_ThrowsGeneralErrorWithExactMessage()
        {
            _dataLinkMock.ThrowExceptionOnExecuteScalar = true;
            _dataLinkMock.ThrowSqlException = false;
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test Post",
                Description = "Test Description",
                UserID = 1,
                CategoryID = 1
            };
            
            var exception = Assert.Throws<Exception>(() => postRepository.CreatePost(post));
            Assert.Contains("Create post general error", exception.Message);
            Assert.Contains("Test exception thrown from mock", exception.Message);
        }

        [Fact]
        public void CreatePost_SqlExceptionCatchBlock_ThrowsDatabaseError()
        {
            _dataLinkMock.ThrowExceptionOnExecuteScalar = true;
            _dataLinkMock.ThrowSqlException = true;
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test Post",
                Description = "Test Description",
                UserID = 1,
                CategoryID = 1
            };
            
            var exception = Assert.Throws<Exception>(() => postRepository.CreatePost(post));
            Assert.Contains("error:", exception.Message);
        }

        [Fact]
        public void CreatePost_GeneralExceptionCatchBlock_ThrowsGeneralError()
        {
            _dataLinkMock.ThrowExceptionOnExecuteScalar = true;
            _dataLinkMock.ThrowSqlException = false;
            var postRepository = new PostRepository(_dataLinkMock);
            var post = new Post
            {
                Title = "Test Post",
                Description = "Test Description",
                UserID = 1,
                CategoryID = 1
            };
            
            var exception = Assert.Throws<Exception>(() => postRepository.CreatePost(post));
            Assert.Contains("general error", exception.Message);
            Assert.Contains("Test exception", exception.Message);
        }

        [Fact]
        public void Mock_OpenConnection_DoesNotThrowException()
        {
            var mock = new MockDatabaseConnectionPostRepository();
            
            mock.OpenConnection();
            
            Assert.True(true);
        }

        [Fact]
        public void Mock_CloseConnection_DoesNotThrowException()
        {
            var mock = new MockDatabaseConnectionPostRepository();
            
            mock.CloseConnection();
            
            Assert.True(true);
        }

        [Fact]
        public void Mock_ExecuteReader_WithUnsupportedProcedure_ReturnsEmptyTable()
        {
            var mock = new MockDatabaseConnectionPostRepository();
            
            var result = mock.ExecuteReader("UnsupportedProcedure", null);
            
            Assert.NotNull(result);
            Assert.Equal(0, result.Rows.Count);
        }

        [Fact]
        public void Mock_ExecuteScalar_WithUnsupportedProcedure_ReturnsDefault()
        {
            var mock = new MockDatabaseConnectionPostRepository();
            
            var result = mock.ExecuteScalar<int>("UnsupportedProcedure", null);
            
            Assert.Equal(0, result); 
        }

        [Fact]
        public void Mock_GetUserIdByPostId_WithLoopCompletion_CoversForeachClosingBrace()
        {
            var mock = new MockDatabaseConnectionPostRepository();
            mock.EnsureLoopCompletion = true;
            
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@PostId", 1) };
            var resultTable = mock.ExecuteReader("GetUserIdByPostId", parameters);
            
            Assert.NotNull(resultTable);
        }
    }
}
