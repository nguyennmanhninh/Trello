# 📧 HƯỚNG DẪN SETUP TÍNH NĂNG ĐĂNG KÝ & XÁC THỰC EMAIL

**Ngày:** October 26, 2025  
**Tính năng:** Đăng ký tài khoản mới + Xác thực email (gửi mã OTP qua Gmail)

---

## ✅ TỔNG QUAN TÍNH NĂNG

Hệ thống đăng ký và xác thực email hoàn chỉnh với các tính năng:

1. **Đăng ký tài khoản mới** - Form đăng ký với validation đầy đủ
2. **Gửi email xác thực** - Mã OTP 6 số gửi qua Gmail SMTP
3. **Xác thực email** - Nhập mã để kích hoạt tài khoản
4. **Gửi lại mã** - Nếu mã hết hạn hoặc chưa nhận được
5. **Welcome email** - Email chào mừng sau khi xác thực thành công

---

## 📁 CÁC FILE ĐÃ TẠO/SỬA

### Files Mới (6 files):
```
Services/EmailService.cs                      (307 dòng) - Email service với Gmail SMTP
Models/ViewModels/RegisterViewModel.cs        (64 dòng)  - ViewModels cho đăng ký & xác thực
Views/Account/Register.cshtml                 (400+ dòng) - UI đăng ký
Views/Account/VerifyEmail.cshtml              (350+ dòng) - UI xác thực email
ADD_EMAIL_VERIFICATION.sql                    (150 dòng) - SQL script cập nhật database
REGISTRATION_SETUP_GUIDE.md                   (File này) - Hướng dẫn setup
```

### Files Đã Sửa (4 files):
```
Models/User.cs                                - Thêm Email, EmailVerified, VerificationCode, etc.
Controllers/AccountController.cs              - Thêm Register, VerifyEmail, ResendVerificationCode endpoints
Program.cs                                    - Đăng ký EmailService
appsettings.json                              - Thêm EmailSettings configuration
```

---

## 🗂️ DATABASE CHANGES

### New Columns in Users Table:

| Column | Type | Description |
|--------|------|-------------|
| `Email` | NVARCHAR(100) NOT NULL | Email của user (unique) |
| `EmailVerified` | BIT NOT NULL | Trạng thái xác thực (default: 0) |
| `VerificationCode` | NVARCHAR(6) NULL | Mã xác thực 6 số |
| `VerificationCodeExpiry` | DATETIME NULL | Thời gian hết hạn mã (15 phút) |
| `CreatedAt` | DATETIME NOT NULL | Ngày tạo tài khoản |
| `LastLoginAt` | DATETIME NULL | Lần login cuối |

### Indexes Created:
- `UQ_Users_Email` - Unique constraint trên Email
- `IX_Users_Email` - Index cho tra cứu nhanh

---

## 🔧 HƯỚNG DẪN CÀI ĐẶT

### Bước 1: Cập Nhật Database

Chạy SQL script để thêm các columns mới:

```powershell
# Option 1: Sử dụng SQL Server Management Studio (SSMS)
# - Mở file ADD_EMAIL_VERIFICATION.sql
# - Connect tới database StudentManagementDB
# - Thực thi script (F5)

# Option 2: Sử dụng PowerShell
sqlcmd -S YOUR_SERVER -d StudentManagementDB -i ADD_EMAIL_VERIFICATION.sql

# Option 3: Sử dụng Azure Data Studio
# - Open ADD_EMAIL_VERIFICATION.sql
# - Run script
```

**Kết quả mong đợi:**
```
✓ Added Email column
✓ Added EmailVerified column
✓ Added VerificationCode column
✓ Added VerificationCodeExpiry column
✓ Added CreatedAt column
✓ Added LastLoginAt column
✓ Updated 3 existing users (với default email)
✓ Email column is now required
✓ Added unique constraint on Email
✓ Added index on Email
✅ EMAIL VERIFICATION FEATURE SETUP COMPLETE!
```

### Bước 2: Cấu Hình Gmail SMTP

#### 2.1. Tạo App Password cho Gmail

**⚠️ QUAN TRỌNG:** Google không cho phép sử dụng mật khẩu Gmail thông thường. Bạn phải tạo "App Password".

**Các bước:**

