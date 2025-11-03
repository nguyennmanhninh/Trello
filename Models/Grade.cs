using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Grade
    {
  [Required]
        [StringLength(10)]
   [Display(Name = "Mã Sinh Viên")]
   public string StudentId { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
[Display(Name = "Mã Môn Học")]
        public string CourseId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Điểm là bắt buộc")]
      [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
     [Display(Name = "Điểm")]
        [Column(TypeName = "decimal(3, 2)")]
        public decimal Score { get; set; }

   [Required]
   [StringLength(50)]
        [Display(Name = "Xếp Loại")]
 public string Classification { get; set; } = string.Empty;

      // Navigation properties
[ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

   [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
    }
}
