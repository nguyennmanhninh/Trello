# AI Chatbox Integration Guide

## 🤖 Tích hợp AI Chatbox vào Student Management System

Project đã được chuẩn bị sẵn `ChatService` để tích hợp chatbox. Bạn có 3 options:

---

## ✅ OPTION 1: Tawk.to (RECOMMENDED - FREE)

### Tại sao chọn Tawk.to?
- ✅ **Hoàn toàn miễn phí** - không giới hạn
- ✅ **AI chatbot** tích hợp sẵn
- ✅ **Live chat** với mobile apps
- ✅ **Đọc được context** - có thể train bot với knowledge base
- ✅ **Analytics** và reporting
- ✅ **Multi-language** support

### Bước 1: Đăng ký Tawk.to

1. Truy cập: https://www.tawk.to/
2. Click "Sign Up Free"
3. Điền thông tin và xác nhận email

### Bước 2: Tạo Property

1. Login vào dashboard
2. Click "Add Property" 
3. Nhập thông tin:
   - **Property Name**: Student Management System
   - **Website URL**: http://localhost:4200 (hoặc domain của bạn)
4. Click "Create Property"

### Bước 3: Lấy Widget Code

1. Vào **Administration** → **Chat Widget**
2. Click tab **"Direct Chat Link"**
3. Copy 2 IDs từ script:
   ```javascript
   https://embed.tawk.to/YOUR_TAWK_ID/YOUR_WIDGET_ID
   ```
   
   Ví dụ:
   ```
   https://embed.tawk.to/6123456789abcdef/1ghijk2345
                      ^^^^^^^^^^^^^^^^    ^^^^^^^^^^
                      YOUR_TAWK_ID        YOUR_WIDGET_ID
   ```

### Bước 4: Cập nhật ChatService

Mở file: `ClientApp/src/app/services/chat.service.ts`

Tìm và thay thế:
```typescript
private tawkId = 'YOUR_TAWK_ID';        // ← Thay bằng Tawk ID của bạn
private tawkWidgetId = 'YOUR_WIDGET_ID'; // ← Thay bằng Widget ID của bạn
```

### Bước 5: Enable Chat Widget

Mở file: `ClientApp/src/app/app.component.ts`

Uncomment dòng này:
```typescript
private loadChatWidget(): void {
  // Uncomment dòng này:
  this.chatService.loadTawkTo(); // ← Remove comment
}
```

### Bước 6: Customize Widget (Optional)

Trong Tawk.to dashboard:
1. **Appearance** → Chọn màu sắc, vị trí widget
2. **Behavior** → Cài đặt greeting message, offline message
3. **Triggers** → Tự động hiện chat khi user vào trang nào

### Bước 7: Setup AI Knowledge Base

1. Vào **Knowledge Base** trong dashboard
2. Click "Add Category"
3. Thêm các câu hỏi thường gặp:
   - "Làm sao để đăng nhập?"
   - "Làm sao để thêm sinh viên?"
   - "Làm sao để nhập điểm?"
   - v.v.
4. Bot sẽ tự động trả lời các câu hỏi này

### Bước 8: Train AI Bot

1. Vào **Chat Pages** → **Chatbot**
2. Click "Add New Node"
3. Tạo conversation flows:
   ```
   User: "Tôi quên mật khẩu"
   Bot: "Bạn vui lòng liên hệ Admin để reset mật khẩu. 
        Email: admin@school.edu.vn hoặc SĐT: 0123456789"
   ```
4. Thêm nhiều scenarios để bot thông minh hơn

---

## ✅ OPTION 2: Custom ChatGPT Widget (Advanced)

