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
        private const int INVALID_USER_ID = 0;
        private const int EMPTY_RESULT_COUNT = 0;
        private const int FIRST_ROW_INDEX = 0;
        private const int USER_ID_COLUMN_INDEX = 0;
        private const int USERNAME_COLUMN_INDEX = 1;
        private const string USER_ID_COLUMN_NAME = "userID";
        private const string USERNAME_COLUMN_NAME = "username";
        private const string CREATE_USER_PROCEDURE = "CreateUser";
        private const string GET_USER_BY_ID_PROCEDURE = "GetUserByID";
        private const string GET_USER_BY_USERNAME_PROCEDURE = "GetUserByUsername";
        private const string USERNAME_PARAMETER = "@Username";
        private const string USER_ID_PARAMETER = "@UserID";

        private readonly IDatabaseConnection _databaseConnection;

        public UserRepository(IDatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
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
                new SqlParameter(USERNAME_PARAMETER, user.Username),
            };
            
            int? result = _databaseConnection.ExecuteScalar<int>(CREATE_USER_PROCEDURE, parameters);
            return result ?? INVALID_USER_ID;
        }

        public User GetUserById(int userId)
        {
            if (userId <= INVALID_USER_ID)
            {
                throw new ArgumentException("Invalid user ID.");
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter(USER_ID_PARAMETER, userId)
            };

            DataTable? dataTable = null;
            try
            {
                dataTable = _databaseConnection.ExecuteReader(GET_USER_BY_ID_PROCEDURE, parameters);
                if (dataTable.Rows.Count == EMPTY_RESULT_COUNT)
                {
                    throw new Exception("User not found.");
                }
                var row = dataTable.Rows[FIRST_ROW_INDEX];
                return new User(
                    Convert.ToInt32(row[USER_ID_COLUMN_INDEX]),
                    row[USERNAME_COLUMN_INDEX]?.ToString() ?? string.Empty
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
                new SqlParameter(USERNAME_PARAMETER, username)
            };
            DataTable? dataTable = null;
            try
            {
                dataTable = _databaseConnection.ExecuteReader(GET_USER_BY_USERNAME_PROCEDURE, parameters);
                if (dataTable.Rows.Count == EMPTY_RESULT_COUNT)
                {
                    return null;
                }

                var row = dataTable.Rows[FIRST_ROW_INDEX];
                
                return new User(
                    Convert.ToInt32(row[USER_ID_COLUMN_NAME]),
                    row[USERNAME_COLUMN_NAME]?.ToString() ?? string.Empty
                );
            }
            finally
            {
                dataTable?.Dispose();
            }
        }
    }
}

