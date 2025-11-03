using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    /// <summary>
    /// Represents individual student attendance record
    /// </summary>
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        public int SessionId { get; set; }

        [Required]
        [StringLength(10)]
        public string StudentId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Absent"; // Present, Absent, Late, Excused

        [DataType(DataType.Time)]
        [Display(Name = "Giờ vào lớp")]
        public TimeSpan? CheckInTime { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [StringLength(10)]
        [Display(Name = "Giáo viên điểm danh")]
        public string? MarkedByTeacherId { get; set; }

        [Display(Name = "Thời gian điểm danh")]
        public DateTime MarkedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("SessionId")]
        public virtual AttendanceSession? Session { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        [ForeignKey("MarkedByTeacherId")]
        public virtual Teacher? MarkedByTeacher { get; set; }
    }

    /// <summary>
    /// DTO for marking attendance (bulk operation)
    /// </summary>
    public class MarkAttendanceRequest
    {
        [Required]
        public int SessionId { get; set; }

        // TeacherId will be set by controller based on session/user role
        public string? TeacherId { get; set; }

        [Required]
        public List<AttendanceRecord> Attendances { get; set; } = new();
    }

    public class AttendanceRecord
    {
        [Required]
        public string StudentId { get; set; }

        [Required]
        public string Status { get; set; } // Present, Absent, Late, Excused

        public string? CheckInTime { get; set; } // "08:30"

        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for attendance statistics
    /// </summary>
    public class AttendanceStatistics
    {
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    /// <summary>
    /// DTO for attendance warnings (students with low attendance)
    /// </summary>
    public class AttendanceWarning
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int TotalSessions { get; set; }
        public int AbsentCount { get; set; }
        public decimal AbsentRate { get; set; }
    }
}
