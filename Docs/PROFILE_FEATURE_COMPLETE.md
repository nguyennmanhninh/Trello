# Profile Feature - Hoàn Thành Đồng Bộ Backend-Frontend

## ✅ Tổng Quan
Trang profile đã được đồng bộ hoàn toàn giữa backend và frontend, hỗ trợ đầy đủ 3 roles: **Admin**, **Teacher**, và **Student**.

---

## 🎯 Chức Năng Đã Hoàn Thành

### 1. **Backend API** (`Controllers/API/ProfileController.cs`)

#### GET `/api/profile`
- Lấy thông tin profile dựa trên session
- Hỗ trợ 3 roles:
  - **Admin**: Trả về `{userId, username, role, entityId}` từ bảng Users
  - **Teacher**: Trả về Teacher entity với Include Department
  - **Student**: Trả về Student entity với Include Class và Department

#### PUT `/api/profile/student`
- Cập nhật thông tin sinh viên
- Quyền: Admin, Teacher, Student (chỉ cập nhật chính mình)
- Các trường được phép:
  - Student tự cập nhật: `phone`, `address`, `email`
  - Admin/Teacher có thể cập nhật thêm: `fullName`, `dateOfBirth`, `gender`

#### PUT `/api/profile/teacher`
- Cập nhật thông tin giảng viên
- Quyền: Admin, Teacher (chỉ cập nhật chính mình)
- Các trường được phép: `fullName`, `dateOfBirth`, `gender`, `phone`, `address`
- Chỉ Admin mới được phép thay đổi `departmentId`

#### PUT `/api/profile/admin`
- Cập nhật thông tin admin
- Quyền: Chỉ Admin
- Các trường được phép: `username` (với validation trùng lặp)

#### PUT `/api/profile/password`
- Đổi mật khẩu cho tất cả roles
- Validation:
  - Mật khẩu cũ phải đúng
  - Mật khẩu mới tối thiểu 6 ký tự
  - Confirm password phải khớp

---

### 2. **Frontend Service** (`services/profile.service.ts`)

#### Interfaces
```typescript
export interface ProfileResponse {
  role: string;
  data: Student | Teacher | AdminProfile;
}

export interface AdminProfile {
  userId: number;
  username: string;
  role: string;
  entityId: string;
}

export interface UpdateAdminProfileRequest {
  username: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}
```

#### Methods
- `getProfile()`: Lấy profile hiện tại
- `updateStudentProfile(student)`: Cập nhật student
- `updateTeacherProfile(teacher)`: Cập nhật teacher
- `updateAdminProfile(request)`: Cập nhật admin
- `changePassword(request)`: Đổi mật khẩu

---

### 3. **Frontend Component** (`components/profile/profile.component.ts`)

#### State Management
- `profileData`: Lưu dữ liệu profile (Student | Teacher | AdminProfile)
- `editedStudent`: Copy để edit student
- `editedTeacher`: Copy để edit teacher
- `editedAdmin`: Copy để edit admin
- `passwordData`: Form đổi mật khẩu

#### Methods
- `loadProfile()`: Load profile từ API
- `enableEditMode()`: Bật chế độ edit (khác nhau cho từng role)
- `saveProfile()`: Lưu thay đổi (dispatch đúng API cho từng role)
- `validateForm()`: Validate form (khác nhau cho từng role)
- `changePassword()`: Đổi mật khẩu
- Role checking: `isAdmin()`, `isTeacher()`, `isStudent()`

---

### 4. **Frontend Template** (`components/profile/profile.component.html`)

#### Cấu Trúc
1. **Header**: Hiển thị title với role badge
2. **Alert Messages**: Success/Error messages
3. **Profile Display**: Hiển thị thông tin theo role
4. **Edit Forms**: Form chỉnh sửa cho từng role
5. **Change Password Modal**: Modal đổi mật khẩu

#### Admin View
- Hiển thị: Username, Role
- Edit: Username (với validation)
- Actions: Chỉnh sửa, Đổi mật khẩu

#### Teacher View
- Hiển thị: Mã GV, Họ tên, SĐT, Địa chỉ, Khoa
- Edit: Họ tên, SĐT, Địa chỉ
- Actions: Chỉnh sửa, Đổi mật khẩu

#### Student View
- Hiển thị: Mã SV, Họ tên, Email, SĐT, Địa chỉ
- Edit: Email, SĐT, Địa chỉ
- Actions: Chỉnh sửa, Đổi mật khẩu

---

## 🔧 Cấu Hình Session & CORS

### Backend (`Program.cs`)
```csharp
// Session Configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".StudentManagement.Session";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.None; // ✅ Allow cross-origin cookies
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // ✅ Required for session cookies
    });
});
```

### Frontend Interceptor (`interceptors/jwt.interceptor.ts`)
```typescript
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
      withCredentials: true  // ✅ Send session cookies
    });
  } else {
    req = req.clone({
      withCredentials: true  // ✅ Send session cookies even without JWT
    });
  }
  
  return next(req);
};
```

---

## 🚀 Login Flow với Session