1. Truy cập: https://myaccount.google.com/security
2. Bật **2-Step Verification** (nếu chưa bật)
3. Sau khi bật 2FA, quay lại Security settings
4. Tìm mục **App passwords** (Mật khẩu ứng dụng)
5. Chọn **Mail** và thiết bị **Other (Custom name)**
6. Nhập tên: `Student Management System`
7. Click **Generate**
8. **Copy mã 16 ký tự** (dạng: `abcd efgh ijkl mnop`)

#### 2.2. Cập Nhật appsettings.json

Mở file `appsettings.json` và cập nhật section `EmailSettings`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-actual-email@gmail.com",
    "SenderPassword": "abcdefghijklmnop",
    "SenderName": "Student Management System",
    "EnableSsl": true
  }
}
```

**Thay thế:**
- `your-actual-email@gmail.com` → Email Gmail thật của bạn
- `abcdefghijklmnop` → App Password vừa tạo (16 ký tự, không có dấu cách)

**⚠️ Lưu ý bảo mật:**
- **KHÔNG commit** file appsettings.json lên Git nếu có password thật
- Nên dùng **Environment Variables** hoặc **Azure Key Vault** cho production
- Thêm `appsettings.json` vào `.gitignore`

### Bước 3: Build và Chạy Application

```powershell
# Navigate to project folder
cd C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem

# Clean previous builds
dotnet clean

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run application
dotnet run
```

**Nếu build thành công:**
```
✅ Build succeeded
→ Listening on: http://localhost:5298
```

### Bước 4: Test Tính Năng

#### 4.1. Test Đăng Ký

1. Mở trình duyệt: `http://localhost:5298/Account/Register`
2. Điền form:
   - **Tên đăng nhập:** `testuser`
   - **Email:** Email thật của bạn (để nhận mã)
   - **Mật khẩu:** `Test@123`
   - **Xác nhận mật khẩu:** `Test@123`
   - **Vai trò:** `Student`
3. Click **Đăng Ký Ngay**

**Kết quả mong đợi:**
- ✅ Redirect to `/Account/VerifyEmail`
- ✅ Success message: "Đăng ký thành công! Vui lòng kiểm tra email..."
- ✅ Email được gửi đến hộp thư của bạn

#### 4.2. Kiểm Tra Email

1. Mở Gmail của bạn
2. Tìm email từ **Student Management System**
3. Subject: **Xác thực tài khoản - Student Management System**
4. Nội dung email có:
   - Mã xác thực **6 chữ số** (ví dụ: `123456`)
   - Thông báo hết hạn: **15 phút**
   - Giao diện đẹp với HTML template

#### 4.3. Test Xác Thực Email

1. Quay lại trang Verify Email
2. Nhập email đã đăng ký
3. Nhập mã xác thực 6 số từ email
4. Click **Xác Thực Ngay**

**Kết quả mong đợi:**
- ✅ Success message: "Xác thực email thành công!"
- ✅ Redirect to `/Account/Login`
- ✅ Email "Chào mừng" được gửi

#### 4.4. Test Đăng Nhập

1. Đăng nhập với tài khoản vừa đăng ký
2. Username: `testuser`
3. Password: `Test@123`

**Kết quả mong đợi:**
- ✅ Đăng nhập thành công
- ✅ Redirect to Dashboard

---

## 🧪 TESTING CHECKLIST

### Unit Tests:

- [ ] **Đăng ký với username đã tồn tại** → Error: "Tên đăng nhập đã tồn tại"
- [ ] **Đăng ký với email đã tồn tại** → Error: "Email đã được sử dụng"
- [ ] **Đăng ký với mật khẩu < 6 ký tự** → Validation error
- [ ] **Đăng ký với mật khẩu không khớp** → Error: "Mật khẩu xác nhận không khớp"
- [ ] **Xác thực với mã sai** → Error: "Mã xác thực không đúng"
- [ ] **Xác thực với mã hết hạn (>15 phút)** → Error: "Mã xác thực đã hết hạn"
- [ ] **Gửi lại mã xác thực** → Mã mới được gửi, expiry reset
- [ ] **Đăng nhập trước khi xác thực email** → (Hiện tại vẫn cho login, có thể chặn nếu cần)

### Email Tests:

