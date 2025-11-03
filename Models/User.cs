using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("PasswordHash")] // Database column name
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // Admin, Teacher, Student

        [StringLength(10)]
        public string? EntityId { get; set; } // Links to TeacherId or StudentId

        // 🆕 Email Verification Fields
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public bool EmailVerified { get; set; } = false;

        [StringLength(6)]
        public string? VerificationCode { get; set; } // 6-digit code

        public DateTime? VerificationCodeExpiry { get; set; }

        // 🆕 Phone & Password Recovery Fields
        [StringLength(15)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        public bool PhoneVerified { get; set; } = false;

        [StringLength(6)]
        public string? ResetCode { get; set; } // 6-digit reset code for password recovery

        public DateTime? ResetCodeExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }
    }
}
