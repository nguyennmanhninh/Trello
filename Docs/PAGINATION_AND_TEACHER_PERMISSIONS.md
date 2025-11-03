# Cập Nhật: Phân Trang và Quyền CRUD Sinh Viên cho Giáo Viên

## Tổng Quan
Đã hoàn thành 2 cập nhật quan trọng theo yêu cầu specification:

### 1. ✅ Thêm Quyền CRUD Sinh Viên cho Giáo Viên
**Yêu cầu gốc:** "Admin và Giáo viên có thể: Thêm mới, chỉnh sửa, xóa sinh viên"

**Thay đổi:**
- Giáo viên có thể **Thêm mới**, **Chỉnh sửa**, **Xóa** sinh viên
- **Validation quan trọng:** Giáo viên chỉ có thể thao tác với sinh viên trong lớp của họ
- Giáo viên chỉ thấy các lớp mà họ đang dạy trong dropdown

### 2. ✅ Thêm Phân Trang cho Danh Sách
**Yêu cầu gốc:** "Có phân trang, tìm kiếm, lọc dữ liệu"

**Thay đổi:**
- Phân trang cho danh sách **Sinh viên** (10/trang)
- Phân trang cho danh sách **Giáo viên** (10/trang)
- Phân trang cho danh sách **Điểm** (15/trang)
- Tự động bảo toàn filters (tìm kiếm, lọc theo lớp, khoa, môn học)

---

## Chi Tiết Kỹ Thuật

### A. Files Mới Tạo

#### 1. `Models/PaginatedList.cs`
```csharp
public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalCount { get; private set; }
    
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    
    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
```

**Chức năng:**
- Generic helper class cho pagination
- Tính toán tự động: TotalPages, HasPreviousPage, HasNextPage
- Async method `CreateAsync` để tối ưu performance

---

### B. Files Đã Chỉnh Sửa

#### 1. `Controllers/StudentsController.cs`

##### a) Index Method - Thêm Pagination
**Trước:**
```csharp
public async Task<IActionResult> Index(string searchString, int? classId, int? departmentId)
{
    var studentsQuery = _context.Students.Include(...);
    return View(await studentsQuery.ToListAsync());
}
```

**Sau:**
```csharp
public async Task<IActionResult> Index(string searchString, int? classId, int? departmentId, int? pageNumber)
{
    var studentsQuery = _context.Students.Include(...);
    
    int pageSize = 10;
    return View(await PaginatedList<Student>.CreateAsync(
        studentsQuery.OrderBy(s => s.StudentId), 
        pageNumber ?? 1, 
        pageSize));
}
```

##### b) Create GET/POST - Thêm Teacher Authorization
**Trước:**
```csharp
[AuthorizeRole("Admin")]
public async Task<IActionResult> Create()
```

**Sau:**
```csharp
[AuthorizeRole("Admin", "Teacher")]
public async Task<IActionResult> Create()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    var userRole = HttpContext.Session.GetString("Role");
    
    // Teacher chỉ thấy các lớp của mình
    if (userRole == "Teacher")
    {
        ViewData["ClassId"] = new SelectList(
            await _context.Classes
                .Where(c => c.TeacherId == userId)
                .ToListAsync(), 
            "ClassId", "ClassName");
    }
    // ... Admin thấy tất cả
}
```

**Validation trong POST:**
```csharp
[HttpPost]
[AuthorizeRole("Admin", "Teacher")]
public async Task<IActionResult> Create([Bind("...")] Student student)
{
    var userRole = HttpContext.Session.GetString("Role");
    var userId = HttpContext.Session.GetInt32("UserId");
    
    // Validation: Teacher chỉ có thể thêm sinh viên vào lớp của mình
    if (userRole == "Teacher")
    {
        var isTeacherClass = await _context.Classes
            .AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);
            
        if (!isTeacherClass)
        {
            return RedirectToAction(nameof(AccessDenied));
        }
    }
    
    // ... tiếp tục xử lý
}
```

