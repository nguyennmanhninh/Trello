using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudentManagementSystem.Services;
using StudentManagementSystem.Models.API;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace StudentManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly RagService _ragService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(RagService ragService, ILogger<ChatController> logger)
        {
            _ragService = ragService;
            _logger = logger;
        }

        /// <summary>
        /// RAG-powered chat endpoint with validation, rate limiting, and comprehensive error handling
        /// </summary>
        /// <param name="request">Chat request with question</param>
        /// <param name="cancellationToken">Cancellation token to respect client disconnection</param>
        /// <returns>Standardized API response with answer, sources, and follow-up questions</returns>
        [HttpPost("ask")]
        [EnableRateLimiting("ChatApi")]
        public async Task<IActionResult> Ask(
            [FromBody] ChatRequest request,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var userId = HttpContext.Session.GetString("UserId") ?? "anonymous";
            var requestId = Guid.NewGuid().ToString("N")[..8];

            _logger.LogInformation(
                "[{RequestId}] Chat request from user {UserId}: {Question}",
                requestId, userId, request.Question?.Substring(0, Math.Min(50, request.Question?.Length ?? 0))
            );

            // Validate ModelState (auto from [Required] attributes)
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("[{RequestId}] Invalid request: ModelState validation failed", requestId);
                
                return BadRequest(ApiResponse<object>.Fail(new ApiError
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Invalid request data",
                    ValidationErrors = ModelState
                        .Where(x => x.Value?.Errors.Any() ?? false)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                }));
            }

            try
            {
                // Sanitize input to prevent prompt injection and XSS
                var sanitizedQuestion = InputSanitizer.SanitizeQuestion(request.Question ?? "");

                _logger.LogDebug("[{RequestId}] Question sanitized successfully", requestId);

                // Get user role from session
                var userRole = HttpContext.Session.GetString("UserRole");

                // Call RAG service with cancellation support
                var response = await _ragService.AskQuestion(
                    sanitizedQuestion,
                    userRole,
                    cancellationToken
                );

                stopwatch.Stop();

                if (!response.Success)
                {
                    _logger.LogWarning(
                        "[{RequestId}] RAG service failed in {Duration}ms: {Error}",
                        requestId, stopwatch.ElapsedMilliseconds, response.Error
                    );

                    return StatusCode(500, ApiResponse<object>.Fail(new ApiError
                    {
                        Code = "AI_SERVICE_ERROR",
                        Message = "Failed to generate answer. Please try again.",
                        Details = response.Error
                    }));
                }

                var fromCache = response.Answer?.Contains("cache") ?? false;

                _logger.LogInformation(
                    "[{RequestId}] Chat request successful in {Duration}ms (Cache: {FromCache})",
                    requestId, stopwatch.ElapsedMilliseconds, fromCache
                );

                return Ok(ApiResponse<ChatResponse>.Ok(new ChatResponse
                {
                    Answer = response.Answer ?? "",
                    Sources = response.Sources?.Select(s => new ChatSource
                    {
                        FileName = s.FileName,
                        FilePath = s.FilePath,
                        CodeSnippet = s.CodeSnippet,
                        Score = s.Score
                    }).ToList() ?? new List<ChatSource>(),
                    FollowUpQuestions = response.FollowUpQuestions ?? new List<string>(),
                    RequestId = requestId,
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    FromCache = fromCache
                }));
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "[{RequestId}] Request was cancelled by client after {Duration}ms",
                    requestId, stopwatch.ElapsedMilliseconds
                );

                return StatusCode(499, ApiResponse<object>.Fail(new ApiError
                {
                    Code = "REQUEST_CANCELLED",
                    Message = "Request was cancelled"
                }));
            }
            catch (ArgumentException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "[{RequestId}] Invalid input detected in {Duration}ms: {Error}",
                    requestId, stopwatch.ElapsedMilliseconds, ex.Message
                );

                return BadRequest(ApiResponse<object>.Fail(new ApiError
                {
                    Code = "INVALID_INPUT",
                    Message = ex.Message
                }));
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "[{RequestId}] External API error in {Duration}ms",
                    requestId, stopwatch.ElapsedMilliseconds
                );

                return StatusCode(503, ApiResponse<object>.Fail(new ApiError
                {
                    Code = "EXTERNAL_API_ERROR",
                    Message = "AI service is temporarily unavailable. Please try again later.",
                    Details = ex.Message
                }));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "[{RequestId}] Unexpected error in {Duration}ms",
                    requestId, stopwatch.ElapsedMilliseconds
                );

                return StatusCode(500, ApiResponse<object>.Fail(new ApiError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred. Please try again.",
                    Details = ex.Message
                }));
            }
        }

        /// <summary>
        /// Comprehensive health check endpoint with real service validation
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            try
            {
                var checks = new Dictionary<string, object>
                {
                    ["aiService"] = await CheckAiServiceHealth(),
                    ["cache"] = CheckCacheHealth(),
                    ["configuration"] = CheckConfiguration()
                };

                var allHealthy = checks.Values.All(c =>
                {
                    var dict = c as Dictionary<string, object>;
                    return dict != null && dict.ContainsKey("healthy") && (bool)dict["healthy"];
                });

                var response = new
                {
                    status = allHealthy ? "healthy" : "degraded",
                    service = "RAG Chat API",
                    timestamp = DateTime.UtcNow,
                    checks
                };

                return allHealthy ? Ok(response) : StatusCode(503, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    service = "RAG Chat API",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }

        private async Task<object> CheckAiServiceHealth()
        {
            try
            {
                // Test with simple question and 5 second timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var testResponse = await _ragService.AskQuestion("test", null, cts.Token);

                return new Dictionary<string, object>
                {
                    ["healthy"] = testResponse.Success,
                    ["provider"] = _ragService.GetProviderName(),
                    ["configured"] = _ragService.IsConfigured(),
                    ["responseTime"] = "< 5s"
                };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["healthy"] = false,
                    ["provider"] = _ragService.GetProviderName(),
                    ["configured"] = _ragService.IsConfigured(),
                    ["error"] = ex.Message
                };
            }
        }

        private object CheckCacheHealth()
        {
            var cacheSize = RagService.GetCacheSize();
            var healthy = cacheSize < 10000; // Warn if cache > 10k items

            var result = new Dictionary<string, object>
            {
                ["healthy"] = healthy,
                ["size"] = cacheSize,
                ["maxAge"] = "1 hour"
            };

            if (!healthy)
            {
                result["warning"] = "Cache size is large, consider clearing old entries";
            }

            return result;
        }

        private object CheckConfiguration()
        {
            var configured = _ragService.IsConfigured();

            return new Dictionary<string, object>
            {
                ["healthy"] = configured,
                ["provider"] = _ragService.GetProviderName(),
                ["message"] = configured ? "AI service is properly configured" : "AI service is not configured. Please check appsettings.json"
            };
        }
    }

    #region Request/Response Models

    /// <summary>
    /// Chat request model with validation
    /// </summary>
    public class ChatRequest
    {
        [Required(ErrorMessage = "Question is required")]
        [StringLength(1000, MinimumLength = 3,
            ErrorMessage = "Question must be between 3 and 1000 characters")]
        public string Question { get; set; } = "";
    }

    #endregion
}
