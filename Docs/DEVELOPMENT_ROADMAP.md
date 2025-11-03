# 🚀 Development Roadmap - Student Management System

## ✅ **Đã hoàn thành (Completed)**

### Backend (ASP.NET Core)
- ✅ Database models (Student, Teacher, Class, Course, Grade, Department, User)
- ✅ ApplicationDbContext với EF Core
- ✅ Custom `AuthorizeRoleAttribute` cho phân quyền
- ✅ Controllers đầy đủ với CRUD operations:
  - StudentsController (Admin, Teacher, Student access)
  - TeachersController (Admin only)
  - ClassesController
  - CoursesController
  - GradesController
  - DepartmentsController
  - DashboardController
- ✅ Services layer (AuthService, JwtService, ExportService, StatisticsService)
- ✅ Export Excel/PDF functionality
- ✅ Pagination với `PaginatedList<T>`
- ✅ Role-based filtering trong Controllers

### Frontend (Angular)
- ✅ Angular 17 standalone components architecture
- ✅ Auth service với JWT interceptor
- ✅ Auth guard và role guard
- ✅ Layout component với sidebar navigation (role-based menu)
- ✅ TypeScript models matching backend
- ✅ Students components (list + form) với:
  - Validation
  - Error handling
  - Class dropdown
  - Search & pagination
  - Export Excel/PDF
- ✅ Dashboard component với Chart.js
  - Statistics overview
  - Grade distribution chart
  - Students by department chart
- ✅ Responsive design với custom CSS
- ✅ Material Design inspired theme

### Database
- ✅ SQL setup scripts (FULL_DATABASE_SETUP.sql)
- ✅ Sample data scripts (INSERT_SAMPLE_DATA.sql)
- ✅ PowerShell import script (ImportSampleData.ps1)

---

## 🔨 **Cần hoàn thiện (To Complete)**

### 1. Teachers Components (Priority: HIGH)
**File cần tạo/sửa:**
- `ClientApp/src/app/components/teachers/teachers-list.component.ts`
- `ClientApp/src/app/components/teachers/teachers-list.component.html`
- `ClientApp/src/app/components/teachers/teachers-form.component.ts`
- `ClientApp/src/app/components/teachers/teachers-form.component.html`

**Yêu cầu:**
- Danh sách giáo viên với search, pagination
- Form thêm/sửa với department dropdown
- Validation đầy đủ
- Export Excel/PDF
- Chỉ Admin mới có quyền CRUD

**Tham khảo:** `students-list.component.ts`, `students-form.component.ts`

---

### 2. Classes Components (Priority: HIGH)
**File cần sửa:**
- `ClientApp/src/app/components/classes/classes-list.component.ts`
- `ClientApp/src/app/components/classes/classes-list.component.html`
- `ClientApp/src/app/components/classes/classes-form.component.ts`
- `ClientApp/src/app/components/classes/classes-form.component.html`

**Yêu cầu:**
- Danh sách lớp với số lượng sinh viên
- Form với department dropdown và teacher dropdown (chọn giáo viên chủ nhiệm)
- Filter theo department
- Admin: Full CRUD
- Teacher: Chỉ xem lớp mình chủ nhiệm

---

### 3. Courses Components (Priority: HIGH)
**File cần sửa:**
- `ClientApp/src/app/components/courses/courses-list.component.ts`
- `ClientApp/src/app/components/courses/courses-form.component.ts`

**Yêu cầu:**
- Danh sách môn học với credits, teacher, department
- Form với validation (credits: 1-10)
- Department và teacher dropdowns
- Admin: CRUD
- Teacher/Student: View only

---

### 4. Grades Components (Priority: HIGH)
**File cần sửa:**
- `ClientApp/src/app/components/grades/grades-list.component.ts`
- `ClientApp/src/app/components/grades/grades-form.component.ts`

