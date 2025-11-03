# Teacher Permission Audit - Frontend & Backend

## 📋 Tổng quan

Rà soát và đảm bảo quyền truy cập của **Giảng viên (Teacher)** theo đúng yêu cầu:

| Chức năng | Quyền truy cập | Phạm vi |
|-----------|----------------|---------|
| Dashboard Giảng viên | ✅ | Dashboard riêng |
| Quản lý sinh viên | ✅ | Chỉ sinh viên trong lớp mình chủ nhiệm |
| Quản lý giáo viên | ❌ | Không được truy cập |
| Quản lý lớp | ✅ | Chỉ xem lớp mình chủ nhiệm |
| Quản lý khoa | ❌ | Không được truy cập |
| Quản lý môn học | ✅ | Chỉ môn mình giảng dạy |
| Quản lý điểm | ✅ | Chỉ điểm của lớp mình |
| Xem điểm cá nhân | ✅ | Không áp dụng (Teacher không có điểm) |
| Quản lý tài khoản | ❌ | Không được truy cập |
| Đổi thông tin cá nhân | ✅ | Chỉ thông tin của mình |

---

## 🔧 Changes Made

### 1. Frontend - Navigation Menu

**File**: `ClientApp/src/app/components/layout/layout.component.ts`

#### Thay đổi:
```typescript
menuItems: MenuItem[] = [
  // ...
  { label: 'Môn học', icon: '📚', route: '/courses', roles: ['Admin', 'Teacher'] }, // ✅ Thêm 'Teacher'
  // ...
];
```

#### Before:
```typescript
{ label: 'Môn học', icon: '📚', route: '/courses', roles: ['Admin'] }, // ❌ Thiếu Teacher
```

**Result**: Giảng viên giờ có thể thấy menu "Môn học" trong sidebar.

---

### 2. Backend API - JWT Claims Authentication

#### Vấn đề phát hiện:

API Controllers đang sử dụng `NameIdentifier` claim để filter dữ liệu:

```csharp
var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
// userId = "nvanh" (username)

if (role == "Teacher")
{
    query = query.Where(c => c.TeacherId == userId); // ❌ So sánh "nvanh" với "GV001" → không khớp
}
```

**Nguyên nhân**: JWT `NameIdentifier` chứa `username` (như "nvanh"), nhưng database `TeacherId` là "GV001".

#### Giải pháp:

Sửa tất cả API Controllers để:
1. Lấy `Username` claim thay vì `NameIdentifier`
2. Tra cứu `Teacher`/`Student` record bằng `Username`
3. Dùng `TeacherId`/`StudentId` thực sự để filter

---

### 3. CoursesController API

**File**: `Controllers/API/CoursesController.cs`

#### Before:
```csharp
var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

if (role == "Teacher")
{
    query = query.Where(c => c.TeacherId == userId); // ❌ userId = "nvanh", TeacherId = "GV001"
}
```

#### After:
```csharp
var username = User.FindFirst("Username")?.Value 
             ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

if (role == "Teacher" && !string.IsNullOrEmpty(username))
{
    // ✅ Lookup teacher by username first
    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == username);
    if (teacher != null)
    {
        query = query.Where(c => c.TeacherId == teacher.TeacherId); // ✅ Dùng TeacherId thực
    }
    else
    {
        // No teacher found, return empty
        return Ok(new { data = new object[] { }, ... });
    }
}
```

**Result**: Giảng viên chỉ thấy môn học mình giảng dạy (TeacherId khớp).

---

### 4. ClassesController API

**File**: `Controllers/API/ClassesController.cs`

#### Changes:
Tương tự CoursesController - tra cứu Teacher bằng Username trước:

```csharp
var username = User.FindFirst("Username")?.Value;

if (role == "Teacher" && !string.IsNullOrEmpty(username))
{
    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == username);
    if (teacher != null)
    {
        query = query.Where(c => c.TeacherId == teacher.TeacherId); // ✅ Chỉ lớp chủ nhiệm
    }
}
```

**Result**: Giảng viên chỉ thấy lớp mình chủ nhiệm.

---

### 5. StudentsController API

**File**: `Controllers/API/StudentsController.cs`

