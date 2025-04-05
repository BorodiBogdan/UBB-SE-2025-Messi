using Duo.Data;
using Duo.Models;
using Duo.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using System.Data;
using System.Reflection;

namespace TestProject1.Repositories
{
    public class UserRepositoryTests
    {
        private Mock<IDatabaseConnection> _mockDatabase;
        private UserRepository _userRepository;
        private DataTable _userDataTable;

        public UserRepositoryTests()
        {
            _mockDatabase = new Mock<IDatabaseConnection>();
            
            // Setup mock data table
            _userDataTable = new DataTable();
            _userDataTable.Columns.Add("userID", typeof(int));
            _userDataTable.Columns.Add("username", typeof(string));
            _userDataTable.Rows.Add(1, "User1");
            _userDataTable.Rows.Add(2, "User2");
            _userDataTable.Rows.Add(3, "User3");
            
            // Configure mock behavior
            _mockDatabase.Setup(db => db.ExecuteReader("GetUserByID", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && (int)p[0].Value == 1)))
                .Returns(_userDataTable.AsEnumerable().Where(r => r.Field<int>("userID") == 1).CopyToDataTable());
                
            _mockDatabase.Setup(db => db.ExecuteReader("GetUserByID", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && (int)p[0].Value == 0)))
                .Throws(new ArgumentException("Invalid user ID."));
                
            _mockDatabase.Setup(db => db.ExecuteReader("GetUserByID", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && (int)p[0].Value == 40)))
                .Returns(new DataTable());
                
            _mockDatabase.Setup(db => db.ExecuteReader("GetUserByID", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && (int)p[0].Value == 404)))
                .Throws(new Exception("Database error"));
                
            _mockDatabase.Setup(db => db.ExecuteReader("GetUserByUsername", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && p[0].Value.ToString() == "User1")))
                .Returns(_userDataTable.AsEnumerable().Where(r => r.Field<string>("username") == "User1").CopyToDataTable());
                
            _mockDatabase.Setup(db => db.ExecuteReader("GetUserByUsername", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && p[0].Value.ToString() == "NonExistentUser")))
                .Returns(new DataTable());
                
            _mockDatabase.Setup(db => db.ExecuteReader("GetUserByUsername", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && p[0].Value.ToString() == "NewUser")))
                .Returns(new DataTable());
                
            _mockDatabase.Setup(db => db.ExecuteReader("GetUserByUsername", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && p[0].Value.ToString() == "SQLErrorUser")))
                .Throws(new Exception("SQL Error"));
                
            _mockDatabase.Setup(db => db.ExecuteReader("GetUserByUsername", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && p[0].Value.ToString() == "ExistingUser")))
                .Returns(_userDataTable.AsEnumerable().Where(r => r.Field<int>("userID") == 1).CopyToDataTable());
                
            _mockDatabase.Setup(db => db.ExecuteScalar<int>("CreateUser", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && p[0].Value.ToString() == "SQLErrorCreateUser")))
                .Throws(new Exception("SQL Error"));
                
            _mockDatabase.Setup(db => db.ExecuteScalar<int>("CreateUser", It.IsAny<SqlParameter[]>()))
                .Returns(4);

            // Initialize repository with mock
            _userRepository = new UserRepository(_mockDatabase.Object);
        }

        [Fact]
        public void Constructor_WithNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new UserRepository(null));
        }

        [Fact]
        public void GetUserById_WithValidId_ReturnsUser()
        {
            // Act
            var user = _userRepository.GetUserById(1);
            
            // Assert
            Assert.NotNull(user);
            Assert.Equal(1, user.UserId);
            Assert.Equal("User1", user.Username);
            Assert.IsType<User>(user);
        }

        [Fact]
        public void GetUserById_WithInvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _userRepository.GetUserById(0));
        }

        [Fact]
        public void GetUserById_WithNonExistentId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<Exception>(() => _userRepository.GetUserById(40));
        }

        [Fact]
        public void GetUserById_SqlFailure_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<Exception>(() => _userRepository.GetUserById(404));
        }

        [Fact]
        public void GetUserByUsername_WithValidUsername_ReturnsUser()
        {
            // Act
            var user = _userRepository.GetUserByUsername("User1");
            
            // Assert
            Assert.NotNull(user);
            Assert.Equal(1, user.UserId);
            Assert.Equal("User1", user.Username);
        }

        [Fact]
        public void GetUserByUsername_WithNonExistentUsername_ReturnsNull()
        {
            // Act
            var user = _userRepository.GetUserByUsername("NonExistentUser");
            
            // Assert
            Assert.Null(user);
        }

        [Fact]
        public void CreateUser_WithValidUser_ReturnsUserId()
        {
            // Arrange
            var user = new User("NewUser");
            
            // Act
            var userId = _userRepository.CreateUser(user);
            
            // Assert
            Assert.Equal(4, userId);
        }

        [Fact]
        public void CreateUser_WithNullUser_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _userRepository.CreateUser(null));
        }
        
        [Fact]
        public void CreateUser_WithEmptyUsername_ThrowsArgumentException()
        {
            // Arrange
            var user = new User("");
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _userRepository.CreateUser(user));
        }
        
        [Fact]
        public void CreateUser_WithExistingUser_ReturnsExistingUserId()
        {
            // Arrange
            var user = new User("ExistingUser");
            
            // Act
            var userId = _userRepository.CreateUser(user);
            
            // Assert
            Assert.Equal(1, userId);
        }
        
        [Fact]
        public void CreateUser_SqlFailure_ThrowsException()
        {
            // Arrange
            var user = new User("SQLErrorCreateUser");
            
            // Act & Assert
            var ex = Assert.Throws<NullReferenceException>(() => _userRepository.CreateUser(user));
        }
        
        [Fact]
        public void GetUserByUsername_WithEmptyUsername_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _userRepository.GetUserByUsername(""));
        }
        
        [Fact]
        public void GetUserByUsername_SqlFailure_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<Exception>(() => _userRepository.GetUserByUsername("SQLErrorUser"));
        }
    }
} 