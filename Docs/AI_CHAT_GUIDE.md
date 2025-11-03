# 🤖 AI RAG Chat - Hướng Dẫn Sử Dụng

## ✅ Đã Hoàn Thành

### 1. **Backend API** (Không thay đổi - Đang hoạt động)
- ✅ ChatController với endpoint `/api/chat/ask`
- ✅ RagService tích hợp Google Gemini API
- ✅ API Key: `AIzaSyDvx269hBCqAyNXcl69HvxQtB8WJWajpbc`
- ✅ Model: `gemini-2.0-flash-exp` (free tier mới nhất)

### 2. **Frontend - Service Layer**
- ✅ `ai-rag-chat.service.ts` (199 dòng)
  - Xử lý cả PascalCase và camelCase
  - Validation client-side (3-1000 ký tự)
  - Cache localStorage (50 tin nhắn cuối)
  - Error handling hoàn chỉnh
  - Reactive state với RxJS BehaviorSubject

### 3. **Frontend - Component Layer**
- ✅ `ai-rag-chat.component.ts` (157 dòng)
  - Typing animation (15ms/ký tự)
  - Sample questions
  - Follow-up questions
  - Copy code functionality
  - Minimize/Maximize/Close controls
- ✅ `ai-rag-chat.component.html` (90 dòng)
  - Welcome message
  - Messages container
  - Sources với toggle
  - Input textarea
  - Floating button khi đóng
- ✅ `ai-rag-chat.component.scss` (450+ dòng)
  - Glassmorphism design
  - Smooth animations
  - Responsive (mobile-first)
  - Code syntax highlighting

### 4. **App Integration**
- ✅ Import trong `app.component.ts`
- ✅ Thêm vào `app.component.html`
- ✅ Chỉ hiện khi đã login: `*ngIf="authService.isLoggedIn"`

---

## 🚀 Cách Sử Dụng

### Khởi Động Hệ Thống

**Terminal 1 - Backend:**
```powershell
cd c:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
dotnet run
```
Đợi: `Now listening on: http://localhost:5298`

**Terminal 2 - Frontend:**
```powershell
cd ClientApp
npm start
```
Đợi: `** Angular Live Development Server is listening on localhost:4200`

### Đăng Nhập

1. Mở: http://localhost:4200
2. Login với:
   - **Username:** `admin`
   - **Password:** `admin123`

### Mở AI Chat

Sau khi login, bạn sẽ thấy:
- 🤖 **Nút "AI Chat"** ở góc phải-dưới màn hình
- Click vào để mở cửa sổ chat

### Sử Dụng Chat

#### Cách 1: Dùng câu hỏi mẫu
1. Nhìn phần "🎯 Câu hỏi mẫu"
2. Click vào một câu hỏi
3. AI sẽ trả lời tự động với typing animation

#### Cách 2: Tự viết câu hỏi
1. Gõ câu hỏi vào ô input (tối thiểu 3 ký tự)
2. Nhấn **Enter** hoặc nút **➤**
3. Xem AI trả lời với hiệu ứng typing

#### Các tính năng:

**📄 Code Sources:**
- Click "📄 X code sources" để xem code liên quan
- Mỗi source có nút "📋 Copy" để copy code

**💡 Follow-up Questions:**
- AI gợi ý câu hỏi tiếp theo
- Click vào để hỏi ngay

**🗑️ Clear Chat:**
- Click icon 🗑️ ở header
- Xác nhận để xóa toàn bộ lịch sử

**🔽 Minimize:**
- Click icon 🔽 để thu nhỏ
- Click lại để mở rộng

**✖️ Close:**
- Click icon ✖️ để đóng hoàn toàn
- Click nút floating "🤖 AI Chat" để mở lại

---

## 🎯 Các Câu Hỏi Thử Nghiệm

### Cơ Bản
```
- Test chatbot
- Hello AI
- Giải thích hệ thống Student Management
```

### Về Models
```
- Grade Model có những thuộc tính gì?
- Student Model được định nghĩa như thế nào?
- Cấu trúc bảng Classes trong database
```

### Về Controllers
```
- Làm sao StudentController validate điểm số?
- GradesController xử lý phân quyền như thế nào?
- Cách AuthorizeRole attribute hoạt động
```

### Về Frontend
```
- Cách Angular gọi API tạo sinh viên mới?
- Component login xử lý authentication thế nào?
- Routing trong Angular được cấu hình ra sao?
```

### Advanced
```
- Explain authentication flow trong hệ thống
- So sánh Session-based vs JWT authentication
- Best practices cho việc validate input
```

---

## 🐛 Troubleshooting

### Issue: Không thấy nút AI Chat
**Nguyên nhân:** Chưa login hoặc Angular chưa compile xong

**Giải pháp:**
1. Kiểm tra đã login chưa (xem góc trên có tên user không)
2. Check console F12 xem có lỗi không
3. Đợi Angular dev server compile xong (xem terminal)

### Issue: Gửi câu hỏi bị lỗi 400
**Nguyên nhân:** Câu hỏi < 3 ký tự hoặc > 1000 ký tự

**Giải pháp:**
- Viết câu hỏi dài hơn 3 ký tự
- Rút ngắn câu hỏi nếu quá dài

### Issue: Backend không kết nối
**Nguyên nhân:** Backend chưa chạy hoặc chạy sai port