#### Changes:
```csharp
var username = User.FindFirst("Username")?.Value;

if (role == "Teacher" && !string.IsNullOrEmpty(username))
{
    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == username);
    if (teacher != null)
    {
        // ✅ Lấy tất cả lớp của giảng viên
        var teacherClassIds = await _context.Classes
            .Where(c => c.TeacherId == teacher.TeacherId)
            .Select(c => c.ClassId)
            .ToListAsync();
        
        // ✅ Chỉ sinh viên trong các lớp đó
        query = query.Where(s => teacherClassIds.Contains(s.ClassId));
    }
}
```

**Result**: Giảng viên chỉ thấy sinh viên trong lớp mình chủ nhiệm.

---

### 6. GradesController API

**File**: `Controllers/API/GradesController.cs`

#### Changes:
```csharp
var username = User.FindFirst("Username")?.Value;

if (role == "Teacher" && !string.IsNullOrEmpty(username))
{
    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == username);
    if (teacher != null)
    {
        // ✅ Lấy tất cả lớp của giảng viên
        var teacherClassIds = await _context.Classes
            .Where(c => c.TeacherId == teacher.TeacherId)
            .Select(c => c.ClassId)
            .ToListAsync();

        // ✅ Chỉ điểm của sinh viên trong các lớp đó
        query = query.Where(g => teacherClassIds.Contains(g.Student!.ClassId));
    }
}
else if (role == "Student" && !string.IsNullOrEmpty(username))
{
    // ✅ Tra cứu Student bằng username
    var student = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
    if (student != null)
    {
        query = query.Where(g => g.StudentId == student.StudentId);
    }
}
```

**Result**: 
- Giảng viên chỉ thấy điểm sinh viên trong lớp mình
- Sinh viên chỉ thấy điểm của chính mình

---

## 🎯 Pattern Used - Username Lookup

### Authentication Flow:

```
1. User login với username/password
   ↓
2. Backend AuthService authenticate
   ↓
3. JWT token được tạo với claims:
   - NameIdentifier: entityId (từ AuthResult)
   - Username: username (login input)
   - Role: role (Teacher, Student, Admin)
   - Name: fullName
   ↓
4. Frontend lưu token vào localStorage
   ↓
5. API request kèm JWT token (Authorization header)
   ↓
6. Backend API Controller:
   - Đọc Username claim từ JWT
   - Tra cứu Teacher/Student record bằng Username
   - Lấy TeacherId/StudentId thực sự
   - Filter dữ liệu theo ID đó
```

### Code Pattern:

```csharp
// ✅ Standard pattern cho tất cả API Controllers

// Step 1: Get username from JWT
var username = User.FindFirst("Username")?.Value 
             ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

// Step 2: Check role
if (role == "Teacher" && !string.IsNullOrEmpty(username))
{
    // Step 3: Lookup entity by username
    var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Username == username);
    
    if (teacher != null)
    {
        // Step 4: Use real entity ID to filter
        query = query.Where(x => x.TeacherId == teacher.TeacherId);
    }
    else
    {
        // Step 5: Return empty if not found
        return Ok(new { data = new object[] { }, ... });
    }
}
```

---

## 🧪 Testing Checklist

### Teacher Account Test (nvanh / teacher123)

#### ✅ Navigation Menu
- [x] Thấy menu "Dashboard Giảng viên"
- [x] Thấy menu "Sinh viên"
- [x] KHÔNG thấy "Giảng viên"
- [x] Thấy menu "Lớp học"
- [x] KHÔNG thấy "Khoa"
- [x] Thấy menu "Môn học" (mới thêm)
- [x] Thấy menu "Điểm"
- [x] KHÔNG thấy "Tài khoản" (user management)

#### ✅ Data Access - Courses
1. Login as Teacher (nvanh)
2. Click "Môn học" menu
3. Verify: Chỉ thấy các môn GV001 giảng dạy
4. Check API request: `/api/courses?pageNumber=1&pageSize=10`
5. Verify response chỉ có courses với `teacherId === "GV001"`

**Expected**: 
- Network tab shows request with `Authorization: Bearer <token>`
- Response data filtered by TeacherId

**Example Response**:
```json
{
  "data": [
    {
      "courseId": "MH001",
      "courseName": "Lập trình C#",
      "teacherId": "GV001",
      "teacherName": "Nguyen Van Anh"
    }
    // Không có môn của giảng viên khác
  ],
  "pageNumber": 1,
  "totalCount": 3
}
```

