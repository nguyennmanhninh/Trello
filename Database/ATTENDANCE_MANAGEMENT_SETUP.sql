-- =============================================
-- ATTENDANCE MANAGEMENT SYSTEM
-- Hệ thống quản lý điểm danh
-- Created: 2025-11-02
-- =============================================

USE StudentManagementSystem;
GO

-- =============================================
-- 1. CREATE TABLES
-- =============================================

-- Bảng AttendanceSessions: Lưu thông tin các buổi điểm danh
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AttendanceSessions')
BEGIN
    CREATE TABLE AttendanceSessions (
        SessionId INT IDENTITY(1,1) PRIMARY KEY,
        CourseId NVARCHAR(10) NOT NULL,
        TeacherId NVARCHAR(10) NOT NULL,
        SessionDate DATE NOT NULL,
        SessionTime TIME NOT NULL,
        SessionTitle NVARCHAR(200) NOT NULL, -- VD: "Buổi 1: Giới thiệu môn học"
        SessionType NVARCHAR(50) DEFAULT N'Lý thuyết', -- Lý thuyết, Thực hành, Kiểm tra
        Location NVARCHAR(100), -- Phòng học
        Duration INT DEFAULT 90, -- Thời lượng (phút)
        Notes NVARCHAR(500), -- Ghi chú
        Status NVARCHAR(20) DEFAULT N'Scheduled', -- Scheduled, Completed, Cancelled
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_AttendanceSessions_Courses FOREIGN KEY (CourseId) 
            REFERENCES Courses(CourseId) ON DELETE CASCADE,
        CONSTRAINT FK_AttendanceSessions_Teachers FOREIGN KEY (TeacherId) 
            REFERENCES Teachers(TeacherId) ON DELETE CASCADE,
        CONSTRAINT CHK_SessionStatus CHECK (Status IN (N'Scheduled', N'Completed', N'Cancelled'))
    );
    
    PRINT '✅ Created table: AttendanceSessions';
END
GO

-- Bảng Attendances: Lưu điểm danh của từng sinh viên
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Attendances')
BEGIN
    CREATE TABLE Attendances (
        AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
        SessionId INT NOT NULL,
        StudentId NVARCHAR(10) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT N'Present', -- Present, Absent, Late, Excused
        CheckInTime TIME, -- Giờ vào lớp
        Notes NVARCHAR(500), -- Ghi chú (lý do vắng, đi muộn...)
        MarkedByTeacherId NVARCHAR(10), -- Giáo viên điểm danh
        MarkedAt DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_Attendances_Sessions FOREIGN KEY (SessionId) 
            REFERENCES AttendanceSessions(SessionId) ON DELETE CASCADE,
        CONSTRAINT FK_Attendances_Students FOREIGN KEY (StudentId) 
            REFERENCES Students(StudentId) ON DELETE CASCADE,
        CONSTRAINT FK_Attendances_Teachers FOREIGN KEY (MarkedByTeacherId) 
            REFERENCES Teachers(TeacherId),
        CONSTRAINT CHK_AttendanceStatus CHECK (Status IN (N'Present', N'Absent', N'Late', N'Excused')),
        CONSTRAINT UQ_Attendance_Session_Student UNIQUE (SessionId, StudentId)
    );
    
    PRINT '✅ Created table: Attendances';
END
GO

-- =============================================
-- 2. CREATE INDEXES
-- =============================================

-- Index cho tìm kiếm sessions theo course và date
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AttendanceSessions_Course_Date')
BEGIN
    CREATE INDEX IX_AttendanceSessions_Course_Date 
    ON AttendanceSessions(CourseId, SessionDate DESC);
    PRINT '✅ Created index: IX_AttendanceSessions_Course_Date';
END
GO

-- Index cho tìm kiếm attendance theo student
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Attendances_Student')
BEGIN
    CREATE INDEX IX_Attendances_Student 
    ON Attendances(StudentId, SessionId);
    PRINT '✅ Created index: IX_Attendances_Student';
END
GO

-- =============================================
-- 3. CREATE STORED PROCEDURES
-- =============================================

-- SP: Tạo session và auto-add tất cả students trong course
IF OBJECT_ID('usp_CreateAttendanceSession', 'P') IS NOT NULL
    DROP PROCEDURE usp_CreateAttendanceSession;
GO

