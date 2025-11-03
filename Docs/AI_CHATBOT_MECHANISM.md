# 🤖 AI CHATBOT - CƠ CHẾ HOẠT ĐỘNG CHI TIẾT

**Ngày tạo:** October 24, 2025  
**Hệ thống:** Student Management System  
**Công nghệ:** RAG (Retrieval Augmented Generation)

---

## 📋 MỤC LỤC

1. [Tổng Quan Kiến Trúc](#1-tổng-quan-kiến-trúc)
2. [Luồng Xử Lý RAG Pipeline](#2-luồng-xử-lý-rag-pipeline)
3. [Frontend (Angular)](#3-frontend-angular)
4. [Backend (ASP.NET Core)](#4-backend-aspnet-core)
5. [AI Models](#5-ai-models)
6. [Cache & Optimization](#6-cache--optimization)
7. [Features Chi Tiết](#7-features-chi-tiết)

---

## 1. TỔNG QUAN KIẾN TRÚC

### 🎯 Mục Đích

AI Chatbot giúp developers hiểu và làm việc với codebase bằng cách:
- Trả lời câu hỏi về code, architecture, logic
- Giải thích các Controller, Service, Model
- Hướng dẫn fix bugs và best practices
- Tìm kiếm code snippets liên quan

### 🏗️ Kiến Trúc Tổng Thể

```
┌─────────────────────────────────────────────────────────────┐
│                      USER INTERFACE                         │
│  ┌───────────────────────────────────────────────────────┐ │
│  │   Angular Component (ai-chat.component.ts)            │ │
│  │   - Chat UI with typing animation                     │ │
│  │   - Message history                                   │ │
│  │   - Code syntax highlighting                          │ │
│  │   - Follow-up questions                               │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                             ↕ HTTP (JSON)
┌─────────────────────────────────────────────────────────────┐
│                      API LAYER                              │
│  ┌───────────────────────────────────────────────────────┐ │
│  │   ChatController.cs                                   │ │
│  │   - POST /api/chat/ask                                │ │
│  │   - GET /api/chat/health                              │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                             ↕
┌─────────────────────────────────────────────────────────────┐
│                    RAG SERVICE LAYER                        │
│  ┌───────────────────────────────────────────────────────┐ │
│  │   RagService.cs                                       │ │
│  │   1. Response Cache (instant answers)                 │ │
│  │   2. Vector Search (find relevant code)               │ │
│  │   3. Context Building (format code snippets)          │ │
│  │   4. AI Generation (OpenAI/Gemini)                    │ │
│  │   5. Follow-up Questions Generation                   │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                             ↕
┌─────────────────────────────────────────────────────────────┐
│                   EXTERNAL SERVICES                         │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐  │
│  │   OpenAI     │  │    Gemini    │  │   Pinecone      │  │
│  │   GPT-4      │  │ gemini-2.0   │  │ Vector Database │  │
│  │  (optional)  │  │  flash-exp   │  │   (optional)    │  │
│  └──────────────┘  └──────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. LUỒNG XỬ LÝ RAG PIPELINE

### 🔄 RAG là gì?

**RAG (Retrieval Augmented Generation)** = Tìm kiếm + Sinh câu trả lời

Thay vì AI "đoán" câu trả lời, RAG:
1. **Retrieve**: Tìm code liên quan từ codebase
2. **Augment**: Bổ sung context cho AI
3. **Generate**: AI tạo câu trả lời dựa trên context thực tế

### 📊 Quy Trình Chi Tiết (8 Bước)

```
┌──────────────────────────────────────────────────────────────┐
│ BƯỚC 1: User gửi câu hỏi                                     │
│ ─────────────────────────────────────────────────────────── │
│ Example: "AuthorizeRole attribute hoạt động như thế nào?"   │
└──────────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────────┐
│ BƯỚC 2: Frontend tạo user message                            │
│ ─────────────────────────────────────────────────────────── │
│ AiChatService.askQuestion(question)                          │
│ - Add message to chat history                                │
│ - Show loading state                                         │
│ - POST to /api/chat/ask                                      │
└──────────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────────┐
│ BƯỚC 3: Backend nhận request                                 │
│ ─────────────────────────────────────────────────────────── │
│ ChatController.Ask(request)                                  │
│ - Validate question                                          │
│ - Get user role from session                                 │
│ - Call RagService.AskQuestion()                              │
└──────────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────────┐
│ BƯỚC 4: Check cache (SPEED OPTIMIZATION)                     │
│ ─────────────────────────────────────────────────────────── │
│ RagService._responseCache                                    │
│ - Key: question.ToLower().Trim()                             │
│ - Expiration: 1 hour                                         │
│ - HIT → Return instant (0ms)                                 │
│ - MISS → Continue to step 5                                  │
└──────────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────────┐
│ BƯỚC 5: Tìm code liên quan (Vector Search)                   │
│ ─────────────────────────────────────────────────────────── │
│ 2 MÔ HÌNH:                                                   │
│                                                              │
│ A) GEMINI MODE (Default - FREE):                             │
│    GetSampleDocuments(topK: 2)                               │
│    - Use pre-defined sample code snippets                    │
│    - No embedding needed → FAST (< 100ms)                    │
│    - Perfect for demo/development                            │
│                                                              │
│ B) OPENAI MODE (Full RAG):                                   │
│    1. GenerateEmbedding(question)                            │
│       → Convert question to vector [768 floats]              │
│    2. SearchVectorDatabase(embedding, topK: 5)               │
│       → Pinecone similarity search                           │
│       → Returns top 5 most relevant code snippets            │
└──────────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────────┐
│ BƯỚC 6: Build context từ code snippets                       │
│ ─────────────────────────────────────────────────────────── │
│ BuildContext(relevantDocs)                                   │
│ - Format: [FileName]\n CodeSnippet \n\n                      │
│ - Example:                                                   │
│   [AuthorizeRoleAttribute.cs]                                │
│   public class AuthorizeRoleAttribute : Attribute...         │
│                                                              │
│   [StudentsController.cs]                                    │
│   [AuthorizeRole("Admin", "Teacher")]...                     │
└──────────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────────┐
│ BƯỚC 7: AI generate answer                                   │
│ ─────────────────────────────────────────────────────────── │
│ GenerateAnswer(question, context, userRole)                  │
│                                                              │
│ A) GEMINI (gemini-2.0-flash-exp):                            │
│    - POST to Google Generative AI API                        │
│    - Prompt: System + Context + Question                     │
│    - Config: temperature=1.0, maxTokens=800 (SPEED)          │
│    - Response time: ~1-3 seconds                             │
│                                                              │
│ B) OPENAI (gpt-4-turbo-preview):                             │
│    - POST to OpenAI Chat Completions API                     │
│    - Messages: [{system}, {user: context + question}]        │
│    - Response time: ~3-8 seconds                             │
└──────────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────────┐
│ BƯỚC 8: Generate follow-up questions                         │
│ ─────────────────────────────────────────────────────────── │
│ GenerateFollowUpQuestions(question, answer)                  │
│ - AI suggests 3 related questions                            │
│ - User can click to continue conversation                    │
│ - Example:                                                   │
│   "Làm sao apply AuthorizeRole cho nhiều roles?"            │
│   "Có thể custom error message không?"                      │
│   "AuthorizeRole khác gì [Authorize(Roles=...)]?"           │
└──────────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────────┐
│ BƯỚC 9: Cache response & Return                              │
│ ─────────────────────────────────────────────────────────── │
│ RagService._responseCache[key] = new CachedResponse          │
│ Return {                                                     │
│   success: true,                                             │
│   answer: "...",                                             │
│   sources: [{fileName, filePath, codeSnippet, score}],       │
│   followUpQuestions: ["...", "...", "..."]                   │
│ }                                                            │
└──────────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────────┐
│ BƯỚC 10: Frontend hiển thị với typing animation              │
│ ─────────────────────────────────────────────────────────── │
│ startTypingAnimation(fullText)                               │
│ - Show text character by character (20ms/char)               │
│ - Effect giống ChatGPT                                       │
│ - After typing done → show follow-up questions               │
└──────────────────────────────────────────────────────────────┘
```

---

## 3. FRONTEND (ANGULAR)

### 📁 Files Structure

```
ClientApp/src/app/
├── components/
│   └── ai-chat/
│       ├── ai-chat.component.ts        # Main logic
│       ├── ai-chat.component.html      # UI template
│       └── ai-chat.component.scss      # Styling
└── services/
    └── ai-chat.service.ts              # API communication
```

### 🎨 Component Logic (ai-chat.component.ts)

#### Key Features:

**1. Message Management**
```typescript
messages: ChatMessage[] = [];  // Chat history
currentQuestion = '';          // Input field
loading = false;              // Loading state
```

**2. Typing Animation (ChatGPT-like)**
```typescript
private startTypingAnimation(fullText: string): void {
  this.isTyping = true;
  this.typingMessage = '';
  
  let index = 0;
  const speed = 20; // 20ms per character = 50 chars/sec
  
  this.typingInterval = setInterval(() => {
    if (index < fullText.length) {
      this.typingMessage += fullText[index];
      index++;
      this.scrollToBottom();
    } else {
      clearInterval(this.typingInterval);
      this.isTyping = false;
      this.typingMessage = '';
      
      // Show follow-up questions after typing completes
      this.displayFollowUpQuestions();
    }
  }, speed);
}
```

**3. Code Syntax Highlighting (highlight.js)**
```typescript
ngAfterViewChecked(): void {
  // Highlight all code blocks
  document.querySelectorAll('.code-snippet pre code').forEach((block) => {
    if (!block.getAttribute('data-highlighted')) {
      hljs.highlightElement(block as HTMLElement);
      block.setAttribute('data-highlighted', 'true');
    }
  });
}
```

**4. Follow-Up Questions**
```typescript
askFollowUpQuestion(question: string): void {
  this.currentQuestion = question;
  this.sendQuestion();  // Continue conversation
}
```

**5. Sample Questions**
```typescript
sampleQuestions = [
  '❓ Làm sao StudentController validate điểm số?',
  '❓ Explain authentication flow trong hệ thống',
  '❓ Grade Model có những thuộc tính gì?',
  '❓ Làm sao để thêm một API endpoint mới?',
  '❓ AuthorizeRole attribute hoạt động như thế nào?'
];
```

### 🌐 Service Logic (ai-chat.service.ts)

#### Key Methods:

**1. Ask Question**
```typescript
askQuestion(question: string): Observable<ChatMessage> {
  this.loadingSubject.next(true);

  // Add user message immediately
  const userMessage: ChatMessage = {
    role: 'user',
    content: question,
    timestamp: new Date()
  };
  this.addMessage(userMessage);

  // Call API
  return this.http.post<ChatResponse>(`${this.apiUrl}/ask`, { question }).pipe(
    map(response => {
      this.loadingSubject.next(false);

      if (!response.success) {
        throw new Error(response.error || 'Failed to get response');
      }

      // Map PascalCase (C#) → camelCase (TypeScript)
      const mappedSources = response.sources?.map((s: any) => ({
        fileName: s.FileName || s.fileName,
        filePath: s.FilePath || s.filePath,
        codeSnippet: s.CodeSnippet || s.codeSnippet,
        score: s.Score ?? s.score
      })) || [];

      // Extract follow-up questions
      const followUps = response.followUpQuestions || [];

      // Add assistant message
      const assistantMessage: ChatMessage = {
        role: 'assistant',
        content: response.answer,
        timestamp: new Date(response.timestamp),
        sources: mappedSources,
        followUpQuestions: followUps
      };
      this.addMessage(assistantMessage);

      return assistantMessage;
    })
  );
}
```

**2. Local Storage Persistence**
```typescript
private saveChatHistory(messages: ChatMessage[]): void {
  try {
    const messagesToSave = messages.slice(-50); // Keep last 50
    localStorage.setItem('ai-chat-history', JSON.stringify(messagesToSave));
  } catch (error) {
    console.warn('Failed to save chat history:', error);
  }
}

private loadChatHistory(): void {
  try {
    const saved = localStorage.getItem('ai-chat-history');
    if (saved) {
      const messages = JSON.parse(saved);
      messages.forEach((msg: any) => {
        msg.timestamp = new Date(msg.timestamp); // Restore Date objects
      });
      this.messagesSubject.next(messages);
    }
  } catch (error) {
    console.warn('Failed to load chat history:', error);
  }
}
```

**3. RxJS State Management**
```typescript
private messagesSubject = new BehaviorSubject<ChatMessage[]>([]);
public messages$ = this.messagesSubject.asObservable();

private loadingSubject = new BehaviorSubject<boolean>(false);
public loading$ = this.loadingSubject.asObservable();
```

---

## 4. BACKEND (ASP.NET CORE)

### 📁 Files Structure

```
Controllers/API/
└── ChatController.cs           # API endpoints

Services/
└── RagService.cs               # Core RAG logic (640 lines)
```

### 🎯 ChatController.cs

**Endpoints:**

```csharp
[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        // 1. Validate
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "Question is required" });

        try
        {
            // 2. Get user role from session
            var userRole = HttpContext.Session.GetString("UserRole");

            // 3. Process with RAG
            var response = await _ragService.AskQuestion(request.Question, userRole);

            // 4. Return JSON response
            return Ok(new
            {
                success = true,
                answer = response.Answer,
                sources = response.Sources,
                followUpQuestions = response.FollowUpQuestions,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                error = $"Error processing request: {ex.Message}"
            });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "RAG Chat API",
            timestamp = DateTime.UtcNow
        });
    }
}
```

### 🧠 RagService.cs (Core Logic)

#### 1. Configuration

```csharp
public class RagService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _aiProvider;        // "OpenAI" or "Gemini"
    private readonly string _openAiApiKey;
    private readonly string _geminiApiKey;
    private readonly string _pineconeApiKey;
    
    // Response cache (in-memory)
    private static readonly Dictionary<string, CachedResponse> _responseCache = new();
    private static readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);
}
```

#### 2. Main RAG Method

```csharp
public async Task<RagResponse> AskQuestion(string question, string? userRole = null)
{
    try
    {
        // STEP 1: Check cache
        var cacheKey = question.ToLower().Trim();
        if (_responseCache.TryGetValue(cacheKey, out var cachedResponse))
        {
            if (DateTime.UtcNow - cachedResponse.Timestamp < _cacheExpiration)
            {
                // Cache hit! Return instantly
                return new RagResponse
                {
                    Success = true,
                    Answer = cachedResponse.Answer + "\n\n✨ *Cache response*",
                    Sources = cachedResponse.Sources,
                    FollowUpQuestions = await GenerateFollowUpQuestions(...)
                };
            }
        }

        // STEP 2: Get relevant documents
        List<RelevantDocument> relevantDocs;
        
        if (_aiProvider == "Gemini")
        {
            // Gemini mode: Use sample documents (FAST)
            relevantDocs = GetSampleDocuments(topK: 2);
        }
        else
        {
            // OpenAI mode: Full vector search
            var embedding = await GenerateEmbedding(question);
            relevantDocs = await SearchVectorDatabase(embedding, topK: 5);
        }

        // STEP 3: Build context
        var context = BuildContext(relevantDocs);

        // STEP 4: Generate answer
        var answer = await GenerateAnswer(question, context, userRole);

        // STEP 5: Generate follow-ups
        var followUps = await GenerateFollowUpQuestions(question, answer);

        // STEP 6: Cache response
        _responseCache[cacheKey] = new CachedResponse
        {
            Answer = answer,
            Sources = ...,
            Timestamp = DateTime.UtcNow
        };

        // STEP 7: Return
        return new RagResponse
        {
            Success = true,
            Answer = answer,
            Sources = ...,
            FollowUpQuestions = followUps
        };
    }
    catch (Exception ex)
    {
        return new RagResponse
        {
            Success = false,
            Error = $"Error: {ex.Message}"
        };
    }
}
```

#### 3. AI Generation (Gemini)

```csharp
private async Task<string> GenerateAnswerWithGemini(
    string question, 
    string context, 
    string? userRole)
{
    // Build prompt
    var systemPrompt = @"AI Assistant for Student Management System.
Answer in Vietnamese. Be concise. Use code examples from context.";

    var prompt = $"{systemPrompt}\n\nContext:\n{context}\n\nQ: {question}\nA:";

    // Gemini API request
    var request = new
    {
        contents = new[]
        {
            new { parts = new[] { new { text = prompt } } }
        },
        generationConfig = new
        {
            temperature = 1.0,        // Max speed
            maxOutputTokens = 800,    // Shorter = faster
            topK = 1,
            topP = 0.8
        }
    };

    // Call Gemini API
    var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent?key={_geminiApiKey}";
    
    var response = await _httpClient.PostAsync(url, ...);
    
    // Parse response
    var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
    var answer = result
        .GetProperty("candidates")[0]
        .GetProperty("content")
        .GetProperty("parts")[0]
        .GetProperty("text")
        .GetString();

    return answer;
}
```

#### 4. Follow-Up Questions Generation

```csharp
private async Task<List<string>> GenerateFollowUpQuestions(
    string originalQuestion, 
    string answer)
{
    var prompt = $@"Based on this Q&A, suggest 3 follow-up questions:

Q: {originalQuestion}
A: {answer}

Generate 3 short, relevant follow-up questions in Vietnamese.
Format: Just the questions, one per line, no numbering.";

    var followUpAnswer = await GenerateAnswerWithGemini(prompt, "", null);
    
    // Parse questions
    var questions = followUpAnswer
        .Split('\n')
        .Where(q => !string.IsNullOrWhiteSpace(q))
        .Select(q => q.Trim())
        .Take(3)
        .ToList();

    return questions;
}
```

---

## 5. AI MODELS

### 🤖 Google Gemini (Default)

**Model:** `gemini-2.0-flash-exp`

**Tại sao chọn Gemini?**
- ✅ **FREE**: No credit card required
- ✅ **FAST**: ~1-3 seconds response time
- ✅ **GOOD QUALITY**: Comparable to GPT-3.5
- ✅ **HIGH QUOTA**: 15 requests/minute (free tier)

**API Endpoint:**
```
https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent?key={API_KEY}
```

**Configuration (appsettings.json):**
```json
{
  "AI": {
    "Provider": "Gemini"
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  }
}
```

**Speed Optimizations:**
- `temperature = 1.0` → Less thinking, faster response
- `maxOutputTokens = 800` → Shorter answers
- `topK = 1` → Pick best choice immediately
- `topP = 0.8` → Less randomness

### 🤖 OpenAI GPT-4 (Optional)

**Model:** `gpt-4-turbo-preview`

**Tại sao optional?**
- ⚠️ **PAID**: Requires credit card
- ⚠️ **SLOW**: ~3-8 seconds response time
- ⚠️ **EXPENSIVE**: $0.01/1K tokens input, $0.03/1K tokens output

**Use case:**
- Production deployments
- When higher quality is needed
- When budget allows

**Configuration:**
```json
{
  "AI": {
    "Provider": "OpenAI"
  },
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY"
  }
}
```

---

## 6. CACHE & OPTIMIZATION

### 🚀 Response Cache (In-Memory)

**Mục đích:** Tăng tốc độ trả lời cho câu hỏi lặp lại

**Implementation:**
```csharp
private static readonly Dictionary<string, CachedResponse> _responseCache = new();
private static readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);
```

**Flow:**
1. User asks: "AuthorizeRole hoạt động thế nào?"
2. Check cache: `_responseCache[question.ToLower().Trim()]`
3. **HIT** → Return instant (0ms, no API call)
4. **MISS** → Call AI API, then cache result

**Benefits:**
- ✅ **0ms response** for repeated questions
- ✅ Save API costs
- ✅ Better UX (instant answers)

**Expiration:** 1 hour (configurable)

### ⚡ Other Optimizations

**1. Minimal Context (Gemini mode)**
```csharp
relevantDocs = GetSampleDocuments(topK: 2);  // Only 2 docs instead of 5
```

**2. Short Prompts**
```csharp
var systemPrompt = @"AI Assistant for SMS. Answer concisely.";
// Instead of long detailed system prompt
```

**3. Limited Output Tokens**
```csharp
maxOutputTokens = 800  // Shorter answer = faster generation
```

**4. No Embeddings (Gemini mode)**
```csharp
// Skip expensive embedding generation
// Use pre-defined sample documents instead
```

---

## 7. FEATURES CHI TIẾT

### ✨ Feature 1: Typing Animation

**Mô tả:** Text hiển thị từng ký tự giống ChatGPT

**Code:**
```typescript
startTypingAnimation(fullText: string): void {
  this.isTyping = true;
  let index = 0;
  const speed = 20; // ms per character
  
  this.typingInterval = setInterval(() => {
    if (index < fullText.length) {
      this.typingMessage += fullText[index];
      index++;
    } else {
      clearInterval(this.typingInterval);
      this.isTyping = false;
    }
  }, speed);
}
```

**Effect:**
```
"AuthorizeRole là một..."
↓
"AuthorizeRole là một custom..."
↓
"AuthorizeRole là một custom attribute..."
```

---

### ✨ Feature 2: Code Syntax Highlighting

**Library:** `highlight.js`

**Supported Languages:**
- C# (csharp)
- TypeScript
- JavaScript
- HTML/XML
- CSS/SCSS
- SQL
- JSON

**Implementation:**
```typescript
highlightCode(code: string, language: string): string {
  try {
    const result = hljs.highlight(code, { language, ignoreIllegals: true });
    return result.value;  // HTML with <span class="hljs-...">
  } catch (err) {
    return hljs.highlightAuto(code).value;  // Auto-detect
  }
}
```

**Result:** Beautiful colored code blocks in chat

---

### ✨ Feature 3: Code Sources Display

**Mô tả:** Show which files the answer came from

**Data Structure:**
```typescript
interface CodeSource {
  fileName: string;      // "StudentsController.cs"
  filePath: string;      // "Controllers/StudentsController.cs"
  codeSnippet: string;   // Actual code
  score: number;         // Relevance score (0-1)
}
```

**UI:**
```html
<div class="sources" *ngIf="message.sources && message.sources.length > 0">
  <button (click)="toggleSources(i)">
    📄 View {{ message.sources.length }} code sources
  </button>
  
  <div *ngIf="areSourcesVisible(i)">
    <div *ngFor="let source of message.sources" class="source-item">
      <div class="source-header">
        {{ getFileIcon(source.fileName) }} {{ source.fileName }}
        <span class="score">Score: {{ (source.score * 100).toFixed(0) }}%</span>
      </div>
      <pre><code [innerHTML]="highlightCode(source.codeSnippet)"></code></pre>
      <button (click)="copyCode(source.codeSnippet)">📋 Copy</button>
    </div>
  </div>
