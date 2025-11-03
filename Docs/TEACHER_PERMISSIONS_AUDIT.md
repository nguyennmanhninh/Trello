# KIỂM TRA CHI TIẾT QUYỀN CỦA GIÁO VIÊN

## Bảng Yêu cầu vs Thực tế

| Chức năng | Quyền yêu cầu | Thực tế hiện tại | Trạng thái | Controller | Ghi chú |
|-----------|---------------|------------------|-----------|------------|---------|
| 1. Quản lý sinh viên | Có (chỉ lớp mình) | ✅ Có (filter theo lớp) | ✅ **ĐÚNG** | StudentsController | Filter: `teacherClasses.Any(tc => tc.ClassId == s.ClassId)` |
| 2. Quản lý giáo viên | Không | ✅ Không (chỉ edit self) | ✅ **ĐÚNG** | TeachersController | Teacher chỉ có quyền Edit thông tin cá nhân |
| 3. Quản lý lớp | Có (xem lớp mình) | ✅ Có (xem lớp mình) | ✅ **ĐÚNG** | ClassesController | Filter: `Where(c => c.TeacherId == userId)` |
| 4. Quản lý khoa | Không | ✅ Không | ✅ **ĐÚNG** | DepartmentsController | Admin only |
| 5. Quản lý môn học | Có (môn dạy) | ⚠️ Có (môn dạy) | ⚠️ **CẦN XÁC NHẬN** | CoursesController | Hiện: Teacher xem môn mình dạy, KHÔNG có quyền CRUD |
| 6. Quản lý điểm | Có (lớp mình) | ✅ Có (lớp mình) | ✅ **ĐÚNG** | GradesController | Filter: `teacherClasses.Any(tc => tc.ClassId == g.Student.ClassId)` + Full CRUD |
| 7. Xem điểm cá nhân | Có | ❌ **KHÔNG** | ❌ **SAI** | GradesController | MyGrades chỉ cho Student, Teacher không có action này |
| 8. Quản lý tài khoản | Không | ✅ Không | ✅ **ĐÚNG** | UsersController | Admin only |
| 9. Đổi thông tin cá nhân | Có | ✅ Có | ✅ **ĐÚNG** | TeachersController.Edit | Teacher có quyền Edit với validation userId |

---

## Chi tiết từng chức năng

### ✅ 1. Quản lý sinh viên (Có - chỉ lớp mình)
**Controller**: `StudentsController`
**Authorization**: `[AuthorizeRole("Admin", "Teacher")]`

**Index Action**:
```csharp
// Teacher can only see students from their classes
if (userRole == "Teacher")
{
    var teacherClasses = _context.Classes.Where(c => c.TeacherId == userId);
    studentsQuery = studentsQuery.Where(s => teacherClasses.Any(tc => tc.ClassId == s.ClassId));
}
```

**Kết quả**: 
- ✅ Teacher có thể **xem danh sách** sinh viên lớp mình dạy
- ✅ Teacher có thể **xem chi tiết** sinh viên lớp mình
- ✅ Teacher **KHÔNG** có quyền Create/Edit/Delete (Admin only)

---

### ✅ 2. Quản lý giáo viên (Không - chỉ edit self)
**Controller**: `TeachersController`

**Authorization**:
- Index/Details/Create/Delete: `[AuthorizeRole("Admin")]` - Admin only
- Edit: `[AuthorizeRole("Admin", "Teacher")]` - Teacher chỉ edit self

**Edit Action**:
```csharp
// Teacher can only edit their own info
if (userRole == "Teacher" && id != userId)
{
    return RedirectToAction("AccessDenied", "Account");
}
```

**Kết quả**: 
- ✅ Teacher **KHÔNG** thể xem danh sách giáo viên khác
- ✅ Teacher chỉ có thể **edit thông tin cá nhân** (họ tên, SĐT, địa chỉ)

---

### ✅ 3. Quản lý lớp (Có - xem lớp mình)
**Controller**: `ClassesController`
**Authorization**: 
- Index/Details: `[AuthorizeRole("Admin", "Teacher")]`
- Create/Edit/Delete: `[AuthorizeRole("Admin")]`

**Index Action**:
```csharp
// Teacher can only see their own classes
if (userRole == "Teacher")
{
    classesQuery = classesQuery.Where(c => c.TeacherId == userId);
}
```

**Details Action**:
```csharp
// Teacher can only view their own classes
if (userRole == "Teacher" && @class.TeacherId != userId)
{
    return RedirectToAction("AccessDenied", "Account");
}
```

