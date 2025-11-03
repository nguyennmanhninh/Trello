using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Student
    {
        [Key]
        [StringLength(10)]
   [Display(Name = "Mã Sinh Viên")]
    public string StudentId { get; set; } = string.Empty;

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

    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string? Email { get; set; } // Optional email field

    [Required]
        [StringLength(15)]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số Điện Thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
     [Display(Name = "Địa Chỉ")]
      public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lớp là bắt buộc")]
      [StringLength(10)]
        [Display(Name = "Mã Lớp")]
   public string ClassId { get; set; } = string.Empty;

[Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50)]
        [Display(Name = "Tên Đăng Nhập")]
public string Username { get; set; } = string.Empty;

 [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật Khẩu")]
public string Password { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }

        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
