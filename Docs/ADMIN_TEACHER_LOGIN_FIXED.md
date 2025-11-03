# ✅ ADMIN & TEACHER LOGIN - FIXED!

## Vấn Đề Đã Khắc Phục

### Nguyên Nhân Lỗi:
1. **Sample data cũ** insert vào bảng `Users` với password plaintext
2. **Stored procedure** tìm cột `PasswordHash` nhưng data có cột `Password`
3. **Hệ thống registration mới** dùng SHA256 hash

### Giải Pháp Đã Thực Hiện:
✅ Cập nhật admin user với password SHA256 hashed  
✅ Cập nhật teacher users với password SHA256 hashed  
✅ Sửa stored procedure để tìm đúng cột `PasswordHash`  
✅ Sửa INSERT_SAMPLE_DATA.sql để dùng PasswordHash thay vì Password

---

## 🔐 Tài Khoản Test

### 1. Admin Account
```
Username: admin
Password: admin123
Role: Admin
```

### 2. Teacher Accounts
```
Username: teacher
Password: teacher123
Role: Teacher
```

```
Username: nvanh
Password: teacher123
Role: Teacher (Nguyễn Văn Anh)
```

```
Username: ttbich
Password: teacher123
Role: Teacher (Trần Thị Bích)
```

```
Username: lmtuan
Password: teacher123
Role: Teacher (Lê Minh Tuấn)
```

### 3. Student Accounts (from old system)
```
Username: sv001
Password: sv001
Role: Student
```

---

## 🧪 Test Steps

### Test 1: Admin Login
1. Mở http://localhost:4200/login
2. Nhập:
   - Username: `admin`
   - Password: `admin123`
3. Click **Đăng nhập**
4. ✅ Expected: Login thành công, redirect to Dashboard
5. ✅ Check: Header hiển thị "Administrator" và role "Admin"

### Test 2: Teacher Login
1. Logout (nếu đang login)
2. Nhập:
   - Username: `teacher`
   - Password: `teacher123`
3. Click **Đăng nhập**
4. ✅ Expected: Login thành công, redirect to Dashboard
5. ✅ Check: Header hiển thị "Teacher Test" và role "Teacher"

### Test 3: Teacher Login (Specific Teacher)
1. Logout
2. Nhập:
   - Username: `nvanh`
   - Password: `teacher123`
3. Click **Đăng nhập**
4. ✅ Expected: Login thành công
5. ✅ Check: Header hiển thị "Nguyen Van Anh" và role "Teacher"

### Test 4: Registration + Login (New User)
1. Logout
2. Click **Đăng ký ngay**
3. Đăng ký tài khoản mới:
   - Username: `student001`
   - Email: `student001@test.com`
   - Password: `password123`
   - Confirm Password: `password123`
   - Role: Student
4. Click **Đăng ký**
5. Nhập mã OTP từ screen
6. Click **Xác thực**
7. ✅ Expected: Verify thành công, redirect to login
8. Login với `student001` / `password123`
9. ✅ Expected: Login thành công

---

## 🔧 Technical Details

### Password Hashing
```csharp
// SHA256 Hash Function (C#)
private string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(password);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}
```

### Hash Values
```
"admin123"   → 0DPiKPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKA=
"teacher123" → jZae726s08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=
```

### Authentication Flow
```
1. User inputs: username + password
2. AuthService.AuthenticateAsync(username, password)
3. Check Users table:
   - Hash input password with SHA256
   - Compare with PasswordHash in database
   - If match: return user info
4. If not found, fallback to stored procedure:
   - usp_AuthenticateUser checks Teachers/Students tables
   - Uses plaintext password comparison (old system)
5. Return success/failure
```

### Database Schema
```sql
-- Users table (new system with email verification)
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL, -- SHA256 Base64
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL, -- Admin, Teacher, Student
    Email NVARCHAR(100) NOT NULL,
    EmailVerified BIT DEFAULT 0,
    VerificationCode NVARCHAR(6) NULL,
    VerificationCodeExpiry DATETIME NULL,
    EntityId NVARCHAR(10) NULL, -- Links to TeacherId/StudentId
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastLoginAt DATETIME NULL
);

-- Teachers table (old system)
CREATE TABLE Teachers (
    TeacherId NVARCHAR(10) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(100) NOT NULL, -- Plaintext
    ...
);
```