##### c) Edit GET/POST - Thêm Teacher Authorization
**Trước:**
```csharp
[AuthorizeRole("Admin", "Student")]
public async Task<IActionResult> Edit(string id)
```

**Sau:**
```csharp
[AuthorizeRole("Admin", "Teacher", "Student")]
public async Task<IActionResult> Edit(string id)
{
    var userId = HttpContext.Session.GetInt32("UserId");
    var userRole = HttpContext.Session.GetString("Role");
    
    // Validation: Teacher chỉ có thể edit sinh viên trong lớp của mình
    if (userRole == "Teacher")
    {
        var isTeacherClass = await _context.Classes
            .AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);
            
        if (!isTeacherClass)
        {
            return RedirectToAction(nameof(AccessDenied));
        }
        
        // Teacher chỉ thấy các lớp của mình trong dropdown
        ViewData["ClassId"] = new SelectList(
            await _context.Classes.Where(c => c.TeacherId == userId).ToListAsync(),
            "ClassId", "ClassName", student.ClassId);
    }
    // ... Student chỉ edit thông tin cá nhân (không đổi lớp)
}
```

**Validation chuyển lớp trong POST:**
```csharp
[HttpPost]
[AuthorizeRole("Admin", "Teacher", "Student")]
public async Task<IActionResult> Edit(string id, [Bind("...")] Student student)
{
    // ... Student validation (không đổi lớp)
    
    // Teacher validation: Chỉ chuyển vào lớp của mình
    if (userRole == "Teacher")
    {
        var isCurrentTeacherClass = await _context.Classes
            .AnyAsync(c => c.ClassId == oldStudent.ClassId && c.TeacherId == userId);
        var isNewTeacherClass = await _context.Classes
            .AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);
            
        if (!isCurrentTeacherClass || !isNewTeacherClass)
        {
            return RedirectToAction(nameof(AccessDenied));
        }
    }
    
    // ... tiếp tục xử lý
}
```

##### d) Delete GET/POST - Thêm Teacher Validation
**Note:** Delete đã có `[AuthorizeRole("Admin", "Teacher")]` từ trước, chỉ thêm validation:

```csharp
[HttpPost, ActionName("Delete")]
[AuthorizeRole("Admin", "Teacher")]
public async Task<IActionResult> DeleteConfirmed(string id)
{
    var userRole = HttpContext.Session.GetString("Role");
    var userId = HttpContext.Session.GetInt32("UserId");
    
    // Validation: Teacher chỉ có thể xóa sinh viên trong lớp của mình
    if (userRole == "Teacher")
    {
        var isTeacherClass = await _context.Classes
            .AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);
            
        if (!isTeacherClass)
        {
            return RedirectToAction(nameof(AccessDenied));
        }
    }
    
    // ... tiếp tục xóa
}
```

---

#### 2. `Controllers/TeachersController.cs`

##### Index Method - Thêm Pagination
**Trước:**
```csharp
public async Task<IActionResult> Index(string searchString, int? departmentId)
{
    var teachersQuery = _context.Teachers.Include(...);
    return View(await teachersQuery.ToListAsync());
}
```

**Sau:**
```csharp
public async Task<IActionResult> Index(string searchString, int? departmentId, int? pageNumber)
{
    var teachersQuery = _context.Teachers.Include(...);
    
    int pageSize = 10;
    return View(await PaginatedList<Teacher>.CreateAsync(
        teachersQuery.OrderBy(t => t.TeacherId), 
        pageNumber ?? 1, 
        pageSize));
}
```

---

#### 3. `Controllers/GradesController.cs`

##### Index Method - Thêm Pagination
**Trước:**
```csharp
public async Task<IActionResult> Index(int? classId, int? courseId)
{
    var gradesQuery = _context.Grades.Include(...);
    return View(await gradesQuery.ToListAsync());
}
```

**Sau:**
```csharp
public async Task<IActionResult> Index(int? classId, int? courseId, int? pageNumber)
{
    var gradesQuery = _context.Grades.Include(...);
    
    int pageSize = 15;
    return View(await PaginatedList<Grade>.CreateAsync(
        gradesQuery.OrderBy(g => g.StudentId).ThenBy(g => g.CourseId), 
        pageNumber ?? 1, 
        pageSize));
}
```

