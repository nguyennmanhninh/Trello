# 🎓 Hệ Thống Quản Lý Sinh Viên

## 📋 Mô Tả Dự Án

Hệ thống quản lý sinh viên được xây dựng bằng **ASP.NET Core 8 MVC + Angular 17** với các chức năng quản lý thông tin sinh viên, giáo viên, lớp học, môn học và điểm số trong một trường đại học.

## 📁 Cấu Trúc Thư Mục

```
StudentManagementSystem/
├── 📄 README.md                    # Tài liệu chính
├── 📄 appsettings.json             # Cấu hình ứng dụng
├── 🔧 Program.cs                   # Entry point
│
├── 📁 Docs/                        # 📚 Tài liệu (68 files)
│   ├── INDEX.md                    # → Danh mục tài liệu
│   ├── AI_CHAT_GUIDE.md           # → Hướng dẫn AI Chat
│   ├── QUICK_START.md             # → Khởi động nhanh
│   └── ...
│
├── 📁 Scripts/                     # 🛠️ Scripts (21 files)
│   ├── INDEX.md                    # → Danh mục scripts
│   ├── run.bat                     # → Chạy ứng dụng
│   ├── ImportSampleData.ps1       # → Import dữ liệu mẫu
│   └── ...
│
├── 📁 Database/                    # 🗄️ SQL Scripts (28 files)
│   ├── INDEX.md                    # → Danh mục database
│   ├── FULL_DATABASE_SETUP.sql    # → Setup database
│   ├── INSERT_SAMPLE_DATA.sql     # → Dữ liệu mẫu
│   └── ...
│
├── 📁 Controllers/                 # API & MVC Controllers
├── 📁 Models/                      # Domain models
├── 📁 Services/                    # Business logic
├── 📁 Views/                       # Razor views
├── 📁 ClientApp/                   # Angular 17 frontend
│   ├── src/app/components/        # UI components
│   ├── src/app/services/          # Angular services
│   └── src/app/guards/            # Route guards
│
└── 📁 wwwroot/                     # Static files
```

## 🚀 Quick Links

- **📖 Documentation:** [Docs/INDEX.md](Docs/INDEX.md)
- **🛠️ Scripts:** [Scripts/INDEX.md](Scripts/INDEX.md)
- **🗄️ Database:** [Database/INDEX.md](Database/INDEX.md)
- **🤖 AI Chat Guide:** [Docs/AI_CHAT_GUIDE.md](Docs/AI_CHAT_GUIDE.md)
- **⚡ Quick Start:** [Docs/QUICK_START.md](Docs/QUICK_START.md)

## T�nh N?ng Ch�nh

### 1. X�c Th?c & Ph�n Quy?n
- **Admin**: To�n quy?n qu?n l� h? th?ng
- **Gi�o Vi�n**: Qu?n l� sinh vi�n trong l?p ch? nhi?m, nh?p ?i?m m�n h?c
- **Sinh Vi�n**: Xem th�ng tin c� nh�n, xem ?i?m s?

### 2. Qu?n L� (Admin)
- ? Qu?n l� Khoa (CRUD)
- ? Qu?n l� Gi�o Vi�n (CRUD + T�m ki?m)
- ? Qu?n l� L?p H?c (CRUD)
- ? Qu?n l� Sinh Vi�n (CRUD + T�m ki?m + Xu?t Excel)
- ? Qu?n l� M�n H?c (CRUD)
- ? Qu?n l� ?i?m (CRUD + Xu?t Excel)

### 3. Ch?c N?ng Gi�o Vi�n
- Xem danh s�ch sinh vi�n trong l?p ch? nhi?m
- Nh?p/S?a/X�a ?i?m cho sinh vi�n
- Xem danh s�ch m�n h?c gi?ng d?y
- Xu?t b�o c�o ?i?m

### 4. Ch?c N?ng Sinh Vi�n
- Xem th�ng tin c� nh�n
- Xem ?i?m thi c?a c�c m�n h?c
- Xem ?i?m trung b�nh
- C?p nh?t th�ng tin c� nh�n (c� gi?i h?n)

### 5. Th?ng K� & B�o C�o
- Th?ng k� s? l??ng sinh vi�n theo l?p, khoa
- Th?ng k� ?i?m trung b�nh theo l?p, m�n h?c
- Xu?t danh s�ch sinh vi�n ra Excel
- Xu?t b?ng ?i?m ra Excel

## C�ng Ngh? S? D?ng

- **Framework**: ASP.NET Core 8 MVC
- **Database**: SQL Server
- **ORM**: Entity Framework Core 8
- **Frontend**: Bootstrap 5, Bootstrap Icons
- **Export**: ClosedXML (Excel)
- **Authentication**: Session-based

## C?u Tr�c D? �n

