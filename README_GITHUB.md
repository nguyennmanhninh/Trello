# 🎓 Student Management System

**Modern full-stack web application built with ASP.NET Core 8 and Angular 17**

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-17-red)](https://angular.io/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

> A comprehensive student management system with AI-powered chatbot, attendance tracking, and advanced reporting features.

![Dashboard Preview](https://via.placeholder.com/800x400/4F46E5/FFFFFF?text=Student+Management+Dashboard)

---

## ✨ Features

### 🎯 Core Functionality
- **Student Management**: Full CRUD operations with validation
- **Teacher Management**: Profile management and class assignments
- **Class Management**: Create, edit, and manage classes with departments
- **Course Management**: Credits, departments, and teacher assignments
- **Grade Management**: Score tracking with automatic classification
- **Attendance System**: Real-time attendance tracking with sessions
- **Dashboard Analytics**: Chart.js powered statistics and insights

### 🤖 AI Chatbot (Phase 1 Complete)
- **RAG (Retrieval Augmented Generation)** architecture
- **Full Codebase Scanning**: Scans 300-400 files in project
- **Google Gemini Integration**: FREE AI API with 3-key rotation
- **Intelligent Keyword Matching**: Vietnamese + English support
- **Response Caching**: 5-minute cache for instant responses
- **Follow-up Questions**: AI generates 3 suggested questions
- **Source Code Display**: Shows relevant code snippets

### 🔐 Security & Authentication
- **Role-based Access Control (RBAC)**: Admin, Teacher, Student roles
- **Session-based Authentication**: Secure session management
- **JWT Token Support**: For API authentication
- **Custom Authorization**: `[AuthorizeRole]` attribute
- **Input Sanitization**: XSS protection on all endpoints

### 📊 Advanced Features
- **Export to Excel/PDF**: EPPlus powered exports
- **Pagination**: Efficient data loading with PaginatedList
- **Rate Limiting**: API protection (10 requests/min)
- **Email Verification**: Gmail SMTP integration
- **SMS Recovery**: Twilio integration for password reset
- **Responsive Design**: Mobile-first CSS with Material Design

---

## 🚀 Quick Start

### Prerequisites
- **.NET 8 SDK** or later
- **Node.js 18+** and npm
- **SQL Server** (LocalDB, Express, or Full)
- **Visual Studio 2022** or VS Code

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/NhuHoa123/StudentManagementSystem.git
cd StudentManagementSystem
```

2. **Setup Database**
```bash
# Run database scripts
sqlcmd -S .\SQLEXPRESS -i Database/FULL_DATABASE_SETUP.sql
sqlcmd -S .\SQLEXPRESS -i Database/INSERT_SAMPLE_DATA.sql

# Or use PowerShell script
.\Scripts\ImportSampleData.ps1
```

3. **Configure Connection String**

Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

4. **Install Dependencies**
```bash
# Backend
dotnet restore
dotnet build

# Frontend
cd ClientApp
npm install
```

5. **Run Application**
```bash
# Option 1: Using scripts
.\Scripts\run.bat

# Option 2: Manual
# Terminal 1 - Backend
dotnet run

# Terminal 2 - Frontend
cd ClientApp
npm start
```

6. **Open Browser**
```
Backend: http://localhost:5298
Frontend: http://localhost:4200
```

### 🔑 Test Accounts

| Username | Password | Role | Description |
|----------|----------|------|-------------|
| `admin` | `admin123` | Admin | Full system access |
| `gv001` | `gv001` | Teacher | Manage assigned classes |
| `sv001` | `sv001` | Student | View grades and profile |

---

## 📁 Project Structure

```
StudentManagementSystem/
├── Controllers/          # MVC & API endpoints
│   ├── API/             # RESTful API controllers
│   └── ...              # MVC controllers
├── Services/            # Business logic layer
│   ├── RagService.cs    # AI chatbot service
│   ├── CodebaseScanner.cs  # Project file scanner
│   └── ...              # Other services
├── Models/              # Domain entities
├── Data/                # EF Core DbContext
├── ClientApp/           # Angular 17 frontend
│   ├── src/app/
│   │   ├── components/  # UI components
│   │   ├── services/    # HTTP services
│   │   ├── guards/      # Auth guards
│   │   └── models/      # TypeScript interfaces
│   └── ...
├── Database/            # SQL scripts
├── Docs/                # Documentation
└── Scripts/             # PowerShell utilities
```

---

## 🔧 Configuration

### API Keys

**Google Gemini API (for AI Chatbot)**
```json
{
  "Gemini": {
    "ApiKeys": [
      "YOUR_GEMINI_API_KEY_1",
      "YOUR_GEMINI_API_KEY_2",
      "YOUR_GEMINI_API_KEY_3"
    ]
  }
}
```

Get free API key: https://makersuite.google.com/app/apikey

**Gmail SMTP (for Email Verification)**
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  }
}
```

**Twilio SMS (for Password Recovery)**
```json
{
  "Twilio": {
    "AccountSid": "YOUR_TWILIO_ACCOUNT_SID",
    "AuthToken": "YOUR_TWILIO_AUTH_TOKEN",
    "PhoneNumber": "+1234567890"
  }
}
```

---

## 🎯 Usage Examples

### AI Chatbot

**Vietnamese Questions:**
```
"Làm sao để thêm sinh viên mới?"
"Dashboard có những thống kê gì?"
"Làm sao để export danh sách sinh viên ra Excel?"
"Attendance system hoạt động như thế nào?"
```

**English Questions:**
```
"How to add a new student?"
"What statistics are on the dashboard?"
"How to export student list to Excel?"
"How does the attendance system work?"
```

### Export Data

**Backend Controller:**
```csharp
[HttpGet("export-excel")]
[AuthorizeRole("Admin", "Teacher")]
public async Task<IActionResult> ExportToExcel()
{
    var students = await _context.Students.ToListAsync();
    var fileContent = _exportService.ExportStudentsToExcel(students);
    return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"DanhSachSinhVien_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
}
```

### Custom Authorization

```csharp
[AuthorizeRole("Admin", "Teacher")]
public async Task<IActionResult> Index()
{
    var userRole = HttpContext.Session.GetString("UserRole");
    var userId = HttpContext.Session.GetString("UserId");
    
    // Teacher can only see students from their classes
    if (userRole == "Teacher")
    {
        var teacherClasses = _context.Classes.Where(c => c.TeacherId == userId);
        students = students.Where(s => teacherClasses.Any(tc => tc.ClassId == s.ClassId));
    }
    
    return View(students);
}
```

---

## 📊 Performance Metrics

### AI Chatbot (Phase 1)

| Metric | Value |
|--------|-------|
| Files scanned | 300-400 files |
| Coverage | 100% of project |
| Cache hit time | 0ms (instant) |
| Cold start time | 3-4 seconds |
| Warm response | 900-1400ms |
| Cache TTL | 5 minutes |
| Memory usage | 50-80MB |

### Database

| Metric | Value |
|--------|-------|
| Sample students | 20 students |
| Sample teachers | 5 teachers |
| Sample classes | 6 classes |
| Sample courses | 8 courses |
| Average query time | < 100ms |

---

## 🛠️ Tech Stack

### Backend
- **ASP.NET Core 8** - Web framework
- **Entity Framework Core 8** - ORM
- **SQL Server** - Database
- **EPPlus** - Excel export
- **Google Gemini API** - AI chatbot
- **JWT** - API authentication

### Frontend
- **Angular 17** - Standalone components
- **RxJS** - Reactive programming
- **Chart.js** - Data visualization
- **marked.js** - Markdown rendering
- **highlight.js** - Code syntax highlighting
- **Bootstrap 5** - UI framework

### DevOps
- **PowerShell** - Automation scripts
- **Git** - Version control
- **GitHub** - Repository hosting

---

## 📚 Documentation

- [Quick Start Guide](Docs/QUICK_START.md)
- [Setup Guide](Docs/SETUP_GUIDE.md)
- [AI Chatbot Phase 1](AI_CHATBOT_PHASE1_CODEBASE_SCANNING.md)
- [API Documentation](Docs/API_ENDPOINT_LOGIC_REVIEW.md)
- [Deployment Guide](Docs/PRODUCTION_DEPLOYMENT_GUIDE.md)
- [Troubleshooting](Docs/TROUBLESHOOTING_LOGIN.md)

---

## 🐛 Known Issues & Fixes

### Fixed Issues ✅
- **Duplicate Message Display**: Fixed in `BUG_FIX_DUPLICATE_MESSAGES.md`
- **Teacher Export Filter**: Fixed in `TEACHER_EXPORT_FILTER_FIX.md`
- **Gemini Rate Limit**: Fixed with API key rotation
- **Password Hash**: Fixed in `Docs/BUG_FIX_PASSWORDHASH.md`

### Active Issues ⚠️
- CSS budget warnings in Angular build (cosmetic, not blocking)
- Nullable reference warnings (pre-existing, not critical)

---

## 🚧 Roadmap

### Phase 2 (Planned)
- [ ] Vector database integration (Pinecone)
- [ ] Semantic search for AI chatbot
- [ ] Real-time WebSocket notifications
- [ ] Advanced analytics dashboard

### Phase 3 (Future)
- [ ] Mobile app (React Native)
- [ ] Multi-language support (i18n)
- [ ] Advanced reporting (custom reports)
- [ ] Integration with external systems

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards
- Follow C# naming conventions (PascalCase for public members)
- Follow TypeScript/Angular style guide
- Add XML comments for public APIs
- Write unit tests for new features
- Update documentation

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**NhuHoa123**
- GitHub: [@NhuHoa123](https://github.com/NhuHoa123)
- Project Link: [https://github.com/NhuHoa123/StudentManagementSystem](https://github.com/NhuHoa123/StudentManagementSystem)

---

## 🙏 Acknowledgments

- **Google Gemini** - FREE AI API for chatbot
- **Chart.js** - Beautiful charts and graphs
- **EPPlus** - Excel export functionality
- **Angular Team** - Amazing frontend framework
- **Microsoft** - ASP.NET Core framework

---

## 📞 Support

Having issues? Check these resources:

1. **Documentation**: Check `Docs/` folder
2. **Issues**: Open an issue on GitHub
3. **AI Chatbot**: Ask the built-in AI chatbot in the app!

---

## ⭐ Star This Repository

If you find this project helpful, please give it a ⭐ on GitHub!

---

**Made with ❤️ by NhuHoa123**

*Last updated: November 3, 2025*
