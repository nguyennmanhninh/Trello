using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Services
{
    public interface ITeacherService
    {
        Task<(List<Teacher> Teachers, int TotalCount)> GetTeachersAsync(
            string userRole,
            string userId,
            string? searchString = null,
            string? departmentId = null,
            int pageNumber = 1,
            int pageSize = 10);

        Task<Teacher?> GetTeacherByIdAsync(string teacherId, string userRole, string userId);
        Task<bool> CreateTeacherAsync(Teacher teacher, string userRole, string userId);
        Task<bool> UpdateTeacherAsync(Teacher teacher, string userRole, string userId);
        Task<bool> DeleteTeacherAsync(string teacherId, string userRole);
    }

    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;

        public TeacherService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Teacher> Teachers, int TotalCount)> GetTeachersAsync(
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
                command.CommandText = "usp_GetTeachers";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });
                command.Parameters.Add(new SqlParameter("@SearchString", SqlDbType.NVarChar, 100) { Value = (object?)searchString ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.NVarChar, 10) { Value = (object?)departmentId ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

                var totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(totalCountParam);

                var teachers = new List<Teacher>();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    teachers.Add(new Teacher
                    {
                        TeacherId = reader.GetString(reader.GetOrdinal("TeacherId")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                        Gender = reader.GetBoolean(reader.GetOrdinal("Gender")),
                        Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone")),
                        Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString(reader.GetOrdinal("Address")),
                        DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        Password = "", // Don't expose password in list
                        Department = new Department
                        {
                            DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                            DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? "" : reader.GetString(reader.GetOrdinal("DepartmentName"))
                        }
                    });
                }

                int totalCount = totalCountParam.Value != DBNull.Value ? (int)totalCountParam.Value : 0;
                return (teachers, totalCount);
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<Teacher?> GetTeacherByIdAsync(string teacherId, string userRole, string userId)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_GetTeacherById";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@TeacherId", SqlDbType.NVarChar, 10) { Value = teacherId });
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Teacher
                    {
                        TeacherId = reader.GetString(reader.GetOrdinal("TeacherId")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                        Gender = reader.GetBoolean(reader.GetOrdinal("Gender")),
                        Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone")),
                        Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString(reader.GetOrdinal("Address")),
                        DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        Password = reader.GetString(reader.GetOrdinal("Password")),
                        Department = new Department
                        {
                            DepartmentId = reader.GetString(reader.GetOrdinal("DepartmentId")),
                            DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? "" : reader.GetString(reader.GetOrdinal("DepartmentName"))
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

        public async Task<bool> CreateTeacherAsync(Teacher teacher, string userRole, string userId)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_CreateTeacher";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@TeacherId", SqlDbType.NVarChar, 10) { Value = teacher.TeacherId });
                command.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = teacher.FullName });
                command.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.Date) { Value = teacher.DateOfBirth });
                command.Parameters.Add(new SqlParameter("@Gender", SqlDbType.Bit) { Value = teacher.Gender });
                command.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 15) { Value = (object?)teacher.Phone ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar, 200) { Value = (object?)teacher.Address ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.NVarChar, 10) { Value = teacher.DepartmentId });
                command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = teacher.Username });
                command.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 100) { Value = teacher.Password });
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

        public async Task<bool> UpdateTeacherAsync(Teacher teacher, string userRole, string userId)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_UpdateTeacher";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@TeacherId", SqlDbType.NVarChar, 10) { Value = teacher.TeacherId });
                command.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = teacher.FullName });
                command.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.Date) { Value = teacher.DateOfBirth });
                command.Parameters.Add(new SqlParameter("@Gender", SqlDbType.Bit) { Value = teacher.Gender });
                command.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 15) { Value = (object?)teacher.Phone ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar, 200) { Value = (object?)teacher.Address ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.NVarChar, 10) { Value = teacher.DepartmentId });
                command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = teacher.Username });
                command.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 100) { Value = (object?)teacher.Password ?? DBNull.Value });
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

        public async Task<bool> DeleteTeacherAsync(string teacherId, string userRole)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_DeleteTeacher";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@TeacherId", SqlDbType.NVarChar, 10) { Value = teacherId });
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });

                var returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValueParam);

                await command.ExecuteNonQueryAsync();
                
                var result = (int)returnValueParam.Value;
                Console.WriteLine($"[TeacherService] usp_DeleteTeacher returned: {result} for TeacherId: {teacherId}");
                
                return result == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TeacherService] Error calling usp_DeleteTeacher: {ex.Message}");
                return false;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }
}
