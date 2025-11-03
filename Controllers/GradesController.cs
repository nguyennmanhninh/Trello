using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Filters;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers
{
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;
        private readonly IGradeService _gradeService;

        public GradesController(ApplicationDbContext context, IExportService exportService, IGradeService gradeService)
        {
            _context = context;
            _exportService = exportService;
            _gradeService = gradeService;
        }

        // GET: Grades
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Index(string classId, string courseId, int? pageNumber)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Use Stored Procedure via GradeService
            int pageSize = 15;
            var result = await _gradeService.GetGradesAsync(
                userRole ?? "Admin", 
                userId ?? "", 
                null,      // studentId
                courseId,  // courseId filter
                classId,   // classId filter
                pageNumber ?? 1, 
                pageSize
            );

            var paginatedList = new PaginatedList<Grade>(result.Grades, result.TotalCount, pageNumber ?? 1, pageSize);

            // Set ViewData for filters
            if (!string.IsNullOrEmpty(classId))
            {
                ViewData["CurrentClass"] = classId;
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                ViewData["CurrentCourse"] = courseId;
            }

            ViewData["Classes"] = new SelectList(await _context.Classes.ToListAsync(), "ClassId", "ClassName");
            ViewData["Courses"] = new SelectList(await _context.Courses.ToListAsync(), "CourseId", "CourseName");

            return View(paginatedList);
        }

        // GET: Grades/Create
   [AuthorizeRole("Admin", "Teacher")]
    public IActionResult Create()
        {
       var userRole = HttpContext.Session.GetString("UserRole");
 var userId = HttpContext.Session.GetString("UserId");

   if (userRole == "Teacher")
  {
      // Teacher can only add grades for students in their classes
       var teacherClasses = _context.Classes.Where(c => c.TeacherId == userId);

 var students = _context.Students
     .Where(s => teacherClasses.Any(tc => tc.ClassId == s.ClassId))
   .ToList();

       var courses = _context.Courses
 .Where(c => c.TeacherId == userId)
      .ToList();

        ViewData["StudentId"] = new SelectList(students, "StudentId", "FullName");
      ViewData["CourseId"] = new SelectList(courses, "CourseId", "CourseName");
            }
 else
    {
      ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName");
   ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName");
    }

   return View();
   }

        // POST: Grades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Create([Bind("StudentId,CourseId,Score")] Grade grade)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (ModelState.IsValid)
            {
                // Auto-calculate classification (will be done by SP using fn_CalculateClassification)
                grade.Classification = CalculateClassification(grade.Score);

                // Use Stored Procedure via GradeService
                bool success = await _gradeService.CreateGradeAsync(grade, userRole ?? "Admin", userId ?? "");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm điểm thành công";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Không thể thêm điểm. Điểm cho sinh viên và môn học này có thể đã tồn tại.");
                }
            }

            if (userRole == "Teacher")
            {
                var teacherClassIds = _context.Classes
                    .Where(c => c.TeacherId == userId)
                    .Select(c => c.ClassId)
                    .ToList();

                var students = _context.Students
                    .Where(s => teacherClassIds.Contains(s.ClassId))
                    .ToList();

                var courses = _context.Courses
                    .Where(c => c.TeacherId == userId)
                    .ToList();

                ViewData["StudentId"] = new SelectList(students, "StudentId", "FullName", grade.StudentId);
                ViewData["CourseId"] = new SelectList(courses, "CourseId", "CourseName", grade.CourseId);
            }
            else
            {
                ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);
                ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", grade.CourseId);
            }

            return View(grade);
        }

   // GET: Grades/Edit/5
 [AuthorizeRole("Admin", "Teacher")]
    public async Task<IActionResult> Edit(string studentId, string courseId)
 {
   if (studentId == null || courseId == null)
    {
 return NotFound();
   }

    var grade = await _context.Grades
    .Include(g => g.Student)
 .Include(g => g.Course)
     .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);

   if (grade == null)
    {
   return NotFound();
   }

   ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);
     ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", grade.CourseId);
return View(grade);
 }

        // POST: Grades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Edit(string studentId, string courseId, [Bind("StudentId,CourseId,Score")] Grade grade)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (studentId != grade.StudentId || courseId != grade.CourseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Auto-calculate classification (will be done by SP using fn_CalculateClassification)
                grade.Classification = CalculateClassification(grade.Score);

                // Use Stored Procedure via GradeService
                bool success = await _gradeService.UpdateGradeAsync(grade, userRole ?? "Admin", userId ?? "");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật điểm thành công";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Không thể cập nhật điểm. Kiểm tra quyền hoặc dữ liệu đầu vào.");
                }
            }

            ViewData["StudentId"] = new SelectList(_context.Students, "StudentId", "FullName", grade.StudentId);
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "CourseName", grade.CourseId);
            return View(grade);
        }

   // GET: Grades/Delete/5
        [AuthorizeRole("Admin", "Teacher")]
 public async Task<IActionResult> Delete(string studentId, string courseId)
   {
       if (studentId == null || courseId == null)
  {
return NotFound();
   }

  var grade = await _context.Grades
      .Include(g => g.Student)
   .Include(g => g.Course)
    .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);

   if (grade == null)
   {
      return NotFound();
   }

   return View(grade);
  }

        // POST: Grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> DeleteConfirmed(string studentId, string courseId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Use Stored Procedure via GradeService
            bool success = await _gradeService.DeleteGradeAsync(studentId, courseId, userRole ?? "Admin");
            
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa điểm thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa điểm. Kiểm tra quyền hoặc dữ liệu.";
            }

            return RedirectToAction(nameof(Index));
        }

