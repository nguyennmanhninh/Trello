# ?? H??NG D?N DEBUG CHI TI?T

## ?? B?T ??U DEBUG

### B??c 1: Ch?y ?ng d?ng

**Option A: Visual Studio**
1. Nh?n **F5** (ho?c nút Play màu xanh)
2. ?ng d?ng s? m? browser t? ??ng

**Option B: Command Line**
```bash
cd C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
dotnet run
```

??i ??n khi th?y:
```
Now listening on: https://localhost:5001
Application started. Press Ctrl+C to shut down.
```

---

### B??c 2: Truy c?p Debug Dashboard

M? browser và truy c?p:
```
https://localhost:5001/Debug
```

B?n s? th?y trang v?i 6 link test.

---

## ?? CÁC TEST C?N CH?Y

### TEST 1: Database Connection
**URL:** `https://localhost:5001/Debug/TestConnection`

**M?c ?ích:** Ki?m tra k?t n?i database và d? li?u

**K?t qu? mong ??i:**
```
=== DATABASE CONNECTION TEST ===

Test 1: Database Connection
Result: ? SUCCESS

Test 2: Tables Existence
Users: 1 records
Teachers: 3 records
Students: 5 records

Test 3: Admin Account
? Admin found
  Username: admin
  Password: admin123
  Role: Admin

Test 4: Authentication Test
Success: True
Role: Admin
EntityId: 
FullName: Admin

Test 5: Session Test
Set value: TestValue123
Get value: TestValue123
Session works: ? YES
```

**N?u th?y ? FAILED:**
- Database ch?a ???c t?o ? Ch?y SQL script
- Connection string sai ? Check `appsettings.json`
- SQL Server không ch?y ? Kh?i ??ng SQL Server

---

### TEST 2: Database Queries
**URL:** `https://localhost:5001/Debug/TestDatabase`

**M?c ?ích:** Ki?m tra truy v?n database

**K?t qu? mong ??i:**
```
=== DATABASE QUERY TEST ===

Test 1: Query Users table
Found 1 users:
  - admin / admin123 / Admin

Test 2: Query Teachers table
Found 3 teachers:
  - GV001: Nguy?n V?n A / gv001
  - GV002: Tr?n Th? B / gv002
  - GV003: Lê V?n C / gv003

Test 3: Query Students table
Found 5 students:
  - SV001: Ph?m V?n D / sv001
  - SV002: Hoàng Th? E / sv002
  - SV003: V? V?n F / sv003

Test 4: Find admin user
? Admin found with correct credentials
```

**N?u th?y "Found 0":**
- Data ch?a ???c insert ? Ch?y ph?n sample data trong SQL script

---

### TEST 3: Check Current Session
**URL:** `https://localhost:5001/Debug/TestSession`

**M?c ?ích:** Xem session hi?n t?i

**K?t qu?:**
```
=== SESSION DEBUG ===

Current Session Values:
UserId: (null)
UserRole: (null)
UserName: (null)
Username: (null)
Session ID: abc123...
Session IsAvailable: True

Cookies:
  .StudentManagement.Session = xyz789...
```

**L?u ý:**
- N?u ch?a login, t?t c? s? là (null) - ?ây là bình th??ng
- Quan tr?ng: `Session IsAvailable: True`
- Ph?i th?y cookie `.StudentManagement.Session`

---

### TEST 4: Test Login (Admin)
**URL:** `https://localhost:5001/Debug/TestLogin?username=admin&password=admin123`

**M?c ?ích:** Test toàn b? flow login

**K?t qu? mong ??i:**
```
=== LOGIN TEST FOR admin ===

Step 1: Clearing session...
? Session cleared

Step 2: Authenticating 'admin' / 'admin123'...
Success: True
Role: Admin
EntityId: 
FullName: Admin

Step 3: Setting session values...
? Session values set

Step 4: Committing session...
? Session committed

Step 5: Verifying session...
UserId in session: 
UserRole in session: Admin
UserName in session: Admin

??? SESSION WORKING CORRECTLY! ???

You can now try to login normally.
If normal login still fails, check browser cookies.
```

**?ây là test QUAN TR?NG nh?t!**

**N?u th?y:**
- `? Authentication failed!` ? Database không có admin ho?c password sai
- `??? SESSION NOT SAVED CORRECTLY!` ? Session middleware có v?n ??

---

### TEST 5 & 6: Test Login Teacher & Student
T??ng t? Test 4 nh?ng v?i:
- Teacher: `gv001` / `gv001pass`
- Student: `sv001` / `sv001pass`

---

## ?? PHÂN TÍCH K?T QU?

### Scenario 1: T?t c? test PASS ?
**Ngh?a là:** Backend ho?t ??ng t?t, v?n ?? ? browser cookies

**Gi?i pháp:**
1. **Clear cookies:**
   - Chrome: F12 ? Application ? Cookies ? Xóa t?t c?
   - Firefox: F12 ? Storage ? Cookies ? Xóa t?t c?

2. **Th? Incognito mode:**
   - Ctrl+Shift+N (Chrome)
   - Ctrl+Shift+P (Firefox)

3. **Th? browser khác:**
   - Edge, Firefox, Chrome...

4. **Disable browser extensions:**
   - Privacy Badger, uBlock, etc có th? block cookies

5. **Check browser settings:**
   - Allow cookies for localhost
   - Disable "Block third-party cookies"

---

### Scenario 2: Test 1 FAILED (Database Connection)
**Nguyên nhân:** Không k?t n?i ???c database

**Gi?i pháp:**

1. **Ki?m tra SQL Server ?ang ch?y:**
   - M? Services (Win+R ? services.msc)
   - Tìm "SQL Server" ? Status ph?i là "Running"