---

#### 4. `Views/Students/Index.cshtml`

##### a) Model Declaration
**Trước:**
```razor
@model IEnumerable<StudentManagementSystem.Models.Student>
```

**Sau:**
```razor
@model StudentManagementSystem.Models.PaginatedList<StudentManagementSystem.Models.Student>
```

##### b) Header Buttons - Thêm Teacher
**Trước:**
```razor
@if (userRole == "Admin")
{
    <a asp-action="Create" class="btn btn-primary">...</a>
}
```

**Sau:**
```razor
@if (userRole == "Admin" || userRole == "Teacher")
{
    <a asp-action="Create" class="btn btn-primary">...</a>
}
```

##### c) Action Buttons - Thêm Teacher
**Trước:**
```razor
@if (userRole == "Admin")
{
    <a asp-action="Edit" ...>Sửa</a>
    <a asp-action="Delete" ...>Xóa</a>
}
```

**Sau:**
```razor
@if (userRole == "Admin" || userRole == "Teacher")
{
    <a asp-action="Edit" ...>Sửa</a>
    <a asp-action="Delete" ...>Xóa</a>
}
```

##### d) Pagination UI
**Thêm mới (sau bảng):**
```razor
@if (Model.TotalPages > 1)
{
    <div class="card-footer">
        <nav aria-label="Page navigation">
            <ul class="pagination justify-content-center mb-0">
                <!-- Previous Button -->
                <li class="page-item @(Model.HasPreviousPage ? "" : "disabled")">
                    <a class="page-link" 
                       asp-action="Index" 
                       asp-route-pageNumber="@(Model.PageIndex - 1)"
                       asp-route-searchString="@ViewData["CurrentFilter"]"
                       asp-route-classId="@ViewData["CurrentClass"]"
                       asp-route-departmentId="@ViewData["CurrentDepartment"]">
                        <i class="bi bi-chevron-left"></i> Trước
                    </a>
                </li>

                <!-- Page Numbers -->
                @for (int i = 1; i <= Model.TotalPages; i++)
                {
                    <li class="page-item @(i == Model.PageIndex ? "active" : "")">
                        <a class="page-link" 
                           asp-action="Index" 
                           asp-route-pageNumber="@i"
                           asp-route-searchString="@ViewData["CurrentFilter"]"
                           asp-route-classId="@ViewData["CurrentClass"]"
                           asp-route-departmentId="@ViewData["CurrentDepartment"]">
                            @i
                        </a>
                    </li>
                }

                <!-- Next Button -->
                <li class="page-item @(Model.HasNextPage ? "" : "disabled")">
                    <a class="page-link" 
                       asp-action="Index" 
                       asp-route-pageNumber="@(Model.PageIndex + 1)"
                       asp-route-searchString="@ViewData["CurrentFilter"]"
                       asp-route-classId="@ViewData["CurrentClass"]"
                       asp-route-departmentId="@ViewData["CurrentDepartment"]">
                        Sau <i class="bi bi-chevron-right"></i>
                    </a>
                </li>
            </ul>
        </nav>
        <p class="text-center text-muted mt-2 mb-0">
            Trang @Model.PageIndex / @Model.TotalPages (Tổng: @Model.TotalCount sinh viên)
        </p>
    </div>
}
```

**Giải thích:**
- **asp-route-pageNumber**: Số trang cần chuyển đến
- **asp-route-searchString**: Bảo toàn từ khóa tìm kiếm
- **asp-route-classId**: Bảo toàn filter lớp
- **asp-route-departmentId**: Bảo toàn filter khoa
- **active class**: Highlight trang hiện tại
- **disabled class**: Vô hiệu hóa nút Trước/Sau khi không thể sử dụng

---

#### 5. `Views/Teachers/Index.cshtml`

##### a) Model Declaration
```razor
@model StudentManagementSystem.Models.PaginatedList<StudentManagementSystem.Models.Teacher>
```

