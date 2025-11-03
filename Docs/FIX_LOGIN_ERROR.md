# 🚨 LỖI VÀ CÁCH SỬA - ĐỌC NGAY!

## ❌ LỖI BẠN ĐANG GẶP

```
1. Admin login bị stuck ở trang login
2. Error: Invalid column name 'DepartmentId'
```

---

## ✅ NGUYÊN NHÂN

**BẠN CHƯA CHẠY SQL SCRIPT ĐỂ CẬP NHẬT DATABASE!**

Database đang thiếu:
- Cột `DepartmentCode` trong bảng `Departments`
- Cột `DepartmentId` trong bảng `Teachers`

---

## 🔧 CÁCH SỬA (2 PHÚT)

### BƯỚC 1: Dừng ứng dụng
```
Nhấn Ctrl + C trong terminal đang chạy
```

### BƯỚC 2: Chạy SQL Script

**Option A: Dùng SQL Server Management Studio (Khuyến nghị)**
1. Mở SQL Server Management Studio
2. Connect tới server của bạn
3. Mở file: `SIMPLE_DB_UPDATE.sql` (trong thư mục dự án)
4. Nhấn F5 hoặc Execute
5. ✅ Done!

**Option B: Dùng Command Line**
```powershell
sqlcmd -S localhost -d StudentManagementSystem -i SIMPLE_DB_UPDATE.sql
```

### BƯỚC 3: Restart ứng dụng
```powershell
dotnet run
```

### BƯỚC 4: Test lại
```
1. Vào: http://localhost:5298/Account/Login
2. Login: admin / admin123
3. ✅ Phải vào được Dashboard!
```

---

## 📋 NẾU VẪN LỖI

### Kiểm tra database đã update chưa:

```sql
-- Chạy query này trong SQL Server Management Studio
USE StudentManagementSystem;

-- Kiểm tra Departments có DepartmentCode chưa
SELECT * FROM Departments;

-- Kiểm tra Teachers có DepartmentId chưa
SELECT * FROM Teachers;
```

**Kết quả mong đợi:**
- Departments phải có cột `DepartmentCode`
- Teachers phải có cột `DepartmentId`

---

## 🆘 VẪN KHÔNG ĐƯỢC?

### Cách 1: Xóa và tạo lại database
```sql
DROP DATABASE StudentManagementSystem;
-- Sau đó chạy lại script tạo database ban đầu trong "New Text Document.txt"
```

### Cách 2: Liên hệ
- Kiểm tra connection string trong `appsettings.json`
- Đảm bảo SQL Server đang chạy
- Kiểm tra quyền truy cập database

---

## 📝 TÓM TẮT

```
❌ LỖI: Admin login không vào được
✅ SỬA: Chạy SIMPLE_DB_UPDATE.sql
⏱️ THỜI GIAN: 2 phút
```

**SAU KHI SỬA:**
- ✅ Admin có thể login
- ✅ Teacher có thể chọn Khoa
- ✅ Department có Mã Khoa
- ✅ Tất cả tính năng hoạt động!

---

🎯 **HÀNH ĐỘNG NGAY BÂY GIỜ:**
1. Dừng app (Ctrl+C)
2. Mở SSMS → Chạy SIMPLE_DB_UPDATE.sql
3. Restart: `dotnet run`
4. Test login: admin/admin123

✅ **DONE!**
