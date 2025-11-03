using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Filters;
using StudentManagementSystem.Models.ViewModels;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers
{
    [AuthorizeRole("Admin", "Teacher", "Student")]
    public class DashboardController : Controller
    {
   private readonly ApplicationDbContext _context;
 private readonly IStatisticsService _statisticsService;

     public DashboardController(ApplicationDbContext context, IStatisticsService statisticsService)
   {
   _context = context;
 _statisticsService = statisticsService;
  }

   public async Task<IActionResult> Index()
{
   var userRole = HttpContext.Session.GetString("UserRole");
  var userId = HttpContext.Session.GetString("UserId");
   var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
      {
     return RedirectToAction("Login", "Account");
   }

    var model = new DashboardViewModel
     {
 UserRole = userRole,
         UserName = userName ?? "",
  EntityId = userId
    };

            if (userRole == "Admin")
     {
  model.TotalStudents = await _statisticsService.GetTotalStudentsAsync();
   model.TotalTeachers = await _statisticsService.GetTotalTeachersAsync();
        model.TotalClasses = await _statisticsService.GetTotalClassesAsync();
      model.TotalCourses = await _statisticsService.GetTotalCoursesAsync();
        model.TotalDepartments = await _statisticsService.GetTotalDepartmentsAsync();
         }
  else if (userRole == "Teacher")
            {
        model.TeacherClasses = await _context.Classes
     .Include(c => c.Department)
      .Where(c => c.TeacherId == userId)
       .ToListAsync();

     model.TeacherCourses = await _context.Courses
      .Include(c => c.Department)
      .Where(c => c.TeacherId == userId)
         .ToListAsync();
   }
     else if (userRole == "Student")
    {
   var student = await _context.Students
    .Include(s => s.Class)
   .ThenInclude(c => c.Department)
        .FirstOrDefaultAsync(s => s.StudentId == userId);

      model.StudentClass = student?.Class;

         model.StudentGrades = await _context.Grades
 .Include(g => g.Course)
   .Where(g => g.StudentId == userId)
     .ToListAsync();

         if (model.StudentGrades.Any())
            {
   model.AverageScore = await _statisticsService.GetAverageScoreByStudentAsync(userId);
            }
  }

    return View(model);
        }
    }
}
