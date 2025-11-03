using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Filters;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using System.Linq;

namespace StudentManagementSystem.Controllers
{
    // NOTE: This MVC Controller is DEPRECATED
    // Use Angular component at /students instead
    // This controller only serves old Razor Views for backward compatibility
    public class StudentsController : Controller
{
        private readonly ApplicationDbContext _context;
     private readonly IExportService _exportService;
        private readonly IStudentService _studentService;

public StudentsController(ApplicationDbContext context, IExportService exportService, IStudentService studentService)
        {
    _context = context;
      _exportService = exportService;
            _studentService = studentService;
     }

        // Redirect all MVC routes to Angular SPA
        [HttpGet]
        public IActionResult RedirectToAngular()
        {
            // Redirect to Angular route
            return Redirect("/#/students");
        }

        // GET: Students
  [AuthorizeRole("Admin", "Teacher")]
 public async Task<IActionResult> Index(string searchString, string classId, string departmentId, int? pageNumber)
        {
      var userRole = HttpContext.Session.GetString("UserRole");
       var userId = HttpContext.Session.GetString("UserId");

            // Use Stored Procedure via StudentService
            int pageSize = 10;
            var result = await _studentService.GetStudentsAsync(
                userRole ?? "Student",
                userId ?? "",
                searchString,
                classId,
                departmentId,
                pageNumber ?? 1,
                pageSize
            );
            
            List<Student> students = result.Students;
            int totalCount = result.TotalCount;

            // Create PaginatedList from SP results
            var paginatedList = new PaginatedList<Student>(
                students,
                totalCount,
                pageNumber ?? 1,
                pageSize
            );

            // Preserve filter values for view
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentClass"] = classId;
            ViewData["CurrentDepartment"] = departmentId;

            // Load dropdowns for filters
            ViewData["Classes"] = new SelectList(await _context.Classes.ToListAsync(), "ClassId", "ClassName");
            ViewData["Departments"] = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName");

            return View(paginatedList);
      }

 // GET: Students/Details/5
 [AuthorizeRole("Admin", "Teacher", "Student")]
 public async Task<IActionResult> Details(string id)
     {
 if (id == null)
       {
   return NotFound();
   }

      var userRole = HttpContext.Session.GetString("UserRole");
 var userId = HttpContext.Session.GetString("UserId");

            // Student can only view their own details
  if (userRole == "Student" && id != userId)
      {
    return RedirectToAction("AccessDenied", "Account");
            }

   var student = await _context.Students
  .Include(s => s.Class)
           .ThenInclude(c => c.Department)
   .Include(s => s.Grades)
  .ThenInclude(g => g.Course)
   .FirstOrDefaultAsync(m => m.StudentId == id);

 if (student == null)
        {
  return NotFound();
    }

  return View(student);
 }

   // GET: Students/Create
    [AuthorizeRole("Admin", "Teacher")]
   public IActionResult Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Teacher can only add students to their classes
            if (userRole == "Teacher")
            {
                var teacherClasses = _context.Classes
                    .Include(c => c.Department)
                    .Where(c => c.TeacherId == userId)
                    .ToList();
                
                if (!teacherClasses.Any())
                {
                    TempData["ErrorMessage"] = "Bạn chưa được phân công lớp chủ nhiệm";
                    return RedirectToAction(nameof(Index));
                }
                
                ViewData["ClassId"] = new SelectList(teacherClasses, "ClassId", "ClassName");
            }
            else
            {
                ViewData["ClassId"] = new SelectList(_context.Classes.Include(c => c.Department), "ClassId", "ClassName");
            }
            
            return View();
        }

