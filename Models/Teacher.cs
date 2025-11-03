using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    public class Teacher
    {
        [Key]
        [StringLength(10)]
        [Display(Name = "Mã Giáo Viên")]
        public string TeacherId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày Sinh")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Giới Tính")]
      public bool Gender { get; set; } // true = Male, false = Female

        [Required]
        [StringLength(15)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số Điện Thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Địa Chỉ")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
      [StringLength(50)]
   [Display(Name = "Tên Đăng Nhập")]
 public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật Khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Khoa là bắt buộc")]
        [StringLength(10)]
        [Display(Name = "Mã Khoa")]
        public string DepartmentId { get; set; } = string.Empty;

        // Navigation properties
        public virtual Department? Department { get; set; }
        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
