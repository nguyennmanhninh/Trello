using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Filters;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AttendanceService _attendanceService;

        public AttendanceController(ApplicationDbContext context, AttendanceService attendanceService)
        {
            _context = context;
            _attendanceService = attendanceService;
        }

        // GET: api/attendance/sessions
        // Get all sessions (Admin) or teacher's sessions
        [HttpGet("sessions")]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> GetSessions([FromQuery] string? courseId = null)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetString("UserId");

                List<AttendanceSession> sessions;

                if (userRole == "Teacher")
                {
                    // Teacher chỉ xem sessions của mình
                    var teacherId = HttpContext.Session.GetString("EntityId"); // Use EntityId for database comparison
                    sessions = await _attendanceService.GetSessionsByTeacherAsync(teacherId!);
                }
                else
                {
                    // Admin xem tất cả hoặc filter theo courseId
                    if (!string.IsNullOrEmpty(courseId))
                    {
                        sessions = await _attendanceService.GetSessionsByCourseAsync(courseId);
                    }
                    else
                    {
                        sessions = await _context.AttendanceSessions
                            .Include(s => s.Course)
                            .Include(s => s.Teacher)
                            .Include(s => s.Attendances)
                            .OrderByDescending(s => s.SessionDate)
                            .ToListAsync();
                    }
                }

                return Ok(new { success = true, data = sessions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/attendance/sessions/{id}
        [HttpGet("sessions/{id}")]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> GetSessionDetails(int id)
        {
            try
            {
                var session = await _attendanceService.GetSessionDetailsAsync(id);
                
                if (session == null)
                    return NotFound(new { success = false, message = "Không tìm thấy buổi điểm danh" });

                // Check authorization
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole == "Teacher")
                {
                    var teacherId = HttpContext.Session.GetString("EntityId"); // Use EntityId for database comparison
                    if (session.TeacherId != teacherId)
                        return Forbid();
                }

                return Ok(new { success = true, data = session });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/attendance/sessions
        [HttpPost("sessions")]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> CreateSession([FromBody] CreateAttendanceSessionDto dto)
        {
            try
            {
                Console.WriteLine($"[CreateSession] Received request - CourseId: {dto?.CourseId}, SessionDate: {dto?.SessionDate}, SessionTime: {dto?.SessionTime}");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine($"[CreateSession] ModelState invalid:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"  - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    return BadRequest(ModelState);
                }

                var userRole = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetString("UserId"); // Username for logging
                var entityId = HttpContext.Session.GetString("EntityId"); // Teacher/Student ID for database comparison
                
                Console.WriteLine($"[CreateSession] UserRole: {userRole}, UserId: {userId}, EntityId: {entityId}");
                
                // Get course to determine teacherId
                var course = await _context.Courses.FindAsync(dto.CourseId);
                Console.WriteLine($"[CreateSession] Course lookup - Found: {course != null}, CourseId: {dto.CourseId}, TeacherId: {course?.TeacherId}");
                
                if (course == null)
                {
                    Console.WriteLine($"[CreateSession] ❌ Course not found");
                    return BadRequest(new { success = false, message = "Môn học không tồn tại" });
                }
                
                // Teacher chỉ tạo session cho course của mình
                if (userRole == "Teacher")
                {
                    Console.WriteLine($"[CreateSession] Teacher check - EntityId: {entityId}, Course.TeacherId: {course.TeacherId}, Match: {course.TeacherId == entityId}");
                    if (course.TeacherId != entityId)
                    {
                        Console.WriteLine($"[CreateSession] ❌ Teacher not authorized for this course");
                        return Forbid();
                    }
                }

                // Convert DTO to AttendanceSession
                var session = new AttendanceSession
                {
                    CourseId = dto.CourseId,
                    TeacherId = course.TeacherId!, // Get from course
                    SessionDate = dto.SessionDate,
                    SessionTime = TimeSpan.Parse(dto.SessionTime), // Convert "08:00" to TimeSpan
                    SessionTitle = dto.SessionTitle,
                    SessionType = dto.SessionType ?? "Lý thuyết",
                    Location = dto.Location,
                    Duration = dto.Duration,
                    Notes = dto.Notes,
                    Status = "Scheduled",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var result = await _attendanceService.CreateAttendanceSessionAsync(session);

                if (result.Success)
                {
                    var createdSession = await _attendanceService.GetSessionDetailsAsync(result.SessionId);
                    return Ok(new { success = true, message = result.Message, data = createdSession });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PUT: api/attendance/mark
        [HttpPut("mark")]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceRequest request)
        {
            Console.WriteLine($"[MarkAttendance] ===== METHOD ENTERED =====");
            Console.WriteLine($"[MarkAttendance] Request is null: {request == null}");
            
            if (request != null)
            {
                Console.WriteLine($"[MarkAttendance] Received request - SessionId: {request.SessionId}, TeacherId: '{request.TeacherId}', AttendanceCount: {request.Attendances?.Count}");
            }
            
            try
            {
                if (!ModelState.IsValid)
                {
                    Console.WriteLine($"[MarkAttendance] ❌ ModelState invalid:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"  - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    return BadRequest(ModelState);
                }

                var userRole = HttpContext.Session.GetString("UserRole");
                var entityId = HttpContext.Session.GetString("EntityId");
                
                Console.WriteLine($"[MarkAttendance] UserRole: {userRole}, EntityId: {entityId}");
                
                // Verify teacher owns this session
                if (userRole == "Teacher")
                {
                    var teacherId = HttpContext.Session.GetString("EntityId"); // Use EntityId for database comparison
                    var session = await _context.AttendanceSessions.FindAsync(request.SessionId);
                    
                    Console.WriteLine($"[MarkAttendance] Session lookup - Found: {session != null}, TeacherId: {session?.TeacherId}");
                    
                    if (session == null || session.TeacherId != teacherId)
                    {
                        Console.WriteLine($"[MarkAttendance] ❌ Teacher not authorized");
                        return Forbid();
                    }
                    
                    request.TeacherId = teacherId!;
                }
                else if (userRole == "Admin")
                {
                    // Admin can mark for any session, use session's teacherId
                    var session = await _context.AttendanceSessions.FindAsync(request.SessionId);
                    if (session != null)
                    {
                        request.TeacherId = session.TeacherId;
                        Console.WriteLine($"[MarkAttendance] Admin marking for TeacherId: {request.TeacherId}");
                    }
                }

                Console.WriteLine($"[MarkAttendance] Calling service with TeacherId: {request.TeacherId}");
                var result = await _attendanceService.MarkAttendanceAsync(request);

                if (result.Success)
                {
                    return Ok(new { success = true, message = result.Message });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/attendance/student/{studentId}/stats
        [HttpGet("student/{studentId}/stats")]
        [AuthorizeRole("Admin", "Teacher", "Student")]
        public async Task<IActionResult> GetStudentStats(string studentId, [FromQuery] string? courseId = null)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                
                // Student chỉ xem stats của chính mình
                if (userRole == "Student")
                {
                    var currentStudentId = HttpContext.Session.GetString("EntityId"); // Use EntityId for database comparison
                    if (studentId != currentStudentId)
                        return Forbid();
                }

                var stats = await _attendanceService.GetStudentAttendanceStatsAsync(studentId, courseId);
                
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/attendance/student/{studentId}/records
        [HttpGet("student/{studentId}/records")]
        [AuthorizeRole("Admin", "Teacher", "Student")]
        public async Task<IActionResult> GetStudentAttendances(string studentId)
        {
            try
            {
                Console.WriteLine($"[GetStudentAttendances] ===== REQUEST =====");
                Console.WriteLine($"[GetStudentAttendances] StudentId: {studentId}");
                
                var userRole = HttpContext.Session.GetString("UserRole");
                Console.WriteLine($"[GetStudentAttendances] UserRole: {userRole}");
                
                // Student chỉ xem records của chính mình
                if (userRole == "Student")
                {
                    var currentStudentId = HttpContext.Session.GetString("EntityId"); // Use EntityId for database comparison
                    Console.WriteLine($"[GetStudentAttendances] Current EntityId: {currentStudentId}");
                    
                    if (studentId != currentStudentId)
                    {
                        Console.WriteLine($"[GetStudentAttendances] ❌ Authorization failed: {studentId} != {currentStudentId}");
                        return Forbid();
                    }
                }

                var attendances = await _attendanceService.GetStudentAttendancesAsync(studentId);
                Console.WriteLine($"[GetStudentAttendances] ✅ Found {attendances.Count} attendance records");
                
                return Ok(new { success = true, data = attendances });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetStudentAttendances] ❌ Error: {ex.Message}");
                Console.WriteLine($"[GetStudentAttendances] Stack: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/attendance/warnings/{courseId}
        [HttpGet("warnings/{courseId}")]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> GetAttendanceWarnings(string courseId, [FromQuery] decimal threshold = 20.0m)
        {
            try
            {
                var warnings = await _attendanceService.GetAttendanceWarningsAsync(courseId, threshold);
                
                return Ok(new { success = true, data = warnings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/attendance/sessions/{id}
        [HttpDelete("sessions/{id}")]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            try
            {
                var session = await _context.AttendanceSessions.FindAsync(id);
                
                if (session == null)
                    return NotFound(new { success = false, message = "Không tìm thấy buổi điểm danh" });

                // Teacher chỉ xóa session của mình
                var userRole = HttpContext.Session.GetString("UserRole");
                if (userRole == "Teacher")
                {
                    var teacherId = HttpContext.Session.GetString("EntityId"); // Use EntityId for database comparison
                    if (session.TeacherId != teacherId)
                        return Forbid();
                }

                var result = await _attendanceService.DeleteSessionAsync(id);

                if (result)
                    return Ok(new { success = true, message = "Xóa buổi điểm danh thành công" });
                else
                    return BadRequest(new { success = false, message = "Xóa buổi điểm danh thất bại" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
