# HƯỚNG DẪN TEST FRONTEND - THEO TỪNG ROLE

**Ngày tạo**: 2025-10-24  
**Môi trường**: Angular 17 + ASP.NET Core 8  
**URL**: http://localhost:4200  

---

## 📋 MỤC LỤC

1. [Chuẩn bị Test](#chuẩn-bị-test)
2. [Test Admin Role](#test-admin-role)
3. [Test Teacher Role](#test-teacher-role)
4. [Test Student Role](#test-student-role)
5. [Test Authorization & Security](#test-authorization--security)
6. [Test Export Features](#test-export-features)
7. [Test AI Chatbot](#test-ai-chatbot)
8. [Checklist tổng hợp](#checklist-tổng-hợp)

---

## 🚀 CHUẨN BỊ TEST

### 1. Khởi động Backend + Frontend
```powershell
# Terminal 1: Backend
cd C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
dotnet run
# Expected: Server running on https://localhost:5298

# Terminal 2: Frontend
cd ClientApp
npm start
# Expected: Angular dev server on http://localhost:4200
```

### 2. Tài khoản Test
| Username | Password | Role | EntityId | Lớp chủ nhiệm | Môn dạy |
|----------|----------|------|----------|---------------|---------|
| admin | admin123 | Admin | 1 | N/A | N/A |
| gv001 | gv001 | Teacher | GV001 | CNTT01 | Lập trình C |
| sv001 | sv001 | Student | SV001 | CNTT01 | N/A |

### 3. Công cụ Test
- **Browser**: Chrome/Edge (F12 DevTools)
- **Network Tab**: Kiểm tra API calls
- **Console**: Xem log errors
- **Extensions**: React DevTools (optional)

### 4. Checklist trước khi test
- [ ] Backend running (port 5298)
- [ ] Frontend running (port 4200)
- [ ] Database có sample data
- [ ] Browser clear cache
- [ ] Network tab enabled

---

## 👨‍💼 TEST ADMIN ROLE

### 📝 Test Case 1: Login & Dashboard

#### Steps:
1. Mở http://localhost:4200
2. Nhập username: `admin`, password: `admin123`
3. Click "Đăng nhập"

#### Expected Results:
✅ **Login thành công**:
- Redirect to `/dashboard`
- URL: `http://localhost:4200/dashboard`
- Thấy welcome message: "Chào mừng admin!"

✅ **Dashboard hiển thị**:
- **Cards** (4-6 cards):
  - Tổng sinh viên: 100+ (số)
  - Tổng giáo viên: 20+ (số)
  - Tổng lớp: 10+ (số)
  - Tổng môn học: 30+ (số)
  - Tổng khoa: 5+ (số)
  
- **Sidebar Menu** (visible):
  - Dashboard ✅
  - Sinh viên ✅
  - Giáo viên ✅
  - Lớp học ✅
  - Môn học ✅
  - Điểm ✅
  - Khoa ✅
  - AI Chatbot ✅
  - Logout ✅

- **Charts** (Chart.js):
  - Biểu đồ số sinh viên theo khoa
  - Biểu đồ điểm trung bình (nếu có)

#### Console Check:
```javascript
// F12 Console should show:
🔐 Auth Guard - Checking route: /dashboard
🔐 Auth Guard - User logged in: true
🔐 Auth Guard - User role: Admin
✅ Auth Guard - Access GRANTED
```

#### Network Check:
- `POST /api/auth/login` → 200 OK
- `GET /api/dashboard/statistics` → 200 OK (nếu có)

---

### 📝 Test Case 2: Quản lý Sinh viên (CRUD)

#### 2.1. View Students List

**Steps**:
1. Click sidebar "Sinh viên"
2. URL: `http://localhost:4200/students`

**Expected**:
✅ Danh sách sinh viên hiển thị:
- Table columns: Mã SV, Họ tên, Ngày sinh, Giới tính, Lớp, Khoa, Actions
- Pagination: Previous/Next buttons, page numbers
- Search box: Tìm kiếm theo tên
- Filters: Dropdown lọc theo lớp, khoa
- Buttons: "Thêm sinh viên", "Export Excel", "Export PDF"

**Sample data**:
| Mã SV | Họ tên | Ngày sinh | Giới tính | Lớp | Khoa |
|-------|--------|-----------|-----------|-----|------|
| SV001 | Nguyễn Văn A | 01/01/2000 | Nam | CNTT01 | Công nghệ thông tin |
| SV002 | Trần Thị B | 02/02/2000 | Nữ | CNTT01 | Công nghệ thông tin |

**Network Check**:
- `GET /api/students?pageNumber=1&pageSize=10` → 200 OK
- Response có `PascalCase` fields (StudentId, FullName, etc.)
- Frontend map sang `camelCase` (studentId, fullName, etc.)

---

#### 2.2. Create Student

**Steps**:
1. Click "Thêm sinh viên"
2. Form hiển thị với fields:
   - Mã sinh viên (required)
   - Họ tên (required, max 100)
   - Ngày sinh (date picker)
   - Giới tính (radio: Nam/Nữ)
   - Số điện thoại (optional)
   - Địa chỉ (optional)
   - Lớp (dropdown - tất cả lớp)
   - Username (required, unique)
   - Password (required, min 6)

3. Nhập dữ liệu:
   - Mã SV: `SV999`
   - Họ tên: `Test Student`
   - Ngày sinh: `01/01/2000`
   - Giới tính: `Nam`
   - Lớp: `CNTT01`
   - Username: `sv999`
   - Password: `sv999`

4. Click "Lưu"

**Expected**:
✅ Thành công:
- Success message: "Thêm sinh viên thành công"
- Redirect to `/students` list
- SV999 xuất hiện trong danh sách

❌ Validation errors (nếu invalid):
- Mã SV trống: "Mã sinh viên là bắt buộc"
- Họ tên > 100 ký tự: "Họ tên không quá 100 ký tự"
- Username đã tồn tại: "Username đã được sử dụng"

**Network Check**:
- `POST /api/students` → 201 Created
- Request body: JSON với PascalCase
- Response: Created student object

**Console Check**:
```javascript
// Should NOT see errors like:
❌ TypeError: Cannot read property 'studentId' of undefined
❌ 404 Not Found
```

---

#### 2.3. Edit Student

**Steps**:
1. Click "Sửa" button trên row SV999
2. Form hiển thị với data đã điền sẵn
3. Sửa họ tên: `Test Student Updated`
4. Sửa lớp: `CNTT02` (nếu có)
5. Click "Cập nhật"

**Expected**:
✅ Thành công:
- Success message: "Cập nhật sinh viên thành công"
- Redirect to `/students`
- Họ tên hiển thị "Test Student Updated"

❌ Admin có thể đổi:
- FullName ✅
- DateOfBirth ✅
- Gender ✅
- Phone ✅
- Address ✅
- ClassId ✅ (Admin can change class)

**Network Check**:
- `PUT /api/students/SV999` → 200 OK
- Request: Updated student object

---

#### 2.4. Delete Student

**Steps**:
1. Click "Xóa" button trên row SV999
2. Confirm dialog xuất hiện: "Bạn có chắc muốn xóa sinh viên này?"
3. Click "OK"

**Expected**:
✅ Thành công (nếu không có điểm):
- Success message: "Xóa sinh viên thành công"
- SV999 biến mất khỏi list

❌ Thất bại (nếu có điểm):
- Error message: "Không thể xóa sinh viên vì còn X điểm số"
- SV999 vẫn còn trong list

**Network Check**:
- `DELETE /api/students/SV999` → 200 OK (success)
- `DELETE /api/students/SV001` → 400 Bad Request (có điểm)

---

#### 2.5. Search & Filter

**Steps**:
1. **Search**: Nhập "Nguyễn" vào search box → Enter
   - Expected: Chỉ hiển thị sinh viên có tên chứa "Nguyễn"

2. **Filter by Class**: Chọn "CNTT01" trong dropdown lớp
   - Expected: Chỉ hiển thị sinh viên lớp CNTT01

3. **Filter by Department**: Chọn "Công nghệ thông tin"
   - Expected: Chỉ hiển thị sinh viên khoa CNTT

4. **Clear filters**: Click "Reset" hoặc reload
   - Expected: Hiển thị lại tất cả

**Network Check**:
- `GET /api/students?searchString=Nguyễn` → 200 OK
- `GET /api/students?classId=CNTT01` → 200 OK
- `GET /api/students?departmentId=CNTT` → 200 OK

---

#### 2.6. Export Excel/PDF

**Steps**:
1. Click "Export Excel"
2. File download: `DanhSachSinhVien_20251024_123456.xlsx`
3. Mở file Excel:
   - Columns: Mã SV, Họ tên, Ngày sinh, Giới tính, Lớp, Khoa
   - Data: Tất cả sinh viên (không filter)
   - Vietnamese characters: Hiển thị đúng

4. Click "Export PDF"
5. File download: `DanhSachSinhVien_20251024_123456.pdf`
6. Mở file PDF:
   - Vietnamese font: Hiển thị đúng (không bị lỗi font)
   - Layout: Table format

**Network Check**:
- `GET /api/students/export/excel` → 200 OK, Content-Type: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- `GET /api/students/export/pdf` → 200 OK, Content-Type: `application/pdf`

---

### 📝 Test Case 3: Quản lý Giáo viên

#### 3.1. View Teachers List

**Steps**:
1. Click sidebar "Giáo viên"
2. URL: `http://localhost:4200/teachers`

**Expected**:
✅ Danh sách giáo viên:
- Columns: Mã GV, Họ tên, Ngày sinh, Giới tính, Khoa, SĐT, Actions
- Buttons: "Thêm giáo viên", "Export Excel", "Export PDF"

---

#### 3.2. Create Teacher

**Steps**:
1. Click "Thêm giáo viên"
2. Nhập:
   - Mã GV: `GV999`
   - Họ tên: `Test Teacher`
   - Ngày sinh: `01/01/1980`
   - Giới tính: `Nam`
   - Khoa: `CNTT` (dropdown)
   - Username: `gv999`
   - Password: `gv999`
3. Click "Lưu"

**Expected**:
✅ Success: "Thêm giáo viên thành công"
✅ GV999 trong list

**Network Check**:
- `POST /api/teachers` → 201 Created

---

#### 3.3. Edit Teacher (Admin can change Department)

**Steps**:
1. Click "Sửa" trên GV999
2. Đổi khoa: `KTMT` (Admin có quyền đổi)
3. Click "Cập nhật"

**Expected**:
✅ Success
✅ Khoa hiển thị "KTMT"

**Admin privileges**:
- Can change: DepartmentId ✅
- Can change: Username ✅
- Can change: Password ✅

---

#### 3.4. Delete Teacher

**Steps**:
1. Click "Xóa" trên GV999
2. Confirm

**Expected**:
✅ Thành công (nếu không có lớp/môn học)
❌ Error: "Không thể xóa giáo viên vì còn X lớp và Y môn học"

---

### 📝 Test Case 4: Quản lý Lớp

#### 4.1. View Classes

**Steps**:
1. Click sidebar "Lớp học"
2. URL: `http://localhost:4200/classes`

**Expected**:
✅ Danh sách lớp:
- Columns: Mã lớp, Tên lớp, Khoa, GVCN, Số SV, Actions
- Admin thấy **tất cả lớp** (không filter)

---

#### 4.2. Create Class

**Steps**:
1. Click "Thêm lớp"
2. Nhập:
   - Mã lớp: `TEST01`
   - Tên lớp: `Lớp Test`
   - Khoa: `CNTT`
   - GVCN: `GV001` (dropdown tất cả GV)
3. Click "Lưu"

**Expected**:
✅ Success
✅ TEST01 trong list

---

#### 4.3. Delete Class

**Steps**:
1. Click "Xóa" trên TEST01

**Expected**:
✅ Thành công (nếu không có SV)
❌ Error: "Không thể xóa lớp vì còn X sinh viên"

---

### 📝 Test Case 5: Quản lý Khoa (Admin Exclusive)

#### 5.1. View Departments

**Steps**:
1. Click sidebar "Khoa"
2. URL: `http://localhost:4200/departments`

**Expected**:
✅ Danh sách khoa:
- Columns: Mã khoa, Mã code, Tên khoa, Actions
- Buttons: "Thêm khoa", "Export Excel", "Export PDF"

**Admin exclusive**: Teacher/Student không thấy menu "Khoa"

---

#### 5.2. Create Department

**Steps**:
1. Click "Thêm khoa"
2. Nhập:
   - Mã khoa: `TEST`
   - Mã code: `TS`
   - Tên khoa: `Khoa Test`
3. Click "Lưu"

**Expected**:
✅ Success
✅ TEST trong list

---

#### 5.3. Delete Department

**Steps**:
1. Click "Xóa" trên TEST

**Expected**:
✅ Thành công (nếu không có lớp/GV)
❌ Error: "Không thể xóa khoa vì còn X lớp và Y giáo viên"

---

### 📝 Test Case 6: Quản lý Môn học

#### 6.1. View Courses

**Steps**:
1. Click sidebar "Môn học"
2. URL: `http://localhost:4200/courses`

**Expected**:
✅ Danh sách môn học:
- Columns: Mã môn, Tên môn, Tín chỉ, Khoa, GV giảng dạy, Actions
- Admin thấy **tất cả môn học**

---

#### 6.2. Create Course

**Steps**:
1. Click "Thêm môn học"
2. Nhập:
   - Mã môn: `TEST01`
   - Tên môn: `Môn Test`
   - Tín chỉ: `3` (1-10)
   - Khoa: `CNTT`
   - GV giảng dạy: `GV001` (dropdown tất cả GV)
3. Click "Lưu"

**Expected**:
✅ Success
✅ Validation: Tín chỉ 1-10

---

### 📝 Test Case 7: Quản lý Điểm (View Only)

#### 7.1. View All Grades

**Steps**:
1. Click sidebar "Điểm"
2. URL: `http://localhost:4200/grades`

**Expected**:
✅ Danh sách điểm:
- Columns: Mã SV, Họ tên, Lớp, Môn học, Điểm, Xếp loại
- Admin thấy **tất cả điểm** (không filter)
- Filters: Lọc theo lớp, môn học

**Admin note**: Admin chỉ xem, không nhập điểm (business rule)

---

### 📝 Test Case 8: Quản lý Users (Admin Exclusive)

#### 8.1. View Users

**Steps**:
1. URL: `http://localhost:4200/users` (nếu có route)
2. Hoặc access via backend: `/Users`

**Expected**:
✅ Danh sách User (admin accounts):
- Columns: UserId, Username, Role
- Role = "Admin" cho tất cả

---

### 📝 Test Case 9: Đổi mật khẩu

**Steps**:
1. Click dropdown user menu (top-right)
2. Click "Đổi mật khẩu"
3. Nhập:
   - Mật khẩu hiện tại: `admin123`
   - Mật khẩu mới: `admin456`
   - Xác nhận: `admin456`
4. Click "Đổi mật khẩu"

**Expected**:
✅ Success: "Đổi mật khẩu thành công"
✅ Logout và login lại với `admin456`

---

## 👨‍🏫 TEST TEACHER ROLE

### 📝 Test Case 1: Login & Dashboard

#### Steps:
1. Logout admin
2. Login: `gv001` / `gv001`

#### Expected:
✅ Login thành công
✅ Redirect to `/dashboard`
✅ Dashboard hiển thị:
- **Lớp chủ nhiệm**:
  - CNTT01 - Công nghệ thông tin (30 sinh viên)
  
- **Môn học giảng dạy**:
  - Lập trình C (3 tín chỉ)
  - Cấu trúc dữ liệu (4 tín chỉ)

- **Sidebar Menu** (filtered):
  - Dashboard ✅
  - Sinh viên ✅
  - Lớp học ✅
  - Môn học ✅
  - Điểm ✅
  - AI Chatbot ✅
  - **KHÔNG thấy**: Giáo viên ❌, Khoa ❌

---

### 📝 Test Case 2: Quản lý Sinh viên (Filtered)

#### 2.1. View Students (Lớp mình chủ nhiệm)

**Steps**:
1. Click sidebar "Sinh viên"

**Expected**:
✅ Chỉ thấy sinh viên lớp CNTT01:
- SV001 (CNTT01) ✅
- SV002 (CNTT01) ✅
- SV101 (KTMT01) ❌ KHÔNG THẤY

**Network Check**:
- `GET /api/students?pageNumber=1` → 200 OK
- Response chỉ chứa students có ClassId trong danh sách lớp GV001 chủ nhiệm

**Console Check**:
```javascript
// Backend filtering log:
Teacher GV001 can only see students from classes: CNTT01
Filtered students: 30 (chỉ CNTT01)
```

---

#### 2.2. Create Student (Chỉ lớp mình)

**Steps**:
1. Click "Thêm sinh viên"
2. Dropdown "Lớp":
   - Chỉ thấy: CNTT01 ✅
   - Không thấy: KTMT01 ❌

3. Nhập:
   - Mã SV: `SV998`
   - Lớp: `CNTT01` (only option)
4. Click "Lưu"

**Expected**:
✅ Success: SV998 được thêm vào CNTT01

**Validation Test**:
Nếu teacher hack form và gửi `ClassId: "KTMT01"`:
- ❌ Error: "Bạn chỉ có thể thêm sinh viên vào lớp mình chủ nhiệm"

---

#### 2.3. Edit Student (Chỉ lớp mình)

**Steps**:
1. Click "Sửa" trên SV998
2. Dropdown "Lớp": Chỉ thấy CNTT01
3. Sửa họ tên
4. Click "Cập nhật"

**Expected**:
✅ Success
❌ Teacher KHÔNG thể chuyển sinh viên sang lớp khác (không quản lý)

---

#### 2.4. Delete Student (Chỉ lớp mình)

**Steps**:
1. Click "Xóa" trên SV998

**Expected**:
✅ Success (nếu không có điểm)

**Security Test**:
Nếu teacher hack URL `DELETE /api/students/SV101` (lớp khác):
- ❌ 403 Forbidden hoặc 404 Not Found

---

#### 2.5. Export (Chỉ lớp mình)

**Steps**:
1. Click "Export Excel"
2. Mở file

**Expected**:
✅ Chỉ chứa sinh viên lớp CNTT01
❌ KHÔNG chứa sinh viên lớp KTMT01

---

### 📝 Test Case 3: Quản lý Giáo viên (View Only + Self-Edit)

#### 3.1. View Teachers (Forbidden)

**Steps**:
1. Click sidebar "Giáo viên"

**Expected**:
❌ Menu item "Giáo viên" KHÔNG xuất hiện trong sidebar

**Security Test**:
Nếu teacher access URL `/teachers` trực tiếp:
- ❌ Redirect to `/dashboard` hoặc AccessDenied

---

#### 3.2. Edit Profile (Self Only)

**Steps**:
1. Click dropdown user menu → "Thông tin cá nhân"
2. URL: `/teachers/edit-profile`

**Expected**:
✅ Form hiển thị với fields:
- Họ tên ✅ (editable)
- Ngày sinh ✅ (editable)
- Giới tính ✅ (editable)
- SĐT ✅ (editable)
- Địa chỉ ✅ (editable)
- Khoa ❌ (read-only, disabled)
- Username ❌ (read-only, disabled)

3. Sửa họ tên: "Giáo viên Test"
4. Click "Cập nhật"

**Expected**:
✅ Success: "Cập nhật thông tin thành công"
✅ Session UserName updated: "Giáo viên Test"

**Validation**:
- Teacher KHÔNG thể đổi DepartmentId (field disabled)
- Teacher KHÔNG thể đổi Username (field disabled)

---

### 📝 Test Case 4: Quản lý Lớp (View Only)

#### 4.1. View Classes (Lớp mình)

**Steps**:
1. Click sidebar "Lớp học"

**Expected**:
✅ Chỉ thấy lớp CNTT01 (TeacherId = GV001)
❌ Không thấy KTMT01 (TeacherId = GV002)

**UI Check**:
- Buttons "Thêm lớp" ❌ KHÔNG hiển thị
- Buttons "Sửa", "Xóa" ❌ KHÔNG hiển thị
- Chỉ có button "Chi tiết" ✅

---

#### 4.2. View Class Details

**Steps**:
1. Click "Chi tiết" trên CNTT01

**Expected**:
✅ Hiển thị:
- Tên lớp: CNTT01
- Khoa: Công nghệ thông tin
- GVCN: GV001 - Giáo viên Test
- Danh sách sinh viên (30 students)

**Security Test**:
Nếu teacher access `/classes/details/KTMT01`:
- ❌ 403 Forbidden

---

### 📝 Test Case 5: Quản lý Khoa (Forbidden)

**Steps**:
1. Check sidebar

**Expected**:
❌ Menu item "Khoa" KHÔNG xuất hiện

**Security Test**:
Nếu access `/departments`:
- ❌ Redirect to AccessDenied

---

### 📝 Test Case 6: Quản lý Môn học (View + Create)

#### 6.1. View Courses (Môn mình dạy)

**Steps**:
1. Click sidebar "Môn học"

**Expected**:
✅ Chỉ thấy môn GV001 dạy:
- Lập trình C (TeacherId = GV001) ✅
- Cấu trúc dữ liệu (TeacherId = GV001) ✅
- Toán cao cấp (TeacherId = GV002) ❌ KHÔNG THẤY

---

#### 6.2. Create Course (Self-assign only)

**Steps**:
1. Click "Thêm môn học"
2. Form hiển thị:
   - Dropdown "GV giảng dạy": Chỉ thấy GV001 (tự động select)
3. Nhập:
   - Mã môn: `TEST02`
   - Tên môn: `Lập trình Java`
   - Tín chỉ: `4`
   - GV: `GV001` (cannot change)
4. Click "Lưu"

**Expected**:
✅ Success: Môn mới được tạo với TeacherId = GV001

**Validation**:
Nếu teacher hack form và gửi `TeacherId: "GV002"`:
- ❌ Error: "Bạn chỉ có thể tạo môn học cho chính mình"

---

#### 6.3. Edit/Delete Course (Forbidden)

**Steps**:
1. Check buttons trên row môn học

**Expected**:
❌ Buttons "Sửa", "Xóa" KHÔNG hiển thị (Admin only)

**Security Test**:
Nếu access `/courses/edit/TEST02`:
- ❌ Redirect to AccessDenied

---

### 📝 Test Case 7: Quản lý Điểm (Full CRUD)

#### 7.1. View Grades (Lớp mình)

**Steps**:
1. Click sidebar "Điểm"

**Expected**:
✅ Chỉ thấy điểm sinh viên lớp CNTT01:
- SV001 - Lập trình C - 8.5 ✅
- SV002 - Cấu trúc dữ liệu - 7.0 ✅
- SV101 - Toán cao cấp ❌ KHÔNG THẤY (lớp khác)

---

#### 7.2. Create Grade (Double validation)

**Steps**:
1. Click "Nhập điểm"
2. Dropdown "Sinh viên": Chỉ thấy SV lớp CNTT01
3. Dropdown "Môn học": Chỉ thấy môn GV001 dạy
4. Nhập:
   - Sinh viên: `SV001`
   - Môn học: `Lập trình C`
   - Điểm: `9.5`
5. Click "Lưu"

**Expected**:
✅ Success
✅ Xếp loại tự động: "Xuất sắc" (9.5 → 9-10)

**Validation Test 1** (Sinh viên lớp khác):
Nếu hack form và gửi `StudentId: "SV101"` (lớp KTMT01):
- ❌ Error: "Sinh viên không thuộc lớp bạn chủ nhiệm"

**Validation Test 2** (Môn học GV khác dạy):
Nếu hack form và gửi `CourseId: "TOAN01"` (GV002 dạy):
- ❌ Error: "Bạn chỉ có thể nhập điểm cho môn học mình giảng dạy"

---

#### 7.3. Edit Grade

**Steps**:
1. Click "Sửa" trên grade (SV001, Lập trình C)
2. Đổi điểm: `8.0`
3. Click "Cập nhật"

**Expected**:
✅ Success
✅ Xếp loại cập nhật: "Giỏi" (8.0 → 8-8.99)

---

#### 7.4. Delete Grade

**Steps**:
1. Click "Xóa" trên grade

**Expected**:
✅ Success: Điểm bị xóa

---

#### 7.5. Export Grades (Lớp mình)

**Steps**:
1. Click "Export Excel"

**Expected**:
✅ File chỉ chứa điểm sinh viên lớp CNTT01
❌ KHÔNG chứa điểm lớp KTMT01

---

### 📝 Test Case 8: Dashboard (Thống kê riêng)

**Steps**:
1. Click sidebar "Dashboard"

**Expected**:
✅ Hiển thị:
- **Lớp chủ nhiệm**: CNTT01 (30 sinh viên)
- **Môn học giảng dạy**: 
  - Lập trình C (3 tín chỉ)
  - Cấu trúc dữ liệu (4 tín chỉ)

❌ KHÔNG hiển thị:
- Tổng số sinh viên toàn trường
- Tổng số giáo viên toàn trường
- Thống kê khoa (Admin only)

---

## 👨‍🎓 TEST STUDENT ROLE

### 📝 Test Case 1: Login & Dashboard

#### Steps:
1. Logout teacher
2. Login: `sv001` / `sv001`

#### Expected:
✅ Login thành công
✅ Redirect to `/dashboard`
✅ Dashboard hiển thị:
- **Thông tin lớp**:
  - Lớp: CNTT01
  - Khoa: Công nghệ thông tin
  - GVCN: GV001

- **Bảng điểm cá nhân**:
  - Lập trình C: 8.5 - Giỏi
  - Cấu trúc dữ liệu: 7.0 - Khá
  - Điểm trung bình: 7.75

- **Sidebar Menu** (minimal):
  - Dashboard ✅
  - Thông tin cá nhân ✅
  - Môn học ✅ (catalog view)
  - Điểm ✅ (own grades)
  - AI Chatbot ✅
  - **KHÔNG thấy**: Sinh viên ❌, Giáo viên ❌, Lớp ❌, Khoa ❌

---

### 📝 Test Case 2: Xem thông tin cá nhân

#### 2.1. View Profile

**Steps**:
1. Click sidebar "Thông tin cá nhân"
2. URL: `/students/my-profile`

**Expected**:
✅ Hiển thị:
- Mã SV: SV001
- Họ tên: Nguyễn Văn A
- Ngày sinh: 01/01/2000
- Giới tính: Nam
- Lớp: CNTT01
- Khoa: Công nghệ thông tin
- SĐT: 0123456789
- Địa chỉ: Hà Nội

**UI Check**:
- Hầu hết fields: Read-only (disabled)
- Chỉ editable: Phone, Address

---

#### 2.2. Update Profile (Limited)

**Steps**:
1. Click "Sửa thông tin"
2. Form hiển thị:
   - Họ tên ❌ (disabled)
   - Ngày sinh ❌ (disabled)
   - Giới tính ❌ (disabled)
   - Lớp ❌ (disabled)
   - SĐT ✅ (editable)
   - Địa chỉ ✅ (editable)

3. Sửa:
   - SĐT: `0987654321`
   - Địa chỉ: `TP. HCM`
4. Click "Cập nhật"

**Expected**:
✅ Success: "Cập nhật thông tin thành công"
✅ SĐT, Địa chỉ được cập nhật

**Validation**:
Student KHÔNG thể đổi:
- FullName ❌
- DateOfBirth ❌
- Gender ❌
- ClassId ❌

---

### 📝 Test Case 3: Xem danh sách môn học (Catalog)

**Steps**:
1. Click sidebar "Môn học"

**Expected**:
✅ Hiển thị tất cả môn học (catalog view):
- Lập trình C (3 tín chỉ) - GV001
- Cấu trúc dữ liệu (4 tín chỉ) - GV001
- Toán cao cấp (4 tín chỉ) - GV002
- ...

❌ Buttons "Thêm", "Sửa", "Xóa" KHÔNG hiển thị
✅ Chỉ có button "Chi tiết" để xem mô tả môn

---

### 📝 Test Case 4: Xem điểm cá nhân

#### 4.1. View Grades (Own only)

**Steps**:
1. Click sidebar "Điểm"

**Expected**:
✅ Chỉ thấy điểm của SV001:
- Lập trình C: 8.5 - Giỏi ✅
- Cấu trúc dữ liệu: 7.0 - Khá ✅
- Điểm TB: 7.75

❌ KHÔNG thấy điểm sinh viên khác (SV002, SV003, ...)

❌ Buttons "Nhập điểm", "Sửa", "Xóa" KHÔNG hiển thị

---

#### 4.2. Security Test

**URL Hack Test**:
Nếu student access `/grades?studentId=SV002`:
- ❌ Backend filter: Chỉ trả về điểm SV001
- SV002 grades KHÔNG hiển thị

---

### 📝 Test Case 5: Forbidden Access

#### 5.1. Sinh viên (Forbidden)

**Steps**:
1. Check sidebar

**Expected**:
❌ Menu "Sinh viên" KHÔNG xuất hiện

**Security Test**:
Nếu access `/students`:
- ❌ Redirect to AccessDenied

---

#### 5.2. Giáo viên, Lớp, Khoa (Forbidden)

**Steps**:
1. Check sidebar

**Expected**:
❌ Menu "Giáo viên", "Lớp học", "Khoa" KHÔNG xuất hiện

**Security Test**:
- `/teachers` → AccessDenied
- `/classes` → AccessDenied
- `/departments` → AccessDenied

---

### 📝 Test Case 6: Đổi mật khẩu

**Steps**:
1. Click dropdown user menu → "Đổi mật khẩu"
2. Nhập:
   - Mật khẩu hiện tại: `sv001`
   - Mật khẩu mới: `sv001new`
   - Xác nhận: `sv001new`
3. Click "Đổi mật khẩu"

**Expected**:
✅ Success: "Đổi mật khẩu thành công"
✅ Logout và login lại với `sv001new`

---

## 🔐 TEST AUTHORIZATION & SECURITY

### 📝 Test Case 1: Route Guards

#### 1.1. Unauthorized Access (Not logged in)

**Steps**:
1. Logout
2. Access URL trực tiếp: `http://localhost:4200/students`

**Expected**:
❌ Redirect to `/login`
❌ Query param: `?returnUrl=/students`

**After Login**:
✅ Redirect về `/students` (nếu có quyền)

---

#### 1.2. Insufficient Permission (Logged in as Student)

**Steps**:
1. Login as Student (sv001)
2. Access URL: `http://localhost:4200/teachers`

**Expected**:
❌ Redirect to `/dashboard`
❌ Error message (optional): "Bạn không có quyền truy cập"

**Console Check**:
```javascript
⛔ Auth Guard - Access DENIED - Redirecting to dashboard
```

---

### 📝 Test Case 2: API Authorization

#### 2.1. Teacher trying to access other class students

**Steps**:
1. Login as Teacher (gv001)
2. Open DevTools → Console
3. Run:
```javascript
fetch('http://localhost:5298/api/students/SV101', {
  method: 'GET',
  credentials: 'include'
}).then(r => r.json()).then(console.log)
```

**Expected**:
❌ 404 Not Found hoặc 403 Forbidden
❌ Response: "Không tìm thấy sinh viên" (backend đã filter)

---

#### 2.2. Student trying to create grade

**Steps**:
1. Login as Student (sv001)
2. DevTools Console:
```javascript
fetch('http://localhost:5298/api/grades', {
  method: 'POST',
  headers: {'Content-Type': 'application/json'},
  credentials: 'include',
  body: JSON.stringify({
    StudentId: 'SV001',
    CourseId: 'CNTT01',
    Score: 10
  })
}).then(r => r.text()).then(console.log)
```

**Expected**:
❌ 403 Forbidden
❌ Response: "Access denied" hoặc redirect to login

---

### 📝 Test Case 3: Session Expiration

**Steps**:
1. Login as Admin
2. Wait 30 minutes (or clear session manually)
3. Click any menu item

**Expected**:
❌ Session expired
❌ Redirect to `/login`
❌ Error message: "Phiên làm việc hết hạn, vui lòng đăng nhập lại"

---

### 📝 Test Case 4: Multiple Tabs

**Steps**:
1. Tab 1: Login as Admin
2. Tab 2: Open same app
3. Tab 2: Should be logged in (session shared)
4. Tab 1: Logout
5. Tab 2: Click any menu

**Expected**:
❌ Tab 2 redirect to login (session cleared)

---

## 📊 TEST EXPORT FEATURES

### 📝 Test Case 1: Excel Export

#### 1.1. Students Excel (Admin - All)

**Steps**:
1. Login as Admin
2. Go to `/students`
3. Click "Export Excel"
4. File downloads: `DanhSachSinhVien_20251024_123456.xlsx`

**File Check**:
✅ Open in Excel:
- Header row: Mã SV, Họ tên, Ngày sinh, Giới tính, Lớp, Khoa
- Data: Tất cả sinh viên (100+ rows)
- Vietnamese: Đúng font (Nguyễn, Trần, etc.)
- Formatting: Bold headers, borders

---

#### 1.2. Students Excel (Teacher - Filtered)

**Steps**:
1. Login as Teacher (gv001)
2. Go to `/students`
3. Click "Export Excel"

**File Check**:
✅ Chỉ chứa sinh viên lớp CNTT01 (30 rows)
❌ KHÔNG chứa sinh viên lớp KTMT01

---

### 📝 Test Case 2: PDF Export

#### 2.1. Students PDF

**Steps**:
1. Login as Admin
2. Go to `/students`
3. Click "Export PDF"
4. File downloads: `DanhSachSinhVien_20251024_123456.pdf`

**File Check**:
✅ Open in PDF reader:
- Vietnamese font: Hiển thị đúng (không bị ??????)
- Font fallback: Arial → Times New Roman → Helvetica
- Table: Borders, headers
- Layout: A4 size, proper margins

**Common Issues**:
❌ If Vietnamese shows as `???`: Font not embedded
✅ Expected: "Nguyễn Văn A" hiển thị chính xác

---

#### 2.2. Grades PDF

**Steps**:
1. Go to `/grades`
2. Click "Export PDF"

**File Check**:
✅ Columns: Mã SV, Họ tên, Môn học, Điểm, Xếp loại
✅ Vietnamese: "Xuất sắc", "Giỏi", "Khá", etc.

---

### 📝 Test Case 3: Export with Filters

**Steps**:
1. Go to `/students`
2. Filter: Chọn lớp CNTT01
3. Search: Nhập "Nguyễn"
4. Click "Export Excel"

**Expected**:
✅ File chỉ chứa:
- Sinh viên lớp CNTT01 ✅
- Có tên chứa "Nguyễn" ✅

**Network Check**:
- `GET /api/students/export/excel?classId=CNTT01&searchString=Nguyễn`

---

## 🤖 TEST AI CHATBOT

### 📝 Test Case 1: Basic Chat

**Steps**:
1. Login (any role)
2. Click "AI Chatbot" icon (bottom-right or sidebar)
3. Chatbot window opens

**Expected UI**:
✅ Chat window:
- Header: "AI Trợ lý"
- Message area: Hiển thị messages
- Input box: "Nhập câu hỏi..."
- Send button: Icon hoặc text

---

### 📝 Test Case 2: Ask Question

**Steps**:
1. Nhập: "Có bao nhiêu sinh viên trong lớp CNTT01?"
2. Click "Gửi"

**Expected**:
✅ Typing indicator: "Đang trả lời..." (3 dots animation)
✅ Response appears (5-10 seconds):
- "Lớp CNTT01 có 30 sinh viên."

✅ Follow-up questions (3 suggestions):
- "Danh sách sinh viên lớp CNTT01?"
- "Giáo viên chủ nhiệm lớp CNTT01?"
- "Điểm trung bình lớp CNTT01?"

**Network Check**:
- `POST /api/chat/ask` → 200 OK
- Request: `{ question: "Có bao nhiêu sinh viên...", conversationId: "..." }`
- Response: `{ answer: "...", followUpQuestions: [...] }`

---

### 📝 Test Case 3: Follow-up Click

**Steps**:
1. Click suggestion: "Danh sách sinh viên lớp CNTT01?"

**Expected**:
✅ Question tự động điền vào input
✅ Auto-send (hoặc click Send)
✅ Response: Danh sách SV001, SV002, ...

---

### 📝 Test Case 4: Gemini API Error Handling

#### 4.1. Rate Limit (15 RPM)

**Steps**:
1. Gửi 16 messages liên tục (nhanh)

**Expected** (after 15th message):
❌ Error message: "Đã vượt quá giới hạn yêu cầu (15 requests/phút). Vui lòng thử lại sau."
✅ Chatbot vẫn hoạt động, không crash

---

#### 4.2. Network Error

**Steps**:
1. Tắt Internet
2. Gửi message: "Test"

**Expected**:
❌ Error message: "Lỗi kết nối. Vui lòng kiểm tra Internet."
✅ Chatbot vẫn hiển thị, có thể retry

---

### 📝 Test Case 5: RAG Context

**Steps**:
1. Hỏi: "Sinh viên SV001 học lớp nào?"

**Expected**:
✅ Response: "Sinh viên SV001 học lớp CNTT01, thuộc khoa Công nghệ thông tin."
✅ Context từ database (RAG):
- Query: `SELECT * FROM Students WHERE StudentId = 'SV001'`
- Context được gửi cho Gemini API

**Backend Log Check**:
```
[RAG] Searching database for context...
[RAG] Found: Student SV001 in class CNTT01
[RAG] Sending context to Gemini API...
[RAG] Response received
```

---

### 📝 Test Case 6: Caching

**Steps**:
1. Hỏi: "Có bao nhiêu sinh viên?"
2. Wait for response
3. Hỏi lại: "Có bao nhiêu sinh viên?" (same question)

**Expected**:
✅ Response thứ 2 nhanh hơn (<1 second)
✅ Console log: "Using cached response"

**Cache TTL**: 1 hour

**Network Check**:
- Request 1: `POST /api/chat/ask` → 200 OK (slow, ~5 seconds)
- Request 2: `POST /api/chat/ask` → 200 OK (fast, <1 second, from cache)

---

## ✅ CHECKLIST TỔNG HỢP

### 📋 Admin Role Checklist

- [ ] Login thành công
- [ ] Dashboard hiển thị system stats
- [ ] **Students**: View all, CRUD, Export
- [ ] **Teachers**: View all, CRUD, Export
- [ ] **Classes**: View all, CRUD, Export
- [ ] **Departments**: View all, CRUD, Export (exclusive)
- [ ] **Courses**: View all, CRUD, Export
- [ ] **Grades**: View all, Export (no CRUD)
- [ ] **Users**: View all, CRUD (exclusive)
- [ ] Change password
- [ ] Logout

### 📋 Teacher Role Checklist

- [ ] Login thành công
- [ ] Dashboard hiển thị own classes/courses
- [ ] **Students**: View lớp mình, CRUD lớp mình, Export
- [ ] **Teachers**: KHÔNG truy cập được (menu hidden)
- [ ] **Classes**: View lớp mình, NO CRUD
- [ ] **Departments**: KHÔNG truy cập được
- [ ] **Courses**: View môn mình, Create (self-assign), NO Edit/Delete
- [ ] **Grades**: View lớp mình, Full CRUD (double validation), Export
- [ ] **Users**: KHÔNG truy cập được
- [ ] Edit profile (limited fields)
- [ ] Change password
- [ ] Logout

### 📋 Student Role Checklist

- [ ] Login thành công
- [ ] Dashboard hiển thị own grades/GPA
- [ ] **Students**: KHÔNG truy cập được
- [ ] **Teachers**: KHÔNG truy cập được
- [ ] **Classes**: KHÔNG truy cập được
- [ ] **Departments**: KHÔNG truy cập được
- [ ] **Courses**: View catalog (all courses)
- [ ] **Grades**: View own grades only
- [ ] View profile
- [ ] Update profile (Phone/Address only)
- [ ] Change password
- [ ] Logout

### 📋 Authorization Checklist

- [ ] Route guards hoạt động (unauthorized → login)
- [ ] Role-based menu (Admin/Teacher/Student khác nhau)
- [ ] API authorization (403 khi không có quyền)
- [ ] Session management (shared across tabs)
- [ ] Session expiration (redirect to login)
- [ ] URL hacking blocked (access denied)

### 📋 Export Checklist

- [ ] Excel: Vietnamese font đúng
- [ ] Excel: Data filtered by role
- [ ] PDF: Vietnamese font embedded
- [ ] PDF: Layout đúng (A4, borders)
- [ ] Export with filters works
- [ ] File naming: `Entity_YYYYMMDD_HHMMSS.xlsx/pdf`

### 📋 Chatbot Checklist

- [ ] Chat window opens
- [ ] Send message works
- [ ] Typing indicator shows
- [ ] Response appears (5-10s)
- [ ] Follow-up questions (3 suggestions)
- [ ] RAG context từ database
- [ ] Caching works (1-hour TTL)
- [ ] Error handling (rate limit, network)
- [ ] Gemini API status: 200 OK

---

## 🐛 COMMON ISSUES & FIXES

### Issue 1: Login không redirect
**Symptom**: Click login, không chuyển trang  
**Check**:
- Console: Có errors?
- Network: `POST /api/auth/login` status?
- Response: `{success: true, role: "Admin", ...}`?

**Fix**:
- Clear browser cache
- Check AuthService.login() trong Angular
- Check session được set chưa (backend)

---

### Issue 2: Menu items không hiển thị
**Symptom**: Sidebar trống hoặc thiếu menu  
**Check**:
- Console: `userRole` có giá trị?
- Layout component: `*ngIf="hasRole(['Admin'])"`

**Fix**:
- Verify `AuthService.userRole` getter
- Check `localStorage` hoặc `sessionStorage`

---

### Issue 3: Filter không hoạt động (Teacher)
**Symptom**: Teacher thấy tất cả students (không filter)  
**Check**:
- Network: Response có filter?
- Backend log: Role detection?

**Fix**:
- Check `HttpContext.Session.GetString("UserRole")`
- Verify SQL query có `WHERE` clause

---

### Issue 4: Export file bị lỗi font
**Symptom**: PDF hiển thị `??????` thay vì Vietnamese  
**Check**:
- `ExportService.GetVietnameseFont()`
- Font path: `c:/windows/fonts/arial.ttf`

**Fix**:
- Install Arial font
- Fallback: Times New Roman hoặc Helvetica

---

### Issue 5: Chatbot không trả lời
**Symptom**: Typing indicator mãi, không có response  
**Check**:
- Console: Errors?
- Network: `POST /api/chat/ask` status?
- Response: 503 (Gemini API down)?

**Fix**:
- Check Gemini API key
- Verify model: `gemini-2.0-flash-exp`
- Test script: `.\test_gemini.ps1`

---

### Issue 6: Validation không hoạt động
**Symptom**: Có thể submit form với data invalid  
**Check**:
- Frontend: `validateForm()` được gọi?
- Backend: ModelState.IsValid?

**Fix**:
- Add `[Required]`, `[StringLength]` attributes
- Angular: Add validators trong FormControl

---

## 📊 TEST METRICS

### Performance Targets
| Metric | Target | Acceptable |
|--------|--------|------------|
| Login time | <2s | <5s |
| List load (10 items) | <1s | <3s |
| List load (100 items) | <2s | <5s |
| Export Excel (<100 rows) | <3s | <10s |
| Export PDF (<100 rows) | <5s | <15s |
| Chatbot response | <10s | <30s |
| Page navigation | <500ms | <1s |

### Browser Compatibility
- ✅ Chrome 120+
- ✅ Edge 120+
- ✅ Firefox 120+
- ⚠️ Safari (test manually)

### Responsive Design
- ✅ Desktop: 1920x1080
- ✅ Laptop: 1366x768
- ✅ Tablet: 768x1024
- ⚠️ Mobile: 375x667 (limited support)

---

## 🎯 FINAL CHECKLIST

### Before Production
- [ ] All 3 roles tested (Admin, Teacher, Student)
- [ ] Authorization working (route guards + API)
- [ ] Export features working (Excel + PDF)
- [ ] Vietnamese fonts correct
- [ ] Chatbot working (Gemini API)
- [ ] No console errors
- [ ] No 404/500 errors in Network tab
- [ ] Performance acceptable (<5s page load)
- [ ] Session management working
- [ ] Logout working (clear session)

### Known Limitations
- ⚠️ Password plain text (no hashing) - Academic project only
- ⚠️ Gemini API: 15 RPM limit (free tier)
- ⚠️ Export large datasets (>1000 rows) may timeout
- ⚠️ Mobile UI not fully optimized

---

**Ngày tạo**: 2025-10-24  
**Version**: 1.0  
**Trạng thái**: ✅ READY FOR TESTING
