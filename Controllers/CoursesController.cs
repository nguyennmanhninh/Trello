using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Filters;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICourseService _courseService;

        public CoursesController(ApplicationDbContext context, ICourseService courseService)
        {
            _context = context;
            _courseService = courseService;
        }

        // GET: Courses
        [AuthorizeRole("Admin", "Teacher", "Student")]
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Use Stored Procedure via CourseService
            // Get all courses (up to 100) - SP handles role-based filtering
            var result = await _courseService.GetCoursesAsync(
                userRole ?? "Student", 
                userId ?? "", 
                null, // searchString
                null, // departmentId
                1,    // pageNumber
                100   // pageSize (get all courses)
            );

            return View(result.Courses);
        }

        // GET: Courses/Details/5
 [AuthorizeRole("Admin", "Teacher", "Student")]
        public async Task<IActionResult> Details(string id)
     {
     if (id == null)
     {
    return NotFound();
     }

      var course = await _context.Courses
  .Include(c => c.Department)
     .Include(c => c.Teacher)
   .FirstOrDefaultAsync(m => m.CourseId == id);

    if (course == null)
   {
   return NotFound();
            }

return View(course);
  }

  // GET: Courses/Create
 [AuthorizeRole("Admin", "Teacher")]
        public IActionResult Create()
  {
    var userRole = HttpContext.Session.GetString("UserRole");
    var userId = HttpContext.Session.GetString("UserId");

    ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
    
    // Teacher can only assign themselves
    if (userRole == "Teacher")
    {
        ViewData["TeacherId"] = new SelectList(_context.Teachers.Where(t => t.TeacherId == userId), "TeacherId", "FullName");
    }
    else
    {
        ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName");
    }
    
    return View();
    }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Create([Bind("CourseId,CourseName,Credits,DepartmentId,TeacherId")] Course course)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Teacher can only create courses for themselves
            if (userRole == "Teacher" && course.TeacherId != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                // Use Stored Procedure via CourseService
                bool success = await _courseService.CreateCourseAsync(course, userRole ?? "Admin", userId ?? "");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm môn học thành công";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Không thể thêm môn học. Mã môn học có thể đã tồn tại.");
                }
            }
           
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", course.DepartmentId);
        
            if (userRole == "Teacher")
            {
                ViewData["TeacherId"] = new SelectList(_context.Teachers.Where(t => t.TeacherId == userId), "TeacherId", "FullName", course.TeacherId);
            }
            else
            {
                ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", course.TeacherId);
            }
        
            return View(course);
        }

        // GET: Courses/Edit/5
 [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Edit(string id)
        {
    var userRole = HttpContext.Session.GetString("UserRole");
    var userId = HttpContext.Session.GetString("UserId");

 if (id == null)
      {
    return NotFound();
}

       var course = await _context.Courses.FindAsync(id);
 if (course == null)
     {
  return NotFound();
       }

    // Teacher can only edit their own courses
    if (userRole == "Teacher" && course.TeacherId != userId)
    {
        return RedirectToAction("AccessDenied", "Account");
    }
  
   ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", course.DepartmentId);
    
    if (userRole == "Teacher")
    {
        ViewData["TeacherId"] = new SelectList(_context.Teachers.Where(t => t.TeacherId == userId), "TeacherId", "FullName", course.TeacherId);
    }
    else
    {
        ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", course.TeacherId);
    }
    
  return View(course);
}

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Edit(string id, [Bind("CourseId,CourseName,Credits,DepartmentId,TeacherId")] Course course)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Teacher can only edit their own courses
            if (userRole == "Teacher" && course.TeacherId != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id != course.CourseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Use Stored Procedure via CourseService
                bool success = await _courseService.UpdateCourseAsync(course, userRole ?? "Admin", userId ?? "");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật môn học thành công";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Không thể cập nhật môn học. Kiểm tra quyền hoặc dữ liệu đầu vào.");
                }
            }
           
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", course.DepartmentId);
        
            if (userRole == "Teacher")
            {
                ViewData["TeacherId"] = new SelectList(_context.Teachers.Where(t => t.TeacherId == userId), "TeacherId", "FullName", course.TeacherId);
            }
            else
            {
                ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", course.TeacherId);
            }
        
            return View(course);
        }

      // GET: Courses/Delete/5
   [AuthorizeRole("Admin", "Teacher")]
 public async Task<IActionResult> Delete(string id)
        {
    var userRole = HttpContext.Session.GetString("UserRole");
    var userId = HttpContext.Session.GetString("UserId");

  if (id == null)
   {
    return NotFound();
  }

    var course = await _context.Courses
   .Include(c => c.Department)
        .Include(c => c.Teacher)
     .FirstOrDefaultAsync(m => m.CourseId == id);

   if (course == null)
{
 return NotFound();
   }

    // Teacher can only delete their own courses
    if (userRole == "Teacher" && course.TeacherId != userId)
    {
        return RedirectToAction("AccessDenied", "Account");
    }

      return View(course);
}

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Use Stored Procedure via CourseService
            bool success = await _courseService.DeleteCourseAsync(id, userRole ?? "Admin");
            
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa môn học thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa môn học. Có thể do có dữ liệu điểm liên quan hoặc không có quyền.";
            }
            
            return RedirectToAction(nameof(Index));
        }

 private bool CourseExists(string id)
{
    return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
