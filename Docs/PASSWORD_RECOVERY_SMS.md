# 📱 Tính năng Khôi phục Mật khẩu qua SĐT

## ✅ Đã hoàn thành

### **Backend Implementation:**

1. **Database Schema** ✅
   - Đã thêm 4 cột mới vào bảng `Users`:
     - `Phone` (nvarchar(15)) - Số điện thoại
     - `PhoneVerified` (bit) - Trạng thái xác thực SĐT
     - `ResetCode` (nvarchar(6)) - Mã reset 6 chữ số
     - `ResetCodeExpiry` (datetime) - Thời hạn mã reset (15 phút)
   
   - ✅ Đã tự động sync SĐT từ Students/Teachers vào Users

2. **SMS Service** ✅
   - `ISmsService` interface cho gửi SMS
   - `SmsService` implementation:
     - **Development mode**: Log message to console (không gửi SMS thật)
     - **Production mode**: Cần integrate với Twilio/Vonage (đã có template sẵn)
   - Đã đăng ký service trong `Program.cs`

3. **API Endpoints** ✅
   Đã thêm 3 endpoints mới vào `/api/auth/`:

   **a) Forgot Password**
   ```http
   POST /api/auth/forgot-password
   Body: { "phone": "0967941364" }
   
   Response:
   {
     "success": true,
     "message": "Mã khôi phục đã được gửi đến số điện thoại của bạn",
     "resetCode": "123456"  // ⚠️ CHỈ trong development
   }
   ```

   **b) Verify Reset Code**
   ```http
   POST /api/auth/verify-reset-code
   Body: { "phone": "0967941364", "code": "123456" }
   
   Response:
   {
     "success": true,
     "message": "Mã khôi phục hợp lệ",
     "resetToken": "base64token..."
   }
   ```

   **c) Reset Password**
   ```http
   POST /api/auth/reset-password
   Body: { 
     "phone": "0967941364", 
     "code": "123456",
     "newPassword": "newpassword",
     "confirmPassword": "newpassword"
   }
   
   Response:
   {
     "success": true,
     "message": "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập ngay bây giờ."
   }
   ```

---

## 📋 Cần làm tiếp (Frontend)

### **Phase 3: Angular UI**

Tạo 2 trang mới:

#### **1. Forgot Password Page** (`/forgot-password`)
```typescript
// forgot-password.component.ts
export class ForgotPasswordComponent {
  phone: string = '';
  
  sendResetCode() {
    this.http.post('/api/auth/forgot-password', { phone: this.phone })
      .subscribe(response => {
        if (response.success) {
          // Navigate to verify-code page
          this.router.navigate(['/verify-reset-code'], { 
            queryParams: { phone: this.phone } 
          });
        }
      });
  }
}
```

**UI Elements:**
- Input field cho số điện thoại (validation: 10-11 chữ số)
- Button "Gửi mã khôi phục"
- Link back to Login page
- Error/Success messages

#### **2. Reset Password Page** (`/reset-password`)
```typescript
// reset-password.component.ts
export class ResetPasswordComponent implements OnInit {
  phone: string = '';
  code: string = '';
  newPassword: string = '';
  confirmPassword: string = '';
  
  ngOnInit() {
    // Get phone from route params
    this.route.queryParams.subscribe(params => {
      this.phone = params['phone'];
    });
  }
  
  resetPassword() {
    this.http.post('/api/auth/reset-password', {
      phone: this.phone,
      code: this.code,
      newPassword: this.newPassword,
      confirmPassword: this.confirmPassword
    }).subscribe(response => {
      if (response.success) {
        this.router.navigate(['/login']);
      }
    });
  }
}
```

**UI Elements:**
- Display phone number (readonly)
- Input field cho 6-digit code
- Input field cho new password (với password strength indicator)
- Input field cho confirm password
- Button "Đặt lại mật khẩu"
- Countdown timer (15 phút)
- "Gửi lại mã" button

#### **3. Add to Login Page**
Thêm link "Quên mật khẩu?" dưới form login:
```html
<a routerLink="/forgot-password">Quên mật khẩu?</a>
```

