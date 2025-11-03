using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Debug: Log all cookies
                Console.WriteLine($"[ProfileAPI] === Request Headers ===");
                Console.WriteLine($"[ProfileAPI] Cookie Header: {HttpContext.Request.Headers["Cookie"]}");
                Console.WriteLine($"[ProfileAPI] Session ID: {HttpContext.Session.Id}");
                Console.WriteLine($"[ProfileAPI] Session IsAvailable: {HttpContext.Session.IsAvailable}");
                
                // Get user info from session
                var userRole = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetString("UserId");
                var entityId = HttpContext.Session.GetString("EntityId");

                Console.WriteLine($"[ProfileAPI] UserRole: {userRole}, UserId: {userId}, EntityId: {entityId}");

                if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("[ProfileAPI] ERROR: Session empty - Unauthorized");
                    return Unauthorized(new { message = "Chưa đăng nhập" });
                }

                object? profileData = null;

                switch (userRole)
                {
                    case "Student":
                        Console.WriteLine($"[ProfileAPI] Loading Student profile for EntityId: {entityId}");
                        if (string.IsNullOrEmpty(entityId))
                        {
                            return BadRequest(new { message = "Không tìm thấy thông tin sinh viên" });
                        }
                        var student = await _context.Students
                            .Include(s => s.Class)
                            .ThenInclude(c => c!.Department)
                            .FirstOrDefaultAsync(s => s.StudentId == entityId);
                        
                        if (student == null)
                        {
                            return NotFound(new { message = "Không tìm thấy sinh viên" });
                        }
                        profileData = student;
                        Console.WriteLine($"[ProfileAPI] Student profile loaded: {student.FullName}");
                        break;

                    case "Teacher":
                        Console.WriteLine($"[ProfileAPI] Loading Teacher profile for EntityId: {entityId}");
                        if (string.IsNullOrEmpty(entityId))
                        {
                            return BadRequest(new { message = "Không tìm thấy thông tin giáo viên" });
                        }
                        var teacher = await _context.Teachers
                            .Include(t => t.Department)
                            .FirstOrDefaultAsync(t => t.TeacherId == entityId);
                        
                        if (teacher == null)
                        {
                            return NotFound(new { message = "Không tìm thấy giáo viên" });
                        }
                        profileData = teacher;
                        Console.WriteLine($"[ProfileAPI] Teacher profile loaded: {teacher.FullName}");
                        break;

                    case "Admin":
                        Console.WriteLine($"[ProfileAPI] Loading Admin profile for UserId: {userId}");
                        // For admin, get user info from Users table
                        var user = await _context.Users
                            .FirstOrDefaultAsync(u => u.Username == userId);
                        
                        if (user == null)
                        {
                            return NotFound(new { message = "Không tìm thấy thông tin admin" });
                        }
                        
                        // Return admin info
                        profileData = new
                        {
                            userId = user.UserId,
                            username = user.Username,
                            role = user.Role,
                            entityId = user.EntityId
                        };
                        Console.WriteLine($"[ProfileAPI] Admin profile loaded: {user.Username}");
                        break;

                    default:
                        return BadRequest(new { message = "Role không hợp lệ" });
                }

                var result = new { 
                    role = userRole,
                    data = profileData 
                };
                Console.WriteLine($"[ProfileAPI] Returning profile data with role: {userRole}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProfileAPI] ERROR: {ex.Message}");
                Console.WriteLine($"[ProfileAPI] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Lỗi khi tải thông tin cá nhân", error = ex.Message });
            }
        }

        // PUT: api/profile/student
        [HttpPut("student")]
        public async Task<IActionResult> UpdateStudentProfile([FromBody] Student student)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                var entityId = HttpContext.Session.GetString("EntityId");

                if (userRole != "Student" && userRole != "Admin" && userRole != "Teacher")
                {
                    return Forbid();
                }

                if (entityId != student.StudentId && userRole == "Student")
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingStudent = await _context.Students.FindAsync(student.StudentId);
                if (existingStudent == null)
                {
                    return NotFound(new { message = "Không tìm thấy sinh viên" });
                }

                // Update allowed fields for student profile
                existingStudent.Phone = student.Phone;
                existingStudent.Address = student.Address;
                existingStudent.Email = student.Email;

                // Admin/Teacher can update more fields
                if (userRole == "Admin" || userRole == "Teacher")
                {
                    existingStudent.FullName = student.FullName;
                    existingStudent.DateOfBirth = student.DateOfBirth;
                    existingStudent.Gender = student.Gender;
                }

                _context.Entry(existingStudent).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật thông tin thành công", data = existingStudent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật thông tin", error = ex.Message });
            }
        }

        // PUT: api/profile/teacher
        [HttpPut("teacher")]
        public async Task<IActionResult> UpdateTeacherProfile([FromBody] Teacher teacher)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                var entityId = HttpContext.Session.GetString("EntityId");

                if (userRole != "Teacher" && userRole != "Admin")
                {
                    return Forbid();
                }

                if (entityId != teacher.TeacherId && userRole == "Teacher")
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingTeacher = await _context.Teachers.FindAsync(teacher.TeacherId);
                if (existingTeacher == null)
                {
                    return NotFound(new { message = "Không tìm thấy giáo viên" });
                }

                // Update allowed fields
                existingTeacher.FullName = teacher.FullName;
                existingTeacher.DateOfBirth = teacher.DateOfBirth;
                existingTeacher.Gender = teacher.Gender;
                existingTeacher.Phone = teacher.Phone;
                existingTeacher.Address = teacher.Address;
                // Note: Teacher model doesn't have Email field in current schema

                // Only admin can change department
                if (userRole == "Admin")
                {
                    existingTeacher.DepartmentId = teacher.DepartmentId;
                }

                _context.Entry(existingTeacher).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật thông tin thành công", data = existingTeacher });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật thông tin", error = ex.Message });
            }
        }

        // PUT: api/profile/admin
        [HttpPut("admin")]
        public async Task<IActionResult> UpdateAdminProfile([FromBody] UpdateAdminProfileRequest request)
        {
            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetString("UserId");

                if (userRole != "Admin")
                {
                    return Forbid();
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Chưa đăng nhập" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                // Admin can only update username
                if (!string.IsNullOrWhiteSpace(request.Username))
                {
                    // Check if username already exists (except current user)
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.UserId != user.UserId);
                    if (existingUser != null)
                    {
                        return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
                    }
                    user.Username = request.Username;
                }

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Return updated admin info
                var updatedData = new
                {
                    userId = user.UserId,
                    username = user.Username,
                    role = user.Role,
                    entityId = user.EntityId
                };

                return Ok(new { message = "Cập nhật thông tin thành công", data = updatedData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật thông tin", error = ex.Message });
            }
        }

        // PUT: api/profile/password
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Chưa đăng nhập" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                // Verify old password
                if (user.Password != request.OldPassword)
                {
                    return BadRequest(new { message = "Mật khẩu cũ không đúng" });
                }

                // Validate new password
                if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự" });
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(new { message = "Mật khẩu xác nhận không khớp" });
                }

                // Update password
                user.Password = request.NewPassword;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi đổi mật khẩu", error = ex.Message });
            }
        }
    }
}
