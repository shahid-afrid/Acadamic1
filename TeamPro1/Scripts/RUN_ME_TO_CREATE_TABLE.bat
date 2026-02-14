@echo off
echo =========================================
echo  Creating Notifications Table
echo =========================================
echo.
echo Connecting to database...
echo.

REM Run the SQL script using sqlcmd
sqlcmd -S "(localdb)\mssqllocaldb" -d TeamPro1 -i "%~dp0AddNotificationsTable.sql"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =========================================
    echo  SUCCESS! Table created successfully!
    echo =========================================
    echo.
    echo Next steps:
    echo 1. Press F5 in Visual Studio to run the app
    echo 2. Test the notification feature
    echo.
    echo Press any key to verify the table was created...
    pause >nul
    
    REM Verify the table
    echo.
    echo Verifying table creation...
    echo.
    sqlcmd -S "(localdb)\mssqllocaldb" -d TeamPro1 -Q "SELECT OBJECT_ID('Notifications') as TableExists; SELECT COUNT(*) as NotificationCount FROM Notifications;"
    
    echo.
    echo If you see a number above, the table exists!
    echo.
) else (
    echo.
    echo =========================================
    echo  ERROR! Something went wrong.
    echo =========================================
    echo.
    echo Possible reasons:
    echo 1. SQL Server LocalDB is not running
    echo 2. TeamPro1 database doesn't exist
    echo 3. Connection string is wrong
    echo.
    echo Try opening SQL Server Management Studio and run the script manually.
    echo.
)

echo Press any key to close...
pause >nul
