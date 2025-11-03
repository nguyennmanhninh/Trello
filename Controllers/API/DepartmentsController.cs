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
    // [Authorize(Roles = "Admin")] // Temporarily disabled for testing
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public DepartmentsController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetDepartments(
            [FromQuery] string? searchString,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Departments.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d =>
                    d.DepartmentName.Contains(searchString) ||
                    d.DepartmentCode.Contains(searchString) ||
                    d.DepartmentId.Contains(searchString));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var departments = await query
                .OrderBy(d => d.DepartmentCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new
                {
                    d.DepartmentId,
                    d.DepartmentCode,
                    d.DepartmentName
                })
                .ToListAsync();

            return Ok(new
            {
                data = departments,
                pageNumber,
                pageSize,
                totalCount,
                totalPages
            });
        }

        // GET: api/Departments/DEPT001
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(string id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound(new { message = "Không tìm thấy khoa" });
            }

            return Ok(department);
        }

        // POST: api/Departments
        [HttpPost]
        public async Task<ActionResult<Department>> CreateDepartment([FromBody] Department department)
        {
            try
            {
                // Validate DepartmentId unique
                if (await _context.Departments.AnyAsync(d => d.DepartmentId == department.DepartmentId))
                {
                    return BadRequest(new { message = "Mã khoa đã tồn tại" });
                }

                // Validate DepartmentCode unique
                if (await _context.Departments.AnyAsync(d => d.DepartmentCode == department.DepartmentCode))
                {
                    return BadRequest(new { message = "Mã khoa (code) đã tồn tại" });
                }

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDepartment), new { id = department.DepartmentId }, department);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo khoa", error = ex.Message });
            }
        }

        // PUT: api/Departments/DEPT001
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(string id, [FromBody] Department department)
        {
            if (id != department.DepartmentId)
            {
                return BadRequest(new { message = "Mã khoa không khớp" });
            }

            var existingDepartment = await _context.Departments.FindAsync(id);
            if (existingDepartment == null)
            {
                return NotFound(new { message = "Không tìm thấy khoa" });
            }

            try
            {
                // Check if DepartmentCode is being changed and if new code already exists
                if (existingDepartment.DepartmentCode != department.DepartmentCode)
                {
                    if (await _context.Departments.AnyAsync(d => d.DepartmentCode == department.DepartmentCode))
                    {
                        return BadRequest(new { message = "Mã khoa (code) đã tồn tại" });
                    }
                }

                existingDepartment.DepartmentCode = department.DepartmentCode;
                existingDepartment.DepartmentName = department.DepartmentName;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật khoa", error = ex.Message });
            }
        }

        // DELETE: api/Departments/DEPT001
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(string id)
        {
            var department = await _context.Departments
                .Include(d => d.Classes)
                .Include(d => d.Courses)
                .Include(d => d.Teachers)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return NotFound(new { message = "Không tìm thấy khoa" });
            }

            // Check if department has associated records
            if (department.Classes.Any())
            {
                return BadRequest(new { message = "Không thể xóa khoa đang có lớp học" });
            }

            if (department.Courses.Any())
            {
                return BadRequest(new { message = "Không thể xóa khoa đang có môn học" });
            }

            if (department.Teachers.Any())
            {
                return BadRequest(new { message = "Không thể xóa khoa đang có giáo viên" });
            }

            try
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa khoa thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa khoa", error = ex.Message });
            }
        }

        // GET: api/Departments/Dropdown - For dropdown lists
        [HttpGet("dropdown")]
        public async Task<ActionResult<IEnumerable<object>>> GetDepartmentsDropdown()
        {
            var departments = await _context.Departments
                .OrderBy(d => d.DepartmentCode)
                .Select(d => new
                {
                    d.DepartmentId,
                    d.DepartmentCode,
                    d.DepartmentName
                })
                .ToListAsync();

            return Ok(departments);
        }

        // ==================== EXPORT METHODS ====================

        // GET: api/departments/export/excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string? searchString)
        {
            try
            {
                var query = _context.Departments.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(d =>
                        d.DepartmentName.Contains(searchString) ||
                        d.DepartmentCode.Contains(searchString) ||
                        d.DepartmentId.Contains(searchString));
                }

                var departments = await query.OrderBy(d => d.DepartmentCode).ToListAsync();
                
                // Get classes for each department
                var departmentClasses = new List<Class>();
                var studentCounts = new Dictionary<string, int>();
                
                foreach (var dept in departments)
                {
                    var classes = await _context.Classes
                        .Include(c => c.Teacher)
                        .Include(c => c.Students)
                        .Where(c => c.DepartmentId == dept.DepartmentId)
                        .ToListAsync();
                    
                    departmentClasses.AddRange(classes);
                    
                    foreach (var cls in classes)
                    {
                        studentCounts[cls.ClassId.ToString()] = cls.Students.Count;
                    }
                }

                var fileBytes = _exportService.ExportDepartmentReportToExcel(
                    searchString ?? "Tất cả",
                    departmentClasses,
                    studentCounts);

                return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DanhSachKhoa_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xuất Excel", error = ex.Message });
            }
        }

        // GET: api/departments/export/pdf
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPdf([FromQuery] string? searchString)
        {
            try
            {
                var query = _context.Departments.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(d =>
                        d.DepartmentName.Contains(searchString) ||
                        d.DepartmentCode.Contains(searchString) ||
                        d.DepartmentId.Contains(searchString));
                }

                var departments = await query.OrderBy(d => d.DepartmentCode).ToListAsync();
                
                // Get classes for each department
                var departmentClasses = new List<Class>();
                var studentCounts = new Dictionary<string, int>();
                
                foreach (var dept in departments)
                {
                    var classes = await _context.Classes
                        .Include(c => c.Teacher)
                        .Include(c => c.Students)
                        .Where(c => c.DepartmentId == dept.DepartmentId)
                        .ToListAsync();
                    
                    departmentClasses.AddRange(classes);
                    
                    foreach (var cls in classes)
                    {
                        studentCounts[cls.ClassId.ToString()] = cls.Students.Count;
                    }
                }

                var fileBytes = _exportService.ExportDepartmentReportToPdf(
                    searchString ?? "Tất cả",
                    departmentClasses,
                    studentCounts);

                return File(fileBytes,
                    "application/pdf",
                    $"DanhSachKhoa_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xuất PDF", error = ex.Message });
            }
        }
    }
}
