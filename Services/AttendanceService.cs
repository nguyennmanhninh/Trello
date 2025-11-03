using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Data;
using System.Text.Json;

namespace StudentManagementSystem.Services
{
    public class AttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AttendanceService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Create attendance session using stored procedure
        /// </summary>
        public async Task<(bool Success, int SessionId, string Message)> CreateAttendanceSessionAsync(AttendanceSession session)
        {
            try
            {
                Console.WriteLine($"[AttendanceService] Creating session - CourseId: {session.CourseId}, TeacherId: {session.TeacherId}");
                
                // Use raw ADO.NET connection for better control over OUTPUT parameters
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("usp_CreateAttendanceSession", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Add input parameters
                command.Parameters.AddWithValue("@CourseId", session.CourseId);
                command.Parameters.AddWithValue("@TeacherId", session.TeacherId);
                command.Parameters.AddWithValue("@SessionDate", session.SessionDate);
                command.Parameters.AddWithValue("@SessionTime", session.SessionTime);
                command.Parameters.AddWithValue("@SessionTitle", session.SessionTitle);
                command.Parameters.AddWithValue("@SessionType", session.SessionType ?? "Lý thuyết");
                command.Parameters.AddWithValue("@Location", (object?)session.Location ?? DBNull.Value);
                command.Parameters.AddWithValue("@Duration", session.Duration);

                // Add output parameter
                var sessionIdParam = new SqlParameter("@SessionId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(sessionIdParam);

                // Add return value parameter
                var returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnParam);

                Console.WriteLine($"[AttendanceService] Opening connection...");
                await connection.OpenAsync();
                
                Console.WriteLine($"[AttendanceService] Executing stored procedure...");
                await command.ExecuteNonQueryAsync();

                var returnValue = (int)returnParam.Value;
                var sessionId = sessionIdParam.Value != DBNull.Value ? (int)sessionIdParam.Value : 0;

                Console.WriteLine($"[AttendanceService] SP Result - ReturnValue: {returnValue}, SessionId: {sessionId}");

                if (returnValue == 1 && sessionId > 0)
                {
                    Console.WriteLine($"[AttendanceService] ✅ Session created successfully: {sessionId}");
                    return (true, sessionId, "Tạo buổi điểm danh thành công");
                }
                else
                {
                    Console.WriteLine($"[AttendanceService] ❌ Session creation failed");
                    return (false, 0, "Tạo buổi điểm danh thất bại");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AttendanceService] ❌ Error creating session: {ex.Message}");
                Console.WriteLine($"[AttendanceService] StackTrace: {ex.StackTrace}");
                return (false, 0, ex.Message);
            }
        }

        /// <summary>
        /// Mark attendance using stored procedure
        /// </summary>
        public async Task<(bool Success, string Message)> MarkAttendanceAsync(MarkAttendanceRequest request)
        {
            try
            {
                Console.WriteLine($"[AttendanceService] MarkAttendanceAsync - SessionId: {request.SessionId}, TeacherId: {request.TeacherId}, Count: {request.Attendances?.Count}");
                
                // Convert attendances to JSON
                var attendanceJson = JsonSerializer.Serialize(request.Attendances);
                Console.WriteLine($"[AttendanceService] Attendance JSON: {attendanceJson.Substring(0, Math.Min(200, attendanceJson.Length))}...");

                // Use raw ADO.NET connection for better control over RETURN VALUE
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using var connection = new SqlConnection(connectionString);
                using var command = new SqlCommand("usp_MarkAttendance", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Add input parameters
                command.Parameters.AddWithValue("@SessionId", request.SessionId);
                command.Parameters.AddWithValue("@TeacherId", request.TeacherId);
                command.Parameters.AddWithValue("@AttendanceData", attendanceJson);

                // Add return value parameter
                var returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnParam);

                Console.WriteLine($"[AttendanceService] Executing usp_MarkAttendance...");
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                var returnValue = (int)returnParam.Value;
                Console.WriteLine($"[AttendanceService] SP Result - ReturnValue: {returnValue}");

                if (returnValue == 1)
                {
                    Console.WriteLine($"[AttendanceService] ✅ Mark attendance successful");
                    return (true, "Điểm danh thành công");
                }
                else
                {
                    Console.WriteLine($"[AttendanceService] ❌ Mark attendance failed");
                    return (false, "Điểm danh thất bại");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AttendanceService] ❌ Error marking attendance: {ex.Message}");
                Console.WriteLine($"[AttendanceService] StackTrace: {ex.StackTrace}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Get student attendance statistics using stored procedure
        /// </summary>
        public async Task<List<AttendanceStatistics>> GetStudentAttendanceStatsAsync(string studentId, string? courseId = null)
        {
            try
            {
                var studentIdParam = new SqlParameter("@StudentId", studentId);
                var courseIdParam = new SqlParameter("@CourseId", (object?)courseId ?? DBNull.Value);

                var result = await _context.Database
                    .SqlQueryRaw<AttendanceStatistics>(
                        "EXEC usp_GetStudentAttendanceStats @StudentId, @CourseId",
                        studentIdParam, courseIdParam)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AttendanceService] Error getting stats: {ex.Message}");
                return new List<AttendanceStatistics>();
            }
        }

        /// <summary>
        /// Get attendance warnings using stored procedure
        /// </summary>
        public async Task<List<AttendanceWarning>> GetAttendanceWarningsAsync(string courseId, decimal threshold = 20.0m)
        {
            try
            {
                var courseIdParam = new SqlParameter("@CourseId", courseId);
                var thresholdParam = new SqlParameter("@ThresholdPercent", threshold);

                var result = await _context.Database
                    .SqlQueryRaw<AttendanceWarning>(
                        "EXEC usp_GetAttendanceWarnings @CourseId, @ThresholdPercent",
                        courseIdParam, thresholdParam)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AttendanceService] Error getting warnings: {ex.Message}");
                return new List<AttendanceWarning>();
            }
        }

        /// <summary>
        /// Get all sessions for a course
        /// </summary>
        public async Task<List<AttendanceSession>> GetSessionsByCourseAsync(string courseId)
        {
            return await _context.AttendanceSessions
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                .Include(s => s.Attendances)
                .Where(s => s.CourseId == courseId)
                .OrderByDescending(s => s.SessionDate)
                .ThenByDescending(s => s.SessionTime)
                .ToListAsync();
        }

        /// <summary>
        /// Get sessions for a teacher
        /// </summary>
        public async Task<List<AttendanceSession>> GetSessionsByTeacherAsync(string teacherId)
        {
            return await _context.AttendanceSessions
                .Include(s => s.Course)
                .Include(s => s.Attendances)
                    .ThenInclude(a => a.Student)
                .Where(s => s.TeacherId == teacherId)
                .OrderByDescending(s => s.SessionDate)
                .ThenByDescending(s => s.SessionTime)
                .ToListAsync();
        }

        /// <summary>
        /// Get session details with attendances
        /// </summary>
        public async Task<AttendanceSession?> GetSessionDetailsAsync(int sessionId)
        {
            return await _context.AttendanceSessions
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                .Include(s => s.Attendances)
                    .ThenInclude(a => a.Student)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }

        /// <summary>
        /// Get attendance records for a student
        /// </summary>
        public async Task<List<object>> GetStudentAttendancesAsync(string studentId)
        {
            try
            {
                Console.WriteLine($"[AttendanceService] GetStudentAttendancesAsync - StudentId: {studentId}");
                
                var attendances = await _context.Attendances
                    .Include(a => a.Session)
                        .ThenInclude(s => s!.Course)
                    .Where(a => a.StudentId == studentId)
                    .OrderByDescending(a => a.Session!.SessionDate)
                    .ToListAsync();
                
                Console.WriteLine($"[AttendanceService] Found {attendances.Count} attendance records");
                
                // Map to anonymous objects to ensure proper serialization with nested data
                var mappedAttendances = attendances.Select(a => new
                {
                    AttendanceId = a.AttendanceId,
                    SessionId = a.SessionId,
                    StudentId = a.StudentId,
                    Status = a.Status,
                    CheckInTime = a.CheckInTime,
                    Notes = a.Notes,
                    MarkedByTeacherId = a.MarkedByTeacherId,
                    MarkedAt = a.MarkedAt,
                    
                    // Flatten session data
                    SessionTitle = a.Session?.SessionTitle,
                    SessionDate = a.Session?.SessionDate,
                    SessionTime = a.Session?.SessionTime,
                    Location = a.Session?.Location,
                    SessionType = a.Session?.SessionType,
                    
                    // Flatten course data from session
                    CourseId = a.Session?.Course?.CourseId,
                    CourseName = a.Session?.Course?.CourseName,
                    
                    // Keep nested for compatibility
                    Session = a.Session == null ? null : new
                    {
                        SessionId = a.Session.SessionId,
                        SessionTitle = a.Session.SessionTitle,
                        SessionDate = a.Session.SessionDate,
                        SessionTime = a.Session.SessionTime,
                        Location = a.Session.Location,
                        CourseId = a.Session.CourseId,
                        Course = a.Session.Course == null ? null : new
                        {
                            CourseId = a.Session.Course.CourseId,
                            CourseName = a.Session.Course.CourseName
                        }
                    }
                }).Cast<object>().ToList();
                
                if (mappedAttendances.Any())
                {
                    Console.WriteLine($"[AttendanceService] First mapped record has course: {mappedAttendances[0]}");
                }
                
                return mappedAttendances;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AttendanceService] ❌ Error: {ex.Message}");
                Console.WriteLine($"[AttendanceService] Stack: {ex.StackTrace}");
                return new List<object>();
            }
        }

        /// <summary>
        /// Delete attendance session
        /// </summary>
        public async Task<bool> DeleteSessionAsync(int sessionId)
        {
            try
            {
                var session = await _context.AttendanceSessions.FindAsync(sessionId);
                if (session == null)
                    return false;

                _context.AttendanceSessions.Remove(session);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AttendanceService] Error deleting session: {ex.Message}");
                return false;
            }
        }
    }
}
