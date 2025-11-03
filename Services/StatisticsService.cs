using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Services
{
    public interface IStatisticsService
{
    Task<int> GetTotalStudentsAsync();
   Task<int> GetTotalTeachersAsync();
        Task<int> GetTotalClassesAsync();
  Task<int> GetTotalCoursesAsync();
       Task<int> GetTotalDepartmentsAsync();
    Task<Dictionary<string, int>> GetStudentCountByClassAsync();
    Task<Dictionary<string, int>> GetStudentCountByDepartmentAsync();
  Task<double> GetAverageScoreByClassAsync(string classId);
 Task<double> GetAverageScoreByCourseAsync(string courseId);
        Task<double> GetAverageScoreByStudentAsync(string studentId);
    }

    public class StatisticsService : IStatisticsService
    {
 private readonly ApplicationDbContext _context;

   public StatisticsService(ApplicationDbContext context)
        {
   _context = context;
        }

        public async Task<int> GetTotalStudentsAsync()
    {
    return await _context.Students.CountAsync();
        }

  public async Task<int> GetTotalTeachersAsync()
        {
return await _context.Teachers.CountAsync();
        }

     public async Task<int> GetTotalClassesAsync()
     {
     return await _context.Classes.CountAsync();
        }

        public async Task<int> GetTotalCoursesAsync()
        {
  return await _context.Courses.CountAsync();
        }

  public async Task<int> GetTotalDepartmentsAsync()
  {
       return await _context.Departments.CountAsync();
        }

      public async Task<Dictionary<string, int>> GetStudentCountByClassAsync()
  {
            // ✅ Using Stored Procedure: usp_GetStudentCountByClass
            var result = new Dictionary<string, int>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC usp_GetStudentCountByClass @UserRole, @UserId";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Add(new SqlParameter("@UserRole", "Admin"));
                command.Parameters.Add(new SqlParameter("@UserId", DBNull.Value));

                await _context.Database.OpenConnectionAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string className = reader.GetString(reader.GetOrdinal("ClassName"));
                        int count = reader.GetInt32(reader.GetOrdinal("StudentCount"));
                        result[className] = count;
                    }
                }
                await _context.Database.CloseConnectionAsync();
            }

            return result;
        }

     public async Task<Dictionary<string, int>> GetStudentCountByDepartmentAsync()
        {
            // ✅ Using Stored Procedure: usp_GetStudentCountByDepartment
            var result = new Dictionary<string, int>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC usp_GetStudentCountByDepartment";
                command.CommandType = System.Data.CommandType.Text;

                await _context.Database.OpenConnectionAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string deptName = reader.GetString(reader.GetOrdinal("DepartmentName"));
                        int count = reader.GetInt32(reader.GetOrdinal("StudentCount"));
                        result[deptName] = count;
                    }
                }
                await _context.Database.CloseConnectionAsync();
            }

            return result;
     }

 public async Task<double> GetAverageScoreByClassAsync(string classId)
        {
            // ✅ Using Stored Procedure: usp_GetAverageScoreByClass
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC usp_GetAverageScoreByClass @ClassId";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Add(new SqlParameter("@ClassId", classId));

                await _context.Database.OpenConnectionAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int scoreOrdinal = reader.GetOrdinal("AverageScore");
                        if (!reader.IsDBNull(scoreOrdinal))
                        {
                            return Convert.ToDouble(reader.GetValue(scoreOrdinal));
                        }
                    }
                }
                await _context.Database.CloseConnectionAsync();
            }

            return 0;
        }

    public async Task<double> GetAverageScoreByCourseAsync(string courseId)
        {
            // ✅ Using Stored Procedure: usp_GetAverageScoreByCourse
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC usp_GetAverageScoreByCourse @CourseId";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Add(new SqlParameter("@CourseId", courseId));

                await _context.Database.OpenConnectionAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int scoreOrdinal = reader.GetOrdinal("AverageScore");
                        if (!reader.IsDBNull(scoreOrdinal))
                        {
                            return Convert.ToDouble(reader.GetValue(scoreOrdinal));
                        }
                    }
                }
                await _context.Database.CloseConnectionAsync();
            }

            return 0;
    }

  public async Task<double> GetAverageScoreByStudentAsync(string studentId)
        {
    var scores = await _context.Grades
     .Where(g => g.StudentId == studentId)
     .Select(g => g.Score)
       .ToListAsync();

return scores.Any() ? (double)scores.Average() : 0;
        }
    }
}
