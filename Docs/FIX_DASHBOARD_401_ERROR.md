# Fix Dashboard 401 Unauthorized Error - Teacher & Student

## 🔍 Vấn đề phát hiện

Khi login bằng tài khoản **Teacher** hoặc **Student**, dashboard không hiển thị dữ liệu và báo lỗi:
```
GET http://localhost:4200/api/dashboard/teacher-stats 401 (Unauthorized)
```

**Admin dashboard hoạt động bình thường** ✅

---

## 🔬 Nguyên nhân

### Luồng Authentication bị mâu thuẫn:

1. **Frontend Angular** sử dụng **JWT Token**:
   - Login → `/api/auth/login` → Nhận JWT token
   - Lưu token vào `localStorage`
   - JWT Interceptor tự động thêm `Authorization: Bearer <token>` vào headers

2. **Backend Dashboard API** sử dụng **Session Cookies**:
   - Endpoints `teacher-stats` và `student-stats` đọc username từ `HttpContext.Session`
   - Không có session → return `Unauthorized`

3. **Admin dashboard hoạt động** vì:
   - Endpoint `admin-stats` **KHÔNG CẦN** session
   - Không filter theo user cụ thể

### Lỗi trong code:

#### ❌ Backend trước khi sửa (DashboardController.cs):
```csharp
[HttpGet("teacher-stats")]
public async Task<IActionResult> GetTeacherStats()
{
    // Chỉ đọc từ Session
    var username = HttpContext.Session.GetString("UserId") 
                ?? HttpContext.Session.GetString("Username");
    
    if (string.IsNullOrEmpty(username))
    {
        return Unauthorized(new { message = "Không tìm thấy thông tin giảng viên" });
    }
    // ...
}
```

#### ❌ JWT không có Username claim (JwtService.cs):
```csharp
public string GenerateToken(string userId, string role, string entityId)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Role, role),
        new Claim("EntityId", entityId),  // Không có Username!
        // ...
    };
}
```

---

## ✅ Giải pháp thực hiện

### 1. Thêm Username claim vào JWT Token

**File**: `Services/JwtService.cs`

#### Interface:
```csharp
public interface IJwtService
{
    // Thêm parameter `username`
    string GenerateToken(string userId, string role, string fullName, string username);
    ClaimsPrincipal? ValidateToken(string token);
}
```

#### Implementation:
```csharp
public string GenerateToken(string userId, string role, string fullName, string username)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Role, role),
        new Claim(ClaimTypes.Name, fullName),
        new Claim("Username", username),  // ✅ Thêm Username claim
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
    // ...
}
```

### 2. Cập nhật AuthController để truyền Username

