using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Temporarily disabled for testing
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IStatisticsService _statisticsService;

        public DashboardController(ApplicationDbContext context, IStatisticsService statisticsService)
        {
            _context = context;
            _statisticsService = statisticsService;
        }

        // Admin Dashboard Stats
        [HttpGet("admin-stats")]
        public async Task<IActionResult> GetAdminStats()
        {
            try
            {
                var stats = new
                {
                    TotalStudents = await _statisticsService.GetTotalStudentsAsync(),
                    TotalTeachers = await _statisticsService.GetTotalTeachersAsync(),
                    TotalClasses = await _statisticsService.GetTotalClassesAsync(),
                    TotalCourses = await _statisticsService.GetTotalCoursesAsync(),
                    TotalDepartments = await _statisticsService.GetTotalDepartmentsAsync()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải dữ liệu dashboard admin", error = ex.Message });
            }
        }

        // Students by Class (for Admin)
        [HttpGet("students-by-class")]
        public async Task<IActionResult> GetStudentsByClass()
        {
            try
            {
                var result = await _statisticsService.GetStudentCountByClassAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải dữ liệu sinh viên theo lớp", error = ex.Message });
            }
        }

        // Students by Department (for Admin)
        [HttpGet("students-by-department")]
        public async Task<IActionResult> GetStudentsByDepartment()
        {
            try
            {
                var result = await _statisticsService.GetStudentCountByDepartmentAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải dữ liệu sinh viên theo khoa", error = ex.Message });
            }
        }

        // Teacher Dashboard Stats
        [HttpGet("teacher-stats")]
        public async Task<IActionResult> GetTeacherStats()
        {
            try
            {
                // Try to get username from JWT claims first, fallback to session
                var username = User.FindFirst("Username")?.Value 
                             ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                             ?? HttpContext.Session.GetString("UserId") 
                             ?? HttpContext.Session.GetString("Username");
                
                Console.WriteLine($"[DashboardController] Teacher stats request");
                Console.WriteLine($"[DashboardController] JWT Username: {User.FindFirst("Username")?.Value}");
                Console.WriteLine($"[DashboardController] JWT Name: {User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value}");
                Console.WriteLine($"[DashboardController] JWT UserId: {User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}");
                Console.WriteLine($"[DashboardController] Session UserId: {HttpContext.Session.GetString("UserId")}");
                Console.WriteLine($"[DashboardController] Session Username: {HttpContext.Session.GetString("Username")}");
                Console.WriteLine($"[DashboardController] Session UserRole: {HttpContext.Session.GetString("UserRole")}");
                Console.WriteLine($"[DashboardController] Resolved username: {username}");
                
                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine($"[DashboardController] ❌ No username found in JWT or session!");
                    return Unauthorized(new { message = "Không tìm thấy thông tin giảng viên" });
                }

                // Find teacher by username to get TeacherId
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == username);
                if (teacher == null)
                {
                    Console.WriteLine($"[DashboardController] ❌ Teacher not found with username: {username}");
                    return NotFound(new { message = "Không tìm thấy thông tin giảng viên" });
                }

                Console.WriteLine($"[DashboardController] ✅ Found teacher: {teacher.FullName} (ID: {teacher.TeacherId})");

                var teacherId = teacher.TeacherId;

                var teacherClasses = await _context.Classes
                    .Include(c => c.Department)
                    .Where(c => c.TeacherId == teacherId)
                    .Select(c => new
                    {
                        c.ClassId,
                        c.ClassName,
                        DepartmentName = c.Department != null ? c.Department.DepartmentName : "",
                        StudentCount = _context.Students.Count(s => s.ClassId == c.ClassId)
                    })
                    .ToListAsync();

                var teacherCourses = await _context.Courses
                    .Include(c => c.Department)
                    .Where(c => c.TeacherId == teacherId)
                    .Select(c => new
                    {
                        c.CourseId,
                        c.CourseName,
                        c.Credits,
                        DepartmentName = c.Department != null ? c.Department.DepartmentName : "",
                        StudentCount = _context.Grades.Count(g => g.CourseId == c.CourseId)
                    })
                    .ToListAsync();

                var result = new
                {
                    TeacherClasses = teacherClasses,
                    TeacherCourses = teacherCourses
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải dữ liệu dashboard giảng viên", error = ex.Message });
            }
        }

        // Student Dashboard Stats
        [HttpGet("student-stats")]
        public async Task<IActionResult> GetStudentStats()
        {
            try
            {
                // Try to get username from JWT claims first, fallback to session
                var username = User.FindFirst("Username")?.Value 
                             ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                             ?? HttpContext.Session.GetString("UserId") 
                             ?? HttpContext.Session.GetString("Username");
                
                Console.WriteLine($"[DashboardController] Student stats request");
                Console.WriteLine($"[DashboardController] JWT Username: {User.FindFirst("Username")?.Value}");
                Console.WriteLine($"[DashboardController] JWT UserId: {User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}");
                Console.WriteLine($"[DashboardController] Resolved username: {username}");
                
                if (string.IsNullOrEmpty(username))
                {
                    Console.WriteLine($"[DashboardController] ❌ No username found in JWT or session!");
                    return Unauthorized(new { message = "Không tìm thấy thông tin sinh viên" });
                }

                // Find student by username to get StudentId
                var studentRecord = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
                if (studentRecord == null)
                {
                    Console.WriteLine($"[DashboardController] ❌ Student not found with username: {username}");
                    return NotFound(new { message = "Không tìm thấy thông tin sinh viên" });
                }

                Console.WriteLine($"[DashboardController] ✅ Found student: {studentRecord.FullName} (ID: {studentRecord.StudentId})");

                var studentId = studentRecord.StudentId;

                var student = await _context.Students
                    .Include(s => s.Class!)
                    .ThenInclude(c => c.Department)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                var studentGrades = await _context.Grades
                    .Include(g => g.Course)
                    .Where(g => g.StudentId == studentId)
                    .Select(g => new
                    {
                        g.CourseId,
                        CourseName = g.Course != null ? g.Course.CourseName : "",
                        g.Score,
                        g.Classification,
                        Credits = g.Course != null ? g.Course.Credits : 0
                    })
                    .ToListAsync();

                var averageScore = studentGrades.Any() 
                    ? await _statisticsService.GetAverageScoreByStudentAsync(studentId)
                    : 0;

                var result = new
                {
                    StudentClass = student?.Class != null ? new
                    {
                        student.Class.ClassId,
                        student.Class.ClassName,
                        DepartmentName = student.Class.Department?.DepartmentName ?? ""
                    } : null,
                    StudentGrades = studentGrades,
                    AverageScore = averageScore
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải dữ liệu dashboard sinh viên", error = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = new
                {
                    totalStudents = await _context.Students.CountAsync(),
                    totalTeachers = await _context.Teachers.CountAsync(),
                    totalClasses = await _context.Classes.CountAsync(),
                    totalCourses = await _context.Courses.CountAsync(),
                    totalDepartments = await _context.Departments.CountAsync(),
                    gradeDistribution = await GetGradeDistribution(),
                    studentsByDepartment = await GetStudentsByDepartmentLegacy()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải dữ liệu dashboard", error = ex.Message });
            }
        }

        private async Task<List<object>> GetGradeDistribution()
        {
            var grades = await _context.Grades.Select(g => g.Score).ToListAsync();

            var distribution = new List<object>
            {
                new { gradeRange = "0-4 (Kém)", count = grades.Count(g => g >= 0 && g < 4) },
                new { gradeRange = "4-5 (Yếu)", count = grades.Count(g => g >= 4 && g < 5) },
                new { gradeRange = "5-7 (Trung bình)", count = grades.Count(g => g >= 5 && g < 7) },
                new { gradeRange = "7-8 (Khá)", count = grades.Count(g => g >= 7 && g < 8) },
                new { gradeRange = "8-10 (Giỏi)", count = grades.Count(g => g >= 8 && g <= 10) }
            };

            return distribution;
        }

        private async Task<List<object>> GetStudentsByDepartmentLegacy()
        {
            var studentsByDept = await _context.Students
                .Join(_context.Classes,
                    student => student.ClassId,
                    @class => @class.ClassId,
                    (student, @class) => new { Student = student, Class = @class })
                .Join(_context.Departments,
                    sc => sc.Class.DepartmentId,
                    dept => dept.DepartmentId,
                    (sc, dept) => new { DepartmentName = dept.DepartmentName, Student = sc.Student })
                .GroupBy(x => x.DepartmentName)
                .Select(g => new
                {
                    departmentName = g.Key,
                    studentCount = g.Count()
                })
                .ToListAsync<object>();

            return studentsByDept;
        }
    }
}
