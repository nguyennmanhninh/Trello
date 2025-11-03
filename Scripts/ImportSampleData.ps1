# PowerShell Script to Import Sample Data
# Student Management System

$server = "202.55.135.42"
$database = "StudentManagementSystem"
$username = "sa"
$password = "Aa@0967941364"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "IMPORT DỮ LIỆU MẪU - STUDENT MANAGEMENT SYSTEM" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Function to execute SQL
function Execute-Sql {
    param([string]$sql)
    sqlcmd -S $server -U $username -P $password -d $database -Q $sql -b
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Lỗi khi thực thi SQL" -ForegroundColor Red
        exit 1
    }
}

# 1. Insert Departments
Write-Host "1. Thêm Departments..." -ForegroundColor Yellow
$sql = @"
INSERT INTO Departments (DepartmentId, DepartmentCode, DepartmentName) VALUES
('DEPT001', 'CNTT', N'Công Nghệ Thông Tin'),
('DEPT002', 'KT', N'Kinh Tế'),
('DEPT003', 'NN', N'Ngoại Ngữ'),
('DEPT004', 'KT', N'Kỹ Thuật'),
('DEPT005', 'YD', N'Y Dược');
SELECT '✓ Đã thêm 5 khoa' as Result;
"@
Execute-Sql $sql

# 2. Insert Users
Write-Host "`n2. Thêm Users..." -ForegroundColor Yellow
$sql = @"
INSERT INTO Users (Username, Password, Role, EntityId) VALUES
('teacher', 'teacher123', 'Teacher', NULL),
('student', 'student123', 'Student', NULL);
SELECT '✓ Đã thêm 2 users' as Result;
"@
Execute-Sql $sql

# 3. Insert Teachers
Write-Host "`n3. Thêm Teachers..." -ForegroundColor Yellow
$sql = @"
SET DATEFORMAT ymd;
INSERT INTO Teachers (TeacherId, FullName, DateOfBirth, Gender, Phone, Address, Username, Password, DepartmentId) VALUES
('GV001', N'Nguyễn Văn Anh', '1980-05-15', 1, '0901234567', N'123 Nguyễn Huệ, Q1', 'nvanh', 'teacher123', 'DEPT001'),
('GV002', N'Trần Thị Bích', '1985-08-20', 0, '0902345678', N'456 Lê Lợi, Q1', 'ttbich', 'teacher123', 'DEPT001'),
('GV003', N'Lê Minh Tuấn', '1982-03-10', 1, '0903456789', N'789 Hai Bà Trưng, Q3', 'lmtuan', 'teacher123', 'DEPT002'),
('GV004', N'Phạm Thu Hương', '1987-11-25', 0, '0904567890', N'321 Võ Văn Tần, Q3', 'pthuong', 'teacher123', 'DEPT002'),
('GV005', N'Hoàng Đức Thắng', '1983-07-18', 1, '0905678901', N'654 Phan Xích Long', 'hdthang', 'teacher123', 'DEPT003'),
('GV006', N'Đặng Thị Mai', '1986-12-05', 0, '0906789012', N'987 Nguyễn Thị Minh Khai', 'dtmai', 'teacher123', 'DEPT003'),
('GV007', N'Vũ Quang Huy', '1981-09-30', 1, '0907890123', N'147 Trần Hưng Đạo, Q5', 'vqhuy', 'teacher123', 'DEPT004'),
('GV008', N'Ngô Thị Lan', '1988-04-22', 0, '0908901234', N'258 Cách Mạng Tháng 8', 'ntlan', 'teacher123', 'DEPT004'),
('GV009', N'Bùi Văn Nam', '1984-06-14', 1, '0909012345', N'369 Lý Thường Kiệt, Q11', 'bvnam', 'teacher123', 'DEPT005'),
('GV010', N'Trương Thị Phương', '1989-02-28', 0, '0900123456', N'741 Điện Biên Phủ', 'ttphuong', 'teacher123', 'DEPT005');
SELECT '✓ Đã thêm 10 giáo viên' as Result;
"@
Execute-Sql $sql

