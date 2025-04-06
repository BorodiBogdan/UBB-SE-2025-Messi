using System;
using System.Collections.Generic;
using Duo.Models;
using Duo.Repositories;
using Duo.Repositories.Interfaces;
using Duo.Services.Interfaces;

namespace Duo.Services
{
    public class UserService: IUserService
    {
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
                throw new ArgumentException("Username cannot be empty", nameof(username));
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

                throw new Exception($"Failed to create or find user: {ex.Message}", ex);
            }
        }

        public User GetCurrentUser()
        {
            if (_currentUser == null)
            {
                throw new InvalidOperationException("No user is currently logged in.");
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
                throw new Exception($"Failed to get user by ID: {ex.Message}", ex);
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
