using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.ObjectModel;
using Duo.Models;
using Duo.Data;
using System.Diagnostics;
using Duo.Repositories.Interfaces;

namespace Duo.Repositories
{
    public class HashtagRepository : IHashtagRepository
    {
        const int EMPTY = 0;
        const int TOP = 0;
        const int INVALID = 0;
        const int QUERRY_ERROR = 0;
        private readonly IDatabaseConnection _dataLink;

        public HashtagRepository(IDatabaseConnection dataLink)
        {
            _dataLink = dataLink;
        }

        public Hashtag GetHashtagByText(string textToSearchBy)
        {
            if(string.IsNullOrWhiteSpace(textToSearchBy)) throw new Exception("Error - GetHashtagByText: Text cannot be null or empty");

            DataTable? dataTable = null;

            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@text", textToSearchBy)
                };

                dataTable = _dataLink.ExecuteReader("GetHashtagByText", sqlParameters);

                if (dataTable.Rows.Count == EMPTY) 
                    throw new Exception("Error - GetHashtagByText: No records found");

                Hashtag foundHashtag = new Hashtag(
                    Convert.ToInt32(dataTable.Rows[TOP]["Id"]),
                    textToSearchBy
                );

                return foundHashtag;
            }
            catch (Exception caughtException)
            {
                return null; 
            }
            finally
            {
                dataTable?.Dispose();
            }

        }

        public Hashtag CreateHashtag(string newHashtagTag)
        {
            if (string.IsNullOrWhiteSpace(newHashtagTag)) throw new Exception("Error - CreateHashtag: Text cannot be null or empty");

            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@Tag", newHashtagTag)
                };
                var result = _dataLink.ExecuteScalar<int>("CreateHashtag", sqlParameters);

                if (result == QUERRY_ERROR) 
                    throw new Exception("Error - CreateHashtag: Hashtag could not be created!");

                Hashtag newHashtag = new Hashtag(
                    result,
                    newHashtagTag
                );

                return newHashtag;
            }
            catch (Exception caughtException)
            {
                throw new Exception($"Error - CreateHashtag: {caughtException.Message}");
            }
        }

        public List<Hashtag> GetHashtagsByPostId(int postId)
        {
            if (postId <= INVALID) throw new Exception("Error - GetHashtagsByPostId: PostId must be greater than 0");
            DataTable? dataTable = null;
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@PostID", postId)
                };
                dataTable = _dataLink.ExecuteReader("GetHashtagsForPost", sqlParameters);

                Debug.WriteLine("am aj  ");
                List<Hashtag> foundHashtags = new List<Hashtag>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var tag = row["Tag"]?.ToString();
                    if (tag == "")
                    {
                        throw new Exception("Error - GetHashtagsByPostId: Tag is null");
                    }
                    Hashtag foundHashtag = new Hashtag(
                        Convert.ToInt32(row["Id"]),
                        tag
                    );
                    foundHashtags.Add(foundHashtag);
                }
                return foundHashtags;
            }
            catch (Exception caughtException)
            {
                throw new Exception($"Error - GetHashtagsByPostId: {caughtException.Message}");
            }
            finally
            {
                dataTable?.Dispose();
            }

        }

        public bool AddHashtagToPost(int postId, int hashtagId)
        {
            if (postId <= INVALID) throw new Exception("Error - AddHashtagToPost: PostId must be greater than 0");
            if (hashtagId <= INVALID) 
                throw new Exception("Error - AddHashtagToPost: HashtagId must be greater than 0");
            
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@PostID", postId),
                    new SqlParameter("@HashtagID", hashtagId)
                };
                
                var result = _dataLink.ExecuteNonQuery("AddHashtagToPost", sqlParameters);
                
                if (result == QUERRY_ERROR)
                {
                    throw new Exception("Error - AddHashtagToPost: Hashtag could not be added to post!");
                }
                
                return true;
            }
            catch (Exception caughtException)
            {
                throw new Exception($"Error - AddHashtagToPost: {caughtException.Message}");
            }
        }
        public bool RemoveHashtagFromPost(int postId, int hashtagId)
        {
            if (postId <= INVALID) throw new Exception("Error - RemoveHashtagFromPost: PostId must be greater than 0");
            if (hashtagId <= INVALID) 
                throw new Exception("Error - RemoveHashtagFromPost: HashtagId must be greater than 0");
            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@PostID", postId),
                    new SqlParameter("@HashtagID", hashtagId)
                };
                var queryResult = _dataLink.ExecuteNonQuery("DeleteHashtagFromPost", sqlParameters);
                if (queryResult == QUERRY_ERROR) 
                    throw new Exception("Error - RemoveHashtagFromPost: Hashtag could not be removed from post!");
                return true;
            }
            catch (Exception caughtException)
            {
                throw new Exception($"Error - RemoveHashtagFromPost: {caughtException.Message}");
            }
        }

        public Hashtag GetHashtagByName(string hashtagName)
        {
            return GetHashtagByText(hashtagName);
        }

        public List<Hashtag> GetAllHashtags()
        {
            DataTable? dataTable = null;

            try
            {
                dataTable = _dataLink.ExecuteReader("GetAllHashtags");

                List<Hashtag> allHashtags = new List<Hashtag>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var tag = row["Tag"]?.ToString();
                    if (tag == "")
                    {
                        continue;
                    }

                    Hashtag currentHashtag = new Hashtag(
                        Convert.ToInt32(row["Id"]),
                        tag
                    );
                    allHashtags.Add(currentHashtag);
                }

                return allHashtags;
            }
            catch (Exception caughtException)
            {
                throw new Exception($"Error - GetAllHashtags: {caughtException.Message}");
            }
            finally
            {
                dataTable?.Dispose();
            }
        }

        public List<Hashtag> GetHashtagsByCategory(int categoryId)
        {
            if (categoryId <= 0) throw new Exception("Error - GetHashtagsByCategory: CategoryId must be greater than 0");

            DataTable? dataTable = null;

            try
            {
                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@CategoryID", categoryId)
                };

                dataTable = _dataLink.ExecuteReader("GetHashtagsByCategory", sqlParameters);

                List<Hashtag> foundHashtags = new List<Hashtag>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var tag = row["Tag"]?.ToString();
                    if (tag == "")
                    {
                        continue;
                    }

                    Hashtag foundHashtag = new Hashtag(
                        Convert.ToInt32(row["Id"]),
                        tag
                    );
                    foundHashtags.Add(foundHashtag);
                }

                return foundHashtags;
            }
            catch (Exception caughtException)
            {
                throw new Exception($"Error - GetHashtagsByCategory: {caughtException.Message}");
            }
            finally
            {
                dataTable?.Dispose();
            }
        }
    }
}