#### **4. Update Routes**
```typescript
// app.routes.ts
{
  path: 'forgot-password',
  component: ForgotPasswordComponent
},
{
  path: 'reset-password',
  component: ResetPasswordComponent
}
```

---

## 🔐 Security Notes

1. **Mã reset code**: 6 chữ số random, hết hạn sau 15 phút
2. **Rate limiting**: Nên thêm throttle để chống spam SMS
3. **Development mode**: Mã sẽ hiện trong backend console log (không gửi SMS thật)
4. **Production mode**: Cần config SMS gateway (Twilio, Vonage, etc.)

---

## 🧪 Testing Flow

### Development Mode:
1. User nhập SĐT vào "Quên mật khẩu" form
2. Backend generate mã 6 số và log ra console:
   ```
   ╔══════════════════════════════════════════════════════════════╗
   ║              📱 SMS MESSAGE (DEV MODE)                       ║
   ╠══════════════════════════════════════════════════════════════╣
   ║ To: 0967941364                                               ║
   ║──────────────────────────────────────────────────────────────║
   ║ [Student Management System]                                  ║
   ║ Mã khôi phục mật khẩu: 123456                                ║
   ║ Mã có hiệu lực trong 15 phút.                                ║
   ║ Không chia sẻ mã này với bất kỳ ai.                          ║
   ╚══════════════════════════════════════════════════════════════╝
   ```
3. User nhập mã từ console vào form "Reset Password"
4. User nhập mật khẩu mới → Submit
5. Backend update password hash → User có thể login với password mới

### Test với existing users:
- Admin: phone = NULL (cần update trước)
- Teacher GV001: phone = "0123456789" (đã sync từ Teachers table)
- Student SV001: phone = "0987654321" (đã sync từ Students table)

---

## 📱 Production SMS Integration (Optional)

### Twilio Example:
```csharp
// In appsettings.Production.json
{
  "Twilio": {
    "AccountSid": "YOUR_ACCOUNT_SID",
    "AuthToken": "YOUR_AUTH_TOKEN",
    "FromNumber": "+1234567890"
  }
}

// In SmsService.cs (production branch)
var accountSid = _configuration["Twilio:AccountSid"];
var authToken = _configuration["Twilio:AuthToken"];
var fromNumber = _configuration["Twilio:FromNumber"];

TwilioClient.Init(accountSid, authToken);
var message = await MessageResource.CreateAsync(
    body: messageText,
    from: new PhoneNumber(fromNumber),
    to: new PhoneNumber(phoneNumber)
);
```

---

## 📂 Files Changed

### Backend:
- ✅ `Models/User.cs` - Added Phone, PhoneVerified, ResetCode, ResetCodeExpiry
- ✅ `Services/ISmsService.cs` - NEW
- ✅ `Services/SmsService.cs` - NEW
- ✅ `Controllers/API/AuthController.cs` - Added 3 endpoints + request models
- ✅ `Program.cs` - Registered SmsService
- ✅ `Database/ADD_PHONE_PASSWORD_RECOVERY.sql` - NEW

### Frontend (TODO):
- ⏳ `components/forgot-password/` - NEW
- ⏳ `components/reset-password/` - NEW
- ⏳ `components/login/` - Add "Quên mật khẩu?" link
- ⏳ `services/auth.service.ts` - Add forgot/reset methods
- ⏳ `models/models.ts` - Add ForgotPasswordRequest, ResetPasswordRequest interfaces
- ⏳ `app.routes.ts` - Add new routes

---

## 🎉 Ready to Use!

Backend đã sẵn sàng! Bạn có thể test ngay bằng **Postman** hoặc **curl**:

```bash
# Step 1: Request reset code
curl -X POST http://localhost:5298/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"phone":"0123456789"}'

# Check backend console for reset code

# Step 2: Verify code (optional)
curl -X POST http://localhost:5298/api/auth/verify-reset-code \
  -H "Content-Type: application/json" \
  -d '{"phone":"0123456789","code":"123456"}'

# Step 3: Reset password
curl -X POST http://localhost:5298/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "phone":"0123456789",
    "code":"123456",
    "newPassword":"newpassword123",
    "confirmPassword":"newpassword123"
  }'
```

Frontend UI cần implement sau theo design của bạn! 🎨
