# 🖥️ Phase 1 Console Output Examples

## What You'll See When Running

### Application Startup

```
📂 Codebase scanner initialized: C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5298
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

## First Request (Initial Scan)

**User asks:** "Làm sao để thêm sinh viên?"

### Console Output:
```
🔍 Scanning codebase for: Làm sao để thêm sinh viên?
🔑 Keywords: thêm, sinh, viên, student, students, sinh_vien, sinhvien, create, add

🔄 Cache expired, rescanning project...
  ⚠️ Skip directory C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem\bin: Excluded
  ⚠️ Skip directory C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem\obj: Excluded
  ⚠️ Skip directory C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem\node_modules: Excluded
✅ Scanned 347 files

📁 Cached 347 files
✅ Found 3 relevant files
  📄 Controllers/StudentsController.cs (score: 42.50)
  📄 Models/Student.cs (score: 38.00)
  📄 ClientApp/src/app/components/students/students.component.ts (score: 35.00)

🤖 Generating answer with Gemini...
✅ Response generated (1,234ms)

🧠 Generating follow-up questions...
🧠 Follow-up raw text: Validation sinh viên như thế nào?
Xóa sinh viên có điểm được không?
Export danh sách sinh viên ra file gì?
🧠 Follow-up parsed: Validation sinh viên như thế nào? | Xóa sinh viên có điểm được không? | Export danh sách sinh viên ra file gì?
🧠 Generated 3 follow-up questions

✅ Response cached for future instant retrieval
```

**Response Time:** ~3-4 seconds (initial scan + Gemini API)

---

## Second Request (Cache Hit - Same Question)

**User asks:** "Làm sao để thêm sinh viên?" (same question)

### Console Output:
```
✨ Cache hit! Returning instant response (0ms)
```

**Response Time:** ~0ms (instant from cache)

---

## Third Request (Cache Hit - Different Question)

**User asks:** "Sinh viên có những trường nào?"

### Console Output:
```
🔍 Scanning codebase for: Sinh viên có những trường nào?
🔑 Keywords: sinh, viên, trường, student, field, property, attribute

📁 Cached 347 files
✅ Found 3 relevant files
  📄 Models/Student.cs (score: 45.00)
  📄 Controllers/StudentsController.cs (score: 32.00)
  📄 ClientApp/src/app/components/students/students.component.ts (score: 28.00)

🤖 Generating answer with Gemini...
✅ Response generated (987ms)

🧠 Generating follow-up questions...
🧠 Follow-up raw text: StudentId có format gì?
DateOfBirth validate như thế nào?
ClassId có nullable không?
🧠 Follow-up parsed: StudentId có format gì? | DateOfBirth validate như thế nào? | ClassId có nullable không?
🧠 Generated 3 follow-up questions

✅ Response cached for future instant retrieval
```

**Response Time:** ~1,000ms (cache hit for files, new Gemini API call)

---

## Fourth Request (Export Question)

**User asks:** "Làm sao để export danh sách sinh viên?"

### Console Output:
```
🔍 Scanning codebase for: Làm sao để export danh sách sinh viên?
🔑 Keywords: export, xuất, sinh, viên, student, danh, sách, list, excel, pdf

📁 Cached 347 files
✅ Found 3 relevant files
  📄 Services/ExportService.cs (score: 52.00)
  📄 Controllers/StudentsController.cs (score: 38.50)
  📄 Models/Student.cs (score: 25.00)

🤖 Generating answer with Gemini...
✅ Response generated (1,145ms)

🧠 Generating follow-up questions...
🧠 Follow-up raw text: Export có thể chọn columns không?
Có thể export ra PDF không?
File Excel có format gì?
🧠 Follow-up parsed: Export có thể chọn columns không? | Có thể export ra PDF không? | File Excel có format gì?
🧠 Generated 3 follow-up questions

✅ Response cached for future instant retrieval
```

**Response Time:** ~1,200ms (cache hit for files, new Gemini API call)

---

## Fifth Request (Dashboard Question)

**User asks:** "Dashboard có những thống kê gì?"

### Console Output:
```
🔍 Scanning codebase for: Dashboard có những thống kê gì?
🔑 Keywords: dashboard, thống, kê, statistics, chart, report, summary

📁 Cached 347 files
✅ Found 3 relevant files
  📄 Controllers/DashboardController.cs (score: 58.00)
  📄 Services/StatisticsService.cs (score: 48.00)
  📄 ClientApp/src/app/components/dashboard/dashboard.component.ts (score: 42.00)

🤖 Generating answer with Gemini...
✅ Response generated (1,078ms)

🧠 Generating follow-up questions...
🧠 Follow-up raw text: Chart.js có những loại biểu đồ nào?
Dashboard có real-time update không?
Thống kê theo khoa như thế nào?
🧠 Follow-up parsed: Chart.js có những loại biểu đồ nào? | Dashboard có real-time update không? | Thống kê theo khoa như thế nào?
🧠 Generated 3 follow-up questions

✅ Response cached for future instant retrieval
```

**Response Time:** ~1,100ms (cache hit for files, new Gemini API call)

---

## Cache Expiration (After 5 Minutes)

**User asks:** "Làm sao để thêm sinh viên?" (same question as before, but 6 minutes later)

### Console Output:
```
🔍 Scanning codebase for: Làm sao để thêm sinh viên?
🔑 Keywords: thêm, sinh, viên, student, students, sinh_vien, sinhvien, create, add