  // POST: Students/Create
   [HttpPost]
    [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Teacher")]
   public async Task<IActionResult> Create([Bind("StudentId,FullName,DateOfBirth,Gender,Phone,Address,ClassId,Username,Password")] Student student)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Teacher can only add students to their classes
            if (userRole == "Teacher")
            {
                var isTeacherClass = await _context.Classes.AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);
                if (!isTeacherClass)
                {
                    TempData["ErrorMessage"] = "Bạn chỉ có thể thêm sinh viên vào lớp mình chủ nhiệm";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ModelState.IsValid)
            {
                // Use Stored Procedure via StudentService
                bool success = await _studentService.CreateStudentAsync(student, userRole ?? "Student", userId ?? "");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm sinh viên thành công";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Mã sinh viên hoặc tên đăng nhập đã tồn tại hoặc không có quyền thêm");
                }
            }
            
            // Reload classes based on role
            if (userRole == "Teacher")
            {
                var teacherClasses = _context.Classes.Include(c => c.Department).Where(c => c.TeacherId == userId);
                ViewData["ClassId"] = new SelectList(teacherClasses, "ClassId", "ClassName", student.ClassId);
            }
            else
            {
                ViewData["ClassId"] = new SelectList(_context.Classes.Include(c => c.Department), "ClassId", "ClassName", student.ClassId);
            }
            
            return View(student);
        }

  // GET: Students/Edit/5
   [AuthorizeRole("Admin", "Teacher", "Student")]
  public async Task<IActionResult> Edit(string id)
  {
 if (id == null)
         {
      return NotFound();
   }

   var userRole = HttpContext.Session.GetString("UserRole");
  var userId = HttpContext.Session.GetString("UserId");

    // Student can only edit their own info (limited fields)
  if (userRole == "Student" && id != userId)
   {
      return RedirectToAction("AccessDenied", "Account");
            }

      var student = await _context.Students.Include(s => s.Class).FirstOrDefaultAsync(s => s.StudentId == id);
  if (student == null)
  {
   return NotFound();
   }

            // Teacher can only edit students in their classes
            if (userRole == "Teacher")
            {
                var isTeacherClass = await _context.Classes.AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);
                if (!isTeacherClass)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
                
                var teacherClasses = _context.Classes.Include(c => c.Department).Where(c => c.TeacherId == userId);
                ViewData["ClassId"] = new SelectList(teacherClasses, "ClassId", "ClassName", student.ClassId);
            }
            else
            {
                ViewData["ClassId"] = new SelectList(_context.Classes.Include(c => c.Department), "ClassId", "ClassName", student.ClassId);
            }
    
       return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
  [ValidateAntiForgeryToken]
 [AuthorizeRole("Admin", "Teacher", "Student")]
        public async Task<IActionResult> Edit(string id, [Bind("StudentId,FullName,DateOfBirth,Gender,Phone,Address,ClassId")] Student student)
        {
if (id != student.StudentId)
   {
       return NotFound();
 }

       var userRole = HttpContext.Session.GetString("UserRole");
      var userId = HttpContext.Session.GetString("UserId");

   // Student can only edit their own info
  if (userRole == "Student" && id != userId)
  {
       return RedirectToAction("AccessDenied", "Account");
   }

            // Teacher can only edit students in their classes
            if (userRole == "Teacher")
            {
                var existingStudent = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == id);
                if (existingStudent == null)
                {
                    return NotFound();
                }

                var isTeacherClass = await _context.Classes.AnyAsync(c => c.ClassId == existingStudent.ClassId && c.TeacherId == userId);
                if (!isTeacherClass)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Teacher cannot change student to different class (unless it's also their class)
                if (student.ClassId != existingStudent.ClassId)
                {
                    var isNewClassTeacher = await _context.Classes.AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);
                    if (!isNewClassTeacher)
                    {
                        ModelState.AddModelError("ClassId", "Bạn chỉ có thể chuyển sinh viên sang lớp mình phụ trách");
                        var teacherClasses = _context.Classes.Include(c => c.Department).Where(c => c.TeacherId == userId);
                        ViewData["ClassId"] = new SelectList(teacherClasses, "ClassId", "ClassName", student.ClassId);
                        return View(student);
                    }
                }
            }

       if (ModelState.IsValid)
       {
            // Get existing student to preserve Username and Password
            var existingStudent = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == id);
            if (existingStudent == null)
            {
                return NotFound();
            }

            // Preserve Username and Password
            student.Username = existingStudent.Username;
            student.Password = existingStudent.Password;

            // Use Stored Procedure via StudentService
            bool success = await _studentService.UpdateStudentAsync(student, userRole ?? "Student", userId ?? "");
            
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật sinh viên thành công";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Không thể cập nhật sinh viên. Kiểm tra quyền hoặc dữ liệu đầu vào.");
            }
       }
       
    ViewData["ClassId"] = new SelectList(_context.Classes.Include(c => c.Department), "ClassId", "ClassName", student.ClassId);
    return View(student);
        }

