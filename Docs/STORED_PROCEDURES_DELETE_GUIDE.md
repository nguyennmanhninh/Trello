# Deploy Stored Procedures - Delete Operations

## 📋 Overview

Dự án sử dụng **Stored Procedures** để xử lý các operations phức tạp, đặc biệt là DELETE operations.

### Tại Sao Dùng Stored Procedures?

✅ **Performance**: Compiled và cached trên database server
✅ **Security**: Giảm SQL injection risks
✅ **Transaction Control**: Dễ dàng quản lý transactions phức tạp
✅ **Maintainability**: Business logic tập trung ở database layer
✅ **Reusability**: Có thể gọi từ nhiều nơi (API, MVC, Reports, etc.)

---

## 🚀 Deployment Steps

### Step 1: Run Stored Procedure Script

**File:** `Database/STORED_PROCEDURES_DELETE.sql`

**Cách chạy:**

#### Option A: SQL Server Management Studio (SSMS)
1. Mở SSMS
2. Connect to SQL Server instance
3. Click **New Query**
4. Open file `STORED_PROCEDURES_DELETE.sql`
5. Ensure database context is `StudentManagementDB`:
   ```sql
   USE StudentManagementDB;
   GO
   ```
6. Click **Execute** (F5)

#### Option B: Azure Data Studio
1. Mở Azure Data Studio
2. Connect to server
3. Right-click on `StudentManagementDB`
4. Select **New Query**
5. Open file `STORED_PROCEDURES_DELETE.sql`
6. Click **Run** or press F5

#### Option C: Command Line (sqlcmd)
```powershell
sqlcmd -S localhost\SQLEXPRESS -d StudentManagementDB -i "Database\STORED_PROCEDURES_DELETE.sql"
```

### Step 2: Verify Deployment

```sql
-- Check if procedures exist
SELECT 
    OBJECT_NAME(object_id) AS ProcedureName,
    create_date AS CreatedDate,
    modify_date AS LastModified
FROM sys.procedures
WHERE name IN ('usp_DeleteStudent', 'usp_DeleteTeacher')
ORDER BY name;
```

Expected output:
```
ProcedureName          CreatedDate              LastModified
usp_DeleteStudent      2025-11-02 10:30:00     2025-11-02 10:30:00
usp_DeleteTeacher      2025-11-02 10:30:00     2025-11-02 10:30:00
```

---

## 📚 Stored Procedures Documentation

### 1. usp_DeleteStudent

**Purpose:** Xóa sinh viên và tất cả dữ liệu liên quan

**Parameters:**
- `@StudentId NVARCHAR(10)` - Mã sinh viên cần xóa
- `@UserRole NVARCHAR(20)` - Role của user thực hiện (Admin/Teacher)

**Returns:** 
- `1` = Success
- `0` = Failed

**Logic Flow:**
```
1. Check student exists
2. Validate user role (Teacher only delete students in their classes)
3. BEGIN TRANSACTION
4. DELETE FROM Grades WHERE StudentId = @StudentId
5. DELETE FROM Users WHERE EntityId = @StudentId AND Role = 'Student'
6. DELETE FROM Students WHERE StudentId = @StudentId
7. COMMIT TRANSACTION
8. RETURN 1 (success)
```

**Example Usage:**
```sql
DECLARE @Result INT;
EXEC @Result = usp_DeleteStudent 
    @StudentId = 'SV001', 
    @UserRole = 'Admin';

IF @Result = 1
    PRINT 'Student deleted successfully'
ELSE
    PRINT 'Failed to delete student'
```

---

### 2. usp_DeleteTeacher

**Purpose:** Xóa giảng viên và User account (chỉ khi không còn classes/courses)

**Parameters:**
- `@TeacherId NVARCHAR(10)` - Mã giảng viên cần xóa
- `@UserRole NVARCHAR(20)` - Chỉ Admin được phép xóa

**Returns:**
- `1` = Success
- `0` = Failed (role không hợp lệ, teacher có classes/courses)

**Validation Rules:**
- ❌ Teacher có classes → Cannot delete
- ❌ Teacher có courses → Cannot delete
- ❌ UserRole != 'Admin' → Cannot delete
- ✅ Teacher không có classes/courses + UserRole = Admin → Can delete

**Logic Flow:**
```
1. Check teacher exists
2. Validate UserRole = 'Admin'
3. Check teacher NOT assigned to any Classes
4. Check teacher NOT teaching any Courses
5. BEGIN TRANSACTION
6. DELETE FROM Users WHERE EntityId = @TeacherId AND Role = 'Teacher'
7. DELETE FROM Teachers WHERE TeacherId = @TeacherId
8. COMMIT TRANSACTION
9. RETURN 1 (success)
```

**Example Usage:**
```sql
DECLARE @Result INT;
EXEC @Result = usp_DeleteTeacher 
    @TeacherId = 'GV001', 
    @UserRole = 'Admin';

SELECT 
    CASE @Result
        WHEN 1 THEN 'Teacher deleted successfully'
        WHEN 0 THEN 'Failed to delete teacher (check if assigned to classes/courses)'
    END AS Result;
```

---

## 🔧 Application Integration

### C# Service Layer

**StudentService.cs:**
```csharp
public async Task<bool> DeleteStudentAsync(string studentId, string userRole)
{
    var connection = _context.Database.GetDbConnection();
    await _context.Database.OpenConnectionAsync();

    try
    {
        using var command = connection.CreateCommand();
        command.CommandText = "usp_DeleteStudent";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@StudentId", SqlDbType.NVarChar, 10) { Value = studentId });
        command.Parameters.Add(new SqlParameter("@UserRole", SqlDbType.NVarChar, 20) { Value = userRole });

        var returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
        command.Parameters.Add(returnValueParam);

        await command.ExecuteNonQueryAsync();
        return (int)returnValueParam.Value == 1;
    }
    finally
    {
        await _context.Database.CloseConnectionAsync();
    }
}
```