---

## 📝 Files Modified

### 1. Database\FIX_ADMIN_LOGIN.sql (NEW)
- Script SQL để update admin và teacher passwords
- Xóa users cũ
- Insert users mới với SHA256 passwords
- Insert teachers từ bảng Teachers vào Users

### 2. FixAdminLogin.ps1 (NEW)
- PowerShell script để chạy FIX_ADMIN_LOGIN.sql
- Hiển thị kết quả

### 3. Database\STORED_PROCEDURES.sql (UPDATED)
- Sửa `usp_AuthenticateUser` để tìm `PasswordHash` thay vì `Password`
- Thêm fallback cho `EntityId` và `FullName`

### 4. Database\INSERT_SAMPLE_DATA.sql (UPDATED)
- Đổi `Password` thành `PasswordHash`
- Thêm `Email` và `EmailVerified` fields
- Sử dụng SHA256 hashed passwords

---

## ✅ Verification Checklist

### Database Check
```sql
-- Check admin user
SELECT Username, LEFT(PasswordHash, 30) + '...' AS PasswordHash, 
       FullName, Role, Email, EmailVerified
FROM Users
WHERE Username = 'admin';

-- Check teacher users
SELECT Username, LEFT(PasswordHash, 30) + '...' AS PasswordHash, 
       FullName, Role, Email, EmailVerified, EntityId
FROM Users
WHERE Role = 'Teacher'
ORDER BY Username;
```

### Backend Check
1. Backend running: http://localhost:5298
2. Check console logs khi login
3. Xem `[AuthService]` logs

### Frontend Check
1. Frontend running: http://localhost:4200
2. Open DevTools Console (F12)
3. Check network tab for API calls
4. Check localStorage for token

---

## 🐛 Troubleshooting

### Problem: "Tên đăng nhập hoặc mật khẩu không đúng"

**Solution 1: Check database**
```sql
SELECT * FROM Users WHERE Username = 'admin';
-- PasswordHash phải là: 0DPiKPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKA=
```

**Solution 2: Re-run fix script**
```powershell
cd c:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
powershell -ExecutionPolicy Bypass -File .\FixAdminLogin.ps1
```

**Solution 3: Check backend logs**
- Mở terminal running `dotnet run`
- Xem logs `[AuthService]` khi login
- Check xem nó hash password như thế nào

### Problem: "Email chưa được xác thực"

**Solution: Update EmailVerified**
```sql
UPDATE Users 
SET EmailVerified = 1 
WHERE Username = 'admin' OR Username = 'teacher';
```

### Problem: Login được nhưng không có permissions

**Solution: Check EntityId**
```sql
-- For teachers, EntityId should link to TeacherId
UPDATE Users 
SET EntityId = 'GV001' 
WHERE Username = 'teacher';
```

---

## 🎯 Next Steps

1. ✅ **Admin login fixed** - Can now access all admin features
2. ✅ **Teacher login fixed** - Can now access teacher features
3. ✅ **Registration system works** - New users can register and login
4. 🔄 **Optional: Migrate old students** - Add students to Users table with hashed passwords
5. 🔄 **Optional: Remove old tables** - After migrating all data to Users table

---

## 📊 Current Status

### Working ✅
- Admin login (admin/admin123)
- Teacher login (teacher/teacher123, nvanh/teacher123, ttbich/teacher123, lmtuan/teacher123)
- New user registration with email verification
- SHA256 password hashing
- JWT token generation
- Role-based access control

### Old System (Still Working) ✅
- Teachers table with plaintext passwords (GV001, GV002, etc.)
- Students table with plaintext passwords (SV001, SV002, etc.)
- Fallback authentication via stored procedure

### Migration Path
- New users: Users table with SHA256 → Preferred method
- Old users: Teachers/Students tables with plaintext → Fallback method
- Eventually migrate all to Users table for consistency

---

**Date:** October 26, 2025  
**Status:** ✅ RESOLVED  
**Tested:** Admin and Teacher login working correctly
