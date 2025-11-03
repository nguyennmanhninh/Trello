-- ============================================
-- Add Phone Number & Password Recovery Fields to Users Table
-- Date: 2025-11-03
-- Purpose: Enable password recovery via SMS
-- ============================================

USE StudentManagementSystem;
GO

-- Check if columns already exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Phone')
BEGIN
    ALTER TABLE Users
    ADD Phone NVARCHAR(15) NULL;
    PRINT '✅ Added Phone column to Users table';
END
ELSE
BEGIN
    PRINT '⚠️ Phone column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PhoneVerified')
BEGIN
    ALTER TABLE Users
    ADD PhoneVerified BIT NOT NULL DEFAULT 0;
    PRINT '✅ Added PhoneVerified column to Users table';
END
ELSE
BEGIN
    PRINT '⚠️ PhoneVerified column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ResetCode')
BEGIN
    ALTER TABLE Users
    ADD ResetCode NVARCHAR(6) NULL;
    PRINT '✅ Added ResetCode column to Users table';
END
ELSE
BEGIN
    PRINT '⚠️ ResetCode column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ResetCodeExpiry')
BEGIN
    ALTER TABLE Users
    ADD ResetCodeExpiry DATETIME NULL;
    PRINT '✅ Added ResetCodeExpiry column to Users table';
END
ELSE
BEGIN
    PRINT '⚠️ ResetCodeExpiry column already exists';
END
GO

-- Update existing users with phone numbers from Students/Teachers tables
UPDATE u
SET u.Phone = s.Phone
FROM Users u
INNER JOIN Students s ON u.EntityId = s.StudentId
WHERE u.Role = 'Student' AND u.Phone IS NULL AND s.Phone IS NOT NULL;
PRINT '✅ Updated Student phone numbers';
GO

UPDATE u
SET u.Phone = t.Phone
FROM Users u
INNER JOIN Teachers t ON u.EntityId = t.TeacherId
WHERE u.Role = 'Teacher' AND u.Phone IS NULL AND t.Phone IS NOT NULL;
PRINT '✅ Updated Teacher phone numbers';
GO

-- Display updated schema
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH, 
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '================================================';
PRINT '✅ Database schema updated successfully!';
PRINT '================================================';
GO
