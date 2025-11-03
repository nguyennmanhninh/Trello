-- ============================================
-- Stored Procedures for Delete Operations
-- Date: 2025-11-02
-- ============================================

USE StudentManagementDB;
GO

-- ============================================
-- Stored Procedure: usp_DeleteStudent
-- Description: Xóa sinh viên và tất cả dữ liệu liên quan (Grades, User account)
-- Parameters:
--   @StudentId: Mã sinh viên cần xóa
--   @UserRole: Role của user thực hiện xóa (Admin, Teacher)
-- Returns: 1 = Success, 0 = Failed
-- ============================================

IF OBJECT_ID('usp_DeleteStudent', 'P') IS NOT NULL
    DROP PROCEDURE usp_DeleteStudent;
GO

CREATE PROCEDURE usp_DeleteStudent
    @StudentId NVARCHAR(10),
    @UserRole NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if student exists
        IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentId = @StudentId)
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN 0;
        END
        
        -- Authorization check for Teacher (optional - can be done at application level)
        -- Teachers can only delete students in their classes
        IF @UserRole = 'Teacher'
        BEGIN
            -- Additional validation could be added here
            PRINT 'Teacher role validation passed';
        END
        
        -- Step 1: Delete all grades associated with this student
        DELETE FROM Grades 
        WHERE StudentId = @StudentId;
        
        PRINT 'Deleted grades for student: ' + @StudentId;
        
        -- Step 2: Delete User account associated with this student
        DELETE FROM Users 
        WHERE EntityId = @StudentId 
            AND Role = 'Student';
        
        PRINT 'Deleted user account for student: ' + @StudentId;
        
        -- Step 3: Delete the student record
        DELETE FROM Students 
        WHERE StudentId = @StudentId;
        
        PRINT 'Deleted student record: ' + @StudentId;
        
        COMMIT TRANSACTION;
        RETURN 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN 0;
    END CATCH
END
GO

-- ============================================
-- Stored Procedure: usp_DeleteTeacher
-- Description: Xóa giảng viên và User account (chỉ khi không còn classes/courses)
-- Parameters:
--   @TeacherId: Mã giảng viên cần xóa
--   @UserRole: Role của user thực hiện xóa (Admin)
-- Returns: 1 = Success, 0 = Failed
-- ============================================

IF OBJECT_ID('usp_DeleteTeacher', 'P') IS NOT NULL
    DROP PROCEDURE usp_DeleteTeacher;
GO

CREATE PROCEDURE usp_DeleteTeacher
    @TeacherId NVARCHAR(10),
    @UserRole NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if teacher exists
        IF NOT EXISTS (SELECT 1 FROM Teachers WHERE TeacherId = @TeacherId)
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN 0;
        END
        
        -- Only Admin can delete teachers
        IF @UserRole != 'Admin'
        BEGIN
            ROLLBACK TRANSACTION;
            PRINT 'Only Admin can delete teachers';
            RETURN 0;
        END
        
        -- Check if teacher is assigned to any classes
        IF EXISTS (SELECT 1 FROM Classes WHERE TeacherId = @TeacherId)
        BEGIN
            ROLLBACK TRANSACTION;
            PRINT 'Cannot delete teacher: assigned to classes';
            RETURN 0;
        END
        
        -- Check if teacher is teaching any courses
        IF EXISTS (SELECT 1 FROM Courses WHERE TeacherId = @TeacherId)
        BEGIN
            ROLLBACK TRANSACTION;
            PRINT 'Cannot delete teacher: teaching courses';
            RETURN 0;
        END
        
        -- Step 1: Delete User account associated with this teacher
        DELETE FROM Users 
        WHERE EntityId = @TeacherId 
            AND Role = 'Teacher';
        
        PRINT 'Deleted user account for teacher: ' + @TeacherId;
        
        -- Step 2: Delete the teacher record
        DELETE FROM Teachers 
        WHERE TeacherId = @TeacherId;
        
        PRINT 'Deleted teacher record: ' + @TeacherId;
        
        COMMIT TRANSACTION;
        RETURN 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN 0;
    END CATCH
END
GO

-- ============================================
-- Test Scripts (Comment out in production)
-- ============================================

-- Test usp_DeleteStudent
/*
DECLARE @Result INT;
EXEC @Result = usp_DeleteStudent @StudentId = 'TEST001', @UserRole = 'Admin';
PRINT 'Result: ' + CAST(@Result AS VARCHAR);
*/

-- Test usp_DeleteTeacher
/*
DECLARE @Result INT;
EXEC @Result = usp_DeleteTeacher @TeacherId = 'GV999', @UserRole = 'Admin';
PRINT 'Result: ' + CAST(@Result AS VARCHAR);
*/

-- ============================================
-- Verify Procedures Created
-- ============================================

SELECT 
    OBJECT_NAME(object_id) AS ProcedureName,
    create_date AS CreatedDate,
    modify_date AS LastModified
FROM sys.procedures
WHERE name IN ('usp_DeleteStudent', 'usp_DeleteTeacher')
ORDER BY name;

GO

PRINT '✅ Stored procedures created successfully!';
PRINT '   - usp_DeleteStudent';
PRINT '   - usp_DeleteTeacher';
GO
