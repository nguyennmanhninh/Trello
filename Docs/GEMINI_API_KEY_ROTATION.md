# 🔄 Gemini API Key Rotation Guide

## Tổng quan

Hệ thống giờ hỗ trợ **nhiều Gemini API keys** với tính năng **tự động rotation** khi gặp rate limit (429 error). Điều này cho phép bạn sử dụng tối đa **15 requests/phút × số lượng keys**.

## Cách hoạt động

### 1. Key Rotation Logic

```
Key 1 (15 req) → Rate limit → Rotate → Key 2 (15 req) → Rotate → Key 3 (15 req)
                    ↓                      ↓                      ↓
                 429 error              429 error              429 error
                    ↓                      ↓                      ↓
                Try Key 2              Try Key 3          All keys exhausted
```

### 2. Lợi ích

- **Key 1:** 15 requests/phút
- **Key 2:** 15 requests/phút
- **Key 3:** 15 requests/phút
- **Tổng:** **45 requests/phút** (với 3 keys)

### 3. Tự động retry

Khi key hiện tại bị rate limit:
1. ✅ Tự động chuyển sang key tiếp theo
2. ✅ Thử lại request
3. ✅ Log console: `🔄 Rotated to API key #2/3`
4. ❌ Nếu hết keys → Báo lỗi: "All X keys exhausted"

## Cách cấu hình

### Bước 1: Lấy thêm Gemini API keys

1. Truy cập: https://aistudio.google.com/app/apikey
2. Tạo thêm 2-3 API keys miễn phí (có thể dùng email khác hoặc cùng email)
3. Copy các keys

### Bước 2: Cập nhật `appsettings.Development.json`

**Cấu hình cũ (1 key):**
```json
"Gemini": {
  "ApiKey": "AIzaSyDvx269hBCqAyNXcl69HvxQtB8WJWajpbc"
}
```

**Cấu hình mới (nhiều keys):**
```json
"Gemini": {
  "ApiKeys": [
    "AIzaSyDvx269hBCqAyNXcl69HvxQtB8WJWajpbc",
    "AIzaSyC_YOUR_SECOND_KEY_HERE_xxxxxxxxxxxxx",
    "AIzaSyC_YOUR_THIRD_KEY_HERE_xxxxxxxxxxxxxx"
  ]
}
```

**Lưu ý:**
- Dùng `ApiKeys` (số nhiều) thay vì `ApiKey`
- Là một **array** `[]` chứa nhiều keys
- Mỗi key trên 1 dòng cho dễ đọc

### Bước 3: Cập nhật `appsettings.Production.json` (cho server)

```json
{
  "Gemini": {
    "ApiKeys": [
      "YOUR_PROD_KEY_1",
      "YOUR_PROD_KEY_2",
      "YOUR_PROD_KEY_3"
    ]
  }
}
```

### Bước 4: Restart ứng dụng

```powershell
# Stop backend
Ctrl+C trong terminal đang chạy dotnet

# Start lại
dotnet run
```

## Testing

### Test rotation thủ công

1. Hỏi AI 15 câu liên tục trong 1 phút
2. Câu thứ 16 sẽ trigger rotation → Console log:
   ```
   🔄 Rotated to API key #2/3
   ⏱️ Rate limit on key #1, trying next key...
   ```
3. Tiếp tục hỏi 15 câu nữa → Rotation sang key #3
4. Sau 60 giây → Key #1 reset → Lại có thể dùng

### Kiểm tra logs

Backend console sẽ hiển thị:
```
[12:34:56] 🔄 Rotated to API key #2/3
[12:35:10] 🔄 Rotated to API key #3/3
[12:36:05] 🔄 Rotated to API key #1/3  (sau 60s)
```

## Xử lý lỗi

### Lỗi 1: "All X keys exhausted"

**Nguyên nhân:** Hết rate limit trên tất cả keys

**Giải pháp:**
- Đợi 60 giây cho keys reset
- Hoặc thêm key thứ 4, 5...

### Lỗi 2: "No API keys configured"

**Nguyên nhân:** `appsettings.json` chưa có `ApiKeys`

**Giải pháp:**
```json
"Gemini": {
  "ApiKeys": ["YOUR_KEY_HERE"]
}
```

### Lỗi 3: Key không hợp lệ

**Nguyên nhân:** Key sai hoặc bị disable

**Giải pháp:**
- Kiểm tra key tại https://aistudio.google.com/app/apikey
- Xóa key lỗi khỏi array
- Thêm key mới

## Code changes

### Backend: RagService.cs

**Trước:**
```csharp
private readonly string _geminiApiKey;

_geminiApiKey = configuration["Gemini:ApiKey"];
```

**Sau:**
```csharp
private readonly List<string> _geminiApiKeys;
private int _currentKeyIndex = 0;

// Load multiple keys
_geminiApiKeys = configuration.GetSection("Gemini:ApiKeys").Get<List<string>>();

// Get current key
private string GetCurrentGeminiApiKey() { ... }

// Rotate to next
private void RotateToNextApiKey() { ... }
```

### Retry logic với rotation

```csharp
var maxRetries = _geminiApiKeys?.Count ?? 1;

for (int retry = 0; retry < maxRetries; retry++)
{
    try
    {
        var currentKey = GetCurrentGeminiApiKey();
        // ... call API ...
        
        if (statusCode == 429 && retry < maxRetries - 1)
        {
            RotateToNextApiKey();
            await Task.Delay(500);
            continue; // Try next key
        }
    }
    catch { ... }
}
```

## Best Practices

### 1. Số lượng keys

- **Development:** 2-3 keys là đủ
- **Production:** 3-5 keys tùy lưu lượng

### 2. Quản lý keys

- Không commit keys vào Git
- Dùng `.gitignore` cho `appsettings.*.json`
- Dùng Azure Key Vault cho production

### 3. Monitoring

- Log rotation events
- Track usage per key
- Alert khi all keys exhausted

### 4. Rate limit per key

Gemini free tier:
- **15 requests/minute**
- **1,500 requests/day**
- Reset mỗi 60 giây

## Tương thích ngược

Hệ thống vẫn hỗ trợ cấu hình cũ với 1 key:

```json
"Gemini": {
  "ApiKey": "SINGLE_KEY_HERE"
}
```

Code sẽ tự động convert thành array 1 phần tử.

## FAQ

**Q: Có thể mix keys từ nhiều Google accounts không?**  
A: Được! Mỗi key độc lập, không cần cùng account.

**Q: Bao nhiêu keys là tối đa?**  
A: Không giới hạn trong code, nhưng 3-5 keys là hợp lý.

**Q: Key rotation có tốn thời gian không?**  
A: Có 500ms delay giữa các retry để tránh spam API.

**Q: Frontend có cần thay đổi gì không?**  
A: KHÔNG. Frontend không biết về keys, chỉ backend xử lý.

**Q: Có thể dynamic thêm keys khi runtime không?**  
A: Hiện tại không, cần restart app. Có thể implement sau nếu cần.

## Liên kết

- [Gemini API Documentation](https://ai.google.dev/gemini-api/docs)
- [Get API Key](https://aistudio.google.com/app/apikey)
- [Rate Limits Info](https://ai.google.dev/pricing)

---

**Tác giả:** AI Assistant  
**Ngày tạo:** 27/10/2025  
**Version:** 1.0
