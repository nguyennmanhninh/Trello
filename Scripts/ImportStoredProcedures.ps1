# =============================================
# Import Stored Procedures to SQL Server
# Created: 2024-10-24
# =============================================

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "IMPORT STORED PROCEDURES" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ServerName = ".\SQLEXPRESS"
$DatabaseName = "StudentManagementDB"
$SqlFile = "Database\STORED_PROCEDURES.sql"

# Check if SQL file exists
if (-not (Test-Path $SqlFile)) {
    Write-Host "❌ Error: SQL file not found at $SqlFile" -ForegroundColor Red
    exit 1
}

Write-Host "📂 SQL File: $SqlFile" -ForegroundColor Green
Write-Host "🗄️  Server: $ServerName" -ForegroundColor Green
Write-Host "💾 Database: $DatabaseName" -ForegroundColor Green
Write-Host ""

# Test connection
try {
    Write-Host "🔗 Testing connection..." -ForegroundColor Yellow
    $testQuery = "SELECT @@VERSION"
    $result = Invoke-Sqlcmd -ServerInstance $ServerName -Database "master" -Query $testQuery -ErrorAction Stop
    Write-Host "✅ Connection successful!" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ Connection failed: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Make sure SQL Server is running" -ForegroundColor White
    Write-Host "  2. Check server name: $ServerName" -ForegroundColor White
    Write-Host "  3. Install SqlServer module: Install-Module -Name SqlServer -Force" -ForegroundColor White
    exit 1
}

# Check if database exists
try {
    $checkDbQuery = "SELECT database_id FROM sys.databases WHERE name = '$DatabaseName'"
    $dbExists = Invoke-Sqlcmd -ServerInstance $ServerName -Database "master" -Query $checkDbQuery
    
    if (-not $dbExists) {
        Write-Host "❌ Database '$DatabaseName' does not exist!" -ForegroundColor Red
        Write-Host "💡 Please create the database first using FULL_DATABASE_SETUP.sql" -ForegroundColor Yellow
        exit 1
    }
    Write-Host "✅ Database '$DatabaseName' found!" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ Error checking database: $_" -ForegroundColor Red
    exit 1
}

# Import stored procedures
try {
    Write-Host "📥 Importing Stored Procedures..." -ForegroundColor Yellow
    Write-Host ""
    
    # Read and execute SQL file
    $sqlContent = Get-Content $SqlFile -Raw
    Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -Query $sqlContent -ErrorAction Stop
    
    Write-Host "✅ Stored Procedures imported successfully!" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "❌ Error importing stored procedures: $_" -ForegroundColor Red
    exit 1
}

# Verify stored procedures
try {
    Write-Host "🔍 Verifying Stored Procedures..." -ForegroundColor Yellow
    Write-Host ""
    
    $verifyQuery = @"
SELECT 
    SCHEMA_NAME(schema_id) AS [Schema],
    name AS ProcedureName,
    create_date AS Created,
    modify_date AS Modified
FROM sys.procedures
WHERE name LIKE 'usp_%'
ORDER BY name
"@
    
    $procedures = Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -Query $verifyQuery
    
    if ($procedures.Count -gt 0) {
        Write-Host "📋 Found $($procedures.Count) stored procedures:" -ForegroundColor Green
        Write-Host ""
        
        $procedures | ForEach-Object {
            Write-Host "  ✅ $($_.Schema).$($_.ProcedureName)" -ForegroundColor Cyan
        }
        
        Write-Host ""
    } else {
        Write-Host "⚠️  No stored procedures found!" -ForegroundColor Yellow
        Write-Host ""
    }
} catch {
    Write-Host "❌ Error verifying stored procedures: $_" -ForegroundColor Red
}

# Test a sample procedure
try {
    Write-Host "🧪 Testing sample procedure (usp_AuthenticateUser)..." -ForegroundColor Yellow
    Write-Host ""
    
    $testQuery = @"
DECLARE @Role NVARCHAR(20), @EntityId NVARCHAR(50), @FullName NVARCHAR(100), @Result INT;
EXEC @Result = usp_AuthenticateUser 'admin', 'admin123', @Role OUTPUT, @EntityId OUTPUT, @FullName OUTPUT;
SELECT @Result AS Result, @Role AS Role, @EntityId AS EntityId, @FullName AS FullName;
"@
    
    $testResult = Invoke-Sqlcmd -ServerInstance $ServerName -Database $DatabaseName -Query $testQuery
    
    if ($testResult.Result -eq 1) {
        Write-Host "  ✅ Authentication test passed!" -ForegroundColor Green
        Write-Host "  👤 Role: $($testResult.Role)" -ForegroundColor Cyan
        Write-Host "  🆔 EntityId: $($testResult.EntityId)" -ForegroundColor Cyan
        Write-Host "  📝 FullName: $($testResult.FullName)" -ForegroundColor Cyan
    } else {
        Write-Host "  ⚠️  Authentication test failed (Result: $($testResult.Result))" -ForegroundColor Yellow
    }
    Write-Host ""
} catch {
    Write-Host "⚠️  Test failed: $_" -ForegroundColor Yellow
    Write-Host ""
}

# Summary
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "✅ IMPORT COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📝 Next steps:" -ForegroundColor Yellow
Write-Host "  1. Update C# services to use stored procedures (AuthService.cs, StatisticsService.cs)" -ForegroundColor White
Write-Host "  2. Test authentication and statistics functions" -ForegroundColor White
Write-Host "  3. Run the application: dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "💡 Stored Procedures imported:" -ForegroundColor Yellow
Write-Host "  • usp_AuthenticateUser - Authenticate user login" -ForegroundColor White
Write-Host "  • usp_ChangePassword - Change user password" -ForegroundColor White
Write-Host "  • usp_GetStudents - Get students with filtering" -ForegroundColor White
Write-Host "  • usp_CreateStudent - Create new student" -ForegroundColor White
Write-Host "  • usp_UpdateStudent - Update student information" -ForegroundColor White
Write-Host "  • usp_DeleteStudent - Delete student" -ForegroundColor White
Write-Host "  • usp_GetStudentById - Get student details" -ForegroundColor White
Write-Host "  • usp_GetDashboardStatistics - Get statistics" -ForegroundColor White
Write-Host "  • usp_GetStudentCountByClass - Count by class" -ForegroundColor White
Write-Host "  • usp_GetStudentCountByDepartment - Count by department" -ForegroundColor White
Write-Host "  • usp_GetAverageScoreByClass - Average by class" -ForegroundColor White
Write-Host "  • usp_GetAverageScoreByCourse - Average by course" -ForegroundColor White
Write-Host ""
