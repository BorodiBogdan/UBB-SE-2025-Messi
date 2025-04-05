using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duo.Models;

namespace TestMessi.Models
{
    public class PostModelTest
    {
        [Fact]
        public void TestPostModel()
        {
            Post post_test = new Post();
            
            post_test.Id = 1;
            Assert.Equal(1, post_test.Id);
            post_test.Title = "Test Title";
            Assert.Equal("Test Title", post_test.Title);
            post_test.Description = "Test Description";
            Assert.Equal("Test Description", post_test.Description);
            post_test.UserID = 1;
            Assert.Equal(1, post_test.UserID);
            post_test.CategoryID = 1;
            Assert.Equal(1, post_test.CategoryID);
            post_test.CreatedAt = DateTime.Now;
            Assert.Equal(DateTime.Now.Date, post_test.CreatedAt.Date);
            post_test.UpdatedAt = DateTime.Now;
            Assert.Equal(DateTime.Now.Date, post_test.UpdatedAt.Date);
            post_test.LikeCount = 0;
            Assert.Equal(0, post_test.LikeCount);
            post_test.Content = "Test Content";
            Assert.Equal("Test Content", post_test.Content);
            post_test.Username = "Test User";
            Assert.Equal("Test User", post_test.Username);
            post_test.Date = DateTime.Now.ToString("yyyy-MM-dd");
            Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd"), post_test.Date);
            post_test.Hashtags.Add("Test");
            Assert.Equal(1, post_test.Hashtags.Count);
            Assert.Equal("Test", post_test.Hashtags[0]);
            post_test.Hashtags.Add("Test2");
            Assert.Equal(2, post_test.Hashtags.Count);
            Assert.Equal("Test2", post_test.Hashtags[1]);
            post_test.Hashtags.Add("Test3");
            Assert.Equal(3, post_test.Hashtags.Count);
            Assert.Equal("Test3", post_test.Hashtags[2]);
            post_test.Hashtags.Add("Test4");
            Assert.Equal(4, post_test.Hashtags.Count);
            Assert.Equal("Test4", post_test.Hashtags[3]);
        }
    }
}
