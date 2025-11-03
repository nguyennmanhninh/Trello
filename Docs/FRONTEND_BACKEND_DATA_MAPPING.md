# Frontend-Backend Data Mapping Guide

## 📋 Tổng quan

Angular frontend sử dụng **camelCase** convention, trong khi ASP.NET Core backend trả về **PascalCase** JSON. Dự án đã implement mapping layer để convert tự động.

---

## 🔄 Cách hoạt động

### Backend Response Format (PascalCase)

```json
{
  "TeacherClasses": [
    {
      "ClassId": "LOP01",
      "ClassName": "Lớp A1",
      "DepartmentName": "Công nghệ thông tin",
      "StudentCount": 30
    }
  ],
  "TeacherCourses": [
    {
      "CourseId": "MH001",
      "CourseName": "Lập trình C#",
      "Credits": 3,
      "DepartmentName": "Công nghệ thông tin",
      "StudentCount": 45
    }
  ]
}
```

### Frontend TypeScript Interface (camelCase)

```typescript
interface TeacherClass {
  classId: string;
  className: string;
  departmentName: string;
  studentCount: number;
}

interface TeacherCourse {
  courseId: string;
  courseName: string;
  credits: number;
  departmentName: string;
  studentCount: number;
}
```

---

## ✅ Mapping Implementation

### Teacher Dashboard Component

**File**: `ClientApp/src/app/components/dashboard-teacher/dashboard-teacher.component.ts`

```typescript
loadDashboardData(): void {
  this.http.get<any>('/api/dashboard/teacher-stats').subscribe({
    next: (data) => {
      console.log('📊 Teacher dashboard raw data:', data);
      
      // ✅ Mapping với fallback: check cả camelCase và PascalCase
      this.teacherClasses = (data.teacherClasses || data.TeacherClasses || []).map((c: any) => ({
        classId: c.classId || c.ClassId || '',
        className: c.className || c.ClassName || '',
        departmentName: c.departmentName || c.DepartmentName || c.Department?.departmentName || c.Department?.DepartmentName || '',
        studentCount: c.studentCount || c.StudentCount || 0
      }));

      this.teacherCourses = (data.teacherCourses || data.TeacherCourses || []).map((c: any) => ({
        courseId: c.courseId || c.CourseId || '',
        courseName: c.courseName || c.CourseName || '',
        credits: c.credits || c.Credits || 0,
        departmentName: c.departmentName || c.DepartmentName || c.Department?.departmentName || c.Department?.DepartmentName || '',
        studentCount: c.studentCount || c.StudentCount || 0
      }));

      this.totalClasses = this.teacherClasses.length;
      this.totalCourses = this.teacherCourses.length;
      this.totalStudents = this.teacherClasses.reduce((sum, c) => sum + c.studentCount, 0);

      console.log('✅ Mapped data:', {
        classes: this.teacherClasses,
        courses: this.teacherCourses,
        totalClasses: this.totalClasses,
        totalCourses: this.totalCourses,
        totalStudents: this.totalStudents
      });

      this.loading = false;
    },
    error: (err) => {
      console.error('Error loading teacher dashboard:', err);
      this.error = 'Không thể tải dữ liệu dashboard';
      this.loading = false;
    }
  });
}
```

### Student Dashboard Component

**File**: `ClientApp/src/app/components/dashboard-student/dashboard-student.component.ts`

```typescript
loadDashboardData(): void {
  this.http.get<any>('/api/dashboard/student-stats').subscribe({
    next: (data) => {
      console.log('📊 Student dashboard raw data:', data);
      
      // Map student class
      const classData = data.studentClass || data.StudentClass;
      if (classData) {
        this.studentClass = {
          classId: classData.classId || classData.ClassId || '',
          className: classData.className || classData.ClassName || '',
          departmentName: classData.departmentName || classData.DepartmentName || classData.Department?.departmentName || classData.Department?.DepartmentName || ''
        };
      }

      // Map student grades
      this.studentGrades = (data.studentGrades || data.StudentGrades || []).map((g: any) => ({
        courseId: g.courseId || g.CourseId || '',
        courseName: g.courseName || g.CourseName || g.Course?.courseName || g.Course?.CourseName || '',
        score: g.score || g.Score || 0,
        classification: g.classification || g.Classification || '',
        credits: g.credits || g.Credits || g.Course?.credits || g.Course?.Credits || 0
      }));

      this.averageScore = data.averageScore || data.AverageScore || 0;
      this.totalCredits = this.studentGrades.reduce((sum, g) => sum + g.credits, 0);

      console.log('✅ Mapped data:', {
        class: this.studentClass,
        grades: this.studentGrades,
        averageScore: this.averageScore,
        totalCredits: this.totalCredits
      });

      this.loading = false;
      
      // Create chart after data is loaded
      if (this.studentGrades.length > 0) {
        setTimeout(() => this.createGradeChart(), 100);
      }
    }
  });
}
```

