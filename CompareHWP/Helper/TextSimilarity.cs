using System;
using System.Collections.Generic;
using System.Linq;

namespace CompareHWP.Helper
{
    public static class TextSimilarity
    {
        /// <summary>
        /// Cosine Similarity 계산 (0~100)
        /// </summary>
        public static double CosineSimilarityPercent(string text1, string text2)
        {
            if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
                return 0;

            var v1 = ToWordVector(text1);
            var v2 = ToWordVector(text2);

            if (v1.Count == 0 || v2.Count == 0)
                return 0;

            var commonWords = v1.Keys.Intersect(v2.Keys);

            double dot = commonWords.Sum(w => v1[w] * v2[w]);
            double mag1 = Math.Sqrt(v1.Values.Sum(v => v * v));
            double mag2 = Math.Sqrt(v2.Values.Sum(v => v * v));

            if (mag1 == 0 || mag2 == 0)
                return 0;

            return (dot / (mag1 * mag2)) * 100.0;
        }

        private static Dictionary<string, int> ToWordVector(string text)
        {
            return text
                .ToLowerInvariant()
                .Split(new[] { ' ', '\r', '\n', '\t', '.', ',', ';', ':', '(', ')', '[', ']' },
                       StringSplitOptions.RemoveEmptyEntries)
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
