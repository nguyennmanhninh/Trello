# Student Management System - Knowledge Base for AI Chatbot

## 📚 HỆ THỐNG QUẢN LÝ SINH VIÊN

### TỔNG QUAN
Đây là hệ thống quản lý sinh viên đầy đủ với:
- Backend: ASP.NET Core 8
- Frontend: Angular 17
- Database: SQL Server
- Authentication: Session-based + JWT
- 6 modules chính: Departments, Students, Teachers, Classes, Courses, Grades

---

## 👥 VAI TRÒ NGƯỜI DÙNG

### 1. ADMIN (Quản trị viên)
**Quyền hạn:**
- Quản lý toàn bộ hệ thống
- Thêm/Sửa/Xóa sinh viên, giáo viên, lớp học, môn học, khoa
- Xem tất cả điểm số
- Export Excel/PDF
- Quản lý tài khoản người dùng

**Đăng nhập:**
- Username: `admin`
- Password: `admin123`

### 2. TEACHER (Giáo viên)
**Quyền hạn:**
- Xem/Sửa sinh viên trong lớp mình chủ nhiệm
- Nhập/Sửa điểm cho môn học mình dạy
- Xem thống kê lớp của mình
- Export danh sách sinh viên

**Giới hạn:**
- Không xóa sinh viên có điểm
- Không xem lớp của giáo viên khác
- Không thay đổi cấu trúc khoa

**Đăng nhập mẫu:**
- Username: `gv001`
- Password: `gv001`

### 3. STUDENT (Sinh viên)
**Quyền hạn:**
- Xem thông tin cá nhân
- Xem điểm của mình
- Cập nhật SĐT, địa chỉ

**Giới hạn:**
- Không thay đổi thông tin học vụ
- Không xem điểm sinh viên khác

**Đăng nhập mẫu:**
- Username: `sv001`
- Password: `sv001`

---

## 📖 HƯỚNG DẪN SỬ DỤNG

### QUẢN LÝ SINH VIÊN

**Q: Làm sao để thêm sinh viên mới?**
A: 
1. Đăng nhập với tài khoản Admin hoặc Teacher
2. Click menu "Sinh Viên" bên trái
3. Click nút "➕ Thêm Sinh Viên" (màu xanh)
4. Điền thông tin:
   - Mã sinh viên (bắt buộc, tối đa 10 ký tự)
   - Họ tên đầy đủ (bắt buộc)
   - Ngày sinh (chọn từ calendar)
   - Giới tính (Nam/Nữ)
   - Email, SĐT, Địa chỉ
   - Chọn Lớp từ dropdown
   - Mật khẩu (bắt buộc)
5. Click "💾 Lưu"

**Q: Làm sao để tìm kiếm sinh viên?**
A:
1. Vào trang Sinh Viên
2. Sử dụng ô tìm kiếm ở góc trên
3. Có thể tìm theo: Mã SV, Họ tên, Email
4. Hoặc lọc theo Lớp bằng dropdown

**Q: Làm sao để xóa sinh viên?**
A:
1. Tìm sinh viên cần xóa
2. Click nút "🗑️ Xóa" màu đỏ
3. Xác nhận xóa
4. **Lưu ý:** Không thể xóa sinh viên đã có điểm

**Q: Làm sao để sửa thông tin sinh viên?**
A:
1. Click nút "✏️ Sửa" màu vàng
2. Cập nhật thông tin cần thiết
3. **Lưu ý:** Không thể thay đổi Mã sinh viên
4. Click "💾 Cập Nhật"

**Q: Làm sao để export danh sách sinh viên?**
A:
1. Click nút "📥 Excel" hoặc "📄 PDF" ở toolbar
2. File sẽ tự động download
3. File bao gồm: Mã SV, Họ tên, Giới tính, Ngày sinh, Lớp, Email, SĐT

---

### QUẢN LÝ GIÁO VIÊN

**Q: Làm sao để thêm giáo viên mới?**
A:
1. Đăng nhập Admin
2. Menu "Giáo Viên" → "➕ Thêm Giáo Viên"
3. Điền:
   - Mã giáo viên (tối đa 10 ký tự)
   - Họ tên, Ngày sinh, Giới tính
   - Khoa (chọn từ dropdown)
   - Email, SĐT, Địa chỉ
   - Mật khẩu
4. Click "💾 Lưu"

**Q: Giáo viên có thể xem lớp nào?**
A: Giáo viên chỉ xem được:
- Các lớp mình làm chủ nhiệm
- Sinh viên trong các lớp đó
- Điểm môn học mình dạy

---

### QUẢN LÝ LỚP HỌC

