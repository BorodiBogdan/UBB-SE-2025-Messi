using System;
using System.Collections.Generic;
using Duo.Models;

namespace Duo.Repositories.Interfaces
{
    public interface IHashtagRepository
    {
        Hashtag GetHashtagByText(string textToSearchBy);
        Hashtag CreateHashtag(string newHashtagTag);
        List<Hashtag> GetHashtagsByPostId(int postId);
        bool AddHashtagToPost(int postId, int hashtagId);
        bool RemoveHashtagFromPost(int postId, int hashtagId);
        Hashtag GetHashtagByName(string hashtagName);
        List<Hashtag> GetAllHashtags();
        List<Hashtag> GetHashtagsByCategory(int categoryId);
    }
} 