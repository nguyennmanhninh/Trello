using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Class
    {
        [Key]
    [StringLength(10)]
        [Display(Name = "Mã Lớp")]
        public string ClassId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên lớp là bắt buộc")]
        [StringLength(100)]
  [Display(Name = "Tên Lớp")]
    public string ClassName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Khoa là bắt buộc")]
 [StringLength(10)]
        [Display(Name = "Mã Khoa")]
        public string DepartmentId { get; set; } = string.Empty;

[Required(ErrorMessage = "Giáo viên chủ nhiệm là bắt buộc")]
   [StringLength(10)]
      [Display(Name = "Mã Giáo Viên")]
   public string TeacherId { get; set; } = string.Empty;

   // Navigation properties
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [ForeignKey("TeacherId")]
     public virtual Teacher? Teacher { get; set; }

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
