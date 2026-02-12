@echo off
echo ========================================
echo  TeamPro1 - Internet Access
echo  Using Ngrok Tunnel
echo ========================================
echo.

REM Check if ngrok is available
where ngrok >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Ngrok not found!
    echo.
    echo Please install ngrok:
    echo 1. Install from Microsoft Store
    echo 2. OR download from https://ngrok.com/download
    echo 3. Configure authtoken: ngrok config add-authtoken YOUR_TOKEN
    echo.
    pause
    exit /b 1
)

echo [INFO] Ngrok found!
echo.

REM Check if authtoken is configured
ngrok config check >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [WARNING] Ngrok authtoken may not be configured.
    echo.
    echo To configure:
    echo 1. Visit https://dashboard.ngrok.com/get-started/your-authtoken
    echo 2. Copy your authtoken
    echo 3. Run: ngrok config add-authtoken YOUR_TOKEN
    echo.
    echo Press any key to continue anyway...
    pause >nul
)

echo ========================================
echo  Starting Application...
echo ========================================
echo.
echo [1/2] Starting ASP.NET Core application on port 5000 (HTTP only)...
echo.

REM Start the application in a new window (HTTP only, different port from Visual Studio)
start "TeamPro1 Application" cmd /k "cd /d %~dp0 && dotnet run --urls http://localhost:5000 --no-launch-profile"

echo Waiting for application to start (15 seconds)...
timeout /t 15 /nobreak >nul

echo.
echo ========================================
echo  Starting Ngrok Tunnel...
echo ========================================
echo.
echo [2/2] Starting ngrok tunnel to port 5000...
echo.
echo Opening ngrok web interface at: http://127.0.0.1:4040
echo.

echo ========================================
echo  NGROK TUNNEL ACTIVE
echo ========================================
echo.
echo Copy the "Forwarding" URL and share it!
echo Example: https://abc123.ngrok-free.app
echo.
echo Keep this window open to maintain the tunnel.
echo Close this window to stop the tunnel.
echo.
echo ========================================

REM Start ngrok web interface in browser
start http://127.0.0.1:4040

REM Start ngrok tunnel
ngrok http 5000

echo.
echo Ngrok tunnel closed.
echo Press any key to exit...
pause >nul
