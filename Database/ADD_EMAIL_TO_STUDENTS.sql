-- ============================================
-- Add Email column to Students table
-- Run this script to add Email field support
-- ============================================

USE StudentManagementSystem; -- Fixed: Use correct database name
GO

-- Check if Email column already exists
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.Students') 
    AND name = 'Email'
)
BEGIN
    PRINT 'Adding Email column to Students table...';
    
    ALTER TABLE dbo.Students
    ADD Email NVARCHAR(100) NULL;
    
    PRINT '✅ Email column added successfully!';
END
ELSE
BEGIN
    PRINT 'ℹ️ Email column already exists in Students table.';
END
GO

-- Optional: Update existing students with placeholder emails
-- Uncomment if you want to set default emails
/*
UPDATE dbo.Students
SET Email = LOWER(Username) + '@student.edu.vn'
WHERE Email IS NULL;
GO
*/

PRINT '';
PRINT '============================================';
PRINT 'Email column setup complete!';
PRINT 'Students can now update their email via Profile page.';
PRINT '============================================';
GO