// GET: Students/Delete/5
   [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Delete(string id)
        {
if (id == null)
     {
   return NotFound();
}

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

 var student = await _context.Students
    .Include(s => s.Class)
        .FirstOrDefaultAsync(m => m.StudentId == id);
       
       if (student == null)
    {
 return NotFound();
       }

            // Teacher can only delete students in their classes
            if (userRole == "Teacher")
            {
                var isTeacherClass = await _context.Classes.AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);
                if (!isTeacherClass)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }

  return View(student);
        }

 // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
 [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> DeleteConfirmed(string id)
 {
            var userRole = HttpContext.Session.GetString("UserRole");

            // Use Stored Procedure via StudentService
            bool success = await _studentService.DeleteStudentAsync(id, userRole ?? "Student");
            
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa sinh viên thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa sinh viên này. Có thể do có dữ liệu điểm liên quan hoặc không có quyền.";
            }
            
            return RedirectToAction(nameof(Index));
 }

     // Export to Excel
   [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> ExportToExcel(string searchString, string classId, string departmentId)
 {
   var userRole = HttpContext.Session.GetString("UserRole");
var userId = HttpContext.Session.GetString("UserId");

 var studentsQuery = _context.Students
     .Include(s => s.Class)
    .AsQueryable();

    if (userRole == "Teacher")
       {
   var teacherClassIds = await _context.Classes
         .Where(c => c.TeacherId == userId)
     .Select(c => c.ClassId)
   .ToListAsync();

studentsQuery = studentsQuery.Where(s => teacherClassIds.Contains(s.ClassId));
    }

   if (!string.IsNullOrEmpty(searchString))
     {
     studentsQuery = studentsQuery.Where(s => s.FullName.Contains(searchString) || s.StudentId.Contains(searchString));
       }

if (!string.IsNullOrEmpty(classId))
       {
   studentsQuery = studentsQuery.Where(s => s.ClassId == classId);
       }

       if (!string.IsNullOrEmpty(departmentId))
      {
   studentsQuery = studentsQuery.Where(s => s.Class != null && s.Class.DepartmentId == departmentId);
         }

   var students = await studentsQuery.ToListAsync();
       var fileContent = _exportService.ExportStudentsToExcel(students);

  return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
              $"DanhSachSinhVien_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        // GET: Students/EditProfile - Student can edit their own profile (limited)
        [AuthorizeRole("Student")]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.Department)
                .FirstOrDefaultAsync(s => s.StudentId == userId);
            
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Student")]
        public async Task<IActionResult> EditProfile([Bind("StudentId,FullName,Phone,Address,ClassId")] Student student, string? currentPassword, string? newPassword)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (student.StudentId != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var existingStudent = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == student.StudentId);
            if (existingStudent == null)
            {
                return NotFound();
            }

            // Keep the existing fields that student cannot change
            student.Username = existingStudent.Username;
            student.Password = existingStudent.Password;
            student.DateOfBirth = existingStudent.DateOfBirth;
            student.Gender = existingStudent.Gender;
            student.ClassId = existingStudent.ClassId; // Student cannot change class

            // If changing password
            if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
            {
                if (existingStudent.Password == currentPassword)
                {
                    student.Password = newPassword;
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công";
                    return RedirectToAction("Index", "Dashboard");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Có lỗi khi cập nhật thông tin");
                }
            }

            // Reload class info for display
            student = await _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.Department)
                .FirstOrDefaultAsync(s => s.StudentId == student.StudentId);

            return View(student);
        }

        private bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }

        // ==================== EXPORT METHODS ====================

        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> ExportExcel(string searchString, string classId, string departmentId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            var entityId = HttpContext.Session.GetString("EntityId");  // ✅ Get EntityId

            var studentsQuery = _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.Department)
                .AsQueryable();

            // Teacher can only export students from their classes
            if (userRole == "Teacher")
            {
                var teacherClassIds = await _context.Classes
                    .Where(c => c.TeacherId == entityId)  // ✅ Use EntityId
                    .Select(c => c.ClassId)
                    .ToListAsync();
                studentsQuery = studentsQuery.Where(s => teacherClassIds.Contains(s.ClassId));
            }

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s =>
                    s.FullName.Contains(searchString) ||
                    s.StudentId.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(classId))
            {
                studentsQuery = studentsQuery.Where(s => s.ClassId == classId);
            }

            if (!string.IsNullOrEmpty(departmentId))
            {
                studentsQuery = studentsQuery.Where(s => s.Class!.DepartmentId == departmentId);
            }

            var students = await studentsQuery.ToListAsync();
            var fileBytes = _exportService.ExportStudentsToExcel(students);

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> ExportPdf(string searchString, string classId, string departmentId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            var entityId = HttpContext.Session.GetString("EntityId");  // ✅ Get EntityId

            var studentsQuery = _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.Department)
                .AsQueryable();

            // Teacher can only export students from their classes
            if (userRole == "Teacher")
            {
                var teacherClassIds = await _context.Classes
                    .Where(c => c.TeacherId == entityId)  // ✅ Use EntityId
                    .Select(c => c.ClassId)
                    .ToListAsync();
                studentsQuery = studentsQuery.Where(s => teacherClassIds.Contains(s.ClassId));
            }

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s =>
                    s.FullName.Contains(searchString) ||
                    s.StudentId.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(classId))
            {
                studentsQuery = studentsQuery.Where(s => s.ClassId == classId);
            }

            if (!string.IsNullOrEmpty(departmentId))
            {
                studentsQuery = studentsQuery.Where(s => s.Class!.DepartmentId == departmentId);
            }

            var students = await studentsQuery.ToListAsync();
            var fileBytes = _exportService.ExportStudentsToPdf(students);

            return File(fileBytes,
                "application/pdf",
                $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}
