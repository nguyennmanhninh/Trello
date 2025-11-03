# =============================================
# SYNC DATA FROM REMOTE TO LOCAL
# Using sqlcmd to export and import data
# =============================================

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SYNC DATABASE: REMOTE -> LOCAL" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$RemoteServer = "202.55.135.42"
$RemoteUser = "sa"
$RemotePassword = "Aa@0967941364"
$LocalServer = ".\SQLEXPRESS"
$DatabaseName = "StudentManagementSystem"

Write-Host "Remote: $RemoteServer" -ForegroundColor Yellow
Write-Host "Local: $LocalServer" -ForegroundColor Yellow
Write-Host ""

# Step 1: Export all data from remote
Write-Host "Step 1: Exporting data from Remote..." -ForegroundColor Green

$ExportSQL = @"
SET NOCOUNT ON;

-- Departments
SELECT 'INSERT INTO Departments VALUES (''' + DepartmentId + ''', ''' + DepartmentCode + ''', ''' + DepartmentName + ''', ' + 
    CASE WHEN Description IS NULL THEN 'NULL' ELSE '''' + REPLACE(Description, '''', '''''') + '''' END + ');'
FROM Departments;

-- Users  
SELECT 'INSERT INTO Users (Username, PasswordHash, FullName, Role, CreatedAt) VALUES (''' + Username + ''', ''' + PasswordHash + ''', ''' + REPLACE(FullName, '''', '''''') + ''', ''' + Role + ''', ''' + CONVERT(VARCHAR, CreatedAt, 120) + ''');'
FROM Users;

-- Teachers
SELECT 'INSERT INTO Teachers VALUES (''' + TeacherId + ''', ''' + REPLACE(FullName, '''', '''''') + ''', ''' + CONVERT(VARCHAR, DateOfBirth, 23) + ''', ' + CAST(Gender AS VARCHAR) + ', ''' + Phone + ''', ''' + REPLACE(Address, '''', '''''') + ''', ''' + Username + ''', ''' + Password + ''', ''' + DepartmentId + ''');'
FROM Teachers;

-- Classes
SELECT 'INSERT INTO Classes VALUES (''' + ClassId + ''', ''' + ClassName + ''', ' + CAST(AcademicYear AS VARCHAR) + ', ''' + TeacherId + ''', ''' + DepartmentId + ''');'
FROM Classes;

-- Courses
SELECT 'INSERT INTO Courses VALUES (''' + CourseId + ''', ''' + CourseCode + ''', ''' + REPLACE(CourseName, '''', '''''') + ''', ' + CAST(Credits AS VARCHAR) + ', ''' + DepartmentId + ''', ' + 
    CASE WHEN TeacherId IS NULL THEN 'NULL' ELSE '''' + TeacherId + '''' END + ');'
FROM Courses;

-- Students
SELECT 'INSERT INTO Students (StudentId, FirstName, LastName, DateOfBirth, Email, Phone, Address, ClassId) VALUES (''' + 
    StudentId + ''', ''' + REPLACE(FirstName, '''', '''''') + ''', ''' + REPLACE(LastName, '''', '''''') + ''', ''' + 
    CONVERT(VARCHAR, DateOfBirth, 23) + ''', ''' + Email + ''', ''' + Phone + ''', ''' + REPLACE(Address, '''', '''''') + ''', ''' + ClassId + ''');'
FROM Students;

-- Grades
SELECT 'INSERT INTO Grades (StudentId, CourseId, ClassId, Semester, Score, GradeDate) VALUES (''' + 
    StudentId + ''', ''' + CourseId + ''', ''' + ClassId + ''', ' + CAST(Semester AS VARCHAR) + ', ' + CAST(Score AS VARCHAR) + ', ''' + 
    CONVERT(VARCHAR, GradeDate, 120) + ''');'
FROM Grades;
"@

$ExportSQL | Out-File -FilePath "export_query.sql" -Encoding ASCII

Write-Host "  Connecting to remote server..." -ForegroundColor Cyan

try {
    sqlcmd -S $RemoteServer -U $RemoteUser -P $RemotePassword -d $DatabaseName -i "export_query.sql" -o "Database\REMOTE_DATA_EXPORT.sql" -h -1 -W
    Write-Host "  Export completed!" -ForegroundColor Green
}
catch {
    Write-Host "  Error: $_" -ForegroundColor Red
    exit 1
}

# Step 2: Clear local data
Write-Host ""
Write-Host "Step 2: Clearing local data..." -ForegroundColor Green

$ClearSQL = @"
DELETE FROM Grades;
DELETE FROM Students;
DELETE FROM Courses;
DELETE FROM Classes;
DELETE FROM Teachers;
DELETE FROM Users;
DELETE FROM Departments;
PRINT 'Local data cleared';
"@

$ClearSQL | Out-File -FilePath "clear_local.sql" -Encoding ASCII
sqlcmd -S $LocalServer -E -d $DatabaseName -i "clear_local.sql"

# Step 3: Import to local
Write-Host "Step 3: Importing to local..." -ForegroundColor Green

sqlcmd -S $LocalServer -E -d $DatabaseName -i "Database\REMOTE_DATA_EXPORT.sql" -o "Database\import_result.txt"

Write-Host ""
Write-Host "Step 4: Verifying data..." -ForegroundColor Green

sqlcmd -S $LocalServer -E -d $DatabaseName -Q "SELECT 'Departments' AS TableName, COUNT(*) AS RowCount FROM Departments UNION ALL SELECT 'Users', COUNT(*) FROM Users UNION ALL SELECT 'Teachers', COUNT(*) FROM Teachers UNION ALL SELECT 'Classes', COUNT(*) FROM Classes UNION ALL SELECT 'Courses', COUNT(*) FROM Courses UNION ALL SELECT 'Students', COUNT(*) FROM Students UNION ALL SELECT 'Grades', COUNT(*) FROM Grades"

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SYNC COMPLETED!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next: Run UPDATE_SCHEMA_FOR_SPS.sql to add missing columns" -ForegroundColor Yellow
