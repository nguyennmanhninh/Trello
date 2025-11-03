# ============================================
# FIX ADMIN AND TEACHER LOGIN
# PowerShell script to execute SQL fix
# ============================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FIX ADMIN & TEACHER LOGIN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ServerInstance = ".\SQLEXPRESS"
$Database = "StudentManagementSystem"
$SqlFile = ".\Database\FIX_ADMIN_LOGIN.sql"

Write-Host "Server: $ServerInstance" -ForegroundColor Yellow
Write-Host "Database: $Database" -ForegroundColor Yellow
Write-Host "SQL File: $SqlFile" -ForegroundColor Yellow
Write-Host ""

# Check if SQL file exists
if (-Not (Test-Path $SqlFile)) {
    Write-Host "ERROR: SQL file not found: $SqlFile" -ForegroundColor Red
    exit 1
}

Write-Host "Executing SQL script..." -ForegroundColor Green
Write-Host ""

try {
    # Execute SQL using sqlcmd
    sqlcmd -S $ServerInstance -d $Database -i $SqlFile -b
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "SUCCESS! Admin and Teacher login fixed" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "You can now login with:" -ForegroundColor Cyan
        Write-Host "  admin / admin123" -ForegroundColor White
        Write-Host "  teacher / teacher123" -ForegroundColor White
        Write-Host "  nvanh / teacher123" -ForegroundColor White
        Write-Host "  ttbich / teacher123" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "ERROR: SQL execution failed" -ForegroundColor Red
        Write-Host "Exit code: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