### Yêu cầu:
- OpenAI API Key (https://platform.openai.com/)
- Cost: ~$0.002 per 1K tokens

### Bước 1: Tạo ChatGPT Service

File đã có sẵn: `chat.service.ts`

Thêm method mới:
```typescript
async askChatGPT(question: string): Promise<string> {
  const response = await fetch('https://api.openai.com/v1/chat/completions', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer YOUR_OPENAI_API_KEY'
    },
    body: JSON.stringify({
      model: 'gpt-3.5-turbo',
      messages: [
        {
          role: 'system',
          content: 'You are a helpful assistant for Student Management System'
        },
        {
          role: 'user',
          content: question
        }
      ]
    })
  });
  
  const data = await response.json();
  return data.choices[0].message.content;
}
```

### Bước 2: Tạo Chat UI Component

(Cần implement custom UI với chat bubbles, input box, etc.)

---

## ✅ OPTION 3: Tidio (Free Plan với AI)

### Setup Tidio:

1. Đăng ký: https://www.tidio.com/
2. Lấy Install Code từ dashboard
3. Paste vào `index.html`:

```html
<!-- Tidio Chat -->
<script src="//code.tidio.co/your-code-here.js" async></script>
```

### Or use ChatService:

```typescript
// In app.component.ts
this.chatService.loadCustomChatbot('//code.tidio.co/your-code-here.js');
```

---

## 🎯 CÁCH CHATBOT ĐỌC NỘI DUNG DỰ ÁN

### Option A: Tawk.to Knowledge Base
1. Vào dashboard Tawk.to
2. **Knowledge Base** → Add articles
3. Viết docs về:
   - Cách sử dụng từng module
   - Quyền của từng role
   - Troubleshooting common issues
4. Bot sẽ search và trả lời dựa trên knowledge base

### Option B: Custom GPT với RAG (Retrieval Augmented Generation)

**Nâng cao** - Cần setup backend:
1. Index toàn bộ docs, code comments vào vector database (Pinecone/Weaviate)
2. Khi user hỏi → search relevant docs
3. Pass context + question vào ChatGPT
4. ChatGPT trả lời dựa trên context của dự án

Example backend endpoint:
```csharp
[HttpPost("api/chat/ask")]
public async Task<IActionResult> AskAI([FromBody] string question)
{
    // 1. Search relevant docs from vector DB
    var relevantDocs = await _vectorDb.Search(question);
    
    // 2. Build context
    var context = string.Join("\n", relevantDocs);
    
    // 3. Call OpenAI with context
    var prompt = $@"
    Context from Student Management System:
    {context}
    
    User Question: {question}
    
    Please answer based on the context above.
    ";
    
    var response = await _openAI.ChatCompletion(prompt);
    return Ok(response);
}
```

---

## 🔧 ADVANCED: Set User Info to Chat

Khi user login, set thông tin vào chat:

```typescript
// In login.component.ts after successful login
import { ChatService } from '../services/chat.service';

constructor(private chatService: ChatService) {}

onLoginSuccess(user: any) {
  // Set user info to chat
  this.chatService.setAttributes(
    user.fullName,
    user.email,
    user.role // Admin, Teacher, Student
  );
  
  // Add tags for better routing
  this.chatService.addTags([user.role, user.departmentName]);
}
```

Benefit: Agent sẽ biết user là ai, role gì để support tốt hơn.

---

## 📊 MONITORING & ANALYTICS

### Tawk.to Dashboard:
- **Dashboard** → Xem số lượng chats
- **Reports** → Chat volume, response time
- **Visitors** → Xem ai đang online
- **Triggers** → Track conversion

### Custom Events:
```typescript
// Track important actions
window.Tawk_API.addEvent('grade-added', {
  studentId: 'SV001',
  courseId: 'CS101',
  score: 8.5
});
```

---

## 🚀 NEXT STEPS

1. **Chọn platform**: Tawk.to (recommended) hoặc Tidio
2. **Đăng ký và lấy code**
3. **Update `chat.service.ts`** với IDs
4. **Uncomment code** trong `app.component.ts`
5. **Run app** và test chatbox
6. **Setup knowledge base** để bot thông minh hơn

---

## ❓ SUPPORT

Nếu cần hỗ trợ tích hợp:
- Tawk.to docs: https://help.tawk.to/
- Tidio docs: https://www.tidio.com/help/
- OpenAI docs: https://platform.openai.com/docs/

---

## 📝 QUICK START - Tawk.to (5 phút)

```bash
# 1. Đăng ký Tawk.to
https://www.tawk.to/ → Sign Up

# 2. Lấy IDs từ widget code

# 3. Update chat.service.ts
private tawkId = 'YOUR_ID_HERE';
private tawkWidgetId = 'YOUR_WIDGET_ID_HERE';

# 4. Uncomment trong app.component.ts
this.chatService.loadTawkTo();

# 5. Run app
cd ClientApp
npm start

# 6. Test tại http://localhost:4200
# Widget sẽ hiện ở góc phải dưới
```

**Done! Chatbox đã hoạt động! 🎉**
