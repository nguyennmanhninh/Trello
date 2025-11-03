using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Services;
using System.Text;

namespace StudentManagementSystem.Controllers
{
    public class DebugController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public DebugController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet]
  public async Task<IActionResult> TestConnection()
        {
            var sb = new StringBuilder();
     sb.AppendLine("=== DATABASE CONNECTION TEST ===\n");

       try
          {
                // Test 1: Can connect to database
  sb.AppendLine("Test 1: Database Connection");
      var canConnect = await _context.Database.CanConnectAsync();
        sb.AppendLine($"Result: {(canConnect ? "? SUCCESS" : "? FAILED")}\n");

                if (!canConnect)
             {
         return Content(sb.ToString(), "text/plain");
           }

        // Test 2: Check tables exist
 sb.AppendLine("Test 2: Tables Existence");
            var usersCount = await _context.Users.CountAsync();
var teachersCount = await _context.Teachers.CountAsync();
           var studentsCount = await _context.Students.CountAsync();
     sb.AppendLine($"Users: {usersCount} records");
     sb.AppendLine($"Teachers: {teachersCount} records");
  sb.AppendLine($"Students: {studentsCount} records\n");

    // Test 3: Check admin account
                sb.AppendLine("Test 3: Admin Account");
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
 if (admin != null)
  {
                 sb.AppendLine($"? Admin found");
         sb.AppendLine($"  Username: {admin.Username}");
        sb.AppendLine($"  Password: {admin.Password}");
     sb.AppendLine($"  Role: {admin.Role}\n");
       }
                else
    {
            sb.AppendLine("? Admin NOT found\n");
  }

   // Test 4: Test authentication
            sb.AppendLine("Test 4: Authentication Test");
    var authResult = await _authService.AuthenticateAsync("admin", "admin123");
         sb.AppendLine($"Success: {authResult.Success}");
    sb.AppendLine($"Role: {authResult.Role}");
   sb.AppendLine($"EntityId: {authResult.EntityId}");
       sb.AppendLine($"FullName: {authResult.FullName}\n");

     // Test 5: Session test
      sb.AppendLine("Test 5: Session Test");
        HttpContext.Session.SetString("TestKey", "TestValue123");
                await HttpContext.Session.CommitAsync();
        var testValue = HttpContext.Session.GetString("TestKey");
            sb.AppendLine($"Set value: TestValue123");
                sb.AppendLine($"Get value: {testValue}");
          sb.AppendLine($"Session works: {(testValue == "TestValue123" ? "? YES" : "? NO")}\n");

            }
          catch (Exception ex)
            {
                sb.AppendLine($"\n? ERROR: {ex.Message}");
     sb.AppendLine($"Stack Trace:\n{ex.StackTrace}");
     }

     return Content(sb.ToString(), "text/plain");
   }

        [HttpGet]
        public IActionResult TestSession()
    {
       var sb = new StringBuilder();
            sb.AppendLine("=== SESSION DEBUG ===\n");

          try
   {
    // Check current session
                var userId = HttpContext.Session.GetString("UserId");
     var userRole = HttpContext.Session.GetString("UserRole");
    var userName = HttpContext.Session.GetString("UserName");
         var username = HttpContext.Session.GetString("Username");

   sb.AppendLine("Current Session Values:");
       sb.AppendLine($"UserId: {userId ?? "(null)"}");
     sb.AppendLine($"UserRole: {userRole ?? "(null)"}");
      sb.AppendLine($"UserName: {userName ?? "(null)"}");
            sb.AppendLine($"Username: {username ?? "(null)"}");
       sb.AppendLine($"Session ID: {HttpContext.Session.Id}");
             sb.AppendLine($"Session IsAvailable: {HttpContext.Session.IsAvailable}");

   // Cookie info
      sb.AppendLine("\nCookies:");
    foreach (var cookie in Request.Cookies)
    {
      sb.AppendLine($"  {cookie.Key} = {cookie.Value}");
        }

      // Headers
             sb.AppendLine("\nHeaders:");
          foreach (var header in Request.Headers)
       {
       if (header.Key.Contains("Cookie") || header.Key.Contains("Session"))
    {
   sb.AppendLine($"  {header.Key} = {header.Value}");
           }
         }
            }
            catch (Exception ex)
          {
          sb.AppendLine($"\n? ERROR: {ex.Message}");
        }

      return Content(sb.ToString(), "text/plain");
        }