**TeacherService.cs:**
```csharp
public async Task<bool> DeleteTeacherAsync(string teacherId, string userRole)
{
    // Similar implementation with usp_DeleteTeacher
}
```

---

## 🧪 Testing Stored Procedures

### Test Script

```sql
USE StudentManagementDB;
GO

-- =============================================
-- TEST 1: Delete student successfully
-- =============================================
PRINT '=== TEST 1: Delete Student ===';

-- Create test student
INSERT INTO Students (StudentId, FullName, DateOfBirth, Gender, ClassId)
VALUES ('TEST001', 'Test Student', '2000-01-01', 1, NULL);

-- Create test user account
INSERT INTO Users (Username, Password, Email, Role, EntityId, EmailVerified, CreatedAt)
VALUES ('testuser', 'test123', 'test@test.com', 'Student', 'TEST001', 1, GETDATE());

-- Create test grade
INSERT INTO Grades (StudentId, CourseId, Score, Classification)
VALUES ('TEST001', 'MH001', 8.5, N'Giỏi');

-- Execute delete
DECLARE @Result1 INT;
EXEC @Result1 = usp_DeleteStudent @StudentId = 'TEST001', @UserRole = 'Admin';

-- Verify
SELECT 
    @Result1 AS DeleteResult,
    CASE WHEN EXISTS (SELECT 1 FROM Students WHERE StudentId = 'TEST001') THEN 'FAILED' ELSE 'SUCCESS' END AS StudentDeleted,
    CASE WHEN EXISTS (SELECT 1 FROM Users WHERE EntityId = 'TEST001') THEN 'FAILED' ELSE 'SUCCESS' END AS UserDeleted,
    CASE WHEN EXISTS (SELECT 1 FROM Grades WHERE StudentId = 'TEST001') THEN 'FAILED' ELSE 'SUCCESS' END AS GradesDeleted;

GO

-- =============================================
-- TEST 2: Delete teacher with classes (should fail)
-- =============================================
PRINT '=== TEST 2: Delete Teacher with Classes (Should Fail) ===';

DECLARE @Result2 INT;
EXEC @Result2 = usp_DeleteTeacher @TeacherId = 'GV001', @UserRole = 'Admin';

SELECT 
    @Result2 AS DeleteResult,
    CASE @Result2 WHEN 0 THEN 'Expected failure - teacher has classes' ELSE 'Unexpected success!' END AS TestResult;

GO
```

---

## 📊 Monitoring & Logging

### Enable Stored Procedure Logging

Add `PRINT` statements in stored procedures are already included:

```sql
PRINT 'Deleted grades for student: ' + @StudentId;
PRINT 'Deleted user account for student: ' + @StudentId;
PRINT 'Deleted student record: ' + @StudentId;
```

View messages in SSMS:
- **Messages** tab after execution
- Or use `RAISERROR` for real-time logging

---

## 🔒 Security Considerations

### Permissions

Grant execute permissions to application user:

```sql
USE StudentManagementDB;
GO

-- Grant execute on stored procedures
GRANT EXECUTE ON usp_DeleteStudent TO [YourApplicationUser];
GRANT EXECUTE ON usp_DeleteTeacher TO [YourApplicationUser];
GO
```

### Audit Trail (Optional)

Create audit table for delete operations:

```sql
CREATE TABLE DeleteAuditLog (
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    DeletedEntityType NVARCHAR(50),  -- 'Student' or 'Teacher'
    DeletedEntityId NVARCHAR(10),
    DeletedBy NVARCHAR(100),
    DeletedAt DATETIME DEFAULT GETDATE(),
    DeletedData NVARCHAR(MAX)  -- JSON of deleted record
);
```

---

## 🚨 Rollback Procedures (Emergency)

If you need to rollback stored procedures:

```sql
-- Drop procedures
DROP PROCEDURE IF EXISTS usp_DeleteStudent;
DROP PROCEDURE IF EXISTS usp_DeleteTeacher;
GO
```

Then restore old version or modify logic as needed.

---

## ✅ Deployment Checklist

- [ ] Backup database before deployment
- [ ] Run `STORED_PROCEDURES_DELETE.sql`
- [ ] Verify procedures created with query above
- [ ] Test with sample data
- [ ] Grant execute permissions to application user
- [ ] Deploy application code (Services already updated)
- [ ] Test delete operations from application
- [ ] Monitor logs for any errors
- [ ] Run cleanup script for existing orphaned accounts

---

## 📞 Troubleshooting

### Issue: "Could not find stored procedure 'usp_DeleteStudent'"

**Solution:**
1. Verify database context: `SELECT DB_NAME()`
2. Re-run deployment script
3. Check permissions

### Issue: "Return value is always 0"

**Solution:**
1. Check RETURN statement in stored procedure
2. Verify returnValueParam configuration in C# code
3. Check for exceptions in TRY-CATCH block

### Issue: "Foreign key constraint error"

**Solution:**
1. Ensure proper delete order (Grades → Users → Student)
2. Check for other tables referencing Student/Teacher
3. Add CASCADE DELETE or handle in stored procedure

---

## 📚 Additional Resources

- SQL Server Stored Procedures: https://docs.microsoft.com/sql/relational-databases/stored-procedures
- ADO.NET Stored Procedure Parameters: https://docs.microsoft.com/dotnet/api/system.data.sqlclient.sqlcommand
- Transaction Management: https://docs.microsoft.com/sql/t-sql/language-elements/transactions

---

**Last Updated:** 2025-11-02
**Maintained By:** Development Team
