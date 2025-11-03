using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Filters;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITeacherService _teacherService;

        public TeachersController(ApplicationDbContext context, ITeacherService teacherService)
        {
            _context = context;
            _teacherService = teacherService;
        }

        // GET: Teachers
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Index(string searchString, string departmentId, int? pageNumber)
  {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Use Stored Procedure via TeacherService
            int pageSize = 10;
            var result = await _teacherService.GetTeachersAsync(
                userRole ?? "Admin",
                userId ?? "",
                searchString,
                departmentId,
                pageNumber ?? 1,
                pageSize
            );

            List<Teacher> teachers = result.Teachers;
            int totalCount = result.TotalCount;

            // Create PaginatedList from SP results
            var paginatedList = new PaginatedList<Teacher>(
                teachers,
                totalCount,
                pageNumber ?? 1,
                pageSize
            );

            // Preserve filter values for view
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentDepartment"] = departmentId;
            ViewData["Departments"] = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName");

            return View(paginatedList);
  }

        // GET: Teachers/Details/5
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .Include(t => t.Department)
                .Include(t => t.Classes)
                    .ThenInclude(c => c.Department)
                .Include(t => t.Classes)
                    .ThenInclude(c => c.Students)
                .Include(t => t.Courses)
                    .ThenInclude(c => c.Department)
                .FirstOrDefaultAsync(m => m.TeacherId == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Teachers/Create
        [AuthorizeRole("Admin")]
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Create([Bind("TeacherId,FullName,DateOfBirth,Gender,Phone,Address,Username,Password,DepartmentId")] Teacher teacher)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (ModelState.IsValid)
            {
                // Use Stored Procedure via TeacherService
                bool success = await _teacherService.CreateTeacherAsync(teacher, userRole ?? "Admin", userId ?? "");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm giáo viên thành công";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Mã giáo viên hoặc tên đăng nhập đã tồn tại hoặc không có quyền thêm");
                }
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", teacher.DepartmentId);
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Teacher can only edit their own info
            if (userRole == "Teacher" && id != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", teacher.DepartmentId);
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Edit(string id, [Bind("TeacherId,FullName,DateOfBirth,Gender,Phone,Address,DepartmentId")] Teacher teacher)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            // Teacher can only edit their own info
            if (userRole == "Teacher" && id != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id != teacher.TeacherId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Get existing teacher to preserve Username and Password
                var existingTeacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.TeacherId == id);
                if (existingTeacher == null)
                {
                    return NotFound();
                }

                // Preserve Username and Password
                teacher.Username = existingTeacher.Username;
                teacher.Password = existingTeacher.Password;

                // Use Stored Procedure via TeacherService
                bool success = await _teacherService.UpdateTeacherAsync(teacher, userRole ?? "Admin", userId ?? "");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật giáo viên thành công";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Không thể cập nhật giáo viên. Kiểm tra quyền hoặc dữ liệu đầu vào.");
                }
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", teacher.DepartmentId);
            return View(teacher);
        }

        // GET: Teachers/EditProfile - Teacher can edit their own profile
        [AuthorizeRole("Teacher")]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacher = await _context.Teachers
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.TeacherId == userId);
            
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Teacher")]
        public async Task<IActionResult> EditProfile([Bind("TeacherId,FullName,DateOfBirth,Gender,Phone,Address,DepartmentId")] Teacher teacher, string? currentPassword, string? newPassword)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (teacher.TeacherId != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var existingTeacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.TeacherId == teacher.TeacherId);
            if (existingTeacher == null)
            {
                return NotFound();
            }

            // Keep the existing username and password
            teacher.Username = existingTeacher.Username;
            teacher.Password = existingTeacher.Password;

            // If changing password
            if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
            {
                if (existingTeacher.Password == currentPassword)
                {
                    teacher.Password = newPassword;
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công";
                    return RedirectToAction("Index", "Dashboard");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Có lỗi khi cập nhật thông tin");
                }
            }

            return View(teacher);
        }

        // GET: Teachers/Delete/5
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .Include(t => t.Department)
                .FirstOrDefaultAsync(m => m.TeacherId == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            // Use Stored Procedure via TeacherService
            bool success = await _teacherService.DeleteTeacherAsync(id, userRole ?? "Admin");
            
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa giáo viên thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa giáo viên này. Có thể do có dữ liệu liên quan (lớp/môn học) hoặc không có quyền.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(string id)
   {
        return _context.Teachers.Any(e => e.TeacherId == id);
   }
    }
}