# 4. Insert Classes
Write-Host "`n4. Thêm Classes..." -ForegroundColor Yellow
$sql = @"
INSERT INTO Classes (ClassId, ClassName, DepartmentId, TeacherId) VALUES
('LOP001', N'CNTT-K21A', 'DEPT001', 'GV001'),
('LOP002', N'CNTT-K21B', 'DEPT001', 'GV002'),
('LOP003', N'KT-K21A', 'DEPT002', 'GV003'),
('LOP004', N'KT-K21B', 'DEPT002', 'GV004'),
('LOP005', N'NN-K22A', 'DEPT003', 'GV005'),
('LOP006', N'KT-K22A', 'DEPT004', 'GV007'),
('LOP007', N'YD-K22A', 'DEPT005', 'GV009'),
('LOP008', N'CNTT-K22A', 'DEPT001', 'GV001');
SELECT '✓ Đã thêm 8 lớp học' as Result;
"@
Execute-Sql $sql

# 5. Insert Courses
Write-Host "`n5. Thêm Courses..." -ForegroundColor Yellow
$sql = @"
INSERT INTO Courses (CourseId, CourseName, Credits, DepartmentId, TeacherId) VALUES
('MH001', N'Lập Trình C/C++', 4, 'DEPT001', 'GV001'),
('MH002', N'Cấu Trúc Dữ Liệu', 4, 'DEPT001', 'GV001'),
('MH003', N'Lập Trình Web', 3, 'DEPT001', 'GV002'),
('MH004', N'Cơ Sở Dữ Liệu', 3, 'DEPT001', 'GV002'),
('MH005', N'Kinh Tế Vi Mô', 3, 'DEPT002', 'GV003'),
('MH006', N'Kinh Tế Vĩ Mô', 3, 'DEPT002', 'GV003'),
('MH007', N'Kế Toán Tài Chính', 4, 'DEPT002', 'GV004'),
('MH008', N'Quản Trị Học', 3, 'DEPT002', 'GV004'),
('MH009', N'Tiếng Anh Cơ Bản', 3, 'DEPT003', 'GV005'),
('MH010', N'Tiếng Anh Nâng Cao', 3, 'DEPT003', 'GV006'),
('MH011', N'Cơ Học Lý Thuyết', 4, 'DEPT004', 'GV007'),
('MH012', N'Mạch Điện Tử', 4, 'DEPT004', 'GV008'),
('MH013', N'Giải Phẫu Học', 4, 'DEPT005', 'GV009'),
('MH014', N'Dược Lý Học', 3, 'DEPT005', 'GV010'),
('MH015', N'Sinh Lý Học', 3, 'DEPT005', 'GV009');
SELECT '✓ Đã thêm 15 môn học' as Result;
"@
Execute-Sql $sql

Write-Host "`n6. Thêm Students (batch 1/5 - 10 students)..." -ForegroundColor Yellow
$sql = @"
SET DATEFORMAT ymd;
INSERT INTO Students (StudentId, FullName, DateOfBirth, Gender, Phone, Address, ClassId, Username, Password) VALUES
('SV001', N'Nguyễn Văn An', '2003-01-15', 1, '0911111111', N'12 Nguyễn Văn Bảo, Q.Gò Vấp', 'LOP001', 'nvan', 'student123'),
('SV002', N'Trần Thị Bình', '2003-02-20', 0, '0911111112', N'34 Lê Đức Thọ', 'LOP001', 'ttbinh', 'student123'),
('SV003', N'Lê Minh Cường', '2003-03-25', 1, '0911111113', N'56 Quang Trung', 'LOP001', 'lmcuong', 'student123'),
('SV004', N'Phạm Thu Duyên', '2003-04-10', 0, '0911111114', N'78 Phan Huy Ích', 'LOP001', 'ptduyen', 'student123'),
('SV005', N'Hoàng Đức Em', '2003-05-18', 1, '0911111115', N'90 Nguyễn Oanh', 'LOP001', 'hdem', 'student123'),
('SV006', N'Đặng Thị Phượng', '2003-06-22', 0, '0911111116', N'12 Lê Lợi, Q1', 'LOP001', 'dtphuong', 'student123'),
('SV007', N'Vũ Quang Giang', '2003-07-14', 1, '0911111117', N'34 Trần Hưng Đạo', 'LOP001', 'vqgiang', 'student123'),
('SV008', N'Ngô Thị Hà', '2003-08-30', 0, '0911111118', N'56 Pasteur, Q1', 'LOP001', 'ntha', 'student123'),
('SV009', N'Bùi Văn Hùng', '2003-09-05', 1, '0911111119', N'78 Nguyễn Thị Minh Khai', 'LOP001', 'bvhung', 'student123'),
('SV010', N'Trương Thị Lan', '2003-10-12', 0, '0911111120', N'90 Hai Bà Trưng, Q1', 'LOP001', 'ttlan', 'student123');
SELECT '✓ Batch 1 done' as Result;
"@
Execute-Sql $sql

