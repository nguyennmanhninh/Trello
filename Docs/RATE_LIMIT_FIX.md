# ⚠️ Gemini API Rate Limit - Hướng Dẫn Khắc Phục

## 🔴 Vấn Đề

Bạn đang gặp lỗi:
```
POST http://localhost:4200/api/chat/ask 500 (Internal Server Error)
```

Backend logs cho thấy:
```
429 - Gemini API rate limit exceeded
⏱️ Free tier allows 15 requests per minute
```

## 🎯 Nguyên Nhân

**Gemini API Free Tier Limits:**
- ✅ **15 requests per minute** (RPM)
- ✅ **1,500 requests per day** (RPD)
- ✅ **1 million tokens per day**

Bạn đã click sample questions nhiều lần → vượt quá 15 requests/phút → API trả về 429 → Backend trả về 500.

## ✅ Giải Pháp

### Cách 1: Đợi 1 Phút (Khuyến Nghị)

```
🕐 Đợi 60 giây để rate limit reset
```

Sau đó thử lại:
1. Refresh browser (F5)
2. Login: admin / admin123
3. Mở AI Chat
4. Hỏi **MỘT** câu duy nhất
5. Đợi AI trả lời xong
6. Hỏi câu tiếp theo

### Cách 2: Xóa Rate Limit Counter (Backend)

Restart backend để reset:

```powershell
# Trong terminal backend (Ctrl+C để stop)
Ctrl+C

# Chạy lại
dotnet run
```

### Cách 3: Test Với Câu Hỏi Đơn Giản

Thử từng bước:

1. **Xóa chat history cũ:**
   - Click nút 🗑️ trong AI Chat header
   - Hoặc clear localStorage:
     ```javascript
     // F12 Console
     localStorage.removeItem('ai-rag-chat');
     ```

2. **Gửi 1 câu hỏi ngắn:**
   ```
   hello
   ```

3. **Đợi response (khoảng 2-3 giây)**

4. **Nếu thành công, thử câu phức tạp:**
   ```
   Grade Model có những thuộc tính gì?
   ```

## 📊 Rate Limit Tracking

**Số requests đã gửi (từ logs):**
- Request 1: "hello" → 200 OK ✅
- Request 2-10: "Cách Angular..." → 429 Rate Limited ❌
- Request 11-15: "Grade Model..." → 429 Rate Limited ❌
- Request 16+: "AuthorizeRole..." → 429 Rate Limited ❌

**Tổng:** ~20+ requests trong < 1 phút → **Vượt quá giới hạn 15 RPM**

## 🛠️ Đã Sửa Trong Code

### Service (ai-rag-chat.service.ts)

```typescript
catchError((err: HttpErrorResponse) => {
  let msg = 'Lỗi kết nối AI';
  
  if (err.status === 500) {
    msg = '🔥 Gemini API đang bị rate limit (429)! ' +
          'Đợi 1-2 phút rồi thử lại nhé. ' +
          'Free tier giới hạn 15 requests/phút';
  }
  
  // Show error in chat
  const errMsg: ChatMessage = {
    id: this.genId(),
    role: 'assistant',
    content: msg,
    timestamp: new Date()
  };
  this.addMessage(errMsg);
  
  return throwError(() => new Error(msg));
})
```

Giờ user sẽ thấy message rõ ràng trong chat thay vì chỉ alert lỗi.

## 🎯 Cách Tránh Rate Limit

### 1. **Không Click Sample Questions Liên Tục**
❌ **Sai:**
- Click "AuthorizeRole attribute..."
- Click "Grade Model..."
- Click "Cách Angular..."
(→ 3 requests trong 1 giây)

✅ **Đúng:**
- Click 1 sample question
- Đợi response
- Đọc câu trả lời
- Đợi thêm 5 giây
- Hỏi câu tiếp theo

### 2. **Debounce Typing**
Service đã có `retry({ count: 2, delay: 1000 })` nhưng chưa có debounce.

Nếu muốn thêm:
```typescript
// Trong component
private searchSubject = new Subject<string>();

ngOnInit() {
  this.searchSubject.pipe(
    debounceTime(500),
    distinctUntilChanged()
  ).subscribe(question => {
    this.aiService.askQuestion(question).subscribe();
  });
}

sendQuestion() {
  this.searchSubject.next(this.currentQuestion);
}
```

### 3. **Cache Responses**
Backend RagService có caching:
```csharp
// Cache hit → không gọi Gemini API
if (cachedResponse != null) {
    return cachedResponse; // Instant!
}
```

Nên hỏi câu giống nhau sẽ nhanh hơn.

## 📝 Test Plan (Sau Khi Đợi 1 Phút)

### Phase 1: Basic Test
1. ✅ Đợi 60 giây
2. ✅ Refresh browser
3. ✅ Login
4. ✅ Gửi: "hello"
5. ✅ Verify response
6. ⏸️ Đợi 10 giây

### Phase 2: Complex Test
7. ✅ Gửi: "Grade Model có những thuộc tính gì?"
8. ✅ Verify response với code sources
9. ⏸️ Đợi 10 giây

### Phase 3: Follow-up Test
10. ✅ Click follow-up question (nếu có)
11. ✅ Verify response
12. ⏸️ Đợi 10 giây

### Phase 4: Sample Questions Test
13. ✅ Click 1 sample question
14. ✅ Verify response
15. ✅ **STOP** - Đã dùng 4/15 requests

## 🔍 Debug Commands

### Check Backend Logs:
```powershell
# Xem logs realtime
Get-Content -Path "backend-logs.txt" -Wait
```

### Check Rate Limit Status:
```powershell
# Test health endpoint
Invoke-RestMethod http://localhost:5298/api/chat/health
```

**Nếu thấy:**
```json
{
  "status": "healthy",
  "configured": true,
  "model": "gemini-2.0-flash-exp"
}
```
→ Backend OK, chỉ cần đợi rate limit reset.

## ⏱️ Timeline

**Hiện tại:** 
- Rate limit hit
- Phải đợi tối thiểu 1 phút

**Sau 1 phút:**
- Rate limit reset
- Có thể gửi 15 requests mới

**Best practice:**
- Gửi tối đa 10 requests/phút
- Đợi ít nhất 6 giây giữa mỗi request
- Tránh spam sample questions

## 🎉 Expected Behavior (Sau Fix)

Khi gửi câu hỏi và gặp rate limit:

**Old:**
```
❌ Lỗi: Lỗi server AI
(không rõ nguyên nhân)
```

**New:**
```
🔥 Gemini API đang bị rate limit (429)! 
Đợi 1-2 phút rồi thử lại nhé. 
Free tier giới hạn 15 requests/phút
```

Message này sẽ hiện trong chat window, không phải alert popup.

## 📞 Next Steps

1. **Đợi 60 giây** ⏰
2. **Refresh browser** (F5)
3. **Test lại với 1 câu hỏi**
4. **Nếu OK** → Tiếp tục test (từ từ)
5. **Nếu vẫn lỗi** → Restart backend

---

**Updated:** October 27, 2025
**Status:** Code fixed, waiting for rate limit reset ⏳