</div>
```

---

### ✨ Feature 4: Follow-Up Questions

**Mô tả:** AI suggest câu hỏi tiếp theo

**Backend Generation:**
```csharp
var prompt = $@"Based on Q&A, suggest 3 follow-up questions:
Q: {originalQuestion}
A: {answer}

Generate 3 short questions in Vietnamese.";

var followUpAnswer = await GenerateAnswerWithGemini(prompt, "", null);
var questions = followUpAnswer.Split('\n').Take(3).ToList();
```

**Frontend Display:**
```html
<div class="follow-ups" *ngIf="message.followUpQuestions?.length">
  <strong>💡 Câu hỏi tiếp theo:</strong>
  <button *ngFor="let q of message.followUpQuestions"
          (click)="askFollowUpQuestion(q)">
    {{ q }}
  </button>
</div>
```

**Example:**
```
Original Q: "AuthorizeRole hoạt động thế nào?"

Follow-ups:
→ "Làm sao apply AuthorizeRole cho nhiều roles?"
→ "Có thể custom error message không?"
→ "AuthorizeRole khác gì [Authorize(Roles=...)]?"
```

---

### ✨ Feature 5: Sample Questions

**Mô tả:** Quick start với câu hỏi mẫu

**Data:**
```typescript
sampleQuestions = [
  '❓ Làm sao StudentController validate điểm số?',
  '❓ Explain authentication flow trong hệ thống',
  '❓ Grade Model có những thuộc tính gì?',
  '❓ Làm sao để thêm một API endpoint mới?',
  '❓ AuthorizeRole attribute hoạt động như thế nào?'
];
```

**UI:**
```html
<button (click)="toggleSampleQuestions()">
  💡 Câu hỏi mẫu