```
StudentManagementSystem/
├── Controllers/      # C�c controller x? l� logic (MVC + API)
│   ├── AccountController.cs
│   ├── DashboardController.cs
│   ├── StudentsController.cs
│   └── API/          # REST API endpoints
│       ├── AuthController.cs
│       ├── ChatController.cs  # 🤖 AI RAG Chatbot API
│       └── DashboardController.cs
├── Models/           # C�c entity models
│   ├── Student.cs, Teacher.cs, Class.cs, Course.cs, Grade.cs
│   ├── Department.cs, User.cs
│   └── ViewModels/
├── Views/            # Razor views (Server-side)
│   ├── Students/, Teachers/, Classes/, Grades/
│   └── Shared/
├── ClientApp/        # 🔥 Angular 17 Frontend (SPA)
│   ├── src/app/
│   │   ├── components/
│   │   │   ├── students/
│   │   │   ├── teachers/
│   │   │   ├── dashboard/
│   │   │   └── ai-chat/  # 🤖 AI Chatbot Component
│   │   ├── services/
│   │   │   ├── ai-chat.service.ts
│   │   │   └── auth.service.ts
│   │   └── guards/
│   └── package.json
├── Data/             # Entity Framework DbContext
│   └── ApplicationDbContext.cs
├── Services/         # Business logic services
│   ├── AuthService.cs
│   ├── RagService.cs     # 🤖 AI RAG Service (Gemini API)
│   ├── ExportService.cs
│   └── StatisticsService.cs
├── Filters/          # Custom authorization
│   └── AuthorizeRoleAttribute.cs
├── Docs/             # 📚 T?t c? t�i li?u dự �n
│   ├── SETUP_GUIDE.md
│   ├── RAG_SETUP_GUIDE.md
│   └── QUICK_START.md
├── Database/         # 🗄️ SQL scripts
│   ├── FULL_DATABASE_SETUP.sql
│   └── INSERT_SAMPLE_DATA.sql
├── Scripts/          # ⚙️ Automation scripts
│   ├── run.bat
│   ├── ImportSampleData.ps1
│   └── index_codebase.py
├── Archive/          # 📦 Logs & temp files
├── Program.cs
└── README.md
```

## 🚀 H??ng D?n C�i ??t

### 1. Y�u C?u H? Th?ng
- **.NET 8 SDK** - Backend
- **Node.js 18+** - Frontend (Angular)
- **SQL Server 2019+** - Database
- **Visual Studio 2022** hoặc **VS Code**

### 2. T?o Database

**Option 1: Dùng PowerShell Script (Khuyến nghị)**
```powershell
cd Scripts
.\ImportSampleData.ps1
```

**Option 2: Manual**
1. Mở SQL Server Management Studio hoặc Azure Data Studio
2. Chạy `Database/FULL_DATABASE_SETUP.sql`
3. Chạy `Database/INSERT_SAMPLE_DATA.sql`

### 3. C?u H�nh Connection String

Mở `appsettings.json` và cập nhật:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Setup Google Gemini API (cho AI Chatbot)

1. Lấy API key từ: https://aistudio.google.com/app/apikey
2. Thêm vào `appsettings.json`:
```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  }
}
```

Chi tiết xem: `Docs/GEMINI_SETUP.md`

### 5. Ch?y ?ng D?ng

**Option 1: Quick Start (Cả Backend + Frontend)**
```cmd
Scripts\run.bat
```

**Option 2: Manual**

Backend:
```bash
dotnet restore
dotnet build
dotnet run
# → http://localhost:5298
```

Frontend (terminal mới):
```bash
cd ClientApp
npm install
npm start
# → http://localhost:4200
```

Ho?c nh?n F5 trong Visual Studio.

### 5. Truy C?p H? Th?ng

M? tr�nh duy?t v� truy c?p: `https://localhost:5001` ho?c `http://localhost:5000`

## T�i Kho?n M?u

### Admin
- **Username**: admin
- **Password**: admin123

### Gi�o Vi�n
- **Username**: gv001
- **Password**: gv001pass

- **Username**: gv002
- **Password**: gv002pass

### Sinh Vi�n
- **Username**: sv001
- **Password**: sv001pass

- **Username**: sv002
- **Password**: sv002pass

## Ch?c N?ng Chi Ti?t

### Admin Dashboard
- Th?ng k� t?ng quan: Sinh vi�n, Gi�o vi�n, L?p, M�n h?c, Khoa
- Truy c?p nhanh ??n c�c module qu?n l�

### Teacher Dashboard
- Danh s�ch l?p ch? nhi?m
- Danh s�ch m�n h?c gi?ng d?y
- Qu?n l� ?i?m sinh vi�n

### Student Dashboard
- Th�ng tin l?p h?c
- ?i?m trung b�nh
- Xem danh s�ch ?i?m c�c m�n

