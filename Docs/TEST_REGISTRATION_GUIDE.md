# 🧪 HƯỚNG DẪN TEST ĐĂNG KÝ ANGULAR

**Thời gian:** 2 phút  
**Status:** ✅ READY TO TEST

---

## 🚀 HỆ THỐNG ĐÃ CHẠY

### Backend:
```
✅ Running at: http://localhost:5298
✅ API Auth endpoints: /api/auth/register, /api/auth/verify-email, /api/auth/resend-code
```

### Frontend:
```
✅ Running at: http://localhost:4200
✅ Register page: http://localhost:4200/register
✅ Verify page: http://localhost:4200/verify-email
✅ Login page: http://localhost:4200/login
```

---

## 📝 TEST CASE 1: ĐĂNG KÝ THÀNH CÔNG

### Bước 1: Mở Trang Đăng Ký
```
URL: http://localhost:4200/register
```
**Hoặc:** Click "Đăng ký ngay" từ trang Login

### Bước 2: Điền Form
```
Tên đăng nhập:    testuser2025
Email:            test2025@gmail.com
Mật khẩu:         Test@123
Xác nhận MK:      Test@123
Vai trò:          Student (hoặc Teacher)
Họ và tên:        (Để trống - không bắt buộc)
```

### Bước 3: Submit
Click nút **"Đăng ký"**

**Expected Result:**
- ✅ Loading spinner hiện ra
- ✅ Chuyển sang trang Verify Email
- ✅ **MÃ OTP 6 SỐ HIỆN RÕ RÀNG** trên màn hình (ví dụ: **123456**)
- ✅ Email information hiển thị
- ✅ Countdown timer bắt đầu (15:00)

### Bước 4: Xác Thực
1. **Copy mã OTP** từ màn hình (click vào mã để copy)
2. **Dán vào ô** "Mã xác thực (6 chữ số)"
3. Click nút **"Xác Thực"**

**Expected Result:**
- ✅ Success message: "Xác thực email thành công!"
- ✅ Auto redirect về /login sau 2 giây
- ✅ Success message hiện trên login page

### Bước 5: Đăng Nhập
```
Username: testuser2025
Password: Test@123
```
Click **"Đăng nhập"**

**Expected Result:**
- ✅ Login thành công
- ✅ Chuyển vào Dashboard
- ✅ Thấy thông tin user ở header

---

## 📝 TEST CASE 2: VALIDATION ERRORS

### Test 2.1: Username Đã Tồn Tại
```
Username: admin (đã có trong DB)
Email: newemail@gmail.com
Password: Test@123
```
**Expected:** ❌ "Tên đăng nhập đã tồn tại"

### Test 2.2: Email Đã Tồn Tại
```
Username: newuser
Email: (email đã dùng trước đó)
Password: Test@123
```
**Expected:** ❌ "Email đã được sử dụng"

### Test 2.3: Password Không Khớp
```
Password:        Test@123
Confirm Password: Test@456
```
**Expected:** ❌ "Mật khẩu xác nhận không khớp"

### Test 2.4: Username Ngắn Quá
```
Username: ab (< 3 chars)
```
**Expected:** ❌ "Tên đăng nhập phải từ 3-50 ký tự"

### Test 2.5: Email Không Hợp Lệ
```
Email: notanemail
```
**Expected:** ❌ "Email không hợp lệ"

### Test 2.6: Password Ngắn Quá
```
Password: 123 (< 6 chars)
```
**Expected:** ❌ "Mật khẩu phải có ít nhất 6 ký tự"

---

## 📝 TEST CASE 3: VERIFY EMAIL ERRORS

### Test 3.1: Mã Sai
Nhập mã: **999999** (khác mã thật)

**Expected:** ❌ "Mã xác thực không đúng hoặc đã hết hạn"

### Test 3.2: Mã Hết Hạn
1. Đợi 15 phút (hoặc change system time)
2. Submit mã

**Expected:** ❌ "Mã xác thực đã hết hạn"