</button>

<div *ngIf="showSampleQuestions">
  <button *ngFor="let q of sampleQuestions"
          (click)="askSampleQuestion(q)">
    {{ q }}
  </button>
</div>
```

---

### ✨ Feature 6: Chat History Persistence

**Storage:** `localStorage`

**Key:** `ai-chat-history`

**Limit:** Last 50 messages

**Save:**
```typescript
private saveChatHistory(messages: ChatMessage[]): void {
  const messagesToSave = messages.slice(-50);
  localStorage.setItem('ai-chat-history', JSON.stringify(messagesToSave));
}
```

**Load on Init:**
```typescript
private loadChatHistory(): void {
  const saved = localStorage.getItem('ai-chat-history');
  if (saved) {
    const messages = JSON.parse(saved);
    messages.forEach(msg => {
      msg.timestamp = new Date(msg.timestamp);  // Restore Date objects
    });
    this.messagesSubject.next(messages);
  }
}
```

---

## 8. ERROR HANDLING

### ⚠️ Common Errors & Solutions

**1. Gemini 404 - Model Not Found**
```
Error: ❌ Gemini model not found
Solution: Use gemini-2.0-flash-exp (tested working model)
```

**2. Gemini 429 - Rate Limit**
```
Error: ⏱️ Rate limit exceeded
Solution: Wait 1 minute (free tier: 15 req/min)
```

**3. Gemini 503 - Service Unavailable**
```
Error: 🔧 Service temporarily unavailable
Solution: Retry after few moments
```

**4. No API Key**
```
Error: API key missing
Solution: Add to appsettings.json:
{
  "Gemini": {
    "ApiKey": "YOUR_KEY_HERE"
  }
}
```

**5. CORS Error (Frontend)**
```
Error: CORS policy blocked
Solution: Already handled by proxy.conf.json:
{
  "/api": {
    "target": "http://localhost:5298",
    "secure": false
  }
}
```

---

## 9. PERFORMANCE METRICS

### ⚡ Response Times

| Scenario | Time | Details |
|----------|------|---------|
| **Cache Hit** | 0-10ms | Instant, no API call |
| **Gemini (no cache)** | 1-3s | API call + generation |
| **OpenAI GPT-4** | 3-8s | API call + generation |
| **With typing animation** | +5-10s | Character-by-character display |

### 💰 Cost Analysis (1000 questions)

| Provider | Cost | Notes |
|----------|------|-------|
| **Gemini (FREE)** | $0 | Free tier: 15 req/min |
| **OpenAI GPT-4** | ~$30-50 | Based on token usage |
| **With 50% cache hit** | $15-25 | Half requests cached |

---

## 10. TROUBLESHOOTING

### 🔧 Debug Checklist

**Frontend Issues:**
```typescript
// 1. Check service injection
constructor(public aiChatService: AiChatService) {}

