using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    /// <summary>
    /// Represents an attendance session (buổi điểm danh)
    /// </summary>
    public class AttendanceSession
    {
        [Key]
        public int SessionId { get; set; }

        [Required]
        [StringLength(10)]
        public string CourseId { get; set; }

        [Required]
        [StringLength(10)]
        public string TeacherId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày học")]
        public DateTime SessionDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ học")]
        public TimeSpan SessionTime { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Tiêu đề buổi học")]
        public string SessionTitle { get; set; }

        [StringLength(50)]
        [Display(Name = "Loại buổi học")]
        public string SessionType { get; set; } = "Lý thuyết"; // Lý thuyết, Thực hành, Kiểm tra

        [StringLength(100)]
        [Display(Name = "Phòng học")]
        public string? Location { get; set; }

        [Display(Name = "Thời lượng (phút)")]
        public int Duration { get; set; } = 90;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher? Teacher { get; set; }

        public virtual ICollection<Attendance>? Attendances { get; set; }

        // Computed properties
        [NotMapped]
        public int TotalStudents => Attendances?.Count ?? 0;

        [NotMapped]
        public int PresentCount => Attendances?.Count(a => a.Status == "Present") ?? 0;

        [NotMapped]
        public int AbsentCount => Attendances?.Count(a => a.Status == "Absent") ?? 0;

        [NotMapped]
        public int LateCount => Attendances?.Count(a => a.Status == "Late") ?? 0;

        [NotMapped]
        public decimal AttendanceRate => TotalStudents > 0 
            ? Math.Round((decimal)PresentCount / TotalStudents * 100, 2) 
            : 0;
    }
}
