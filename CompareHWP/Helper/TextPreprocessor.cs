using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareHWP.Helper
{
    public static class TextPreprocessor
    {
        private static readonly HashSet<string> StopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "보고서","수행","내용","결과","멘토링","지도","사업","이행",
            "전략","등록","신청","교부","지원","과제","범위",
            "국고","보조금","자기부담금","합계","소계",
            "전담","pm","업체","수행일자","수행시간"
        };

        public static string Preprocess(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = text
                .ToLowerInvariant()
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");

            var words = normalized
                .Split(' ', (char)StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !StopWords.Contains(w));

            return string.Join(" ", words);
        }
    }
}