##### b) Pagination UI
**Tương tự Students/Index, nhưng chỉ bảo toàn:**
- `searchString`
- `departmentId`

```razor
@if (Model.TotalPages > 1)
{
    <div class="card-footer">
        <!-- ... tương tự Students/Index ... -->
        <p>Trang @Model.PageIndex / @Model.TotalPages (Tổng: @Model.TotalCount giáo viên)</p>
    </div>
}
```

---

#### 6. `Views/Grades/Index.cshtml`

##### a) Model Declaration
```razor
@model StudentManagementSystem.Models.PaginatedList<StudentManagementSystem.Models.Grade>
```

##### b) Pagination UI
**Tương tự Students/Index, nhưng chỉ bảo toàn:**
- `classId`
- `courseId`

```razor
@if (Model.TotalPages > 1)
{
    <div class="card-footer">
        <nav aria-label="Page navigation">
            <ul class="pagination justify-content-center mb-0">
                <li class="page-item @(Model.HasPreviousPage ? "" : "disabled")">
                    <a class="page-link" 
                       asp-action="Index" 
                       asp-route-pageNumber="@(Model.PageIndex - 1)"
                       asp-route-classId="@ViewData["CurrentClass"]"
                       asp-route-courseId="@ViewData["CurrentCourse"]">
                        <i class="bi bi-chevron-left"></i> Trước
                    </a>
                </li>

                @for (int i = 1; i <= Model.TotalPages; i++)
                {
                    <li class="page-item @(i == Model.PageIndex ? "active" : "")">
                        <a class="page-link" 
                           asp-action="Index" 
                           asp-route-pageNumber="@i"
                           asp-route-classId="@ViewData["CurrentClass"]"
                           asp-route-courseId="@ViewData["CurrentCourse"]">
                            @i
                        </a>
                    </li>
                }

                <li class="page-item @(Model.HasNextPage ? "" : "disabled")">
                    <a class="page-link" 
                       asp-action="Index" 
                       asp-route-pageNumber="@(Model.PageIndex + 1)"
                       asp-route-classId="@ViewData["CurrentClass"]"
                       asp-route-courseId="@ViewData["CurrentCourse"]">
                        Sau <i class="bi bi-chevron-right"></i>
                    </a>
                </li>
            </ul>
        </nav>
        <p class="text-center text-muted mt-2 mb-0">
            Trang @Model.PageIndex / @Model.TotalPages (Tổng: @Model.TotalCount điểm)
        </p>
    </div>
}
```

---

## Bảo Mật & Validation

### 1. Teacher CRUD Validation - 3 Tầng Bảo Vệ

#### Tầng 1: Authorization Attribute
```csharp
[AuthorizeRole("Admin", "Teacher")]
```
- Chặn truy cập từ vai trò không hợp lệ
- Redirect về Login nếu chưa đăng nhập
- Redirect về AccessDenied nếu không có quyền

#### Tầng 2: Class Ownership Validation
```csharp
var isTeacherClass = await _context.Classes
    .AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);
    
if (!isTeacherClass)
{
    return RedirectToAction(nameof(AccessDenied));
}
```
- Kiểm tra Teacher có sở hữu lớp không
- Query cả `ClassId` và `TeacherId` để đảm bảo quyền sở hữu
- Chặn Teacher truy cập sinh viên của Teacher khác

#### Tầng 3: UI-Level Filtering
```csharp
if (userRole == "Teacher")
{
    ViewData["ClassId"] = new SelectList(
        await _context.Classes.Where(c => c.TeacherId == userId).ToListAsync(),
        "ClassId", "ClassName");
}
```
- Teacher chỉ thấy các lớp của mình trong dropdown
- Ngăn chặn việc chọn lớp không hợp lệ từ đầu
- Tối ưu UX (không hiển thị option không thể chọn)

### 2. Student Self-Edit Protection
```csharp
if (userRole == "Student")
{
    if (student.ClassId != oldStudent.ClassId)
    {
        ModelState.AddModelError("ClassId", "Sinh viên không thể tự thay đổi lớp học.");
        // ... return view với error
    }
}
```
- Sinh viên không thể tự chuyển lớp
- Chỉ có thể sửa thông tin cá nhân (Tên, SĐT, Địa chỉ, etc.)

