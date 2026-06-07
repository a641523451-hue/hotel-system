@echo off
chcp 65001 >nul
title 酒店管理系统 - 启动中

echo ====================================
echo   酒店管理系统 v1.0
echo ====================================
echo.

:: 获取本机局域网IP
for /f "tokens=2 delims=:" %%i in ('ipconfig ^| findstr /R /C:"IPv4 Address" /C:"IPv4 地址"') do set IP=%%i
set IP=%IP: =%

echo 本机IP地址: %IP%
echo 数据库类型: SQLite（无需安装）
echo 数据库文件: HotelDB.sqlite（自动创建）
echo.
echo 本机访问: http://localhost:5001
echo 其他电脑: http://%IP%:5001
echo.
echo 管理员账号: admin / admin123
echo 前台账号:   front / front123
echo.
echo ====================================
echo 正在启动，请稍候...
echo.

dotnet run --project "%~dp0Hotel.web.csproj" --urls "http://0.0.0.0:5001"

pause
