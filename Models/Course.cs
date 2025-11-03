using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Course
    {
        [Key]
    [StringLength(10)]
    [Display(Name = "Mã Môn Học")]
   public string CourseId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên môn học là bắt buộc")]
   [StringLength(100)]
        [Display(Name = "Tên Môn Học")]
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số tín chỉ là bắt buộc")]
[Range(1, 10, ErrorMessage = "Số tín chỉ phải từ 1 đến 10")]
        [Display(Name = "Số Tín Chỉ")]
        public int Credits { get; set; }

   [Required(ErrorMessage = "Khoa là bắt buộc")]
        [StringLength(10)]
     [Display(Name = "Mã Khoa")]
     public string DepartmentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giảng viên là bắt buộc")]
   [StringLength(10)]
        [Display(Name = "Mã Giáo Viên")]
        public string TeacherId { get; set; } = string.Empty;

        // Navigation properties
 [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher? Teacher { get; set; }

      public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