### API Login Endpoint (`Controllers/API/AuthController.cs`)
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var result = await _authService.AuthenticateAsync(request.Username, request.Password);
    
    if (!result.Success)
    {
        return Ok(new { success = false, message = "Sai username/password" });
    }

    // ✅ Tạo session cho API calls
    HttpContext.Session.SetString("UserId", request.Username);
    HttpContext.Session.SetString("UserRole", result.Role!);
    HttpContext.Session.SetString("EntityId", result.EntityId!);
    HttpContext.Session.SetString("UserName", result.FullName!);
    await HttpContext.Session.CommitAsync();

    _logger.LogInformation($"[API Login] Session created - UserId: {request.Username}, Role: {result.Role}, EntityId: {result.EntityId}");

    // Generate JWT token
    var token = _jwtService.GenerateToken(...);
    
    return Ok(new { success = true, token, user = {...} });
}
```

---

## ✅ Validation Rules

### Admin
- **Username**: 
  - Bắt buộc
  - Tối thiểu 3 ký tự
  - Không được trùng với username khác trong hệ thống

### Teacher
- **Họ tên**: Bắt buộc
- **Số điện thoại**: 10-11 chữ số (nếu có nhập)
- **Địa chỉ**: Không bắt buộc

### Student
- **Email**: Định dạng email hợp lệ (nếu có nhập)
- **Số điện thoại**: 10-11 chữ số (nếu có nhập)
- **Địa chỉ**: Không bắt buộc

### Change Password (All Roles)
- **Mật khẩu cũ**: Bắt buộc, phải khớp với DB
- **Mật khẩu mới**: Bắt buộc, tối thiểu 6 ký tự
- **Xác nhận mật khẩu**: Phải khớp với mật khẩu mới

---

## 📋 Models Created

### Backend (`Models/ProfileModels.cs`)
```csharp
namespace StudentManagementSystem.Models
{
    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdateAdminProfileRequest
    {
        public string? Username { get; set; }
    }
}
```

---

## 🧪 Testing Checklist

### Admin Profile
- [x] View profile information
- [x] Edit username
- [x] Validate username uniqueness
- [x] Change password
- [x] Display success/error messages

### Teacher Profile
- [x] View profile information (with department)
- [x] Edit full name, phone, address
- [x] Validate phone format
- [x] Change password
- [x] Display success/error messages

### Student Profile
- [x] View profile information (with class & department)
- [x] Edit email, phone, address
- [x] Validate email format
- [x] Validate phone format
- [x] Change password
- [x] Display success/error messages

---

## 📊 Session Debug Logs

Backend sẽ log các thông tin sau:
```
[API Login] Session created - UserId: admin, Role: Admin, EntityId: admin
[ProfileAPI] UserRole: Admin, UserId: admin, EntityId: admin
[ProfileAPI] Loading Admin profile for UserId: admin
[ProfileAPI] Admin profile loaded: admin
[ProfileAPI] Returning profile data with role: Admin
```

---

## 🎨 UI/UX Features

1. **Loading States**: Hiển thị spinner khi đang load data
2. **Alert Messages**: Success (green) và Error (red) với auto-hide sau 5s
3. **Validation Errors**: Hiển thị ngay dưới mỗi input field
4. **Modal**: Change password modal với backdrop click-to-close
5. **Responsive**: Mobile-friendly design
6. **Role Badge**: Hiển thị role hiện tại ở page header
7. **Animations**: Smooth transitions cho alerts và modal

---

## 🔒 Security Features

1. **Session-based Authentication**: Session cookies với HttpOnly
2. **JWT Token**: Dual authentication (Session + JWT)
3. **CORS with Credentials**: Cấu hình đúng để gửi cookies cross-origin
4. **Role-based Access**: Kiểm tra role ở cả backend và frontend
5. **Password Hashing**: SHA256 (nên nâng cấp lên BCrypt trong production)
6. **Password Validation**: Mật khẩu cũ phải đúng mới được đổi
7. **Authorization Checks**: Kiểm tra entityId match với session

---

## 📁 Files Modified/Created

### Created
- `Models/ProfileModels.cs` - Request models cho profile APIs
- `Docs/PROFILE_FEATURE_COMPLETE.md` - This documentation

### Modified
- `Controllers/API/ProfileController.cs` - Added Admin update endpoint
- `Controllers/API/AuthController.cs` - Added session creation in login
- `ClientApp/src/app/services/profile.service.ts` - Added admin update method
- `ClientApp/src/app/components/profile/profile.component.ts` - Added admin edit logic
- `ClientApp/src/app/components/profile/profile.component.html` - Added admin edit form
- `ClientApp/src/app/components/profile/profile.component.scss` - Added modal & validation styles
- `ClientApp/src/app/app.routes.ts` - Updated profile route to allow all roles

---

## 🚀 How to Test

1. **Start Backend**:
   ```powershell
   cd c:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
   dotnet run
   ```
   Backend: http://localhost:5298

2. **Start Frontend**:
   ```powershell
   cd ClientApp
   npm start
   ```
   Frontend: http://localhost:4200

3. **Login với các tài khoản test**:
   - Admin: `admin` / `admin123`
   - Teacher: `gv001` / `gv001`
   - Student: `sv001` / `sv001`

4. **Test Profile**:
   - Click vào menu "Thông tin cá nhân"
   - Xem thông tin hiển thị đúng theo role
   - Click "Chỉnh sửa" và thay đổi thông tin
   - Click "Lưu" và kiểm tra success message
   - Click "Đổi mật khẩu" và test form đổi password
   - Kiểm tra validation errors khi nhập sai

---

## ✅ Kết Luận

Trang profile đã được **đồng bộ hoàn toàn** giữa backend và frontend với:
- ✅ API endpoints đầy đủ cho cả 3 roles
- ✅ Session management hoạt động đúng
- ✅ Frontend component xử lý đúng logic cho từng role
- ✅ Validation chặt chẽ ở cả client và server
- ✅ UI/UX thân thiện với user
- ✅ Security được đảm bảo

Hệ thống đã sẵn sàng cho production sau khi:
1. Nâng cấp password hashing lên BCrypt/Argon2
2. Add HTTPS trong production
3. Add rate limiting cho sensitive endpoints
4. Add audit logs cho profile changes
