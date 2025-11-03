@echo off
echo ========================================
echo  🚀 STUDENT MANAGEMENT SYSTEM - QUICK START
echo ========================================
echo.

cd ..

echo [1/3] Checking backend...
cd c:\Users\TDG\source\repos\StudentManagementSystem\StudentManagementSystem

echo.
echo [2/3] Starting Backend (ASP.NET Core)...
start "Backend - ASP.NET Core" cmd /k "dotnet run"
timeout /t 5 /nobreak >nul

echo.
echo [3/3] Starting Frontend (Angular)...
cd ClientApp
start "Frontend - Angular" cmd /k "npm start"

echo.
echo ========================================
echo  ✅ Application Starting...
echo  Backend:  http://localhost:5298
echo  Frontend: http://localhost:4200
echo ========================================
echo.
echo 🔐 Test accounts:
echo   Admin:   admin / admin123
echo   Teacher: gv001 / gv001
echo   Student: sv001 / sv001
echo.
echo 💡 Tip: Open http://localhost:4200 in browser
echo 🤖 AI Chatbot: Click icon at bottom-right
echo.
echo Press any key to close this window...
echo (Backend and Frontend will keep running)
echo ========================================
pause >nul