### Test 3.3: Resend Code
1. Click nút **"Gửi lại mã"**

**Expected:**
- ✅ Loading spinner
- ✅ Mã mới hiển thị (khác mã cũ)
- ✅ Timer reset về 15:00
- ✅ Success message: "Mã xác thực mới đã được gửi"

---

## 📝 TEST CASE 4: UI/UX FEATURES

### Test 4.1: Password Visibility Toggle
1. Nhập password
2. Click icon **mắt** bên phải

**Expected:**
- ✅ Password hiện rõ (type="text")
- ✅ Icon đổi thành mắt gạch (eye-slash)
- ✅ Click lại → ẩn password

### Test 4.2: Copy Code Button
1. Ở trang Verify Email
2. Click vào **mã OTP to**

**Expected:**
- ✅ Mã được copy vào clipboard
- ✅ Có thể paste vào input field

### Test 4.3: Paste Code Button
1. Copy mã từ đâu đó
2. Click icon **paste** bên phải input

**Expected:**
- ✅ Mã tự động điền vào ô input

### Test 4.4: Countdown Timer
1. Quan sát timer

**Expected:**
- ✅ Đếm ngược từ 15:00 → 14:59 → ...
- ✅ Khi hết: Hiển thị "Mã đã hết hạn"

### Test 4.5: Responsive Design
1. Resize browser xuống mobile (< 768px)

**Expected:**
- ✅ Form vẫn hiển thị đẹp
- ✅ Buttons stack vertically
- ✅ Code size nhỏ hơn (36px)
- ✅ Touch-friendly

---

## 📝 TEST CASE 5: NAVIGATION

### Test 5.1: Login → Register
1. Từ /login
2. Click "Đăng ký ngay"

**Expected:** ✅ Navigate to /register

### Test 5.2: Register → Login
1. Từ /register
2. Click "Đăng nhập ngay" (ở footer)

**Expected:** ✅ Navigate to /login

### Test 5.3: Verify → Register
1. Từ /verify-email
2. Click "Quay lại đăng ký"

**Expected:** ✅ Navigate to /register

### Test 5.4: Direct Access to Verify (No Email)
```
URL: http://localhost:4200/verify-email
```
(Truy cập trực tiếp không qua register)

**Expected:** ✅ Auto redirect to /register

---

## 📝 TEST CASE 6: API INTEGRATION

### Test 6.1: Check API Called
1. Open Browser DevTools (F12)
2. Go to **Network** tab
3. Đăng ký user mới

**Expected API Calls:**
```
POST http://localhost:5298/api/auth/register
Request:
{
  "username": "testuser2025",
  "email": "test2025@gmail.com",
  "password": "Test@123",
  "confirmPassword": "Test@123",
  "role": "Student"
}

Response:
{
  "success": true,
  "message": "Đăng ký thành công!",
  "verificationCode": "123456",
  "email": "test2025@gmail.com"
}
```

### Test 6.2: Verify Email API
```
POST http://localhost:5298/api/auth/verify-email
Request:
{
  "email": "test2025@gmail.com",
  "code": "123456"
}

Response:
{
  "success": true,
  "message": "Xác thực email thành công!"
}
```

### Test 6.3: Resend Code API
```
POST http://localhost:5298/api/auth/resend-code
Request:
{
  "email": "test2025@gmail.com"
}

Response:
{
  "success": true,
  "message": "Mã xác thực mới đã được gửi",
  "verificationCode": "654321"
}
```

---

## 📝 TEST CASE 7: DATABASE VERIFICATION

### Check User Created
Sau khi đăng ký, check database:

```sql
SELECT * FROM Users 
WHERE Username = 'testuser2025';
```

**Expected:**
```
UserId: 13 (auto-increment)
Username: testuser2025
Email: test2025@gmail.com
Password: [SHA256 hash] (không phải plain text)
Role: Student
EmailVerified: 0 (false - chưa verify)
VerificationCode: 123456
VerificationCodeExpiry: 2025-10-26 19:37:00 (15 phút sau createdAt)
CreatedAt: 2025-10-26 19:22:00
LastLoginAt: NULL
```