---

## 🎯 Mapping Pattern

### Basic Pattern (Single Field)

```typescript
// Backend: { "FieldName": "value" }
// Frontend mapping:
fieldName: data.fieldName || data.FieldName || defaultValue
```

### Nested Object Pattern

```typescript
// Backend: { "Class": { "ClassName": "value" } }
// Frontend mapping:
className: data.class?.className 
        || data.Class?.className 
        || data.class?.ClassName 
        || data.Class?.ClassName 
        || ''
```

### Array Mapping Pattern

```typescript
// Backend: { "Items": [...] }
// Frontend mapping:
const items = (data.items || data.Items || []).map((item: any) => ({
  id: item.id || item.Id || '',
  name: item.name || item.Name || ''
}));
```

---

## 🔍 Debug & Troubleshooting

### 1. Kiểm tra Backend Response

Mở **Browser DevTools** → **Network tab** → Tìm request `teacher-stats` hoặc `student-stats` → Xem **Response**:

```json
{
  "TeacherClasses": [...],  // ← Backend trả về PascalCase
  "TeacherCourses": [...]
}
```

### 2. Kiểm tra Console Logs

Frontend sẽ log ra 2 messages:

```javascript
// Raw data từ backend
📊 Teacher dashboard raw data: {TeacherClasses: [...], TeacherCourses: [...]}

// Data sau khi mapping
✅ Mapped data: {classes: [...], courses: [...], totalClasses: 3, ...}
```

### 3. Common Issues

#### Issue: Dashboard shows empty data

**Nguyên nhân**: Backend trả về field name khác với expected

**Giải pháp**:
1. Check Backend `DashboardController.cs` → endpoint response
2. Check Frontend mapping có đủ fallback cases
3. Add console.log để debug raw data

#### Issue: Property undefined error

**Nguyên nhân**: Backend trả về `null` hoặc field không tồn tại

**Giải pháp**: Thêm null coalescing operator
```typescript
// ❌ Lỗi nếu department null
departmentName: data.Department.DepartmentName

// ✅ Safe với optional chaining
departmentName: data.Department?.DepartmentName || ''
```

---

## 📝 Best Practices

### 1. Always Use Fallback Pattern

```typescript
// ✅ Good: Check cả camelCase và PascalCase
value: data.fieldName || data.FieldName || defaultValue

// ❌ Bad: Chỉ check 1 case
value: data.FieldName
```

### 2. Use Optional Chaining for Nested Objects

```typescript
// ✅ Good: Safe với optional chaining
name: data.department?.name || data.Department?.Name || ''

// ❌ Bad: Throw error nếu null
name: data.department.name
```

### 3. Provide Default Values

```typescript
// ✅ Good: Default value prevents undefined
count: data.count || data.Count || 0
items: data.items || data.Items || []

// ❌ Bad: Có thể undefined
count: data.Count
```

### 4. Add Debug Logs (Development Only)

```typescript
console.log('📊 Raw data:', data);  // Debug raw response
console.log('✅ Mapped data:', mappedData);  // Debug after mapping
```

**Note**: Remove hoặc wrap trong `if (environment.debug)` khi production

---

## 🔧 Adding New Endpoints

Khi thêm endpoint mới, follow pattern này:

### 1. Backend Controller

```csharp
[HttpGet("new-stats")]
public async Task<IActionResult> GetNewStats()
{
    var result = new
    {
        DataField = someData,  // PascalCase
        NestedObject = new { ... }
    };
    
    return Ok(result);
}
```

### 2. Frontend TypeScript Interface

```typescript
interface NewStats {
  dataField: any;  // camelCase
  nestedObject: { ... };
}
```

### 3. Frontend Service/Component

```typescript
this.http.get<any>('/api/dashboard/new-stats').subscribe({
  next: (data) => {
    console.log('📊 Raw data:', data);
    
    // Map with fallback
    const mapped: NewStats = {
      dataField: data.dataField || data.DataField || null,
      nestedObject: data.nestedObject || data.NestedObject || {}
    };
    
    console.log('✅ Mapped:', mapped);
  }
});
```

---

## 📚 References

- **Angular Style Guide**: https://angular.io/guide/styleguide#naming
- **C# Naming Conventions**: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
- **TypeScript Optional Chaining**: https://www.typescriptlang.org/docs/handbook/release-notes/typescript-3-7.html#optional-chaining

---

**Ngày cập nhật**: 2025-01-11  
**Trạng thái**: ✅ Hoạt động tốt với JWT authentication