---

## Testing Checklist

### A. Teacher CRUD Permissions

#### Test Case 1: Teacher Create Student
1. **Setup:**
   - Login với tài khoản Teacher (Username: `GV001`, Password: `123456`)
   - Truy cập `/Students/Index`

2. **Expected Results:**
   - ✅ Thấy nút "Thêm sinh viên mới"
   - ✅ Click vào → Form Create hiện ra
   - ✅ Dropdown "Lớp học" chỉ hiển thị các lớp Teacher đang dạy
   - ✅ Có thể submit form thành công với lớp hợp lệ

3. **Security Test:**
   - ❌ Nếu modify form HTML để chọn lớp khác → Submit → Redirect to AccessDenied

#### Test Case 2: Teacher Edit Student
1. **Setup:**
   - Login Teacher, click "Sửa" trên sinh viên trong lớp của mình

2. **Expected Results:**
   - ✅ Form Edit hiện ra với thông tin sinh viên
   - ✅ Dropdown "Lớp học" chỉ hiển thị các lớp Teacher đang dạy
   - ✅ Có thể sửa thông tin và submit thành công

3. **Security Test:**
   - ❌ Thử truy cập `/Students/Edit/SV999` (sinh viên lớp khác) → AccessDenied
   - ❌ Thử chuyển sinh viên sang lớp khác (modify HTML) → AccessDenied

#### Test Case 3: Teacher Delete Student
1. **Setup:**
   - Login Teacher, click "Xóa" trên sinh viên trong lớp của mình

2. **Expected Results:**
   - ✅ Trang Delete confirmation hiện ra
   - ✅ Click "Xóa" → Xóa thành công

3. **Security Test:**
   - ❌ Thử truy cập `/Students/Delete/SV999` (sinh viên lớp khác) → AccessDenied

#### Test Case 4: Cross-Teacher Access
1. **Setup:**
   - Teacher A login, lấy URL Edit/Delete của sinh viên của Teacher B
   - Teacher A thử truy cập URL đó

2. **Expected Results:**
   - ❌ Redirect to AccessDenied

---

### B. Pagination Testing

#### Test Case 1: Students Pagination
1. **Setup:**
   - Database có > 10 sinh viên
   - Truy cập `/Students/Index`

2. **Expected Results:**
   - ✅ Chỉ hiển thị 10 sinh viên đầu tiên
   - ✅ Có pagination controls ở cuối bảng
   - ✅ Hiển thị "Trang 1 / X (Tổng: Y sinh viên)"
   - ✅ Nút "Trước" bị disable (trang 1)
   - ✅ Nút "Sau" active (có trang sau)

3. **Navigation Test:**
   - ✅ Click trang 2 → URL có `?pageNumber=2`
   - ✅ Hiển thị sinh viên 11-20
   - ✅ Trang 2 được highlight
   - ✅ Nút "Trước" active, "Sau" phụ thuộc vào tổng số trang

4. **Filter Preservation Test:**
   - ✅ Tìm kiếm "Nguyen" → Chuyển trang 2 → Vẫn giữ filter "Nguyen"
   - ✅ Lọc theo lớp → Chuyển trang → Vẫn giữ lọc lớp
   - ✅ URL: `?pageNumber=2&searchString=Nguyen&classId=5`

#### Test Case 2: Teachers Pagination
1. **Setup:**
   - Database có > 10 giáo viên

2. **Expected Results:**
   - ✅ Tương tự Students, pageSize = 10
   - ✅ Bảo toàn: `searchString`, `departmentId`

#### Test Case 3: Grades Pagination
1. **Setup:**
   - Database có > 15 điểm

2. **Expected Results:**
   - ✅ Tương tự Students, pageSize = 15
   - ✅ Bảo toàn: `classId`, `courseId`

---

## Performance Optimization