CREATE PROCEDURE usp_CreateAttendanceSession
    @CourseId NVARCHAR(10),
    @TeacherId NVARCHAR(10),
    @SessionDate DATE,
    @SessionTime TIME,
    @SessionTitle NVARCHAR(200),
    @SessionType NVARCHAR(50) = N'Lý thuyết',
    @Location NVARCHAR(100) = NULL,
    @Duration INT = 90,
    @SessionId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Insert session
        INSERT INTO AttendanceSessions (CourseId, TeacherId, SessionDate, SessionTime, 
                                        SessionTitle, SessionType, Location, Duration, Status)
        VALUES (@CourseId, @TeacherId, @SessionDate, @SessionTime, 
                @SessionTitle, @SessionType, @Location, @Duration, N'Scheduled');
        
        SET @SessionId = SCOPE_IDENTITY();
        
        -- Auto-add all students enrolled in this course (from Grades table)
        INSERT INTO Attendances (SessionId, StudentId, Status, MarkedByTeacherId)
        SELECT @SessionId, StudentId, N'Absent', @TeacherId
        FROM Grades
        WHERE CourseId = @CourseId
        GROUP BY StudentId;
        
        COMMIT TRANSACTION;
        RETURN 1; -- Success
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        PRINT ERROR_MESSAGE();
        RETURN 0; -- Failure
    END CATCH
END
GO

PRINT '✅ Created procedure: usp_CreateAttendanceSession';
GO

-- SP: Cập nhật điểm danh cho nhiều sinh viên
IF OBJECT_ID('usp_MarkAttendance', 'P') IS NOT NULL
    DROP PROCEDURE usp_MarkAttendance;
GO

CREATE PROCEDURE usp_MarkAttendance
    @SessionId INT,
    @TeacherId VARCHAR(10),
    @AttendanceData NVARCHAR(MAX) -- JSON: [{"StudentId":"SV001","Status":"Present","CheckInTime":"08:00"}]
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Parse JSON và update attendance
        UPDATE a
        SET a.Status = j.Status,
            a.CheckInTime = CASE WHEN j.CheckInTime IS NOT NULL THEN CAST(j.CheckInTime AS TIME) ELSE NULL END,
            a.Notes = j.Notes,
            a.MarkedByTeacherId = @TeacherId,
            a.MarkedAt = GETDATE()
        FROM Attendances a
        INNER JOIN OPENJSON(@AttendanceData)
        WITH (
            StudentId VARCHAR(10) '$.StudentId',
            Status NVARCHAR(20) '$.Status',
            CheckInTime VARCHAR(8) '$.CheckInTime',
            Notes NVARCHAR(500) '$.Notes'
        ) j ON a.StudentId = j.StudentId
        WHERE a.SessionId = @SessionId;
        
        -- Update session status to Completed
        UPDATE AttendanceSessions
        SET Status = N'Completed', UpdatedAt = GETDATE()
        WHERE SessionId = @SessionId;
        
        COMMIT TRANSACTION;
        RETURN 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        PRINT ERROR_MESSAGE();
        RETURN 0;
    END CATCH
END
GO

PRINT '✅ Created procedure: usp_MarkAttendance';
GO

-- SP: Lấy thống kê attendance của student
IF OBJECT_ID('usp_GetStudentAttendanceStats', 'P') IS NOT NULL
    DROP PROCEDURE usp_GetStudentAttendanceStats;
GO

CREATE PROCEDURE usp_GetStudentAttendanceStats
    @StudentId NVARCHAR(10),
    @CourseId NVARCHAR(10) = NULL -- NULL = all courses
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.CourseId,
        c.CourseName,
        COUNT(DISTINCT s.SessionId) AS TotalSessions,
        SUM(CASE WHEN a.Status = N'Present' THEN 1 ELSE 0 END) AS PresentCount,
        SUM(CASE WHEN a.Status = N'Absent' THEN 1 ELSE 0 END) AS AbsentCount,
        SUM(CASE WHEN a.Status = N'Late' THEN 1 ELSE 0 END) AS LateCount,
        SUM(CASE WHEN a.Status = N'Excused' THEN 1 ELSE 0 END) AS ExcusedCount,
        CAST(
            CASE 
                WHEN COUNT(DISTINCT s.SessionId) > 0 
                THEN (SUM(CASE WHEN a.Status = N'Present' THEN 1 ELSE 0 END) * 100.0 / COUNT(DISTINCT s.SessionId))
                ELSE 0 
            END AS DECIMAL(5,2)
        ) AS AttendanceRate
    FROM Courses c
    LEFT JOIN AttendanceSessions s ON c.CourseId = s.CourseId
    LEFT JOIN Attendances a ON s.SessionId = a.SessionId AND a.StudentId = @StudentId
    WHERE (@CourseId IS NULL OR c.CourseId = @CourseId)
        AND s.Status = N'Completed'
    GROUP BY c.CourseId, c.CourseName
    ORDER BY c.CourseName;
END
GO

PRINT '✅ Created procedure: usp_GetStudentAttendanceStats';
GO

-- SP: Lấy danh sách students cần cảnh báo vắng nhiều
IF OBJECT_ID('usp_GetAttendanceWarnings', 'P') IS NOT NULL
    DROP PROCEDURE usp_GetAttendanceWarnings;
GO

