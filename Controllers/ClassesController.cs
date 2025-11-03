using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Filters;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers
{
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IClassService _classService;

        public ClassesController(ApplicationDbContext context, IClassService classService)
        {
            _context = context;
            _classService = classService;
        }

        // GET: Classes
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            
            // Use Stored Procedure via ClassService
            var result = await _classService.GetClassesAsync(
                userRole ?? "Student",
                userId ?? "",
                null,  // searchString
                null,  // departmentId
                1,     // pageNumber
                100    // pageSize (get all for Index view)
            );
            
            return View(result.Classes);
        }

        // GET: Classes/Details/5
        [AuthorizeRole("Admin", "Teacher")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            var @class = await _context.Classes
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .FirstOrDefaultAsync(m => m.ClassId == id);
        
            if (@class == null)
            {
                return NotFound();
            }

            // Teacher can only view their own classes
            if (userRole == "Teacher" && @class.TeacherId != userId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(@class);
        }

        // GET: Classes/Create
        [AuthorizeRole("Admin")]
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName");
            return View();
        }

        // POST: Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Create([Bind("ClassId,ClassName,DepartmentId,TeacherId")] Class @class)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (ModelState.IsValid)
            {
                // Use Stored Procedure via ClassService
                bool success = await _classService.CreateClassAsync(@class, userRole ?? "Admin", userId ?? "");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm lớp học thành công";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Mã lớp đã tồn tại hoặc không có quyền thêm");
                }
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", @class.DepartmentId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", @class.TeacherId);
            return View(@class);
        }

        // GET: Classes/Edit/5
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", @class.DepartmentId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", @class.TeacherId);
            return View(@class);
        }

        // POST: Classes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Edit(string id, [Bind("ClassId,ClassName,DepartmentId,TeacherId")] Class @class)
        {
            if (id != @class.ClassId)
       {
      return NotFound();
    }

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (ModelState.IsValid)
            {
                // Use Stored Procedure via ClassService
                bool success = await _classService.UpdateClassAsync(@class, userRole ?? "Admin", userId ?? "");
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật lớp học thành công";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Không thể cập nhật lớp học. Kiểm tra quyền hoặc dữ liệu đầu vào.");
                }
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", @class.DepartmentId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", @class.TeacherId);
            return View(@class);
        }

        // GET: Classes/Delete/5
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(c => c.Department)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.ClassId == id);
          
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Use Stored Procedure via ClassService
            bool success = await _classService.DeleteClassAsync(id, userRole ?? "Admin");
            
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa lớp học thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa lớp học. Có thể do có sinh viên trong lớp hoặc không có quyền.";
            }
            
            return RedirectToAction(nameof(Index));
        }

     private bool ClassExists(string id)
        {
   return _context.Classes.Any(e => e.ClassId == id);
        }
    }
}
