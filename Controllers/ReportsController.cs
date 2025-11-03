using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Filters;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public ReportsController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        // GET: Reports Dashboard
        [AuthorizeRole("Admin", "Teacher")]
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");  // ✅ Changed from "Role" to "UserRole"
            ViewData["UserRole"] = userRole;
            
            // Populate dropdowns for filters
            ViewData["Departments"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["Classes"] = new SelectList(_context.Classes, "ClassId", "ClassName");
            ViewData["Courses"] = new SelectList(_context.Courses, "CourseId", "CourseName");
            
            if (userRole == "Teacher")
            {
                var userId = HttpContext.Session.GetString("UserId");
                ViewData["Classes"] = new SelectList(
                    _context.Classes.Where(c => c.TeacherId == userId),
                    "ClassId", "ClassName");
                ViewData["Courses"] = new SelectList(
                    _context.Courses.Where(c => c.TeacherId == userId),
                    "CourseId", "CourseName");
            }
            
            return View();
        }

        // ==================== CLASS REPORT ====================

        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> ExportClassReportExcel(string classId)
        {
            if (string.IsNullOrEmpty(classId))
            {
                return BadRequest("Class ID is required");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            var entityId = HttpContext.Session.GetString("EntityId");  // ✅ Get EntityId

            // Get class info
            var classInfo = await _context.Classes
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (classInfo == null)
            {
                return NotFound("Class not found");
            }

            // Verify teacher permission
            if (userRole == "Teacher" && classInfo.TeacherId != entityId)  // ✅ Use EntityId
            {
                return Forbid();
            }

            // Get students in class
            var students = await _context.Students
                .Where(s => s.ClassId == classId)
                .OrderBy(s => s.StudentId)
                .ToListAsync();

            // Get grades for each student
            var studentIds = students.Select(s => s.StudentId).ToList();
            var grades = await _context.Grades
                .Include(g => g.Course)
                .Where(g => studentIds.Contains(g.StudentId))
                .ToListAsync();

            var studentGrades = grades.GroupBy(g => g.StudentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var fileBytes = _exportService.ExportClassReportToExcel(
                classInfo.ClassId,
                classInfo.ClassName,
                students,
                studentGrades);

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCaoLop_{classInfo.ClassName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> ExportClassReportPdf(string classId)
        {
            if (string.IsNullOrEmpty(classId))
            {
                return BadRequest("Class ID is required");
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            var entityId = HttpContext.Session.GetString("EntityId");  // ✅ Get EntityId

            // Get class info
            var classInfo = await _context.Classes
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (classInfo == null)
            {
                return NotFound("Class not found");
            }

            // Verify teacher permission
            if (userRole == "Teacher" && classInfo.TeacherId != entityId)  // ✅ Use EntityId
            {
                return Forbid();
            }

            // Get students in class
            var students = await _context.Students
                .Where(s => s.ClassId == classId)
                .OrderBy(s => s.StudentId)
                .ToListAsync();

            // Get grades for each student
            var studentIds = students.Select(s => s.StudentId).ToList();
            var grades = await _context.Grades
                .Include(g => g.Course)
                .Where(g => studentIds.Contains(g.StudentId))
                .ToListAsync();

            var studentGrades = grades.GroupBy(g => g.StudentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var fileBytes = _exportService.ExportClassReportToPdf(
                classInfo.ClassId,
                classInfo.ClassName,
                students,
                studentGrades);

            return File(fileBytes,
                "application/pdf",
                $"BaoCaoLop_{classInfo.ClassName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        // ==================== DEPARTMENT REPORT ====================

        [AuthorizeRole("Admin")]
        public async Task<IActionResult> ExportDepartmentReportExcel(string departmentId)
        {
            if (string.IsNullOrEmpty(departmentId))
            {
                return BadRequest("Department ID is required");
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);

            if (department == null)
            {
                return NotFound("Department not found");
            }

            var classes = await _context.Classes
                .Include(c => c.Teacher)
                .Where(c => c.DepartmentId == departmentId)
                .OrderBy(c => c.ClassId)
                .ToListAsync();

            // Count students in each class
            var studentCounts = new Dictionary<string, int>();
            foreach (var cls in classes)
            {
                var count = await _context.Students.CountAsync(s => s.ClassId == cls.ClassId);
                studentCounts[cls.ClassId] = count;
            }

            var fileBytes = _exportService.ExportDepartmentReportToExcel(
                department.DepartmentName,
                classes,
                studentCounts);

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCaoKhoa_{department.DepartmentName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [AuthorizeRole("Admin")]
        public async Task<IActionResult> ExportDepartmentReportPdf(string departmentId)
        {
            if (string.IsNullOrEmpty(departmentId))
            {
                return BadRequest("Department ID is required");
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);

            if (department == null)
            {
                return NotFound("Department not found");
            }

            var classes = await _context.Classes
                .Include(c => c.Teacher)
                .Where(c => c.DepartmentId == departmentId)
                .OrderBy(c => c.ClassId)
                .ToListAsync();

            // Count students in each class
            var studentCounts = new Dictionary<string, int>();
            foreach (var cls in classes)
            {
                var count = await _context.Students.CountAsync(s => s.ClassId == cls.ClassId);
                studentCounts[cls.ClassId] = count;
            }

            var fileBytes = _exportService.ExportDepartmentReportToPdf(
                department.DepartmentName,
                classes,
                studentCounts);

            return File(fileBytes,
                "application/pdf",
                $"BaoCaoKhoa_{department.DepartmentName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        // ==================== TEACHER REPORT ====================

        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> ExportTeacherReportExcel(string teacherId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            var entityId = HttpContext.Session.GetString("EntityId");  // ✅ Get EntityId

            // If teacher role, can only export their own report
            if (userRole == "Teacher")
            {
                teacherId = entityId ?? teacherId;  // ✅ Use EntityId
            }

            if (string.IsNullOrEmpty(teacherId))
            {
                return BadRequest("Teacher ID is required");
            }

            var teacher = await _context.Teachers
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
            {
                return NotFound("Teacher not found");
            }

            var classes = await _context.Classes
                .Include(c => c.Department)
                .Where(c => c.TeacherId == teacherId)
                .OrderBy(c => c.ClassId)
                .ToListAsync();

            var courses = await _context.Courses
                .Include(c => c.Department)
                .Where(c => c.TeacherId == teacherId)
                .OrderBy(c => c.CourseId)
                .ToListAsync();

            var fileBytes = _exportService.ExportTeacherReportToExcel(
                teacher.FullName,
                classes,
                courses);

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BaoCaoGiaoVien_{teacher.FullName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> ExportTeacherReportPdf(string teacherId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            var entityId = HttpContext.Session.GetString("EntityId");  // ✅ Get EntityId

            // If teacher role, can only export their own report
            if (userRole == "Teacher")
            {
                teacherId = entityId ?? teacherId;  // ✅ Use EntityId
            }

            if (string.IsNullOrEmpty(teacherId))
            {
                return BadRequest("Teacher ID is required");
            }

            var teacher = await _context.Teachers
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
            {
                return NotFound("Teacher not found");
            }

            var classes = await _context.Classes
                .Include(c => c.Department)
                .Where(c => c.TeacherId == teacherId)
                .OrderBy(c => c.ClassId)
                .ToListAsync();

            var courses = await _context.Courses
                .Include(c => c.Department)
                .Where(c => c.TeacherId == teacherId)
                .OrderBy(c => c.CourseId)
                .ToListAsync();

            var fileBytes = _exportService.ExportTeacherReportToPdf(
                teacher.FullName,
                classes,
                courses);

            return File(fileBytes,
                "application/pdf",
                $"BaoCaoGiaoVien_{teacher.FullName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}
