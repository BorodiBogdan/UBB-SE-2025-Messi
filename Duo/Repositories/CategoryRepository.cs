namespace Duo.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using Duo.Data;
    using Duo.Models;
    using Duo.Repositories.Interfaces;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Repository for managing categories.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
    /// </remarks>
    /// <param name="dataLink">The database connection.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataLink is null.</exception>
    public class CategoryRepository(IDatabaseConnection dataLink) : ICategoryRepository
    {
        // Constants for stored procedure names
        private const string GetCategoriesProcedure = "GetCategories";
        private const string GetCategoryByNameProcedure = "GetCategoryByName";

        // Constants for parameter names
        private const string NameParameter = "@Name";

        // Constants for column names
        private const string IdColumn = "Id";
        private const string NameColumn = "Name";

        // Constants for error messages
        private const string ErrorRetrievingCategories = "Error retrieving categories: {0}";
        private const string ErrorFetchingCategory = "Error fetching category '{0}': {1}";
        private const string CategoryNotFound = "Category '{0}' not found.";

        private readonly IDatabaseConnection dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));

        /// <inheritdoc/>
        /// <summary>
        /// Retrieves a list of categories.
        /// </summary>
        /// <param name="parameters">Optional SQL parameters.</param>
        /// <returns>A list of categories.</returns>
        public List<Category> GetCategories(SqlParameter[]? parameters = null)
        {
            List<Category> categories = new List<Category>();
            DataTable dataTable = new DataTable();

            try
            {
                dataTable = this.dataLink.ExecuteReader(GetCategoriesProcedure, parameters);

                foreach (DataRow row in dataTable.Rows)
                {
                    categories.Add(new Category(
                        Convert.ToInt32(row[IdColumn]),
                        row[NameColumn] != DBNull.Value ? row[NameColumn]?.ToString() ?? string.Empty : string.Empty));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(ErrorRetrievingCategories, ex.Message));
            }
            finally
            {
                dataTable?.Dispose();
            }

            return categories;
        }

        /// <inheritdoc/> 
        /// <summary>
        /// Retrieves a category by its name.
        /// </summary>
        /// <param name="name">The name of the category.</param>
        /// <returns>The category with the specified name.</returns>
        /// <exception cref="Exception">Thrown when the category is not found or an error occurs.</exception>
        public Category GetCategoryByName(string name)
        {
            try
            {
                SqlParameter[] parameters = new SqlParameter[]
                {
                            new SqlParameter(NameParameter, name),
                };
                var dataTable = this.dataLink.ExecuteReader(GetCategoryByNameProcedure, parameters);

                if (dataTable.Rows.Count == 0)
                {
                    throw new Exception(string.Format(CategoryNotFound, name));
                }

                DataRow row = dataTable.Rows[0];
                return new Category(
                    Convert.ToInt32(row[IdColumn]),
                    row[NameColumn] != DBNull.Value ? row[NameColumn]?.ToString() ?? string.Empty : string.Empty);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(ErrorFetchingCategory, name, ex.Message));
            }
        }
    }
}