**Q: Làm sao để tạo lớp mới?**
A:
1. Menu "Lớp Học" → "➕ Thêm Lớp"
2. Điền:
   - Mã lớp (tối đa 10 ký tự)
   - Tên lớp
   - Chọn Khoa (dropdown sẽ load danh sách giáo viên của khoa)
   - Chọn Giáo viên chủ nhiệm
3. Click "💾 Lưu"

**Q: Lớp học có thể có bao nhiêu sinh viên?**
A: Không giới hạn số lượng sinh viên trong 1 lớp

**Q: Làm sao để chuyển sinh viên sang lớp khác?**
A:
1. Vào menu "Sinh Viên"
2. Click "✏️ Sửa" sinh viên cần chuyển
3. Thay đổi dropdown "Lớp"
4. Click "💾 Cập Nhật"

---

### QUẢN LÝ MÔN HỌC

**Q: Làm sao để thêm môn học?**
A:
1. Menu "Môn Học" → "➕ Thêm Môn Học"
2. Điền:
   - Mã môn học (tối đa 10 ký tự)
   - Tên môn học
   - Số tín chỉ (1-10)
   - Chọn Khoa
   - Chọn Giáo viên giảng dạy
3. Click "💾 Lưu"

**Q: Tín chỉ môn học từ bao nhiêu đến bao nhiêu?**
A: Từ 1 đến 10 tín chỉ

---

### QUẢN LÝ ĐIỂM

**Q: Làm sao để nhập điểm cho sinh viên?**
A:
1. Menu "Điểm" → "➕ Thêm Điểm"
2. Chọn:
   - Sinh viên (từ dropdown)
   - Môn học (từ dropdown)
3. Nhập điểm (0 - 10, tối đa 2 chữ số thập phân)
4. Hệ thống tự động tính xếp loại:
   - **Xuất sắc** (pink): 9.0 - 10.0
   - **Giỏi** (green): 8.0 - 8.9
   - **Khá** (blue): 7.0 - 7.9
   - **Trung bình** (orange): 5.5 - 6.9
   - **Yếu** (light orange): 4.0 - 5.4
   - **Kém** (red): 0 - 3.9
5. Click "💾 Lưu"

**Q: Làm sao để xem điểm theo lớp?**
A:
1. Vào trang "Điểm"
2. Chọn Lớp từ dropdown "Lọc theo lớp"
3. Danh sách điểm sẽ hiển thị

**Q: Làm sao để xem điểm theo môn học?**
A:
1. Vào trang "Điểm"
2. Chọn Môn học từ dropdown "Lọc theo môn học"
3. Danh sách điểm sẽ hiển thị

**Q: Làm sao để sửa điểm?**
A:
1. Click "✏️ Sửa" điểm cần thay đổi
2. Nhập điểm mới (0 - 10)
3. Xem preview xếp loại mới
4. Click "💾 Cập Nhật"

**Q: Làm sao để xóa điểm?**
A:
1. Click "🗑️ Xóa" điểm cần xóa
2. Xác nhận thông tin: Sinh viên, Môn học, Điểm
3. Click "Xóa"

**Q: Có thể nhập điểm âm không?**
A: Không. Điểm phải từ 0 đến 10

**Q: Điểm có thể có bao nhiêu chữ số thập phân?**
A: Tối đa 2 chữ số thập phân (ví dụ: 8.75)

---

### QUẢN LÝ KHOA

**Q: Làm sao để thêm khoa mới?**
A:
1. Menu "Khoa" → "➕ Thêm Khoa"
2. Điền:
   - Mã khoa (tối đa 10 ký tự)
   - Tên khoa
3. Click "💾 Lưu"

---

## 🔐 BẢO MẬT & TÀI KHOẢN

**Q: Làm sao để đổi mật khẩu?**
A: 
1. Click vào tên người dùng góc phải trên
2. Chọn "Đổi mật khẩu"
3. Nhập mật khẩu cũ và mật khẩu mới
4. Click "Cập nhật"

**Q: Quên mật khẩu thì làm sao?**
A: Liên hệ Admin để reset mật khẩu

**Q: Làm sao để đăng xuất?**
A: Click nút "🚪 Đăng Xuất" ở góc phải trên

---

## 📊 DASHBOARD & THỐNG KÊ

**Q: Dashboard hiển thị những gì?**
A:
- **Admin** thấy:
  - Tổng số sinh viên
  - Tổng số giáo viên
  - Tổng số lớp học
  - Tổng số môn học
  - Biểu đồ sinh viên theo khoa
  - Biểu đồ giáo viên theo khoa
  - Top sinh viên có điểm cao

