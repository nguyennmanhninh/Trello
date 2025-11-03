-- ============================================
-- Script: Clean Up Orphaned User Accounts
-- Purpose: Xóa các User accounts không còn Student/Teacher record tương ứng
-- Date: 2025-11-02
-- ============================================

-- 1. Tìm các Student accounts không còn Student record
SELECT 
    u.UserId,
    u.Username,
    u.EntityId,
    u.Role,
    'No Student Record' AS Issue
FROM Users u
WHERE u.Role = 'Student'
    AND NOT EXISTS (
        SELECT 1 FROM Students s WHERE s.StudentId = u.EntityId
    );

-- 2. Tìm các Teacher accounts không còn Teacher record
SELECT 
    u.UserId,
    u.Username,
    u.EntityId,
    u.Role,
    'No Teacher Record' AS Issue
FROM Users u
WHERE u.Role = 'Teacher'
    AND NOT EXISTS (
        SELECT 1 FROM Teachers t WHERE t.TeacherId = u.EntityId
    );

-- 3. XÓA các Student accounts không còn Student record
-- CẢNH BÁO: Chạy lệnh này sẽ XÓA VĨNH VIỄN các tài khoản!
DELETE FROM Users
WHERE Role = 'Student'
    AND NOT EXISTS (
        SELECT 1 FROM Students s WHERE s.StudentId = Users.EntityId
    );

PRINT 'Đã xóa ' + CAST(@@ROWCOUNT AS VARCHAR) + ' Student accounts không còn Student record';

-- 4. XÓA các Teacher accounts không còn Teacher record
DELETE FROM Users
WHERE Role = 'Teacher'
    AND NOT EXISTS (
        SELECT 1 FROM Teachers t WHERE t.TeacherId = Users.EntityId
    );

PRINT 'Đã xóa ' + CAST(@@ROWCOUNT AS VARCHAR) + ' Teacher accounts không còn Teacher record';

-- 5. Verify kết quả - không còn orphaned accounts
SELECT 
    'After Cleanup' AS Status,
    COUNT(*) AS OrphanedStudentAccounts
FROM Users u
WHERE u.Role = 'Student'
    AND NOT EXISTS (SELECT 1 FROM Students s WHERE s.StudentId = u.EntityId);

SELECT 
    'After Cleanup' AS Status,
    COUNT(*) AS OrphanedTeacherAccounts
FROM Users u
WHERE u.Role = 'Teacher'
    AND NOT EXISTS (SELECT 1 FROM Teachers t WHERE t.TeacherId = u.EntityId);

GO
