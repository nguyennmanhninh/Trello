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
    public class GradesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public GradesController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        // GET: api/Grades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetGrades(
            [FromQuery] string? classId,
            [FromQuery] string? courseId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var username = User.FindFirst("Username")?.Value 
                         ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var query = _context.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s!.Class)
                .Include(g => g.Course)
                .AsQueryable();

            // Role-based filtering
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

                    query = query.Where(g => teacherClassIds.Contains(g.Student!.ClassId));
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
            else if (role == "Student" && !string.IsNullOrEmpty(username))
            {
                // Lookup student by username first
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
                if (student != null)
                {
                    query = query.Where(g => g.StudentId == student.StudentId);
                }
                else
                {
                    // No student found, return empty
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
            if (!string.IsNullOrEmpty(classId))
            {
                query = query.Where(g => g.Student!.ClassId == classId);
            }

            if (!string.IsNullOrEmpty(courseId))
            {
                query = query.Where(g => g.CourseId == courseId);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var grades = await query
                .OrderBy(g => g.StudentId)
                .ThenBy(g => g.CourseId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new
                {
                    g.StudentId,
                    StudentName = g.Student!.FullName,
                    ClassName = g.Student.Class!.ClassName,
                    g.CourseId,
                    CourseName = g.Course!.CourseName,
                    g.Score,
                    g.Classification
                })
                .ToListAsync();

            return Ok(new
            {
                data = grades,
                pageNumber,
                pageSize,
                totalCount,
                totalPages
            });
        }

        // GET: api/Grades/student/SV001
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentGrades(string studentId)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var entityId = User.FindFirst("EntityId")?.Value;

            // Student can only view their own grades
            if (role == "Student" && entityId != studentId)
            {
                return Forbid();
            }

            var grades = await _context.Grades
                .Include(g => g.Course)
                .ThenInclude(c => c!.Department)
                .Where(g => g.StudentId == studentId)
                .Select(g => new
                {
                    g.StudentId,
                    g.CourseId,
                    CourseName = g.Course!.CourseName,
                    Credits = g.Course.Credits,
                    DepartmentName = g.Course.Department!.DepartmentName,
                    g.Score,
                    g.Classification
                })
                .ToListAsync();

            return Ok(grades);
        }

        // GET: api/Grades/{studentId}/{courseId}
        [HttpGet("{studentId}/{courseId}")]
        public async Task<ActionResult<object>> GetGrade(string studentId, string courseId)
        {
            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Course)
                .Where(g => g.StudentId == studentId && g.CourseId == courseId)
                .Select(g => new
                {
                    g.StudentId,
                    StudentName = g.Student!.FullName,
                    g.CourseId,
                    CourseName = g.Course!.CourseName,
                    g.Score,
                    g.Classification
                })
                .FirstOrDefaultAsync();

            if (grade == null)
            {
                return NotFound(new { message = "Không tìm thấy điểm" });
            }

            return Ok(grade);
        }

        // POST: api/Grades
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<Grade>> CreateGrade([FromBody] GradeDto gradeDto)
        {
            // Check if grade already exists
            if (await _context.Grades.AnyAsync(g =>
                g.StudentId == gradeDto.StudentId && g.CourseId == gradeDto.CourseId))
            {
                return BadRequest(new { message = "Điểm cho môn học này đã tồn tại" });
            }

            var classification = CalculateClassification(gradeDto.Score);

            var grade = new Grade
            {
                StudentId = gradeDto.StudentId,
                CourseId = gradeDto.CourseId,
                Score = gradeDto.Score,
                Classification = classification
            };

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGrade),
                new { studentId = grade.StudentId, courseId = grade.CourseId }, grade);
        }

        // PUT: api/Grades/{studentId}/{courseId}
        [HttpPut("{studentId}/{courseId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateGrade(string studentId, string courseId, [FromBody] GradeDto gradeDto)
        {
            if (studentId != gradeDto.StudentId || courseId != gradeDto.CourseId)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var grade = await _context.Grades
                .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);

            if (grade == null)
            {
                return NotFound(new { message = "Không tìm thấy điểm" });
            }

            grade.Score = gradeDto.Score;
            grade.Classification = CalculateClassification(gradeDto.Score);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GradeExists(studentId, courseId))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Grades/{studentId}/{courseId}
        [HttpDelete("{studentId}/{courseId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteGrade(string studentId, string courseId)
        {
            var grade = await _context.Grades
                .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);

            if (grade == null)
            {
                return NotFound(new { message = "Không tìm thấy điểm" });
            }

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Grades/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<object>> GetStatistics([FromQuery] string? classId, [FromQuery] string? courseId)
        {
            var query = _context.Grades.AsQueryable();

            if (!string.IsNullOrEmpty(classId))
            {
                query = query.Where(g => g.Student!.ClassId == classId);
            }

            if (!string.IsNullOrEmpty(courseId))
            {
                query = query.Where(g => g.CourseId == courseId);
            }

            var grades = await query.ToListAsync();

            if (!grades.Any())
            {
                return Ok(new
                {
                    totalGrades = 0,
                    averageScore = 0,
                    distribution = new { }
                });
            }

            var distribution = new
            {
                excellent = grades.Count(g => g.Score >= 9.0m),
                good = grades.Count(g => g.Score >= 8.0m && g.Score < 9.0m),
                average = grades.Count(g => g.Score >= 7.0m && g.Score < 8.0m),
                belowAverage = grades.Count(g => g.Score >= 5.5m && g.Score < 7.0m),
                weak = grades.Count(g => g.Score >= 4.0m && g.Score < 5.5m),
                poor = grades.Count(g => g.Score < 4.0m)
            };

            return Ok(new
            {
                totalGrades = grades.Count,
                averageScore = Math.Round(grades.Average(g => g.Score), 2),
                distribution
            });
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

        // GET: api/grades/export/excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string? classId, [FromQuery] string? courseId)
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRole");
                var entityId = HttpContext.Session.GetString("EntityId");
                
                Console.WriteLine($"[GRADES API EXPORT EXCEL] Role: {role}, EntityId: {entityId}");

                var gradesQuery = _context.Grades
                    .Include(g => g.Student)
                        .ThenInclude(s => s.Class)
                    .Include(g => g.Course)
                    .AsQueryable();

                // ✅ Teacher can only export grades from their classes
                if (role == "Teacher" && !string.IsNullOrEmpty(entityId))
                {
                    var teacherClassIds = await _context.Classes
                        .Where(c => c.TeacherId == entityId)
                        .Select(c => c.ClassId)
                        .ToListAsync();
                    
                    gradesQuery = gradesQuery.Where(g => g.Student != null && teacherClassIds.Contains(g.Student.ClassId));
                    Console.WriteLine($"[GRADES API EXPORT EXCEL] Teacher filter applied. Class IDs: {string.Join(", ", teacherClassIds)}");
                }

                if (!string.IsNullOrEmpty(classId))
                {
                    gradesQuery = gradesQuery.Where(g => g.Student != null && g.Student.ClassId == classId);
                }

                if (!string.IsNullOrEmpty(courseId))
                {
                    gradesQuery = gradesQuery.Where(g => g.CourseId == courseId);
                }

                var grades = await gradesQuery.ToListAsync();
                var fileBytes = _exportService.ExportGradesToExcel(grades);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"Grades_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting to Excel", error = ex.Message });
            }
        }

        // GET: api/grades/export/pdf
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPdf([FromQuery] string? classId, [FromQuery] string? courseId)
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRole");
                var entityId = HttpContext.Session.GetString("EntityId");
                
                Console.WriteLine($"[GRADES API EXPORT PDF] Role: {role}, EntityId: {entityId}");

                var gradesQuery = _context.Grades
                    .Include(g => g.Student)
                        .ThenInclude(s => s.Class)
                    .Include(g => g.Course)
                    .AsQueryable();

                // ✅ Teacher can only export grades from their classes
                if (role == "Teacher" && !string.IsNullOrEmpty(entityId))
                {
                    var teacherClassIds = await _context.Classes
                        .Where(c => c.TeacherId == entityId)
                        .Select(c => c.ClassId)
                        .ToListAsync();
                    
                    gradesQuery = gradesQuery.Where(g => g.Student != null && teacherClassIds.Contains(g.Student.ClassId));
                    Console.WriteLine($"[GRADES API EXPORT PDF] Teacher filter applied. Class IDs: {string.Join(", ", teacherClassIds)}");
                }

                if (!string.IsNullOrEmpty(classId))
                {
                    gradesQuery = gradesQuery.Where(g => g.Student != null && g.Student.ClassId == classId);
                }

                if (!string.IsNullOrEmpty(courseId))
                {
                    gradesQuery = gradesQuery.Where(g => g.CourseId == courseId);
                }

                var grades = await gradesQuery.ToListAsync();
                var fileBytes = _exportService.ExportGradesToPdf(grades);

                return File(fileBytes, "application/pdf", 
                    $"Grades_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting to PDF", error = ex.Message });
            }
        }

        // DELETE: api/Grades/DeleteAllByStudent/{studentId}
        [HttpDelete("DeleteAllByStudent/{studentId}")]
        public async Task<IActionResult> DeleteAllGradesByStudent(string studentId)
        {
            try
            {
                // Check if student exists
                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                {
                    return NotFound(new { message = "Không tìm thấy sinh viên" });
                }

                // Get all grades for this student
                var grades = await _context.Grades
                    .Where(g => g.StudentId == studentId)
                    .ToListAsync();

                if (!grades.Any())
                {
                    return Ok(new { 
                        success = true, 
                        message = "Sinh viên chưa có điểm nào",
                        deletedCount = 0 
                    });
                }

                var count = grades.Count;
                
                // Delete all grades
                _context.Grades.RemoveRange(grades);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = $"Đã xóa thành công {count} điểm số",
                    deletedCount = count 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Lỗi khi xóa điểm số", 
                    error = ex.Message 
                });
            }
        }
    }

    public class GradeDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public decimal Score { get; set; }
    }
}