// 2. Check API response in console
console.log('AI Response:', response);

// 3. Check error handling
error: (error) => {
  console.error('Error:', error);
}
```

**Backend Issues:**
```csharp
// 1. Check API key configuration
Console.WriteLine($"AI Provider: {_aiProvider}");
Console.WriteLine($"Has Gemini Key: {!string.IsNullOrEmpty(_geminiApiKey)}");

// 2. Test health endpoint
GET /api/chat/health
→ Should return { status: "healthy" }

// 3. Check error messages
catch (Exception ex)
{
    Console.WriteLine($"RAG Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}
```

**API Testing:**
```bash
# Test with curl
curl -X POST http://localhost:5298/api/chat/ask \
  -H "Content-Type: application/json" \
  -d '{"question": "Hello AI"}'

# Expected response:
{
  "success": true,
  "answer": "...",
  "sources": [...],
  "followUpQuestions": [...],
  "timestamp": "2025-10-24T..."
}
```

---

## 11. FUTURE ENHANCEMENTS

### 🚀 Planned Features

1. **Streaming Responses**
   - Server-Sent Events (SSE)
   - Real-time token-by-token display
   - Better UX for long answers

2. **Voice Input**
   - Web Speech API
   - Speech-to-text
   - Hands-free interaction

3. **Code Execution**
   - Run C# snippets in sandbox
   - Show live output
   - Interactive debugging

4. **Multi-language Support**
   - English, Vietnamese, Japanese
   - Auto-detect user language
   - Translate answers

5. **Conversation Threads**
   - Group related questions
   - Thread history
   - Context preservation

---

## 📚 REFERENCES

**Documentation:**
- Google Gemini API: https://ai.google.dev/docs
- OpenAI API: https://platform.openai.com/docs
- Highlight.js: https://highlightjs.org/
- Angular HttpClient: https://angular.io/api/common/http/HttpClient

**Related Files:**
- `/Docs/RAG_SYSTEM_COMPLETE.md` - Full RAG documentation
- `/Docs/RAG_SETUP_GUIDE.md` - Setup instructions
- `/ClientApp/THEME_GUIDE.md` - UI styling guide

---

**Document created by:** AI Code Analysis System  
**Date:** October 24, 2025  
**Status:** ✅ COMPLETE & DETAILED  
**Version:** 1.0
