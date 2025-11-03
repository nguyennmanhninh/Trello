namespace StudentManagementSystem.Services
{
    /// <summary>
    /// Interface for SMS sending service
    /// Supports password recovery and phone verification via SMS
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// Send password reset code via SMS
        /// </summary>
        Task<bool> SendPasswordResetSmsAsync(string phoneNumber, string resetCode);

        /// <summary>
        /// Send phone verification code via SMS
        /// </summary>
        Task<bool> SendPhoneVerificationSmsAsync(string phoneNumber, string verificationCode);

        /// <summary>
        /// Send generic SMS message
        /// </summary>
        Task<bool> SendSmsAsync(string phoneNumber, string message);
    }
}