CREATE PROCEDURE usp_GetAttendanceWarnings
    @CourseId NVARCHAR(10),
    @ThresholdPercent DECIMAL(5,2) = 20.0 -- Vắng > 20%
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        s.StudentId,
        s.FullName,
        s.Email,
        s.Phone,
        COUNT(DISTINCT sess.SessionId) AS TotalSessions,
        SUM(CASE WHEN a.Status = N'Absent' THEN 1 ELSE 0 END) AS AbsentCount,
        CAST(
            SUM(CASE WHEN a.Status = N'Absent' THEN 1 ELSE 0 END) * 100.0 / COUNT(DISTINCT sess.SessionId)
            AS DECIMAL(5,2)
        ) AS AbsentRate
    FROM Students s
    INNER JOIN Attendances a ON s.StudentId = a.StudentId
    INNER JOIN AttendanceSessions sess ON a.SessionId = sess.SessionId
    WHERE sess.CourseId = @CourseId
        AND sess.Status = N'Completed'
    GROUP BY s.StudentId, s.FullName, s.Email, s.Phone
    HAVING 
        SUM(CASE WHEN a.Status = N'Absent' THEN 1 ELSE 0 END) * 100.0 / COUNT(DISTINCT sess.SessionId) > @ThresholdPercent
    ORDER BY AbsentRate DESC;
END
GO

PRINT '✅ Created procedure: usp_GetAttendanceWarnings';
GO

-- =============================================
-- 4. INSERT SAMPLE DATA
-- =============================================

-- Sample attendance sessions for MH001 (Course 1)
DECLARE @SessionId1 INT;
EXEC usp_CreateAttendanceSession 
    @CourseId = 'MH001',
    @TeacherId = 'GV001',
    @SessionDate = '2025-10-01',
    @SessionTime = '08:00',
    @SessionTitle = N'Buổi 1: Giới thiệu môn học',
    @SessionType = N'Lý thuyết',
    @Location = N'Phòng A101',
    @Duration = 90,
    @SessionId = @SessionId1 OUTPUT;

PRINT '✅ Created sample session with ID: ' + CAST(@SessionId1 AS VARCHAR);

-- Mark some attendances
UPDATE Attendances 
SET Status = N'Present', CheckInTime = '08:05', MarkedAt = GETDATE()
WHERE SessionId = @SessionId1 AND StudentId IN (SELECT TOP 3 StudentId FROM Students);

UPDATE Attendances 
SET Status = N'Late', CheckInTime = '08:20', Notes = N'Đi muộn 20 phút', MarkedAt = GETDATE()
WHERE SessionId = @SessionId1 AND StudentId IN (SELECT TOP 1 StudentId FROM Students WHERE StudentId NOT IN (SELECT TOP 3 StudentId FROM Students));

PRINT '✅ Inserted sample attendance data';
GO

-- =============================================
-- 5. CREATE VIEWS FOR REPORTING
-- =============================================

-- View: Overall attendance statistics
IF OBJECT_ID('vw_AttendanceOverview', 'V') IS NOT NULL
    DROP VIEW vw_AttendanceOverview;
GO

CREATE VIEW vw_AttendanceOverview AS
SELECT 
    s.StudentId,
    s.FullName AS StudentName,
    c.CourseId,
    c.CourseName,
    COUNT(DISTINCT sess.SessionId) AS TotalSessions,
    SUM(CASE WHEN a.Status = N'Present' THEN 1 ELSE 0 END) AS PresentCount,
    SUM(CASE WHEN a.Status = N'Absent' THEN 1 ELSE 0 END) AS AbsentCount,
    SUM(CASE WHEN a.Status = N'Late' THEN 1 ELSE 0 END) AS LateCount,
    CAST(
        CASE 
            WHEN COUNT(DISTINCT sess.SessionId) > 0 
            THEN (SUM(CASE WHEN a.Status = N'Present' THEN 1 ELSE 0 END) * 100.0 / COUNT(DISTINCT sess.SessionId))
            ELSE 0 
        END AS DECIMAL(5,2)
    ) AS AttendanceRate
FROM Students s
CROSS JOIN Courses c
LEFT JOIN AttendanceSessions sess ON c.CourseId = sess.CourseId AND sess.Status = N'Completed'
LEFT JOIN Attendances a ON sess.SessionId = a.SessionId AND s.StudentId = a.StudentId
GROUP BY s.StudentId, s.FullName, c.CourseId, c.CourseName;
GO

PRINT '✅ Created view: vw_AttendanceOverview';
GO

PRINT '';
PRINT '==============================================';
PRINT '✅ ATTENDANCE MANAGEMENT SETUP COMPLETED!';
PRINT '==============================================';
PRINT '';
PRINT 'Created objects:';
PRINT '  - Tables: AttendanceSessions, Attendances';
PRINT '  - Indexes: Performance optimization';
PRINT '  - Stored Procedures: 4 procedures';
PRINT '  - Views: vw_AttendanceOverview';
PRINT '  - Sample Data: 1 session with attendance records';
PRINT '';
PRINT 'Next steps:';
PRINT '  1. Run this script in SQL Server';
PRINT '  2. Verify tables created: SELECT * FROM AttendanceSessions';
PRINT '  3. Continue with backend C# models';
PRINT '';
