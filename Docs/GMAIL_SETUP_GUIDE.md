# 📧 HƯỚNG DẪN CẤU HÌNH GMAIL ĐỂ GỬI EMAIL THẬT

**Thời gian:** 5 phút  
**Yêu cầu:** Tài khoản Gmail

---

## 🎯 MỤC TIÊU

Sau khi làm theo hướng dẫn này, hệ thống sẽ:
- ✅ Gửi email xác thực OTP đến Gmail thật
- ✅ Người dùng nhận email trong vòng 5 giây
- ✅ Email hiển thị đẹp với HTML template
- ✅ Hoạt động như website thật (Facebook, Google, Banking)

---

## 📋 BƯỚC 1: TẠO GMAIL APP PASSWORD

### 1.1. Bật 2-Step Verification

1. Mở trình duyệt và truy cập:
   ```
   https://myaccount.google.com/security
   ```

2. Tìm phần **"2-Step Verification"** (Xác minh 2 bước)

3. Click **"Turn on"** (Bật)

4. Làm theo hướng dẫn:
   - Nhập số điện thoại
   - Nhận mã xác thực qua SMS
   - Nhập mã và xác nhận

5. ✅ Xong! Bạn đã bật 2FA

### 1.2. Tạo App Password

1. Sau khi bật 2FA, truy cập:
   ```
   https://myaccount.google.com/apppasswords
   ```

2. Nhập mật khẩu Gmail nếu được yêu cầu

3. Click **"Select app"** → Chọn **"Mail"**

4. Click **"Select device"** → Chọn **"Windows Computer"**

5. Click **"GENERATE"** (Tạo)

6. **QUAN TRỌNG:** Sao chép mã 16 ký tự hiển thị
   ```
   Ví dụ: abcd efgh ijkl mnop
   ```

7. ⚠️ **LƯU Ý:** Mã này chỉ hiển thị 1 lần duy nhất!

---

## 📋 BƯỚC 2: CẬP NHẬT appsettings.json

### 2.1. Mở File

Mở file sau trong VS Code:
```
StudentManagementSystem/appsettings.json
```

### 2.2. Tìm Phần EmailSettings

Tìm đoạn code này:
```json
"EmailSettings": {
  "SmtpServer": "sandbox.smtp.mailtrap.io",
  "Port": 2525,
  "SenderEmail": "noreply@studentmanagement.com",
  "SenderPassword": "**********",
  "SenderName": "Student Management System",
  "EnableSsl": false
}
```

### 2.3. Thay Đổi

**QUAN TRỌNG:** Thay đổi toàn bộ thành:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "Port": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderPassword": "abcd efgh ijkl mnop",
  "SenderName": "Student Management System",
  "EnableSsl": true
}
```

**Giải thích:**
- `SmtpServer`: **smtp.gmail.com** (Gmail SMTP server)
- `Port`: **587** (TLS port cho Gmail)
- `SenderEmail`: **Email Gmail của bạn** (ví dụ: `anhhoane24@gmail.com`)
- `SenderPassword`: **Mã App Password 16 ký tự** (không có dấu cách!)
- `EnableSsl`: **true** (Gmail yêu cầu SSL/TLS)

### 2.4. Ví Dụ Cụ Thể

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "Port": 587,
  "SenderEmail": "anhhoane24@gmail.com",
  "SenderPassword": "abcdefghijklmnop",
  "SenderName": "Student Management System",
  "EnableSsl": true
}
```

### 2.5. Lưu File

- Nhấn **Ctrl + S** để lưu
- ✅ Xong!

---

## 📋 BƯỚC 3: RESTART APPLICATION

### 3.1. Dừng App Hiện Tại

Trong terminal VS Code:
1. Nhấn **Ctrl + C**
2. Chờ message "Application is shutting down..."

### 3.2. Chạy Lại

```powershell
dotnet run
```

Hoặc sử dụng file bat:
```powershell
.\run.bat
```

### 3.3. Chờ App Khởi Động

Bạn sẽ thấy:
```
info: Now listening on: http://localhost:5298
info: EmailService initialized - SMTP: smtp.gmail.com:587, Sender: anhhoane24@gmail.com
```

✅ **Thấy `smtp.gmail.com`** → Cấu hình đúng!  
❌ **Vẫn thấy `sandbox.smtp.mailtrap.io`** → File chưa được lưu, quay lại Bước 2

---

## 📋 BƯỚC 4: TEST GỬI EMAIL

### 4.1. Mở Trang Đăng Ký

```
http://localhost:5298/Account/Register
```

### 4.2. Đăng Ký Tài Khoản Mới

Điền thông tin:
```
Username:       testuser123
Email:          your-real-email@gmail.com  ← Email thật của bạn để nhận OTP
Password:       Test@123
Confirm Pass:   Test@123
Role:           Student
```

### 4.3. Click "Đăng Ký"

Hệ thống sẽ:
1. Tạo mã OTP 6 số (ví dụ: **756123**)
2. Hiển thị mã trên màn hình (cho dễ test)
3. 📧 **GỬI EMAIL đến Gmail của bạn**

### 4.4. Kiểm Tra Gmail

