using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Services
{
    public interface IGradeService
    {
        Task<(List<Grade> Grades, int TotalCount)> GetGradesAsync(
            string userRole, 
            string userId, 
            string? studentId, 
            string? courseId, 
            string? classId, 
            int pageNumber, 
            int pageSize);
        Task<Grade?> GetGradeByIdAsync(string studentId, string courseId, string userRole, string userId);
        Task<bool> CreateGradeAsync(Grade grade, string userRole, string userId);
        Task<bool> UpdateGradeAsync(Grade grade, string userRole, string userId);
        Task<bool> DeleteGradeAsync(string studentId, string courseId, string userRole);
    }

    public class GradeService : IGradeService
    {
        private readonly ApplicationDbContext _context;

        public GradeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Grade> Grades, int TotalCount)> GetGradesAsync(
            string userRole, 
            string userId, 
            string? studentId, 
            string? courseId, 
            string? classId, 
            int pageNumber, 
            int pageSize)
        {
            var grades = new List<Grade>();
            int totalCount = 0;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "usp_GetGrades";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });
                command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.NVarChar, 10) { Value = (object?)studentId ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.NVarChar, 10) { Value = (object?)courseId ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.NVarChar, 10) { Value = (object?)classId ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

                var totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(totalCountParam);

                await _context.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var grade = new Grade
                        {
                            StudentId = reader.GetString(reader.GetOrdinal("StudentId")),
                            CourseId = reader.GetString(reader.GetOrdinal("CourseId")),
                            Score = reader.GetDecimal(reader.GetOrdinal("Score")),
                            Classification = reader.GetString(reader.GetOrdinal("Classification")),
                            Student = reader.IsDBNull(reader.GetOrdinal("StudentId")) ? null : new Student
                            {
                                StudentId = reader.GetString(reader.GetOrdinal("StudentId")),
                                FullName = reader.GetString(reader.GetOrdinal("StudentName"))
                            },
                            Course = reader.IsDBNull(reader.GetOrdinal("CourseId")) ? null : new Course
                            {
                                CourseId = reader.GetString(reader.GetOrdinal("CourseId")),
                                CourseName = reader.GetString(reader.GetOrdinal("CourseName"))
                            }
                        };
                        grades.Add(grade);
                    }
                }

                totalCount = (int)(totalCountParam.Value ?? 0);
            }

            return (grades, totalCount);
        }

        public async Task<Grade?> GetGradeByIdAsync(string studentId, string courseId, string userRole, string userId)
        {
            Grade? grade = null;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "usp_GetGradeById";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.NVarChar, 10) { Value = studentId });
                command.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.NVarChar, 10) { Value = courseId });
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });

                await _context.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        grade = new Grade
                        {
                            StudentId = reader.GetString(reader.GetOrdinal("StudentId")),
                            CourseId = reader.GetString(reader.GetOrdinal("CourseId")),
                            Score = reader.GetDecimal(reader.GetOrdinal("Score")),
                            Classification = reader.GetString(reader.GetOrdinal("Classification")),
                            Student = reader.IsDBNull(reader.GetOrdinal("StudentId")) ? null : new Student
                            {
                                StudentId = reader.GetString(reader.GetOrdinal("StudentId")),
                                FullName = reader.GetString(reader.GetOrdinal("StudentName"))
                            },
                            Course = reader.IsDBNull(reader.GetOrdinal("CourseId")) ? null : new Course
                            {
                                CourseId = reader.GetString(reader.GetOrdinal("CourseId")),
                                CourseName = reader.GetString(reader.GetOrdinal("CourseName"))
                            }
                        };
                    }
                }
            }

            return grade;
        }

        public async Task<bool> CreateGradeAsync(Grade grade, string userRole, string userId)
        {
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "usp_CreateGrade";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.NVarChar, 10) { Value = grade.StudentId });
                    command.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.NVarChar, 10) { Value = grade.CourseId });
                    command.Parameters.Add(new SqlParameter("@Score", SqlDbType.Decimal) { Value = grade.Score, Precision = 3, Scale = 2 });
                    command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                    command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });

                    var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                    command.Parameters.Add(returnValue);

                    await _context.Database.OpenConnectionAsync();
                    await command.ExecuteNonQueryAsync();

                    return (int)(returnValue.Value ?? -1) == 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateGradeAsync(Grade grade, string userRole, string userId)
        {
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "usp_UpdateGrade";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.NVarChar, 10) { Value = grade.StudentId });
                    command.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.NVarChar, 10) { Value = grade.CourseId });
                    command.Parameters.Add(new SqlParameter("@Score", SqlDbType.Decimal) { Value = grade.Score, Precision = 3, Scale = 2 });
                    command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                    command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });

                    var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                    command.Parameters.Add(returnValue);

                    await _context.Database.OpenConnectionAsync();
                    await command.ExecuteNonQueryAsync();

                    return (int)(returnValue.Value ?? -1) == 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteGradeAsync(string studentId, string courseId, string userRole)
        {
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "usp_DeleteGrade";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.NVarChar, 10) { Value = studentId });
                    command.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.NVarChar, 10) { Value = courseId });
                    command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });

                    var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                    command.Parameters.Add(returnValue);

                    await _context.Database.OpenConnectionAsync();
                    await command.ExecuteNonQueryAsync();

                    return (int)(returnValue.Value ?? -1) == 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