**Giải pháp:**
```powershell
# Kiểm tra backend
curl http://localhost:5298/api/chat/health

# Nếu lỗi, chạy lại backend
dotnet run
```

### Issue: AI trả lời chậm
**Nguyên nhân:** Gemini API free tier có rate limit

**Giải pháp:**
- Đợi vài giây rồi thử lại
- Câu hỏi phức tạp sẽ mất thời gian xử lý hơn

### Issue: Typing animation bị lag
**Nguyên nhân:** Browser đang xử lý nhiều tác vụ

**Giải pháp:**
- Đóng các tab không dùng
- Clear browser cache
- Thử browser khác (Chrome, Edge)

---

## 📊 So Sánh Phiên Bản Cũ vs Mới

| Feature | Old Version ❌ | New Version ✅ |
|---------|---------------|---------------|
| UI Design | Basic chat box | Glassmorphism premium |
| Animations | None | Typing, fade, slide |
| Error Handling | Simple alert | Comprehensive handling |
| Code Sources | No display | Toggle view + copy |
| Follow-up Questions | No | Yes with suggestions |
| Sample Questions | No | Yes with quick start |
| Mobile Responsive | Partial | Fully responsive |
| PascalCase Support | Buggy | Complete dual-format |
| Validation | Backend only | Client + Server |
| Caching | None | LocalStorage (50 msgs) |
| State Management | Props | RxJS BehaviorSubject |
| Performance | Slow | Optimized with debounce |

---

## 🎨 Design Features

### Glassmorphism
- Frosted glass effect với `backdrop-filter: blur(20px)`
- Semi-transparent backgrounds `rgba(255, 255, 255, 0.15)`
- Subtle borders và shadows
- Gradient overlays

### Animations
- **fadeInUp:** Messages appear smoothly
- **slideIn:** Chat window enters from right
- **blink:** Typing cursor effect
- **float:** AI icon subtle movement

### Responsive Breakpoints
- **Desktop (>768px):** Floating window 450px wide
- **Tablet (≤768px):** Full width with margins
- **Mobile (≤480px):** Fullscreen overlay

---

## 🔧 Configuration

### Thay đổi Gemini API Key
File: `appsettings.Development.json`
```json
{
  "Gemini": {
    "ApiKey": "YOUR_NEW_API_KEY_HERE",
    "Model": "gemini-2.0-flash-exp"
  }
}
```

### Thay đổi số tin nhắn cache
File: `ai-rag-chat.service.ts`
```typescript
private saveChatHistory(messages: ChatMessage[]): void {
  const maxHistory = 100; // Tăng từ 50 lên 100
  const recentMessages = messages.slice(-maxHistory);
  localStorage.setItem('ai_chat_history', JSON.stringify(recentMessages));
}
```

### Thay đổi typing speed
File: `ai-rag-chat.component.ts`
```typescript
this.typingTimer = setInterval(() => {
  // ...
}, 10); // Giảm từ 15ms xuống 10ms = nhanh hơn
```

---

## 📝 Notes

### Về localStorage
- Key: `ai_chat_history`
- Max: 50 tin nhắn cuối
- Auto load khi refresh page
- Clear khi click "Clear Chat"

### Về Validation
- **Client-side:** 3-1000 ký tự
- **Server-side:** Tương tự + sanitization
- Lỗi hiện alert user-friendly

### Về Error Handling
- Network errors → Retry suggestion
- Validation errors → Input guidance
- Server errors → Error message in chat
- Timeout → "Please try again"

---

## 🚀 Next Steps (Optional)

### Enhancement Ideas
1. **Markdown Support:** Parse AI responses với markdown
2. **Code Highlighting:** Thêm highlight.js cho syntax
3. **Voice Input:** Speech-to-text
4. **Export Chat:** Download lịch sử as PDF
5. **Multi-language:** Tiếng Anh/Việt switch
6. **Themes:** Light/Dark mode
7. **Keyboard Shortcuts:** Ctrl+K to open
8. **Search History:** Find in past conversations

### Performance Optimizations
1. Virtual scrolling cho messages list dài
2. Lazy load code sources
3. Debounce typing indicators
4. Service worker caching

---

## ✅ Checklist Kiểm Tra

- [ ] Backend running on port 5298
- [ ] Frontend running on port 4200
- [ ] Login thành công
- [ ] Thấy nút "🤖 AI Chat"
- [ ] Click mở chat window
- [ ] Welcome message hiển thị
- [ ] Sample questions có 4 câu
- [ ] Click sample question → gửi tự động
- [ ] Typing animation hoạt động
- [ ] Code sources toggle được
- [ ] Copy code button hoạt động
- [ ] Follow-up questions xuất hiện
- [ ] Minimize/Maximize works
- [ ] Close → nút floating hiện
- [ ] Open lại từ floating button
- [ ] Clear chat → confirm → xóa
- [ ] Refresh page → history load lại
- [ ] Mobile view responsive đúng

---

## 🎉 Kết Luận

Bạn đã có một **AI RAG Chat phiên bản Pro** hoàn chỉnh với:
- ✅ Google Gemini API integration
- ✅ Glassmorphism UI design
- ✅ Typing animation như ChatGPT
- ✅ Code sources với copy button
- ✅ Follow-up questions tự động
- ✅ Full responsive mobile
- ✅ Error handling toàn diện
- ✅ LocalStorage caching

Enjoy chatting với AI! 🚀🤖✨
