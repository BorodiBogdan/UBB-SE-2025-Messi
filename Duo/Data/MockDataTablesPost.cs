// <copyright file="MockDataTablesPost.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Duo.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides mock data tables for testing post-related operations.
    /// </summary>
    public class MockDataTablesPost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockDataTablesPost"/> class.
        /// </summary>
        public MockDataTablesPost()
        {
            this.PostRepositoryDataTable = new DataTable
            {
                Columns =
                {
                    new DataColumn("Id", typeof(int)),
                    new DataColumn("Title", typeof(string)),
                    new DataColumn("Description", typeof(string)),
                    new DataColumn("UserID", typeof(int)),
                    new DataColumn("CategoryID", typeof(int)),
                    new DataColumn("CreatedAt", typeof(DateTime)),
                    new DataColumn("UpdatedAt", typeof(DateTime)),
                    new DataColumn("LikeCount", typeof(int)),
                },
                Rows =
                {
                    { 1, "First Post", "This is the first post description", 1, 1, DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-3), 5 },
                    { 2, "Second Post", "This is the second post description", 1, 2, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-2), 10 },
                    { 3, "Third Post", "This is the third post description", 2, 1, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1), 15 },
                    { 4, "Fourth Post", "This is the fourth post description", 2, 2, DateTime.Now, DateTime.Now, 0 },
                    { 5, "Fifth Post", "This is the fifth post description", 3, 3, DateTime.Now, DateTime.Now, 0 },
                    { 6, "Searchable Post", "This post has a searchable title", 3, 3, DateTime.Now, DateTime.Now, 0 },
                    { 7, "First Category Post", "This is a category 1 post", 4, 1, DateTime.Now, DateTime.Now, 0 },
                    { 8, "User Post", "This is user 1's post", 1, 4, DateTime.Now, DateTime.Now, 0 },
                    { 9, "User Post 2", "This is another user 1's post", 1, 4, DateTime.Now, DateTime.Now, 0 },
                },
            };
        }
        /// <summary>
        /// Gets or sets the mock data table for post repository testing.
        /// </summary>
        public DataTable PostRepositoryDataTable { get; set; }
    }
} 