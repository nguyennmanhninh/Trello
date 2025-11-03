using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StudentManagementSystem.Services
{
    /// <summary>
    /// Email Service for sending verification emails, password reset, etc.
    /// Uses Gmail SMTP server
    /// </summary>
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string toEmail, string userName, string verificationCode);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;
        private readonly bool _enableSsl;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Load email settings from appsettings.json
            _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
            _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
            _senderPassword = _configuration["EmailSettings:SenderPassword"] ?? "";
            _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
            _senderName = _configuration["EmailSettings:SenderName"] ?? "Student Management System";

            _logger.LogInformation($"EmailService initialized - SMTP: {_smtpServer}:{_smtpPort}, Sender: {_senderEmail}");
        }

        /// <summary>
        /// Send verification code to user's email
        /// </summary>
        public async Task<bool> SendVerificationEmailAsync(string toEmail, string userName, string verificationCode)
        {
            try
            {
                _logger.LogInformation($"Sending verification email to {toEmail}");

                var subject = "Xác thực tài khoản - Student Management System";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .verification-code {{ background: #fff; border: 2px dashed #667eea; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; color: #667eea; margin: 20px 0; letter-spacing: 5px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎓 Xác Thực Tài Khoản</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>Student Management System</strong>!</p>
            <p>Để hoàn tất đăng ký, vui lòng sử dụng mã xác thực bên dưới:</p>
            
            <div class='verification-code'>
                {verificationCode}
            </div>
            
            <p><strong>⏰ Lưu ý:</strong> Mã xác thực này có hiệu lực trong <strong>15 phút</strong>.</p>
            
            <p>Nếu bạn không thực hiện đăng ký này, vui lòng bỏ qua email này.</p>
            
            <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
            
            <p style='color: #666; font-size: 14px;'>
                <strong>Hướng dẫn:</strong><br>
                1. Quay lại trang đăng ký<br>
                2. Nhập mã xác thực 6 số ở trên<br>
                3. Nhấn nút &quot;Xác Thực&quot;<br>
                4. Đăng nhập và bắt đầu sử dụng!
            </p>
        </div>
        <div class='footer'>
            <p>© 2025 Student Management System. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send verification email to {toEmail}");
                return false;
            }
        }

        /// <summary>
        /// Send welcome email after successful registration
        /// </summary>
        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            try
            {
                _logger.LogInformation($"Sending welcome email to {toEmail}");

                var subject = "Chào mừng đến với Student Management System!";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Chào Mừng!</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Tài khoản của bạn đã được xác thực thành công! 🎊</p>
            <p>Bạn có thể bắt đầu sử dụng <strong>Student Management System</strong> với các tính năng:</p>
            
            <ul>
                <li>📚 Quản lý thông tin sinh viên</li>
                <li>👨‍🏫 Quản lý giáo viên và lớp học</li>
                <li>📊 Quản lý điểm số và báo cáo</li>
                <li>🤖 Chatbot AI hỗ trợ 24/7</li>
            </ul>
            
            <p>Nếu bạn cần hỗ trợ, vui lòng liên hệ với chúng tôi.</p>
            
            <p>Chúc bạn có trải nghiệm tuyệt vời! 🚀</p>
        </div>
        <div class='footer'>
            <p>© 2025 Student Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {toEmail}");
                return false;
            }
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            try
            {
                _logger.LogInformation($"Sending password reset email to {toEmail}");

                var subject = "Đặt lại mật khẩu - Student Management System";
                var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔒 Đặt Lại Mật Khẩu</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            <p>Nhấn vào nút bên dưới để đặt lại mật khẩu:</p>
            
            <p style='text-align: center;'>
                <a href='{resetLink}' class='button'>Đặt Lại Mật Khẩu</a>
            </p>
            
            <p><strong>⏰ Lưu ý:</strong> Link này có hiệu lực trong <strong>30 phút</strong>.</p>
            
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
        </div>
        <div class='footer'>
            <p>© 2025 Student Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {toEmail}");
                return false;
            }
        }

        /// <summary>
        /// Core method to send email using SMTP
        /// </summary>
        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_senderPassword))
                {
                    _logger.LogError("Email settings not configured. Please set SenderEmail and SenderPassword in appsettings.json");
                    return false;
                }

                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_senderEmail, _senderPassword),
                    EnableSsl = _enableSsl,
                    Timeout = 30000 // 30 seconds timeout
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail, _senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    Priority = MailPriority.High
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation($"✅ Email sent successfully to {toEmail}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"SMTP error sending email to {toEmail}: {smtpEx.Message}");
                _logger.LogError($"SMTP Status Code: {smtpEx.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error sending email to {toEmail}");
                return false;
            }
        }
    }
}
