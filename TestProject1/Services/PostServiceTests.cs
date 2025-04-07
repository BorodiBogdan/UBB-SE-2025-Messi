using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Duo.Models;
using Duo.Repositories.Interfaces;
using Duo.Services;
using Duo.Services.Interfaces;
using Xunit;

namespace TestProject1.Services
{
    public class PostServiceTests
    {
        private Mock<IPostRepository> _mockPostRepository;
        private Mock<IHashtagRepository> _mockHashtagRepository;
        private Mock<IUserService> _mockUserService;
        private Mock<ISearchService> _mockSearchService;
        private PostService _postService;
        
        // Test data
        private const int VALID_POST_ID = 1;
        private const int INVALID_POST_ID = 0;
        private const int VALID_USER_ID = 1;
        private const int VALID_CATEGORY_ID = 1;
        private const int VALID_HASHTAG_ID = 1;
        private const int VALID_PAGE_NUMBER = 1;
        private const int VALID_PAGE_SIZE = 10;
        
        public PostServiceTests()
        {
            _mockPostRepository = new Mock<IPostRepository>();
            _mockHashtagRepository = new Mock<IHashtagRepository>();
            _mockUserService = new Mock<IUserService>();
            _mockSearchService = new Mock<ISearchService>();
            
            // Setup the SearchService mock correctly
            _mockSearchService.Setup(service => service.LevenshteinSimilarity(
                It.IsAny<string>(), It.IsAny<string>()))
                .Returns(0.8);
                
            _mockSearchService.Setup(service => service.FindFuzzySearchMatches(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<double>()))
                .Returns(new List<string>());
                
            _postService = new PostService(
                _mockPostRepository.Object,
                _mockHashtagRepository.Object,
                _mockUserService.Object,
                _mockSearchService.Object
            );
        }
        
        #region CreatePost Tests
        
        [Fact]
        public void CreatePost_WithValidPost_ReturnsPostId()
        {
            // Arrange
            var post = new Post
            {
                Title = "Test Title",
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            _mockPostRepository.Setup(repo => repo.CreatePost(It.IsAny<Post>()))
                .Returns(VALID_POST_ID);
            
            // Act
            int result = _postService.CreatePost(post);
            
            // Assert
            Assert.Equal(VALID_POST_ID, result);
            _mockPostRepository.Verify(repo => repo.CreatePost(It.IsAny<Post>()), Times.Once);
        }
        
        [Fact]
        public void CreatePost_WithNullTitle_ThrowsArgumentException()
        {
            // Arrange
            var post = new Post
            {
                Title = null,
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.CreatePost(post));
        }
        
        [Fact]
        public void CreatePost_WithEmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            var post = new Post
            {
                Title = "",
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.CreatePost(post));
        }
        
        [Fact]
        public void CreatePost_WithNullDescription_ThrowsArgumentException()
        {
            // Arrange
            var post = new Post
            {
                Title = "Test Title",
                Description = null,
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.CreatePost(post));
        }
        
        [Fact]
        public void CreatePost_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var post = new Post
            {
                Title = "Test Title",
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            _mockPostRepository.Setup(repo => repo.CreatePost(It.IsAny<Post>()))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.CreatePost(post));
        }
        
        #endregion
        
        #region DeletePost Tests
        
