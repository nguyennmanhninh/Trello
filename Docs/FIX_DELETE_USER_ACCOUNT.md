# Fix: Xóa Student/Teacher Phải Xóa Luôn User Account

## 🐛 Vấn Đề

Khi xóa Student hoặc Teacher trên frontend, hệ thống chỉ xóa record trong bảng `Students` hoặc `Teachers` nhưng **KHÔNG xóa tài khoản** trong bảng `Users`.

### Triệu Chứng
```
[API Login] Session created - UserId: nvan, Role: Student, EntityId: nvan
[ProfileAPI] Loading Student profile for EntityId: nvan
❌ Student not found with username: nvan
```

User vẫn login được nhưng không tìm thấy Student record → lỗi khi load profile, dashboard, grades, etc.

---

## ✅ Giải Pháp

Cập nhật logic xóa ở **4 nơi** để xóa cả User account:

### 1. API StudentsController (`Controllers/API/StudentsController.cs`)

**Before:**
```csharp
_context.Students.Remove(student);
await _context.SaveChangesAsync();
```

**After:**
```csharp
// ✅ Xóa User account trước
var userAccount = await _context.Users
    .FirstOrDefaultAsync(u => u.EntityId == id && u.Role == "Student");

if (userAccount != null)
{
    _context.Users.Remove(userAccount);
}

// Xóa Student record
_context.Students.Remove(student);
await _context.SaveChangesAsync();
```

### 2. API TeachersController (`Controllers/API/TeachersController.cs`)

Tương tự, thêm logic xóa User account với `Role == "Teacher"`

### 3. StudentService (`Services/StudentService.cs`)

**Before:** Dùng stored procedure `usp_DeleteStudent`

**After:** Dùng EF Core trực tiếp và xóa User account:
```csharp
public async Task<bool> DeleteStudentAsync(string studentId, string userRole)
{
    var student = await _context.Students
        .Include(s => s.Grades)
        .FirstOrDefaultAsync(s => s.StudentId == studentId);

    if (student == null) return false;

    // Delete grades
    if (student.Grades.Any())
    {
        _context.Grades.RemoveRange(student.Grades);
    }

    // ✅ Delete User account
    var userAccount = await _context.Users
        .FirstOrDefaultAsync(u => u.EntityId == studentId && u.Role == "Student");
    
    if (userAccount != null)
    {
        _context.Users.Remove(userAccount);
    }

    // Delete student
    _context.Students.Remove(student);
    await _context.SaveChangesAsync();

    return true;
}
```

### 4. TeacherService (`Services/TeacherService.cs`)

Tương tự StudentService, thêm validation:
- Không cho xóa nếu Teacher đang dạy classes
- Không cho xóa nếu Teacher đang dạy courses
- Xóa User account trước khi xóa Teacher

---

## 🧹 Clean Up Orphaned Accounts

Đã tạo script SQL để dọn dẹp các User accounts đã bị "mồ côi":

**File:** `Database/CLEANUP_ORPHANED_USERS.sql`

### Cách Dùng

1. **Kiểm tra** orphaned accounts:
```sql
-- Student accounts không còn Student record
SELECT u.* FROM Users u
WHERE u.Role = 'Student'
    AND NOT EXISTS (SELECT 1 FROM Students s WHERE s.StudentId = u.EntityId);

-- Teacher accounts không còn Teacher record
SELECT u.* FROM Users u
WHERE u.Role = 'Teacher'
    AND NOT EXISTS (SELECT 1 FROM Teachers t WHERE t.TeacherId = u.EntityId);
```

2. **Xóa** orphaned accounts:
```sql
-- Chạy script CLEANUP_ORPHANED_USERS.sql
```

---

## 📋 Delete Flow Mới

### Khi Xóa Student:
```
1. Load Student với Include(Grades)
2. Xóa tất cả Grades (nếu có)
3. ✅ Tìm và xóa User account (EntityId = StudentId, Role = "Student")
4. Xóa Student record
5. SaveChanges()
```

