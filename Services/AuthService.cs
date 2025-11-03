using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using System.Data;

namespace StudentManagementSystem.Services
{
public interface IAuthService
    {
        Task<(bool Success, string Role, string EntityId, string FullName)> AuthenticateAsync(string username, string password);
        Task<bool> ChangePasswordAsync(string username, string role, string currentPassword, string newPassword);
    }

    public class AuthService : IAuthService
 {
   private readonly ApplicationDbContext _context;

   public AuthService(ApplicationDbContext context)
        {
   _context = context;
    }

        public async Task<(bool Success, string Role, string EntityId, string FullName)> AuthenticateAsync(string username, string password)
        {
            try
            {
                // ✅ STEP 1: Check if user exists in Users table (for new registration system)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                
                if (user != null)
                {
                    // User found in new registration system
                    Console.WriteLine($"[AuthService] Found user in Users table: {username}");
                    
                    // Check if email is verified
                    if (!user.EmailVerified)
                    {
                        Console.WriteLine($"[AuthService] User email not verified: {username}");
                        return (false, "", "", "");
                    }

                    // Hash the input password with SHA256 (same as registration)
                    string hashedPassword = HashPassword(password);
                    Console.WriteLine($"[AuthService] Input password hash: {hashedPassword.Substring(0, 20)}...");
                    Console.WriteLine($"[AuthService] Stored password hash: {user.Password.Substring(0, 20)}...");

                    // Compare hashed passwords
                    if (user.Password == hashedPassword)
                    {
                        Console.WriteLine($"[AuthService] Password match! Role: {user.Role}");
                        
                        // Update LastLoginAt
                        user.LastLoginAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        // Return user info based on role - lookup real EntityId from respective tables
                        string entityId = user.Username;
                        string fullName = user.Username;
                        
                        if (user.Role == "Student")
                        {
                            var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == user.Username);
                            if (student != null)
                            {
                                entityId = student.StudentId;
                                fullName = student.FullName;
                                Console.WriteLine($"[AuthService] Found Student: {entityId} - {fullName}");
                            }
                        }
                        else if (user.Role == "Teacher")
                        {
                            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == user.Username);
                            if (teacher != null)
                            {
                                entityId = teacher.TeacherId;
                                fullName = teacher.FullName;
                                Console.WriteLine($"[AuthService] Found Teacher: {entityId} - {fullName}");
                            }
                        }
                        else if (user.Role == "Admin")
                        {
                            // Admin doesn't have entity in Students/Teachers table
                            entityId = user.Username;
                            fullName = user.Username; // Or could be from a separate Admins table
                        }
                        
                        return (true, user.Role, entityId, fullName);
                    }
                    else
                    {
                        Console.WriteLine($"[AuthService] Password mismatch!");
                        return (false, "", "", "");
                    }
                }

                // ✅ STEP 2: Fallback to stored procedure for old users (Students, Teachers, Admin in old system)
                Console.WriteLine($"[AuthService] User not in Users table, trying stored procedure...");
                
                var connection = _context.Database.GetDbConnection();
                await _context.Database.OpenConnectionAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "usp_AuthenticateUser";
                    command.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    var usernameParam = new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username };
                    var passwordParam = new SqlParameter("@Password", SqlDbType.NVarChar, 100) { Value = password };
                    
                    // Output parameters
                    var roleParam = new SqlParameter("@Role", SqlDbType.NVarChar, 20) { Direction = ParameterDirection.Output };
                    var entityIdParam = new SqlParameter("@EntityId", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                    var fullNameParam = new SqlParameter("@FullName", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                    
                    // Return value parameter
                    var returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };

                    command.Parameters.Add(returnValueParam);
                    command.Parameters.Add(usernameParam);
                    command.Parameters.Add(passwordParam);
                    command.Parameters.Add(roleParam);
                    command.Parameters.Add(entityIdParam);
                    command.Parameters.Add(fullNameParam);

                    await command.ExecuteNonQueryAsync();

                    await _context.Database.CloseConnectionAsync();

                    int returnValue = (int)returnValueParam.Value;
                    bool success = returnValue == 1;

                    if (success)
                    {
                        string role = roleParam.Value?.ToString() ?? "";
                        string entityId = entityIdParam.Value?.ToString() ?? "";
                        string fullName = fullNameParam.Value?.ToString() ?? "";
                        Console.WriteLine($"[AuthService] Stored procedure auth success! Role: {role}");
                        return (true, role, entityId, fullName);
                    }

                    Console.WriteLine($"[AuthService] Stored procedure auth failed");
                    return (false, "", "", "");
                }
            }
            catch (Exception ex)
            {
                // Log error for debugging
                Console.WriteLine($"[AuthService] Authentication error: {ex.Message}");
                Console.WriteLine($"[AuthService] Stack trace: {ex.StackTrace}");
                await _context.Database.CloseConnectionAsync();
                return (false, "", "", "");
            }
        }

        private string HashPassword(string password)
        {
            // SHA256 hash - same method as AccountController registration
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<bool> ChangePasswordAsync(string username, string role, string currentPassword, string newPassword)
        {
            try
            {
                // ✅ Using Stored Procedure: usp_ChangePassword
                var connection = _context.Database.GetDbConnection();
                await _context.Database.OpenConnectionAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "usp_ChangePassword";
                    command.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    var usernameParam = new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username };
                    var oldPasswordParam = new SqlParameter("@OldPassword", SqlDbType.NVarChar, 100) { Value = currentPassword };
                    var newPasswordParam = new SqlParameter("@NewPassword", SqlDbType.NVarChar, 100) { Value = newPassword };
                    
                    // Return value parameter
                    var returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };

                    command.Parameters.Add(returnValueParam);
                    command.Parameters.Add(usernameParam);
                    command.Parameters.Add(oldPasswordParam);
                    command.Parameters.Add(newPasswordParam);

                    await command.ExecuteNonQueryAsync();

                    await _context.Database.CloseConnectionAsync();

                    int returnValue = (int)returnValueParam.Value;
                    return returnValue == 1;
                }
            }
            catch (Exception ex)
            {
                // Log error for debugging
                Console.WriteLine($"Change password error: {ex.Message}");
                await _context.Database.CloseConnectionAsync();
                return false;
            }
        }
    }
}
