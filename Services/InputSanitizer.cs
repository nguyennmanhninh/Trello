using System.Text.RegularExpressions;
using System.Net;

namespace StudentManagementSystem.Services
{
    /// <summary>
    /// Input sanitization service to prevent prompt injection and XSS attacks
    /// </summary>
    public static class InputSanitizer
    {
        private static readonly string[] _bannedPhrases = new[]
        {
            "ignore previous",
            "ignore all",
            "ignore instructions",
            "you are now",
            "admin mode",
            "system mode",
            "reveal password",
            "show password",
            "show database",
            "drop table",
            "delete from",
            "<script",
            "</script>",
            "javascript:",
            "onerror=",
            "onclick=",
            "onload=",
            "eval(",
            "execute(",
            "system(",
            "exec("
        };

        /// <summary>
        /// Sanitize user question to prevent injection attacks
        /// </summary>
        /// <param name="question">Raw user input</param>
        /// <returns>Sanitized safe question</returns>
        /// <exception cref="ArgumentException">Thrown if input contains harmful content</exception>
        public static string SanitizeQuestion(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException("Question cannot be empty");

            // 1. Trim whitespace
            question = question.Trim();

            // 2. Remove HTML tags (prevent XSS)
            question = Regex.Replace(question, @"<[^>]*>", "", RegexOptions.IgnoreCase);

            // 3. Remove script tags explicitly
            question = Regex.Replace(question, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", "", RegexOptions.IgnoreCase);

            // 4. Check for banned phrases (prompt injection prevention)
            var lowerQuestion = question.ToLower();
            foreach (var phrase in _bannedPhrases)
            {
                if (lowerQuestion.Contains(phrase))
                {
                    throw new ArgumentException(
                        $"Question contains potentially harmful content: '{phrase}'"
                    );
                }
            }

            // 5. Check for excessive special characters (likely injection attempt)
            var specialCharCount = question.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
            var specialCharRatio = (double)specialCharCount / question.Length;
            if (specialCharRatio > 0.3) // More than 30% special chars
            {
                throw new ArgumentException("Question contains too many special characters");
            }

            // 6. Limit length (prevent abuse)
            if (question.Length > 1000)
            {
                question = question.Substring(0, 1000);
            }

            // 7. Remove null bytes and control characters
            question = Regex.Replace(question, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");

            // 8. Encode special characters (final safety layer)
            question = WebUtility.HtmlEncode(question);

            // 9. Final check after encoding
            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException("Question is empty after sanitization");

            return question;
        }

        /// <summary>
        /// Validate if a string is safe without modification
        /// </summary>
        public static bool IsQuestionSafe(string question)
        {
            try
            {
                SanitizeQuestion(question);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
