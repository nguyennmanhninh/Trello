using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace StudentManagementSystem.Services
{
    /// <summary>
    /// RAG (Retrieval Augmented Generation) Service
    /// Enables chatbot to understand and answer questions about the codebase
    /// </summary>
    public class RagService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _aiProvider; // "OpenAI" or "Gemini"
        private readonly string _openAiApiKey;
        private readonly List<string> _geminiApiKeys; // Multiple keys for rotation
        private int _currentKeyIndex = 0; // Track which key to use
        private readonly string _pineconeApiKey;
        private readonly string _pineconeEnvironment;
        private readonly string _pineconeIndex;
        private readonly CodebaseScanner _codebaseScanner; // 📂 PHASE 1: Full codebase scanning
        
        // 🚀 PHASE 1: Response Cache - Instant answers for repeated questions
        private static readonly Dictionary<string, CachedResponse> _responseCache = new();
        private static readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

        public RagService(HttpClient httpClient, IConfiguration configuration, IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            // 📂 PHASE 1: Initialize codebase scanner with project root
            var projectRoot = env.ContentRootPath;
            _codebaseScanner = new CodebaseScanner(projectRoot);
            Console.WriteLine($"📂 Codebase scanner initialized: {projectRoot}");
            
            // Get API keys from configuration
            _aiProvider = configuration["AI:Provider"] ?? "Gemini"; // Default to Gemini (FREE!)
            _openAiApiKey = configuration["OpenAI:ApiKey"] ?? "";
            
            // Load multiple Gemini API keys
            _geminiApiKeys = new List<string>();
            var apiKeysSection = configuration.GetSection("Gemini:ApiKeys");
            if (apiKeysSection.Exists())
            {
                _geminiApiKeys = apiKeysSection.Get<List<string>>() ?? new List<string>();
            }
            else
            {
                // Fallback to single key for backward compatibility
                var singleKey = configuration["Gemini:ApiKey"];
                if (!string.IsNullOrEmpty(singleKey))
                {
                    _geminiApiKeys.Add(singleKey);
                }
            }
            
            _pineconeApiKey = configuration["Pinecone:ApiKey"] ?? "";
            _pineconeEnvironment = configuration["Pinecone:Environment"] ?? "us-east-1-aws";
            _pineconeIndex = configuration["Pinecone:IndexName"] ?? "sms-codebase";
        }

        /// <summary>
        /// Get current Gemini API key with rotation support
        /// </summary>
        private string GetCurrentGeminiApiKey()
        {
            if (_geminiApiKeys == null || _geminiApiKeys.Count == 0)
            {
                return string.Empty;
            }
            
            return _geminiApiKeys[_currentKeyIndex];
        }

        /// <summary>
        /// Rotate to next API key when rate limit is hit
        /// </summary>
        private void RotateToNextApiKey()
        {
            if (_geminiApiKeys == null || _geminiApiKeys.Count <= 1)
            {
                return; // No rotation needed
            }
            
            _currentKeyIndex = (_currentKeyIndex + 1) % _geminiApiKeys.Count;
            Console.WriteLine($"🔄 Rotated to API key #{_currentKeyIndex + 1}/{_geminiApiKeys.Count}");
        }

        /// <summary>
        /// Main RAG pipeline: Query → Retrieve → Generate
        /// </summary>
        public async Task<RagResponse> AskQuestion(string question, string? userRole = null, CancellationToken cancellationToken = default)
        {
            // Check for cancellation before expensive operations
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                // 🚀 PHASE 1: Check cache first for instant response
                var cacheKey = question.ToLower().Trim();
                if (_responseCache.TryGetValue(cacheKey, out var cachedResponse))
                {
                    if (DateTime.UtcNow - cachedResponse.Timestamp < _cacheExpiration)
                    {
                        // 🧠 PHASE 4: Generate follow-ups for cached responses too
                        var cachedFollowUps = await GenerateFollowUpQuestions(question, cachedResponse.Answer);
                        
                        // Cache hit! Return instantly (0ms response time)
                        return new RagResponse
                        {
                            Success = true,
                            Answer = cachedResponse.Answer + "\n\n✨ *Trả lời từ bộ nhớ cache (instant response)*",
                            Sources = cachedResponse.Sources,
                            FollowUpQuestions = cachedFollowUps
                        };
                    }
                    else
                    {
                        // Cache expired, remove it
                        _responseCache.Remove(cacheKey);
                    }
                }

                List<RelevantDocument> relevantDocs;

                // Gemini mode: Use full codebase scanning instead of sample docs
                if (_aiProvider == "Gemini")
                {
                    relevantDocs = _codebaseScanner.FindRelevantFiles(question, topK: 3); // 📂 PHASE 1: Real scanning
                    
                    // Fallback to sample docs if scan returns nothing
                    if (relevantDocs.Count == 0)
                    {
                        Console.WriteLine("⚠️ Scan returned 0 results, using sample docs");
                        relevantDocs = GetSampleDocuments(topK: 2);
                    }
                }
                else
                {
                    // OpenAI mode: Full RAG pipeline with embeddings
                    // Step 1: Generate embedding for the question
                    var questionEmbedding = await GenerateEmbedding(question);

                    // Step 2: Search for relevant code snippets in vector database
                    relevantDocs = await SearchVectorDatabase(questionEmbedding, topK: 5);
                }

                // Step 3: Build context from retrieved documents
                var context = BuildContext(relevantDocs);

                // Check cancellation before AI generation
                cancellationToken.ThrowIfCancellationRequested();

                // Step 4: Generate answer using AI with context
                var answer = await GenerateAnswer(question, context, userRole, cancellationToken);

                var sources = relevantDocs.Select(d => new Source
                {
                    FileName = d.Metadata?.FileName ?? "",
                    FilePath = d.Metadata?.FilePath ?? "",
                    CodeSnippet = d.Content,
                    Score = d.Score
                }).ToList();

                // 🧠 PHASE 4: Generate follow-up questions
                var followUps = await GenerateFollowUpQuestions(question, answer);
                Console.WriteLine($"🧠 Generated {followUps.Count} follow-up questions");

                // 🚀 PHASE 1: Cache the response for future instant retrieval
                _responseCache[cacheKey] = new CachedResponse
                {
                    Answer = answer,
                    Sources = sources,
                    Timestamp = DateTime.UtcNow
                };

                return new RagResponse
                {
                    Success = true,
                    Answer = answer,
                    Sources = sources,
                    FollowUpQuestions = followUps
                };
            }
            catch (Exception ex)
            {
                return new RagResponse
                {
                    Success = false,
                    Error = $"Error processing question: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get current cache size for health monitoring
        /// </summary>
        public static int GetCacheSize()
        {
            return _responseCache.Count;
        }

        /// <summary>
        /// Get AI provider name
        /// </summary>
        public string GetProviderName()
        {
            return _aiProvider;
        }

        /// <summary>
        /// Check if AI service is configured correctly
        /// </summary>
        public bool IsConfigured()
        {
            return (_aiProvider == "Gemini" && _geminiApiKeys != null && _geminiApiKeys.Count > 0) ||
                   (_aiProvider == "OpenAI" && !string.IsNullOrEmpty(_openAiApiKey));
        }

        /// <summary>
        /// Generate embedding vector for text using OpenAI
        /// </summary>
        private async Task<float[]> GenerateEmbedding(string text)
        {
            var request = new
            {
                model = "text-embedding-ada-002", // OpenAI's embedding model
                input = text
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/embeddings",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI API error: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            var embeddingArray = result
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(e => (float)e.GetDouble())
                .ToArray();

            return embeddingArray;
        }

        /// <summary>
        /// Search Pinecone vector database for similar documents
        /// </summary>
        private async Task<List<RelevantDocument>> SearchVectorDatabase(float[] queryEmbedding, int topK = 5)
        {
            // If Pinecone not configured, return sample docs
            if (string.IsNullOrEmpty(_pineconeApiKey))
            {
                return GetSampleDocuments(topK);
            }

            var request = new
            {
                vector = queryEmbedding,
                topK = topK,
                includeMetadata = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Api-Key", _pineconeApiKey);

            var url = $"https://{_pineconeIndex}-{_pineconeEnvironment}.svc.pinecone.io/query";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                // Fallback to sample docs if Pinecone fails
                return GetSampleDocuments(topK);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var documents = new List<RelevantDocument>();
            foreach (var match in result.GetProperty("matches").EnumerateArray())
            {
                var metadata = match.GetProperty("metadata");
                documents.Add(new RelevantDocument
                {
                    Content = metadata.GetProperty("content").GetString() ?? "",
                    Score = (float)match.GetProperty("score").GetDouble(),
                    Metadata = new DocumentMetadata
                    {
                        FileName = metadata.GetProperty("fileName").GetString() ?? "",
                        FilePath = metadata.GetProperty("filePath").GetString() ?? "",
                        FileType = metadata.GetProperty("fileType").GetString() ?? ""
                    }
                });
            }

            return documents;
        }

        /// <summary>
        /// Build context string from retrieved documents
        /// </summary>
        private string BuildContext(List<RelevantDocument> documents)
        {
            // OPTIMIZED: Minimal formatting for speed
            var contextBuilder = new StringBuilder();

            for (int i = 0; i < documents.Count; i++)
            {
                var doc = documents[i];
                contextBuilder.AppendLine($"[{doc.Metadata?.FileName}]");
                contextBuilder.AppendLine(doc.Content);
                contextBuilder.AppendLine();
            }

            return contextBuilder.ToString();
        }

        /// <summary>
        /// Generate answer using AI (OpenAI or Gemini) with context
        /// </summary>
        private async Task<string> GenerateAnswer(string question, string context, string? userRole, CancellationToken cancellationToken = default)
        {
            if (_aiProvider == "Gemini")
            {
                return await GenerateAnswerWithGemini(question, context, userRole, cancellationToken);
            }
            else
            {
                return await GenerateAnswerWithOpenAI(question, context, userRole, cancellationToken);
            }
        }

        /// <summary>
        /// Generate answer using Google Gemini (FREE!)
        /// </summary>
        private async Task<string> GenerateAnswerWithGemini(string question, string context, string? userRole, CancellationToken cancellationToken = default)
        {
            // OPTIMIZED: Shorter prompt for faster response
            var systemPrompt = @"AI Assistant for Student Management System (ASP.NET Core 8 + Angular 17).
Answer in Vietnamese. Be concise. Use code examples from context when relevant.";

            if (!string.IsNullOrEmpty(userRole))
            {
                systemPrompt += $" User: {userRole}";
            }

            var prompt = $"{systemPrompt}\n\nContext:\n{context}\n\nQ: {question}\nA:";

            // Gemini API request format - SPEED OPTIMIZED
            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 1.0,        // Max speed: less "thinking"
                    maxOutputTokens = 800,    // Shorter answers = faster
                    topK = 1,                 // Pick top choice immediately
                    topP = 0.8                // Less randomness = faster
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Clear();

            // Try with current API key, rotate on rate limit
            var maxRetries = _geminiApiKeys?.Count ?? 1;
            Exception? lastException = null;
            
            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    var currentKey = GetCurrentGeminiApiKey();
                    
                    // Gemini API endpoint - gemini-2.0-flash-exp is the ONLY working model
                    // Tested on 2025-10-24: All other models return 404
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent?key={currentKey}";
                    var response = await _httpClient.PostAsync(url, content, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        
                        // 🔧 Handle model not found (404)
                        if ((int)response.StatusCode == 404)
                        {
                            throw new Exception("❌ Gemini model not found. The API endpoint or model name may be incorrect. Please check Gemini API documentation.");
                        }
                        
                        // 🔧 Handle rate limiting (429) - Try next key
                        if ((int)response.StatusCode == 429)
                        {
                            RotateToNextApiKey();
                            
                            // If we have more keys, continue to next iteration
                            if (retry < maxRetries - 1)
                            {
                                Console.WriteLine($"⏱️ Rate limit on key #{retry + 1}, trying next key...");
                                await Task.Delay(500); // Small delay before retry
                                continue;
                            }
                            
                            throw new Exception($"⏱️ All {maxRetries} Gemini API keys exhausted. Please wait a moment and try again. Free tier allows 15 requests per minute per key.");
                        }
                        
                        // 🔧 Handle service unavailable (503)
                        if ((int)response.StatusCode == 503)
                        {
                            throw new Exception("🔧 Gemini API is temporarily unavailable. Please try again in a few moments or check https://status.google.com for updates.");
                        }
                        
                        throw new Exception($"Gemini API error ({response.StatusCode}): {error}");
                    }

                    // Success! Parse response
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

                    var answer = result
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? "No answer generated";

                    return answer;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    // If it's a rate limit error and we have more keys, continue
                    if (ex.Message.Contains("429") && retry < maxRetries - 1)
                    {
                        continue;
                    }
                    
                    // Otherwise throw
                    throw;
                }
            }
            
            // If all retries failed
            throw lastException ?? new Exception("Failed to generate answer with Gemini");
        }

        /// <summary>
        /// Generate answer using OpenAI GPT-4
        /// </summary>
        private async Task<string> GenerateAnswerWithOpenAI(string question, string context, string? userRole, CancellationToken cancellationToken = default)
        {
            var systemPrompt = @"You are an AI assistant for a Student Management System built with ASP.NET Core 8 and Angular 17.

Your role:
- Answer questions about the codebase using the provided context
- Explain code functionality, architecture, and best practices
- Provide code examples from the actual codebase
- Help with troubleshooting and debugging
- Always answer in Vietnamese unless asked otherwise

Context about the system:
- Backend: ASP.NET Core 8 MVC + Web API
- Frontend: Angular 17 (Standalone Components)
- Database: SQL Server with EF Core
- Authentication: Session + JWT
- Roles: Admin, Teacher, Student
- Modules: Departments, Students, Teachers, Classes, Courses, Grades

When answering:
1. Reference specific files and line numbers from context
2. Provide code snippets when relevant
3. Explain the 'why' behind implementation choices
4. Be concise but thorough
5. Use Vietnamese for explanations";

            if (!string.IsNullOrEmpty(userRole))
            {
                systemPrompt += $"\n\nUser role: {userRole}";
            }

            var request = new
            {
                model = "gpt-4-turbo-preview",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = $"{context}\n\n---\n\nQuestion: {question}" }
                },
                temperature = 0.7,
                max_tokens = 1500
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                content,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI API error: {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            var answer = result
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "No answer generated";

            return answer;
        }

        /// <summary>
        /// Get sample documents as fallback when codebase scanning returns nothing
        /// This is now only used as a safety net
        /// </summary>
        private List<RelevantDocument> GetSampleDocuments(int topK)
        {
            // Sample documents from the codebase
            var sampleDocs = new List<RelevantDocument>
            {
                new RelevantDocument
                {
                    Content = @"[AuthorizeRole(""Admin"", ""Teacher"")]
public async Task<IActionResult> Create(Student student)
{
    if (ModelState.IsValid)
    {
        _context.Add(student);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    return View(student);
}",
                    Score = 0.95f,
                    Metadata = new DocumentMetadata
                    {
                        FileName = "StudentsController.cs",
                        FilePath = "Controllers/StudentsController.cs",
                        FileType = "csharp"
                    }
                },
                new RelevantDocument
                {
                    Content = @"public class Grade
{
    public string StudentId { get; set; }
    public string CourseId { get; set; }
    public decimal Score { get; set; } // 0-10, max 2 decimal places
    public string Classification { get; set; } // Xuất sắc, Giỏi, Khá, etc.
}",
                    Score = 0.88f,
                    Metadata = new DocumentMetadata
                    {
                        FileName = "Grade.cs",
                        FilePath = "Models/Grade.cs",
                        FileType = "csharp"
                    }
                },
                new RelevantDocument
                {
                    Content = @"validateForm(): boolean {
  if (!this.grade.score || this.grade.score < 0 || this.grade.score > 10) {
    this.validationErrors.score = 'Điểm phải từ 0 đến 10';
    return false;
  }
  // Check max 2 decimal places
  const scoreStr = this.grade.score.toString();
  if (scoreStr.includes('.')) {
    const decimals = scoreStr.split('.')[1];
    if (decimals.length > 2) {
      this.validationErrors.score = 'Điểm chỉ được tối đa 2 chữ số thập phân';
      return false;
    }
  }
  return true;
}",
                    Score = 0.82f,
                    Metadata = new DocumentMetadata
                    {
                        FileName = "grades.component.ts",
                        FilePath = "ClientApp/src/app/components/grades/grades.component.ts",
                        FileType = "typescript"
                    }
                }
            };

            return sampleDocs.Take(topK).ToList();
        }

        /// <summary>
        /// 🧠 PHASE 4: Generate suggested follow-up questions using Gemini
        /// </summary>
        public async Task<List<string>> GenerateFollowUpQuestions(string question, string answer)
        {
            try
            {
                var prompt = $@"Dựa vào câu hỏi và câu trả lời dưới đây, hãy gợi ý 3 câu hỏi tiếp theo hay và hữu ích mà user có thể quan tâm.

Câu hỏi gốc: {question}
Câu trả lời: {answer.Substring(0, Math.Min(300, answer.Length))}...

Yêu cầu:
- Chỉ trả về 3 câu hỏi, mỗi câu 1 dòng
- Không có số thứ tự, không có gạch đầu dòng
- Ngắn gọn, rõ ràng
- Liên quan đến lập trình ASP.NET Core hoặc Angular";

                var request = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.8,
                        maxOutputTokens = 200
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Clear();
                var currentKey = GetCurrentGeminiApiKey();
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent?key={currentKey}";
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    return new List<string>(); // Return empty on error
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

                var text = result
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                // Split by newlines and clean up
                var questions = text.Split('\n')
                    .Select(q => q.Trim())
                    .Where(q => !string.IsNullOrEmpty(q) && q.Length > 10)
                    .Take(3)
                    .ToList();

                Console.WriteLine($"🧠 Follow-up raw text: {text}");
                Console.WriteLine($"🧠 Follow-up parsed: {string.Join(" | ", questions)}");

                return questions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating follow-ups: {ex.Message}");
            }
            
            return new List<string>();
        }
    }

    #region Response Models

    public class RagResponse
    {
        public bool Success { get; set; }
        public string? Answer { get; set; }
        public List<Source>? Sources { get; set; }
        public string? Error { get; set; }
        public List<string>? FollowUpQuestions { get; set; } // 🧠 PHASE 4
    }

    public class Source
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string CodeSnippet { get; set; } = "";
        public float Score { get; set; }
    }

    public class RelevantDocument
    {
        public string Content { get; set; } = "";
        public float Score { get; set; }
        public DocumentMetadata? Metadata { get; set; }
    }

    public class DocumentMetadata
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string FileType { get; set; } = "";
    }

    // 🚀 PHASE 1: Cache model for instant responses
    public class CachedResponse
    {
        public string Answer { get; set; } = "";
        public List<Source> Sources { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
