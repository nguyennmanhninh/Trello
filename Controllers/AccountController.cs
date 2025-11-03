using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models.ViewModels;
using StudentManagementSystem.Services;
using StudentManagementSystem.Filters;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace StudentManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public AccountController(
            IAuthService authService, 
            ILogger<AccountController> logger,
            IEmailService emailService,
            ApplicationDbContext context)
        {
            _authService = authService;
            _logger = logger;
            _emailService = emailService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                _logger.LogInformation("User already logged in, redirecting to dashboard");
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation("=== LOGIN ATTEMPT ===");
            _logger.LogInformation($"Username: {model.Username}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed");
                return View(model);
            }

            _logger.LogInformation("Calling AuthService.AuthenticateAsync...");
            var result = await _authService.AuthenticateAsync(model.Username, model.Password);

            _logger.LogInformation($"Auth result - Success: {result.Success}, Role: {result.Role}, EntityId: {result.EntityId}");

            if (result.Success)
            {
                _logger.LogInformation("Authentication successful, setting session...");

                // Set session values
                HttpContext.Session.SetString("UserId", model.Username); // ✅ Username for authentication
                HttpContext.Session.SetString("EntityId", result.EntityId); // ✅ Entity ID for queries
                HttpContext.Session.SetString("UserRole", result.Role);
                HttpContext.Session.SetString("UserName", result.FullName);
                HttpContext.Session.SetString("Username", model.Username);

                _logger.LogInformation($"Session values set - UserId: {model.Username}, EntityId: {result.EntityId}, Role: {result.Role}");

                // Commit session changes
                await HttpContext.Session.CommitAsync();
                _logger.LogInformation("Session committed");

                // Verify session was saved
                var savedUserId = HttpContext.Session.GetString("UserId");
                var savedRole = HttpContext.Session.GetString("UserRole");
                _logger.LogInformation($"Verification - UserId: {savedUserId}, Role: {savedRole}");

                if (savedUserId == result.EntityId && savedRole == result.Role)
                {
                    _logger.LogInformation("✓ Session verification successful!");
                    TempData["SuccessMessage"] = $"Chào mừng {result.FullName}!";

                    _logger.LogInformation("Redirecting to Dashboard...");
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    _logger.LogError("❌ Session verification FAILED!");
                    _logger.LogError($"Expected UserId: {result.EntityId}, Got: {savedUserId}");
                    _logger.LogError($"Expected Role: {result.Role}, Got: {savedRole}");
                    ModelState.AddModelError("", "Lỗi hệ thống: Session không được lưu. Vui lòng thử lại.");
                    return View(model);
                }
            }

            _logger.LogWarning("Authentication failed");
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _logger.LogInformation("User logging out");
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đã đăng xuất thành công";
            return RedirectToAction("Login");
        }

        // ==================== REGISTRATION ENDPOINTS ====================

        [HttpGet]
        public IActionResult Register()
        {
            // If already logged in, redirect to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogInformation($"=== REGISTRATION ATTEMPT === Username: {model.Username}, Email: {model.Email}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed");
                return View(model);
            }

            try
            {
                // Check if username already exists
                var existingUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Check if email already exists
                var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Generate 6-digit verification code
                var verificationCode = GenerateVerificationCode();
                _logger.LogInformation($"Generated verification code: {verificationCode}");

                // Hash password (using simple hash for now, should use BCrypt in production)
                var hashedPassword = HashPassword(model.Password);

                // Create new user
                var newUser = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = hashedPassword,
                    Role = model.Role,
                    EmailVerified = false,
                    VerificationCode = verificationCode,
                    VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15), // Code expires in 15 minutes
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✓ User created with ID: {newUser.UserId}");

                // 🚀 DUAL MODE: Vừa hiển thị mã, vừa gửi email thật
                _logger.LogInformation($"🔑 Verification Code for {model.Email}: {verificationCode}");
                
                // Gửi email thật đến Gmail của user
                var emailSent = await _emailService.SendVerificationEmailAsync(
                    model.Email, 
                    model.Username, 
                    verificationCode
                );

                if (emailSent)
                {
                    _logger.LogInformation($"✅ Verification email sent to {model.Email}");
                    TempData["SuccessMessage"] = $@"
                        <div style='text-align: center;'>
                            <p>✅ Đăng ký thành công!</p>
                            <p>📧 Email xác thực đã được gửi đến <strong>{model.Email}</strong></p>
                            <hr style='margin: 20px 0; border: none; border-top: 1px solid #dee2e6;'>
                            <p style='color: #666;'>Hoặc sử dụng mã này ngay:</p>
                            <div style='font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 8px; font-family: monospace; background: #f8f9fa; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                                {verificationCode}
                            </div>
                            <small style='color: #999;'>Mã có hiệu lực trong 15 phút</small>
                        </div>
                    ";
                    TempData["Email"] = model.Email;
                    TempData["VerificationCode"] = verificationCode;
                    return RedirectToAction("VerifyEmail");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Failed to send email, but showing code anyway");
                    TempData["InfoMessage"] = $@"
                        <div style='text-align: center;'>
                            <p>✅ Đăng ký thành công!</p>
                            <p>⚠️ Không thể gửi email, nhưng bạn có thể sử dụng mã này:</p>
                            <div style='font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 8px; font-family: monospace; background: #f8f9fa; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                                {verificationCode}
                            </div>
                        </div>
                    ";
                    TempData["Email"] = model.Email;
                    TempData["VerificationCode"] = verificationCode;
                    return RedirectToAction("VerifyEmail");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại.");
                return View(model);
            }
        }

        // ==================== EMAIL VERIFICATION ENDPOINTS ====================

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            var email = TempData["Email"]?.ToString();
            if (!string.IsNullOrEmpty(email))
            {
                ViewBag.Email = email;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            _logger.LogInformation($"=== EMAIL VERIFICATION ATTEMPT === Email: {model.Email}, Code: {model.VerificationCode}");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Email không tồn tại trong hệ thống");
                    return View(model);
                }

                if (user.EmailVerified)
                {
                    TempData["InfoMessage"] = "Email đã được xác thực trước đó. Bạn có thể đăng nhập.";
                    return RedirectToAction("Login");
                }

                if (string.IsNullOrEmpty(user.VerificationCode))
                {
                    ModelState.AddModelError("", "Không tìm thấy mã xác thực. Vui lòng đăng ký lại.");
                    return View(model);
                }

                // Check if code expired
                if (user.VerificationCodeExpiry < DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "Mã xác thực đã hết hạn. Vui lòng yêu cầu mã mới.");
                    ViewBag.ShowResendButton = true;
                    return View(model);
                }

                // Verify code
                if (user.VerificationCode != model.VerificationCode)
                {
                    ModelState.AddModelError("VerificationCode", "Mã xác thực không đúng");
                    return View(model);
                }

                // Mark email as verified
                user.EmailVerified = true;
                user.VerificationCode = null;
                user.VerificationCodeExpiry = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✓ Email verified for user: {user.Username}");

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.Username);

                TempData["SuccessMessage"] = "Xác thực email thành công! Bạn có thể đăng nhập ngay bây giờ.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình xác thực. Vui lòng thử lại.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode(string email)
        {
            _logger.LogInformation($"=== RESEND VERIFICATION CODE === Email: {email}");

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Email không tồn tại trong hệ thống";
                    return RedirectToAction("VerifyEmail");
                }

                if (user.EmailVerified)
                {
                    TempData["InfoMessage"] = "Email đã được xác thực trước đó.";
                    return RedirectToAction("Login");
                }

                // Generate new verification code
                var verificationCode = GenerateVerificationCode();
                user.VerificationCode = verificationCode;
                user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();

                // Send new verification email
                var emailSent = await _emailService.SendVerificationEmailAsync(
                    user.Email,
                    user.Username,
                    verificationCode
                );

                if (emailSent)
                {
                    _logger.LogInformation($"✓ New verification code sent to {email}");
                    TempData["SuccessMessage"] = "Mã xác thực mới đã được gửi đến email của bạn.";
                }
                else
                {
                    _logger.LogError($"✗ Failed to send verification email to {email}");
                    TempData["ErrorMessage"] = "Không thể gửi email. Vui lòng thử lại sau.";
                }

                return RedirectToAction("VerifyEmail");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification code");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi. Vui lòng thử lại.";
                return RedirectToAction("VerifyEmail");
            }
        }

        // ==================== HELPER METHODS ====================

        private string GenerateVerificationCode()
        {
            // Generate random 6-digit number
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }

        private string HashPassword(string password)
        {
            // Simple SHA256 hash (should use BCrypt or Argon2 in production)
            using var sha256 = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // ==================== EXISTING ENDPOINTS ====================

        [AuthorizeRole("Admin", "Teacher", "Student")]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [AuthorizeRole("Admin", "Teacher", "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login");
            }

            var result = await _authService.ChangePasswordAsync(username, role, model.CurrentPassword, model.NewPassword);

            if (result)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công";
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
            return View(model);
        }

        public IActionResult AccessDenied()
        {
            ViewBag.Message = "B?n kh�ng c� quy?n truy c?p trang n�y";
            return View();
        }
    }
}
