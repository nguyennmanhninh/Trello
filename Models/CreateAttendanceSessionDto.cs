using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    /// <summary>
    /// DTO for creating an attendance session
    /// TeacherId will be set by backend based on session/role
    /// </summary>
    public class CreateAttendanceSessionDto
    {
        [Required(ErrorMessage = "CourseId là bắt buộc")]
        [StringLength(10)]
        public string CourseId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày học là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime SessionDate { get; set; }

        [Required(ErrorMessage = "Giờ học là bắt buộc")]
        public string SessionTime { get; set; } = string.Empty; // "08:00" format, will be converted to TimeSpan

        [Required(ErrorMessage = "Tiêu đề buổi học là bắt buộc")]
        [StringLength(200)]
        public string SessionTitle { get; set; } = string.Empty;

        [StringLength(50)]
        public string SessionType { get; set; } = "Lý thuyết"; // Lý thuyết, Thực hành, Kiểm tra

        [StringLength(100)]
        public string? Location { get; set; }

        public int Duration { get; set; } = 90; // minutes

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
