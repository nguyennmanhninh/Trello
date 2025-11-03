# ?? H??NG D?N CH?Y DEBUG - NHANH

## ? 3 B??C ??N GI?N

### B??C 1: Ch?y ?ng D?ng

**Visual Studio:**
- Nh?n **F5**

**Command Line:**
```bash
cd StudentManagementSystem
dotnet run
```

??i ??n khi th?y:
```
Now listening on: https://localhost:5001
```

---

### B??C 2: M? Debug Dashboard

Trong browser, truy c?p:
```
https://localhost:5001/Debug
```

B?n s? th?y trang v?i 6 nút test màu xanh.

---

### B??C 3: Ch?y Test Theo Th? T?

Click vào t?ng nút (m? tab m?i):

#### ? Test 1: Database Connection
- Ph?i th?y: `? SUCCESS`
- N?u FAILED ? Database ch?a t?o ho?c connection string sai

#### ? Test 2: Database Queries  
- Ph?i th?y: Admin, Teachers, Students
- N?u không có data ? Ch?y SQL script

#### ? Test 3: Session Status
- Ph?i th?y: `Session IsAvailable: True`
- Ph?i có cookie: `.StudentManagement.Session`

#### ? Test 4: Test Login Admin ? QUAN TR?NG
- Ph?i th?y: `??? SESSION WORKING CORRECTLY! ???`
- N?u th?y dòng này ? Backend OK, v?n ?? ? browser

---

## ?? K?T QU?

### N?u Test 4 hi?n: ??? SESSION WORKING CORRECTLY!

**V?n ??:** Browser cookies b? block ho?c clear

**Gi?i pháp:**

1. **Clear cookies:**
   - F12 ? Application ? Cookies ? Delete All
   - Ho?c: Ctrl+Shift+Delete ? Clear cookies

2. **Th? Incognito:**
   - Chrome: Ctrl+Shift+N
   - Firefox: Ctrl+Shift+P

3. **Th? browser khác**

4. **Login l?i**

---

### N?u Test 4 hi?n: ??? SESSION NOT SAVED

**V?n ??:** Session middleware không ho?t ??ng

**Gi?i pháp:**

1. Stop app
2. Run:
```bash
dotnet clean
dotnet build
dotnet run
```
3. Test l?i

---

### N?u Test 1 FAILED

**V?n ??:** Không k?t n?i ???c database

**Gi?i pháp:**

1. **Check SQL Server ?ang ch?y:**
   - Win+R ? `services.msc`
   - Tìm "SQL Server" ? Start

2. **Test trong SSMS:**
   ```
   Server: 202.55.135.42
   Login: sa
 Password: Aa@0967941364
   ```

3. **Check appsettings.json:**
 ```json
   "DefaultConnection": "Server=202.55.135.42;Database=StudentManagementSystem;..."
   ```

---

## ?? XEM LOGS CHI TI?T

### Visual Studio:
- View ? Output
- Dropdown ch?n "Debug" ho?c "StudentManagementSystem"

### Command Line:
- Logs hi?n th? ngay trong terminal

### Tìm ki?m:
```
=== LOGIN ATTEMPT ===
Username: admin
Auth result - Success: True
Session values set
? Session verification successful!
```

N?u th?y `? Session verification successful!` ? Login thành công

---

## ?? G?I K?T QU? DEBUG

N?u v?n l?i, g?i cho tôi:

### 1. K?t qu? Test 4
Copy toàn b? text t?:
```
https://localhost:5001/Debug/TestLogin?username=admin&password=admin123
```

### 2. Browser DevTools
- F12 ? Application ? Cookies
- Screenshot

### 3. Console Logs
- Copy t? Visual Studio Output ho?c terminal

### 4. Network Tab
- F12 ? Network
- Login
- Screenshot request `POST /Account/Login`

---

## ? CHECKLIST

- [ ] App ?ang ch?y (https://localhost:5001)
- [ ] Test 1: Database ? PASS
- [ ] Test 2: Data ? Found users/teachers/students
- [ ] Test 3: Session ? IsAvailable: True
- [ ] Test 4: Login ? SESSION WORKING CORRECTLY
- [ ] Clear cookies
- [ ] Th? login bình th??ng
- [ ] N?u v?n l?i ? Incognito mode
- [ ] N?u v?n l?i ? Browser khác

---

## ?? G?I CHO TÔI

### Template báo l?i:

```
Test 1 (Database): [PASS/FAIL]
Test 2 (Data): [PASS/FAIL]  
Test 3 (Session): [PASS/FAIL]
Test 4 (Login): [PASS/FAIL - Copy k?t qu?]

Mô t? l?i:
[Mô t? chính xác b?n th?y gì khi login]

Browser: [Chrome/Firefox/Edge]
Screenshot: [?ính kèm]
```

---

**URL Debug:** https://localhost:5001/Debug

**Chúc may m?n! ??**