### 1. Database Query Optimization
```csharp
// Sử dụng Skip/Take thay vì ToList() toàn bộ
var items = await source.Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
```
**Lợi ích:**
- Chỉ query số lượng records cần thiết
- Giảm memory usage
- Faster response time với dataset lớn

### 2. Count Query Optimization
```csharp
var count = await source.CountAsync();
```
**Lợi ích:**
- SQL Server optimize count query (không load data)
- Async operation (không block thread)

### 3. OrderBy for Consistent Pagination
```csharp
// Students
gradesQuery.OrderBy(s => s.StudentId)

// Grades
gradesQuery.OrderBy(g => g.StudentId).ThenBy(g => g.CourseId)
```
**Lợi ích:**
- Kết quả pagination nhất quán giữa các lần load
- Tránh duplicate/missing records khi data thay đổi

---

## UI/UX Enhancements

### 1. Responsive Pagination
```html
<ul class="pagination justify-content-center mb-0">
```
- Tự động center trên mọi màn hình
- Bootstrap responsive classes

### 2. Visual Feedback
```html
<li class="page-item active">  <!-- Current page -->
<li class="page-item disabled">  <!-- Can't navigate -->
```
- Highlight trang hiện tại
- Disable các nút không thể click

### 3. Information Display
```html
Trang @Model.PageIndex / @Model.TotalPages (Tổng: @Model.TotalCount sinh viên)
```
- User biết vị trí hiện tại
- User biết tổng số records

### 4. Icon Usage
```html
<i class="bi bi-chevron-left"></i> Trước
Sau <i class="bi bi-chevron-right"></i>
```
- Bootstrap Icons cho Previous/Next
- Cải thiện visual clarity

---

## Migration Notes

### Database
- **Không cần migration** (không thay đổi schema)
- Chỉ thay đổi logic layer

### Backward Compatibility
- ✅ `pageNumber` parameter là optional (`int?`)
- ✅ Default value: `pageNumber ?? 1` (trang 1)
- ✅ URLs cũ vẫn hoạt động (tự động hiển thị trang 1)

### Production Deployment
1. **Backup database** (best practice)
2. **Deploy code changes**
3. **Test với production data:**
   - Verify pagination với large datasets
   - Check Teacher permissions với real accounts
4. **Monitor logs** cho AccessDenied attempts

---

## Summary

### Thay Đổi Chính
1. ✅ **Teacher CRUD for Students**: 3-layer security validation
2. ✅ **Pagination**: 3 Index views (Students, Teachers, Grades)
3. ✅ **Filter Preservation**: Automatic via asp-route-* attributes
4. ✅ **UI Consistency**: Bootstrap 5 pagination design

### Files Modified
- **1 file created**: `Models/PaginatedList.cs`
- **3 controllers modified**: StudentsController, TeachersController, GradesController
- **3 views modified**: Students/Index, Teachers/Index, Grades/Index

### Performance Impact
- ✅ **Improved**: Database queries (Skip/Take thay vì load all)
- ✅ **Improved**: Page load time (less data transferred)
- ✅ **No regression**: Existing features không bị ảnh hưởng

### Security Posture
- ✅ **Enhanced**: Teacher permissions có validation chặt chẽ
- ✅ **Maintained**: Student self-edit restrictions vẫn hoạt động
- ✅ **Improved**: UI-level filtering giảm attack surface

---

## Quick Start Testing Commands

```bash
# Build project
dotnet build

# Run application
dotnet run

# Test URLs
http://localhost:5298/Students/Index
http://localhost:5298/Students/Index?pageNumber=2
http://localhost:5298/Students/Index?pageNumber=2&searchString=Nguyen&classId=5
http://localhost:5298/Teachers/Index?pageNumber=2
http://localhost:5298/Grades/Index?pageNumber=2&classId=5

# Teacher Login Credentials (from previous context)
Username: GV001
Password: 123456
```

---

## Support

Nếu gặp vấn đề:
1. Kiểm tra logs trong terminal
2. Verify database connection
3. Check session values (UserId, Role)
4. Test với Admin account trước để isolate permission issues
