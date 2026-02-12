@echo off
title TeamPro1 - Local Server
color 0A

echo ================================================================
echo                TeamPro1 - Local Server
echo ================================================================
echo.

REM Get the local IP address
echo Detecting network configuration...
for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /c:"IPv4 Address"') do (
    set "ip=%%a"
    goto :found
)
:found
set ip=%ip:=%

echo.
echo ================================================================
echo                    SERVER ACCESS INFORMATION
echo ================================================================
echo.
echo   Local Access:
echo    http://localhost:5000
echo.
echo   Network Access (share this with other users):
echo    http://%ip%:5000
echo.
echo   Mobile/Tablet Access:
echo    Connect to same Wi-Fi and use: http://%ip%:5000
echo.
echo ================================================================

echo.
echo Preparing database...
echo Applying any pending migrations...
dotnet ef database update
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Database setup failed!
    echo Please ensure SQL Server/LocalDB is running.
    pause
    exit /b 1
)

echo.
echo Database ready!
echo.
echo Starting TeamPro1 server...
echo.
echo IMPORTANT NOTES:
echo    - Keep this window open while server is running
echo    - Press Ctrl+C to stop the server
echo    - Make sure Windows Firewall allows port 5000
echo.
echo ================================================================
echo                     SERVER STARTING...
echo ================================================================
echo.

REM Start the application (HTTP only on port 5000)
dotnet run --environment Production --urls "http://0.0.0.0:5000" --no-launch-profile

echo.
echo Server stopped.
pause