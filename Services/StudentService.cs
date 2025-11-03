using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Services
{
    public interface IStudentService
    {
        Task<(List<Student> Students, int TotalCount)> GetStudentsAsync(
            string userRole, 
            string userId, 
            string? searchString = null, 
            string? classId = null, 
            string? departmentId = null,
            int pageNumber = 1, 
            int pageSize = 10);
        
        Task<Student?> GetStudentByIdAsync(string studentId, string userRole, string userId);
        Task<bool> CreateStudentAsync(Student student, string userRole, string userId);
        Task<bool> UpdateStudentAsync(Student student, string userRole, string userId);
        Task<bool> DeleteStudentAsync(string studentId, string userRole);
    }

    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Student> Students, int TotalCount)> GetStudentsAsync(
            string userRole, 
            string userId, 
            string? searchString = null, 
            string? classId = null, 
            string? departmentId = null,
            int pageNumber = 1, 
            int pageSize = 10)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_GetStudents";
                command.CommandType = CommandType.StoredProcedure;

                // Input parameters
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });
                command.Parameters.Add(new SqlParameter("@SearchString", SqlDbType.NVarChar, 100) { Value = (object?)searchString ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.NVarChar, 10) { Value = (object?)classId ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.NVarChar, 10) { Value = (object?)departmentId ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

                // Output parameter
                var totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(totalCountParam);

                // Execute and read results
                var students = new List<Student>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        students.Add(new Student
                        {
                            StudentId = reader.GetString(reader.GetOrdinal("StudentId")),
                            FullName = reader.GetString(reader.GetOrdinal("FullName")),
                            DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                            Gender = reader.GetBoolean(reader.GetOrdinal("Gender")),
                            Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone")),
                            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString(reader.GetOrdinal("Address")),
                            ClassId = reader.GetString(reader.GetOrdinal("ClassId")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            Password = "", // Don't expose password in list
                            Class = new Class
                            {
                                ClassId = reader.GetString(reader.GetOrdinal("ClassId")),
                                ClassName = reader.IsDBNull(reader.GetOrdinal("ClassName")) ? "" : reader.GetString(reader.GetOrdinal("ClassName")),
                                DepartmentId = reader.IsDBNull(reader.GetOrdinal("DepartmentId")) ? "" : reader.GetString(reader.GetOrdinal("DepartmentId")),
                                Department = new Department
                                {
                                    DepartmentId = reader.IsDBNull(reader.GetOrdinal("DepartmentId")) ? "" : reader.GetString(reader.GetOrdinal("DepartmentId")),
                                    DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? "" : reader.GetString(reader.GetOrdinal("DepartmentName"))
                                }
                            }
                        });
                    }
                }

                int totalCount = (int)totalCountParam.Value;
                return (students, totalCount);
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public async Task<Student?> GetStudentByIdAsync(string studentId, string userRole, string userId)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_GetStudentById";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.NVarChar, 10) { Value = studentId });
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 10) { Value = userId });

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Student
                    {
                        StudentId = reader.GetString(reader.GetOrdinal("StudentId")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                        Gender = reader.GetBoolean(reader.GetOrdinal("Gender")),
                        Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone")),
                        Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString(reader.GetOrdinal("Address")),
                        ClassId = reader.GetString(reader.GetOrdinal("ClassId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        Password = reader.GetString(reader.GetOrdinal("Password")),
                        Class = new Class
                        {
                            ClassId = reader.GetString(reader.GetOrdinal("ClassId")),
                            ClassName = reader.IsDBNull(reader.GetOrdinal("ClassName")) ? "" : reader.GetString(reader.GetOrdinal("ClassName"))
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

        public async Task<bool> CreateStudentAsync(Student student, string userRole, string userId)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_CreateStudent";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.NVarChar, 10) { Value = student.StudentId });
                command.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = student.FullName });
                command.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.Date) { Value = student.DateOfBirth });
                command.Parameters.Add(new SqlParameter("@Gender", SqlDbType.Bit) { Value = student.Gender });
                command.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 15) { Value = (object?)student.Phone ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar, 200) { Value = (object?)student.Address ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.NVarChar, 10) { Value = student.ClassId });
                command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = student.Username });
                command.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 100) { Value = student.Password });
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

        public async Task<bool> UpdateStudentAsync(Student student, string userRole, string userId)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_UpdateStudent";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.NVarChar, 10) { Value = student.StudentId });
                command.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Value = student.FullName });
                command.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.Date) { Value = student.DateOfBirth });
                command.Parameters.Add(new SqlParameter("@Gender", SqlDbType.Bit) { Value = student.Gender });
                command.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 15) { Value = (object?)student.Phone ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar, 200) { Value = (object?)student.Address ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.NVarChar, 10) { Value = student.ClassId });
                command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = student.Username });
                command.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 100) { Value = (object?)student.Password ?? DBNull.Value });
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

        public async Task<bool> DeleteStudentAsync(string studentId, string userRole)
        {
            var connection = _context.Database.GetDbConnection();
            await _context.Database.OpenConnectionAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "usp_DeleteStudent";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.NVarChar, 10) { Value = studentId });
                command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });

                var returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValueParam);

                await command.ExecuteNonQueryAsync();
                
                var result = (int)returnValueParam.Value;
                Console.WriteLine($"[StudentService] usp_DeleteStudent returned: {result} for StudentId: {studentId}");
                
                return result == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StudentService] Error calling usp_DeleteStudent: {ex.Message}");
                return false;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }
}
