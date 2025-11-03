namespace StudentManagementSystem.Models.API
{
    /// <summary>
    /// Chat response data structure
    /// </summary>
    public class ChatResponse
    {
        /// <summary>
        /// AI-generated answer
        /// </summary>
        public string Answer { get; set; } = "";

        /// <summary>
        /// Source code snippets used for context
        /// </summary>
        public List<ChatSource> Sources { get; set; } = new();

        /// <summary>
        /// Suggested follow-up questions
        /// </summary>
        public List<string> FollowUpQuestions { get; set; } = new();

        /// <summary>
        /// Unique request identifier for tracking
        /// </summary>
        public string RequestId { get; set; } = "";

        /// <summary>
        /// Response time in milliseconds
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Indicates if response came from cache
        /// </summary>
        public bool FromCache { get; set; }
    }

    /// <summary>
    /// Source code reference in chat response
    /// </summary>
    public class ChatSource
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string CodeSnippet { get; set; } = "";
        public float Score { get; set; }
    }
}
