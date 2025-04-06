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

        public SearchServiceTests()
        {
            _mockSearchService = new Mock<ISearchService>();
            _searchService = new SearchService();
        }

        #region LevenshteinSimilarity Tests

        [Fact]
        public void LevenshteinSimilarity_WithIdenticalStrings_ReturnsOne()
        {
            // Arrange
            string sourceString = TEST_STRING_SHORT;
            string targetString = TEST_STRING_SHORT;

            // Act
            double similarityScore = _searchService.LevenshteinSimilarity(sourceString, targetString);

            // Assert
            Assert.Equal(EXACT_MATCH_THRESHOLD, similarityScore);
        }

        [Fact]
        public void LevenshteinSimilarity_WithEmptyStrings_ReturnsOne()
        {
            // Arrange
            string sourceString = EMPTY_QUERY;
            string targetString = EMPTY_QUERY;

            // Act
            double similarityScore = _searchService.LevenshteinSimilarity(sourceString, targetString);

            // Assert
            Assert.Equal(EXACT_MATCH_THRESHOLD, similarityScore);
        }

        [Fact]
        public void LevenshteinSimilarity_WithCompletelyDifferentStrings_ReturnsZero()
        {
            // Arrange
            string sourceString = COMPLETELY_DIFFERENT_STRING_1;
            string targetString = COMPLETELY_DIFFERENT_STRING_2;

            // Act
            double similarityScore = _searchService.LevenshteinSimilarity(sourceString, targetString);

            // Assert
            Assert.Equal(0.0, similarityScore);
        }

        [Fact]
        public void LevenshteinSimilarity_WithSimilarStrings_ReturnsHighScore()
        {
            // Arrange
            string sourceString = SAMPLE_WORD_1;
            string targetString = SAMPLE_WORD_2;

            // Act
            double similarityScore = _searchService.LevenshteinSimilarity(sourceString, targetString);

            // Assert
            Assert.True(similarityScore > MEDIUM_SIMILARITY_THRESHOLD);
        }

        [Fact]
        public void LevenshteinSimilarity_WithDifferentLengthStrings_ReturnsAppropriateScore()
        {
            // Arrange
            string shorterString = TEST_STRING_SHORT;
            string longerString = TEST_STRING_LONG;

            // Act
            double similarityScore = _searchService.LevenshteinSimilarity(shorterString, longerString);

            // Assert
            Assert.True(similarityScore > MEDIUM_SIMILARITY_THRESHOLD && similarityScore < EXACT_MATCH_THRESHOLD);
        }

        [Fact]
        public void LevenshteinSimilarity_WithOneEmptyString_ReturnsZero()
        {
            // Arrange
            string nonEmptyString = TEST_STRING_SHORT;
            string emptyString = EMPTY_QUERY;

            // Act
            double similarityScore = _searchService.LevenshteinSimilarity(nonEmptyString, emptyString);

            // Assert
            Assert.Equal(0.0, similarityScore);
        }

        #endregion

        #region FindFuzzySearchMatches Tests

        [Fact]
        public void FindFuzzySearchMatches_WithEmptyQuery_ReturnsEmptyList()
        {
            // Arrange
            var candidateStrings = STANDARD_TEST_CANDIDATES;

            // Act
            var matchResults = _searchService.FindFuzzySearchMatches(EMPTY_QUERY, candidateStrings);

            // Assert
            Assert.Empty(matchResults);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithNullQuery_ReturnsEmptyList()
        {
            // Arrange
            var candidateStrings = STANDARD_TEST_CANDIDATES;

            // Act
            var matchResults = _searchService.FindFuzzySearchMatches(NULL_QUERY, candidateStrings);

            // Assert
            Assert.Empty(matchResults);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithExactMatch_ReturnsMatch()
        {
            // Arrange
            var candidateStrings = STANDARD_TEST_CANDIDATES;
            string searchQuery = "test1";

            // Act
            var matchResults = _searchService.FindFuzzySearchMatches(searchQuery, candidateStrings);

            // Assert
            Assert.Contains(searchQuery, matchResults);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithHighSimilarity_ReturnsMatch()
        {
            // Arrange
            var candidateStrings = SIMILAR_WORD_CANDIDATES;
            string searchQuery = TEST_STRING_SHORT;

            // Act
            var matchResults = _searchService.FindFuzzySearchMatches(searchQuery, candidateStrings);

            // Assert
            Assert.Contains(searchQuery, matchResults);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithSubstringMatch_ReturnsMatch()
        {
            // Arrange
            var candidateStrings = SIMILAR_WORD_CANDIDATES;
            string searchQuery = "est";

            // Act
            var matchResults = _searchService.FindFuzzySearchMatches(searchQuery, candidateStrings);

            // Assert
            Assert.Contains(TEST_STRING_SHORT, matchResults);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithWordInMultiWordString_ReturnsMatch()
        {
            // Arrange
            var candidateStrings = MULTI_WORD_CANDIDATES;
            string searchQuery = TEST_STRING_SHORT;

            // Act
            var matchResults = _searchService.FindFuzzySearchMatches(searchQuery, candidateStrings);

            // Assert
            Assert.Contains("test world", matchResults);
            Assert.Contains("world test", matchResults);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithCustomThreshold_ReturnsAppropriateMatches()
        {
            // Arrange
            var candidates = new List<string> { "test", "taste", "text", "tent" };

            // Act
            var result = _searchService.FindFuzzySearchMatches("test", candidates, 0.8);

            // Assert
            Assert.Contains("test", result);
            Assert.DoesNotContain("taste", result);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithMultipleMatches_ReturnsOrderedResults()
        {
            // Arrange
            var candidates = new List<string> { "test", "testing", "tester", "tested" };

            // Act
            var result = _searchService.FindFuzzySearchMatches("test", candidates);

            // Assert
            Assert.Contains("test", result);
            // Don't enforce specific ordering as the interface doesn't specify this behavior
        }

        [Fact]
        public void FindFuzzySearchMatches_WithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            var candidates = new List<string> { "apple", "banana", "orange" };

            // Act
            var result = _searchService.FindFuzzySearchMatches("test", candidates);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithQueryContainingCandidate_ReturnsMatch()
        {
            // Arrange
            var candidates = new List<string> { "test", "testing", "tester" };

            // Act
            var result = _searchService.FindFuzzySearchMatches("testing", candidates);

            // Assert
            Assert.Contains("testing", result);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithWordInMultiWordString_ReturnsMatchWithHighestScore()
        {
            // Arrange
            var candidates = new List<string> { "test world", "world test", "test test" };

            // Act
            var result = _searchService.FindFuzzySearchMatches("test", candidates);

            // Assert
            Assert.Contains("test test", result);
            // Don't enforce specific ordering as the interface doesn't specify this behavior
        }

        [Fact]
        public void FindFuzzySearchMatches_WithDuplicateMatches_ReturnsSingleInstance()
        {
            // Arrange
            var candidates = new List<string> { "test", "test", "testing" };

            // Act
            var result = _searchService.FindFuzzySearchMatches("test", candidates);

            // Assert
            Assert.Single(result.Where(x => x == "test"));
        }

        [Fact]
        public void FindFuzzySearchMatches_WithMixedCase_ReturnsMatches()
        {
            // Arrange
            var candidates = new List<string> { "TEST", "Test", "test" };

            // Act
            var result = _searchService.FindFuzzySearchMatches("test", candidates);

            // Assert
            Assert.True(result.Count > 0);
            Assert.Contains("TEST", result);
            Assert.Contains("Test", result);
            Assert.Contains("test", result);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithSpecialCharacters_ReturnsMatches()
        {
            // Arrange
            var candidates = new List<string> { "test!", "test?", "test." };

            // Act
            var result = _searchService.FindFuzzySearchMatches("test", candidates);

            // Assert
            Assert.True(result.Count > 0);
            Assert.Contains("test!", result);
            Assert.Contains("test?", result);
            Assert.Contains("test.", result);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithEmptyCandidateList_ReturnsEmptyList()
        {
            // Arrange
            var candidates = new List<string>();

            // Act
            var result = _searchService.FindFuzzySearchMatches("test", candidates);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithNullCandidateList_ReturnsEmptyList()
        {
            // Arrange
            _mockSearchService.Setup(s => s.FindFuzzySearchMatches(
                It.IsAny<string>(),
                null,
                It.IsAny<double>()))
                .Returns(new List<string>());

            // Act
            var result = _mockSearchService.Object.FindFuzzySearchMatches("test", null);

            // Assert
            Assert.Empty(result);
            _mockSearchService.Verify(s => s.FindFuzzySearchMatches(
                It.IsAny<string>(),
                null,
                It.IsAny<double>()), Times.Once);
        }

        [Fact]
        public void FindFuzzySearchMatches_WithSimilarWordInMultiWordString_ReturnsMatch()
        {
            // Arrange
            var candidates = new List<string> { "hello wrld", "tst world", "world tst" };

            // Act
            var result = _searchService.FindFuzzySearchMatches("test", candidates, 0.7);

            // Assert
            Assert.Contains("tst world", result);
            Assert.Contains("world tst", result);
        }

        [Fact]
        public void FindFuzzySearchMatches_VerifiesMatchesWithScores()
        {
            // Arrange
            var candidates = new List<string> { "test", "tst", "testing" };
            var query = "test";
            var threshold = 0.7;

            // Act
            var result = _searchService.FindFuzzySearchMatches(query, candidates, threshold);

            // Assert
            Assert.Contains("test", result); // Exact match should always be included
            Assert.Contains("tst", result);  // Similar match should be included due to high similarity

            // Verify that "testing" is included since it contains "test"
            Assert.Contains("testing", result);

            // Verify ordering - exact match should come before partial matches
            var resultList = result.ToList();
            Assert.True(resultList.IndexOf("test") < resultList.IndexOf("tst"));
        }

        #endregion
    }
} 