### Khi Xóa Teacher:
```
1. Validate: Teacher không được dạy classes/courses
2. ✅ Tìm và xóa User account (EntityId = TeacherId, Role = "Teacher")
3. Xóa Teacher record
4. SaveChanges()
```

---

## 🔍 Logging

Các log mới được thêm vào:

```csharp
Console.WriteLine($"[DELETE API] Found user account for student {id} (Username: {userAccount.Username}). Deleting user account...");
Console.WriteLine($"[DELETE API] No user account found for student {id}");
```

---

## ✅ Testing Checklist

### Test Delete Student
- [ ] Xóa student qua Angular frontend
- [ ] Verify Student record đã bị xóa trong DB
- [ ] Verify User account cũng đã bị xóa trong DB
- [ ] Thử login với username đã xóa → phải báo lỗi "Sai username/password"
- [ ] Check orphaned accounts: `SELECT * FROM Users WHERE Role='Student' AND EntityId NOT IN (SELECT StudentId FROM Students)`

### Test Delete Teacher
- [ ] Xóa teacher qua Angular frontend (chỉ Admin được xóa)
- [ ] Verify Teacher record đã bị xóa
- [ ] Verify User account cũng đã bị xóa
- [ ] Thử login với username teacher đã xóa → phải báo lỗi
- [ ] Test không cho xóa teacher đang dạy classes/courses

---

## 🚨 Breaking Changes

### StudentService & TeacherService
- **Before:** Dùng stored procedures (`usp_DeleteStudent`, `usp_DeleteTeacher`)
- **After:** Dùng EF Core trực tiếp

**Impact:** 
- Nếu có stored procedures custom trong DB, chúng sẽ không còn được gọi
- Cần verify logic trong stored procedures và đảm bảo đã được port sang EF Core code

---

## 🔄 Migration Guide

Nếu đang có data production với orphaned accounts:

1. **Backup database** trước khi chạy cleanup
2. **List orphaned accounts** để review:
   ```sql
   SELECT * FROM Users u
   WHERE (u.Role = 'Student' AND NOT EXISTS (SELECT 1 FROM Students s WHERE s.StudentId = u.EntityId))
      OR (u.Role = 'Teacher' AND NOT EXISTS (SELECT 1 FROM Teachers t WHERE t.TeacherId = u.EntityId));
   ```
3. **Run cleanup script**: `CLEANUP_ORPHANED_USERS.sql`
4. **Verify**: Không còn orphaned accounts
5. **Deploy new code** với logic xóa User account

---

## 📊 Database Relationships

```
Users (Authentication)
  ↓ EntityId
  ├─ Students (StudentId) → 1:1 relationship
  └─ Teachers (TeacherId) → 1:1 relationship
```

**Rule:** Khi xóa Student/Teacher → PHẢI xóa User account tương ứng để tránh orphaned records.

---

## 🎯 Benefits

✅ **Data Integrity**: Không còn orphaned User accounts
✅ **Security**: Users bị xóa không thể login
✅ **Consistency**: Xóa ở 1 nơi = xóa toàn bộ data liên quan
✅ **Better UX**: Không còn lỗi "Student not found" sau khi login

---

## 📁 Files Modified

1. `Controllers/API/StudentsController.cs` - Added User deletion in DeleteStudent
2. `Controllers/API/TeachersController.cs` - Added User deletion in DeleteTeacher
3. `Services/StudentService.cs` - Replaced stored proc with EF Core + User deletion
4. `Services/TeacherService.cs` - Replaced stored proc with EF Core + User deletion
5. `Database/CLEANUP_ORPHANED_USERS.sql` - New cleanup script

---

## 🚀 Next Steps

1. Run cleanup script để xóa orphaned accounts hiện có
2. Build và deploy backend mới
3. Test delete flow với từng role
4. Monitor logs để verify User accounts được xóa đúng cách

---

## ⚠️ Important Notes

- **Cascade Delete**: Grades sẽ được tự động xóa khi xóa Student
- **Foreign Key Constraints**: Đảm bảo DB không có FK constraints block việc xóa
- **Soft Delete Alternative**: Nếu cần giữ lại data, consider thêm `IsDeleted` flag thay vì xóa thật
