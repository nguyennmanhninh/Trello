namespace StudentManagementSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string UserRole { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        
      // Statistics
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
        public int TotalCourses { get; set; }
     public int TotalDepartments { get; set; }
        
        // For teachers
        public List<Class>? TeacherClasses { get; set; }
        public List<Course>? TeacherCourses { get; set; }
        
// For students
  public Class? StudentClass { get; set; }
    public List<Grade>? StudentGrades { get; set; }
        public double? AverageScore { get; set; }
    }
}
