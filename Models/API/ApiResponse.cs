namespace StudentManagementSystem.Models.API
{
    /// <summary>
    /// Standardized API response wrapper for consistent client handling
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response data (null if error occurred)
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Error details (null if successful)
        /// </summary>
        public ApiError? Error { get; set; }

        /// <summary>
        /// Response timestamp (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Create successful response
        /// </summary>
        public static ApiResponse<T> Ok(T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Error = null
            };
        }

        /// <summary>
        /// Create error response
        /// </summary>
        public static ApiResponse<T> Fail(ApiError error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Error = error
            };
        }
    }

    /// <summary>
    /// Standardized error details with error codes for specific handling
    /// </summary>
    public class ApiError
    {
        /// <summary>
        /// Machine-readable error code (e.g., "VALIDATION_ERROR", "RATE_LIMIT")
        /// </summary>
        public string Code { get; set; } = "";

        /// <summary>
        /// Human-readable error message
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Additional technical details (for debugging, optional)
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Field-specific validation errors (for form validation)
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        /// <summary>
        /// Retry after seconds (for rate limiting)
        /// </summary>
        public int? RetryAfter { get; set; }
    }
}
