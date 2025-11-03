# 🤖 RAG SYSTEM SETUP GUIDE

## ✅ HOÀN THÀNH! RAG System Infrastructure Ready

### Những gì đã được tạo:

1. ✅ **RagService.cs** - Backend RAG service với OpenAI integration
2. ✅ **ChatController.cs** - API endpoint `/api/chat/ask`
3. ✅ **AiChatService.ts** - Angular service for calling RAG API
4. ✅ **AiChatComponent** - Beautiful chat UI với code sources display
5. ✅ **Configuration** - appsettings.json với OpenAI + Pinecone placeholders

---

## 🚀 QUICK START (30 PHÚT)

### BƯỚC 1: Lấy OpenAI API Key (5 phút)

1. **Truy cập**: https://platform.openai.com/
2. **Sign Up** (free $5 credit cho new users)
3. **API Keys** → Create new secret key
4. **Copy** key (chỉ hiện 1 lần!)

**Pricing:**
- gpt-3.5-turbo: ~$0.002 / 1K tokens (rẻ, nhanh)
- gpt-4-turbo-preview: ~$0.01 / 1K tokens (thông minh hơn)
- text-embedding-ada-002: ~$0.0001 / 1K tokens (embedding)

**Cost estimate:** ~$0.50 / 100 câu hỏi với GPT-4

### BƯỚC 2: Cập nhật API Key

Mở: `appsettings.Development.json`

```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-xxxxxxxxxxxxxxxxxxxxxxxxx", // ← Paste key here
    "Model": "gpt-4-turbo-preview" // hoặc "gpt-3.5-turbo" cho rẻ hơn
  }
}
```

### BƯỚC 3: Build Backend

```powershell
cd c:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
dotnet build
dotnet run
```

### BƯỚC 4: Test RAG API

```powershell
# Test health endpoint
curl http://localhost:5298/api/chat/health

# Test ask endpoint (without vector DB - sẽ dùng sample docs)
curl -X POST http://localhost:5298/api/chat/ask `
  -H "Content-Type: application/json" `
  -d '{"question": "Làm sao StudentController validate điểm?"}'
```

### BƯỚC 5: Run Angular

```powershell
cd ClientApp
npm start
```

### BƯỚC 6: Test UI!

1. Mở: http://localhost:4200
2. Click nút **"🤖 AI Assistant"** góc phải dưới
3. Hỏi: **"Làm sao StudentController validate điểm số?"**
4. AI sẽ trả lời với code snippets! 🎉

---

## 🔥 NÂNG CAO: Setup Vector Database (Optional - 1-2 giờ)

### Option A: Pinecone (RECOMMENDED - Free tier)

**Tại sao cần Vector DB?**
- Current: RAG dùng **sample docs** hardcoded trong code
- With Vector DB: Bot sẽ **search toàn bộ codebase** thực sự

**Setup:**

1. **Đăng ký Pinecone**:
   - https://www.pinecone.io/
   - Free tier: 1 index, 100K vectors
   
2. **Create Index**:
   - Name: `sms-codebase`
   - Dimensions: `1536` (OpenAI ada-002 embedding size)
   - Metric: `cosine`

3. **Get API Key**:
   - Settings → API Keys → Copy

4. **Update Config**:
   ```json
   {
     "Pinecone": {
       "ApiKey": "your-pinecone-api-key",
       "Environment": "us-east-1-aws", // từ dashboard
       "IndexName": "sms-codebase"
     }
   }
   ```

### Option B: ChromaDB (Local - Free)

**Install:**
```powershell
pip install chromadb
```

**Run Server:**
```powershell
chroma run --host localhost --port 8000
```

**Update RagService.cs** để call ChromaDB thay vì Pinecone (cần sửa code)

---

## 📚 INDEX CODEBASE VÀO VECTOR DB

### Script Python để index files:

