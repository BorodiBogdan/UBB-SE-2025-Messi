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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDatabaseConnection _dataLink;

        public CategoryRepository(IDatabaseConnection dataLink)
        {
            _dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }

        /// <inheritdoc/> //this line is used to specify that the method is implemented from the interface
        public List<Category> GetCategories(SqlParameter[] parameters = null)
        {
            List<Category> categories = new List<Category>();
            DataTable dataTable = null;

            try
            {
                dataTable = _dataLink.ExecuteReader("GetCategories", parameters);

                foreach (DataRow row in dataTable.Rows)
                {
                    categories.Add(new Category(
                        Convert.ToInt32(row["Id"]),
                        row["Name"] != DBNull.Value ? row["Name"].ToString() : ""
                    ));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving categories: {ex.Message}");
            }
            finally
            {
                dataTable?.Dispose();
            }

            return categories;
        }

        /// <inheritdoc/> 
        public Category GetCategoryByName(string name)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                new SqlParameter("@Name", name)
                };
                var dataTable = _dataLink.ExecuteReader("GetCategoryByName", parameters);

                if (dataTable.Rows.Count == 0)
                {
                    throw new Exception($"Category '{name}' not found.");
                }

                DataRow row = dataTable.Rows[0];
                return new Category(
                    Convert.ToInt32(row["Id"]),
                    row["Name"] != DBNull.Value ? row["Name"].ToString() : ""
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching category '{name}': {ex.Message}");
            }
        }
    }
}