**Kết quả**: 
- ✅ Teacher có thể **xem danh sách** lớp mình dạy
- ✅ Teacher có thể **xem chi tiết** lớp mình dạy
- ✅ Teacher **KHÔNG** có quyền Create/Edit/Delete lớp

---

### ✅ 4. Quản lý khoa (Không)
**Controller**: `DepartmentsController`
**Authorization**: `[AuthorizeRole("Admin")]` - Controller level

**Kết quả**: 
- ✅ Teacher **KHÔNG** có quyền truy cập bất kỳ action nào

---

### ⚠️ 5. Quản lý môn học (Có - môn dạy)
**Controller**: `CoursesController`
**Authorization**: 
- Index/Details: `[AuthorizeRole("Admin", "Teacher", "Student")]`
- Create/Edit/Delete: `[AuthorizeRole("Admin")]`

**Index Action**:
```csharp
// Teacher can only see their courses
if (userRole == "Teacher")
{
    coursesQuery = coursesQuery.Where(c => c.TeacherId == userId);
}
```

**Hiện trạng**:
- ✅ Teacher có thể **xem danh sách** môn học mình dạy
- ✅ Teacher có thể **xem chi tiết** môn học
- ❌ Teacher **KHÔNG** có quyền Create/Edit/Delete môn học

**⚠️ VẤN ĐỀ CẦN XÁC NHẬN**:
- Yêu cầu "Quản lý môn học" nghĩa là gì?
  - Nếu chỉ **XEM** môn dạy: ✅ ĐÃ ĐÚNG
  - Nếu cần **CRUD** môn dạy: ❌ CHƯA ĐÚNG (cần thêm quyền Edit cho Teacher)

**Khuyến nghị**: Xác nhận với khách hàng xem Teacher có cần quyền:
- Chỉnh sửa thông tin môn học mình dạy (tên, mô tả)?
- Hay chỉ cần xem thông tin?

---

### ✅ 6. Quản lý điểm (Có - lớp mình)
**Controller**: `GradesController`
**Authorization**: `[AuthorizeRole("Admin", "Teacher")]`

**Index Action**:
```csharp
// Teacher can only see grades for their classes
if (userRole == "Teacher")
{
    var teacherClasses = _context.Classes.Where(c => c.TeacherId == userId);
    gradesQuery = gradesQuery.Where(g => teacherClasses.Any(tc => tc.ClassId == g.Student.ClassId));
}
```

**Create/Edit/Delete**: 
- `[AuthorizeRole("Admin", "Teacher")]` - Teacher có đầy đủ quyền CRUD

**Kết quả**: 
- ✅ Teacher có thể **xem điểm** sinh viên lớp mình dạy
- ✅ Teacher có thể **thêm điểm** cho sinh viên lớp mình
- ✅ Teacher có thể **sửa điểm** sinh viên lớp mình
- ✅ Teacher có thể **xóa điểm** sinh viên lớp mình

---

### ❌ 7. Xem điểm cá nhân (Có) - **VẤN ĐỀ**
**Controller**: `GradesController`
**Action hiện tại**: `MyGrades()`
**Authorization**: `[AuthorizeRole("Student")]` - CHỈ Student

**Vấn đề**:
```csharp
// My Grades - for students to view their own grades
[AuthorizeRole("Student")]
public async Task<IActionResult> MyGrades()
{
    var userId = HttpContext.Session.GetString("UserId");
    var grades = await _context.Grades
        .Include(g => g.Course)
        .ThenInclude(c => c.Department)
        .Where(g => g.StudentId == userId)
        .ToListAsync();
    // ...
}
```

**Hiện trạng**:
- ❌ Teacher **KHÔNG** có quyền truy cập MyGrades (chỉ Student)
- ❌ Teacher **KHÔNG** có action riêng để xem điểm cá nhân của mình

**⚠️ CHƯA RÕ YÊU CẦU**:
- Yêu cầu "Xem điểm cá nhân" cho Teacher nghĩa là gì?
  - Teacher không phải sinh viên nên không có điểm
  - Có thể đây là yêu cầu nhầm lẫn?
  - Hoặc có thể là "Xem thống kê điểm của lớp mình"?

**Khuyến nghị**: Xác nhận với khách hàng:
- Teacher có phải là sinh viên kiêm nhiệm không?
- Nếu có: Cần tạo action MyGrades cho Teacher với TeacherId
- Nếu không: Có thể bỏ yêu cầu này hoặc hiểu là "Xem thống kê lớp"

---

