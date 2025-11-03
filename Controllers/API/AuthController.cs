using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Services;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace StudentManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService, 
            IJwtService jwtService,
            IEmailService emailService,
            ISmsService smsService,
            ApplicationDbContext context,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _jwtService = jwtService;
            _emailService = emailService;
            _smsService = smsService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return Ok(new { 
                    success = false,
                    message = "Username và password là bắt buộc" 
                });
            }

            var result = await _authService.AuthenticateAsync(request.Username, request.Password);

            if (!result.Success)
            {
                return Ok(new { 
                    success = false,
                    message = "Tên đăng nhập hoặc mật khẩu không đúng" 
                });
            }

            // ✅ Create session for API calls (important for profile and other session-based endpoints)
            HttpContext.Session.SetString("UserId", request.Username);
            HttpContext.Session.SetString("UserRole", result.Role!);
            HttpContext.Session.SetString("EntityId", result.EntityId!);
            HttpContext.Session.SetString("UserName", result.FullName!);
            await HttpContext.Session.CommitAsync();

            _logger.LogInformation($"[API Login] Session created - UserId: {request.Username}, Role: {result.Role}, EntityId: {result.EntityId}");

            // Generate JWT token
            var token = _jwtService.GenerateToken(
                result.EntityId!,
                result.Role!,
                result.FullName!,
                request.Username
            );

            return Ok(new
            {
                success = true,
                message = "Đăng nhập thành công",
                token = token,
                user = new
                {
                    userId = result.EntityId,
                    username = request.Username,
                    role = result.Role,
                    fullName = result.FullName,
                    entityId = result.EntityId
                }
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "Vui lòng đăng nhập" });
            }

            var result = await _authService.ChangePasswordAsync(
                userId,
                role,
                request.OldPassword,
                request.NewPassword
            );

            if (!result)
            {
                return BadRequest(new { message = "Mật khẩu cũ không đúng" });
            }

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }

        // ============================================
        // Registration & Email Verification Endpoints
        // ============================================

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation($"API Registration attempt: {request.Username}, {request.Email}");

            try
            {
                // Check username exists
                var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
                if (existingUsername != null)
                {
                    return Ok(new { success = false, message = "Tên đăng nhập đã tồn tại" });
                }

                // Check email exists
                var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingEmail != null)
                {
                    return Ok(new { success = false, message = "Email đã được sử dụng" });
                }

                // Generate verification code
                var verificationCode = GenerateVerificationCode();
                _logger.LogInformation($"Generated code: {verificationCode}");

                // Hash password
                var hashedPassword = HashPassword(request.Password);

                // Create new user
                var newUser = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    Password = hashedPassword,
                    Role = request.Role,
                    EmailVerified = false,
                    VerificationCode = verificationCode,
                    VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User created with ID: {newUser.UserId}");

                // Attempt to send email (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendVerificationEmailAsync(request.Email, request.Username, verificationCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Email send failed: {ex.Message}");
                    }
                });

                return Ok(new
                {
                    success = true,
                    message = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực.",
                    verificationCode = verificationCode, // For testing/development
                    email = request.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                return Ok(new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            _logger.LogInformation($"Email verification attempt: {request.Email}");

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return Ok(new { success = false, message = "Email không tồn tại" });
                }

                if (user.EmailVerified)
                {
                    return Ok(new { success = false, message = "Email đã được xác thực" });
                }

                if (string.IsNullOrEmpty(user.VerificationCode))
                {
                    return Ok(new { success = false, message = "Mã xác thực không tồn tại" });
                }

                if (user.VerificationCodeExpiry < DateTime.UtcNow)
                {
                    return Ok(new { success = false, message = "Mã xác thực đã hết hạn" });
                }

                if (user.VerificationCode != request.Code)
                {
                    return Ok(new { success = false, message = "Mã xác thực không đúng" });
                }

                // Mark as verified
                user.EmailVerified = true;
                user.VerificationCode = null;
                user.VerificationCodeExpiry = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Email verified for user: {user.Username}");

                // Send welcome email (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(user.Email, user.Username);
                    }
                    catch { }
                });

                return Ok(new
                {
                    success = true,
                    message = "Xác thực email thành công! Bạn có thể đăng nhập ngay bây giờ."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Verification error: {ex.Message}");
                return Ok(new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }

        [HttpPost("resend-code")]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendCodeRequest request)
        {
            _logger.LogInformation($"Resend code request: {request.Email}");

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return Ok(new { success = false, message = "Email không tồn tại" });
                }

                if (user.EmailVerified)
                {
                    return Ok(new { success = false, message = "Email đã được xác thực" });
                }

                // Generate new code
                var verificationCode = GenerateVerificationCode();
                user.VerificationCode = verificationCode;
                user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New code generated: {verificationCode}");

                // Attempt to send email
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendVerificationEmailAsync(user.Email, user.Username, verificationCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Email send failed: {ex.Message}");
                    }
                });

                return Ok(new
                {
                    success = true,
                    message = "Mã xác thực mới đã được gửi",
                    verificationCode = verificationCode // For testing
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Resend error: {ex.Message}");
                return Ok(new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }

        // ============================================
        // Helper Methods
        // ============================================

        private string GenerateVerificationCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // ============================================
        // Password Recovery via Phone Number
        // ============================================

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            _logger.LogInformation($"Password recovery attempt for phone: {request.Phone}");

            try
            {
                // Find user by phone number
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);
                
                if (user == null)
                {
                    // Don't reveal if phone exists for security
                    return Ok(new { 
                        success = true, 
                        message = "Nếu số điện thoại tồn tại, mã khôi phục đã được gửi" 
                    });
                }

                // Generate reset code
                var resetCode = GenerateVerificationCode();
                user.ResetCode = resetCode;
                user.ResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Reset code generated: {resetCode} for user: {user.Username}");

                // Send SMS (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _smsService.SendPasswordResetSmsAsync(request.Phone, resetCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"SMS send failed: {ex.Message}");
                    }
                });

                return Ok(new
                {
                    success = true,
                    message = "Mã khôi phục đã được gửi đến số điện thoại của bạn",
                    resetCode = resetCode, // ⚠️ For development only - remove in production
                    phone = request.Phone
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Forgot password error: {ex.Message}");
                return Ok(new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequest request)
        {
            _logger.LogInformation($"Reset code verification for phone: {request.Phone}");

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);

                if (user == null)
                {
                    return Ok(new { success = false, message = "Số điện thoại không tồn tại" });
                }

                if (string.IsNullOrEmpty(user.ResetCode))
                {
                    return Ok(new { success = false, message = "Mã khôi phục không tồn tại" });
                }

                if (user.ResetCodeExpiry < DateTime.UtcNow)
                {
                    return Ok(new { success = false, message = "Mã khôi phục đã hết hạn" });
                }

                if (user.ResetCode != request.Code)
                {
                    return Ok(new { success = false, message = "Mã khôi phục không đúng" });
                }

                // Code is valid - return token for password reset
                return Ok(new
                {
                    success = true,
                    message = "Mã khôi phục hợp lệ",
                    resetToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user.UserId}:{user.ResetCode}:{DateTime.UtcNow.Ticks}"))
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Verify reset code error: {ex.Message}");
                return Ok(new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _logger.LogInformation($"Password reset attempt for phone: {request.Phone}");

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone);

                if (user == null)
                {
                    return Ok(new { success = false, message = "Số điện thoại không tồn tại" });
                }

                if (string.IsNullOrEmpty(user.ResetCode))
                {
                    return Ok(new { success = false, message = "Mã khôi phục không tồn tại" });
                }

                if (user.ResetCodeExpiry < DateTime.UtcNow)
                {
                    return Ok(new { success = false, message = "Mã khôi phục đã hết hạn" });
                }

                if (user.ResetCode != request.Code)
                {
                    return Ok(new { success = false, message = "Mã khôi phục không đúng" });
                }

                // Reset password
                user.Password = HashPassword(request.NewPassword);
                user.ResetCode = null;
                user.ResetCodeExpiry = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset successful for user: {user.Username}");

                return Ok(new
                {
                    success = true,
                    message = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập ngay bây giờ."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reset password error: {ex.Message}");
                return Ok(new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }
    }

    // ============================================
    // Request Models
    // ============================================

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Role { get; set; } = "Student";
        public string? FullName { get; set; }
    }

    public class VerifyEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class ResendCodeRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string Phone { get; set; } = string.Empty;
    }

    public class VerifyResetCodeRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
