# ?? H??NG D?N CH?Y NHANH - 5 PHÚT

## ? QUICK START

### B??c 1: Chu?n b? (2 phút)

1. **Ki?m tra .NET 8**
```bash
dotnet --version
```
N?u ch?a có, t?i: https://dotnet.microsoft.com/download/dotnet/8.0

2. **Ki?m tra SQL Server ?ang ch?y**
- M? SQL Server Management Studio
- K?t n?i thành công = OK

### B??c 2: T?o Database (1 phút)

1. M? SQL Server Management Studio
2. Copy toàn b? n?i dung file `New Text Document.txt`
3. Paste vào query window
4. Nh?n Execute (F5)
5. Refresh ?? th?y database `StudentManagementSystem`

### B??c 3: C?u hình Connection String (30 giây)

M? file `appsettings.json`, thay ??i n?u c?n:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentManagementSystem;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**L?u ý:**
- `localhost` = máy local
- `.` ho?c `(localdb)\mssqllocaldb` = LocalDB
- `.\SQLEXPRESS` = SQL Express

### B??c 4: Ch?y ?ng D?ng (1 phút)

#### Option A: Visual Studio (Recommended)
1. M? file `StudentManagementSystem.sln`
2. Nh?n **F5**
3. Trình duy?t t? ??ng m?

#### Option B: Command Line
```bash
cd StudentManagementSystem
dotnet restore
dotnet run
```
Sau ?ó m?: https://localhost:5001

### B??c 5: ??ng Nh?p & Test (1 phút)

**Admin:**
- Username: `admin`
- Password: `admin123`

**Teacher:**
- Username: `gv001`
- Password: `gv001pass`

**Student:**
- Username: `sv001`
- Password: `sv001pass`

---

## ? CHECKLIST NHANH

- [ ] .NET 8 ?ã cài
- [ ] SQL Server ?ang ch?y
- [ ] Database ?ã t?o
- [ ] Connection string ?ã c?u hình
- [ ] `dotnet restore` ch?y thành công
- [ ] `dotnet build` không có l?i
- [ ] ?ng d?ng ch?y
- [ ] Login thành công

---

## ?? L?I TH??NG G?P & GI?I PHÁP NHANH

### L?i: "Cannot open database"
**Gi?i pháp:** Ki?m tra connection string, ??m b?o database ?ã ???c t?o

### L?i: "Package restore failed"
**Gi?i pháp:**
```bash
dotnet nuget locals all --clear
dotnet restore
```

### L?i: "Port already in use"
**Gi?i pháp:** Thay ??i port trong `Properties/launchSettings.json`

### L?i: "Session not available"
**Gi?i pháp:** ??m b?o `app.UseSession()` có trong `Program.cs`

---

## ?? TEST NHANH CÁC TÍNH N?NG

### Test Admin (2 phút)
1. Login v?i admin/admin123
2. Vào "Qu?n Lý Sinh Viên"
3. Click "Thêm Sinh Viên"
4. Nh?p thông tin và Save
5. Th? Export Excel
6. Test xong ? Logout

### Test Teacher (2 phút)
1. Login v?i gv001/gv001pass
2. Xem l?p ch? nhi?m
3. Vào "Qu?n Lý ?i?m"
4. Th? nh?p ?i?m m?i
5. Test xong ? Logout

### Test Student (1 phút)
1. Login v?i sv001/sv001pass
2. Click "?i?m C?a Tôi"
3. Xem ?i?m trung bình
4. Test xong ? Logout

---

## ?? TÍNH N?NG CHÍNH

### Admin có th?:
- ? Qu?n lý toàn b? (CRUD t?t c?)
- ? Xem th?ng kê
- ? Export Excel
- ? Tìm ki?m & l?c

### Teacher có th?:
- ? Xem l?p & môn c?a mình
- ? Qu?n lý ?i?m sinh viên
- ? Export ?i?m
- ? Xem danh sách sinh viên

### Student có th?:
- ? Xem ?i?m c?a mình
- ? Xem thông tin cá nhân
- ? Xem ?i?m trung bình
- ? ??i m?t kh?u

---

## ?? SAMPLE DATA TRONG DATABASE

### Tài Kho?n
- **1 Admin**: admin/admin123
- **3 Teachers**: gv001-gv003 / gv00Xpass
- **5 Students**: sv001-sv005 / sv00Xpass

### D? Li?u
- 2 Khoa (CNTT, Kinh T?)
- 2 L?p (L001, L002)
- 4 Môn h?c
- 9 ?i?m ?ã nh?p s?n

---

## ?? C?P NH?T D? ÁN

```bash
# Pull latest changes (n?u có)
git pull

# Restore packages
dotnet restore

# Clean và rebuild
dotnet clean
dotnet build

# Ch?y l?i
dotnet run
```

---

## ?? TÀI LI?U CHI TI?T

Xem thêm:
- **README.md** - Mô t? ??y ?? d? án
- **SETUP_GUIDE.md** - H??ng d?n cài ??t chi ti?t
- **PROJECT_STATUS.md** - Tr?ng thái d? án

---

## ?? TIPS & TRICKS

### Shortcut trong Visual Studio
- `F5` - Run (Debug)
- `Ctrl+F5` - Run (No Debug)
- `Shift+F5` - Stop
- `Ctrl+Shift+B` - Build

### Xem Log
- Visual Studio: Output window (View ? Output)
- Command Line: Hi?n th? tr?c ti?p

### Clear Cache
```bash
dotnet clean
rm -rf bin obj
dotnet restore
```

---

## ? DEMO WORKFLOW

### Workflow 1: Thêm Sinh Viên M?i (Admin)
1. Login ? Dashboard
2. "Qu?n Lý Sinh Viên" ? "Thêm Sinh Viên"
3. Nh?p: Mã SV, Tên, Ngày sinh, Gi?i tính, S?T, ??a ch?, L?p, Username, Password
4. Click "L?u"
5. Th?y thông báo thành công
6. Sinh viên m?i xu?t hi?n trong danh sách

### Workflow 2: Nh?p ?i?m (Teacher)
1. Login ? Dashboard
2. "Qu?n Lý ?i?m" ? "Nh?p ?i?m"
3. Ch?n Sinh Viên và Môn H?c
4. Nh?p ?i?m (0-10)
5. Click "L?u ?i?m"
6. H? th?ng t? ??ng tính X?p Lo?i

### Workflow 3: Xem ?i?m (Student)
1. Login ? Dashboard
2. Click "?i?m C?a Tôi"
3. Xem b?ng ?i?m ??y ??
4. Xem ?i?m trung bình
5. Xem th?ng kê (Xu?t s?c, Gi?i, Khá...)

---

## ?? K?T QU?

Sau 5 phút, b?n s? có:
- ? Database ??y ?? v?i sample data
- ? ?ng d?ng ch?y m??t mà
- ? 3 lo?i tài kho?n ho?t ??ng
- ? T?t c? tính n?ng ?ã test

**Chúc m?ng! D? án c?a b?n ?ã s?n sàng! ??**

---

## ?? H? TR?

N?u g?p v?n ??:
1. ??c l?i t?ng b??c
2. Ki?m tra error messages
3. Xem SETUP_GUIDE.md
4. Check SQL Server connection
5. Verify .NET 8 installed

**Happy Coding! ??**
