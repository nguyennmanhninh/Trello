using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Services;
using StudentManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ApplicationDbContext _context;

        public StatisticsController(IStatisticsService statisticsService, ApplicationDbContext context)
        {
            _statisticsService = statisticsService;
            _context = context;
        }

        // GET: api/statistics/overview
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            try
            {
                var overview = new
                {
                    TotalStudents = await _statisticsService.GetTotalStudentsAsync(),
                    TotalTeachers = await _statisticsService.GetTotalTeachersAsync(),
                    TotalClasses = await _statisticsService.GetTotalClassesAsync(),
                    TotalCourses = await _statisticsService.GetTotalCoursesAsync(),
                    TotalDepartments = await _statisticsService.GetTotalDepartmentsAsync()
                };

                return Ok(overview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving statistics", error = ex.Message });
            }
        }

        // GET: api/statistics/students-by-class
        [HttpGet("students-by-class")]
        public async Task<IActionResult> GetStudentsByClass()
        {
            try
            {
                var data = await _statisticsService.GetStudentCountByClassAsync();
                var result = data.Select(kvp => new
                {
                    ClassName = kvp.Key,
                    StudentCount = kvp.Value
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving students by class", error = ex.Message });
            }
        }

        // GET: api/statistics/students-by-department
        [HttpGet("students-by-department")]
        public async Task<IActionResult> GetStudentsByDepartment()
        {
            try
            {
                var data = await _statisticsService.GetStudentCountByDepartmentAsync();
                var result = data.Select(kvp => new
                {
                    DepartmentName = kvp.Key,
                    StudentCount = kvp.Value
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving students by department", error = ex.Message });
            }
        }

        // GET: api/statistics/average-by-class/{classId}
        [HttpGet("average-by-class/{classId}")]
        public async Task<IActionResult> GetAverageByClass(string classId)
        {
            try
            {
                var average = await _statisticsService.GetAverageScoreByClassAsync(classId);
                return Ok(new { ClassId = classId, AverageScore = Math.Round(average, 2) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving class average", error = ex.Message });
            }
        }

        // GET: api/statistics/average-by-course/{courseId}
        [HttpGet("average-by-course/{courseId}")]
        public async Task<IActionResult> GetAverageByCourse(string courseId)
        {
            try
            {
                var average = await _statisticsService.GetAverageScoreByCourseAsync(courseId);
                return Ok(new { CourseId = courseId, AverageScore = Math.Round(average, 2) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving course average", error = ex.Message });
            }
        }

        // GET: api/statistics/grade-distribution
        [HttpGet("grade-distribution")]
        public async Task<IActionResult> GetGradeDistribution()
        {
            try
            {
                var grades = await _context.Grades
                    .Select(g => g.Classification)
                    .ToListAsync();

                var distribution = grades
                    .GroupBy(c => c)
                    .Select(g => new
                    {
                        Classification = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                return Ok(distribution);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving grade distribution", error = ex.Message });
            }
        }

        // GET: api/statistics/top-students
        [HttpGet("top-students")]
        public async Task<IActionResult> GetTopStudents(int count = 10)
        {
            try
            {
                var studentAverages = await _context.Students
                    .Select(s => new
                    {
                        s.StudentId,
                        s.FullName,
                        AverageScore = _context.Grades
                            .Where(g => g.StudentId == s.StudentId)
                            .Average(g => (double?)g.Score) ?? 0
                    })
                    .Where(s => s.AverageScore > 0)
                    .OrderByDescending(s => s.AverageScore)
                    .Take(count)
                    .ToListAsync();

                var result = studentAverages.Select(s => new
                {
                    s.StudentId,
                    s.FullName,
                    AverageScore = Math.Round(s.AverageScore, 2)
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving top students", error = ex.Message });
            }
        }

        // GET: api/statistics/class-performance
        [HttpGet("class-performance")]
        public async Task<IActionResult> GetClassPerformance()
        {
            try
            {
                var classPerformance = await _context.Classes
                    .Select(c => new
                    {
                        c.ClassId,
                        c.ClassName,
                        StudentCount = _context.Students.Count(s => s.ClassId == c.ClassId),
                        AverageScore = _context.Grades
                            .Where(g => _context.Students.Any(s => s.StudentId == g.StudentId && s.ClassId == c.ClassId))
                            .Average(g => (double?)g.Score) ?? 0
                    })
                    .Where(c => c.StudentCount > 0)
                    .OrderByDescending(c => c.AverageScore)
                    .ToListAsync();

                var result = classPerformance.Select(c => new
                {
                    c.ClassId,
                    c.ClassName,
                    c.StudentCount,
                    AverageScore = Math.Round(c.AverageScore, 2)
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving class performance", error = ex.Message });
            }
        }
    }
}