```python
# index_codebase.py
import os
import openai
from pinecone import Pinecone

# Config
openai.api_key = "YOUR_OPENAI_KEY"
pc = Pinecone(api_key="YOUR_PINECONE_KEY")
index = pc.Index("sms-codebase")

# Files to index
file_extensions = ['.cs', '.ts', '.html', '.scss']
root_dir = "c:/Users/TDG/source/repos/StudentManagementSystem/StudentManagementSystem"

def chunk_code(content, chunk_size=1000):
    """Split code into chunks"""
    lines = content.split('\n')
    chunks = []
    current_chunk = []
    current_size = 0
    
    for line in lines:
        current_chunk.append(line)
        current_size += len(line)
        
        if current_size >= chunk_size:
            chunks.append('\n'.join(current_chunk))
            current_chunk = []
            current_size = 0
    
    if current_chunk:
        chunks.append('\n'.join(current_chunk))
    
    return chunks

def get_embedding(text):
    """Generate embedding using OpenAI"""
    response = openai.Embedding.create(
        model="text-embedding-ada-002",
        input=text
    )
    return response['data'][0]['embedding']

def index_file(file_path, relative_path):
    """Index a single file"""
    print(f"Indexing: {relative_path}")
    
    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()
    
    # Skip empty files
    if not content.strip():
        return
    
    # Split into chunks
    chunks = chunk_code(content)
    
    # Index each chunk
    for i, chunk in enumerate(chunks):
        chunk_id = f"{relative_path}_chunk_{i}"
        
        # Generate embedding
        embedding = get_embedding(chunk)
        
        # Upsert to Pinecone
        index.upsert(vectors=[{
            "id": chunk_id,
            "values": embedding,
            "metadata": {
                "fileName": os.path.basename(file_path),
                "filePath": relative_path,
                "fileType": os.path.splitext(file_path)[1][1:],
                "content": chunk,
                "chunkIndex": i
            }
        }])
        
        print(f"  ✓ Chunk {i+1}/{len(chunks)}")

def index_all_files():
    """Index all files in the project"""
    print("🚀 Starting indexing...")
    
    file_count = 0
    for root, dirs, files in os.walk(root_dir):
        # Skip node_modules, bin, obj
        dirs[:] = [d for d in dirs if d not in ['node_modules', 'bin', 'obj', '.git']]
        
        for file in files:
            if any(file.endswith(ext) for ext in file_extensions):
                file_path = os.path.join(root, file)
                relative_path = os.path.relpath(file_path, root_dir)
                
                try:
                    index_file(file_path, relative_path)
                    file_count += 1
                except Exception as e:
                    print(f"  ✗ Error: {e}")
    
    print(f"\n✅ Indexed {file_count} files!")

if __name__ == "__main__":
    index_all_files()
```

**Run script:**
```powershell
python index_codebase.py
```

**Cost:** ~$0.50 to index ~500 files (one-time cost)

---

## 🎯 CÁC LOẠI CÂU HỎI BOT CÓ THỂ TRẢ LỜI:

### Architecture & Design:
- ❓ "Giải thích authentication flow trong hệ thống"
- ❓ "Làm sao session và JWT được sử dụng?"
- ❓ "Mô tả cấu trúc của Grade Model"

### Code Navigation:
- ❓ "StudentController ở đâu và làm gì?"
- ❓ "Tìm code validate điểm số từ 0-10"
- ❓ "AuthorizeRole attribute được implement như thế nào?"

### Debugging:
- ❓ "Tại sao không xóa được sinh viên có điểm?"
- ❓ "Làm sao fix lỗi OPENJSON trong pagination?"
- ❓ "Giải thích lỗi 'Port 4200 already in use'"

### Best Practices:
- ❓ "Cách tốt nhất để thêm validation cho model?"
- ❓ "Làm sao implement role-based authorization?"
- ❓ "Best practice cho EF Core relationships?"

### Feature Implementation:
- ❓ "Làm sao để thêm một API endpoint mới?"
- ❓ "Steps để tạo CRUD module cho entity mới"
- ❓ "Cách export data sang Excel như trong StudentsController?"

---

## 📊 SO SÁNH: Tawk.to KB vs RAG System

| Feature | Tawk.to KB | RAG System |
|---------|-----------|------------|
| **Setup Time** | 10 phút | 30 phút - 2 giờ |
| **Cost** | FREE | ~$10/tháng |
| **Câu hỏi support** | General Q&A | Code-specific |
| **Hiểu code** | ❌ | ✅ |
| **Show code snippets** | ❌ | ✅ |
| **Context aware** | ❌ | ✅ Full |
| **Update knowledge** | Manual | Auto (từ code) |
| **Use cases** | User support | Developer assistant |