- [ ] **Email verification gửi thành công** → Check logs: "✅ Email sent successfully"
- [ ] **Email có mã 6 số đúng format** → Số từ 100000-999999
- [ ] **Email có HTML template đẹp** → Gradient header, dashed border
- [ ] **Welcome email sau xác thực** → Email chào mừng được gửi
- [ ] **Email gửi trong < 5 giây** → Performance check

### Security Tests:

- [ ] **Password được hash** → SHA256 (nên dùng BCrypt trong production)
- [ ] **Verification code random** → Không dự đoán được
- [ ] **Code expiry working** → Hết hạn sau 15 phút
- [ ] **Email unique constraint** → Không thể đăng ký trùng email
- [ ] **SQL injection prevention** → EF Core parameterized queries

---

## 🚨 TROUBLESHOOTING

### Problem 1: Email không được gửi

**Symptoms:**
- Log: "✗ Failed to send verification email"
- SMTP exception

**Solutions:**
```
1. Kiểm tra App Password:
   - Phải là 16 ký tự
   - Không có dấu cách
   - Copy chính xác từ Google

2. Kiểm tra 2-Step Verification:
   - Phải bật 2FA trên Gmail trước

3. Kiểm tra Less Secure Apps:
   - Gmail có thể chặn ứng dụng không an toàn
   - Sử dụng App Password là giải pháp đúng

4. Check firewall:
   - Port 587 phải được mở
   - Hoặc thử port 465 với SSL

5. Check logs:
   cd logs/
   tail -f api-*.log
```

### Problem 2: Database migration failed

**Symptoms:**
- SQL error: "Column already exists"
- SQL error: "Cannot insert NULL"

**Solutions:**
```sql
-- Check if columns exist
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' 
AND COLUMN_NAME IN ('Email', 'EmailVerified', 'VerificationCode');

-- If columns exist but script failed, drop and re-run:
ALTER TABLE Users DROP COLUMN Email;
ALTER TABLE Users DROP COLUMN EmailVerified;
-- ... then re-run ADD_EMAIL_VERIFICATION.sql
```

### Problem 3: Build errors

**Symptoms:**
- "IEmailService not found"
- "RegisterViewModel not found"

**Solutions:**
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

# Check if all files created
Get-ChildItem -Recurse -Include EmailService.cs, RegisterViewModel.cs
```

### Problem 4: Verification code expired

**User complaint:** "Mã xác thực hết hạn quá nhanh"

**Solutions:**
```csharp
// In AccountController.cs, line ~157
// Change expiry time from 15 minutes to 30 minutes:
VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(30), // Changed from 15 to 30
```

### Problem 5: Gmail SMTP timeout

**Symptoms:**
- "Timeout waiting for response"
- Takes > 30 seconds to send

**Solutions:**
```json
// Increase timeout in EmailService.cs line ~281:
Timeout = 60000 // Change from 30000 to 60000 (60 seconds)

// Or use async/await properly:
await smtpClient.SendMailAsync(mailMessage);
```

---

## 🔒 SECURITY BEST PRACTICES

### 1. Password Hashing

**⚠️ Current:** SHA256 (basic)  
**✅ Recommended:** BCrypt or Argon2

```csharp
// Install BCrypt.Net-Next package
dotnet add package BCrypt.Net-Next

// Update HashPassword method:
private string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password);
}

// Update password verification:
var isValid = BCrypt.Net.BCrypt.Verify(inputPassword, user.Password);
```

### 2. Environment Variables

**Thay vì hardcode trong appsettings.json:**

```json
{
  "EmailSettings": {
    "SenderEmail": "${EMAIL_SENDER}",
    "SenderPassword": "${EMAIL_PASSWORD}"
  }
}
```

**Set environment variables:**
```powershell
# Windows
$env:EMAIL_SENDER="your-email@gmail.com"
$env:EMAIL_PASSWORD="your-app-password"

# Linux/Mac
export EMAIL_SENDER="your-email@gmail.com"
export EMAIL_PASSWORD="your-app-password"
```

### 3. Rate Limiting

**Thêm rate limiting cho đăng ký:**

```csharp
// In Program.cs, add rate limiting policy:
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Register", options =>
    {
        options.PermitLimit = 3; // 3 registrations
        options.Window = TimeSpan.FromHours(1); // per hour
    });
});

