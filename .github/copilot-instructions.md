# Student Management System - AI Development Guide

## Project Overview
This is an **ASP.NET Core 8 + Angular 17** full-stack student management system (SMS) with role-based authentication (Admin, Teacher, Student).

**Tech Stack:**
- **Backend**: ASP.NET Core 8 MVC + Web API
- **Frontend**: Angular 17 (Standalone components)
- **Database**: SQL Server
- **Authentication**: Session-based + JWT
- **Styling**: Custom CSS with Material Design inspiration
- **Charts**: Chart.js for dashboard statistics

---

## Architecture & Key Patterns

### Backend Structure
```
StudentManagementSystem/
├── Controllers/           # MVC & API endpoints
│   ├── AccountController  # Login/Logout
│   ├── StudentsController # CRUD students (Admin, Teacher, Student access)
│   ├── TeachersController # CRUD teachers (Admin only)
│   ├── ClassesController  # CRUD classes
│   ├── CoursesController  # CRUD courses
│   ├── GradesController   # Grades management
│   └── DashboardController# Statistics
├── Models/               # Domain entities (Student, Teacher, Class, etc.)
├── Services/             # Business logic layer
│   ├── AuthService       # Authentication
│   ├── JwtService        # JWT token handling
│   ├── ExportService     # Excel/PDF export
│   └── StatisticsService # Dashboard stats
├── Data/                 # EF Core DbContext
├── Filters/              # Custom attributes
│   └── AuthorizeRoleAttribute  # Custom role-based authorization
└── Views/                # Razor views (server-side rendering)
```

### Frontend Structure
```
ClientApp/src/app/
├── components/
│   ├── layout/           # Main layout with sidebar navigation
│   ├── login/            # Login page
│   ├── dashboard/        # Dashboard with Chart.js
│   ├── students/         # Students list & form
│   ├── teachers/         # Teachers list & form
│   ├── classes/          # Classes management
│   ├── courses/          # Courses management
│   ├── grades/           # Grades management
│   └── departments/      # Departments management
├── services/             # HTTP services for API calls
├── guards/               # Auth & role guards
├── interceptors/         # JWT interceptor
└── models/               # TypeScript interfaces (models.ts)
```

---

## Critical Development Patterns

### 1. **Database: NO EF Migrations - Use SQL Scripts**
⚠️ **IMPORTANT**: This project does NOT use EF migrations. All database changes must be done via SQL scripts in the root folder:
- `FULL_DATABASE_SETUP.sql` - Complete schema
- `INSERT_SAMPLE_DATA.sql` - Sample data
- `ImportSampleData.ps1` - PowerShell import script

**Why**: The project uses `ApplicationDbContext` for CRUD only, not schema management.

### 2. **Authorization Pattern: Custom `[AuthorizeRole]` Attribute**
❌ **Don't use**: `[Authorize(Roles = "Admin")]`  
✅ **Use**: `[AuthorizeRole("Admin", "Teacher")]`

Example from `StudentsController.cs`:
```csharp
[AuthorizeRole("Admin", "Teacher")]
public async Task<IActionResult> Index(...)
{
    var userRole = HttpContext.Session.GetString("UserRole");
    var userId = HttpContext.Session.GetString("UserId");
    
    // Teacher can only see students from their classes
    if (userRole == "Teacher")
    {
        var teacherClasses = _context.Classes.Where(c => c.TeacherId == userId);
        studentsQuery = studentsQuery.Where(s => teacherClasses.Any(tc => tc.ClassId == s.ClassId));
    }
    // ...
}
```

### 3. **Pagination Pattern: Use `PaginatedList<T>`**
All list endpoints should use `Models/PaginatedList.cs`:
```csharp
return View(await PaginatedList<Student>.CreateAsync(studentsQuery.OrderBy(s => s.StudentId), pageNumber ?? 1, pageSize));
```

### 4. **Angular: PascalCase → camelCase Mapping**
Backend returns PascalCase JSON, Angular uses camelCase. Always map:
```typescript
this.students = rawStudents.map((s: any) => ({
  studentId: s.StudentId || s.studentId,
  fullName: s.FullName || s.fullName,
  // ...
}));
```

### 5. **Model Naming Conventions**
- **Students**: `StudentId` (varchar(10)), `FullName`, `DateOfBirth`, `Gender` (bool: true=Male, false=Female)
- **Teachers**: `TeacherId` (varchar(10)), linked to `DepartmentId`
- **Classes**: `ClassId`, `ClassName`, `DepartmentId`, `TeacherId` (giáo viên chủ nhiệm)
- **Courses**: `CourseId`, `CourseName`, `Credits` (int 1-10), `DepartmentId`, `TeacherId`
- **Grades**: `StudentId`, `CourseId`, `Score` (decimal 0-10), `Classification` (Xuất sắc, Giỏi, Khá, Trung bình, Yếu, Kém)

---

## Role-Based Access Control (RBAC)

### Admin
- Full CRUD on all entities
- View all statistics
- Export Excel/PDF
- Manage teachers, classes, departments

### Teacher
- View/Edit students in their assigned classes only
- View/Edit grades for their courses
- Limited class management (own classes)
- Cannot delete students with grades

### Student
- View own profile and grades only
- Update limited profile fields (phone, address)
- Cannot change class or academic info

---

## Developer Workflows

