# 🗄️ Database Scripts

Folder này chứa tất cả SQL scripts cho database.

## 📋 Files

### Setup Scripts (Thực hiện theo thứ tự)
1. `FULL_DATABASE_SETUP.sql` - **Tạo toàn bộ database schema**
   - Tạo tables: Students, Teachers, Classes, Courses, Grades, Departments, Users
   - Tạo relationships và indexes
   - Chạy file này TRƯỚC TIÊN

2. `INSERT_SAMPLE_DATA.sql` - **Import dữ liệu mẫu**
   - 50+ sinh viên, 10+ giáo viên
   - Classes, courses, grades
   - User accounts (admin/admin123, gv001/gv001, sv001/sv001)

### Update Scripts
- `DATABASE_UPDATE.sql` - Cập nhật schema (nếu cần)
- `SIMPLE_DB_UPDATE.sql` - Updates đơn giản

### Quick Start
- `QUICK_IMPORT.sql` - Import nhanh data
- `TEST_CONNECTION.sql` - Test kết nối database

## 🚀 Quick Setup

```powershell
# Option 1: Dùng PowerShell script
cd c:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem\Scripts
.\ImportSampleData.ps1

# Option 2: Manual trong SSMS/Azure Data Studio
# 1. Mở FULL_DATABASE_SETUP.sql → Execute
# 2. Mở INSERT_SAMPLE_DATA.sql → Execute
```

## 🔗 Connection String

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

## ⚠️ Lưu ý

- Dự án **KHÔNG dùng EF Migrations**
- Mọi thay đổi schema phải làm qua SQL scripts
- Test trên dev trước khi apply lên production
