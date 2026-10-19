@echo off
chcp 65001 >nul
title BTL_StudentMGMT - Running Server

:: Di chuyển vào thư mục của file bat
cd /d "%~dp0"

echo ========================================================
echo    DỰ ÁN QUẢN LÝ SINH VIÊN - BTL_StudentMGMT
echo ========================================================
echo.
echo [*] Đang khởi động web server...
echo [*] Địa chỉ web: https://localhost:7263
echo [*] Nhấn Ctrl + C để dừng server.
echo.

:: Tự động mở trình duyệt sau 3 giây
start "" "https://localhost:7263"

:: Chạy dự án với profile https
dotnet run --launch-profile "https"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [!] Server đã dừng hoặc gặp lỗi.
    pause
)
