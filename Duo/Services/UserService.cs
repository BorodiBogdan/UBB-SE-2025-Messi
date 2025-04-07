using System;
using System.Collections.Generic;
using Duo.Models;
using Duo.Repositories.Interfaces;
using Duo.Services.Interfaces;

namespace Duo.Services
{
    public class UserService : IUserService
    {
        private const string NO_USER_LOGGED_IN_MESSAGE = "No user is currently logged in.";
        private const string FAILED_TO_CREATE_OR_FIND_USER_MESSAGE = "Failed to create or find user: {0}";
        private const string FAILED_TO_GET_USER_BY_ID_MESSAGE = "Failed to get user by ID: {0}";
        private const string USERNAME_CANNOT_BE_EMPTY_MESSAGE = "Username cannot be empty";

        private readonly IUserRepository _userRepository;
        private User _currentUser;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public void setUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException(USERNAME_CANNOT_BE_EMPTY_MESSAGE, nameof(username));
            }

            try 
            {
                var existingUser = GetUserByUsername(username);

                if (existingUser != null)
                {
                    _currentUser = existingUser;
                    return;
                }

                var newUser = new User(username);
                int userId = _userRepository.CreateUser(newUser);
                _currentUser = new User(userId, username);
            }
            catch (Exception ex)
            {
                var lastAttemptUser = GetUserByUsername(username);
                if (lastAttemptUser != null)
                {
                    _currentUser = lastAttemptUser;
                    return;
                }

                throw new Exception(string.Format(FAILED_TO_CREATE_OR_FIND_USER_MESSAGE, ex.Message), ex);
            }
        }

        public User GetCurrentUser()
        {
            if (_currentUser == null)
            {
                throw new InvalidOperationException(NO_USER_LOGGED_IN_MESSAGE);
            }
            return _currentUser;
        }

        public User GetUserById(int id)
        {
            try
            {
                return _userRepository.GetUserById(id);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(FAILED_TO_GET_USER_BY_ID_MESSAGE, ex.Message), ex);
            }
        }

        public User GetUserByUsername(string username)
        {
            try
            {
                return _userRepository.GetUserByUsername(username);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
