// <copyright file="ICategoryRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Duo.Repositories.Interfaces
{
    using System.Collections.Generic;
    using Duo.Models;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Interface for Category repository.
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Gets the list of categories.
        /// </summary>
        /// <param name="parameters">The SQL parameters.</param>
        /// <returns>A list of categories.</returns>
        List<Category> GetCategories(SqlParameter[]? parameters = null);

        /// <summary>
        /// Gets a category by its name.
        /// </summary>
        /// <param name="name">The name of the category.</param>
        /// <returns>The category with the specified name.</returns>
        Category GetCategoryByName(string name);
    }
}