### ✅ 8. Quản lý tài khoản (Không)
**Controller**: `UsersController`
**Authorization**: `[AuthorizeRole("Admin")]` - Controller level

**Kết quả**: 
- ✅ Teacher **KHÔNG** có quyền truy cập bất kỳ action nào

---

### ✅ 9. Đổi thông tin cá nhân (Có)
**Controller**: `TeachersController.Edit`
**Authorization**: `[AuthorizeRole("Admin", "Teacher")]`

**Edit Action** (GET & POST):
```csharp
var userRole = HttpContext.Session.GetString("UserRole");
var userId = HttpContext.Session.GetString("UserId");

// Teacher can only edit their own info
if (userRole == "Teacher" && id != userId)
{
    return RedirectToAction("AccessDenied", "Account");
}
```

**Kết quả**: 
- ✅ Teacher có thể **edit thông tin cá nhân**:
  - Họ và tên (FullName)
  - Ngày sinh (DateOfBirth)
  - Giới tính (Gender)
  - Số điện thoại (Phone)
  - Địa chỉ (Address)
  - Username/Password
  - Khoa (DepartmentId)
- ✅ Teacher **KHÔNG** thể edit thông tin giáo viên khác

---

## Tổng kết

### ✅ Đúng: 7/9 quyền (77.8%)

1. ✅ Quản lý sinh viên (chỉ lớp mình)
2. ✅ Quản lý giáo viên (không - chỉ edit self)
3. ✅ Quản lý lớp (xem lớp mình)
4. ✅ Quản lý khoa (không)
5. ⚠️ Quản lý môn học (xem môn dạy - CẦN XÁC NHẬN nếu cần CRUD)
6. ✅ Quản lý điểm (lớp mình - full CRUD)
7. ❌ Xem điểm cá nhân (CHƯA CÓ - cần làm rõ yêu cầu)
8. ✅ Quản lý tài khoản (không)
9. ✅ Đổi thông tin cá nhân (có)

---

## Vấn đề cần giải quyết

### ❌ VẤN ĐỀ 1: "Xem điểm cá nhân" cho Teacher (Mục 7)

**Câu hỏi cần trả lời**:
1. Teacher có phải là sinh viên kiêm nhiệm không?
2. Nếu có: Cần tạo MyGrades cho Teacher
3. Nếu không: Yêu cầu này có thể là nhầm lẫn

**Giải pháp đề xuất nếu Teacher là sinh viên**:
- Tạo action mới: `TeacherMyGrades()` 
- Hoặc sửa MyGrades cho phép cả Teacher: `[AuthorizeRole("Student", "Teacher")]`
- Filter theo EntityId từ bảng Users

**Giải pháp đề xuất nếu không phải**:
- Bỏ yêu cầu này khỏi bảng phân quyền
- Hoặc hiểu là "Xem thống kê điểm của lớp" (đã có ở Dashboard)

---

### ⚠️ VẤN ĐỀ 2: "Quản lý môn học" cho Teacher (Mục 5)

**Câu hỏi cần trả lời**:
1. "Quản lý" nghĩa là chỉ XEM hay bao gồm CRUD?
2. Teacher có cần chỉnh sửa thông tin môn học mình dạy không?

**Hiện trạng**:
- Teacher có thể **XEM** môn học mình dạy
- Teacher **KHÔNG** thể Create/Edit/Delete môn học

**Giải pháp nếu cần CRUD**:
- Thêm `[AuthorizeRole("Admin", "Teacher")]` cho Create/Edit/Delete
- Thêm validation: Teacher chỉ được edit môn mình dạy (`c.TeacherId == userId`)

---

## Khuyến nghị hành động

### Cần làm rõ với khách hàng:

1. **Mục 7 - "Xem điểm cá nhân"**:
   - [ ] Teacher có phải sinh viên kiêm nhiệm?
   - [ ] Nếu có: Triển khai MyGrades cho Teacher
   - [ ] Nếu không: Xóa yêu cầu này hoặc làm rõ ý nghĩa

2. **Mục 5 - "Quản lý môn học"**:
   - [ ] Chỉ xem môn dạy? → ✅ ĐÃ XONG
   - [ ] Cần CRUD môn dạy? → Cần triển khai thêm

### Nếu khách hàng xác nhận yêu cầu như hiện tại (chỉ xem):
- **Tỷ lệ hoàn thành: 100%** (bỏ qua mục 7 do không áp dụng)
- **Sẵn sàng production**: ✅ YES

---

**Báo cáo tạo ngày**: 2024
**Trạng thái hệ thống**: http://localhost:5298
**Người kiểm tra**: GitHub Copilot
