# =============================================
# SYNC DATA FROM REMOTE TO LOCAL
# Copy all data from Production (202.55.135.42) to Local (.\SQLEXPRESS)
# =============================================

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SYNC DATABASE: REMOTE → LOCAL" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$RemoteServer = "202.55.135.42"
$RemoteUser = "sa"
$RemotePassword = "Aa@0967941364"
$LocalServer = ".\SQLEXPRESS"
$DatabaseName = "StudentManagementSystem"

Write-Host "📡 Remote Server: $RemoteServer" -ForegroundColor Yellow
Write-Host "💾 Local Server: $LocalServer" -ForegroundColor Yellow
Write-Host "🗄️  Database: $DatabaseName" -ForegroundColor Yellow
Write-Host ""

# Step 1: Export data from Remote
Write-Host "Step 1: Exporting data from Remote server..." -ForegroundColor Green
Write-Host ""

$ExportFile = "Database\REMOTE_DATA_EXPORT.sql"

# Export tables in correct order (respect foreign keys)
$tables = @(
    "Departments",
    "Users", 
    "Teachers",
    "Classes",
    "Courses",
    "Students",
    "Grades"
)

$exportScript = @"
-- =============================================
-- DATA EXPORT FROM REMOTE SERVER
-- Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
-- Source: $RemoteServer
-- =============================================

USE StudentManagementSystem;
GO

PRINT 'Clearing existing data...';

-- Delete in reverse order (respect foreign keys)
DELETE FROM Grades;
DELETE FROM Students;
DELETE FROM Courses;
DELETE FROM Classes;
DELETE FROM Teachers;
DELETE FROM Users;
DELETE FROM Departments;

PRINT 'Data cleared successfully.';
PRINT '';

"@

foreach ($table in $tables) {
    Write-Host "  📦 Exporting $table..." -ForegroundColor Cyan
    
    try {
        # Get data from remote
        $query = "SELECT * FROM $table"
        $data = Invoke-Sqlcmd -ServerInstance $RemoteServer -Database $DatabaseName -Username $RemoteUser -Password $RemotePassword -Query $query -ErrorAction Stop
        
        if ($data.Count -gt 0) {
            $exportScript += "PRINT 'Inserting $($data.Count) rows into $table...';" + "`n"
            
            # Generate INSERT statements
            foreach ($row in $data) {
                $columns = $row.PSObject.Properties.Name
                $values = @()
                
                foreach ($col in $columns) {
                    $value = $row.$col
                    
                    if ($null -eq $value) {
                        $values += "NULL"
                    }
                    elseif ($value -is [string]) {
                        $escapedValue = $value.Replace("'", "''")
                        $values += "'$escapedValue'"
                    }
                    elseif ($value -is [datetime]) {
                        $values += "'$($value.ToString("yyyy-MM-dd HH:mm:ss"))'"
                    }
                    elseif ($value -is [bool]) {
                        $values += if ($value) { "1" } else { "0" }
                    }
                    else {
                        $values += $value
                    }
                }
                
                $columnList = ($columns -join ", ")
                $valueList = ($values -join ", ")
                $exportScript += "INSERT INTO $table ($columnList) VALUES ($valueList);" + "`n"
            }
            
            $exportScript += "PRINT '  ✓ Inserted $($data.Count) rows into $table';" + "`n"
            $exportScript += "GO" + "`n`n"
            
            Write-Host "  ✓ Exported $($data.Count) rows" -ForegroundColor Green
        }
        else {
            Write-Host "  ⚠ No data found in $table" -ForegroundColor Yellow
            $exportScript += "PRINT '  ⚠ No data in $table';" + "`n"
            $exportScript += "GO" + "`n`n"
        }
    }
    catch {
        Write-Host "  ❌ Error exporting $table : $_" -ForegroundColor Red
        $exportScript += "PRINT '  ❌ Error exporting $table';" + "`n"
        $exportScript += "GO" + "`n`n"
    }
}

$exportScript += @"

PRINT '';
PRINT '====================================';
PRINT 'DATA IMPORT COMPLETED';
PRINT '====================================';

-- Verify counts
SELECT 'Departments' AS TableName, COUNT(*) AS RowCount FROM Departments
UNION ALL
SELECT 'Users', COUNT(*) FROM Users
UNION ALL
SELECT 'Teachers', COUNT(*) FROM Teachers
UNION ALL
SELECT 'Classes', COUNT(*) FROM Classes
UNION ALL
SELECT 'Courses', COUNT(*) FROM Courses
UNION ALL
SELECT 'Students', COUNT(*) FROM Students
UNION ALL
SELECT 'Grades', COUNT(*) FROM Grades;
GO
"@

# Save export script
$exportScript | Out-File -FilePath $ExportFile -Encoding UTF8
Write-Host ""
Write-Host "✓ Export script saved to: $ExportFile" -ForegroundColor Green
Write-Host ""

# Step 2: Import to Local
Write-Host "Step 2: Importing data to Local server..." -ForegroundColor Green
Write-Host ""

try {
    sqlcmd -S $LocalServer -E -d $DatabaseName -i $ExportFile -o "Database\import_result.txt"
    
    Write-Host "✓ Data imported successfully!" -ForegroundColor Green
    Write-Host ""
    
    # Show results
    Write-Host "Import Results:" -ForegroundColor Cyan
    Get-Content "Database\import_result.txt" -Tail 20
}
catch {
    Write-Host "❌ Error importing data: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "✓ SYNC COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Summary:" -ForegroundColor Yellow
Write-Host "  Remote: $RemoteServer → Local: $LocalServer" -ForegroundColor White
Write-Host "  Database: $DatabaseName" -ForegroundColor White
Write-Host ""
Write-Host "📝 Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Update schema: sqlcmd -S .\SQLEXPRESS -E -d StudentManagementSystem -i Database\UPDATE_SCHEMA_FOR_SPS.sql" -ForegroundColor White
Write-Host "  2. Verify data: sqlcmd -S .\SQLEXPRESS -E -d StudentManagementSystem -Q 'SELECT COUNT(*) FROM Students'" -ForegroundColor White
Write-Host "  3. Test app: dotnet run" -ForegroundColor White
Write-Host ""
