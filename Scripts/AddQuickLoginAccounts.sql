-- ==============================
-- ADD QUICK LOGIN ACCOUNTS
-- Student Management System
-- ==============================
-- Purpose: Add teacher (nvanh) and student (nvan) accounts to Users table
--          for quick login functionality in the login page
-- ==============================

USE StudentManagementSystem;
GO

-- Check if accounts already exist
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'nvanh')
BEGIN
    -- Hash for 'teacher123' using SHA256: zeOD7ujuekQArfehX3FvHsrCVLCnctQW1qubdD6PSTI=
    INSERT INTO Users (Username, Email, PasswordHash, Role, EmailVerified, CreatedAt, LastLoginAt)
    VALUES (
        'nvanh',
        'nvanh@studentmanagement.edu.vn',
        'zeOD7ujuekQArfehX3FvHsrCVLCnctQW1qubdD6PSTI=',
        'Teacher',
        1, -- Email already verified
        GETUTCDATE(),
        NULL
    );
    PRINT '✓ Added teacher account: nvanh';
END
ELSE
BEGIN
    PRINT '⚠ Teacher account nvanh already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'nvan')
BEGIN
    -- Hash for 'student123' using SHA256: 3lFvmL9+7PPgqI5ZPnGGjGHmx1E0aRQleFLmVnXg/nc=
    INSERT INTO Users (Username, Email, PasswordHash, Role, EmailVerified, CreatedAt, LastLoginAt)
    VALUES (
        'nvan',
        'nvan@studentmanagement.edu.vn',
        '3lFvmL9+7PPgqI5ZPnGGjGHmx1E0aRQleFLmVnXg/nc=',
        'Student',
        1, -- Email already verified
        GETUTCDATE(),
        NULL
    );
    PRINT '✓ Added student account: nvan';
END
ELSE
BEGIN
    PRINT '⚠ Student account nvan already exists';
END
GO

-- Verify accounts
SELECT 
    Username,
    Role,
    EmailVerified,
    CreatedAt
FROM Users
WHERE Username IN ('nvanh', 'nvan');
GO

PRINT '';
PRINT '========================================';
PRINT '✓ QUICK LOGIN ACCOUNTS READY';
PRINT '========================================';
PRINT 'Teacher: nvanh / teacher123';
PRINT 'Student: nvan / student123';
PRINT '========================================';
