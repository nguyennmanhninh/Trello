# ⚡ Quick Start Guide - Student Management System

## 🎯 **Mục tiêu**
Hướng dẫn chạy dự án Student Management System trên môi trường local trong 5 phút.

---

## ✅ **Yêu cầu hệ thống**

### 1. Phần mềm cần cài đặt:
- ✅ **Visual Studio 2022** hoặc **VS Code** + C# extension
- ✅ **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- ✅ **Node.js 18+** và **npm** - [Download](https://nodejs.org/)
- ✅ **SQL Server** (LocalDB/Express/Developer) - [Download](https://www.microsoft.com/sql-server/sql-server-downloads)
- ✅ **SQL Server Management Studio (SSMS)** hoặc **Azure Data Studio** (optional)

### 2. Kiểm tra cài đặt:
```powershell
# Kiểm tra .NET
dotnet --version
# Output: 8.0.x

# Kiểm tra Node.js
node --version
# Output: v18.x.x hoặc cao hơn

# Kiểm tra npm
npm --version
# Output: 9.x.x hoặc cao hơn
```

---

## 🚀 **Bước 1: Clone/Open Project**

```powershell
# Nếu clone từ Git
git clone <repository-url>
cd StudentManagementSystem/StudentManagementSystem

# Hoặc mở folder có sẵn
cd c:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
```

---

## 💾 **Bước 2: Setup Database**

### Option A: Sử dụng PowerShell Script (Recommended)
```powershell
# Chạy script import tự động
.\ImportSampleData.ps1
```

### Option B: Manual trong SSMS
1. Mở **SQL Server Management Studio**
2. Connect đến instance của bạn (thường là `.\SQLEXPRESS` hoặc `localhost`)
3. Tạo database mới tên `StudentManagementDB`
4. Execute file `FULL_DATABASE_SETUP.sql`
5. Execute file `INSERT_SAMPLE_DATA.sql`

### Kiểm tra Database
```sql
-- Chạy query này trong SSMS để kiểm tra
USE StudentManagementDB;
SELECT COUNT(*) FROM Students;  -- Phải có ít nhất 1 row
SELECT COUNT(*) FROM Teachers;
SELECT COUNT(*) FROM Classes;
```

---

## 🔧 **Bước 3: Cấu hình Connection String**

Mở file `appsettings.Development.json` và kiểm tra connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Lưu ý:** 
- Nếu dùng SQL Server instance khác, sửa `Server=...`
- Nếu dùng SQL Authentication, thay `Trusted_Connection=True` bằng `User Id=...;Password=...`

---

## 🎨 **Bước 4: Setup Frontend (Angular)**

```powershell
# Di chuyển vào thư mục Angular
cd ClientApp

# Cài đặt dependencies
npm install

# Quay lại root folder
cd ..
```

**Troubleshooting:**
- Nếu gặp lỗi npm, thử: `npm cache clean --force` rồi `npm install` lại
- Nếu thiếu Angular CLI: `npm install -g @angular/cli`

---

## ▶️ **Bước 5: Chạy ứng dụng**

### Option A: Sử dụng Helper Scripts (Easiest)

```powershell
# Quick start (chạy backend và frontend cùng lúc)
.\run.bat

# Hoặc debug mode
.\debug.bat
```

### Option B: Chạy Manual

**Terminal 1 - Backend:**
```powershell
# Restore packages
dotnet restore

# Build project
dotnet build

# Run backend
dotnet run
# Output: Now listening on: http://localhost:5298
```

**Terminal 2 - Frontend (PowerShell mới):**
```powershell
cd ClientApp
npm start
# Output: Angular app running at http://localhost:4200
```

---

## 🌐 **Bước 6: Truy cập ứng dụng**

1. Mở trình duyệt
2. Truy cập: **http://localhost:4200**
3. Đăng nhập với tài khoản test:

| Username | Password | Role    | Mô tả |
|----------|----------|---------|-------|
| `admin`  | `admin123` | Admin   | Full quyền CRUD |
| `gv001`  | `gv001`    | Teacher | Quản lý lớp/sinh viên/điểm |
| `sv001`  | `sv001`    | Student | Xem thông tin cá nhân |

---

## ✨ **Test các chức năng cơ bản**

### Admin (username: admin, password: admin123)
- ✅ Dashboard: Xem thống kê tổng quan
- ✅ Sinh viên: Thêm/sửa/xóa sinh viên
- ✅ Giáo viên: Quản lý giáo viên
- ✅ Lớp học: Tạo lớp, gán giáo viên chủ nhiệm
- ✅ Môn học: Quản lý môn học
- ✅ Khoa: Quản lý khoa
- ✅ Điểm: Xem/sửa điểm tất cả sinh viên
- ✅ Export: Xuất Excel/PDF

### Teacher (username: gv001, password: gv001)
- ✅ Xem sinh viên trong lớp mình chủ nhiệm
- ✅ Nhập/sửa điểm cho môn mình dạy
- ✅ Xem thống kê lớp mình

### Student (username: sv001, password: sv001)
- ✅ Xem thông tin cá nhân
- ✅ Xem điểm của mình
- ✅ Sửa thông tin cá nhân (giới hạn)

---

## 🐛 **Troubleshooting - Các lỗi thường gặp**

### 1. **Lỗi kết nối SQL Server**
```
Error: Cannot open database "StudentManagementDB"
```
**Giải pháp:**
- Kiểm tra SQL Server đang chạy: Services → SQL Server (SQLEXPRESS) → Start
- Kiểm tra connection string trong `appsettings.Development.json`
- Test connection bằng SSMS

### 2. **Lỗi port đã được sử dụng**
```
Error: Failed to bind to address http://localhost:5298
```
**Giải pháp:**
```powershell
# Tìm process đang dùng port
netstat -ano | findstr :5298

# Kill process (thay <PID> bằng số PID từ lệnh trên)
taskkill /PID <PID> /F
```

### 3. **Lỗi Angular không build được**
```
Error: Module not found
```
**Giải pháp:**
```powershell
cd ClientApp
rm -r node_modules
rm package-lock.json
npm cache clean --force
npm install
```

### 4. **Lỗi CORS khi gọi API**
```
Access to XMLHttpRequest blocked by CORS policy
```
**Giải pháp:**
- Kiểm tra `Program.cs` có enable CORS
- Đảm bảo frontend chạy trên `http://localhost:4200`
- Đảm bảo backend chạy trên `http://localhost:5298`

### 5. **Login không thành công**
**Giải pháp:**
- Kiểm tra database đã có sample data chưa:
```sql
SELECT * FROM Users;
SELECT * FROM Students WHERE Username = 'sv001';
```
- Clear browser cache và cookies
- Kiểm tra Console trong DevTools (F12)

---

## 📁 **Cấu trúc Project quan trọng**

```
StudentManagementSystem/
├── Controllers/              # Backend API endpoints
├── Models/                   # C# domain models
├── Services/                 # Business logic
├── Data/                     # Database context
├── ClientApp/                # Angular frontend
│   ├── src/app/
│   │   ├── components/      # UI components
│   │   ├── services/        # HTTP services
│   │   ├── guards/          # Route guards
│   │   └── models/          # TypeScript models
│   └── package.json         # npm dependencies
├── appsettings.json          # Configuration
├── FULL_DATABASE_SETUP.sql   # Database schema
├── INSERT_SAMPLE_DATA.sql    # Sample data
└── run.bat                   # Quick start script
```

---

## 🔐 **Security Note**

⚠️ **QUAN TRỌNG:** Dự án này là **demo/development version**
- Passwords được lưu plain text (KHÔNG dùng cho production!)
- Connection string có trong source code
- Debug mode enabled

**Trước khi deploy production:**
1. Hash passwords (BCrypt hoặc ASP.NET Core Identity)
2. Sử dụng Azure Key Vault hoặc User Secrets
3. Enable HTTPS
4. Configure proper CORS
5. Remove DebugController

---

## 📚 **Next Steps**

Sau khi chạy thành công:

1. ✅ Đọc `DEVELOPMENT_ROADMAP.md` để biết phần nào cần phát triển tiếp
2. ✅ Đọc `.github/copilot-instructions.md` để hiểu patterns và conventions
3. ✅ Test các chức năng với 3 roles khác nhau
4. ✅ Bắt đầu phát triển features mới theo roadmap

---

## 💡 **Useful Commands**

```powershell
# Backend - Restore & Run
dotnet restore
dotnet build
dotnet run

# Backend - Watch mode (auto-reload)
dotnet watch run

# Frontend - Dev server
cd ClientApp
npm start

# Frontend - Build production
npm run build

# Database - Reset (nếu cần)
# Chạy lại ImportSampleData.ps1 hoặc execute SQL scripts

# Clean build
dotnet clean
dotnet build
```

---

## 🆘 **Need Help?**

- 📖 Đọc `README.md` chi tiết hơn
- 🐛 Kiểm tra `TROUBLESHOOTING_LOGIN.md` nếu lỗi login
- 🚀 Xem `DEVELOPMENT_ROADMAP.md` cho development plan
- 💬 Check GitHub Issues (nếu có repository)

---

**Happy Coding! 🎓👨‍💻**