**KHUYẾN NGHỊ:**
- **Tawk.to**: Cho end users (Admin, Teacher, Student)
- **RAG**: Cho developers và technical questions

**BOTH**: Bật cả 2! User hỏi "Làm sao thêm sinh viên?" → Tawk.to trả lời. Developer hỏi "StudentController.Create method hoạt động thế nào?" → RAG trả lời.

---

## 🔧 TROUBLESHOOTING

### Lỗi: "OpenAI API key not found"
- Check `appsettings.Development.json` có API key chưa
- Verify key còn valid: https://platform.openai.com/api-keys

### Lỗi: "Rate limit exceeded"
- OpenAI có limit: 3 requests/min (free tier)
- Upgrade to paid tier hoặc wait 1 phút

### Lỗi: "Pinecone connection failed"
- Check API key và environment name
- Verify index name đúng
- Fallback: Bot vẫn work với sample docs

### Bot trả lời không chính xác:
- Current: Bot dùng sample docs → không có đủ context
- Solution: Index codebase vào Pinecone (see above)

### Câu hỏi không được trả lời:
- Check Console (F12) xem có lỗi API không
- Verify backend đang chạy trên port 5298
- Check proxy config trong angular.json

---

## 💡 OPTIMIZATION TIPS

### 1. Reduce Cost:
```json
{
  "OpenAI": {
    "Model": "gpt-3.5-turbo" // Thay vì gpt-4, rẻ hơn 10x
  }
}
```

### 2. Improve Speed:
- Cache frequent questions trong localStorage
- Implement streaming responses (SSE)
- Use smaller chunk sizes (500 tokens)

### 3. Better Accuracy:
- Index more context (comments, README, docs)
- Increase `topK` từ 5 → 10 documents
- Add example Q&A pairs vào system prompt

### 4. Production Ready:
- Add rate limiting
- Implement user feedback (👍/👎)
- Track popular questions
- Add conversation memory

---

## 📈 USAGE METRICS

Track trong backend:
```csharp
// Log every question
_logger.LogInformation("RAG Question: {question} from {user}", question, userId);

// Track response time
var stopwatch = Stopwatch.StartNew();
var response = await _ragService.AskQuestion(question);
stopwatch.Stop();
_logger.LogInformation("Response time: {ms}ms", stopwatch.ElapsedMilliseconds);
```

---

## 🎓 LEARNING RESOURCES

**RAG Concepts:**
- https://www.pinecone.io/learn/retrieval-augmented-generation/
- https://platform.openai.com/docs/guides/embeddings

**OpenAI API:**
- https://platform.openai.com/docs/api-reference
- https://cookbook.openai.com/

**Vector Databases:**
- Pinecone: https://docs.pinecone.io/
- ChromaDB: https://www.trychroma.com/

---

## ✅ CHECKLIST

- [ ] Get OpenAI API key
- [ ] Update `appsettings.Development.json`
- [ ] Build backend (dotnet build)
- [ ] Test `/api/chat/health` endpoint
- [ ] Test `/api/chat/ask` with sample question
- [ ] Run Angular (npm start)
- [ ] Open http://localhost:4200
- [ ] Click "🤖 AI Assistant" button
- [ ] Ask technical question about code
- [ ] Verify response with code snippets
- [ ] (Optional) Setup Pinecone
- [ ] (Optional) Run index_codebase.py
- [ ] (Optional) Test with full codebase context

---

## 🚀 NEXT STEPS

1. **Test với OpenAI** (30 phút):
   - Get API key
   - Update config
   - Run và test

2. **Setup Vector DB** (1-2 giờ) nếu muốn bot thông minh hơn:
   - Sign up Pinecone
   - Run indexing script
   - Test với real codebase search

3. **Customize** (optional):
   - Thay đổi system prompt trong RagService.cs
   - Add thêm sample questions
   - Customize UI colors

4. **Production** (nếu deploy):
   - Move API keys to Azure Key Vault
   - Add rate limiting
   - Implement caching
   - Monitor costs

---

**🎉 RAG System sẵn sàng! Bot giờ có thể đọc và hiểu CODE thực sự!**