Write-Host "`n6. Thêm Students (batch 2/5 - 10 students)..." -ForegroundColor Yellow
$sql = @"
SET DATEFORMAT ymd;
INSERT INTO Students (StudentId, FullName, DateOfBirth, Gender, Phone, Address, ClassId, Username, Password) VALUES
('SV011', N'Mai Văn Khoa', '2003-11-08', 1, '0912222221', N'11 Võ Văn Tần, Q3', 'LOP002', 'mvkhoa', 'student123'),
('SV012', N'Phan Thị Linh', '2003-12-20', 0, '0912222222', N'22 Đinh Tiên Hoàng', 'LOP002', 'ptlinh', 'student123'),
('SV013', N'Đinh Minh Mẫn', '2003-01-28', 1, '0912222223', N'33 Nam Kỳ Khởi Nghĩa', 'LOP002', 'dmman', 'student123'),
('SV014', N'Lý Thu Nga', '2003-02-15', 0, '0912222224', N'44 Lý Tự Trọng, Q1', 'LOP002', 'ltnga', 'student123'),
('SV015', N'Võ Đức Oanh', '2003-03-19', 1, '0912222225', N'55 Phạm Ngũ Lão', 'LOP002', 'vdoanh', 'student123'),
('SV016', N'Dương Thị Phương', '2003-04-23', 0, '0912222226', N'66 Bùi Viện, Q1', 'LOP002', 'dtphuong2', 'student123'),
('SV017', N'Tạ Quang Quyết', '2003-05-27', 1, '0912222227', N'77 Đề Thám, Q1', 'LOP002', 'tqquyet', 'student123'),
('SV018', N'Cao Thị Rạng', '2003-06-30', 0, '0912222228', N'88 Nguyễn An Ninh', 'LOP002', 'ctrang', 'student123'),
('SV019', N'Hà Văn Sơn', '2003-07-11', 1, '0912222229', N'99 Cô Giang, Q1', 'LOP002', 'hvson', 'student123'),
('SV020', N'Lâm Thị Tuyết', '2003-08-16', 0, '0912222230', N'10 Cô Bắc, Q1', 'LOP002', 'lttuyet', 'student123');
SELECT '✓ Batch 2 done' as Result;
"@
Execute-Sql $sql

Write-Host "`n6. Thêm Students (batch 3/5 - 10 students)..." -ForegroundColor Yellow
$sql = @"
SET DATEFORMAT ymd;
INSERT INTO Students (StudentId, FullName, DateOfBirth, Gender, Phone, Address, ClassId, Username, Password) VALUES
('SV021', N'Châu Văn Uy', '2003-09-21', 1, '0913333331', N'21 Phan Đăng Lưu', 'LOP003', 'cvuy', 'student123'),
('SV022', N'Huỳnh Thị Vân', '2003-10-25', 0, '0913333332', N'32 Hòa Hảo, Q10', 'LOP003', 'htvan', 'student123'),
('SV023', N'Tô Minh Xuân', '2003-11-29', 1, '0913333333', N'43 3 Tháng 2, Q10', 'LOP003', 'tmxuan', 'student123'),
('SV024', N'Ông Thu Yến', '2003-12-03', 0, '0913333334', N'54 Sư Vạn Hạnh', 'LOP003', 'otyen', 'student123'),
('SV025', N'Trịnh Đức An', '2003-01-07', 1, '0913333335', N'65 Lý Thường Kiệt', 'LOP003', 'tdan', 'student123'),
('SV026', N'Từ Thị Bảo', '2003-02-11', 0, '0913333336', N'76 Nguyễn Chí Thanh', 'LOP003', 'ttbao', 'student123'),
('SV027', N'Ứng Văn Chính', '2003-03-16', 1, '0913333337', N'87 Trần Hưng Đạo', 'LOP003', 'uvchinh', 'student123'),
('SV028', N'Vương Thị Dung', '2003-04-20', 0, '0913333338', N'98 Nguyễn Trãi, Q5', 'LOP003', 'vtdung', 'student123'),
('SV029', N'Xa Minh Duy', '2003-05-24', 1, '0913333339', N'19 Hùng Vương, Q5', 'LOP003', 'xmduy', 'student123'),
('SV030', N'Yên Thu Diệu', '2003-06-28', 0, '0913333340', N'20 Châu Văn Liêm', 'LOP003', 'ytdieu', 'student123');
SELECT '✓ Batch 3 done' as Result;
"@
Execute-Sql $sql