🔄 Cache expired, rescanning project...
✅ Scanned 347 files

📁 Cached 347 files
✅ Found 3 relevant files
  📄 Controllers/StudentsController.cs (score: 42.50)
  📄 Models/Student.cs (score: 38.00)
  📄 ClientApp/src/app/components/students/students.component.ts (score: 35.00)

🤖 Generating answer with Gemini...
✅ Response generated (1,198ms)

🧠 Generating follow-up questions...
🧠 Generated 3 follow-up questions

✅ Response cached for future instant retrieval
```

**Response Time:** ~3-4 seconds (re-scan + Gemini API)

---

## No Results Found (Fallback to Sample Docs)

**User asks:** "abcdefghijklmnop" (gibberish)

### Console Output:
```
🔍 Scanning codebase for: abcdefghijklmnop
🔑 Keywords: abcdefghijklmnop

📁 Cached 347 files
✅ Found 0 relevant files
⚠️ Scan returned 0 results, using sample docs

🤖 Generating answer with Gemini...
✅ Response generated (892ms)

🧠 Generating follow-up questions...
🧠 Generated 3 follow-up questions

✅ Response cached for future instant retrieval
```

**Response Time:** ~900ms (sample docs fallback)

---

## Error Handling (File Read Error)

### Console Output:
```
🔍 Scanning codebase for: Làm sao để login?
🔑 Keywords: login, đăng, nhập, auth, authenticate, account

🔄 Cache expired, rescanning project...
  ⚠️ Skip file C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem\Logs\app.log: System.IO.IOException: The process cannot access the file
✅ Scanned 346 files (1 skipped due to errors)

📁 Cached 346 files
✅ Found 3 relevant files
  📄 Controllers/AccountController.cs (score: 62.00)
  📄 Services/AuthService.cs (score: 55.00)
  📄 ClientApp/src/app/components/login/login.component.ts (score: 48.00)

🤖 Generating answer with Gemini...
✅ Response generated (1,056ms)

🧠 Generating follow-up questions...
🧠 Generated 3 follow-up questions

✅ Response cached for future instant retrieval
```

**Response Time:** ~3-4 seconds (scan with error handling)

---

## Performance Summary

| Scenario | Response Time | Notes |
|----------|--------------|-------|
| **First request (cold start)** | 3-4 seconds | Initial file scan + Gemini API |
| **Cache hit (same question)** | 0ms | Instant from response cache |
| **Cache hit (different question)** | 900-1,400ms | File cache + new Gemini API call |
| **Cache expired (6+ minutes)** | 3-4 seconds | Re-scan project + Gemini API |
| **Fallback to sample docs** | 800-1,000ms | No scan overhead |

---

## Monitoring Tips

### 1. Watch for Cache Hits
```
📁 Cached 347 files    ← Good! Using file cache
```

### 2. Monitor Scan Times
```
🔄 Cache expired, rescanning project...
✅ Scanned 347 files    ← Should complete in 2-3 seconds
```

### 3. Check Keyword Extraction
```
🔑 Keywords: sinh, viên, student, students, sinh_vien    ← Should see Vietnamese + English
```

### 4. Verify Relevance Scoring
```
✅ Found 3 relevant files
  📄 Controllers/StudentsController.cs (score: 42.50)    ← Higher scores = more relevant
  📄 Models/Student.cs (score: 38.00)
  📄 students.component.ts (score: 35.00)
```

### 5. Track Response Times
```
✅ Response generated (1,234ms)    ← Should be 800-1,400ms (Gemini API)
```

---

## Troubleshooting Console Output

### ⚠️ "Scan returned 0 results, using sample docs"
**Cause:** Keywords didn't match any files  
**Fix:** Add more term mappings in `ExtractKeywords()` method

### ⚠️ "Skip file X: System.IO.IOException"
**Cause:** File locked by another process  
**Fix:** Normal behavior, scanner continues with other files

### ⚠️ Slow scan (> 5 seconds)
**Cause:** Too many files or large files  
**Fix:** 
- Reduce file size limit (500KB → 200KB)
- Add more exclusion paths
- Check for network drives (slow I/O)

### ❌ "Cache expired, rescanning project..." on every request
**Cause:** Cache TTL too short or server restarting  
**Fix:**
- Increase cache TTL (5 min → 10 min)
- Check for application restarts
- Verify static cache dictionary persists

---

## Pro Tips

### 1. Enable Verbose Logging (Development Only)
Add to `CodebaseScanner.cs`:
```csharp
Console.WriteLine($"[DEBUG] Scanning file: {file}");
Console.WriteLine($"[DEBUG] File size: {fileInfo.Length} bytes");
Console.WriteLine($"[DEBUG] Score: {score:F2}");
```

### 2. Monitor Memory Usage
```csharp
Console.WriteLine($"[MEMORY] Cache size: {_fileCache.Count} files");
Console.WriteLine($"[MEMORY] Estimated memory: {_fileCache.Count * 50}KB");
```

### 3. Track API Key Rotation
```csharp
Console.WriteLine($"🔑 Using Gemini API key #{_currentKeyIndex + 1}/{_geminiApiKeys.Count}");
```

### 4. Benchmark Scan Performance
```csharp
var sw = Stopwatch.StartNew();
ScanDirectory(_projectRoot);
sw.Stop();
Console.WriteLine($"⏱️ Scan completed in {sw.ElapsedMilliseconds}ms");
```

---

**Happy Monitoring!** 🖥️✨
