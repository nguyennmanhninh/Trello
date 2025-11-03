# 🔧 CẤU HÌNH GMAIL SMTP - HƯỚNG DẪN NHANH

**Vấn đề hiện tại:** 
```
SMTP error: The SMTP server requires a secure connection or the client was not authenticated.
5.7.0 Authentication Required
```

---

## ✅ GIẢI PHÁP - 3 BƯỚC

### Bước 1: Tạo Gmail App Password

1. Truy cập: **https://myaccount.google.com/security**

2. Tìm mục **"2-Step Verification"** (Xác minh 2 bước)
   - Nếu chưa bật → Click **Get Started** → Làm theo hướng dẫn để bật

3. Sau khi bật 2FA, quay lại **Security** page

4. Tìm mục **"App passwords"** (Mật khẩu ứng dụng)
   - Hoặc truy cập trực tiếp: **https://myaccount.google.com/apppasswords**

5. Click **"Select app"** → Chọn **"Mail"**

6. Click **"Select device"** → Chọn **"Other (Custom name)"**

7. Nhập tên: **"Student Management System"** → Click **Generate**

8. **Copy mã 16 ký tự** (dạng: `abcd efgh ijkl mnop`)
   - ⚠️ Lưu ý: Mã này chỉ hiển thị 1 lần!

---

### Bước 2: Cập nhật appsettings.json

Mở file: `appsettings.json`

Tìm section `EmailSettings`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-email@gmail.com",      ← ĐỔI DÒNG NÀY
    "SenderPassword": "your-app-password",       ← ĐỔI DÒNG NÀY
    "SenderName": "Student Management System",
    "EnableSsl": true
  }
}
```

**Thay thế:**

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "anhhoane24@gmail.com",           ← Email thật của bạn
    "SenderPassword": "abcdefghijklmnop",            ← App Password (không có dấu cách)
    "SenderName": "Student Management System",
    "EnableSsl": true
  }
}
```

**⚠️ LƯU Ý:**
- Sử dụng **App Password** (16 ký tự), KHÔNG phải mật khẩu Gmail thường
- Xóa hết dấu cách trong App Password (chỉ giữ 16 ký tự liền nhau)
- Ví dụ: `abcd efgh ijkl mnop` → `abcdefghijklmnop`

---

### Bước 3: Restart Application

Trong terminal PowerShell:

```powershell
# Stop application (Ctrl+C)

# Restart application
dotnet run
```

---

## 🧪 TEST LẠI

1. Truy cập: **http://localhost:5298/Account/Register**

2. Điền form:
   - Username: `testuser`
   - Email: `anhhoane24@gmail.com` (hoặc email khác của bạn)
   - Password: `Test@123`
   - Confirm Password: `Test@123`
   - Role: `Student`

3. Click **Đăng ký**

4. **Kết quả mong đợi:**
   - ✅ Thông báo: "Đăng ký thành công! Vui lòng kiểm tra email..."
   - ✅ Redirect to VerifyEmail page
   - ✅ Email với mã 6 số được gửi đến hộp thư

5. **Check Gmail:**
   - Inbox → Tìm email từ "Student Management System"
   - Subject: "Xác thực tài khoản - Student Management System"
   - Nội dung: Mã xác thực 6 số (ví dụ: 123456)

6. **Nhập mã xác thực:**
   - Trên trang VerifyEmail, nhập mã 6 số
   - Click **Xác thực**
   - ✅ Success → Redirect to Login

---

## 🐛 NẾU VẪN LỖI

### Lỗi 1: "Authentication Required"
**Nguyên nhân:** Chưa bật 2FA hoặc App Password sai

**Giải pháp:**
```
1. Kiểm tra 2-Step Verification đã bật chưa
2. Tạo App Password mới
3. Copy lại App Password (không có dấu cách)
4. Cập nhật lại appsettings.json
5. Restart dotnet run
```

### Lỗi 2: "Invalid credentials"
**Nguyên nhân:** SenderEmail hoặc SenderPassword sai

**Giải pháp:**
```
1. Kiểm tra SenderEmail đúng với Gmail đã tạo App Password
2. Kiểm tra SenderPassword không có dấu cách, đúng 16 ký tự
3. Thử đăng nhập Gmail bằng browser để verify account không bị khóa
```

### Lỗi 3: "SMTP timeout"
**Nguyên nhân:** Firewall block port 587

**Giải pháp:**
```
1. Check Windows Firewall cho phép port 587
2. Thử đổi Port từ 587 → 465 và EnableSsl = true
3. Check antivirus có block SMTP không
```

---

## 📊 KIỂM TRA LOG

Sau khi restart `dotnet run`, check terminal output:

**✅ Success logs:**
```
info: EmailService initialized - SMTP: smtp.gmail.com:587, Sender: anhhoane24@gmail.com
info: Generated verification code: 123456
info: ✓ User created with ID: 11
info: Sending verification email to anhhoane24@gmail.com
info: ✓ Verification email sent to anhhoane24@gmail.com
```

**❌ Error logs (nếu vẫn sai):**
```
fail: SMTP error sending email: Authentication Required
fail: SMTP Status Code: MustIssueStartTlsFirst
```

→ Nếu thấy error → Kiểm tra lại App Password

---

## 🔒 BẢO MẬT

**⚠️ QUAN TRỌNG:**

1. **KHÔNG commit appsettings.json** lên Git sau khi có password thật
   ```powershell
   # Thêm vào .gitignore
   echo "appsettings.json" >> .gitignore
   echo "appsettings.Development.json" >> .gitignore
   ```

2. **Sử dụng Environment Variables cho Production:**
   ```json
   "SenderEmail": "${EMAIL_SENDER}",
   "SenderPassword": "${EMAIL_PASSWORD}"
   ```

3. **Hoặc dùng User Secrets (Development):**
   ```powershell
   dotnet user-secrets init
   dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
   dotnet user-secrets set "EmailSettings:SenderPassword" "your-app-password"
   ```

---

## ✅ CHECKLIST

- [ ] Bật 2-Step Verification trên Gmail
- [ ] Tạo App Password (16 ký tự)
- [ ] Copy App Password (xóa hết dấu cách)
- [ ] Cập nhật SenderEmail trong appsettings.json
- [ ] Cập nhật SenderPassword trong appsettings.json
- [ ] Restart `dotnet run`
- [ ] Test đăng ký lại
- [ ] Check Gmail inbox
- [ ] Nhập mã xác thực
- [ ] Verify user có thể login

---

## 📞 SUPPORT

**Nếu vẫn không được:**

1. Screenshot terminal logs (phần error)
2. Screenshot appsettings.json EmailSettings section (che mất password)
3. Báo lỗi chi tiết

---

**🎯 SAU KHI CẤU HÌNH XONG, TÍNH NĂNG ĐĂNG KÝ SẼ HOẠT ĐỘNG 100%!**

*Estimated time: 5-10 phút*