- **Teacher** thấy:
  - Số sinh viên trong lớp chủ nhiệm
  - Số môn học đang dạy
  - Biểu đồ điểm của sinh viên

- **Student** thấy:
  - Thông tin cá nhân
  - Điểm các môn học
  - GPA trung bình

---

## 🔧 KỸ THUẬT

**Q: Hệ thống sử dụng công nghệ gì?**
A:
- Backend: ASP.NET Core 8 MVC + Web API
- Frontend: Angular 17 (Standalone Components)
- Database: SQL Server
- Authentication: Session + JWT tokens
- UI/UX: Custom CSS với Material Design

**Q: Port chạy ở đâu?**
A:
- Backend: `http://localhost:5298`
- Frontend: `http://localhost:4200`

**Q: Làm sao để chạy project?**
A:
```powershell
# Backend
cd StudentManagementSystem
dotnet restore
dotnet build
dotnet run

# Frontend (terminal khác)
cd ClientApp
npm install
npm start
```

**Q: Làm sao để import database?**
A:
```powershell
# PowerShell
.\ImportSampleData.ps1

# Hoặc thủ công trong SSMS:
# 1. Execute FULL_DATABASE_SETUP.sql
# 2. Execute INSERT_SAMPLE_DATA.sql
```

---

## ❓ CÂU HỎI THƯỜNG GẶP

**Q: Tại sao không thể xóa sinh viên?**
A: Sinh viên đã có điểm không thể xóa. Phải xóa điểm trước.

**Q: Tại sao không thấy tất cả sinh viên?**
A: Nếu bạn là Teacher, bạn chỉ thấy sinh viên trong lớp mình chủ nhiệm.

**Q: Làm sao để thay đổi giáo viên chủ nhiệm?**
A:
1. Vào "Lớp Học"
2. Click "✏️ Sửa" lớp cần đổi
3. Chọn giáo viên mới từ dropdown
4. Click "💾 Cập Nhật"

**Q: Có thể export dữ liệu không?**
A: Có! Mọi danh sách đều có nút Export Excel và PDF.

**Q: Hệ thống có hỗ trợ tiếng Việt không?**
A: Có! Toàn bộ giao diện và dữ liệu đều tiếng Việt.

**Q: Có thể thêm nhiều môn học cho 1 giáo viên không?**
A: Có! 1 giáo viên có thể dạy nhiều môn.

**Q: Có giới hạn số lượng bản ghi không?**
A: Có phân trang. Mỗi trang hiển thị 10-15 bản ghi.

---

## 🐛 TROUBLESHOOTING

**Q: Lỗi "Port 4200 is already in use"?**
A: 
```powershell
# Kill process trên port 4200
Get-Process -Id (Get-NetTCPConnection -LocalPort 4200).OwningProcess | Stop-Process -Force

# Hoặc dùng port khác
ng serve --port 4201
```

**Q: Lỗi "Cannot connect to SQL Server"?**
A: Kiểm tra:
1. SQL Server đang chạy?
2. Connection string đúng trong `appsettings.json`?
3. Database đã được tạo chưa?

**Q: Lỗi "Unauthorized" khi gọi API?**
A: 
1. Đăng nhập lại
2. Xóa cache browser
3. Kiểm tra token trong localStorage

**Q: Trang trắng khi load Angular?**
A:
1. Kiểm tra Console (F12) xem lỗi gì
2. Rebuild frontend: `npm run build`
3. Clear cache: Ctrl + Shift + R

---

## 📞 HỖ TRỢ

**Q: Cần hỗ trợ thêm liên hệ ai?**
A: 
- Email: admin@school.edu.vn
- Hotline: 0123-456-789
- Chat trực tuyến: Widget này! 💬

**Q: Tài liệu kỹ thuật ở đâu?**
A: 
- README.md
- SETUP_GUIDE.md
- API_DOCUMENTATION.md (trong source code)

---

## 🎓 LUỒNG CÔNG VIỆC MẪU

### Đầu năm học:
1. Admin tạo các Khoa mới (nếu có)
2. Admin thêm Giáo viên mới
3. Admin tạo Lớp học → gán Giáo viên chủ nhiệm
4. Admin thêm Môn học → gán Giáo viên giảng dạy
5. Admin/Teacher thêm Sinh viên vào các Lớp

### Trong học kỳ:
1. Teacher nhập điểm cho sinh viên
2. Student đăng nhập xem điểm
3. Admin theo dõi thống kê

### Cuối học kỳ:
1. Export báo cáo điểm
2. Export danh sách sinh viên
3. Thống kê phân loại học lực

---

**Chatbot này sẽ trả lời mọi câu hỏi dựa trên knowledge base trên! 🤖✨**
