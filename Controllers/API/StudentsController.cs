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
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public StudentsController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetStudents(
            [FromQuery] string? searchString,
            [FromQuery] string? classId,
            [FromQuery] string? departmentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var username = User.FindFirst("Username")?.Value 
                         ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var query = _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.Department)
                .AsQueryable();

            // Teacher can only see students from their classes
            if (role == "Teacher" && !string.IsNullOrEmpty(username))
            {
                // Lookup teacher by username first
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == username);
                if (teacher != null)
                {
                    var teacherClassIds = await _context.Classes
                        .Where(c => c.TeacherId == teacher.TeacherId)
                        .Select(c => c.ClassId)
                        .ToListAsync();
                    query = query.Where(s => teacherClassIds.Contains(s.ClassId));
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
                query = query.Where(s =>
                    s.FullName.Contains(searchString) ||
                    s.StudentId.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(classId))
            {
                query = query.Where(s => s.ClassId == classId);
            }

            if (!string.IsNullOrEmpty(departmentId))
            {
                query = query.Where(s => s.Class!.DepartmentId == departmentId);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var students = await query
                .OrderBy(s => s.StudentId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.StudentId,
                    s.FullName,
                    s.DateOfBirth,
                    s.Gender,
                    s.Phone,
                    s.Address,
                    s.ClassId,
                    ClassName = s.Class!.ClassName,
                    DepartmentId = s.Class.DepartmentId,
                    DepartmentName = s.Class.Department!.DepartmentName
                })
                .ToListAsync();

            return Ok(new
            {
                data = students,
                pageNumber,
                pageSize,
                totalCount,
                totalPages
            });
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetStudent(string id)
        {
            var student = await _context.Students
                .Include(s => s.Class)
                .ThenInclude(c => c!.Department)
                .Include(s => s.Grades)
                .ThenInclude(g => g.Course)
                .Where(s => s.StudentId == id)
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound(new { message = "Không tìm thấy sinh viên" });
            }

            var result = new
            {
                student.StudentId,
                student.FullName,
                student.DateOfBirth,
                student.Gender,
                student.Email, // Add email to response
                student.Phone,
                student.Address,
                student.ClassId,
                ClassName = student.Class!.ClassName,
                DepartmentName = student.Class.Department!.DepartmentName,
                GradeCount = student.Grades.Count, // Thêm số lượng điểm
                Grades = student.Grades.Select(g => new
                {
                    g.CourseId,
                    CourseName = g.Course!.CourseName,
                    g.Score,
                    g.Classification
                }).ToList()
            };

            return Ok(result);
        }

        // POST: api/Students
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<Student>> CreateStudent([FromBody] StudentDto studentDto)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Validate teacher permission
            if (role == "Teacher")
            {
                var isTeacherClass = await _context.Classes
                    .AnyAsync(c => c.ClassId == studentDto.ClassId && c.TeacherId == userId);

                if (!isTeacherClass)
                {
                    return Forbid();
                }
            }

            // Check if student ID already exists
            if (await _context.Students.AnyAsync(s => s.StudentId == studentDto.StudentId))
            {
                return BadRequest(new { message = "Mã sinh viên đã tồn tại" });
            }

            var student = new Student
            {
                StudentId = studentDto.StudentId,
                FullName = studentDto.FullName,
                DateOfBirth = studentDto.DateOfBirth,
                Gender = studentDto.Gender,
                Email = studentDto.Email, // Add email
                Phone = studentDto.Phone,
                Address = studentDto.Address,
                ClassId = studentDto.ClassId,
                Username = studentDto.Username,
                Password = studentDto.Password
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.StudentId }, student);
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher,Student")]
        public async Task<IActionResult> UpdateStudent(string id, [FromBody] StudentDto studentDto)
        {
            Console.WriteLine($"[UPDATE API] Received request to update student: {id}");
            Console.WriteLine($"[UPDATE API] Payload: {System.Text.Json.JsonSerializer.Serialize(studentDto)}");
            
            if (id != studentDto.StudentId)
            {
                return BadRequest(new { message = "Student ID mismatch" });
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound(new { message = "Không tìm thấy sinh viên" });
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Student can only edit their own info (except ClassId)
            if (role == "Student" && userId != id)
            {
                return Forbid();
            }

            if (role == "Student" && studentDto.ClassId != student.ClassId)
            {
                return BadRequest(new { message = "Sinh viên không thể tự thay đổi lớp học" });
            }

            // Teacher validation
            if (role == "Teacher")
            {
                var isCurrentTeacherClass = await _context.Classes
                    .AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);
                var isNewTeacherClass = await _context.Classes
                    .AnyAsync(c => c.ClassId == studentDto.ClassId && c.TeacherId == userId);

                if (!isCurrentTeacherClass || !isNewTeacherClass)
                {
                    return Forbid();
                }
            }

            // Update properties
            student.FullName = studentDto.FullName;
            student.DateOfBirth = studentDto.DateOfBirth;
            student.Gender = studentDto.Gender;
            student.Email = studentDto.Email; // Update email if provided
            student.Phone = studentDto.Phone;
            student.Address = studentDto.Address;
            student.ClassId = studentDto.ClassId;

            if (!string.IsNullOrEmpty(studentDto.Password))
            {
                student.Password = studentDto.Password;
            }

            Console.WriteLine($"[UPDATE API] Before SaveChanges - Student {id}: Email={student.Email}, Phone={student.Phone}, Address={student.Address}");

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"[UPDATE API] SaveChanges SUCCESS for student {id}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            Console.WriteLine($"[DELETE API] Received request to delete student: {id}");
            
            var student = await _context.Students
                .Include(s => s.Grades)
                .FirstOrDefaultAsync(s => s.StudentId == id);
            
            if (student == null)
            {
                Console.WriteLine($"[DELETE API] Student {id} not found in database");
                return NotFound(new { message = "Không tìm thấy sinh viên" });
            }

            Console.WriteLine($"[DELETE API] Found student {id}: {student.FullName}, Grades count: {student.Grades.Count}");

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Teacher validation
            if (role == "Teacher")
            {
                var isTeacherClass = await _context.Classes
                    .AnyAsync(c => c.ClassId == student.ClassId && c.TeacherId == userId);

                if (!isTeacherClass)
                {
                    return Forbid();
                }
            }

            // Check if student has grades - delete grades first (cascade delete)
            if (student.Grades.Any())
            {
                Console.WriteLine($"[DELETE API] Student {id} has {student.Grades.Count} grades. Deleting grades first...");
                _context.Grades.RemoveRange(student.Grades);
            }

            try
            {
                // ✅ IMPORTANT: Also delete the User account associated with this student
                // Note: This is done in API controller for direct API calls
                // For MVC calls, the stored procedure usp_DeleteStudent handles this
                var userAccount = await _context.Users
                    .FirstOrDefaultAsync(u => u.EntityId == id && u.Role == "Student");
                
                if (userAccount != null)
                {
                    Console.WriteLine($"[DELETE API] Found user account for student {id} (Username: {userAccount.Username}). Deleting user account...");
                    _context.Users.Remove(userAccount);
                }
                else
                {
                    Console.WriteLine($"[DELETE API] No user account found for student {id}");
                }

                // Delete student record
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[DELETE API] Successfully deleted student {id} from database");
                return Ok(new { message = "Xóa sinh viên thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DELETE API] ERROR deleting student {id}: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi xóa sinh viên", error = ex.Message });
            }
        }

        private bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }

        // ==================== EXPORT METHODS ====================

        // GET: api/Students/export/excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel(
            [FromQuery] string? searchString,
            [FromQuery] string? classId,
            [FromQuery] string? departmentId)
        {
            try
            {
                // Use Session instead of JWT claims for authentication
                var role = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetString("UserId");
                var entityId = HttpContext.Session.GetString("EntityId");  // ✅ Get EntityId (TeacherId for teachers)

                Console.WriteLine($"[EXPORT EXCEL] Role: {role}, UserId: {userId}, EntityId: {entityId}");
                Console.WriteLine($"[EXPORT EXCEL] Filters - searchString: {searchString}, classId: {classId}, departmentId: {departmentId}");

                var query = _context.Students
                    .Include(s => s.Class)
                    .ThenInclude(c => c!.Department)
                    .AsQueryable();

                // Teacher can only export students from their classes
                if (role == "Teacher")
                {
                    var teacherClassIds = await _context.Classes
                        .Where(c => c.TeacherId == entityId)  // ✅ Use EntityId instead of UserId
                        .Select(c => c.ClassId)
                        .ToListAsync();
                    query = query.Where(s => teacherClassIds.Contains(s.ClassId));
                    Console.WriteLine($"[EXPORT EXCEL] Teacher filter applied. Class IDs: {string.Join(", ", teacherClassIds)}");
                }

                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(s =>
                        s.FullName.Contains(searchString) ||
                        s.StudentId.Contains(searchString));
                    Console.WriteLine($"[EXPORT EXCEL] Search filter applied: {searchString}");
                }

                if (!string.IsNullOrEmpty(classId))
                {
                    query = query.Where(s => s.ClassId == classId);
                    Console.WriteLine($"[EXPORT EXCEL] Class filter applied: {classId}");
                }

                if (!string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(s => s.Class!.DepartmentId == departmentId);
                    Console.WriteLine($"[EXPORT EXCEL] Department filter applied: {departmentId}");
                }

                var students = await query.OrderBy(s => s.StudentId).ToListAsync();
                Console.WriteLine($"[EXPORT EXCEL] Total students to export: {students.Count}");
                
                var fileBytes = _exportService.ExportStudentsToExcel(students);

                return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xuất Excel", error = ex.Message });
            }
        }

        // GET: api/Students/export/pdf
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPdf(
            [FromQuery] string? searchString,
            [FromQuery] string? classId,
            [FromQuery] string? departmentId)
        {
            try
            {
                // Use Session instead of JWT claims for authentication
                var role = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetString("UserId");
                var entityId = HttpContext.Session.GetString("EntityId");  // ✅ Get EntityId (TeacherId for teachers)

                Console.WriteLine($"[EXPORT PDF] Role: {role}, UserId: {userId}, EntityId: {entityId}");
                Console.WriteLine($"[EXPORT PDF] Filters - searchString: {searchString}, classId: {classId}, departmentId: {departmentId}");

                var query = _context.Students
                    .Include(s => s.Class)
                    .ThenInclude(c => c!.Department)
                    .AsQueryable();

                // Teacher can only export students from their classes
                if (role == "Teacher")
                {
                    var teacherClassIds = await _context.Classes
                        .Where(c => c.TeacherId == entityId)  // ✅ Use EntityId instead of UserId
                        .Select(c => c.ClassId)
                        .ToListAsync();
                    query = query.Where(s => teacherClassIds.Contains(s.ClassId));
                    Console.WriteLine($"[EXPORT PDF] Teacher filter applied. Class IDs: {string.Join(", ", teacherClassIds)}");
                }

                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(s =>
                        s.FullName.Contains(searchString) ||
                        s.StudentId.Contains(searchString));
                    Console.WriteLine($"[EXPORT PDF] Search filter applied: {searchString}");
                }

                if (!string.IsNullOrEmpty(classId))
                {
                    query = query.Where(s => s.ClassId == classId);
                    Console.WriteLine($"[EXPORT PDF] Class filter applied: {classId}");
                }

                if (!string.IsNullOrEmpty(departmentId))
                {
                    query = query.Where(s => s.Class!.DepartmentId == departmentId);
                    Console.WriteLine($"[EXPORT PDF] Department filter applied: {departmentId}");
                }

                var students = await query.OrderBy(s => s.StudentId).ToListAsync();
                Console.WriteLine($"[EXPORT PDF] Total students to export: {students.Count}");
                
                var fileBytes = _exportService.ExportStudentsToPdf(students);

                return File(fileBytes,
                    "application/pdf",
                    $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xuất PDF", error = ex.Message });
            }
        }
    }

    public class StudentDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string? Email { get; set; } // Optional email field
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
