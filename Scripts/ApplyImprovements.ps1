# =============================================
# APPLY DATABASE IMPROVEMENTS - AUTOMATED
# Purpose: Apply all 3 improvements automatically
# Date: October 24, 2025
# =============================================

param(
    [switch]$CheckOnly,
    [switch]$ApplyAll,
    [string]$BaseUrl = "http://localhost:5299"
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DATABASE IMPROVEMENTS AUTOMATION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to call API and display result
function Invoke-ApiCheck {
    param(
        [string]$Endpoint,
        [string]$Description
    )
    
    Write-Host "Checking: $Description" -ForegroundColor Yellow
    Write-Host "Endpoint: $BaseUrl/$Endpoint" -ForegroundColor Gray
    Write-Host ""
    
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/$Endpoint" -Method Get -ErrorAction Stop
        Write-Host $response -ForegroundColor White
        Write-Host ""
        return $true
    }
    catch {
        Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        return $false
    }
}

# =============================================
# STEP 1: Check Current Status
# =============================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host "STEP 1: CURRENT STATUS" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host ""

Invoke-ApiCheck -Endpoint "Debug/DatabaseImprovementsSummary" -Description "Overall Summary"

if ($CheckOnly) {
    Write-Host "Check-only mode. Exiting..." -ForegroundColor Yellow
    exit 0
}

# =============================================
# STEP 2: Check for Duplicate Grades
# =============================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host "STEP 2: CHECK DUPLICATE GRADES" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host ""

$noDuplicates = Invoke-ApiCheck -Endpoint "Debug/CheckDuplicateGrades" -Description "Duplicate Grades Check"

# =============================================
# STEP 3: Add UNIQUE Constraint (if no duplicates)
# =============================================
if ($ApplyAll -and $noDuplicates) {
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
    Write-Host "STEP 3: ADD UNIQUE CONSTRAINT" -ForegroundColor Green
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "⚠️  About to add UNIQUE constraint to Grades table" -ForegroundColor Yellow
    Write-Host "This will prevent duplicate grades (StudentId + CourseId)" -ForegroundColor Yellow
    Write-Host ""
    
    $confirm = Read-Host "Continue? (Y/N)"
    if ($confirm -eq "Y" -or $confirm -eq "y") {
        Invoke-ApiCheck -Endpoint "Debug/AddUniqueConstraint" -Description "Add UNIQUE Constraint"
    }
    else {
        Write-Host "❌ Skipped by user" -ForegroundColor Yellow
    }
}

# =============================================
# STEP 4: Verify Teacher Delete Validation
# =============================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host "STEP 4: TEACHER DELETE VALIDATION" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host ""

Invoke-ApiCheck -Endpoint "Debug/CheckTeacherDeleteValidation" -Description "Teacher Delete SP Validation"

# =============================================
# STEP 5: Final Summary
# =============================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host "STEP 5: FINAL SUMMARY" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host ""

Invoke-ApiCheck -Endpoint "Debug/DatabaseImprovementsSummary" -Description "Updated Summary"

# =============================================
# COMPLETION
# =============================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "AUTOMATION COMPLETED" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($ApplyAll) {
    Write-Host "✅ All automated improvements applied" -ForegroundColor Green
}
else {
    Write-Host "ℹ️  Check-only completed. Use -ApplyAll to apply changes" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📋 MANUAL ACTIONS STILL REQUIRED:" -ForegroundColor Yellow
Write-Host "  1. Review Grade Deletion Policy" -ForegroundColor White
Write-Host "     File: Database/FIX_GRADE_DELETION_POLICY.sql" -ForegroundColor Gray
Write-Host "     Choose: Option 1 (Admin only) or Option 2 (Audit trail)" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Update Frontend (if changed deletion policy)" -ForegroundColor White
Write-Host "     - GradesController" -ForegroundColor Gray
Write-Host "     - Angular grades component" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Test all CRUD operations" -ForegroundColor White
Write-Host "     - Try creating duplicate grade (should fail)" -ForegroundColor Gray
Write-Host "     - Try deleting teacher with classes (should fail)" -ForegroundColor Gray
Write-Host "     - Try deleting grade as Teacher (policy dependent)" -ForegroundColor Gray
Write-Host ""

# Open improvements panel in browser
Write-Host "Opening improvements panel in browser..." -ForegroundColor Cyan
Start-Process "$BaseUrl/Debug/ImprovementsPanel"

Write-Host ""
Write-Host "USAGE EXAMPLES:" -ForegroundColor Yellow
Write-Host "  .\ApplyImprovements.ps1 -CheckOnly      # Check status only" -ForegroundColor Gray
Write-Host "  .\ApplyImprovements.ps1 -ApplyAll       # Apply all improvements" -ForegroundColor Gray
Write-Host "  .\ApplyImprovements.ps1 -BaseUrl http://localhost:5298" -ForegroundColor Gray
Write-Host ""