## C�c API/Endpoints Ch�nh

### Authentication
- `GET /Account/Login` - Trang ??ng nh?p
- `POST /Account/Login` - X? l� ??ng nh?p
- `POST /Account/Logout` - ??ng xu?t
- `GET /Account/ChangePassword` - ??i m?t kh?u

### Dashboard
- `GET /Dashboard/Index` - Trang ch? (ph�n quy?n theo role)

### Students
- `GET /Students/Index` - Danh s�ch sinh vi�n
- `GET /Students/Create` - Form th�m sinh vi�n
- `POST /Students/Create` - L?u sinh vi�n m?i
- `GET /Students/Edit/{id}` - Form s?a sinh vi�n
- `POST /Students/Edit/{id}` - C?p nh?t sinh vi�n
- `GET /Students/Delete/{id}` - X�c nh?n x�a
- `POST /Students/Delete/{id}` - X�a sinh vi�n
- `GET /Students/ExportToExcel` - Xu?t Excel

### Grades
- `GET /Grades/Index` - Danh s�ch ?i?m (Admin/Teacher)
- `GET /Grades/MyGrades` - ?i?m c?a sinh vi�n
- `GET /Grades/Create` - Form nh?p ?i?m
- `POST /Grades/Create` - L?u ?i?m
- `GET /Grades/ExportToExcel` - Xu?t Excel

## T�nh N?ng N?i B?t

### 1. Ph�n Quy?n Chi Ti?t
- Session-based authentication
- Custom AuthorizeRole attribute
- Ki?m tra quy?n truy c?p ? m?i action

### 2. Xu?t B�o C�o Excel
- Xu?t danh s�ch sinh vi�n v?i c�c b? l?c
- Xu?t b?ng ?i?m theo l?p/m�n h?c
- S? d?ng ClosedXML library

### 3. T�m Ki?m & L?c
- T�m ki?m sinh vi�n theo t�n, m�, l?p, khoa
- L?c ?i?m theo l?p v� m�n h?c
- T�m ki?m gi�o vi�n theo t�n

### 4. Validation
- Client-side validation (jQuery Validation)
- Server-side validation (Data Annotations)
- Th�ng b�o l?i th�n thi?n

### 5. UI/UX
- Responsive design v?i Bootstrap 5
- Bootstrap Icons
- Alert messages v?i TempData
- Card-based layout
- Gradient colors

## M? R?ng D? �n

### C�c t�nh n?ng c� th? th�m:
1. ? Reset password
2. ? Email notification
3. ? File upload (?nh ??i di?n)
4. ? Advanced reporting (Charts)
5. ? Import Excel
6. ? Audit logging
7. ? Role-based dashboard customization
8. ? API for mobile app
9. ? Real-time notifications (SignalR)
10. ? Multi-language support

### Th�m Views c�n thi?u:

B?n c?n t?o th�m c�c views CRUD cho:
- **Departments**: Edit.cshtml, Delete.cshtml, Details.cshtml
- **Teachers**: Index.cshtml, Create.cshtml, Edit.cshtml, Delete.cshtml, Details.cshtml
- **Classes**: Index.cshtml, Create.cshtml, Edit.cshtml, Delete.cshtml, Details.cshtml
- **Students**: Edit.cshtml, Delete.cshtml, Details.cshtml
- **Courses**: Index.cshtml, Create.cshtml, Edit.cshtml, Delete.cshtml, Details.cshtml
- **Grades**: Index.cshtml, Create.cshtml, Edit.cshtml, Delete.cshtml

**M?u view c� th? tham kh?o t? c�c view ?� t?o (Departments, Students).**

## X? L� L?i Th??ng G?p

### L?i Connection String
```
Error: Cannot open database "StudentManagementSystem"
```
**Gi?i ph�p**: Ki?m tra connection string trong `appsettings.json` v� ??m b?o SQL Server ?ang ch?y.

### L?i Migration
```
Error: No migrations found
```
**Gi?i ph�p**: D? �n n�y kh�ng s? d?ng EF Migrations m� s? d?ng SQL Script tr?c ti?p.

### L?i Session
```
Error: Session is not available
```
**Gi?i ph�p**: ??m b?o `app.UseSession()` ???c g?i trong `Program.cs`.

## ?�ng G�p

N?u b?n mu?n ?�ng g�p v�o d? �n:
1. Fork repository
2. T?o branch m?i (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. M? Pull Request

## Gi?y Ph�p

D? �n n�y ???c ph�t tri?n cho m?c ?�ch h?c t?p v� nghi�n c?u.

## Li�n H?

N?u c� c�u h?i, vui l�ng li�n h? qua email ho?c t?o Issue tr�n GitHub.

---

**Ph�t tri?n b?i ASP.NET Core 8 MVC** ??
