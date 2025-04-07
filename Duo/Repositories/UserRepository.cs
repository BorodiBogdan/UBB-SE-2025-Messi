using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Duo.Models;
using Duo.Data;
using Duo.Repositories.Interfaces;

namespace Duo.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnection dataLink;
        public UserRepository(IDatabaseConnection dataLink)
        {
            this.dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }
        
        public int CreateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("Username cannot be empty.");
            }

            var existingUser = GetUserByUsername(user.Username);
            if (existingUser != null)
            {
                return existingUser.UserId;
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", user.Username),
            };
            
            int? result = dataLink.ExecuteScalar<int>("CreateUser", parameters);
            return result ?? 0;
        }

        public User GetUserById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid user ID.");
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", id)
            };

            DataTable? dataTable = null;
            try
            {
                dataTable = dataLink.ExecuteReader("GetUserByID", parameters);
                if (dataTable.Rows.Count == 0)
                {
                    throw new Exception("User not found.");
                }
                var row = dataTable.Rows[0];
                return new User(
                    Convert.ToInt32(row[0]),
                    row[1]?.ToString() ?? string.Empty
                );
            }
            finally
            {
                dataTable?.Dispose();
            }
        }

        public User GetUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Invalid username.");
            }
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", username)
            };
            DataTable? dataTable = null;
            try
            {
                dataTable = dataLink.ExecuteReader("GetUserByUsername", parameters);
                if (dataTable.Rows.Count == 0)
                {
                    return null;
                }

                var row = dataTable.Rows[0];
                
                return new User(
                    Convert.ToInt32(row["userID"]),
                    row["username"]?.ToString() ?? string.Empty
                );
            }
            finally
            {
                dataTable?.Dispose();
            }
        }
    }
}