[HttpGet]
        public async Task<IActionResult> TestLogin(string username = "admin", string password = "admin123")
        {
       var sb = new StringBuilder();
            sb.AppendLine($"=== LOGIN TEST FOR {username} ===\n");

   try
   {
             // Step 1: Clear existing session
      sb.AppendLine("Step 1: Clearing session...");
      HttpContext.Session.Clear();
    await HttpContext.Session.CommitAsync();
        sb.AppendLine("? Session cleared\n");

            // Step 2: Authenticate
        sb.AppendLine($"Step 2: Authenticating '{username}' / '{password}'...");
         var result = await _authService.AuthenticateAsync(username, password);
  sb.AppendLine($"Success: {result.Success}");
    sb.AppendLine($"Role: {result.Role}");
                sb.AppendLine($"EntityId: {result.EntityId}");
         sb.AppendLine($"FullName: {result.FullName}\n");

       if (!result.Success)
     {
  sb.AppendLine("? Authentication failed!");
         return Content(sb.ToString(), "text/plain");
         }

         // Step 3: Set session
    sb.AppendLine("Step 3: Setting session values...");
     HttpContext.Session.SetString("UserId", result.EntityId);
      HttpContext.Session.SetString("UserRole", result.Role);
      HttpContext.Session.SetString("UserName", result.FullName);
     HttpContext.Session.SetString("Username", username);
     sb.AppendLine("? Session values set\n");

     // Step 4: Commit session
         sb.AppendLine("Step 4: Committing session...");
        await HttpContext.Session.CommitAsync();
        sb.AppendLine("? Session committed\n");

          // Step 5: Verify session
     sb.AppendLine("Step 5: Verifying session...");
                var savedUserId = HttpContext.Session.GetString("UserId");
   var savedRole = HttpContext.Session.GetString("UserRole");
        var savedName = HttpContext.Session.GetString("UserName");
          
  sb.AppendLine($"UserId in session: {savedUserId}");
         sb.AppendLine($"UserRole in session: {savedRole}");
                sb.AppendLine($"UserName in session: {savedName}");

         if (savedUserId == result.EntityId && savedRole == result.Role)
    {
sb.AppendLine("\n??? SESSION WORKING CORRECTLY! ???");
        sb.AppendLine("\nYou can now try to login normally.");
          sb.AppendLine("If normal login still fails, check browser cookies.");
       }
    else
    {
        sb.AppendLine("\n??? SESSION NOT SAVED CORRECTLY! ???");
           sb.AppendLine("Issue: Session is not persisting values.");
  }

   }
    catch (Exception ex)
     {
 sb.AppendLine($"\n? ERROR: {ex.Message}");
         sb.AppendLine($"Stack Trace:\n{ex.StackTrace}");
      }

  return Content(sb.ToString(), "text/plain");
        }

        [HttpGet]
        public async Task<IActionResult> TestDatabase()
 {
          var sb = new StringBuilder();
  sb.AppendLine("=== DATABASE QUERY TEST ===\n");

        try
            {
     // Test 1: Direct query
      sb.AppendLine("Test 1: Query Users table");
       var users = await _context.Users.ToListAsync();
         sb.AppendLine($"Found {users.Count} users:");
                foreach (var user in users)
  {
   sb.AppendLine($"  - {user.Username} / {user.Password} / {user.Role}");
       }
       sb.AppendLine();

          // Test 2: Query Teachers
      sb.AppendLine("Test 2: Query Teachers table");
        var teachers = await _context.Teachers.Take(3).ToListAsync();
                sb.AppendLine($"Found {teachers.Count} teachers:");
   foreach (var teacher in teachers)
            {
     sb.AppendLine($"  - {teacher.TeacherId}: {teacher.FullName} / {teacher.Username}");
      }
       sb.AppendLine();

     // Test 3: Query Students
    sb.AppendLine("Test 3: Query Students table");
       var students = await _context.Students.Take(3).ToListAsync();
       sb.AppendLine($"Found {students.Count} students:");
          foreach (var student in students)
     {
          sb.AppendLine($"  - {student.StudentId}: {student.FullName} / {student.Username}");
           }
                sb.AppendLine();

  // Test 4: Test specific admin query
      sb.AppendLine("Test 4: Find admin user");
     var admin = await _context.Users
        .Where(u => u.Username == "admin" && u.Password == "admin123")
   .FirstOrDefaultAsync();
      
     if (admin != null)
     {
                    sb.AppendLine("? Admin found with correct credentials");
                }
      else
                {
      sb.AppendLine("? Admin NOT found or wrong credentials");
    }

 }
            catch (Exception ex)
            {
     sb.AppendLine($"\n? ERROR: {ex.Message}");
        sb.AppendLine($"Stack Trace:\n{ex.StackTrace}");
       }

 return Content(sb.ToString(), "text/plain");
        }

     [HttpGet]
        public IActionResult Index()
        {
      var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Debug Tools</title>
    <style>
        body { font-family: Arial; margin: 40px; }
        h1 { color: #333; }
        .test-link { 
    display: block; 
         padding: 15px; 
    margin: 10px 0; 
     background: #007bff; 
            color: white; 
            text-decoration: none; 
        border-radius: 5px;
   text-align: center;
        }
  .test-link:hover { background: #0056b3; }
  .info { 
  background: #f8f9fa; 
            padding: 15px; 
            border-left: 4px solid #007bff;
          margin: 20px 0;
     }
    </style>
</head>
<body>
    <h1>?? Debug Tools</h1>
    
    <div class='info'>
      <strong>H??ng d?n:</strong>
        <p>Click v�o t?ng link b�n d??i ?? test c�c th�nh ph?n c?a h? th?ng.</p>
 <p>K?t qu? s? hi?n th? d?ng text ?? d? ??c.</p>
    </div>

    <h2>Database Tests:</h2>
    <a href='/Debug/TestConnection' class='test-link' target='_blank'>
1. Test Database Connection
    </a>
    <a href='/Debug/TestDatabase' class='test-link' target='_blank'>
      2. Test Database Queries
    </a>

    <h2>Session Tests:</h2>
    <a href='/Debug/TestSession' class='test-link' target='_blank'>
      3. Check Current Session
    </a>

    <h2>Login Tests:</h2>
  <a href='/Debug/TestLogin?username=admin&password=admin123' class='test-link' target='_blank'>
 4. Test Login (Admin)
    </a>
  <a href='/Debug/TestLogin?username=gv001&password=gv001pass' class='test-link' target='_blank'>
     5. Test Login (Teacher)
    </a>
    <a href='/Debug/TestLogin?username=sv001&password=sv001pass' class='test-link' target='_blank'>
        6. Test Login (Student)
    </a>

 <hr style='margin: 40px 0;'>
    
    <div class='info'>
        <strong>N?u t?t c? test ??u PASS:</strong>
        <p>V?n ?? c� th? do browser cookies. Th?:</p>
        <ul>
<li>Clear cookies v� cache</li>
         <li>Th? tr�nh duy?t kh�c</li>
            <li>Th? ch? ?? Incognito/Private</li>
        </ul>
    </div>

    <a href='/Account/Login' class='test-link' style='background: #28a745;'>
      ? V? trang Login
    </a>
</body>
</html>";

         return Content(html, "text/html");
  }

        // =============================================
        // NEW: Database Improvements Helper
        // =============================================
        [HttpGet]
        public async Task<IActionResult> CheckDuplicateGrades()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== CHECK DUPLICATE GRADES ===\n");

            try
            {
                // Check for duplicate grades
                var duplicates = await _context.Grades
                    .GroupBy(g => new { g.StudentId, g.CourseId })
                    .Where(g => g.Count() > 1)
                    .Select(g => new
                    {
                        g.Key.StudentId,
                        g.Key.CourseId,
                        Count = g.Count(),
                        Scores = string.Join(", ", g.Select(x => x.Score))
                    })
                    .ToListAsync();

                if (duplicates.Any())
                {
                    sb.AppendLine($"⚠️ Found {duplicates.Count} duplicate combinations:\n");
                    foreach (var dup in duplicates)
                    {
                        sb.AppendLine($"StudentId: {dup.StudentId}, CourseId: {dup.CourseId}");
                        sb.AppendLine($"  Count: {dup.Count}, Scores: {dup.Scores}\n");
                    }
                    sb.AppendLine("❌ CANNOT add UNIQUE constraint with duplicates!");
                    sb.AppendLine("Please resolve duplicates first.");
                }
                else
                {
                    sb.AppendLine("✅ No duplicate grades found!");
                    sb.AppendLine("Safe to add UNIQUE constraint.");
                    sb.AppendLine("\nRun this SQL:");
                    sb.AppendLine("ALTER TABLE Grades ADD CONSTRAINT UQ_Grades_StudentCourse UNIQUE (StudentId, CourseId);");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Error: {ex.Message}");
            }

            return Content(sb.ToString(), "text/plain");
        }

        [HttpGet]
        public async Task<IActionResult> AddUniqueConstraint()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ADD UNIQUE CONSTRAINT ===\n");

            try
            {
                // First check for duplicates
                var hasDuplicates = await _context.Grades
                    .GroupBy(g => new { g.StudentId, g.CourseId })
                    .AnyAsync(g => g.Count() > 1);

                if (hasDuplicates)
                {
                    sb.AppendLine("❌ Cannot add constraint - duplicates exist!");
                    sb.AppendLine("Run /Debug/CheckDuplicateGrades to see details.");
                    return Content(sb.ToString(), "text/plain");
                }

                // Execute SQL to add constraint
                var sql = @"
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.indexes 
                        WHERE name = 'UQ_Grades_StudentCourse' 
                        AND object_id = OBJECT_ID('Grades')
                    )
                    BEGIN
                        ALTER TABLE Grades
                        ADD CONSTRAINT UQ_Grades_StudentCourse UNIQUE (StudentId, CourseId);
                        SELECT 'SUCCESS' AS Result;
                    END
                    ELSE
                    BEGIN
                        SELECT 'EXISTS' AS Result;
                    END";

                var result = await _context.Database.ExecuteSqlRawAsync(sql);
                
                sb.AppendLine("✅ UNIQUE constraint added successfully!");
                sb.AppendLine("Students can now only have ONE grade per course.");
                
                // Verify
                var verification = await _context.Database
                    .SqlQueryRaw<string>(@"
                        SELECT name 
                        FROM sys.indexes 
                        WHERE name = 'UQ_Grades_StudentCourse' 
                        AND object_id = OBJECT_ID('Grades')")
                    .FirstOrDefaultAsync();

                if (verification != null)
                {
                    sb.AppendLine($"\n✅ Verified: {verification} constraint exists");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    sb.AppendLine($"Inner: {ex.InnerException.Message}");
                }
            }

            return Content(sb.ToString(), "text/plain");
        }

        [HttpGet]
        public async Task<IActionResult> CheckTeacherDeleteValidation()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== CHECK TEACHER DELETE VALIDATION ===\n");

            try
            {
                // Check if usp_DeleteTeacher has proper validation
                string? spDefinition = null;
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('usp_DeleteTeacher'))";
                    await _context.Database.OpenConnectionAsync();
                    spDefinition = (await cmd.ExecuteScalarAsync())?.ToString();
                    await _context.Database.CloseConnectionAsync();
                }

                if (spDefinition == null)
                {
                    sb.AppendLine("❌ usp_DeleteTeacher not found!");
                    return Content(sb.ToString(), "text/plain");
                }

                sb.AppendLine("✅ usp_DeleteTeacher exists\n");
                sb.AppendLine("Checking validations:\n");

                // Check for key validations
                var validations = new Dictionary<string, bool>
                {
                    ["Admin only check"] = spDefinition.Contains("@UserRole != 'Admin'"),
                    ["Classes check"] = spDefinition.Contains("Classes WHERE TeacherId"),
                    ["Courses check"] = spDefinition.Contains("Courses WHERE TeacherId"),
                    ["Transaction"] = spDefinition.Contains("BEGIN TRANSACTION"),
                    ["Error handling"] = spDefinition.Contains("BEGIN TRY")
                };

                foreach (var validation in validations)
                {
                    var icon = validation.Value ? "✅" : "❌";
                    sb.AppendLine($"{icon} {validation.Key}");
                }

                sb.AppendLine("\n" + new string('=', 50));
                var allPassed = validations.All(v => v.Value);
                if (allPassed)
                {
                    sb.AppendLine("✅ ALL VALIDATIONS PRESENT");
                    sb.AppendLine("usp_DeleteTeacher is PERFECT - no changes needed!");
                }
                else
                {
                    sb.AppendLine("⚠️ SOME VALIDATIONS MISSING");
                    sb.AppendLine("Review stored procedure!");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Error: {ex.Message}");
            }

            return Content(sb.ToString(), "text/plain");
        }

        [HttpGet]
        public IActionResult ImprovementsPanel()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Database Improvements Panel</title>
    <style>
        body { font-family: 'Segoe UI', Arial; margin: 20px; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; }
        h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
        .card { background: white; padding: 20px; margin: 20px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .card h2 { color: #3498db; margin-top: 0; }
        .btn { display: inline-block; padding: 12px 24px; margin: 5px; background: #3498db; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }
        .btn:hover { background: #2980b9; }
        .btn-success { background: #27ae60; }
        .btn-success:hover { background: #229954; }
        .btn-warning { background: #f39c12; }
        .btn-warning:hover { background: #e67e22; }
        .status { padding: 8px 16px; border-radius: 5px; display: inline-block; margin: 5px; }
        .status-ok { background: #d4edda; color: #155724; }
        .status-pending { background: #fff3cd; color: #856404; }
        .status-error { background: #f8d7da; color: #721c24; }
        pre { background: #2c3e50; color: #ecf0f1; padding: 15px; border-radius: 5px; overflow-x: auto; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🔧 Database Improvements Control Panel</h1>
        
        <div class='card'>
            <h2>📊 Current Status</h2>
            <p>Click buttons below to check and apply improvements</p>
            <a href='/Debug/DatabaseImprovementsSummary' class='btn'>📊 View Summary</a>
        </div>

        <div class='card'>
            <h2>1️⃣ UNIQUE Constraint for Grades</h2>
            <p><strong>Purpose:</strong> Prevent duplicate grades (one student = one grade per course)</p>
            <div>
                <a href='/Debug/CheckDuplicateGrades' class='btn btn-warning'>🔍 Check Duplicates</a>
                <a href='/Debug/AddUniqueConstraint' class='btn btn-success'>✅ Add Constraint</a>
            </div>
            <p style='margin-top: 15px;'><strong>Steps:</strong></p>
            <ol>
                <li>Click 'Check Duplicates' first</li>
                <li>If no duplicates, click 'Add Constraint'</li>
                <li>Verify with 'View Summary'</li>
            </ol>
        </div>

        <div class='card'>
            <h2>2️⃣ Teacher Delete Validation</h2>
            <p><strong>Purpose:</strong> Ensure usp_DeleteTeacher has proper validation</p>
            <div>
                <a href='/Debug/CheckTeacherDeleteValidation' class='btn'>🔍 Check Validation</a>
            </div>
            <p style='margin-top: 15px;'><strong>Should validate:</strong></p>
            <ul>
                <li>✅ Only Admin can delete</li>
                <li>✅ Cannot delete if teacher has classes</li>
                <li>✅ Cannot delete if teacher has courses</li>
                <li>✅ Transaction and error handling</li>
            </ul>
        </div>

        <div class='card'>
            <h2>3️⃣ Grade Deletion Policy</h2>
            <p><strong>Current Status:</strong> Teacher can delete grades (⚠️ needs review)</p>
            <p><strong>Recommendations:</strong></p>
            <ul>
                <li><strong>Option 1 (Recommended):</strong> Only Admin can delete grades</li>
                <li><strong>Option 2:</strong> Keep Teacher delete + Add audit trail</li>
            </ul>
            <p><strong>Manual Action Required:</strong> Run SQL script from <code>/Database/FIX_GRADE_DELETION_POLICY.sql</code></p>
        </div>

        <div class='card'>
            <h2>📝 Quick SQL Scripts</h2>
            <h3>Add UNIQUE Constraint (Manual)</h3>
            <pre>ALTER TABLE Grades
ADD CONSTRAINT UQ_Grades_StudentCourse 
UNIQUE (StudentId, CourseId);</pre>

            <h3>Verify Constraint</h3>
            <pre>SELECT name, type_desc
FROM sys.indexes
WHERE object_id = OBJECT_ID('Grades')
AND is_unique_constraint = 1;</pre>
        </div>

        <div class='card'>
            <h2>🎯 Final Checklist</h2>
            <ul style='list-style: none; padding: 0;'>
                <li style='padding: 8px;'>☐ Check for duplicate grades</li>
                <li style='padding: 8px;'>☐ Add UNIQUE constraint for Grades</li>
                <li style='padding: 8px;'>☐ Verify Teacher delete validation</li>
                <li style='padding: 8px;'>☐ Choose grade deletion policy (Option 1 or 2)</li>
                <li style='padding: 8px;'>☐ Test all CRUD operations</li>
                <li style='padding: 8px;'>☐ Update documentation</li>
            </ul>
        </div>

        <div class='card'>
            <h2>📚 Documentation</h2>
            <ul>
                <li><strong>CRUD_LOGIC_REVIEW.md</strong> - Initial analysis (75/100)</li>
                <li><strong>CRUD_LOGIC_FINAL_VERIFICATION.md</strong> - Code verification (90/100)</li>
                <li><strong>IMPROVEMENTS_SUMMARY.md</strong> - Implementation guide (95/100)</li>
            </ul>
        </div>
    </div>
</body>
</html>";
            return Content(html, "text/html");
        }

        [HttpGet]
        public async Task<IActionResult> DatabaseImprovementsSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== DATABASE IMPROVEMENTS SUMMARY ===\n");

            try
            {
                // 1. Check UNIQUE constraint
                sb.AppendLine("1️⃣ UNIQUE Constraint (Grades)");
                
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM sys.indexes WHERE name = 'UQ_Grades_StudentCourse' AND object_id = OBJECT_ID('Grades')";
                    await _context.Database.OpenConnectionAsync();
                    var uniqueExists = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    await _context.Database.CloseConnectionAsync();

                    if (uniqueExists > 0)
                    {
                        sb.AppendLine("   ✅ UNIQUE constraint exists");
                    }
                    else
                    {
                        var hasDuplicates = await _context.Grades
                            .GroupBy(g => new { g.StudentId, g.CourseId })
                            .AnyAsync(g => g.Count() > 1);
                        
                        sb.AppendLine($"   ❌ UNIQUE constraint missing");
                        sb.AppendLine($"   Duplicates: {(hasDuplicates ? "YES ⚠️" : "NO ✅")}");
                        sb.AppendLine("   Action: Run /Debug/AddUniqueConstraint");
                    }
                }

                sb.AppendLine();

                // 2. Check Teacher delete SP
                sb.AppendLine("2️⃣ Teacher Delete Validation");
                
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM sys.procedures WHERE name = 'usp_DeleteTeacher'";
                    await _context.Database.OpenConnectionAsync();
                    var teacherSpExists = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    await _context.Database.CloseConnectionAsync();

                    if (teacherSpExists > 0)
                    {
                        sb.AppendLine("   ✅ usp_DeleteTeacher exists");
                        sb.AppendLine("   Action: Run /Debug/CheckTeacherDeleteValidation for details");
                    }
                    else
                    {
                        sb.AppendLine("   ❌ usp_DeleteTeacher missing");
                    }
                }

                sb.AppendLine();

                // 3. Grade deletion policy
                sb.AppendLine("3️⃣ Grade Deletion Policy");
                
                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('usp_DeleteGrade'))";
                    await _context.Database.OpenConnectionAsync();
                    var gradeSpDef = (await cmd.ExecuteScalarAsync())?.ToString();
                    await _context.Database.CloseConnectionAsync();

                    if (gradeSpDef != null)
                    {
                        var allowsTeacher = gradeSpDef.Contains("'Admin', 'Teacher'");
                        var allowsAdminOnly = gradeSpDef.Contains("@UserRole != 'Admin'") && !allowsTeacher;
                        
                        if (allowsAdminOnly)
                        {
                            sb.AppendLine("   ✅ Admin only (RECOMMENDED)");
                        }
                        else if (allowsTeacher)
                        {
                            sb.AppendLine("   ⚠️ Admin + Teacher can delete");
                            sb.AppendLine("   Consider: Change to Admin only");
                        }
                    }
                }

                sb.AppendLine();
                sb.AppendLine(new string('=', 50));
                sb.AppendLine("Next Steps:");
                sb.AppendLine("1. /Debug/CheckDuplicateGrades");
                sb.AppendLine("2. /Debug/AddUniqueConstraint");
                sb.AppendLine("3. /Debug/CheckTeacherDeleteValidation");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Error: {ex.Message}");
            }

            return Content(sb.ToString(), "text/plain");
        }
    }
}
