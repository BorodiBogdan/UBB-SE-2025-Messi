using System;
using System.Collections.Generic;
using System.Linq;
using Duo.Services;
using Duo.Services.Interfaces;
using Xunit;
using Moq;

namespace TestProject1.Services
{
    public class SearchServiceTests
    {
        private readonly ISearchService _searchService;
        private readonly Mock<ISearchService> _mockSearchService;

        // Test constants
        private const string TEST_QUERY = "test";
        private const string EMPTY_QUERY = "";
        private const string NULL_QUERY = null;
        private const double DEFAULT_SIMILARITY_THRESHOLD = 0.6;
        private const double HIGH_SIMILARITY_THRESHOLD = 0.8;
        private const double EXACT_MATCH_THRESHOLD = 1.0;
        private const double MEDIUM_SIMILARITY_THRESHOLD = 0.5;
        
        // Test string constants
        private const string SAMPLE_WORD_1 = "kitten";
        private const string SAMPLE_WORD_2 = "sitting";
        private const string TEST_STRING_SHORT = "test";
        private const string TEST_STRING_LONG = "testing";
        private const string COMPLETELY_DIFFERENT_STRING_1 = "abc";
        private const string COMPLETELY_DIFFERENT_STRING_2 = "xyz";

        // Test data sets
        private static readonly List<string> STANDARD_TEST_CANDIDATES = new() { "test1", "test2", "test3" };
        private static readonly List<string> SIMILAR_WORD_CANDIDATES = new() { "testing", "test", "tester" };
        private static readonly List<string> MULTI_WORD_CANDIDATES = new() { "hello world", "test world", "world test" };

        private static readonly List<string> TEST_CANDIDATES = new() 
        { 
            "test",          // Exact match
            "tst",           // Similar word
            "testing",       // Contains query
            "hello test",    // Multi-word with exact match
            "hello tst",     // Multi-word with similar word
            "xyz",           // No match
            "te"            // Shorter string contained in query
        };

        public SearchServiceTests()
        {
            _mockSearchService = new Mock<ISearchService>();
            _searchService = new SearchService();
        }

        #region LevenshteinSimilarity Tests

        [Theory]
        [InlineData("kitten", "sitting", true)]  // Similar strings
        [InlineData("test", "test", true)]       // Identical strings
        [InlineData("", "", true)]               // Empty strings
        [InlineData("abc", "xyz", false)]        // Different strings
        [InlineData("test", "", false)]          // One empty string
        public void LevenshteinSimilarity_ReturnsExpectedSimilarity(string source, string target, bool shouldBeHighSimilarity)
        {
            // Act
            double similarityScore = _searchService.LevenshteinSimilarity(source, target);

            // Assert
            if (shouldBeHighSimilarity)
            {
                Assert.True(similarityScore > 0.5);
            }
            else
            {
                Assert.True(similarityScore <= 0.5);
            }
        }

        #endregion

        #region FindFuzzySearchMatches Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void FindFuzzySearchMatches_WithInvalidQuery_ReturnsEmptyList(string query)
        {
            var result = _searchService.FindFuzzySearchMatches(query, TEST_CANDIDATES);
            Assert.Empty(result);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithValidQuery_ReturnsMatchesInCorrectOrder()
        {
            // Act
            var results = _searchService.FindFuzzySearchMatches(TEST_QUERY, TEST_CANDIDATES).ToList();

            // Assert
            Assert.NotEmpty(results);
            
            // Exact match should be first
            Assert.Equal("test", results.First());
            
            // Similar matches should be included
            Assert.Contains("testing", results);
            Assert.Contains("hello test", results);
            
            // Non-matching strings should not be included
            Assert.DoesNotContain("xyz", results);

            // Verify order (exact matches before partial matches)
            if (results.Contains("test") && results.Contains("tst"))
            {
                Assert.True(results.IndexOf("test") < results.IndexOf("tst"));
            }
        }

        [Fact]
        public void FindFuzzySearchMatches_WithCustomThreshold_RespectsThreshold()
        {
            // Act
            var highThresholdResults = _searchService.FindFuzzySearchMatches(TEST_QUERY, TEST_CANDIDATES, 0.9);
            var lowThresholdResults = _searchService.FindFuzzySearchMatches(TEST_QUERY, TEST_CANDIDATES, 0.3);

            // Assert
            Assert.True(highThresholdResults.Count < lowThresholdResults.Count);
            Assert.Contains("test", highThresholdResults);
            Assert.Contains("tst", lowThresholdResults);
        }

        [Fact]
        public void FindFuzzySearchMatches_WhenQueryContainsCandidate_IncludesMatch()
        {
            // Arrange
            string longQuery = "testing";
            var candidates = new List<string> { "test", "te", "ing" };

            // Act
            var results = _searchService.FindFuzzySearchMatches(longQuery, candidates).ToList();

            // Assert
            Assert.Contains("te", results);  // Should be included because "testing" contains "te"
            Assert.Contains("ing", results); // Should be included because "testing" contains "ing"
        }

        #endregion
    }
} 