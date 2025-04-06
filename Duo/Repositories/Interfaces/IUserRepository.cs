using Duo.Models;

namespace Duo.Repositories.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>
        /// Creates a new user or returns an existing user with the same username
        /// </summary>
        /// <param name="user">The user to create</param>
        /// <returns>The ID of the created or existing user</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the user is null</exception>
        /// <exception cref="System.ArgumentException">Thrown when the username is empty</exception>
        int CreateUser(User user);

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="id">The ID of the user to retrieve</param>
        /// <returns>The user with the specified ID</returns>
        /// <exception cref="System.ArgumentException">Thrown when the ID is invalid</exception>
        /// <exception cref="System.Exception">Thrown when the user is not found or an error occurs</exception>
        User GetUserById(int id);

        /// <summary>
        /// Gets a user by their username
        /// </summary>
        /// <param name="username">The username of the user to retrieve</param>
        /// <returns>The user with the specified username, or null if not found</returns>
        /// <exception cref="System.ArgumentException">Thrown when the username is empty</exception>
        User GetUserByUsername(string username);
    }
} 