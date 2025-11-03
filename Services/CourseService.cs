using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Services
{
    public interface ICourseService
    {
        Task<(List<Course> Courses, int TotalCount)> GetCoursesAsync(
            string userRole, 
            string userId, 
            string? searchString, 
            string? departmentId, 
            int pageNumber, 
            int pageSize);
        Task<Course?> GetCourseByIdAsync(string courseId, string userRole, string userId);
        Task<bool> CreateCourseAsync(Course course, string userRole, string userId);
        Task<bool> UpdateCourseAsync(Course course, string userRole, string userId);
        Task<bool> DeleteCourseAsync(string courseId, string userRole);
    }

    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Course> Courses, int TotalCount)> GetCoursesAsync(
            string userRole, 
            string userId, 
            string? searchString, 
            string? departmentId, 
            int pageNumber, 
            int pageSize)
        {
            var courses = new List<Course>();
            int totalCount = 0;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "usp_GetCourses";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });
                command.Parameters.Add(new SqlParameter("@SearchString", SqlDbType.NVarChar, 100) { Value = (object?)searchString ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.NVarChar, 10) { Value = (object?)departmentId ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

                var totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(totalCountParam);

                await _context.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var course = new Course
                        {
                            CourseId = reader.GetString(reader.GetOrdinal("CourseId")),
                            CourseName = reader.GetString(reader.GetOrdinal("CourseName")),
                            Credits = reader.GetInt32(reader.GetOrdinal("Credits")),
                            DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                            TeacherId = reader.GetString(reader.GetOrdinal("TeacherId")),
                            Department = reader.IsDBNull(reader.GetOrdinal("DepartmentId")) ? null : new Department
                            {
                                DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName"))
                            },
                            Teacher = reader.IsDBNull(reader.GetOrdinal("TeacherId")) ? null : new Teacher
                            {
                                TeacherId = reader.GetString(reader.GetOrdinal("TeacherId")),
                                FullName = reader.GetString(reader.GetOrdinal("TeacherName"))
                            }
                        };
                        courses.Add(course);
                    }
                }

                totalCount = (int)(totalCountParam.Value ?? 0);
            }

            return (courses, totalCount);
        }

        public async Task<Course?> GetCourseByIdAsync(string courseId, string userRole, string userId)
        {
            Course? course = null;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "usp_GetCourseById";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.NVarChar, 10) { Value = courseId });
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });

                await _context.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        course = new Course
                        {
                            CourseId = reader.GetString(reader.GetOrdinal("CourseId")),
                            CourseName = reader.GetString(reader.GetOrdinal("CourseName")),
                            Credits = reader.GetInt32(reader.GetOrdinal("Credits")),
                            DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                            TeacherId = reader.GetString(reader.GetOrdinal("TeacherId")),
                            Department = reader.IsDBNull(reader.GetOrdinal("DepartmentId")) ? null : new Department
                            {
                                DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName"))
                            },
                            Teacher = reader.IsDBNull(reader.GetOrdinal("TeacherId")) ? null : new Teacher
                            {
                                TeacherId = reader.GetString(reader.GetOrdinal("TeacherId")),
                                FullName = reader.GetString(reader.GetOrdinal("TeacherName"))
                            }
                        };
                    }
                }
            }

            return course;
        }

        public async Task<bool> CreateCourseAsync(Course course, string userRole, string userId)
        {
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "usp_CreateCourse";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.NVarChar, 10) { Value = course.CourseId });
                    command.Parameters.Add(new SqlParameter("@CourseName", SqlDbType.NVarChar, 100) { Value = course.CourseName });
                    command.Parameters.Add(new SqlParameter("@Credits", SqlDbType.Int) { Value = course.Credits });
                    command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.NVarChar, 10) { Value = course.DepartmentId });
                    command.Parameters.Add(new SqlParameter("@TeacherId", SqlDbType.NVarChar, 10) { Value = course.TeacherId });
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

        public async Task<bool> UpdateCourseAsync(Course course, string userRole, string userId)
        {
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "usp_UpdateCourse";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.NVarChar, 10) { Value = course.CourseId });
                    command.Parameters.Add(new SqlParameter("@CourseName", SqlDbType.NVarChar, 100) { Value = course.CourseName });
                    command.Parameters.Add(new SqlParameter("@Credits", SqlDbType.Int) { Value = course.Credits });
                    command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.NVarChar, 10) { Value = course.DepartmentId });
                    command.Parameters.Add(new SqlParameter("@TeacherId", SqlDbType.NVarChar, 10) { Value = course.TeacherId });
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

        public async Task<bool> DeleteCourseAsync(string courseId, string userRole)
        {
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "usp_DeleteCourse";
                    command.CommandType = CommandType.StoredProcedure;

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
