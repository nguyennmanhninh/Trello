# Profile Feature - Quick Reference

## 🎯 URLs

- **Backend API**: `http://localhost:5298`
- **Frontend App**: `http://localhost:4200`
- **API Docs**: `http://localhost:5298/api/swagger`

---

## 🔑 Test Accounts

| Username | Password | Role    |
|----------|----------|---------|
| admin    | admin123 | Admin   |
| gv001    | gv001    | Teacher |
| sv001    | sv001    | Student |

---

## 📡 API Endpoints

### GET `/api/profile`
**Lấy thông tin profile hiện tại**
- Headers: Cookie (session)
- Response: `{ role: string, data: Student | Teacher | AdminProfile }`

### PUT `/api/profile/student`
**Cập nhật thông tin sinh viên**
- Body: `Student` object
- Quyền: Admin, Teacher, Student (own)

### PUT `/api/profile/teacher`
**Cập nhật thông tin giảng viên**
- Body: `Teacher` object
- Quyền: Admin, Teacher (own)

### PUT `/api/profile/admin`
**Cập nhật thông tin admin**
- Body: `{ username: string }`
- Quyền: Admin only

### PUT `/api/profile/password`
**Đổi mật khẩu**
- Body: `{ oldPassword, newPassword, confirmPassword }`
- Quyền: All roles

---

## 🛠️ Quick Commands

### Start Backend
```powershell
cd C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
dotnet run
```

### Start Frontend
```powershell
cd C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem\ClientApp
npm start
```

### Build Backend
```powershell
dotnet build
```

### Kill All Dotnet Processes
```powershell
taskkill /F /IM dotnet.exe
```

---

## 🧪 Testing Steps

1. ✅ Login với 3 roles khác nhau
2. ✅ Truy cập menu "Thông tin cá nhân"
3. ✅ Kiểm tra thông tin hiển thị đúng
4. ✅ Click "Chỉnh sửa" và thay đổi dữ liệu
5. ✅ Click "Lưu" và verify success message
6. ✅ Refresh page và verify dữ liệu đã được lưu
7. ✅ Click "Đổi mật khẩu"
8. ✅ Test validation (mật khẩu sai, confirm không khớp)
9. ✅ Đổi mật khẩu thành công và login lại

---

## 🐛 Debug Tips

### Check Session in Backend
Look for logs:
```
[API Login] Session created - UserId: xxx, Role: xxx, EntityId: xxx
[ProfileAPI] UserRole: xxx, UserId: xxx, EntityId: xxx
```

### Check HTTP Request in Browser
- F12 → Network tab
- Look for `/api/profile` request
- Check Headers: Should have `Cookie` with session
- Check Response: Should have `role` and `data`

### Common Issues

**401 Unauthorized on /api/profile**
- Session chưa được tạo khi login
- Check `withCredentials: true` trong HTTP interceptor
- Check backend có `app.UseSession()` và `app.UseCors("AllowAngular")`

**Profile data null hoặc undefined**
- Check backend logs để xem query có lỗi không
- Verify `EntityId` trong session match với DB

**Edit form không hiển thị**
- Check `isEditMode` flag
- Check role detection: `isAdmin()`, `isTeacher()`, `isStudent()`
- Check console logs để debug

---

## 📋 Feature Matrix

| Feature | Admin | Teacher | Student |
|---------|-------|---------|---------|
| View Profile | ✅ | ✅ | ✅ |
| Edit Username | ✅ | ❌ | ❌ |
| Edit Full Name | ❌ | ✅ | ❌ |
| Edit Email | ❌ | ❌ | ✅ |
| Edit Phone | ❌ | ✅ | ✅ |
| Edit Address | ❌ | ✅ | ✅ |
| Change Password | ✅ | ✅ | ✅ |

---

## 🔄 Session Flow

```
1. User login → POST /api/auth/login
2. Backend creates session:
   - UserId (username)
   - UserRole
   - EntityId
   - UserName (full name)
3. Frontend stores JWT token in localStorage
4. Subsequent requests send both:
   - JWT in Authorization header
   - Session cookie automatically
5. Backend uses session for /api/profile
```

---

## 📁 Key Files

### Backend
- `Controllers/API/ProfileController.cs` - Profile API endpoints
- `Controllers/API/AuthController.cs` - Login with session creation
- `Models/ProfileModels.cs` - Request/Response models
- `Program.cs` - Session & CORS configuration

### Frontend
- `services/profile.service.ts` - API calls
- `components/profile/profile.component.ts` - Logic
- `components/profile/profile.component.html` - Template
- `components/profile/profile.component.scss` - Styles
- `interceptors/jwt.interceptor.ts` - Add withCredentials
- `app.routes.ts` - Route configuration

---

## ✅ Completion Checklist

- [x] Backend API endpoints cho cả 3 roles
- [x] Session creation trong login API
- [x] Frontend service methods
- [x] Component logic cho 3 roles
- [x] Edit forms cho 3 roles
- [x] Change password modal
- [x] Validation (client & server)
- [x] Error handling
- [x] Success messages
- [x] Route permissions updated
- [x] Styling & responsive
- [x] Testing với 3 accounts
- [x] Documentation

---

## 🚀 Next Steps (Optional Enhancements)

1. **Security**:
   - [ ] Replace SHA256 with BCrypt for password hashing
   - [ ] Add rate limiting for profile endpoints
   - [ ] Add audit logs for profile changes

2. **Features**:
   - [ ] Upload avatar/profile picture
   - [ ] Email verification when changing email
   - [ ] 2FA support
   - [ ] Password strength meter
   - [ ] Password reset via email

3. **UX**:
   - [ ] Confirm dialog before saving
   - [ ] Undo changes button
   - [ ] Auto-save draft
   - [ ] Profile completion percentage

---

## 📞 Support

Nếu có vấn đề:
1. Check terminal logs (backend & frontend)
2. Check browser console (F12)
3. Review API requests in Network tab
4. Read `PROFILE_FEATURE_COMPLETE.md` for detailed docs
