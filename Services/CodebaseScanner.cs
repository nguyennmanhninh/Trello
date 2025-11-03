using System.Text;
using System.Text.RegularExpressions;

namespace StudentManagementSystem.Services
{
    /// <summary>
    /// 📂 PHASE 1: Full Codebase Scanner
    /// Scans entire project and finds relevant files for RAG context
    /// </summary>
    public class CodebaseScanner
    {
        private readonly string _projectRoot;
        private readonly List<string> _relevantExtensions = new() { ".cs", ".ts", ".html", ".css", ".json", ".sql" };
        private readonly List<string> _excludePaths = new() 
        { 
            "\\bin\\", "\\obj\\", "\\node_modules\\", "\\wwwroot\\lib\\", 
            "\\dist\\", "\\.git\\", "\\.vs\\", "\\Archive\\"
        };

        // Cache scanned files to avoid repeated disk reads
        private static Dictionary<string, CachedFile>? _fileCache;
        private static DateTime _cacheTime = DateTime.MinValue;
        private static readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public CodebaseScanner(string projectRoot)
        {
            _projectRoot = projectRoot;
        }

        /// <summary>
        /// Scan entire project and find files relevant to the question
        /// </summary>
        public List<RelevantDocument> FindRelevantFiles(string question, int topK = 5)
        {
            try
            {
                Console.WriteLine($"🔍 Scanning codebase for: {question}");

                // Extract keywords from question
                var keywords = ExtractKeywords(question);
                Console.WriteLine($"🔑 Keywords: {string.Join(", ", keywords)}");

                // Get or refresh file cache
                var files = GetCachedFiles();
                Console.WriteLine($"📁 Cached {files.Count} files");

                // Score each file based on relevance
                var scoredFiles = files.Select(f => new
                {
                    File = f,
                    Score = CalculateRelevanceScore(f, keywords)
                })
                .Where(sf => sf.Score > 0)
                .OrderByDescending(sf => sf.Score)
                .Take(topK)
                .ToList();

                Console.WriteLine($"✅ Found {scoredFiles.Count} relevant files");

                // Convert to RelevantDocument format
                var results = scoredFiles.Select(sf => new RelevantDocument
                {
                    Content = sf.File.Content,
                    Score = sf.Score,
                    Metadata = new DocumentMetadata
                    {
                        FileName = Path.GetFileName(sf.File.FilePath),
                        FilePath = GetRelativePath(sf.File.FilePath),
                        FileType = GetFileType(sf.File.FilePath)
                    }
                }).ToList();

                // Log results
                foreach (var result in results)
                {
                    Console.WriteLine($"  📄 {result.Metadata.FilePath} (score: {result.Score:F2})");
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Codebase scan error: {ex.Message}");
                return new List<RelevantDocument>();
            }
        }

        /// <summary>
        /// Extract meaningful keywords from question
        /// </summary>
        private List<string> ExtractKeywords(string question)
        {
            // Remove Vietnamese stopwords
            var stopwords = new HashSet<string> 
            { 
                "là", "của", "và", "thì", "có", "được", "trong", "cho", "các", 
                "một", "này", "đó", "như", "với", "để", "hay", "bao", "nhiêu",
                "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
                "làm", "thế", "nào", "sao", "gì", "ai", "đâu", "khi", "bị", "lỗi"
            };

            // Split and clean
            var words = Regex.Split(question.ToLower(), @"\W+")
                .Where(w => w.Length >= 3 && !stopwords.Contains(w))
                .Distinct()
                .ToList();

            // Detect programming concepts
            var keywords = new List<string>();
            
            // Technical terms mapping (Vietnamese → English + variants)
            var termMappings = new Dictionary<string, List<string>>
            {
                { "sinh viên", new() { "student", "students", "sinh_vien", "sinhvien" } },
                { "sinhvien", new() { "student", "students", "sinh_vien" } },
                { "giáo viên", new() { "teacher", "teachers", "giao_vien", "giaovien" } },
                { "giaovien", new() { "teacher", "teachers", "giao_vien" } },
                { "lớp", new() { "class", "classes", "lop", "classroom" } },
                { "khóa học", new() { "course", "courses", "khoa_hoc" } },
                { "điểm", new() { "grade", "grades", "score", "diem" } },
                { "đăng nhập", new() { "login", "auth", "authenticate", "account" } },
                { "xuất", new() { "export", "excel", "pdf" } },
                { "thống kê", new() { "statistics", "dashboard", "report", "thong_ke" } },
                { "phân quyền", new() { "authorization", "role", "permission" } },
                { "api", new() { "api", "controller", "endpoint", "webapi" } },
                { "database", new() { "database", "sql", "dbcontext", "entity" } },
                { "frontend", new() { "angular", "component", "typescript", "html" } },
                { "validation", new() { "validate", "validation", "validator", "error" } }
            };

            // Add original words
            keywords.AddRange(words);

            // Expand with mapped terms
            foreach (var word in words)
            {
                foreach (var mapping in termMappings)
                {
                    if (mapping.Key.Contains(word) || word.Contains(mapping.Key))
                    {
                        keywords.AddRange(mapping.Value);
                    }
                }
            }

            return keywords.Distinct().ToList();
        }

        /// <summary>
        /// Calculate relevance score for a file based on keywords
        /// </summary>
        private float CalculateRelevanceScore(CachedFile file, List<string> keywords)
        {
            float score = 0f;
            var content = file.Content.ToLower();
            var fileName = Path.GetFileNameWithoutExtension(file.FilePath).ToLower();
            var filePath = file.FilePath.ToLower();

            foreach (var keyword in keywords)
            {
                // Exact match in file name = high score
                if (fileName.Contains(keyword))
                {
                    score += 10f;
                }

                // Match in file path = medium score
                if (filePath.Contains(keyword))
                {
                    score += 5f;
                }

                // Match in content = base score (count occurrences)
                var occurrences = Regex.Matches(content, Regex.Escape(keyword), RegexOptions.IgnoreCase).Count;
                score += occurrences * 0.5f;
            }

            // Boost score for important file types
            if (file.FilePath.EndsWith("Controller.cs"))
            {
                score *= 1.5f; // Controllers are central to application logic
            }
            else if (file.FilePath.EndsWith("Service.cs"))
            {
                score *= 1.3f; // Services contain business logic
            }
            else if (file.FilePath.Contains("\\Models\\"))
            {
                score *= 1.2f; // Models define data structure
            }

            return score;
        }

        /// <summary>
        /// Get cached files or scan project if cache expired
        /// </summary>
        private List<CachedFile> GetCachedFiles()
        {
            if (_fileCache != null && DateTime.UtcNow - _cacheTime < _cacheExpiration)
            {
                return _fileCache.Values.ToList();
            }

            Console.WriteLine("🔄 Cache expired, rescanning project...");
            _fileCache = new Dictionary<string, CachedFile>();
            _cacheTime = DateTime.UtcNow;

            ScanDirectory(_projectRoot);

            Console.WriteLine($"✅ Scanned {_fileCache.Count} files");
            return _fileCache.Values.ToList();
        }

        /// <summary>
        /// Recursively scan directory for relevant files
        /// </summary>
        private void ScanDirectory(string path)
        {
            try
            {
                // Skip excluded paths
                if (_excludePaths.Any(ex => path.Contains(ex, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                // Process files in current directory
                foreach (var file in Directory.GetFiles(path))
                {
                    try
                    {
                        var ext = Path.GetExtension(file);
                        if (!_relevantExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                            continue;

                        // Skip large files (> 500KB)
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.Length > 500 * 1024)
                            continue;

                        // Read file content
                        var content = File.ReadAllText(file);

                        // Store in cache
                        _fileCache![file] = new CachedFile
                        {
                            FilePath = file,
                            Content = content,
                            LastModified = fileInfo.LastWriteTimeUtc
                        };
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Skip file {file}: {ex.Message}");
                    }
                }

                // Recursively scan subdirectories
                foreach (var subdir in Directory.GetDirectories(path))
                {
                    ScanDirectory(subdir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Skip directory {path}: {ex.Message}");
            }
        }

        /// <summary>
        /// Get relative path from project root
        /// </summary>
        private string GetRelativePath(string fullPath)
        {
            if (fullPath.StartsWith(_projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(_projectRoot.Length).TrimStart('\\', '/');
            }
            return fullPath;
        }

        /// <summary>
        /// Get file type for metadata
        /// </summary>
        private string GetFileType(string filePath)
        {
            return Path.GetExtension(filePath).TrimStart('.').ToLower();
        }

        /// <summary>
        /// Clear cache (useful for testing)
        /// </summary>
        public static void ClearCache()
        {
            _fileCache = null;
            _cacheTime = DateTime.MinValue;
            Console.WriteLine("🗑️ Cache cleared");
        }
    }

    /// <summary>
    /// Cached file information
    /// </summary>
    public class CachedFile
    {
        public string FilePath { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime LastModified { get; set; }
    }
}
