using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Services
{
    public interface IClassService
    {
        Task<(List<Class> Classes, int TotalCount)> GetClassesAsync(
            string userRole,
            string userId,
            string? searchString = null,
            string? departmentId = null,
            int pageNumber = 1,
            int pageSize = 10);

        Task<Class?> GetClassByIdAsync(string classId, string userRole, string userId);
        Task<bool> CreateClassAsync(Class classEntity, string userRole, string userId);
        Task<bool> UpdateClassAsync(Class classEntity, string userRole, string userId);
        Task<bool> DeleteClassAsync(string classId, string userRole);
    }

    public class ClassService : IClassService
    {
        private readonly ApplicationDbContext _context;

        public ClassService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Class> Classes, int TotalCount)> GetClassesAsync(
            string userRole,
            string userId,
            string? searchString = null,
            string? departmentId = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_GetClasses";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });
                command.Parameters.Add(new SqlParameter("@SearchString", SqlDbType.NVarChar, 100) { Value = (object?)searchString ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.NVarChar, 10) { Value = (object?)departmentId ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

                var totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(totalCountParam);

                var classes = new List<Class>();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    classes.Add(new Class
                    {
                        ClassId = reader.GetString(reader.GetOrdinal("ClassId")),
                        ClassName = reader.GetString(reader.GetOrdinal("ClassName")),
                        DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                        TeacherId = reader.IsDBNull(reader.GetOrdinal("TeacherId")) ? null : reader.GetString(reader.GetOrdinal("TeacherId")),
                        Department = new Department
                        {
                            DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                            DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? "" : reader.GetString(reader.GetOrdinal("DepartmentName"))
                        },
                        Teacher = reader.IsDBNull(reader.GetOrdinal("TeacherId")) ? null : new Teacher
                        {
                            TeacherId = reader.GetString(reader.GetOrdinal("TeacherId")),
                            FullName = reader.IsDBNull(reader.GetOrdinal("TeacherName")) ? "" : reader.GetString(reader.GetOrdinal("TeacherName"))
                        }
                    });
                }

                int totalCount = totalCountParam.Value != DBNull.Value ? (int)totalCountParam.Value : 0;
                return (classes, totalCount);
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<Class?> GetClassByIdAsync(string classId, string userRole, string userId)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_GetClassById";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.NVarChar, 10) { Value = classId });
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Class
                    {
                        ClassId = reader.GetString(reader.GetOrdinal("ClassId")),
                        ClassName = reader.GetString(reader.GetOrdinal("ClassName")),
                        DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                        TeacherId = reader.IsDBNull(reader.GetOrdinal("TeacherId")) ? null : reader.GetString(reader.GetOrdinal("TeacherId")),
                        Department = new Department
                        {
                            DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                            DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? "" : reader.GetString(reader.GetOrdinal("DepartmentName"))
                        },
                        Teacher = reader.IsDBNull(reader.GetOrdinal("TeacherId")) ? null : new Teacher
                        {
                            TeacherId = reader.GetString(reader.GetOrdinal("TeacherId")),
                            FullName = reader.IsDBNull(reader.GetOrdinal("TeacherName")) ? "" : reader.GetString(reader.GetOrdinal("TeacherName"))
                        }
                    };
                }

                return null;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<bool> CreateClassAsync(Class classEntity, string userRole, string userId)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_CreateClass";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.NVarChar, 10) { Value = classEntity.ClassId });
                command.Parameters.Add(new SqlParameter("@ClassName", SqlDbType.NVarChar, 50) { Value = classEntity.ClassName });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.NVarChar, 10) { Value = classEntity.DepartmentId });
                command.Parameters.Add(new SqlParameter("@TeacherId", SqlDbType.NVarChar, 10) { Value = (object?)classEntity.TeacherId ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@AcademicYear", SqlDbType.NVarChar, 20) { Value = DBNull.Value }); // AcademicYear not in model
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });

                var returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValueParam);

                await command.ExecuteNonQueryAsync();
                return (int)returnValueParam.Value == 1;
            }
            catch
            {
                return false;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<bool> UpdateClassAsync(Class classEntity, string userRole, string userId)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_UpdateClass";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.NVarChar, 10) { Value = classEntity.ClassId });
                command.Parameters.Add(new SqlParameter("@ClassName", SqlDbType.NVarChar, 50) { Value = classEntity.ClassName });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.NVarChar, 10) { Value = classEntity.DepartmentId });
                command.Parameters.Add(new SqlParameter("@TeacherId", SqlDbType.NVarChar, 10) { Value = (object?)classEntity.TeacherId ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@AcademicYear", SqlDbType.NVarChar, 20) { Value = DBNull.Value }); // AcademicYear not in model
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });

                var returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValueParam);

                await command.ExecuteNonQueryAsync();
                return (int)returnValueParam.Value == 1;
            }
            catch
            {
                return false;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<bool> DeleteClassAsync(string classId, string userRole)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_DeleteClass";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.NVarChar, 10) { Value = classId });
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });

                var returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValueParam);

                await command.ExecuteNonQueryAsync();
                return (int)returnValueParam.Value == 1;
            }
            catch
            {
                return false;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }
}