**File**: `Controllers/API/AuthController.cs`

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var result = await _authService.AuthenticateAsync(request.Username, request.Password);
    
    if (!result.Success)
    {
        return Ok(new { success = false, message = "..." });
    }

    // ✅ Truyền thêm request.Username vào GenerateToken
    var token = _jwtService.GenerateToken(
        result.EntityId!,
        result.Role!,
        result.FullName!,
        request.Username  // ← Username được thêm vào JWT
    );

    return Ok(new
    {
        success = true,
        token = token,
        user = new { ... }
    });
}
```

### 3. Sửa DashboardController để đọc từ JWT Claims

**File**: `Controllers/API/DashboardController.cs`

#### Teacher Stats Endpoint:
```csharp
[HttpGet("teacher-stats")]
public async Task<IActionResult> GetTeacherStats()
{
    try
    {
        // ✅ Đọc từ JWT Claims TRƯỚC, fallback sang Session
        var username = User.FindFirst("Username")?.Value 
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                     ?? HttpContext.Session.GetString("UserId") 
                     ?? HttpContext.Session.GetString("Username");
        
        Console.WriteLine($"[DashboardController] JWT Username: {User.FindFirst("Username")?.Value}");
        Console.WriteLine($"[DashboardController] Resolved username: {username}");
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "Không tìm thấy thông tin giảng viên" });
        }

        // Lookup teacher by username
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == username);
        if (teacher == null)
        {
            return NotFound(new { message = "Không tìm thấy thông tin giảng viên" });
        }

        var teacherId = teacher.TeacherId;
        
        // Query teacher's classes and courses...
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Lỗi khi tải dữ liệu dashboard giảng viên", error = ex.Message });
    }
}
```

#### Student Stats Endpoint:
```csharp
[HttpGet("student-stats")]
public async Task<IActionResult> GetStudentStats()
{
    try
    {
        // ✅ Đọc từ JWT Claims TRƯỚC, fallback sang Session
        var username = User.FindFirst("Username")?.Value 
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                     ?? HttpContext.Session.GetString("UserId") 
                     ?? HttpContext.Session.GetString("Username");
        
        Console.WriteLine($"[DashboardController] Student stats request");
        Console.WriteLine($"[DashboardController] Resolved username: {username}");
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { message = "Không tìm thấy thông tin sinh viên" });
        }

        // Lookup student by username
        var studentRecord = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
        if (studentRecord == null)
        {
            return NotFound(new { message = "Không tìm thấy thông tin sinh viên" });
        }

        var studentId = studentRecord.StudentId;
        
        // Query student's grades...
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Lỗi khi tải dữ liệu dashboard sinh viên", error = ex.Message });
    }
}
```

---

## 🧪 Cách kiểm tra

### 1. Kiểm tra JWT Token có Username claim:

Sau khi login, copy JWT token từ `localStorage` trong DevTools Console:
```javascript
localStorage.getItem('token')
```

Paste token vào https://jwt.io/ để decode. Xem payload phải có:
```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "...",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Teacher",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "Nguyễn Văn Anh",
  "Username": "nvanh",  // ← Phải có claim này
  "jti": "..."
}
```

### 2. Test Teacher Dashboard:

1. Mở trình duyệt mới (hoặc Incognito)
2. Truy cập: http://localhost:4200
3. Click **"Đăng nhập nhanh - Giáo viên"**
   - Username: `nvanh`
   - Password: `teacher123`
4. Kiểm tra:
   - ✅ Redirect đến `/dashboard-teacher`
   - ✅ Hiển thị danh sách lớp chủ nhiệm
   - ✅ Hiển thị danh sách môn học giảng dạy
   - ✅ Không có lỗi 401 trong Console

### 3. Test Student Dashboard:

1. Logout và login lại với **"Đăng nhập nhanh - Sinh viên"**
   - Username: `nvan`
   - Password: `student123`
2. Kiểm tra:
   - ✅ Redirect đến `/dashboard-student`
   - ✅ Hiển thị thông tin lớp
   - ✅ Hiển thị bảng điểm
   - ✅ Không có lỗi 401 trong Console

### 4. Kiểm tra Backend Console Logs:

Backend sẽ in ra logs:
```
[DashboardController] Teacher stats request
[DashboardController] JWT Username: nvanh
[DashboardController] JWT UserId: ...
[DashboardController] Resolved username: nvanh
[DashboardController] ✅ Found teacher: Nguyễn Văn Anh (ID: GV001)
```

---

## 📝 Files đã thay đổi

| File | Nội dung thay đổi |
|------|-------------------|
| `Services/JwtService.cs` | ✅ Thêm `username` parameter vào `GenerateToken()` và thêm claim `Username` |
| `Controllers/API/AuthController.cs` | ✅ Truyền `request.Username` vào `GenerateToken()` |
| `Controllers/API/DashboardController.cs` | ✅ Đọc username từ JWT Claims (`User.FindFirst("Username")`) trước khi dùng Session |

---

## 🎯 Kết quả

- ✅ **Admin dashboard**: Vẫn hoạt động bình thường (không ảnh hưởng)
- ✅ **Teacher dashboard**: Hiển thị dữ liệu đầy đủ, không còn lỗi 401
- ✅ **Student dashboard**: Hiển thị bảng điểm và thông tin lớp, không còn lỗi 401
- ✅ **Backward compatible**: Vẫn hỗ trợ Session-based authentication (fallback)
- ✅ **JWT-first approach**: Ưu tiên đọc từ JWT Claims, phù hợp với Angular SPA

---

## 🔐 Best Practices áp dụng

1. **Consistent Authentication**: Frontend và Backend cùng sử dụng JWT
2. **Claim-based Authorization**: Lưu thông tin user cần thiết trong JWT claims
3. **Fallback Strategy**: Hỗ trợ cả JWT và Session để đảm bảo compatibility
4. **Debug Logging**: Console logs chi tiết để dễ troubleshoot
5. **Username in JWT**: Lưu Username claim để tránh phải query database nhiều lần

---

## 📚 Tham khảo

- JWT Claims: https://datatracker.ietf.org/doc/html/rfc7519#section-4
- ASP.NET Core Claims: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims
- Angular HTTP Interceptors: https://angular.io/guide/http-intercept-requests-and-responses

---

**Ngày sửa**: 2025-01-11  
**Người thực hiện**: GitHub Copilot AI Assistant  
**Trạng thái**: ✅ Đã hoàn thành và test thành công
