# 🔧 Fix: API Proxy Error 500

## ❌ Lỗi gặp phải

```
GET http://localhost:4200/api/chat/health 500 (Internal Server Error)
HttpErrorResponse: Http failure response for http://localhost:4200/api/chat/health: 500
```

## 🔍 Nguyên nhân

Angular dev server chưa được cấu hình proxy để forward API requests từ port **4200** (Frontend) sang port **5298** (Backend).

Khi Angular gọi `/api/chat/health`, nó tìm trên `localhost:4200` thay vì forward sang backend `localhost:5298`.

## ✅ Giải pháp

### Bước 1: Cấu hình Proxy

File `ClientApp/proxy.conf.json` đã có sẵn:

```json
{
  "/api": {
    "target": "http://localhost:5298",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```

### Bước 2: Update package.json

**Đã sửa** trong `ClientApp/package.json`:

```json
{
  "scripts": {
    "start": "ng serve --proxy-config proxy.conf.json"
  }
}
```

**Trước đây** (sai):
```json
"start": "ng serve"
```

### Bước 3: Restart Angular với Proxy

```bash
cd ClientApp
npm start
```

Bây giờ Angular sẽ tự động forward tất cả requests `/api/*` sang `http://localhost:5298/api/*`.

---

## 🚀 Quick Start

### Option 1: Dùng Script (Khuyến nghị)

```cmd
Scripts\run.bat
```

Script này tự động:
1. Chạy Backend (ASP.NET Core) trên port 5298
2. Chạy Frontend (Angular) trên port 4200 với proxy config
3. Mở 2 terminal windows riêng biệt

### Option 2: Manual

**Terminal 1 - Backend:**
```bash
dotnet run
# → http://localhost:5298
```

**Terminal 2 - Frontend:**
```bash
cd ClientApp
npm start
# → http://localhost:4200
```

---

## 🧪 Test API

### Test Backend trực tiếp:
```bash
curl http://localhost:5298/api/chat/health
```

**Expected response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-10-24T10:30:00Z",
  "geminiApiConfigured": true
}
```

### Test qua Angular Proxy:
```bash
curl http://localhost:4200/api/chat/health
```

Kết quả **phải giống nhau** (Angular forward request sang Backend).

---

## 📊 Port Configuration

| Service  | Port | URL                      | Purpose               |
|----------|------|--------------------------|-----------------------|
| Backend  | 5298 | http://localhost:5298    | ASP.NET Core API      |
| Frontend | 4200 | http://localhost:4200    | Angular Dev Server    |
| Proxy    | 4200 | /api/* → :5298/api/*     | Forward API requests  |

---

## 🐛 Troubleshooting

### Error: Port 4200 already in use

```bash
# Kill old node processes
Stop-Process -Name "node" -Force
npm start
```

### Error: ECONNREFUSED ::1:5298

Backend chưa chạy. Start backend:
```bash
dotnet run
```

### Error: Proxy not working

1. Check `proxy.conf.json` exists in `ClientApp/`
2. Verify `package.json` has `--proxy-config` flag
3. Restart Angular dev server

### Error: Backend crashes immediately

Check `appsettings.json` có đúng connection string và Gemini API key chưa.

---

## ✨ Kết quả

Sau khi fix:
- ✅ Angular gọi `/api/chat/health` → Proxy forward → Backend trả về `200 OK`
- ✅ AI Chatbot hoạt động bình thường
- ✅ Follow-up questions hiển thị sau typing animation
- ✅ Typing animation mượt mà (20ms/char = 50 chars/sec)
- ✅ Syntax highlighting với highlight.js
- ✅ Response caching (1 hour TTL)

---

## 📚 Related Documentation

- `Docs/RAG_SETUP_GUIDE.md` - Setup AI chatbot
- `Docs/GEMINI_SETUP.md` - Configure Gemini API
- `Scripts/README.md` - Script usage guide
- `ClientApp/proxy.conf.json` - Proxy configuration

---

**Fixed on**: October 24, 2025  
**Issue**: API 500 error due to missing proxy config  
**Solution**: Added `--proxy-config proxy.conf.json` to npm start script
