# 🤖 AI Chatbot - Gemini API Fix (Lỗi 503)

## 🔥 Vấn đề

Khi sử dụng AI chatbot, gặp lỗi:
```
Failed to load resource: the server responded with a status of 500 (Internal Server Error)
api/chat/ask:1
```

**Backend logs:**
```
Received HTTP response headers after 9571ms - 503
Received HTTP response headers after 1047ms - 503
```

## ✅ Giải pháp đã áp dụng

### 1. Đổi Gemini Model

**Trước đây (LỖI):**
```
gemini-2.0-flash-exp  // Experimental - HTTP 503
gemini-1.5-flash      // HTTP 404 (không tồn tại)
gemini-1.5-flash-latest // HTTP 404 (không tồn tại)
```

**Sau khi fix:**
```
gemini-pro            // Stable model - hoạt động ổn định, FREE
```

### 2. Thêm Error Handling

**RagService.cs** giờ xử lý các lỗi:
```csharp
// 🔧 Handle model not found (404)
if ((int)response.StatusCode == 404)
{
    throw new Exception("❌ Gemini model not found...");
}

// 🔧 Handle rate limiting (429)
if ((int)response.StatusCode == 429)
{
    throw new Exception("⏱️ Gemini API rate limit exceeded...");
}

// 🔧 Handle service unavailable (503)
if ((int)response.StatusCode == 503)
{
    throw new Exception("🔧 Gemini API is temporarily unavailable...");
}
```

## 🧪 Cách test

1. **Mở chatbot** (góc dưới bên phải)
2. **Gửi câu hỏi test:**
   - "Hệ thống quản lý sinh viên là gì?"
   - "Làm thế nào để thêm sinh viên mới?"
   - "Role-based authentication hoạt động như thế nào?"

3. **Kết quả mong đợi:**
   - ✅ Chatbot trả lời trong 2-5 giây
   - ✅ Hiển thị typing animation
   - ✅ Có 3 câu hỏi follow-up

## 🔑 API Key hiện tại

**appsettings.Development.json:**
```json
{
  "AI": {
    "Provider": "Gemini"
  },
  "Gemini": {
    "ApiKey": "AIzaSyDvx269hBCqAyNXcl69HvxQtB8WJWajpbc"
  }
}
```

## 🚨 Nếu vẫn gặp lỗi

### Lỗi 503 - Service Unavailable
**Nguyên nhân:** Gemini API đang bảo trì hoặc overload

**Giải pháp:**
1. Đợi 5-10 phút rồi thử lại
2. Kiểm tra status: https://status.google.com
3. Tạo API key mới tại: https://aistudio.google.com/app/apikey

### Lỗi 429 - Rate Limit
**Nguyên nhân:** Vượt quá 15 requests/minute (Free tier)

**Giải pháp:**
1. Đợi 1 phút
2. Tránh spam câu hỏi
3. Sử dụng cache (câu hỏi giống nhau trả lời instant)

### Lỗi 400 - Bad Request
**Nguyên nhân:** API key không hợp lệ

**Giải pháp:**
1. Kiểm tra API key trong appsettings.json
2. Tạo key mới: https://aistudio.google.com/app/apikey
3. Copy key vào `appsettings.Development.json`

## 📊 Gemini Models So Sánh

| Model | Tốc độ | Độ ổn định | Giá | Rate Limit | Status |
|-------|--------|-----------|-----|------------|--------|
| **gemini-pro** ✅ | Trung bình | Cao | FREE | 60 RPM | Working |
| gemini-2.0-flash-exp ❌ | Rất nhanh | Thấp (503) | FREE | 15 RPM | Unavailable |
| gemini-1.5-flash ❌ | - | - | - | - | Not Found (404) |
| gemini-1.5-flash-latest ❌ | - | - | - | - | Not Found (404) |
| gemini-1.5-pro | Chậm | Cao | FREE | 2 RPM | Available |

## 🎯 Tính năng hiện tại

✅ **Hoạt động:**
- Response cache (instant cho câu hỏi lặp lại)
- Typing animation
- Follow-up questions (3 câu)
- Vietnamese support
- Role-based context (Admin, Teacher, Student)
- Sample documents (không cần Pinecone)

⏳ **Chưa có:**
- Vector database (Pinecone) - Optional
- Full codebase search
- OpenAI integration

## 📝 Files đã sửa

1. **Services/RagService.cs**
   - Line ~317: `gemini-2.0-flash-exp` → `gemini-pro`
   - Line ~552: `gemini-2.0-flash-exp` → `gemini-pro`
   - Added 404 error handling
   - Added 503 error handling

---

**Cập nhật:** 2025-10-24  
**Status:** ✅ Fixed - Gemini Pro hoạt động ổn định
**Model:** `gemini-pro` (60 requests/minute)
