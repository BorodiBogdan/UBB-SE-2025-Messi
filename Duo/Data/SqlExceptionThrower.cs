using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Data
{
    public class SqlExceptionThrower
    {
        public SqlExceptionThrower()
        {
            // Constructor
        }
        public SqlException throwSqlException()
        {
            // Use reflection to create one
            var sqlErrorNumber = 1234; // any SQL error number
            var collectionConstructor = typeof(SqlErrorCollection)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
            var errorCollection = (SqlErrorCollection)collectionConstructor.Invoke(null);

            var errorConstructor = typeof(SqlError)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(Exception) }, null);

            var sqlError = (SqlError)errorConstructor.Invoke(new object[]
            {
        sqlErrorNumber, (byte)0, (byte)0, "server", "error message", "proc", 0, null
            });

            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(errorCollection, new object[] { sqlError });

            var exceptionConstructor = typeof(SqlException)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new[] { typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid) }, null);

            var sqlException = (SqlException)exceptionConstructor.Invoke(new object[]
            {
        "Mocked SqlException", errorCollection, null, Guid.NewGuid()
            });

            return sqlException;
        }

    }
}