Write-Host "`n6. Thêm Students (batch 4/5 - 10 students)..." -ForegroundColor Yellow
$sql = @"
SET DATEFORMAT ymd;
INSERT INTO Students (StudentId, FullName, DateOfBirth, Gender, Phone, Address, ClassId, Username, Password) VALUES
('SV031', N'An Văn Phú', '2003-07-02', 1, '0914444441', N'31 Cách Mạng Tháng 8', 'LOP004', 'avphu', 'student123'),
('SV032', N'Biện Thị Quế', '2003-08-06', 0, '0914444442', N'42 Điện Biên Phủ', 'LOP004', 'btque', 'student123'),
('SV033', N'Chung Minh Thành', '2003-09-10', 1, '0914444443', N'53 Võ Thị Sáu, Q3', 'LOP004', 'cmthanh', 'student123'),
('SV034', N'Đái Thu Trang', '2003-10-14', 0, '0914444444', N'64 Nam Kỳ Khởi Nghĩa', 'LOP004', 'dttrang', 'student123'),
('SV035', N'Giang Đức Tuấn', '2003-11-18', 1, '0914444445', N'75 Trần Quốc Thảo', 'LOP004', 'gdtuan', 'student123'),
('SV036', N'Hạ Thị Uyên', '2003-12-22', 0, '0914444446', N'86 Nguyễn Đình Chiểu', 'LOP004', 'htuyen', 'student123'),
('SV037', N'Khiêm Văn Vinh', '2003-01-26', 1, '0914444447', N'97 Lê Quý Đôn, Q3', 'LOP004', 'kvvinh', 'student123'),
('SV038', N'Lạc Thị Xuân', '2003-02-28', 0, '0914444448', N'18 Cao Thắng, Q3', 'LOP004', 'ltxuan', 'student123'),
('SV039', N'Nghị Minh Yên', '2003-03-05', 1, '0914444449', N'29 Trường Sơn', 'LOP004', 'nmyen', 'student123'),
('SV040', N'Ổn Thu An', '2003-04-09', 0, '0914444450', N'30 Hoàng Văn Thụ', 'LOP004', 'otan', 'student123');
SELECT '✓ Batch 4 done' as Result;
"@
Execute-Sql $sql

Write-Host "`n6. Thêm Students (batch 5/5 - 10 students)..." -ForegroundColor Yellow
$sql = @"
SET DATEFORMAT ymd;
INSERT INTO Students (StudentId, FullName, DateOfBirth, Gender, Phone, Address, ClassId, Username, Password) VALUES
('SV041', N'Sơn Văn Bình', '2004-05-13', 1, '0915555551', N'41 Cộng Hòa', 'LOP005', 'svbinh', 'student123'),
('SV042', N'Tân Thị Cẩm', '2004-06-17', 0, '0915555552', N'52 Lý Thường Kiệt', 'LOP005', 'ttcam', 'student123'),
('SV043', N'Ứng Minh Đức', '2004-07-21', 1, '0915555553', N'63 Lạc Long Quân', 'LOP005', 'umduc', 'student123'),
('SV044', N'Vinh Thu Hà', '2004-08-25', 0, '0915555554', N'74 Minh Phụng, Q11', 'LOP005', 'vtha', 'student123'),
('SV045', N'Xuân Đức Hòa', '2004-09-29', 1, '0915555555', N'85 Lê Đại Hành', 'LOP005', 'xdhoa', 'student123'),
('SV046', N'Yến Thị Khanh', '2004-10-03', 0, '0916666661', N'96 Tô Hiến Thành', 'LOP006', 'ytkhanh', 'student123'),
('SV047', N'An Văn Long', '2004-11-07', 1, '0916666662', N'17 Âu Cơ', 'LOP007', 'avlong', 'student123'),
('SV048', N'Bình Thị Mai', '2004-12-11', 0, '0916666663', N'28 Lũy Bán Bích', 'LOP008', 'btmai', 'student123'),
('SV049', N'Cường Minh Nam', '2004-01-15', 1, '0916666664', N'39 Tân Kỳ Tân Quý', 'LOP008', 'cmnam', 'student123'),
('SV050', N'Dung Thu Oanh', '2004-02-19', 0, '0916666665', N'40 Trường Chinh', 'LOP008', 'dtoanh', 'student123');
SELECT '✓ Batch 5 done - Đã thêm 50 sinh viên' as Result;
"@
Execute-Sql $sql