### Check After Verification
```sql
SELECT * FROM Users 
WHERE Username = 'testuser2025';
```

**Expected:**
```
EmailVerified: 1 (true)
VerificationCode: NULL (đã xóa)
VerificationCodeExpiry: NULL (đã xóa)
```

---

## 🎯 QUICK TEST SCRIPT

**Để test nhanh trong 2 phút:**

```
1. http://localhost:4200/register
2. Fill:
   - Username: quicktest
   - Email: quick@test.com
   - Password: Test@123
   - Confirm: Test@123
   - Role: Student
3. Submit → Thấy mã OTP to
4. Copy mã → Paste vào verify form
5. Submit → Success
6. Auto redirect to login
7. Login với quicktest / Test@123
8. Vào dashboard → DONE! ✅
```

---

## ⚠️ TROUBLESHOOTING

### Problem: Form không submit
**Check:**
- Console errors (F12 → Console)
- Network tab có API call không?
- Backend đang chạy không? (http://localhost:5298)

### Problem: Không redirect sau verify
**Check:**
- Console có error không?
- Response có success: true không?
- Đợi 2 giây (có delay)

### Problem: Mã OTP không hiển thị
**Check:**
- Navigation state có pass email không?
- Console log response từ register API

### Problem: Validation không hoạt động
**Check:**
- Input có [(ngModel)] binding không?
- validationErrors object có data không?

### Problem: Style lỗi
**Check:**
- SCSS có compile không?
- Browser cache (Ctrl + Shift + R để hard refresh)

---

## 🎨 UI ELEMENTS TO CHECK

### Register Page:
- [ ] ✅ Gradient background (#667eea → #764ba2)
- [ ] ✅ White card with shadow
- [ ] ✅ Icons cho mỗi field
- [ ] ✅ Password show/hide buttons
- [ ] ✅ Red asterisks (*) for required fields
- [ ] ✅ Error messages in red
- [ ] ✅ Submit button với gradient
- [ ] ✅ Loading spinner khi submit
- [ ] ✅ Register link ở footer

### Verify Email Page:
- [ ] ✅ Large OTP code display (48px)
- [ ] ✅ Dashed border around code
- [ ] ✅ Monospace font (Courier New)
- [ ] ✅ Click-to-copy functionality
- [ ] ✅ Email info box (blue background)
- [ ] ✅ Countdown timer (red text)
- [ ] ✅ 6-digit input field (centered text)
- [ ] ✅ Paste button
- [ ] ✅ Two buttons (Verify + Resend)
- [ ] ✅ Instructions panel (yellow background)
- [ ] ✅ Back to register link

---

## ✅ EXPECTED BEHAVIOR SUMMARY

| Action | Expected Result |
|--------|----------------|
| **Open /register** | Form hiển thị đầy đủ 6 fields |
| **Fill valid data** | No errors |
| **Submit** | → Navigate to /verify-email |
| **See OTP code** | 6 digits, large, clickable |
| **Copy code** | Code in clipboard |
| **Paste & verify** | Success message + redirect |
| **Login** | Dashboard accessible |
| **Database** | User record exists với EmailVerified=true |
| **Validation errors** | Red text below fields |
| **Resend code** | New code, timer reset |
| **Expired code** | Error message |
| **Wrong code** | Error message |

---

## 🎯 KẾT QUẢ MONG ĐỢI

Sau khi test xong, bạn sẽ có:

- ✅ 1 user mới trong database
- ✅ EmailVerified = true
- ✅ Có thể login vào hệ thống
- ✅ Thấy dashboard với data
- ✅ Tất cả validation hoạt động
- ✅ UI đẹp và responsive
- ✅ API integration hoạt động
- ✅ Navigation flow smooth

---

**🚀 BẮT ĐẦU TEST NGAY!**

Mở: **http://localhost:4200/register**