**Yêu cầu đặc biệt:**
- **Auto-calculate classification** dựa trên điểm:
  ```typescript
  getClassification(score: number): string {
    if (score >= 9.0) return 'Xuất sắc';
    if (score >= 8.0) return 'Giỏi';
    if (score >= 6.5) return 'Khá';
    if (score >= 5.0) return 'Trung bình';
    if (score >= 3.0) return 'Yếu';
    return 'Kém';
  }
  ```
- Teacher: Chỉ nhập điểm cho môn mình dạy
- Student: Chỉ xem điểm của mình
- Admin: Xem tất cả

---

### 5. Departments Components (Priority: MEDIUM)
**File cần sửa:**
- `ClientApp/src/app/components/departments/departments-list.component.ts`
- `ClientApp/src/app/components/departments/departments-form.component.ts`

**Yêu cầu:**
- Simple CRUD (Admin only)
- Hiển thị số lượng teachers/students/classes trong mỗi department
- Validation: DepartmentCode phải unique

---

### 6. Student Grade View (Priority: HIGH)
**File cần tạo:**
- `ClientApp/src/app/components/student/my-grades.component.ts`
- `ClientApp/src/app/components/student/my-grades.component.html`

**Yêu cầu:**
- Student chỉ xem điểm của chính mình
- Hiển thị theo môn học
- Tính GPA (điểm trung bình)
- Export transcript (bảng điểm) PDF

**Route cần thêm vào `app.routes.ts`:**
```typescript
{
  path: 'my-grades',
  component: MyGradesComponent,
  canActivate: [authGuard],
  data: { roles: ['Student'] }
}
```

---

### 7. Reports Module (Priority: MEDIUM)
**File cần tạo:**
- `ClientApp/src/app/components/reports/reports.component.ts`

**Báo cáo cần có:**
- Danh sách sinh viên theo lớp
- Danh sách sinh viên theo khoa
- Bảng điểm theo lớp/môn học
- Thống kê điểm trung bình
- Top sinh viên (highest GPA)
- Xuất PDF/Excel

---

### 8. Enable Auth Guards (Priority: CRITICAL)
**File cần sửa:** `app.routes.ts`

Hiện tại guards đang bị comment. Cần uncomment:
```typescript
{
  path: 'students',
  component: StudentsListComponent,
  canActivate: [authGuard],  // ← Uncomment
  data: { roles: ['Admin', 'Teacher'] }
}
```

**Kiểm tra:**
- [ ] Login required cho tất cả routes (trừ /login)
- [ ] Role-based access working
- [ ] Redirect to login if not authenticated
- [ ] Redirect to dashboard if wrong role

---

### 9. Responsive Design Improvements (Priority: LOW)
**File cần kiểm tra:**
- All component CSS files
- `shared-styles.css`
- `styles.css` (global)

**Cần test:**
- Mobile view (< 768px)
- Tablet view (768px - 1024px)
- Desktop view (> 1024px)
- Sidebar collapsible on mobile

---

### 10. Error Handling & UX (Priority: MEDIUM)
**Cần cải thiện:**
- [ ] Loading states cho tất cả API calls
- [ ] Error messages user-friendly (Vietnamese)
- [ ] Success toast notifications
- [ ] Confirm dialogs before delete
- [ ] Form validation real-time
- [ ] Disable submit button while loading

---

## 📚 **Tính năng nâng cao (Advanced Features)**

### A. Attendance System (Điểm danh)
**Mô tả:** Teacher điểm danh sinh viên theo buổi học

**Cần tạo:**
1. Backend:
   - `Models/Attendance.cs`
   - `Controllers/AttendanceController.cs`
   - SQL script để tạo bảng `Attendances`

2. Frontend:
   - `components/attendance/` module
   - Teacher: Điểm danh theo lớp/buổi
   - Student: Xem attendance history của mình

**Database schema:**
```sql
CREATE TABLE Attendances (
    AttendanceId INT PRIMARY KEY IDENTITY,
    StudentId VARCHAR(10) NOT NULL,
    CourseId VARCHAR(10) NOT NULL,
    ClassId VARCHAR(10) NOT NULL,
    AttendanceDate DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- 'Có mặt', 'Vắng có phép', 'Vắng không phép'
    Note NVARCHAR(500),
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId),
    FOREIGN KEY (ClassId) REFERENCES Classes(ClassId)
);
```

