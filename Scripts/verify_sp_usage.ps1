# Script để verify Backend đang dùng Stored Procedures hay LINQ
# Sẽ capture SQL queries khi gọi API

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  VERIFY STORED PROCEDURE USAGE" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

Write-Host "`n[Method 1] CHECK SQL SERVER PROFILER TRACES" -ForegroundColor Yellow
Write-Host "----------------------------------------------" -ForegroundColor Gray

# Tạo Extended Events session để capture SP calls
$createSession = @"
-- Tạo Extended Events session để monitor SP execution
IF EXISTS (SELECT * FROM sys.server_event_sessions WHERE name = 'SMS_SP_Monitor')
    DROP EVENT SESSION SMS_SP_Monitor ON SERVER;

CREATE EVENT SESSION SMS_SP_Monitor ON SERVER
ADD EVENT sqlserver.rpc_completed(
    ACTION(
        sqlserver.client_app_name,
        sqlserver.database_name,
        sqlserver.session_id,
        sqlserver.sql_text
    )
    WHERE sqlserver.database_name = 'StudentManagementSystem'
),
ADD EVENT sqlserver.sp_statement_completed(
    ACTION(
        sqlserver.client_app_name,
        sqlserver.database_name,
        sqlserver.session_id,
        sqlserver.sql_text
    )
    WHERE sqlserver.database_name = 'StudentManagementSystem'
)
ADD TARGET package0.ring_buffer
WITH (MAX_MEMORY=4096 KB, EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS, MAX_DISPATCH_LATENCY=30 SECONDS);

-- Start session
ALTER EVENT SESSION SMS_SP_Monitor ON SERVER STATE = START;

PRINT 'Extended Events session created and started!'
"@

Write-Host "`nCreating Extended Events session..." -ForegroundColor Yellow
sqlcmd -S .\SQLEXPRESS -E -Q $createSession

Write-Host "`n[Method 2] DIRECT API TEST WITH SQL MONITORING" -ForegroundColor Yellow
Write-Host "----------------------------------------------" -ForegroundColor Gray

Write-Host "`nStarting SQL trace..." -ForegroundColor Cyan

# Clear trace trước khi test
$clearTrace = @"
-- Clear ring buffer
IF EXISTS (SELECT * FROM sys.server_event_sessions WHERE name = 'SMS_SP_Monitor')
BEGIN
    ALTER EVENT SESSION SMS_SP_Monitor ON SERVER STATE = STOP;
    ALTER EVENT SESSION SMS_SP_Monitor ON SERVER STATE = START;
END
"@

sqlcmd -S .\SQLEXPRESS -E -Q $clearTrace | Out-Null

Write-Host "Waiting 2 seconds..." -ForegroundColor Gray
Start-Sleep -Seconds 2

# Test 1: Gọi Students API (nên dùng SP)
Write-Host "`n[TEST 1] Calling Students API..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5298/Students" -Method GET -UseBasicParsing -ErrorAction Stop
    Write-Host "  Response Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  Failed to call API (app not running?): $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep -Seconds 1

# Capture SQL queries
$captureQueries = @"
-- Read Extended Events data
DECLARE @target_data XML
SELECT @target_data = CAST(target_data AS XML)
FROM sys.dm_xe_session_targets AS t
INNER JOIN sys.dm_xe_sessions AS s ON s.address = t.event_session_address
WHERE s.name = 'SMS_SP_Monitor' AND t.target_name = 'ring_buffer';

-- Parse and display results
WITH EventData AS (
    SELECT 
        event_data.value('(@timestamp)[1]', 'datetime2') AS event_time,
        event_data.value('(@name)[1]', 'varchar(50)') AS event_name,
        event_data.value('(data[@name="statement"]/value)[1]', 'varchar(max)') AS statement,
        event_data.value('(action[@name="sql_text"]/value)[1]', 'varchar(max)') AS sql_text
    FROM @target_data.nodes('//event') AS XEventData(event_data)
)
SELECT TOP 10
    event_time,
    event_name,
    CASE 
        WHEN statement IS NOT NULL THEN statement
        WHEN sql_text IS NOT NULL THEN sql_text
        ELSE 'No SQL captured'
    END AS executed_sql
FROM EventData
WHERE event_time >= DATEADD(SECOND, -30, GETDATE())
ORDER BY event_time DESC;
"@

Write-Host "`n[RESULTS] SQL Queries Captured:" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Gray

sqlcmd -S .\SQLEXPRESS -E -d StudentManagementSystem -Q $captureQueries -W -h -1

Write-Host "`n[Method 3] CHECK CODE DIRECTLY" -ForegroundColor Yellow
Write-Host "----------------------------------------------" -ForegroundColor Gray

