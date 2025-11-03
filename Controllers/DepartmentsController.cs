using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Filters;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    [AuthorizeRole("Admin")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

public DepartmentsController(ApplicationDbContext context)
   {
          _context = context;
 }

      // GET: Departments
   public async Task<IActionResult> Index()
  {
   return View(await _context.Departments.ToListAsync());
   }

  // GET: Departments/Details/5
   public async Task<IActionResult> Details(string id)
  {
     if (id == null)
   {
return NotFound();
    }

var department = await _context.Departments
   .FirstOrDefaultAsync(m => m.DepartmentId == id);
  if (department == null)
            {
  return NotFound();
            }

        return View(department);
 }

        // GET: Departments/Create
        public IActionResult Create()
   {
return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentId,DepartmentCode,DepartmentName")] Department department)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(department);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm khoa thành công";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Mã khoa đã tồn tại hoặc có lỗi khi lưu dữ liệu");
                }
            }
            return View(department);
        }

        // GET: Departments/Edit/5
   public async Task<IActionResult> Edit(string id)
  {
 if (id == null)
       {
       return NotFound();
    }

   var department = await _context.Departments.FindAsync(id);
       if (department == null)
      {
     return NotFound();
    }
    return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("DepartmentId,DepartmentCode,DepartmentName")] Department department)
        {
            if (id != department.DepartmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật khoa thành công";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(department);
        }

   // GET: Departments/Delete/5
   public async Task<IActionResult> Delete(string id)
        {
  if (id == null)
      {
     return NotFound();
   }

       var department = await _context.Departments
      .FirstOrDefaultAsync(m => m.DepartmentId == id);
     if (department == null)
  {
  return NotFound();
       }

    return View(department);
   }

        // POST: Departments/Delete/5
  [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
      public async Task<IActionResult> DeleteConfirmed(string id)
        {
    try
  {
         var department = await _context.Departments.FindAsync(id);
        if (department != null)
         {
        _context.Departments.Remove(department);
 await _context.SaveChangesAsync();
   TempData["SuccessMessage"] = "Xóa khoa thành công";
   }
       return RedirectToAction(nameof(Index));
   }
     catch (DbUpdateException)
   {
  TempData["ErrorMessage"] = "Không thể xóa khoa này vì có dữ liệu liên quan";
    return RedirectToAction(nameof(Index));
     }
   }

   private bool DepartmentExists(string id)
        {
    return _context.Departments.Any(e => e.DepartmentId == id);
        }
    }
}
