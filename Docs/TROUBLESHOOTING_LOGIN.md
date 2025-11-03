# ?? H??NG D?N X? LÝ L?I: LOGIN THÀNH CÔNG NH?NG V?N ? TRANG CH?

## ?? PHÂN TÍCH V?N ??

Khi b?n login thành công nh?ng v?n b? redirect v? trang login, có 3 nguyên nhân chính:

1. **Session không ???c l?u ?úng cách**
2. **Database connection th?t b?i**
3. **Session cookie không ???c set**

---

## ? GI?I PHÁP ?Ã ÁP D?NG

### 1. C?i thi?n Session Configuration
?ã c?p nh?t `Program.cs`:
- Thêm session cookie name
- C?i thi?n cookie security policy
- ??m b?o middleware order ?úng

### 2. Thêm Session Commit
?ã c?p nh?t `AccountController.cs`:
- Thêm `await HttpContext.Session.CommitAsync()` sau khi set session
- ??m b?o session ???c l?u tr??c khi redirect

---

## ?? B??C KI?M TRA

### B??C 1: Test Database Connection

1. **M? SQL Server Management Studio (SSMS)**

2. **K?t n?i v?i thông tin:**
   - Server: `202.55.135.42`
   - Authentication: SQL Server Authentication
   - Login: `sa`
   - Password: `Aa@0967941364`

3. **Ch?y script ki?m tra:**
   - M? file `TEST_CONNECTION.sql`
   - Execute (F5)

4. **Ki?m tra k?t qu?:**
```
Database StudentManagementSystem exists ?
Admin login test: SUCCESS ?
Teacher login test: SUCCESS ?
Student login test: SUCCESS ?
```

**N?U TH?T B?I:**
- Database ch?a ???c t?o ? Ch?y `New Text Document.txt`
- Connection string sai ? Xem ph?n d??i

---

### B??C 2: Verify Connection String

M? `appsettings.json`, ki?m tra:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=202.55.135.42;Database=StudentManagementSystem;User Id=sa;Password=Aa@0967941364;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Các ?i?m c?n ki?m tra:**
- ? Server: `202.55.135.42`
- ? Database: `StudentManagementSystem`
- ? User Id: `sa`
- ? Password: `Aa@0967941364`
- ? `TrustServerCertificate=True`
- ? `MultipleActiveResultSets=true`

---

### B??C 3: Test Network Connection

1. **Test Ping:**
```cmd
ping 202.55.135.42
```
K?t qu? ph?i: `Reply from 202.55.135.42: bytes=32 time=...`

2. **Test SQL Port:**
```cmd
telnet 202.55.135.42 1433
```
Ho?c dùng PowerShell:
```powershell
Test-NetConnection -ComputerName 202.55.135.42 -Port 1433
```

**N?u th?t b?i:**
- Firewall ch?n port 1433
- SQL Server không cho phép remote connections
- Liên h? admin server

---

### B??C 4: Clean & Rebuild

1. **Stop ?ng d?ng** (n?u ?ang ch?y)

2. **Clean:**
```bash
cd C:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem
dotnet clean
```

3. **Delete cache:**
```bash
rmdir /s /q bin
rmdir /s /q obj
```

4. **Restore packages:**
```bash
dotnet restore
```

5. **Build:**
```bash
dotnet build
```

6. **Run:**
```bash
dotnet run
```

---

### B??C 5: Test Login v?i Browser Developer Tools

1. **M? ?ng d?ng**: https://localhost:5001

2. **M? Developer Tools** (F12)

3. **Vào tab "Network"**

4. **Login v?i admin/admin123**

5. **Ki?m tra:**

**Request POST `/Account/Login`:**
- Status Code: ph?i là `302 Found` (redirect)
- Response Headers: ph?i có `Location: /Dashboard`
- Set-Cookie: ph?i có `.StudentManagement.Session`

**Request GET `/Dashboard/Index`:**
- Status Code: n?u `200 OK` = thành công
- Status Code: n?u `302 Found` redirect v? `/Account/Login` = l?i session

6. **Vào tab "Application" ? Cookies**
- Ph?i th?y cookie: `.StudentManagement.Session`
- Value: m?t chu?i dài
- HttpOnly: checked
- SameSite: Lax

---

### B??C 6: Check Console Logs

**Trong Visual Studio:**
1. View ? Output
2. Select "Debug" ho?c "Web Server"
3. Xem có error messages không

**Trong Command Line:**
```bash
dotnet run --verbosity detailed
```