// In AccountController.cs:
[HttpPost]
[EnableRateLimiting("Register")]
public async Task<IActionResult> Register(RegisterViewModel model)
```

### 4. CAPTCHA

**Ngăn bot đăng ký:**

```html
<!-- Add Google reCAPTCHA -->
<script src="https://www.google.com/recaptcha/api.js"></script>
<div class="g-recaptcha" data-sitekey="YOUR_SITE_KEY"></div>
```

---

## 📊 MONITORING & LOGS

### Key Metrics to Monitor:

1. **Registration Success Rate**
   - Target: > 95%
   - Track: Registrations vs Verifications

2. **Email Delivery Rate**
   - Target: > 99%
   - Track: Sent vs Failed emails

3. **Verification Time**
   - Average time: User receives email → Verifies
   - Target: < 5 minutes

4. **Email Send Duration**
   - Target: < 5 seconds
   - Track in logs: "Email sent successfully" with duration

### Log Analysis:

```powershell
# Check registration attempts
Select-String -Path "logs/api-*.log" -Pattern "REGISTRATION ATTEMPT"

# Check email send success
Select-String -Path "logs/api-*.log" -Pattern "Email sent successfully"

# Check failed emails
Select-String -Path "logs/api-*.log" -Pattern "Failed to send"

# Check verification attempts
Select-String -Path "logs/api-*.log" -Pattern "EMAIL VERIFICATION ATTEMPT"
```

---

## 🚀 PRODUCTION DEPLOYMENT

### Before Deployment:

1. **Update Password Hashing:** SHA256 → BCrypt
2. **Move Email Credentials:** appsettings.json → Environment Variables
3. **Add Rate Limiting:** Prevent abuse
4. **Add CAPTCHA:** Prevent bot registrations
5. **Setup Email Monitoring:** Track delivery rates
6. **Test Email Templates:** In different email clients (Gmail, Outlook, Yahoo)
7. **Backup Database:** Before running migration

### Deployment Steps:

```powershell
# 1. Backup database
sqlcmd -Q "BACKUP DATABASE StudentManagementDB TO DISK='C:\Backups\SMS_PreEmailFeature.bak'"

# 2. Run migration
sqlcmd -S PROD_SERVER -d StudentManagementDB -i ADD_EMAIL_VERIFICATION.sql

# 3. Update appsettings.Production.json
# (Use environment variables for credentials)

# 4. Build Release
dotnet publish --configuration Release --output C:\Publish\SMS

# 5. Deploy to IIS
# Copy files to IIS folder
# Restart Application Pool

# 6. Smoke test
curl https://your-domain.com/Account/Register
```

---

## 📞 SUPPORT & CONTACTS

**Issues:** GitHub Issues  
**Docs:** `REGISTRATION_SETUP_GUIDE.md`  
**Email:** your-email@gmail.com

---

## ✅ COMPLETION CHECKLIST

**Development:**
- [x] EmailService created (Gmail SMTP)
- [x] User model updated with email fields
- [x] Database migration script created
- [x] RegisterViewModel created
- [x] Registration endpoints implemented
- [x] Email verification endpoints implemented
- [x] Register UI created
- [x] VerifyEmail UI created
- [x] appsettings.json updated
- [x] Program.cs updated (EmailService registered)

**Testing:**
- [ ] Database migration tested
- [ ] Email configuration tested
- [ ] Registration flow tested (end-to-end)
- [ ] Email delivery tested
- [ ] Verification code tested
- [ ] Resend code tested
- [ ] Error handling tested
- [ ] Security tested (SQL injection, XSS, etc.)

**Production Ready:**
- [ ] Password hashing upgraded to BCrypt
- [ ] Email credentials in environment variables
- [ ] Rate limiting added
- [ ] CAPTCHA integrated
- [ ] Monitoring setup
- [ ] Documentation complete

---

**🎉 DONE! Hệ thống đăng ký & xác thực email đã sẵn sàng!**

**Next Steps:**
1. Chạy SQL script `ADD_EMAIL_VERIFICATION.sql`
2. Cấu hình Gmail SMTP trong `appsettings.json`
3. Build và test application
4. Deploy to production khi đã test kỹ

**Happy Coding! 🚀**
