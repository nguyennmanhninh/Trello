using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    public class Department
    {
        [Key]
        [StringLength(10)]
        [Display(Name = "Mã Khoa")]
        public string DepartmentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã khoa là bắt buộc")]
        [StringLength(20)]
        [Display(Name = "Mã Khoa (Code)")]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khoa là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Tên Khoa")]
        public string DepartmentName { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
