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
    public class TeachersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public TeachersController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        // GET: api/teachers
        [HttpGet]
        public async Task<IActionResult> GetTeachers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var query = _context.Teachers
                    .Include(t => t.Department)
                    .AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(t =>
                        t.FullName.Contains(searchTerm) ||
                        t.Username.Contains(searchTerm) ||
                        t.Phone.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var teachers = await query
                    .OrderBy(t => t.FullName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.TeacherId,
                        t.FullName,
                        t.DateOfBirth,
                        t.Gender,
                        t.Phone,
                        t.Address,
                        t.Username,
                        t.DepartmentId,
                        DepartmentName = t.Department != null ? t.Department.DepartmentName : null
                    })
                    .ToListAsync();

                var result = new
                {
                    items = teachers,
                    totalCount,
                    pageNumber,
                    pageSize,
                    totalPages
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải danh sách giáo viên", error = ex.Message });
            }
        }

        // GET: api/teachers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacher(string id)
        {
            try
            {
                var teacher = await _context.Teachers
                    .Include(t => t.Department)
                    .Where(t => t.TeacherId == id)
                    .Select(t => new
                    {
                        t.TeacherId,
                        t.FullName,
                        t.DateOfBirth,
                        t.Gender,
                        t.Phone,
                        t.Address,
                        t.Username,
                        t.DepartmentId,
                        DepartmentName = t.Department != null ? t.Department.DepartmentName : null
                    })
                    .FirstOrDefaultAsync();

                if (teacher == null)
                {
                    return NotFound(new { message = "Không tìm thấy giáo viên" });
                }

                return Ok(teacher);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải thông tin giáo viên", error = ex.Message });
            }
        }

        // POST: api/teachers
        [HttpPost]
        public async Task<IActionResult> CreateTeacher([FromBody] Teacher teacher)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTeacher), new { id = teacher.TeacherId }, teacher);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo giáo viên", error = ex.Message });
            }
        }

        // PUT: api/teachers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeacher(string id, [FromBody] Teacher teacher)
        {
            try
            {
                if (id != teacher.TeacherId)
                {
                    return BadRequest(new { message = "ID không khớp" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _context.Entry(teacher).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TeacherExists(id))
                    {
                        return NotFound(new { message = "Không tìm thấy giáo viên" });
                    }
                    throw;
                }

                return Ok(teacher);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật giáo viên", error = ex.Message });
            }
        }

        // DELETE: api/teachers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(string id)
        {
            try
            {
                var teacher = await _context.Teachers.FindAsync(id);
                if (teacher == null)
                {
                    return NotFound(new { message = "Không tìm thấy giáo viên" });
                }

                // ✅ IMPORTANT: Also delete the User account associated with this teacher
                var userAccount = await _context.Users
                    .FirstOrDefaultAsync(u => u.EntityId == id && u.Role == "Teacher");
                
                if (userAccount != null)
                {
                    Console.WriteLine($"[DELETE Teacher API] Found user account for teacher {id} (Username: {userAccount.Username}). Deleting user account...");
                    _context.Users.Remove(userAccount);
                }
                else
                {
                    Console.WriteLine($"[DELETE Teacher API] No user account found for teacher {id}");
                }

                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa giáo viên", error = ex.Message });
            }
        }

        private async Task<bool> TeacherExists(string id)
        {
            return await _context.Teachers.AnyAsync(e => e.TeacherId == id);
        }

        // ==================== EXPORT METHODS ====================

        // GET: api/teachers/export/excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string? searchTerm = null)
        {
            try
            {
                var query = _context.Teachers
                    .Include(t => t.Department)
                    .AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(t =>
                        t.FullName.Contains(searchTerm) ||
                        t.Username.Contains(searchTerm) ||
                        t.Phone.Contains(searchTerm));
                }

                var teachers = await query.OrderBy(t => t.FullName).ToListAsync();
                var fileBytes = _exportService.ExportTeachersToExcel(teachers);

                return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DanhSachGiaoVien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xuất Excel", error = ex.Message });
            }
        }

        // GET: api/teachers/export/pdf
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPdf([FromQuery] string? searchTerm = null)
        {
            try
            {
                var query = _context.Teachers
                    .Include(t => t.Department)
                    .AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(t =>
                        t.FullName.Contains(searchTerm) ||
                        t.Username.Contains(searchTerm) ||
                        t.Phone.Contains(searchTerm));
                }

                var teachers = await query.OrderBy(t => t.FullName).ToListAsync();
                var fileBytes = _exportService.ExportTeachersToPdf(teachers);

                return File(fileBytes,
                    "application/pdf",
                    $"DanhSachGiaoVien_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xuất PDF", error = ex.Message });
            }
        }
    }
}
