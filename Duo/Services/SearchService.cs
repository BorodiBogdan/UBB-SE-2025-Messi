using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Input;
using Windows.Graphics.Display;
using Duo.Services.Interfaces;

namespace Duo.Services
{
    public class SearchService : ISearchService
    {
        private const double EXACT_MATCH_FUZZY_SEARCHSCORE = 0.9;
        private const double HIGH_SIMILARITY_FUZZY_SEARCH_SCORE = 0.8;
        private const double FUZZY_SEARCH_SCORE_DEFAULT_THRESHOLD = 0.6;

        public double LevenshteinSimilarity(string source, string target)
        {
            int[,] distance = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
                distance[i, 0] = i;
            for (int j = 0; j <= target.Length; j++)
                distance[0, j] = j;

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;

                    distance[i, j] = Math.Min(
                        Math.Min(
                            distance[i - 1, j] + 1,
                            distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            int maxLength = Math.Max(source.Length, target.Length);
            return maxLength == 0 ? 1.0 : 1.0 - ((double)distance[source.Length, target.Length] / maxLength);
        }

        public List<string> FindFuzzySearchMatches(string searchQuery, IEnumerable<string> candidateStrings, double similarityThreshold = FUZZY_SEARCH_SCORE_DEFAULT_THRESHOLD)
        {
            if (string.IsNullOrEmpty(searchQuery))
                return new List<string>();

            searchQuery = searchQuery.ToLower();
            var matches = new List<(string Text, double Score)>();

            foreach (var candidate in candidateStrings)
            {
                string candidateLower = candidate.ToLower();
                double levenScore = LevenshteinSimilarity(searchQuery, candidateLower);

                if (levenScore >= HIGH_SIMILARITY_FUZZY_SEARCH_SCORE)
                {
                    matches.Add((candidate, levenScore));
                    continue;
                }

                if (candidateLower.Contains(searchQuery))
                {
                    matches.Add((candidate, EXACT_MATCH_FUZZY_SEARCHSCORE));
                    continue;
                }

                if (searchQuery.Contains(candidateLower))
                {
                    matches.Add((candidate, HIGH_SIMILARITY_FUZZY_SEARCH_SCORE));
                    continue;
                }

                if (candidate.Contains(" "))
                {
                    string[] words = candidateLower.Split(' ');
                    foreach (var word in words)
                    {
                        double wordScore = LevenshteinSimilarity(searchQuery, word);
                        if (wordScore >= similarityThreshold)
                        {
                            matches.Add((candidate, wordScore));
                            break;
                        }
                    }
                    continue;
                }

                if (levenScore >= similarityThreshold)
                {
                    matches.Add((candidate, levenScore));
                }
            }

            return matches
                .GroupBy(m => m.Text)
                .Select(g => g.OrderByDescending(m => m.Score).First())
                .OrderByDescending(m => m.Score)
                .Select(m => m.Text)
                .ToList();
        }
    }
}