        [Fact]
        public void DeletePost_WithValidId_CallsRepository()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.DeletePost(VALID_POST_ID))
                .Verifiable();
            
            // Act
            _postService.DeletePost(VALID_POST_ID);
            
            // Assert
            _mockPostRepository.Verify(repo => repo.DeletePost(VALID_POST_ID), Times.Once);
        }
        
        [Fact]
        public void DeletePost_WithInvalidId_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _postService.DeletePost(INVALID_POST_ID));
        }
        
        [Fact]
        public void DeletePost_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.DeletePost(VALID_POST_ID))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.DeletePost(VALID_POST_ID));
        }
        
        #endregion
        
        #region GetPostById Tests
        
        [Fact]
        public void GetPostById_WithValidId_ReturnsPost()
        {
            // Arrange
            var expectedPost = new Post
            {
                Id = VALID_POST_ID,
                Title = "Test Title",
                Description = "Test Description"
            };
            
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Returns(expectedPost);
            
            // Act
            var result = _postService.GetPostById(VALID_POST_ID);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPost.Id, result.Id);
            Assert.Equal(expectedPost.Title, result.Title);
            Assert.Equal(expectedPost.Description, result.Description);
            _mockPostRepository.Verify(repo => repo.GetPostById(VALID_POST_ID), Times.Once);
        }
        
        [Fact]
        public void GetPostById_WithInvalidId_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _postService.GetPostById(INVALID_POST_ID));
        }
        
        [Fact]
        public void GetPostById_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.GetPostById(VALID_POST_ID));
        }
        
        #endregion
        
        #region GetPostsByCategory Tests
        
        [Fact]
        public void GetPostsByCategory_WithValidParameters_ReturnsPosts()
        {
            // Arrange
            var expectedPosts = new Collection<Post>
            {
                new Post { Id = 1, Title = "Post 1", CategoryID = VALID_CATEGORY_ID },
                new Post { Id = 2, Title = "Post 2", CategoryID = VALID_CATEGORY_ID }
            };
            
            _mockPostRepository.Setup(repo => repo.GetPostsByCategoryId(
                VALID_CATEGORY_ID, VALID_PAGE_NUMBER, VALID_PAGE_SIZE))
                .Returns(expectedPosts);
            
            // Act
            var result = _postService.GetPostsByCategory(
                VALID_CATEGORY_ID, VALID_PAGE_NUMBER, VALID_PAGE_SIZE);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPosts.Count, result.Count);
            Assert.Equal(expectedPosts[0].Title, result[0].Title);
            Assert.Equal(expectedPosts[1].Title, result[1].Title);
            _mockPostRepository.Verify(repo => repo.GetPostsByCategoryId(
                VALID_CATEGORY_ID, VALID_PAGE_NUMBER, VALID_PAGE_SIZE), Times.Once);
        }
        
        [Fact]
        public void GetPostsByCategory_WithInvalidCategoryId_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _postService.GetPostsByCategory(INVALID_POST_ID, VALID_PAGE_NUMBER, VALID_PAGE_SIZE));
        }
        
        [Fact]
        public void GetPostsByCategory_WithInvalidPageNumber_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _postService.GetPostsByCategory(VALID_CATEGORY_ID, 0, VALID_PAGE_SIZE));
        }
        
        [Fact]
        public void GetPostsByCategory_WithInvalidPageSize_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _postService.GetPostsByCategory(VALID_CATEGORY_ID, VALID_PAGE_NUMBER, 0));
        }
        
        [Fact]
        public void GetPostsByCategory_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.GetPostsByCategoryId(
                VALID_CATEGORY_ID, VALID_PAGE_NUMBER, VALID_PAGE_SIZE))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.GetPostsByCategory(VALID_CATEGORY_ID, VALID_PAGE_NUMBER, VALID_PAGE_SIZE));
        }
        
        #endregion
        
        #region LikePost Tests
        
        [Fact]
        public void LikePost_WithValidId_IncrementsLikeCountAndReturnsTrue()
        {
            // Arrange
            var post = new Post
            {
                Id = VALID_POST_ID,
                Title = "Test Title",
                Description = "Test Description",
                LikeCount = 5
            };
            
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Returns(post);
            
            _mockPostRepository.Setup(repo => repo.UpdatePost(It.IsAny<Post>()))
                .Verifiable();
            
            // Act
            bool result = _postService.LikePost(VALID_POST_ID);
            
            // Assert
            Assert.True(result);
            Assert.Equal(6, post.LikeCount);
            _mockPostRepository.Verify(repo => repo.GetPostById(VALID_POST_ID), Times.Once);
            _mockPostRepository.Verify(repo => repo.UpdatePost(It.Is<Post>(p => 
                p.Id == VALID_POST_ID && p.LikeCount == 6)), Times.Once);
        }
        
        [Fact]
        public void LikePost_WithInvalidId_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _postService.LikePost(INVALID_POST_ID));
        }
        
        [Fact]
        public void LikePost_PostNotFound_ThrowsException()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Returns((Post)null);
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.LikePost(VALID_POST_ID));
        }
        
        [Fact]
        public void LikePost_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.LikePost(VALID_POST_ID));
        }
        
        #endregion
        
        #region AddHashtagToPost Tests
        
        [Fact]
        public void AddHashtagToPost_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var post = new Post
            {
                Id = VALID_POST_ID,
                Title = "Test Title",
                Description = "Test Description",
                UserID = VALID_USER_ID
            };
            
            var hashtag = new Hashtag(VALID_HASHTAG_ID, "TestHashtag");
            
            var currentUser = new User(VALID_USER_ID, "TestUser");
            
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Returns(post);
            
            _mockUserService.Setup(service => service.GetCurrentUser())
                .Returns(currentUser);
            
            _mockHashtagRepository.Setup(repo => repo.GetHashtagByText("TestHashtag"))
                .Returns(hashtag);
            
            _mockHashtagRepository.Setup(repo => repo.CreateHashtag("TestHashtag"))
                .Returns(hashtag);
            
            _mockHashtagRepository.Setup(repo => repo.AddHashtagToPost(VALID_POST_ID, VALID_HASHTAG_ID))
                .Returns(true);
            
            // Act
            bool result = _postService.AddHashtagToPost(VALID_POST_ID, "TestHashtag", VALID_USER_ID);
            
            // Assert
            Assert.True(result);
            _mockPostRepository.Verify(repo => repo.GetPostById(VALID_POST_ID), Times.Once);
            _mockUserService.Verify(service => service.GetCurrentUser(), Times.Once);
            _mockHashtagRepository.Verify(repo => repo.GetHashtagByText("TestHashtag"), Times.Once);
            _mockHashtagRepository.Verify(repo => repo.CreateHashtag("TestHashtag"), Times.Once);
            _mockHashtagRepository.Verify(repo => repo.AddHashtagToPost(VALID_POST_ID, VALID_HASHTAG_ID), Times.Once);
        }
        
        [Fact]
        public void AddHashtagToPost_WithInvalidPostId_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _postService.AddHashtagToPost(INVALID_POST_ID, "TestHashtag", VALID_USER_ID));
        }
        
        [Fact]
        public void AddHashtagToPost_WithEmptyHashtagName_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _postService.AddHashtagToPost(VALID_POST_ID, "", VALID_USER_ID));
        }
        
        [Fact]
        public void AddHashtagToPost_WithInvalidUserId_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _postService.AddHashtagToPost(VALID_POST_ID, "TestHashtag", INVALID_POST_ID));
        }
        
        [Fact]
        public void AddHashtagToPost_PostNotFound_ThrowsException()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Returns((Post)null);
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.AddHashtagToPost(VALID_POST_ID, "TestHashtag", VALID_USER_ID));
        }
        
        [Fact]
        public void AddHashtagToPost_UserNotAuthorized_ThrowsException()
        {
            // Arrange
            var post = new Post
            {
                Id = VALID_POST_ID,
                Title = "Test Title",
                Description = "Test Description",
                UserID = VALID_USER_ID
            };
            
            var currentUser = new User(VALID_USER_ID + 1, "DifferentUser");
            
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Returns(post);
            
            _mockUserService.Setup(service => service.GetCurrentUser())
                .Returns(currentUser);
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.AddHashtagToPost(VALID_POST_ID, "TestHashtag", VALID_USER_ID));
        }
        
        #endregion
        
        #region GetHashtagsByPostId Tests
        
        [Fact]
        public void GetHashtagsByPostId_WithValidId_ReturnsHashtags()
        {
            // Arrange
            var expectedHashtags = new List<Hashtag>
            {
                new Hashtag(1, "Tag1"),
                new Hashtag(2, "Tag2")
            };
            
            _mockHashtagRepository.Setup(repo => repo.GetHashtagsByPostId(VALID_POST_ID))
                .Returns(expectedHashtags);
            
            // Act
            var result = _postService.GetHashtagsByPostId(VALID_POST_ID);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedHashtags.Count, result.Count);
            Assert.Equal(expectedHashtags[0].Tag, result[0].Tag);
            Assert.Equal(expectedHashtags[1].Tag, result[1].Tag);
            _mockHashtagRepository.Verify(repo => repo.GetHashtagsByPostId(VALID_POST_ID), Times.Once);
        }
        
        [Fact]
        public void GetHashtagsByPostId_WithInvalidId_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _postService.GetHashtagsByPostId(INVALID_POST_ID));
        }
        
        [Fact]
        public void GetHashtagsByPostId_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockHashtagRepository.Setup(repo => repo.GetHashtagsByPostId(VALID_POST_ID))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.GetHashtagsByPostId(VALID_POST_ID));
        }
        
        #endregion
        
        #region GetAllHashtags Tests
        
        [Fact]
        public void GetAllHashtags_ReturnsAllHashtags()
        {
            // Arrange
            var expectedHashtags = new List<Hashtag>
            {
                new Hashtag(1, "Tag1"),
                new Hashtag(2, "Tag2"),
                new Hashtag(3, "Tag3")
            };
            
            _mockHashtagRepository.Setup(repo => repo.GetAllHashtags())
                .Returns(expectedHashtags);
            
            // Act
            var result = _postService.GetAllHashtags();
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedHashtags.Count, result.Count);
            Assert.Equal(expectedHashtags[0].Tag, result[0].Tag);
            Assert.Equal(expectedHashtags[1].Tag, result[1].Tag);
            Assert.Equal(expectedHashtags[2].Tag, result[2].Tag);
            _mockHashtagRepository.Verify(repo => repo.GetAllHashtags(), Times.Once);
        }
        
        [Fact]
        public void GetAllHashtags_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockHashtagRepository.Setup(repo => repo.GetAllHashtags())
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.GetAllHashtags());
        }
        
        #endregion
        
        #region CreatePostWithHashtags Tests
        
        [Fact]
        public void CreatePostWithHashtags_WithValidParameters_ReturnsPostId()
        {
            // Arrange
            var post = new Post
            {
                Title = "Test Title",
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            var hashtag1 = new Hashtag(1, "Tag1");
            var hashtag2 = new Hashtag(2, "Tag2");
            
            _mockPostRepository.Setup(repo => repo.CreatePost(It.IsAny<Post>()))
                .Returns(VALID_POST_ID);
            
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Returns(post);
            
            _mockHashtagRepository.Setup(repo => repo.GetHashtagByText("Tag1"))
                .Returns(hashtag1);
                
            _mockHashtagRepository.Setup(repo => repo.GetHashtagByText("Tag2"))
                .Returns(hashtag2);
                
            _mockHashtagRepository.Setup(repo => repo.CreateHashtag("Tag1"))
                .Returns(hashtag1);
                
            _mockHashtagRepository.Setup(repo => repo.CreateHashtag("Tag2"))
                .Returns(hashtag2);
                
            _mockHashtagRepository.Setup(repo => repo.AddHashtagToPost(VALID_POST_ID, 1))
                .Returns(true);
                
            _mockHashtagRepository.Setup(repo => repo.AddHashtagToPost(VALID_POST_ID, 2))
                .Returns(true);
            
            // Act
            int result = _postService.CreatePostWithHashtags(post, hashtags, VALID_USER_ID);
            
            // Assert
            Assert.Equal(VALID_POST_ID, result);
            _mockPostRepository.Verify(repo => repo.CreatePost(It.IsAny<Post>()), Times.Once);
        }
        
        [Fact]
        public void CreatePostWithHashtags_WithNullTitle_ThrowsArgumentException()
        {
            // Arrange
            var post = new Post
            {
                Title = null,
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.CreatePostWithHashtags(post, hashtags, VALID_USER_ID));
        }
        
        [Fact]
        public void CreatePostWithHashtags_WithEmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            var post = new Post
            {
                Title = "",
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.CreatePostWithHashtags(post, hashtags, VALID_USER_ID));
        }
        
        [Fact]
        public void CreatePostWithHashtags_InvalidPostIdReturned_ThrowsException()
        {
            // Arrange
            var post = new Post
            {
                Title = "Test Title",
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            _mockPostRepository.Setup(repo => repo.CreatePost(It.IsAny<Post>()))
                .Returns(INVALID_POST_ID);
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.CreatePostWithHashtags(post, hashtags, VALID_USER_ID));
        }
        
        [Fact]
        public void CreatePostWithHashtags_NoHashtagsProvided_StillCreatesPost()
        {
            // Arrange
            var post = new Post
            {
                Title = "Test Title",
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            List<string> hashtags = null; // No hashtags
            
            _mockPostRepository.Setup(repo => repo.CreatePost(It.IsAny<Post>()))
                .Returns(VALID_POST_ID);
            
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Returns(post);
            
            // Act
            int result = _postService.CreatePostWithHashtags(post, hashtags, VALID_USER_ID);
            
            // Assert
            Assert.Equal(VALID_POST_ID, result);
            _mockPostRepository.Verify(repo => repo.CreatePost(It.IsAny<Post>()), Times.Once);
            // Should not try to add hashtags
            _mockHashtagRepository.Verify(repo => repo.AddHashtagToPost(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
        
        [Fact]
        public void CreatePostWithHashtags_GetPostByIdThrowsException_ContinuesExecution()
        {
            // Arrange
            var post = new Post
            {
                Title = "Test Title",
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            _mockPostRepository.Setup(repo => repo.CreatePost(It.IsAny<Post>()))
                .Returns(VALID_POST_ID);
            
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Throws(new Exception("Database error"));
            
            // Setup rest of the method to run successfully
            var hashtag1 = new Hashtag(1, "Tag1");
            _mockHashtagRepository.Setup(repo => repo.GetHashtagByText("Tag1"))
                .Returns(hashtag1);
            _mockHashtagRepository.Setup(repo => repo.CreateHashtag("Tag1"))
                .Returns(hashtag1);
            _mockHashtagRepository.Setup(repo => repo.AddHashtagToPost(VALID_POST_ID, 1))
                .Returns(true);
            
            // Act
            int result = _postService.CreatePostWithHashtags(post, hashtags, VALID_USER_ID);
            
            // Assert
            Assert.Equal(VALID_POST_ID, result);
            _mockPostRepository.Verify(repo => repo.GetPostById(VALID_POST_ID), Times.Once);
        }
        
        [Fact]
        public void CreatePostWithHashtags_HashtagAddingThrowsException_ContinuesExecution()
        {
            // Arrange
            var post = new Post
            {
                Title = "Test Title",
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            _mockPostRepository.Setup(repo => repo.CreatePost(It.IsAny<Post>()))
                .Returns(VALID_POST_ID);
            
            _mockPostRepository.Setup(repo => repo.GetPostById(VALID_POST_ID))
                .Returns(post);
            
            var hashtag1 = new Hashtag(1, "Tag1");
            _mockHashtagRepository.Setup(repo => repo.GetHashtagByText("Tag1"))
                .Returns(hashtag1);
            _mockHashtagRepository.Setup(repo => repo.CreateHashtag("Tag1"))
                .Returns(hashtag1);
            
            // Setup AddHashtagToPost to throw for Tag1
            _mockHashtagRepository.Setup(repo => repo.AddHashtagToPost(VALID_POST_ID, 1))
                .Throws(new Exception("Database error"));
            
            // Setup Tag2 to work correctly
            var hashtag2 = new Hashtag(2, "Tag2");
            _mockHashtagRepository.Setup(repo => repo.GetHashtagByText("Tag2"))
                .Returns(hashtag2);
            _mockHashtagRepository.Setup(repo => repo.CreateHashtag("Tag2"))
                .Returns(hashtag2);
            _mockHashtagRepository.Setup(repo => repo.AddHashtagToPost(VALID_POST_ID, 2))
                .Returns(true);
            
            // Act
            int result = _postService.CreatePostWithHashtags(post, hashtags, VALID_USER_ID);
            
            // Assert
            Assert.Equal(VALID_POST_ID, result);
            _mockHashtagRepository.Verify(repo => repo.GetHashtagByText("Tag1"), Times.Once);
            _mockHashtagRepository.Verify(repo => repo.GetHashtagByText("Tag2"), Times.Once);
            _mockHashtagRepository.Verify(repo => repo.AddHashtagToPost(VALID_POST_ID, 1), Times.Once);
        }
        
        [Fact]
        public void CreatePostWithHashtags_WithInnerException_HandlesInnerException()
        {
            // Arrange
            var post = new Post
            {
                Title = "Test Title",
                Description = "Test Description",
                UserID = VALID_USER_ID,
                CategoryID = VALID_CATEGORY_ID
            };
            
            var hashtags = new List<string> { "Tag1" };
            
            var innerException = new Exception("Inner database error");
            var outerException = new Exception("Outer error", innerException);
            
            _mockPostRepository.Setup(repo => repo.CreatePost(It.IsAny<Post>()))
                .Throws(outerException);
            
            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _postService.CreatePostWithHashtags(post, hashtags, VALID_USER_ID));
            Assert.Contains("Error creating post with hashtags", exception.Message);
            Assert.Same(outerException, exception.InnerException);
        }
        
        #endregion
        
        #region GetPaginatedPosts Tests
        
        [Fact]
        public void GetPaginatedPosts_WithValidParameters_ReturnsPosts()
        {
            // Arrange
            var expectedPosts = new List<Post>
            {
                new Post { Id = 1, Title = "Post 1" },
                new Post { Id = 2, Title = "Post 2" }
            };
            
            _mockPostRepository.Setup(repo => repo.GetPaginatedPosts(
                VALID_PAGE_NUMBER, VALID_PAGE_SIZE))
                .Returns(expectedPosts);
            
            // Act
            var result = _postService.GetPaginatedPosts(VALID_PAGE_NUMBER, VALID_PAGE_SIZE);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPosts.Count, result.Count);
            Assert.Equal(expectedPosts[0].Title, result[0].Title);
            Assert.Equal(expectedPosts[1].Title, result[1].Title);
            _mockPostRepository.Verify(repo => repo.GetPaginatedPosts(
                VALID_PAGE_NUMBER, VALID_PAGE_SIZE), Times.Once);
        }
        
        [Fact]
        public void GetPaginatedPosts_WithInvalidPageNumber_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.GetPaginatedPosts(0, VALID_PAGE_SIZE));
        }
        
        [Fact]
        public void GetPaginatedPosts_WithInvalidPageSize_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.GetPaginatedPosts(VALID_PAGE_NUMBER, 0));
        }
        
        [Fact]
        public void GetPaginatedPosts_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.GetPaginatedPosts(
                VALID_PAGE_NUMBER, VALID_PAGE_SIZE))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.GetPaginatedPosts(VALID_PAGE_NUMBER, VALID_PAGE_SIZE));
        }
        
        #endregion
        
        #region GetTotalPostCount Tests
        
        [Fact]
        public void GetTotalPostCount_ReturnsCount()
        {
            // Arrange
            const int expectedCount = 10;
            
            _mockPostRepository.Setup(repo => repo.GetTotalPostCount())
                .Returns(expectedCount);
            
            // Act
            int result = _postService.GetTotalPostCount();
            
            // Assert
            Assert.Equal(expectedCount, result);
            _mockPostRepository.Verify(repo => repo.GetTotalPostCount(), Times.Once);
        }
        
        [Fact]
        public void GetTotalPostCount_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.GetTotalPostCount())
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.GetTotalPostCount());
        }
        
        #endregion
        
        #region GetPostCountByCategoryId Tests
        
        [Fact]
        public void GetPostCountByCategoryId_WithValidId_ReturnsCount()
        {
            // Arrange
            const int expectedCount = 5;
            
            _mockPostRepository.Setup(repo => repo.GetPostCountByCategory(VALID_CATEGORY_ID))
                .Returns(expectedCount);
            
            // Act
            int result = _postService.GetPostCountByCategoryId(VALID_CATEGORY_ID);
            
            // Assert
            Assert.Equal(expectedCount, result);
            _mockPostRepository.Verify(repo => repo.GetPostCountByCategory(VALID_CATEGORY_ID), Times.Once);
        }
        
        [Fact]
        public void GetPostCountByCategoryId_WithInvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.GetPostCountByCategoryId(INVALID_POST_ID));
        }
        
        [Fact]
        public void GetPostCountByCategoryId_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.GetPostCountByCategory(VALID_CATEGORY_ID))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.GetPostCountByCategoryId(VALID_CATEGORY_ID));
        }
        
        #endregion
        
        #region ValidatePostOwnership Tests
        
        [Fact]
        public void ValidatePostOwnership_UserIsOwner_ReturnsTrue()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.GetUserIdByPostId(VALID_POST_ID))
                .Returns(VALID_USER_ID);
            
            // Act
            bool result = _postService.ValidatePostOwnership(VALID_USER_ID, VALID_POST_ID);
            
            // Assert
            Assert.True(result);
            _mockPostRepository.Verify(repo => repo.GetUserIdByPostId(VALID_POST_ID), Times.Once);
        }
        
        [Fact]
        public void ValidatePostOwnership_UserIsNotOwner_ReturnsFalse()
        {
            // Arrange
            const int differentUserId = VALID_USER_ID + 1;
            
            _mockPostRepository.Setup(repo => repo.GetUserIdByPostId(VALID_POST_ID))
                .Returns(VALID_USER_ID);
            
            // Act
            bool result = _postService.ValidatePostOwnership(differentUserId, VALID_POST_ID);
            
            // Assert
            Assert.False(result);
            _mockPostRepository.Verify(repo => repo.GetUserIdByPostId(VALID_POST_ID), Times.Once);
        }
        
        [Fact]
        public void ValidatePostOwnership_PostDoesNotExist_ReturnsFalse()
        {
            // Arrange
            _mockPostRepository.Setup(repo => repo.GetUserIdByPostId(VALID_POST_ID))
                .Returns((int?)null);
            
            // Act
            bool result = _postService.ValidatePostOwnership(VALID_USER_ID, VALID_POST_ID);
            
            // Assert
            Assert.False(result);
            _mockPostRepository.Verify(repo => repo.GetUserIdByPostId(VALID_POST_ID), Times.Once);
        }
        
        #endregion
        
        #region UpdatePost Tests
        
        [Fact]
        public void UpdatePost_WithValidPost_UpdatesSuccessfully()
        {
            // Arrange
            var post = new Post
            {
                Id = VALID_POST_ID,
                Title = "Updated Title",
                Description = "Updated Description",
                UserID = VALID_USER_ID
            };
            
            _mockPostRepository.Setup(repo => repo.UpdatePost(It.IsAny<Post>()))
                .Verifiable();
            
            // Act
            _postService.UpdatePost(post);
            
            // Assert
            _mockPostRepository.Verify(repo => repo.UpdatePost(It.Is<Post>(p => 
                p.Id == VALID_POST_ID && 
                p.Title == "Updated Title" && 
                p.Description == "Updated Description")), Times.Once);
        }
        
        [Fact]
        public void UpdatePost_WithInvalidId_ThrowsArgumentException()
        {
            // Arrange
            var post = new Post
            {
                Id = INVALID_POST_ID,
                Title = "Updated Title",
                Description = "Updated Description"
            };
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.UpdatePost(post));
        }
        
        [Fact]
        public void UpdatePost_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var post = new Post
            {
                Id = VALID_POST_ID,
                Title = "Updated Title",
                Description = "Updated Description"
            };
            
            _mockPostRepository.Setup(repo => repo.UpdatePost(It.IsAny<Post>()))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.UpdatePost(post));
        }
        
        #endregion
        
        #region GetPostCountByHashtags Tests
        
        [Fact]
        public void GetPostCountByHashtags_WithValidHashtags_ReturnsCount()
        {
            // Arrange
            const int expectedCount = 3;
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            _mockPostRepository.Setup(repo => repo.GetPostCountByHashtags(It.IsAny<List<string>>()))
                .Returns(expectedCount);
            
            // Act
            int result = _postService.GetPostCountByHashtags(hashtags);
            
            // Assert
            Assert.Equal(expectedCount, result);
            _mockPostRepository.Verify(repo => repo.GetPostCountByHashtags(It.Is<List<string>>(
                list => list.Count == 2 && list.Contains("Tag1") && list.Contains("Tag2"))), Times.Once);
        }
        
        [Fact]
        public void GetPostCountByHashtags_WithNullHashtags_ReturnsTotalPostCount()
        {
            // Arrange
            const int expectedTotalCount = 10;
            List<string> hashtags = null;
            
            _mockPostRepository.Setup(repo => repo.GetTotalPostCount())
                .Returns(expectedTotalCount);
            
            // Act
            int result = _postService.GetPostCountByHashtags(hashtags);
            
            // Assert
            Assert.Equal(expectedTotalCount, result);
            _mockPostRepository.Verify(repo => repo.GetTotalPostCount(), Times.Once);
            _mockPostRepository.Verify(repo => repo.GetPostCountByHashtags(It.IsAny<List<string>>()), Times.Never);
        }
        
        [Fact]
        public void GetPostCountByHashtags_WithEmptyHashtags_ReturnsTotalPostCount()
        {
            // Arrange
            const int expectedTotalCount = 10;
            var hashtags = new List<string>();
            
            _mockPostRepository.Setup(repo => repo.GetTotalPostCount())
                .Returns(expectedTotalCount);
            
            // Act
            int result = _postService.GetPostCountByHashtags(hashtags);
            
            // Assert
            Assert.Equal(expectedTotalCount, result);
            _mockPostRepository.Verify(repo => repo.GetTotalPostCount(), Times.Once);
            _mockPostRepository.Verify(repo => repo.GetPostCountByHashtags(It.IsAny<List<string>>()), Times.Never);
        }
        
        [Fact]
        public void GetPostCountByHashtags_WithEmptyStringInList_FiltersEmptyStrings()
        {
            // Arrange
            const int expectedTotalCount = 10;
            var hashtags = new List<string> { "", "   ", null };
            
            _mockPostRepository.Setup(repo => repo.GetTotalPostCount())
                .Returns(expectedTotalCount);
            
            // Act
            int result = _postService.GetPostCountByHashtags(hashtags);
            
            // Assert
            Assert.Equal(expectedTotalCount, result);
            _mockPostRepository.Verify(repo => repo.GetTotalPostCount(), Times.Once);
            _mockPostRepository.Verify(repo => repo.GetPostCountByHashtags(It.IsAny<List<string>>()), Times.Never);
        }
        
        [Fact]
        public void GetPostCountByHashtags_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            _mockPostRepository.Setup(repo => repo.GetPostCountByHashtags(It.IsAny<List<string>>()))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.GetPostCountByHashtags(hashtags));
        }
        
        #endregion
        
        #region GetHashtagsByCategory Tests
        
        [Fact]
        public void GetHashtagsByCategory_WithValidCategoryId_ReturnsHashtags()
        {
            // Arrange
            var expectedHashtags = new List<Hashtag>
            {
                new Hashtag(1, "CategoryTag1"),
                new Hashtag(2, "CategoryTag2")
            };
            
            _mockHashtagRepository.Setup(repo => repo.GetHashtagsByCategory(VALID_CATEGORY_ID))
                .Returns(expectedHashtags);
            
            // Act
            var result = _postService.GetHashtagsByCategory(VALID_CATEGORY_ID);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedHashtags.Count, result.Count);
            Assert.Equal(expectedHashtags[0].Tag, result[0].Tag);
            Assert.Equal(expectedHashtags[1].Tag, result[1].Tag);
            _mockHashtagRepository.Verify(repo => repo.GetHashtagsByCategory(VALID_CATEGORY_ID), Times.Once);
        }
        
        [Fact]
        public void GetHashtagsByCategory_WithInvalidCategoryId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.GetHashtagsByCategory(INVALID_POST_ID));
        }
        
        [Fact]
        public void GetHashtagsByCategory_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockHashtagRepository.Setup(repo => repo.GetHashtagsByCategory(VALID_CATEGORY_ID))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.GetHashtagsByCategory(VALID_CATEGORY_ID));
        }
        
        #endregion
        
        #region GetPostsByHashtags Tests
        
        [Fact]
        public void GetPostsByHashtags_WithValidParameters_ReturnsPosts()
        {
            // Arrange
            var expectedPosts = new List<Post>
            {
                new Post { Id = 1, Title = "Tagged Post 1" },
                new Post { Id = 2, Title = "Tagged Post 2" }
            };
            
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            _mockPostRepository.Setup(repo => repo.GetPostsByHashtags(
                It.IsAny<List<string>>(), VALID_PAGE_NUMBER, VALID_PAGE_SIZE))
                .Returns(expectedPosts);
            
            // Act
            var result = _postService.GetPostsByHashtags(hashtags, VALID_PAGE_NUMBER, VALID_PAGE_SIZE);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPosts.Count, result.Count);
            Assert.Equal(expectedPosts[0].Title, result[0].Title);
            Assert.Equal(expectedPosts[1].Title, result[1].Title);
            _mockPostRepository.Verify(repo => repo.GetPostsByHashtags(
                It.Is<List<string>>(list => list.Count == 2 && list.Contains("Tag1") && list.Contains("Tag2")), 
                VALID_PAGE_NUMBER, VALID_PAGE_SIZE), Times.Once);
        }
        
        [Fact]
        public void GetPostsByHashtags_WithInvalidPageNumber_ThrowsArgumentException()
        {
            // Arrange
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.GetPostsByHashtags(hashtags, 0, VALID_PAGE_SIZE));
        }
        
        [Fact]
        public void GetPostsByHashtags_WithInvalidPageSize_ThrowsArgumentException()
        {
            // Arrange
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.GetPostsByHashtags(hashtags, VALID_PAGE_NUMBER, 0));
        }
        
        [Fact]
        public void GetPostsByHashtags_WithNullHashtags_ReturnsPaginatedPosts()
        {
            // Arrange
            var expectedPosts = new List<Post>
            {
                new Post { Id = 1, Title = "Post 1" },
                new Post { Id = 2, Title = "Post 2" }
            };
            
            List<string> hashtags = null;
            
            _mockPostRepository.Setup(repo => repo.GetPaginatedPosts(VALID_PAGE_NUMBER, VALID_PAGE_SIZE))
                .Returns(expectedPosts);
            
            // Act
            var result = _postService.GetPostsByHashtags(hashtags, VALID_PAGE_NUMBER, VALID_PAGE_SIZE);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPosts.Count, result.Count);
            _mockPostRepository.Verify(repo => repo.GetPaginatedPosts(VALID_PAGE_NUMBER, VALID_PAGE_SIZE), Times.Once);
            _mockPostRepository.Verify(repo => repo.GetPostsByHashtags(
                It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
        
        [Fact]
        public void GetPostsByHashtags_WithEmptyHashtags_ReturnsPaginatedPosts()
        {
            // Arrange
            var expectedPosts = new List<Post>
            {
                new Post { Id = 1, Title = "Post 1" },
                new Post { Id = 2, Title = "Post 2" }
            };
            
            var hashtags = new List<string>();
            
            _mockPostRepository.Setup(repo => repo.GetPaginatedPosts(VALID_PAGE_NUMBER, VALID_PAGE_SIZE))
                .Returns(expectedPosts);
            
            // Act
            var result = _postService.GetPostsByHashtags(hashtags, VALID_PAGE_NUMBER, VALID_PAGE_SIZE);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPosts.Count, result.Count);
            _mockPostRepository.Verify(repo => repo.GetPaginatedPosts(VALID_PAGE_NUMBER, VALID_PAGE_SIZE), Times.Once);
            _mockPostRepository.Verify(repo => repo.GetPostsByHashtags(
                It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
        
        [Fact]
        public void GetPostsByHashtags_WithWhitespaceHashtags_FiltersEmptyStrings()
        {
            // Arrange
            var expectedPosts = new List<Post>
            {
                new Post { Id = 1, Title = "Post 1" },
                new Post { Id = 2, Title = "Post 2" }
            };
            
            var hashtags = new List<string> { "", "   ", null };
            
            _mockPostRepository.Setup(repo => repo.GetPaginatedPosts(VALID_PAGE_NUMBER, VALID_PAGE_SIZE))
                .Returns(expectedPosts);
            
            // Act
            var result = _postService.GetPostsByHashtags(hashtags, VALID_PAGE_NUMBER, VALID_PAGE_SIZE);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPosts.Count, result.Count);
            _mockPostRepository.Verify(repo => repo.GetPaginatedPosts(VALID_PAGE_NUMBER, VALID_PAGE_SIZE), Times.Once);
            _mockPostRepository.Verify(repo => repo.GetPostsByHashtags(
                It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
        
        [Fact]
        public void GetPostsByHashtags_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var hashtags = new List<string> { "Tag1", "Tag2" };
            
            _mockPostRepository.Setup(repo => repo.GetPostsByHashtags(
                It.IsAny<List<string>>(), VALID_PAGE_NUMBER, VALID_PAGE_SIZE))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.GetPostsByHashtags(hashtags, VALID_PAGE_NUMBER, VALID_PAGE_SIZE));
        }
        
        #endregion
        
        #region RemoveHashtagFromPost Tests
        
        [Fact]
        public void RemoveHashtagFromPost_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var currentUser = new User(VALID_USER_ID, "TestUser");
            
            _mockUserService.Setup(service => service.GetCurrentUser())
                .Returns(currentUser);
            
            _mockHashtagRepository.Setup(repo => repo.RemoveHashtagFromPost(VALID_POST_ID, VALID_HASHTAG_ID))
                .Returns(true);
            
            // Act
            bool result = _postService.RemoveHashtagFromPost(VALID_POST_ID, VALID_HASHTAG_ID, VALID_USER_ID);
            
            // Assert
            Assert.True(result);
            _mockUserService.Verify(service => service.GetCurrentUser(), Times.Once);
            _mockHashtagRepository.Verify(repo => repo.RemoveHashtagFromPost(VALID_POST_ID, VALID_HASHTAG_ID), Times.Once);
        }
        
        [Fact]
        public void RemoveHashtagFromPost_WithInvalidPostId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.RemoveHashtagFromPost(INVALID_POST_ID, VALID_HASHTAG_ID, VALID_USER_ID));
        }
        
        [Fact]
        public void RemoveHashtagFromPost_WithInvalidHashtagId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.RemoveHashtagFromPost(VALID_POST_ID, INVALID_POST_ID, VALID_USER_ID));
        }
        
        [Fact]
        public void RemoveHashtagFromPost_WithInvalidUserId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _postService.RemoveHashtagFromPost(VALID_POST_ID, VALID_HASHTAG_ID, INVALID_POST_ID));
        }
        
        [Fact]
        public void RemoveHashtagFromPost_UserNotAuthorized_ThrowsException()
        {
            // Arrange
            var currentUser = new User(VALID_USER_ID + 1, "DifferentUser");
            
            _mockUserService.Setup(service => service.GetCurrentUser())
                .Returns(currentUser);
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.RemoveHashtagFromPost(VALID_POST_ID, VALID_HASHTAG_ID, VALID_USER_ID));
        }
        
        [Fact]
        public void RemoveHashtagFromPost_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var currentUser = new User(VALID_USER_ID, "TestUser");
            
            _mockUserService.Setup(service => service.GetCurrentUser())
                .Returns(currentUser);
            
            _mockHashtagRepository.Setup(repo => repo.RemoveHashtagFromPost(VALID_POST_ID, VALID_HASHTAG_ID))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _postService.RemoveHashtagFromPost(VALID_POST_ID, VALID_HASHTAG_ID, VALID_USER_ID));
        }
        
        #endregion
    }
} 