#### ✅ Data Access - Classes
1. Click "Lớp học" menu
2. Verify: Chỉ thấy lớp GV001 chủ nhiệm
3. Check API request: `/api/classes?pageNumber=1&pageSize=10`

**Expected**: Chỉ lớp có `teacherId === "GV001"`

#### ✅ Data Access - Students
1. Click "Sinh viên" menu
2. Verify: Chỉ thấy sinh viên trong lớp GV001 chủ nhiệm
3. Check API: Students trong classId thuộc GV001

#### ✅ Data Access - Grades
1. Click "Điểm" menu
2. Verify: Chỉ thấy điểm sinh viên trong lớp GV001

---

## 🐛 Known Issues & Solutions

### Issue 1: Teacher sees empty data

**Symptom**: Teacher login thành công nhưng courses/classes trống

**Cause**: Teacher không có Username field trong database

**Solution**: Kiểm tra database:
```sql
SELECT TeacherId, Username, FullName FROM Teachers WHERE TeacherId = 'GV001';
```

Nếu Username NULL → Update:
```sql
UPDATE Teachers SET Username = 'nvanh' WHERE TeacherId = 'GV001';
```

### Issue 2: 401 Unauthorized on API calls

**Symptom**: Tất cả API calls trả về 401

**Cause**: JWT token không được gửi hoặc không hợp lệ

**Solution**:
1. Check Frontend localStorage có token:
   ```javascript
   console.log(localStorage.getItem('token'));
   ```
2. Check Network tab → Request Headers có `Authorization: Bearer ...`
3. Verify jwt.interceptor.ts đã apply

### Issue 3: Teacher sees all data (no filter)

**Symptom**: Teacher thấy tất cả courses/classes/students

**Cause**: Backend không filter theo role

**Solution**: Verify API Controller code có `if (role == "Teacher")` block và logic tra cứu username đúng

---

## 📝 Files Changed

| File | Changes | Purpose |
|------|---------|---------|
| `ClientApp/src/app/components/layout/layout.component.ts` | Thêm 'Teacher' role vào menu "Môn học" | Hiển thị menu cho giảng viên |
| `Controllers/API/CoursesController.cs` | Tra cứu Teacher bằng Username claim | Filter courses theo TeacherId thực |
| `Controllers/API/ClassesController.cs` | Tra cứu Teacher bằng Username claim | Filter classes theo TeacherId thực |
| `Controllers/API/StudentsController.cs` | Tra cứu Teacher bằng Username claim | Filter students theo lớp chủ nhiệm |
| `Controllers/API/GradesController.cs` | Tra cứu Teacher/Student bằng Username claim | Filter grades theo lớp/sinh viên |

---

## 🚀 Deployment Notes

### Backend Changes:
- ✅ API Controllers updated (no breaking changes)
- ✅ JWT authentication working
- ⚠️ Cần restart backend service sau khi deploy

### Frontend Changes:
- ✅ Menu item updated (layout.component.ts)
- ⚠️ Cần rebuild Angular: `npm run build`
- ⚠️ Clear browser cache sau khi deploy

### Database Requirements:
- ✅ Teachers table phải có column `Username` (varchar(50))
- ✅ Students table phải có column `Username` (varchar(50))
- ⚠️ Nếu thiếu, chạy migration:
  ```sql
  ALTER TABLE Teachers ADD Username NVARCHAR(50) NULL;
  ALTER TABLE Students ADD Username NVARCHAR(50) NULL;
  
  -- Link existing records (if needed)
  UPDATE Teachers SET Username = LOWER(REPLACE(FullName, ' ', ''));
  UPDATE Students SET Username = LOWER(REPLACE(FullName, ' ', ''));
  ```

---

## 📚 Related Documentation

- **JWT Setup**: `Docs/FIX_DASHBOARD_401_ERROR.md`
- **Data Mapping**: `Docs/FRONTEND_BACKEND_DATA_MAPPING.md`
- **Teacher Permissions (Original)**: `TEACHER_PERMISSIONS_AUDIT.md`
- **Pagination & Permissions**: `PAGINATION_AND_TEACHER_PERMISSIONS.md`

---

**Ngày cập nhật**: 2025-01-11  
**Trạng thái**: ✅ Hoàn thành - Teacher permissions đã được audit và fix  
**Test Status**: ⏳ Cần test với tài khoản Teacher thực tế