---

### B. Schedule/Timetable (Thời khóa biểu)
**Mô tả:** Lịch học theo tuần cho từng lớp

**Cần tạo:**
1. Backend:
   - `Models/Schedule.cs`
   - `Controllers/ScheduleController.cs`

2. Frontend:
   - `components/schedule/` module
   - Calendar view (weekly)
   - Admin: Tạo/sửa schedule
   - Teacher: Xem schedule của mình
   - Student: Xem schedule của lớp

---

### C. Notifications System
**Mô tả:** Thông báo real-time cho students/teachers

**Tech stack cần thêm:**
- SignalR (backend)
- Angular SignalR client

**Use cases:**
- Teacher đăng thông báo cho lớp
- Admin thông báo toàn trường
- Tự động thông báo khi có điểm mới

---

### D. Parent Portal
**Mô tả:** Phụ huynh xem thông tin con

**Cần tạo:**
- New role: `Parent`
- Link parent to student
- Parent can view: grades, attendance, schedule

---

### E. Assignment/Homework System
**Mô tả:** Teacher giao bài tập, student nộp online

**Tính năng:**
- Upload/download files
- Deadline tracking
- Grading assignments
- Comments/feedback

---

### F. File Upload (Avatar/Documents)
**Cần:**
- Student avatar upload
- Document attachments
- Azure Blob Storage hoặc local file system

---

## 🎯 **Recommended Development Order**

### **Phase 1: Core CRUD Completion** (Week 1-2)
1. ✅ Students (Done)
2. Teachers components
3. Classes components
4. Courses components
5. Departments components
6. Enable auth guards

### **Phase 2: Grades & Reports** (Week 2-3)
7. Grades components (with classification)
8. Student grade view
9. Reports module
10. Export functionality test

### **Phase 3: Polish & Testing** (Week 3-4)
11. Responsive design
12. Error handling
13. Loading states
14. UX improvements
15. Full system testing

### **Phase 4: Advanced Features** (Week 4+)
16. Attendance system
17. Schedule/Timetable
18. Notifications (optional)
19. Other advanced features

---

## 🐛 **Known Issues to Fix**

1. **Password field in edit mode**: Cần thêm tính năng đổi mật khẩu riêng
2. **Search không search theo Department**: Cần thêm department filter dropdown
3. **Export filename**: Cần format datetime đúng format Việt Nam
4. **Validation messages**: Một số message còn bằng tiếng Anh
5. **Mobile menu**: Sidebar không tự close sau khi navigate (cần test)

---

## 📝 **Testing Checklist**

### Backend API Testing
- [ ] All CRUD endpoints working
- [ ] Role-based access control correct
- [ ] Pagination working
- [ ] Search/filter working
- [ ] Export Excel/PDF working
- [ ] Validation working
- [ ] Error handling proper

### Frontend Testing
- [ ] Login/logout working
- [ ] Navigation role-based correct
- [ ] All forms validation working
- [ ] Success/error messages showing
- [ ] Loading states showing
- [ ] Pagination working
- [ ] Search working
- [ ] Export buttons working
- [ ] Responsive on mobile
- [ ] No console errors

### Database Testing
- [ ] Foreign keys working
- [ ] Cascading deletes handled
- [ ] Sample data loads correctly
- [ ] Queries optimized (no N+1)

---

## 📖 **Documentation to Update**

- [ ] README.md với screenshots
- [ ] API documentation (Swagger/OpenAPI)
- [ ] User manual (Vietnamese)
- [ ] Deployment guide
- [ ] Troubleshooting guide

---

## 🚀 **Deployment Checklist**

- [ ] Update connection string for production
- [ ] Remove/disable DebugController
- [ ] Enable HTTPS
- [ ] Set up proper CORS
- [ ] Configure logging
- [ ] Set up database backups
- [ ] Configure email service (if needed)
- [ ] Performance testing
- [ ] Security audit

---

**Good luck with development! 🎓**
