using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Data
{
    public interface IDatabaseConnection
    {

        public void OpenConnection();

        public void CloseConnection();

        public T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null);

        public DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null);

        public int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null);
    }
}