**Tìm các dòng:**
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (Xms) [Parameters=[@__username_0='?' (Size = 4000), ...
```

**N?u không th?y ? Database connection failed**

---

### B??C 7: Test v?i Postman ho?c cURL

**Test Login API:**

```bash
curl -X POST https://localhost:5001/Account/Login ^
  -H "Content-Type: application/x-www-form-urlencoded" ^
  -d "Username=admin&Password=admin123" ^
  -c cookies.txt ^
  -L
```

**Test Dashboard (v?i cookie):**
```bash
curl https://localhost:5001/Dashboard/Index ^
  -b cookies.txt ^
  -L
```

**K?t qu? mong ??i:**
- L?n 1: Th?y HTML c?a Dashboard
- L?n 2: Không b? redirect v? Login

---

## ?? GI?I PHÁP NHANH (QUICK FIX)

### Option 1: Thêm Logging ?? Debug

Thêm vào `AccountController.cs` trong method `Login`:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var result = await _authService.AuthenticateAsync(model.Username, model.Password);

    if (result.Success)
    {
        // DEBUG: Log ?? ki?m tra
     Console.WriteLine($"Login Success: {result.FullName}, Role: {result.Role}, ID: {result.EntityId}");
        
        HttpContext.Session.SetString("UserId", result.EntityId);
        HttpContext.Session.SetString("UserRole", result.Role);
        HttpContext.Session.SetString("UserName", result.FullName);
        HttpContext.Session.SetString("Username", model.Username);

        await HttpContext.Session.CommitAsync();

        // DEBUG: Verify session was set
        var savedUserId = HttpContext.Session.GetString("UserId");
        Console.WriteLine($"Session UserId after set: {savedUserId}");

        TempData["SuccessMessage"] = $"Chào m?ng {result.FullName}!";
        return RedirectToAction("Index", "Dashboard");
    }

    ModelState.AddModelError("", "Tên ??ng nh?p ho?c m?t kh?u không ?úng");
    return View(model);
}
```

Ch?y l?i và xem console output.

---

### Option 2: T?o Test Controller

T?o file `TestController.cs` ?? test session:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace StudentManagementSystem.Controllers
{
    public class TestController : Controller
    {
    [HttpGet]
        public IActionResult SetSession()
        {
        HttpContext.Session.SetString("TestKey", "TestValue");
            return Content("Session set");
        }

        [HttpGet]
    public IActionResult GetSession()
        {
   var value = HttpContext.Session.GetString("TestKey");
            return Content($"Session value: {value ?? "NULL"}");
        }

        [HttpGet]
  public IActionResult Info()
     {
            var userId = HttpContext.Session.GetString("UserId");
   var role = HttpContext.Session.GetString("UserRole");
     var userName = HttpContext.Session.GetString("UserName");

            return Content($@"
UserId: {userId ?? "NULL"}
UserRole: {role ?? "NULL"}
UserName: {userName ?? "NULL"}
Session ID: {HttpContext.Session.Id}
          ");
        }
  }
}
```

Test:
1. Truy c?p: https://localhost:5001/Test/SetSession
2. Truy c?p: https://localhost:5001/Test/GetSession (ph?i th?y "TestValue")
3. Login
4. Truy c?p: https://localhost:5001/Test/Info (ph?i th?y thông tin user)

---

### Option 3: Bypass Authentication (Temporary)

**CH? ?? TEST**, t?m th?i disable authorization trong `DashboardController`:

```csharp
//[AuthorizeRole("Admin", "Teacher", "Student")]  // Comment out
public class DashboardController : Controller
{
    public async Task<IActionResult> Index()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        var userId = HttpContext.Session.GetString("UserId");

   // DEBUG: Log session values
     Console.WriteLine($"Dashboard - UserId: {userId}, Role: {userRole}");

        if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
        {
     // DEBUG: Which value is null?
            Console.WriteLine($"Session is null - UserId: {userId == null}, Role: {userRole == null}");
  return RedirectToAction("Login", "Account");
        }

   // ... rest of code
    }
}
```

---

## ?? CHECKLIST X? LÝ

Làm theo th? t?:

- [ ] Ch?y `TEST_CONNECTION.sql` trong SSMS ? Thành công?
- [ ] Test ping `202.55.135.42` ? Thành công?
- [ ] Test telnet port 1433 ? Thành công?
- [ ] Connection string trong `appsettings.json` ? ?úng?
- [ ] `dotnet clean` + `dotnet build` ? Thành công?
- [ ] Xem Browser DevTools ? Có cookie session?
- [ ] Xem Console logs ? Có error?
- [ ] Test v?i `/Test/SetSession` ? Session ho?t ??ng?
- [ ] Thêm debug logging ? Th?y gì trong console?

---

## ?? CÁC NGUYÊN NHÂN TH??NG G?P

### 1. Database ch?a t?o ho?c sai tên
**Gi?i pháp:**
```sql
-- Trong SSMS
SELECT name FROM sys.databases WHERE name = 'StudentManagementSystem';
-- N?u empty ? Ch?y creation script
```

### 2. Connection string sai
**Tri?u ch?ng:** Exception khi login
**Gi?i pháp:** Verify t?ng ph?n c?a connection string

### 3. Session cookie b? block
**Tri?u ch?ng:** Cookie không hi?n th? trong Browser DevTools
**Gi?i pháp:** 
- Check browser privacy settings
- Try incognito mode
- Clear all cookies

### 4. HTTPS redirect issue
**Tri?u ch?ng:** Cookie b? m?t khi redirect
**Gi?i pháp:** Ch?y v?i HTTP:
```bash
dotnet run --urls "http://localhost:5000"
```

### 5. Firewall/Antivirus blocking
**Tri?u ch?ng:** Không k?t n?i ???c SQL Server
**Gi?i pháp:**
- T?m th?i disable firewall ?? test
- Add exception cho port 1433

---

## ?? N?U V?N KHÔNG ???C

### G?i cho tôi thông tin sau:

1. **K?t qu? c?a `TEST_CONNECTION.sql`**

2. **Screenshot Browser DevTools:**
   - Tab Network (request POST /Account/Login)
   - Tab Application ? Cookies

3. **Console output khi login:**
```bash
dotnet run > output.txt 2>&1
# Login r?i g?i file output.txt
```

4. **Test connection trong SSMS:**
```sql
SELECT @@SERVERNAME, DB_NAME();
SELECT COUNT(*) FROM Users WHERE Username = 'admin';
```

5. **Browser console errors:**
   - F12 ? Console tab
   - Screenshot any errors

---

## ? K?T LU?N

V?i các thay ??i ?ã áp d?ng:
1. ? Session commit ???c thêm
2. ? Session cookie configuration c?i thi?n
3. ? Middleware order ?úng

**Hãy:**
1. **Stop** ?ng d?ng hi?n t?i
2. **Clean & Rebuild**
3. **Ch?y l?i**
4. **Test login**

N?u v?n l?i, hãy làm theo checklist và g?i thông tin debug cho tôi!