### Running the Application (Windows PowerShell)
```powershell
# Backend
cd c:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
dotnet restore
dotnet build
dotnet run

# Frontend (separate terminal)
cd ClientApp
npm install
npm start

# Or use helper scripts
.\run.bat      # Quick start
.\debug.bat    # Debug mode
```

### Database Setup
```powershell
# Option 1: PowerShell script
.\ImportSampleData.ps1

# Option 2: Manual SQL execution
# In SSMS or Azure Data Studio:
# 1. Execute FULL_DATABASE_SETUP.sql
# 2. Execute INSERT_SAMPLE_DATA.sql
```

### Connection String
Check `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Common Development Tasks

### Adding a New Entity
1. Create C# model in `Models/` (follow existing naming conventions)
2. Add `DbSet<T>` in `Data/ApplicationDbContext.cs`
3. Create SQL script to alter database (in root folder)
4. Create Controller with `[AuthorizeRole]` attributes
5. Create Angular service in `services/`
6. Create Angular components (list + form)
7. Update TypeScript models in `models/models.ts`
8. Add route in `app.routes.ts`
9. Add nav item in `layout.component.html` (role-based)

### Adding Validation
**Backend (C#)**:
```csharp
[Required(ErrorMessage = "Họ tên là bắt buộc")]
[StringLength(100)]
public string FullName { get; set; }
```

**Frontend (Angular)**:
```typescript
validateForm(): boolean {
  if (!this.student.fullName || this.student.fullName.length > 100) {
    this.validationErrors.fullName = 'Họ tên là bắt buộc và không quá 100 ký tự';
    return false;
  }
  return true;
}
```

```html
<input [(ngModel)]="student.fullName" [class.error]="validationErrors.fullName" maxlength="100" required />
<small class="error-text" *ngIf="validationErrors.fullName">{{ validationErrors.fullName }}</small>
```

### Exporting Data
Use `ExportService` in backend:
```csharp
public async Task<IActionResult> ExportToExcel(string searchString, ...)
{
    var students = await studentsQuery.ToListAsync();
    var fileContent = _exportService.ExportStudentsToExcel(students);
    return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"DanhSachSinhVien_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
}
```

---

## Quick Reference Checklist

### Before Modifying Database
- [ ] Create SQL script (don't use migrations)
- [ ] Update `ApplicationDbContext.cs`
- [ ] Update C# models
- [ ] Update Angular TypeScript models
- [ ] Test locally first

### Before Adding API Endpoint
- [ ] Add `[AuthorizeRole("Role1", "Role2")]`
- [ ] Check `HttpContext.Session.GetString("UserRole")` if filtering needed
- [ ] Return `PaginatedList<T>` for list endpoints
- [ ] Handle errors with try-catch and return appropriate messages

### Before Creating Angular Component
- [ ] Use `standalone: true`
- [ ] Import `CommonModule`, `FormsModule`, `RouterModule`
- [ ] Map PascalCase API response to camelCase
- [ ] Add validation with error messages
- [ ] Add loading states
- [ ] Handle API errors gracefully

### Before Committing
- [ ] Test all CRUD operations
- [ ] Verify role-based access works
- [ ] Check responsive design (mobile view)
- [ ] Ensure no console errors
- [ ] Update any affected documentation

---

## Important Files to Check Before Changes

**Authentication**:
- `Services/AuthService.cs`
- `Filters/AuthorizeRoleAttribute.cs`
- `services/auth.service.ts`
- `guards/auth.guard.ts`

**Database**:
- `Data/ApplicationDbContext.cs`
- Root SQL scripts

**Routing**:
- `app.routes.ts` (Angular)
- `Program.cs` (ASP.NET Core)

**Configuration**:
- `appsettings.json` / `appsettings.Development.json`
- `package.json` (Angular dependencies)

---

## Known Issues & Workarounds

1. **OPENJSON not available in SQL Server 2012**: Use explicit joins instead of `Contains()` with arrays
2. **Password in edit mode**: Password field excluded from edit to prevent accidental overwrites
3. **Gender as bool**: `true` = Male, `false` = Female (C# convention)
4. **Classification auto-calculation**: Grades component should calculate based on score ranges

---

## Testing Accounts (from sample data)

| Username | Password | Role    |
|----------|----------|---------|
| admin    | admin123 | Admin   |
| gv001    | gv001    | Teacher |
| sv001    | sv001    | Student |

---

## References & Documentation

- Original project docs: `README.md`, `QUICK_START.md`, `SETUP_GUIDE.md`
- Theme guide: `ClientApp/THEME_GUIDE.md`
- Permission audit: `TEACHER_PERMISSIONS_AUDIT.md`
- Pagination notes: `PAGINATION_AND_TEACHER_PERMISSIONS.md`

---

## Style Guide

**C# Naming**:
- PascalCase for classes, properties, methods
- camelCase for private fields (with `_` prefix)
- Display names in Vietnamese with `[Display(Name = "...")]`

**TypeScript/Angular**:
- camelCase for variables, properties, methods
- PascalCase for interfaces/types
- Use `readonly` for API URLs

**CSS**:
- Use CSS variables from `styles.css` root (e.g., `var(--primary-500)`)
- Avoid inline styles
- Mobile-first responsive design

**Comments**:
- Vietnamese for business logic explanations
- English for technical/code comments
