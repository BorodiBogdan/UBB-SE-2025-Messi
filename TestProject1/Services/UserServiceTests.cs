using Moq;
using System;
using System.Collections.Generic;
using Duo.Models;
using Duo.Repositories.Interfaces;
using Duo.Services;
using Xunit;

namespace TestProject1.Services
{
    public class UserServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private UserService _userService;
        
        // Test data
        private const int VALID_USER_ID = 1;
        private const int INVALID_USER_ID = -1;
        private const string VALID_USERNAME = "validUsername";
        private const string INVALID_USERNAME = "";
        
        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepository.Object);
        }
        
        #region Constructor Tests
        
        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserService(null));
        }
        
        #endregion
        
        #region SetUser Tests
        
        [Fact]
        public void SetUser_WithExistingUser_SetsCurrentUser()
        {
            // Arrange
            var existingUser = new User(VALID_USER_ID, VALID_USERNAME);
            
            _mockUserRepository.Setup(repo => repo.GetUserByUsername(VALID_USERNAME))
                .Returns(existingUser);
            
            // Act
            _userService.setUser(VALID_USERNAME);
            
            // Assert
            var currentUser = _userService.GetCurrentUser();
            Assert.NotNull(currentUser);
            Assert.Equal(VALID_USER_ID, currentUser.UserId);
            Assert.Equal(VALID_USERNAME, currentUser.Username);
            _mockUserRepository.Verify(repo => repo.GetUserByUsername(VALID_USERNAME), Times.Once);
            _mockUserRepository.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Never);
        }
        
        [Fact]
        public void SetUser_WithNewUser_CreatesAndSetsCurrentUser()
        {
            // Arrange
            var newUsername = "newUser";
            
            _mockUserRepository.Setup(repo => repo.GetUserByUsername(newUsername))
                .Returns((User)null);
            
            _mockUserRepository.Setup(repo => repo.CreateUser(It.Is<User>(u => u.Username == newUsername)))
                .Returns(VALID_USER_ID);
            
            // Second call after user creation should return the new user
            _mockUserRepository.SetupSequence(repo => repo.GetUserByUsername(newUsername))
                .Returns((User)null)
                .Returns(new User(VALID_USER_ID, newUsername));
            
            // Act
            _userService.setUser(newUsername);
            
            // Assert
            var currentUser = _userService.GetCurrentUser();
            Assert.NotNull(currentUser);
            Assert.Equal(VALID_USER_ID, currentUser.UserId);
            Assert.Equal(newUsername, currentUser.Username);
            _mockUserRepository.Verify(repo => repo.GetUserByUsername(newUsername), Times.AtLeastOnce);
            _mockUserRepository.Verify(repo => repo.CreateUser(It.Is<User>(u => u.Username == newUsername)), Times.Once);
        }
        
        [Fact]
        public void SetUser_WithNullOrEmptyUsername_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _userService.setUser(null));
            Assert.Throws<ArgumentException>(() => _userService.setUser(""));
            Assert.Throws<ArgumentException>(() => _userService.setUser("   "));
        }
        
        [Fact]
        public void SetUser_CreateUserThrowsException_FallsBackToGetUser()
        {
            // Arrange
            var username = "failedCreateUser";
            
            // First call returns null (user doesn't exist)
            // Second call returns a user (after fallback)
            _mockUserRepository.SetupSequence(repo => repo.GetUserByUsername(username))
                .Returns((User)null)
                .Returns(new User(VALID_USER_ID, username));
            
            // CreateUser throws an exception
            _mockUserRepository.Setup(repo => repo.CreateUser(It.IsAny<User>()))
                .Throws(new Exception("Database error"));
            
            // Act
            _userService.setUser(username);
            
            // Assert
            var currentUser = _userService.GetCurrentUser();
            Assert.NotNull(currentUser);
            Assert.Equal(VALID_USER_ID, currentUser.UserId);
            Assert.Equal(username, currentUser.Username);
        }
        
        [Fact]
        public void SetUser_BothCreateAndGetUserFail_ThrowsException()
        {
            // Arrange
            var username = "totalFailure";
            
            // GetUserByUsername always returns null
            _mockUserRepository.Setup(repo => repo.GetUserByUsername(username))
                .Returns((User)null);
            
            // CreateUser throws an exception
            _mockUserRepository.Setup(repo => repo.CreateUser(It.IsAny<User>()))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _userService.setUser(username));
        }
        
        #endregion
        
        #region GetCurrentUser Tests
        
        [Fact]
        public void GetCurrentUser_WhenUserIsSet_ReturnsCurrentUser()
        {
            // Arrange
            var user = new User(VALID_USER_ID, VALID_USERNAME);
            
            _mockUserRepository.Setup(repo => repo.GetUserByUsername(VALID_USERNAME))
                .Returns(user);
            
            _userService.setUser(VALID_USERNAME);
            
            // Act
            var result = _userService.GetCurrentUser();
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(VALID_USER_ID, result.UserId);
            Assert.Equal(VALID_USERNAME, result.Username);
        }
        
        [Fact]
        public void GetCurrentUser_WhenNoUserIsSet_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _userService.GetCurrentUser());
        }
        
        #endregion
        
        #region GetUserById Tests
        
        [Fact]
        public void GetUserById_WithValidId_ReturnsUser()
        {
            // Arrange
            var expectedUser = new User(VALID_USER_ID, VALID_USERNAME);
            
            _mockUserRepository.Setup(repo => repo.GetUserById(VALID_USER_ID))
                .Returns(expectedUser);
            
            // Act
            var result = _userService.GetUserById(VALID_USER_ID);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(VALID_USER_ID, result.UserId);
            Assert.Equal(VALID_USERNAME, result.Username);
            _mockUserRepository.Verify(repo => repo.GetUserById(VALID_USER_ID), Times.Once);
        }
        
        [Fact]
        public void GetUserById_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(repo => repo.GetUserById(INVALID_USER_ID))
                .Returns((User)null);
            
            // Act
            var result = _userService.GetUserById(INVALID_USER_ID);
            
            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(repo => repo.GetUserById(INVALID_USER_ID), Times.Once);
        }
        
        [Fact]
        public void GetUserById_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockUserRepository.Setup(repo => repo.GetUserById(VALID_USER_ID))
                .Throws(new Exception("Database error"));
            
            // Act & Assert
            Assert.Throws<Exception>(() => _userService.GetUserById(VALID_USER_ID));
            _mockUserRepository.Verify(repo => repo.GetUserById(VALID_USER_ID), Times.Once);
        }
        
        #endregion
        
        #region GetUserByUsername Tests
        
        [Fact]
        public void GetUserByUsername_WithValidUsername_ReturnsUser()
        {
            // Arrange
            var expectedUser = new User(VALID_USER_ID, VALID_USERNAME);
            
            _mockUserRepository.Setup(repo => repo.GetUserByUsername(VALID_USERNAME))
                .Returns(expectedUser);
            
            // Act
            var result = _userService.GetUserByUsername(VALID_USERNAME);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(VALID_USER_ID, result.UserId);
            Assert.Equal(VALID_USERNAME, result.Username);
            _mockUserRepository.Verify(repo => repo.GetUserByUsername(VALID_USERNAME), Times.Once);
        }
        
        [Fact]
        public void GetUserByUsername_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(repo => repo.GetUserByUsername("nonexistent"))
                .Returns((User)null);
            
            // Act
            var result = _userService.GetUserByUsername("nonexistent");
            
            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(repo => repo.GetUserByUsername("nonexistent"), Times.Once);
        }
        
        [Fact]
        public void GetUserByUsername_RepositoryThrowsException_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(repo => repo.GetUserByUsername(VALID_USERNAME))
                .Throws(new Exception("Database error"));
            
            // Act
            var result = _userService.GetUserByUsername(VALID_USERNAME);
            
            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(repo => repo.GetUserByUsername(VALID_USERNAME), Times.Once);
        }
        
        #endregion
    }
}