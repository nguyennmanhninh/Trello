namespace StudentManagementSystem.Services
{
    /// <summary>
    /// SMS Service Implementation
    /// For DEVELOPMENT: Logs messages to console (no real SMS sent)
    /// For PRODUCTION: Integrate with Twilio, Vonage, or local SMS gateway
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly IConfiguration _configuration;
        private readonly bool _isDevelopment;

        public SmsService(
            ILogger<SmsService> logger, 
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _configuration = configuration;
            _isDevelopment = environment.IsDevelopment();
        }

        public async Task<bool> SendPasswordResetSmsAsync(string phoneNumber, string resetCode)
        {
            var message = $"[Student Management System]\nMã khôi phục mật khẩu: {resetCode}\nMã có hiệu lực trong 15 phút.\nKhông chia sẻ mã này với bất kỳ ai.";
            return await SendSmsAsync(phoneNumber, message);
        }

        public async Task<bool> SendPhoneVerificationSmsAsync(string phoneNumber, string verificationCode)
        {
            var message = $"[Student Management System]\nMã xác thực số điện thoại: {verificationCode}\nMã có hiệu lực trong 15 phút.";
            return await SendSmsAsync(phoneNumber, message);
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                if (_isDevelopment)
                {
                    // 🔧 DEVELOPMENT MODE: Log to console (no real SMS)
                    _logger.LogInformation("╔══════════════════════════════════════════════════════════════╗");
                    _logger.LogInformation("║              📱 SMS MESSAGE (DEV MODE)                       ║");
                    _logger.LogInformation("╠══════════════════════════════════════════════════════════════╣");
                    _logger.LogInformation($"║ To: {phoneNumber.PadRight(51)}║");
                    _logger.LogInformation("║──────────────────────────────────────────────────────────────║");
                    
                    // Split message into lines for better display
                    var lines = message.Split('\n');
                    foreach (var line in lines)
                    {
                        _logger.LogInformation($"║ {line.PadRight(59)}║");
                    }
                    
                    _logger.LogInformation("╚══════════════════════════════════════════════════════════════╝");
                    
                    // Simulate network delay
                    await Task.Delay(100);
                    return true;
                }
                else
                {
                    // 🚀 PRODUCTION MODE: Send real SMS
                    // TODO: Integrate with SMS gateway (Twilio, Vonage, etc.)
                    
                    // Example for Twilio:
                    // var accountSid = _configuration["Twilio:AccountSid"];
                    // var authToken = _configuration["Twilio:AuthToken"];
                    // var fromNumber = _configuration["Twilio:FromNumber"];
                    // TwilioClient.Init(accountSid, authToken);
                    // var messageResponse = await MessageResource.CreateAsync(
                    //     body: message,
                    //     from: new PhoneNumber(fromNumber),
                    //     to: new PhoneNumber(phoneNumber)
                    // );
                    // return messageResponse.Status == MessageResource.StatusEnum.Sent;

                    _logger.LogWarning("⚠️ SMS sending not configured for production. Message not sent.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error sending SMS to {phoneNumber}: {ex.Message}");
                return false;
            }
        }
    }
}
