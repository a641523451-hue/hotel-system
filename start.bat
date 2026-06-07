@echo off
chcp 65001 >nul
title 酒店管理系统

cd /d "%~dp0"

echo [1/4] 正在清理端口 5001...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5001" ^| findstr "LISTENING"') do (
    taskkill /f /pid %%a >nul 2>nul
)
timeout /t 1 /nobreak >nul

echo [2/4] 正在启动服务...
start /B dotnet run --project "%~dp0Hotel.web.csproj" --urls "http://0.0.0.0:5001"

echo [3/4] 等待服务就绪...
ping 127.0.0.1 -n 4 >nul

echo [4/4] 正在打开浏览器...
start http://localhost:5001

for /f "tokens=2 delims=:" %%i in ('ipconfig ^| findstr /R /C:"IPv4 Address" /C:"IPv4"') do set IP=%%i
set IP=%IP: =%

cls
echo ====================================
echo   酒店管理系统 v1.0
echo ====================================
echo.
echo  服务运行中...
echo.
echo  本机访问:  http://localhost:5001
echo  其他电脑:  http://%IP%:5001
echo.
echo  管理员:    admin / admin123
echo  前台:      front / front123
echo.
echo  浏览器已自动打开
echo  关闭本窗口即可停止服务
echo.
echo ====================================
pause
taskkill /f /im Hotel.web.exe >nul 2>nul
taskkill /f /im dotnet.exe >nul 2>nul