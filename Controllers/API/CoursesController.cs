using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Temporarily disabled for testing
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public CoursesController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCourses(
            [FromQuery] string? searchString,
            [FromQuery] string? departmentId,
            [FromQuery] string? teacherId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            // Try JWT first, fallback to Session
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value 
                    ?? HttpContext.Session.GetString("UserRole");
            var entityId = User.FindFirst("UserId")?.Value 
                        ?? HttpContext.Session.GetString("EntityId");

            var query = _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .AsQueryable();

            // Teacher can only see their own courses
            if (role == "Teacher" && !string.IsNullOrEmpty(entityId))
            {
                query = query.Where(c => c.TeacherId == entityId);
            }

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c =>
                    c.CourseName.Contains(searchString) ||
                    c.CourseId.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(departmentId))
            {
                query = query.Where(c => c.DepartmentId == departmentId);
            }

            if (!string.IsNullOrEmpty(teacherId))
            {
                query = query.Where(c => c.TeacherId == teacherId);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var courses = await query
                .OrderBy(c => c.CourseId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName,
                    c.Credits,
                    c.DepartmentId,
                    DepartmentName = c.Department!.DepartmentName,
                    DepartmentCode = c.Department.DepartmentCode,
                    c.TeacherId,
                    TeacherName = c.Teacher!.FullName
                })
                .ToListAsync();

            return Ok(new
            {
                data = courses,
                pageNumber,
                pageSize,
                totalCount,
                totalPages
            });
        }

        // GET: api/Courses/CS001
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCourse(string id)
        {
            var course = await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .Include(c => c.Grades)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound(new { message = "Không tìm thấy môn học" });
            }

            // Check authorization: Teacher can only view their own courses
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (role == "Teacher" && course.TeacherId != userId)
            {
                return Forbid();
            }

            var result = new
            {
                course.CourseId,
                course.CourseName,
                course.Credits,
                course.DepartmentId,
                DepartmentName = course.Department?.DepartmentName,
                DepartmentCode = course.Department?.DepartmentCode,
                course.TeacherId,
                TeacherName = course.Teacher?.FullName,
                StudentCount = course.Grades.Select(g => g.StudentId).Distinct().Count()
            };

            return Ok(result);
        }

        // POST: api/Courses
        [HttpPost]
        // [Authorize(Roles = "Admin")] // Only Admin can create courses
        public async Task<ActionResult<Course>> CreateCourse([FromBody] Course course)
        {
            try
            {
                // Validate CourseId unique
                if (await _context.Courses.AnyAsync(c => c.CourseId == course.CourseId))
                {
                    return BadRequest(new { message = "Mã môn học đã tồn tại" });
                }

                // Validate DepartmentId exists
                if (!await _context.Departments.AnyAsync(d => d.DepartmentId == course.DepartmentId))
                {
                    return BadRequest(new { message = "Khoa không tồn tại" });
                }

                // Validate TeacherId exists (if provided)
                if (!string.IsNullOrEmpty(course.TeacherId))
                {
                    if (!await _context.Teachers.AnyAsync(t => t.TeacherId == course.TeacherId))
                    {
                        return BadRequest(new { message = "Giáo viên không tồn tại" });
                    }
                }

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                // Reload with includes
                var createdCourse = await _context.Courses
                    .Include(c => c.Department)
                    .Include(c => c.Teacher)
                    .FirstOrDefaultAsync(c => c.CourseId == course.CourseId);

                return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, createdCourse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo môn học", error = ex.Message });
            }
        }

        // PUT: api/Courses/CS001
        [HttpPut("{id}")]
        // [Authorize(Roles = "Admin")] // Only Admin can update courses
        public async Task<IActionResult> UpdateCourse(string id, [FromBody] Course course)
        {
            if (id != course.CourseId)
            {
                return BadRequest(new { message = "Mã môn học không khớp" });
            }

            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null)
            {
                return NotFound(new { message = "Không tìm thấy môn học" });
            }

            try
            {
                // Validate DepartmentId exists
                if (!await _context.Departments.AnyAsync(d => d.DepartmentId == course.DepartmentId))
                {
                    return BadRequest(new { message = "Khoa không tồn tại" });
                }

                // Validate TeacherId exists (if provided)
                if (!string.IsNullOrEmpty(course.TeacherId))
                {
                    if (!await _context.Teachers.AnyAsync(t => t.TeacherId == course.TeacherId))
                    {
                        return BadRequest(new { message = "Giáo viên không tồn tại" });
                    }
                }

                existingCourse.CourseName = course.CourseName;
                existingCourse.Credits = course.Credits;
                existingCourse.DepartmentId = course.DepartmentId;
                existingCourse.TeacherId = course.TeacherId;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật môn học", error = ex.Message });
            }
        }

        // DELETE: api/Courses/CS001
        [HttpDelete("{id}")]
        // [Authorize(Roles = "Admin")] // Only Admin can delete courses
        public async Task<IActionResult> DeleteCourse(string id)
        {
            var course = await _context.Courses
                .Include(c => c.Grades)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound(new { message = "Không tìm thấy môn học" });
            }

            // Check if course has grades
            if (course.Grades.Any())
            {
                return BadRequest(new { message = "Không thể xóa môn học đã có điểm" });
            }

            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa môn học thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa môn học", error = ex.Message });
            }
        }

        // GET: api/Courses/Dropdown - For dropdown lists
        [HttpGet("dropdown")]
        public async Task<ActionResult<IEnumerable<object>>> GetCoursesDropdown([FromQuery] string? departmentId = null)
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(departmentId))
            {
                query = query.Where(c => c.DepartmentId == departmentId);
            }

            var courses = await query
                .OrderBy(c => c.CourseId)
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName,
                    c.Credits
                })
                .ToListAsync();

            return Ok(courses);
        }

        // ==================== EXPORT METHODS ====================

        // GET: api/courses/export/excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel(
            [FromQuery] string? searchString,
            [FromQuery] string? departmentId,
            [FromQuery] string? teacherId)
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRole");
                var entityId = HttpContext.Session.GetString("EntityId");
                
                Console.WriteLine($"[COURSES EXPORT EXCEL] Role: {role}, EntityId: {entityId}");

                var query = _context.Courses
                    .Include(c => c.Department)
                    .Include(c => c.Teacher)
                    .AsQueryable();

                // ✅ Teacher can only export their own courses
                if (role == "Teacher" && !string.IsNullOrEmpty(entityId))
                {
                    query = query.Where(c => c.TeacherId == entityId);
                    Console.WriteLine($"[COURSES EXPORT EXCEL] Teacher filter applied for TeacherId: {entityId}");
                }

                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(c =>
                        c.CourseName.Contains(searchString) ||
                        c.CourseId.Contains(searchString));
                }

                if (!string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(c => c.DepartmentId == departmentId);
                }

                if (!string.IsNullOrEmpty(teacherId))
                {
                    query = query.Where(c => c.TeacherId == teacherId);
                }

                var courses = await query.OrderBy(c => c.CourseId).ToListAsync();
                
                // For now, use TeacherReport export method
                var fileBytes = _exportService.ExportTeacherReportToExcel(
                    "Danh Sách Môn Học",
                    new List<Class>(),
                    courses);

                return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DanhSachMonHoc_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xuất Excel", error = ex.Message });
            }
        }

        // GET: api/courses/export/pdf
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPdf(
            [FromQuery] string? searchString,
            [FromQuery] string? departmentId,
            [FromQuery] string? teacherId)
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRole");
                var entityId = HttpContext.Session.GetString("EntityId");
                
                Console.WriteLine($"[COURSES EXPORT PDF] Role: {role}, EntityId: {entityId}");

                var query = _context.Courses
                    .Include(c => c.Department)
                    .Include(c => c.Teacher)
                    .AsQueryable();

                // ✅ Teacher can only export their own courses
                if (role == "Teacher" && !string.IsNullOrEmpty(entityId))
                {
                    query = query.Where(c => c.TeacherId == entityId);
                    Console.WriteLine($"[COURSES EXPORT PDF] Teacher filter applied for TeacherId: {entityId}");
                }

                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(c =>
                        c.CourseName.Contains(searchString) ||
                        c.CourseId.Contains(searchString));
                }

                if (!string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(c => c.DepartmentId == departmentId);
                }

                if (!string.IsNullOrEmpty(teacherId))
                {
                    query = query.Where(c => c.TeacherId == teacherId);
                }

                var courses = await query.OrderBy(c => c.CourseId).ToListAsync();
                
                var fileBytes = _exportService.ExportTeacherReportToPdf(
                    "Danh Sách Môn Học",
                    new List<Class>(),
                    courses);

                return File(fileBytes,
                    "application/pdf",
                    $"DanhSachMonHoc_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xuất PDF", error = ex.Message });
            }
        }
    }
}