2. **Test connection trong SSMS:**
   ```
   Server: 202.55.135.42
   Authentication: SQL Server
   Login: sa
   Password: Aa@0967941364
   ```

3. **Test ping:**
   ```cmd
   ping 202.55.135.42
   ```

4. **Check firewall:**
   - Port 1433 ph?i ???c m?
   - T?m th?i disable firewall ?? test

5. **Verify connection string:**
   M? `appsettings.json`:
   ```json
   "DefaultConnection": "Server=202.55.135.42;Database=StudentManagementSystem;User Id=sa;Password=Aa@0967941364;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```

---

### Scenario 3: Test 2 FAILED (No Data)
**Nguyên nhân:** Database t?n t?i nh?ng không có d? li?u

**Gi?i pháp:**
1. M? SSMS
2. Connect to server
3. Ch?y ph?n sample data t? `New Text Document.txt`:
   ```sql
   USE StudentManagementSystem;
   GO
   
   -- Ch?y t? dòng "-- Sample Data" ??n h?t
   EXEC AddDepartment 'CNTT', N'Công ngh? Thông tin';
   -- ... rest of inserts
   ```

---

### Scenario 4: Test 4 FAILED (Authentication Failed)
**Nguyên nhân:** Admin account không t?n t?i ho?c sai password

**Gi?i pháp:**
1. Ch?y query trong SSMS:
   ```sql
   USE StudentManagementSystem;
   SELECT * FROM Users WHERE Username = 'admin';
   ```

2. N?u không có k?t qu?:
   ```sql
   EXEC AddUser 'admin', 'admin123', 'Admin', NULL;
   ```

3. N?u có nh?ng password khác:
   ```sql
   UPDATE Users SET Password = 'admin123' WHERE Username = 'admin';
   ```

---

### Scenario 5: Test 4 - Session Not Saved
**Nguyên nhân:** Session middleware không ho?t ??ng

**Gi?i pháp:**

1. **Check Program.cs** có:
   ```csharp
   builder.Services.AddSession(options =>
   {
       options.IdleTimeout = TimeSpan.FromMinutes(30);
   options.Cookie.HttpOnly = true;
options.Cookie.IsEssential = true;
       options.Cookie.Name = ".StudentManagement.Session";
     options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
   });
   
   // ...
   
   app.UseSession();  // Ph?i có dòng này
   ```

2. **Rebuild:**
   ```bash
   dotnet clean
   dotnet build
   ```

3. **Restart ?ng d?ng**

---

## ?? WORKFLOW DEBUG CHU?N

### Làm theo th? t?:

1. ? **Ch?y Test 1** ? Verify database connection
2. ? **Ch?y Test 2** ? Verify data exists
3. ? **Ch?y Test 3** ? Check session availability
4. ? **Ch?y Test 4** ? Test login flow
5. ? **N?u Test 4 PASS** ? Clear cookies và th? login bình th??ng
6. ? **N?u v?n l?i** ? Th? incognito mode
7. ? **N?u v?n l?i** ? G?i screenshot k?t qu? các test cho tôi

---

## ?? G?I K?T QU? DEBUG

N?u t?t c? test ??u pass nh?ng v?n l?i, g?i cho tôi:

### 1. Screenshot k?t qu? Test 4
Copy toàn b? text t?:
```
https://localhost:5001/Debug/TestLogin?username=admin&password=admin123
```

### 2. Screenshot Browser DevTools
Khi ?ang ? trang login:
- Nh?n F12
- Tab **Application** ? **Cookies** ? `https://localhost:5001`
- Screenshot danh sách cookies

### 3. Screenshot Network Tab
- F12 ? Tab **Network**
- Login
- Tìm request `POST /Account/Login`
- Click vào ? Tab **Headers**
- Screenshot Response Headers (tìm Set-Cookie)

### 4. Console Logs
Copy output t? terminal/Visual Studio Output window

---

## ?? QUICK FIX T?M TH?I

N?u c?n ch?y ngay và debug sau:

### Option 1: Bypass Authorization (CH? ?? TEST)

Comment out trong `DashboardController.cs`:
```csharp
//[AuthorizeRole("Admin", "Teacher", "Student")]  // Comment dòng này
public class DashboardController : Controller
{
    // ...
}
```

Sau ?ó truy c?p tr?c ti?p:
```
https://localhost:5001/Dashboard/Index
```

### Option 2: Set Session th? công

Truy c?p:
```
https://localhost:5001/Debug/TestLogin?username=admin&password=admin123
```

Sau ?ó truy c?p:
```
https://localhost:5001/Dashboard/Index
```

---

## ? CHECKLIST HOÀN CH?NH

- [ ] Ch?y `dotnet build` ? Thành công
- [ ] SQL Server ?ang ch?y
- [ ] Database `StudentManagementSystem` t?n t?i
- [ ] Có data trong b?ng Users, Students, Teachers
- [ ] Test 1 (Connection) ? PASS
- [ ] Test 2 (Database) ? PASS
- [ ] Test 3 (Session) ? IsAvailable = True
- [ ] Test 4 (Login) ? SESSION WORKING CORRECTLY
- [ ] Clear browser cookies
- [ ] Th? login l?i
- [ ] N?u v?n l?i ? Th? incognito mode
- [ ] N?u v?n l?i ? Th? browser khác

---

## ?? LIÊN H? H? TR?

G?i cho tôi:
1. ? K?t qu? Test 1-4 (copy text ho?c screenshot)
2. ? Screenshot Browser DevTools (Cookies + Network)
3. ? Console output khi ch?y app
4. ? Mô t? chính xác l?i b?n g?p

**URL Debug Dashboard:**
```
https://localhost:5001/Debug
```

**Chúc b?n debug thành công! ??**
