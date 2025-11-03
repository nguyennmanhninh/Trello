# ===================================================================
# SYNC DATABASE: REMOTE → LOCAL (CORRECT SCHEMA)
# ===================================================================
# Remote: 202.55.135.42 (Production - has different schema)
# Local: .\SQLEXPRESS (Development - new schema with stored procedures)
# ===================================================================

$remoteServer = "202.55.135.42"
$remoteUser = "sa"
$remotePassword = "Aa@0967941364"
$remoteDb = "StudentManagementSystem"

$localServer = ".\SQLEXPRESS"
$localDb = "StudentManagementSystem"

$exportFile = "Database\REMOTE_DATA_EXPORT_CORRECT.sql"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SYNC DATABASE: REMOTE -> LOCAL" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Remote: $remoteServer" -ForegroundColor Yellow
Write-Host "Local: $localServer" -ForegroundColor Yellow
Write-Host ""

# ===================================================================
# STEP 1: Export from Remote (with CORRECT schema)
# ===================================================================
Write-Host "Step 1: Exporting data from Remote..." -ForegroundColor Green

$exportQuery = @"
USE StudentManagementSystem;
GO

-- Disable constraints for import
SET NOCOUNT ON;
GO

PRINT 'Exporting data from Remote Server...';
GO

-- 1. Departments (SIMPLE: DepartmentId, DepartmentName, DepartmentCode)
SELECT 'INSERT INTO Departments (DepartmentId, DepartmentName, DepartmentCode) VALUES (''' + 
       DepartmentId + ''', N''' + DepartmentName + ''', N''' + DepartmentCode + ''');'
FROM Departments;

-- 2. Users (SIMPLE: UserId, Username, Password, Role, EntityId)
SELECT 'INSERT INTO Users (UserId, Username, Password, Role, EntityId) VALUES (' + 
       CAST(UserId AS VARCHAR) + ', ''' + Username + ''', ''' + Password + ''', ''' + Role + ''', ''' + 
       ISNULL(EntityId, '') + ''');'
FROM Users;

-- 3. Teachers (SCHEMA: TeacherId, FullName, DateOfBirth, Gender, Phone, Address, Username, Password, DepartmentId)
SELECT 'INSERT INTO Teachers (TeacherId, FullName, DateOfBirth, Gender, Phone, Address, Username, Password, DepartmentId) VALUES (''' + 
       TeacherId + ''', N''' + FullName + ''', ''' + CONVERT(VARCHAR, DateOfBirth, 23) + ''', ' + 
       CAST(Gender AS VARCHAR) + ', ''' + ISNULL(Phone, '') + ''', N''' + ISNULL(Address, '') + ''', ''' + 
       Username + ''', ''' + Password + ''', ''' + DepartmentId + ''');'
FROM Teachers;

-- 4. Classes (SCHEMA: ClassId, ClassName, DepartmentId, TeacherId)
SELECT 'INSERT INTO Classes (ClassId, ClassName, DepartmentId, TeacherId) VALUES (''' + 
       ClassId + ''', N''' + ClassName + ''', ''' + DepartmentId + ''', ''' + TeacherId + ''');'
FROM Classes;

-- 5. Courses (SCHEMA: CourseId, CourseName, Credits, DepartmentId, TeacherId)
SELECT 'INSERT INTO Courses (CourseId, CourseName, Credits, DepartmentId, TeacherId) VALUES (''' + 
       CourseId + ''', N''' + CourseName + ''', ' + CAST(Credits AS VARCHAR) + ', ''' + 
       DepartmentId + ''', ''' + TeacherId + ''');'
FROM Courses;

-- 6. Students (SCHEMA: StudentId, FullName, DateOfBirth, Gender, Phone, Address, ClassId, Username, Password)
-- Note: Remote has FullName directly (not FirstName/LastName)
SELECT 'INSERT INTO Students (StudentId, FullName, DateOfBirth, Gender, Phone, Address, ClassId, Username, Password) VALUES (''' + 
       StudentId + ''', N''' + FullName + ''', ''' + CONVERT(VARCHAR, DateOfBirth, 23) + ''', ' + 
       CAST(Gender AS VARCHAR) + ', ''' + ISNULL(Phone, '') + ''', N''' + ISNULL(Address, '') + ''', ''' + 
       ClassId + ''', ''' + Username + ''', ''' + Password + ''');'
FROM Students;

-- 7. Grades (SCHEMA: StudentId, CourseId, Score, Classification)
SELECT 'INSERT INTO Grades (StudentId, CourseId, Score, Classification) VALUES (''' + 
       StudentId + ''', ''' + CourseId + ''', ' + CAST(Score AS VARCHAR) + ', N''' + 
       ISNULL(Classification, '') + ''');'
FROM Grades;

PRINT 'Export completed!';
GO
"@

Write-Host "  Connecting to remote server..." -ForegroundColor Gray
sqlcmd -S $remoteServer -U $remoteUser -P $remotePassword -d $remoteDb -Q $exportQuery -o $exportFile -h -1 -W | Out-Null

if ($?) {
    Write-Host "  Export completed!" -ForegroundColor Green
    $lineCount = (Get-Content $exportFile | Measure-Object -Line).Lines
    Write-Host "  Generated file: $exportFile ($lineCount lines)" -ForegroundColor Gray
} else {
    Write-Host "  Export FAILED!" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ===================================================================
# STEP 2: Clear Local Data
# ===================================================================
Write-Host "Step 2: Clearing local data..." -ForegroundColor Green

$clearQuery = @"
USE StudentManagementSystem;
GO

-- Clear in reverse dependency order
DELETE FROM Grades;
SELECT '  (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows) Grades - DELETED';

DELETE FROM Students;
SELECT '  (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows) Students - DELETED';

DELETE FROM Courses;
SELECT '  (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows) Courses - DELETED';

DELETE FROM Classes;
SELECT '  (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows) Classes - DELETED';

DELETE FROM Teachers;
SELECT '  (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows) Teachers - DELETED';

DELETE FROM Users;
SELECT '  (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows) Users - DELETED';

DELETE FROM Departments;
SELECT '  (' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows) Departments - DELETED';

PRINT 'Local data cleared';
GO
"@

sqlcmd -S $localServer -E -d $localDb -Q $clearQuery

Write-Host ""

# ===================================================================
# STEP 3: Import to Local
# ===================================================================
Write-Host "Step 3: Importing to local..." -ForegroundColor Green

# IMPORTANT: Students table in local has computed column FullName
# We need to split FullName into FirstName + LastName
# OR drop the computed column temporarily

Write-Host "  Preparing Students table for import..." -ForegroundColor Gray
sqlcmd -S $localServer -E -d $localDb -Q @"
ALTER TABLE Students DROP COLUMN FullName;
ALTER TABLE Students ADD FirstName NVARCHAR(50) NULL;
ALTER TABLE Students ADD LastName NVARCHAR(50) NULL;
"@ | Out-Null

sqlcmd -S $localServer -E -d $localDb -i $exportFile

if ($?) {
    Write-Host "  Import completed!" -ForegroundColor Green
} else {
    Write-Host "  Import FAILED!" -ForegroundColor Red
    exit 1
}

# Re-add computed column
Write-Host "  Updating FirstName/LastName from FullName..." -ForegroundColor Gray
sqlcmd -S $localServer -E -d $localDb -Q @"
-- Split FullName (from remote) into FirstName + LastName
UPDATE Students
SET FirstName = LEFT(FullName, CHARINDEX(' ', FullName + ' ') - 1),
    LastName = CASE 
        WHEN CHARINDEX(' ', FullName) > 0 
        THEN SUBSTRING(FullName, CHARINDEX(' ', FullName) + 1, 100)
        ELSE ''
    END;

-- Drop FullName column (was imported from remote)
ALTER TABLE Students DROP COLUMN FullName;

-- Re-add as computed column
ALTER TABLE Students ADD FullName AS (FirstName + ' ' + LastName);
"@ | Out-Null

Write-Host ""

# ===================================================================
# STEP 4: Verify
# ===================================================================
Write-Host "Step 4: Verifying data..." -ForegroundColor Green

$verifyQuery = @"
USE StudentManagementSystem;
GO

SELECT 'Departments: ' + CAST(COUNT(*) AS VARCHAR) FROM Departments;
SELECT 'Users: ' + CAST(COUNT(*) AS VARCHAR) FROM Users;
SELECT 'Teachers: ' + CAST(COUNT(*) AS VARCHAR) FROM Teachers;
SELECT 'Classes: ' + CAST(COUNT(*) AS VARCHAR) FROM Classes;
SELECT 'Courses: ' + CAST(COUNT(*) AS VARCHAR) FROM Courses;
SELECT 'Students: ' + CAST(COUNT(*) AS VARCHAR) FROM Students;
SELECT 'Grades: ' + CAST(COUNT(*) AS VARCHAR) FROM Grades;

PRINT '';
PRINT 'SYNC COMPLETED!';
GO
"@

sqlcmd -S $localServer -E -d $localDb -Q $verifyQuery

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SYNC COMPLETED!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