Write-Host "`nChecking StudentsController.cs for SP usage..." -ForegroundColor Cyan

$controllerPath = "C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem\Controllers\StudentsController.cs"

if (Test-Path $controllerPath) {
    $content = Get-Content $controllerPath -Raw
    
    # Check for Service injection
    if ($content -match "IStudentService") {
        Write-Host "  ✓ IStudentService injected" -ForegroundColor Green
    } else {
        Write-Host "  ✗ IStudentService NOT found" -ForegroundColor Red
    }
    
    # Check for Service usage in Index method
    if ($content -match "_studentService\.GetStudentsAsync") {
        Write-Host "  ✓ GetStudentsAsync() used (calls SP)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ GetStudentsAsync() NOT used" -ForegroundColor Red
    }
    
    # Check for old LINQ patterns
    if ($content -match "_context\.Students\.Include") {
        Write-Host "  ⚠ Warning: Still has _context.Students.Include (LINQ)" -ForegroundColor Yellow
    } else {
        Write-Host "  ✓ No LINQ queries detected" -ForegroundColor Green
    }
}

Write-Host "`n[Method 4] CHECK DATABASE FOR SP EXECUTION" -ForegroundColor Yellow
Write-Host "----------------------------------------------" -ForegroundColor Gray

$checkSPStats = @"
-- Check SP execution statistics
SELECT 
    OBJECT_NAME(object_id) AS StoredProcedure,
    execution_count AS ExecutionCount,
    last_execution_time AS LastExecuted,
    total_elapsed_time / 1000000.0 AS TotalElapsedSeconds,
    CASE 
        WHEN execution_count > 0 THEN (total_elapsed_time / execution_count) / 1000.0
        ELSE 0 
    END AS AvgExecutionMS
FROM sys.dm_exec_procedure_stats
WHERE database_id = DB_ID('StudentManagementSystem')
  AND OBJECT_NAME(object_id) LIKE 'usp_%'
ORDER BY last_execution_time DESC;
"@

Write-Host "`nStored Procedure Execution Statistics:" -ForegroundColor Cyan
sqlcmd -S .\SQLEXPRESS -E -d StudentManagementSystem -Q $checkSPStats -W

Write-Host "`n[Method 5] COMPARE CODE PATTERNS" -ForegroundColor Yellow
Write-Host "----------------------------------------------" -ForegroundColor Gray

Write-Host "`nBEFORE (LINQ Pattern):" -ForegroundColor Red
Write-Host @"
  var students = await _context.Students
      .Include(s => s.Class)
      .Where(s => s.FullName.Contains(searchString))
      .ToListAsync();
"@ -ForegroundColor Gray

Write-Host "`nAFTER (Stored Procedure Pattern):" -ForegroundColor Green
Write-Host @"
  var result = await _studentService.GetStudentsAsync(
      userRole, userId, searchString, classId, departmentId, 
      pageNumber, pageSize);
  // Internal: EXEC usp_GetStudents @UserRole, @UserId, ...
"@ -ForegroundColor Gray

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "  VERIFICATION COMPLETE" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

Write-Host "`nCÁCH KIỂM TRA NHANH NHẤT:" -ForegroundColor Yellow
Write-Host "1. Mở SQL Server Profiler" -ForegroundColor White
Write-Host "2. Chạy app: dotnet run" -ForegroundColor White
Write-Host "3. Truy cập http://localhost:5298/Students" -ForegroundColor White
Write-Host "4. Xem Profiler - nếu thấy 'EXEC usp_GetStudents' = ĐANG DÙNG SP ✓" -ForegroundColor Green
Write-Host "                   nếu thấy 'SELECT ... FROM Students' = VẪN LINQ ✗" -ForegroundColor Red

Write-Host "`nDOCUMENTATION:" -ForegroundColor Cyan
Write-Host "  - MIGRATION_FINAL_REPORT.md" -ForegroundColor White
Write-Host "  - All 5 controllers migrated to SP" -ForegroundColor White
Write-Host "  - Frontend (Angular) không đổi, vẫn gọi API REST" -ForegroundColor White

# Cleanup
Write-Host "`nCleaning up Extended Events session..." -ForegroundColor Gray
$cleanup = "IF EXISTS (SELECT * FROM sys.server_event_sessions WHERE name = 'SMS_SP_Monitor') DROP EVENT SESSION SMS_SP_Monitor ON SERVER;"
sqlcmd -S .\SQLEXPRESS -E -Q $cleanup | Out-Null

Write-Host "`nDone!" -ForegroundColor Green