1. Mở Gmail inbox: https://mail.google.com
2. Tìm email từ **Student Management System**
3. Subject: **"Xác thực tài khoản của bạn"**
4. Mở email → Thấy mã OTP 6 số với thiết kế đẹp

### 4.5. Xác Thực

1. Copy mã 6 số từ email (hoặc từ màn hình)
2. Dán vào form xác thực
3. Click "Xác Thực"
4. 📧 **Nhận thêm welcome email**
5. ✅ Hoàn tất!

---

## ✅ DẤU HIỆU THÀNH CÔNG

### Trong Terminal:
```
info: Sending verification email to your-email@gmail.com
info: ✓ Email sent successfully to your-email@gmail.com
```

### Trong Gmail:
- Email đến trong vòng **5 giây**
- Hiển thị đúng HTML (gradient header, dashed border code)
- Không vào Spam

### Sau Xác Thực:
- Nhận welcome email
- Login thành công
- Vào Dashboard

---

## 🐛 TROUBLESHOOTING

### Vấn đề 1: Email không đến
**Check:**
```
1. App Password có 16 ký tự không?
2. Đã xóa hết dấu cách chưa?
3. EnableSsl = true chưa?
4. Port = 587 chưa?
5. Đã restart app chưa?
```

**Giải pháp:**
- Copy lại App Password cẩn thận
- Xóa hết dấu cách: `abcd efgh ijkl mnop` → `abcdefghijklmnop`
- Check terminal log có thấy `smtp.gmail.com` không

### Vấn đề 2: Lỗi "The SMTP server requires a secure connection"
**Nguyên nhân:** EnableSsl = false

**Giải pháp:**
```json
"EnableSsl": true  ← Phải là true!
```

### Vấn đề 3: Lỗi "Authentication failed"
**Nguyên nhân:** 
- Chưa bật 2-Step Verification
- App Password sai

**Giải pháp:**
1. Bật 2FA: https://myaccount.google.com/security
2. Tạo App Password mới: https://myaccount.google.com/apppasswords
3. Copy chính xác mã 16 ký tự

### Vấn đề 4: Email vào Spam
**Giải pháp:**
- Thêm sender email vào Contacts
- Click "Not spam" trong Gmail
- Lần sau sẽ vào Inbox

### Vấn đề 5: Lỗi "Username or Password incorrect"
**Nguyên nhân:** App Password hết hạn hoặc bị thu hồi

**Giải pháp:**
1. Xóa App Password cũ trong Google Account
2. Tạo App Password mới
3. Cập nhật lại appsettings.json

---

## 🔐 BẢO MẬT

### ⚠️ QUAN TRỌNG:

1. **Không commit appsettings.json lên Git**
   ```bash
   # Thêm vào .gitignore
   appsettings.json
   appsettings.*.json
   ```

2. **Sử dụng Environment Variables cho Production**
   ```json
   "SenderPassword": "${GMAIL_APP_PASSWORD}"
   ```

3. **App Password khác với Gmail Password**
   - App Password: 16 ký tự random
   - Chỉ dùng cho app, không dùng để login Gmail
   - Có thể thu hồi bất cứ lúc nào

4. **Giới hạn gửi email**
   - Gmail free: 500 emails/ngày
   - Nếu vượt quá → bị block tạm thời 24h

---

## 🚀 PRODUCTION DEPLOYMENT

### Khuyến Nghị:

Với production (website thật), nên dùng:

1. **SendGrid (100 emails/day free)**
   - Website: https://sendgrid.com
   - Đăng ký free account
   - Lấy API key
   - Độ tin cậy cao hơn Gmail

2. **Mailgun (100 emails/day free)**
   - Website: https://mailgun.com
   - Hỗ trợ tốt cho developers

3. **AWS SES (Very cheap)**
   - $0.10 cho 1,000 emails
   - Cần AWS account

### So Sánh:

| Service | Free Tier | Pros | Cons |
|---------|-----------|------|------|
| **Gmail SMTP** | 500/day | Dễ setup, miễn phí | Giới hạn, có thể bị block |
| **SendGrid** | 100/day | Reliable, Analytics | Cần verify domain |
| **Mailgun** | 100/day | Developer-friendly | Cần credit card |
| **AWS SES** | 62,000/month (free 1 year) | Rất rẻ, scalable | Phức tạp hơn |

---

## 🎉 KẾT LUẬN

Sau khi setup xong, bạn có:

✅ Hệ thống gửi email OTP thật như Facebook, Gmail  
✅ User nhận email trong vòng 5 giây  
✅ HTML email đẹp và professional  
✅ Welcome email sau verification  
✅ Hoàn toàn free (Gmail 500 emails/day)  
✅ Sẵn sàng cho production!  

---

## 📞 CẦN GIÚP ĐỠ?

### Logs để Check:

```powershell
# Xem logs khi gửi email
dotnet run
```

Tìm dòng:
```
info: Sending verification email to xxx@gmail.com
info: ✓ Email sent successfully
```

Hoặc:
```
fail: SMTP error sending email: ...
```

### Test SMTP Connection:

```powershell
# Test xem Gmail SMTP có hoạt động không
Test-NetConnection smtp.gmail.com -Port 587
```

Kết quả:
```
TcpTestSucceeded : True  ← OK!
```

---

**Chúc bạn thành công! 🎉**

*Last updated: October 26, 2025*