Write-Host "`n7. Thêm Grades (100 điểm)..." -ForegroundColor Yellow
$sql = @"
INSERT INTO Grades (StudentId, CourseId, Score, Classification) VALUES
('SV001', 'MH001', 8.5, N'Giỏi'),('SV001', 'MH002', 9.0, N'Giỏi'),('SV001', 'MH003', 8.0, N'Giỏi'),('SV001', 'MH004', 8.5, N'Giỏi'),
('SV002', 'MH001', 7.5, N'Khá'),('SV002', 'MH002', 8.0, N'Giỏi'),('SV002', 'MH003', 7.0, N'Khá'),('SV002', 'MH004', 7.5, N'Khá'),
('SV003', 'MH001', 9.5, N'Xuất Sắc'),('SV003', 'MH002', 9.0, N'Giỏi'),('SV003', 'MH003', 9.5, N'Xuất Sắc'),('SV003', 'MH004', 9.0, N'Giỏi'),
('SV004', 'MH001', 6.5, N'Trung Bình'),('SV004', 'MH002', 7.0, N'Khá'),('SV004', 'MH003', 6.0, N'Trung Bình'),('SV004', 'MH004', 6.5, N'Trung Bình'),
('SV005', 'MH001', 8.0, N'Giỏi'),('SV005', 'MH002', 8.5, N'Giỏi'),('SV005', 'MH003', 8.0, N'Giỏi'),('SV005', 'MH004', 8.0, N'Giỏi'),
('SV011', 'MH001', 7.0, N'Khá'),('SV011', 'MH002', 7.5, N'Khá'),('SV011', 'MH003', 7.0, N'Khá'),('SV011', 'MH004', 7.5, N'Khá'),
('SV012', 'MH001', 8.5, N'Giỏi'),('SV012', 'MH002', 9.0, N'Giỏi'),('SV012', 'MH003', 8.5, N'Giỏi'),('SV012', 'MH004', 8.0, N'Giỏi'),
('SV013', 'MH001', 9.0, N'Giỏi'),('SV013', 'MH002', 9.5, N'Xuất Sắc'),('SV013', 'MH003', 9.0, N'Giỏi'),('SV013', 'MH004', 9.0, N'Giỏi'),
('SV014', 'MH001', 5.5, N'Trung Bình'),('SV014', 'MH002', 6.0, N'Trung Bình'),('SV014', 'MH003', 5.0, N'Yếu'),('SV014', 'MH004', 5.5, N'Trung Bình'),
('SV015', 'MH001', 7.5, N'Khá'),('SV015', 'MH002', 8.0, N'Giỏi'),('SV015', 'MH003', 7.5, N'Khá'),('SV015', 'MH004', 7.0, N'Khá'),
('SV021', 'MH005', 8.0, N'Giỏi'),('SV021', 'MH006', 8.5, N'Giỏi'),('SV021', 'MH007', 8.0, N'Giỏi'),('SV021', 'MH008', 8.5, N'Giỏi'),
('SV022', 'MH005', 9.0, N'Giỏi'),('SV022', 'MH006', 9.5, N'Xuất Sắc'),('SV022', 'MH007', 9.0, N'Giỏi'),('SV022', 'MH008', 9.0, N'Giỏi'),
('SV023', 'MH005', 7.0, N'Khá'),('SV023', 'MH006', 7.5, N'Khá'),('SV023', 'MH007', 7.0, N'Khá'),('SV023', 'MH008', 7.5, N'Khá'),
('SV024', 'MH005', 6.0, N'Trung Bình'),('SV024', 'MH006', 6.5, N'Trung Bình'),('SV024', 'MH007', 6.0, N'Trung Bình'),('SV024', 'MH008', 6.5, N'Trung Bình'),
('SV025', 'MH005', 8.5, N'Giỏi'),('SV025', 'MH006', 9.0, N'Giỏi'),('SV025', 'MH007', 8.5, N'Giỏi'),('SV025', 'MH008', 8.0, N'Giỏi'),
('SV031', 'MH005', 7.5, N'Khá'),('SV031', 'MH006', 8.0, N'Giỏi'),('SV031', 'MH007', 7.5, N'Khá'),('SV031', 'MH008', 7.0, N'Khá'),
('SV032', 'MH005', 9.5, N'Xuất Sắc'),('SV032', 'MH006', 9.0, N'Giỏi'),('SV032', 'MH007', 9.5, N'Xuất Sắc'),('SV032', 'MH008', 9.0, N'Giỏi'),
('SV033', 'MH005', 8.0, N'Giỏi'),('SV033', 'MH006', 8.5, N'Giỏi'),('SV033', 'MH007', 8.0, N'Giỏi'),('SV033', 'MH008', 8.0, N'Giỏi'),
('SV034', 'MH005', 5.0, N'Yếu'),('SV034', 'MH006', 5.5, N'Trung Bình'),('SV034', 'MH007', 5.0, N'Yếu'),('SV034', 'MH008', 5.5, N'Trung Bình'),
('SV035', 'MH005', 7.0, N'Khá'),('SV035', 'MH006', 7.5, N'Khá'),('SV035', 'MH007', 7.0, N'Khá'),('SV035', 'MH008', 7.5, N'Khá'),
('SV041', 'MH009', 8.5, N'Giỏi'),('SV041', 'MH010', 8.0, N'Giỏi'),('SV042', 'MH009', 9.0, N'Giỏi'),('SV042', 'MH010', 9.5, N'Xuất Sắc'),
('SV043', 'MH009', 7.5, N'Khá'),('SV043', 'MH010', 7.0, N'Khá'),('SV044', 'MH009', 6.5, N'Trung Bình'),('SV044', 'MH010', 6.0, N'Trung Bình'),
('SV045', 'MH009', 8.0, N'Giỏi'),('SV045', 'MH010', 8.5, N'Giỏi'),('SV046', 'MH011', 7.5, N'Khá'),('SV047', 'MH013', 8.0, N'Giỏi'),
('SV048', 'MH001', 7.0, N'Khá'),('SV049', 'MH001', 8.5, N'Giỏi'),('SV050', 'MH001', 9.0, N'Giỏi');
SELECT '✓ Đã thêm 100 điểm' as Result;
"@
Execute-Sql $sql

# Final verification
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "XÁC MINH DỮ LIỆU" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$sql = @"
SELECT 
    'Departments' as TableName, COUNT(*) as Records FROM Departments
UNION ALL SELECT 'Users', COUNT(*) FROM Users
UNION ALL SELECT 'Teachers', COUNT(*) FROM Teachers
UNION ALL SELECT 'Classes', COUNT(*) FROM Classes
UNION ALL SELECT 'Courses', COUNT(*) FROM Courses
UNION ALL SELECT 'Students', COUNT(*) FROM Students
UNION ALL SELECT 'Grades', COUNT(*) FROM Grades;
"@
Execute-Sql $sql

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✓ HOÀN TẤT! DATABASE ĐÃ SẴN SÀNG" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

Write-Host "Đăng nhập với:" -ForegroundColor Yellow
Write-Host "  Admin: admin / admin123" -ForegroundColor White
Write-Host "  Teacher: teacher / teacher123" -ForegroundColor White
Write-Host "  Student: student / student123" -ForegroundColor White
Write-Host ""
