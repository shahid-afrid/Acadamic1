@echo off
color 0A
title TeamPro1 - LAN Server

echo ========================================
echo   TeamPro1 - LAN Server Setup
echo ========================================
echo.

REM Clean and build first to avoid view caching issues
echo [1/5] Cleaning previous build...
dotnet clean >nul 2>&1
echo     Done!
echo.

echo [2/5] Building project...
dotnet build >nul 2>&1
if errorlevel 1 (
    echo     ERROR: Build failed!
    pause
    exit /b 1
)
echo     Done!
echo.

REM Get local IP address
echo [3/5] Detecting your local IP address...
for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /c:"IPv4 Address"') do (
    set IP=%%a
    goto :found
)
:found
set IP=%IP:~1%
echo     Your Local IP: %IP%
echo.

REM Display firewall warning
echo [4/5] Firewall Configuration:
echo     Make sure Windows Firewall allows port 5000
echo     If blocked, run: netsh advfirewall firewall add rule name="TeamPro1" dir=in action=allow protocol=TCP localport=5000
echo.

REM Display URLs for other computers
echo [5/5] Starting server...
echo.
echo ========================================
echo   ACCESS FROM OTHER COMPUTERS:
echo ========================================
echo.
echo   HTTP:  http://%IP%:5000
echo.
echo   Share this URL with others on your LAN
echo ========================================
echo.
echo Server is starting... Press Ctrl+C to stop
echo.

REM Run the application (HTTP only on port 5000)
dotnet run --urls "http://0.0.0.0:5000" --no-launch-profile

pause