// Export to Excel
   [AuthorizeRole("Admin", "Teacher")]
public async Task<IActionResult> ExportToExcel(string classId, string courseId)
 {
    var userRole = HttpContext.Session.GetString("UserRole");
 var userId = HttpContext.Session.GetString("UserId");

       var gradesQuery = _context.Grades
    .Include(g => g.Student)
  .ThenInclude(s => s.Class)
   .Include(g => g.Course)
     .AsQueryable();

  if (userRole == "Teacher")
{
    var teacherClasses = _context.Classes.Where(c => c.TeacherId == userId);
    gradesQuery = gradesQuery.Where(g => teacherClasses.Any(tc => tc.ClassId == g.Student.ClassId));
     }

 if (!string.IsNullOrEmpty(classId))
 {
       gradesQuery = gradesQuery.Where(g => g.Student.ClassId == classId);
  }

       if (!string.IsNullOrEmpty(courseId))
  {
      gradesQuery = gradesQuery.Where(g => g.CourseId == courseId);
}

var grades = await gradesQuery.ToListAsync();
   var fileContent = _exportService.ExportGradesToExcel(grades);

return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
       $"BangDiem_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
   }

  // My Grades - for students and teachers to view their own grades
  [AuthorizeRole("Student", "Teacher")]
 public async Task<IActionResult> MyGrades()
        {
   var userId = HttpContext.Session.GetString("UserId");
    var userRole = HttpContext.Session.GetString("UserRole");

    // Get EntityId from Users table if Teacher
    var entityId = userId;
    if (userRole == "Teacher")
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
        if (user == null || string.IsNullOrEmpty(user.EntityId))
        {
            ViewBag.Message = "Bạn chưa được liên kết với tài khoản sinh viên";
            ViewBag.Student = null;
            ViewBag.AverageScore = 0;
            return View(new List<Grade>());
        }
        entityId = user.EntityId;
    }

     var grades = await _context.Grades
        .Include(g => g.Course)
    .ThenInclude(c => c.Department)
     .Where(g => g.StudentId == entityId)
   .ToListAsync();

   var student = await _context.Students
    .Include(s => s.Class)
    .FirstOrDefaultAsync(s => s.StudentId == entityId);

    ViewBag.Student = student;
   ViewBag.AverageScore = grades.Any() ? Math.Round(grades.Average(g => g.Score), 2) : 0;

return View(grades);
        }

        private bool GradeExists(string studentId, string courseId)
        {
            return _context.Grades.Any(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        private string CalculateClassification(decimal score)
        {
            if (score >= 9.0m) return "Xuất sắc";
            if (score >= 8.0m) return "Giỏi";
            if (score >= 7.0m) return "Khá";
            if (score >= 5.5m) return "Trung bình";
            if (score >= 4.0m) return "Yếu";
            return "Kém";
        }

        // ==================== EXPORT METHODS ====================

        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> ExportExcel(string classId, string courseId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            var entityId = HttpContext.Session.GetString("EntityId");  // ✅ Get EntityId

            var gradesQuery = _context.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s!.Class)
                .Include(g => g.Course)
                .AsQueryable();

            // Teacher filter
            if (userRole == "Teacher")
            {
                var teacherClassIds = await _context.Classes
                    .Where(c => c.TeacherId == entityId)  // ✅ Use EntityId
                    .Select(c => c.ClassId)
                    .ToListAsync();
                    
                gradesQuery = gradesQuery.Where(g => teacherClassIds.Contains(g.Student!.ClassId));
            }

            // Apply filters
            if (!string.IsNullOrEmpty(classId))
            {
                gradesQuery = gradesQuery.Where(g => g.Student!.ClassId == classId);
            }

            if (!string.IsNullOrEmpty(courseId))
            {
                gradesQuery = gradesQuery.Where(g => g.CourseId == courseId);
            }

            var grades = await gradesQuery.ToListAsync();
            var fileBytes = _exportService.ExportGradesToExcel(grades);

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BangDiem_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> ExportPdf(string classId, string courseId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            var entityId = HttpContext.Session.GetString("EntityId");  // ✅ Get EntityId

            var gradesQuery = _context.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s!.Class)
                .Include(g => g.Course)
                .AsQueryable();

            // Teacher filter
            if (userRole == "Teacher")
            {
                var teacherClassIds = await _context.Classes
                    .Where(c => c.TeacherId == entityId)  // ✅ Use EntityId
                    .Select(c => c.ClassId)
                    .ToListAsync();
                    
                gradesQuery = gradesQuery.Where(g => teacherClassIds.Contains(g.Student!.ClassId));
            }

            // Apply filters
            if (!string.IsNullOrEmpty(classId))
            {
                gradesQuery = gradesQuery.Where(g => g.Student!.ClassId == classId);
            }

            if (!string.IsNullOrEmpty(courseId))
            {
                gradesQuery = gradesQuery.Where(g => g.CourseId == courseId);
            }

            var grades = await gradesQuery.ToListAsync();
            var fileBytes = _exportService.ExportGradesToPdf(grades);

            return File(fileBytes,
                "application/pdf",
                $"BangDiem_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}
