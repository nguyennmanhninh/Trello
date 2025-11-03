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
    public class ClassesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public ClassesController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        // GET: api/Classes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetClasses(
            [FromQuery] string? searchString,
            [FromQuery] string? departmentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var username = User.FindFirst("Username")?.Value 
                         ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var query = _context.Classes
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .AsQueryable();

            // Teacher can only see their own classes
            if (role == "Teacher" && !string.IsNullOrEmpty(username))
            {
                // Lookup teacher by username first
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == username);
                if (teacher != null)
                {
                    query = query.Where(c => c.TeacherId == teacher.TeacherId);
                }
                else
                {
                    // No teacher found, return empty
                    return Ok(new
                    {
                        data = new object[] { },
                        pageNumber,
                        pageSize,
                        totalCount = 0,
                        totalPages = 0
                    });
                }
            }

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c =>
                    c.ClassName.Contains(searchString) ||
                    c.ClassId.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(departmentId))
            {
                query = query.Where(c => c.DepartmentId == departmentId);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var classes = await query
                .OrderBy(c => c.ClassId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.ClassId,
                    c.ClassName,
                    c.DepartmentId,
                    DepartmentName = c.Department!.DepartmentName,
                    DepartmentCode = c.Department.DepartmentCode,
                    c.TeacherId,
                    TeacherName = c.Teacher!.FullName,
                    StudentCount = c.Students.Count
                })
                .ToListAsync();

            return Ok(new
            {
                data = classes,
                pageNumber,
                pageSize,
                totalCount,
                totalPages
            });
        }

        // GET: api/Classes/CL001
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetClass(string id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (classEntity == null)
            {
                return NotFound(new { message = "Không tìm thấy lớp học" });
            }

            // Check authorization: Teacher can only view their own class
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (role == "Teacher" && classEntity.TeacherId != userId)
            {
                return Forbid();
            }

            var result = new
            {
                classEntity.ClassId,
                classEntity.ClassName,
                classEntity.DepartmentId,
                DepartmentName = classEntity.Department?.DepartmentName,
                DepartmentCode = classEntity.Department?.DepartmentCode,
                classEntity.TeacherId,
                TeacherName = classEntity.Teacher?.FullName,
                StudentCount = classEntity.Students.Count,
                Students = classEntity.Students.Select(s => new
                {
                    s.StudentId,
                    s.FullName,
                    s.DateOfBirth,
                    s.Gender,
                    s.Phone
                }).ToList()
            };

            return Ok(result);
        }

        // POST: api/Classes
        [HttpPost]
        // [Authorize(Roles = "Admin")] // Only Admin can create classes
        public async Task<ActionResult<Class>> CreateClass([FromBody] Class classEntity)
        {
            try
            {
                // Validate ClassId unique
                if (await _context.Classes.AnyAsync(c => c.ClassId == classEntity.ClassId))
                {
                    return BadRequest(new { message = "Mã lớp đã tồn tại" });
                }

                // Validate DepartmentId exists
                if (!await _context.Departments.AnyAsync(d => d.DepartmentId == classEntity.DepartmentId))
                {
                    return BadRequest(new { message = "Khoa không tồn tại" });
                }

                // Validate TeacherId exists
                if (!await _context.Teachers.AnyAsync(t => t.TeacherId == classEntity.TeacherId))
                {
                    return BadRequest(new { message = "Giáo viên không tồn tại" });
                }

                _context.Classes.Add(classEntity);
                await _context.SaveChangesAsync();

                // Reload with includes
                var createdClass = await _context.Classes
                    .Include(c => c.Department)
                    .Include(c => c.Teacher)
                    .FirstOrDefaultAsync(c => c.ClassId == classEntity.ClassId);

                return CreatedAtAction(nameof(GetClass), new { id = classEntity.ClassId }, createdClass);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo lớp học", error = ex.Message });
            }
        }

        // PUT: api/Classes/CL001
        [HttpPut("{id}")]
        // [Authorize(Roles = "Admin")] // Only Admin can update classes
        public async Task<IActionResult> UpdateClass(string id, [FromBody] Class classEntity)
        {
            if (id != classEntity.ClassId)
            {
                return BadRequest(new { message = "Mã lớp không khớp" });
            }

            var existingClass = await _context.Classes.FindAsync(id);
            if (existingClass == null)
            {
                return NotFound(new { message = "Không tìm thấy lớp học" });
            }

            try
            {
                // Validate DepartmentId exists
                if (!await _context.Departments.AnyAsync(d => d.DepartmentId == classEntity.DepartmentId))
                {
                    return BadRequest(new { message = "Khoa không tồn tại" });
                }

                // Validate TeacherId exists
                if (!await _context.Teachers.AnyAsync(t => t.TeacherId == classEntity.TeacherId))
                {
                    return BadRequest(new { message = "Giáo viên không tồn tại" });
                }

                existingClass.ClassName = classEntity.ClassName;
                existingClass.DepartmentId = classEntity.DepartmentId;
                existingClass.TeacherId = classEntity.TeacherId;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật lớp học", error = ex.Message });
            }
        }

        // DELETE: api/Classes/CL001
        [HttpDelete("{id}")]
        // [Authorize(Roles = "Admin")] // Only Admin can delete classes
        public async Task<IActionResult> DeleteClass(string id)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (classEntity == null)
            {
                return NotFound(new { message = "Không tìm thấy lớp học" });
            }

            // Check if class has students
            if (classEntity.Students.Any())
            {
                return BadRequest(new { message = "Không thể xóa lớp đang có sinh viên" });
            }

            try
            {
                _context.Classes.Remove(classEntity);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa lớp học thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa lớp học", error = ex.Message });
            }
        }

        // GET: api/Classes/Dropdown - For dropdown lists
        [HttpGet("dropdown")]
        public async Task<ActionResult<IEnumerable<object>>> GetClassesDropdown()
        {
            var classes = await _context.Classes
                .OrderBy(c => c.ClassName)
                .Select(c => new
                {
                    c.ClassId,
                    c.ClassName
                })
                .ToListAsync();

            return Ok(classes);
        }

        // ==================== EXPORT METHODS ====================

        // GET: api/classes/export/excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel(
            [FromQuery] string? searchString,
            [FromQuery] string? departmentId)
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRole");
                var entityId = HttpContext.Session.GetString("EntityId");
                
                Console.WriteLine($"[CLASSES EXPORT EXCEL] Role: {role}, EntityId: {entityId}");

                var query = _context.Classes
                    .Include(c => c.Department)
                    .Include(c => c.Teacher)
                    .Include(c => c.Students)
                    .AsQueryable();

                // ✅ Teacher can only export their own classes
                if (role == "Teacher" && !string.IsNullOrEmpty(entityId))
                {
                    query = query.Where(c => c.TeacherId == entityId);
                    Console.WriteLine($"[CLASSES EXPORT EXCEL] Teacher filter applied for TeacherId: {entityId}");
                }

                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(c =>
                        c.ClassName.Contains(searchString) ||
                        c.ClassId.Contains(searchString));
                }

                if (!string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(c => c.DepartmentId == departmentId);
                }

                var classes = await query.OrderBy(c => c.ClassId).ToListAsync();
                
                // Get student counts for each class
                var studentCounts = classes.ToDictionary(
                    c => c.ClassId.ToString(),
                    c => c.Students.Count
                );

                var departmentName = !string.IsNullOrEmpty(departmentId) 
                    ? (await _context.Departments.FindAsync(departmentId))?.DepartmentName ?? "Tất cả"
                    : "Tất cả";

                var fileBytes = _exportService.ExportDepartmentReportToExcel(
                    departmentName, 
                    classes, 
                    studentCounts);

                return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DanhSachLop_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xuất Excel", error = ex.Message });
            }
        }

        // GET: api/classes/export/pdf
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPdf(
            [FromQuery] string? searchString,
            [FromQuery] string? departmentId)
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRole");
                var entityId = HttpContext.Session.GetString("EntityId");
                
                Console.WriteLine($"[CLASSES EXPORT PDF] Role: {role}, EntityId: {entityId}");

                var query = _context.Classes
                    .Include(c => c.Department)
                    .Include(c => c.Teacher)
                    .Include(c => c.Students)
                    .AsQueryable();

                // ✅ Teacher can only export their own classes
                if (role == "Teacher" && !string.IsNullOrEmpty(entityId))
                {
                    query = query.Where(c => c.TeacherId == entityId);
                    Console.WriteLine($"[CLASSES EXPORT PDF] Teacher filter applied for TeacherId: {entityId}");
                }

                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(c =>
                        c.ClassName.Contains(searchString) ||
                        c.ClassId.Contains(searchString));
                }

                if (!string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(c => c.DepartmentId == departmentId);
                }

                var classes = await query.OrderBy(c => c.ClassId).ToListAsync();
                
                // Get student counts for each class
                var studentCounts = classes.ToDictionary(
                    c => c.ClassId.ToString(),
                    c => c.Students.Count
                );

                var departmentName = !string.IsNullOrEmpty(departmentId) 
                    ? (await _context.Departments.FindAsync(departmentId))?.DepartmentName ?? "Tất cả"
                    : "Tất cả";

                var fileBytes = _exportService.ExportDepartmentReportToPdf(
                    departmentName, 
                    classes, 
                    studentCounts);

                return File(fileBytes,
                    "application/pdf",
                    $"DanhSachLop_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xuất PDF", error = ex.Message });
            }
        }
    }
}
