using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Duo.Models;
using Duo.Data;

namespace Duo.Repositories
{
    public class CategoryRepository
    {
        private readonly IDatabaseConnection _dataLink;

        public CategoryRepository(IDatabaseConnection dataLink)
        {
            _dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }

        public List<Category> GetCategories()
        {
            List<Category> categories = new List<Category>();
            DataTable dataTable = null;

            try
            {
                dataTable = _dataLink.ExecuteReader("GetCategories");

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