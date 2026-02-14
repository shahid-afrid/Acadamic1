# =========================================
# Create Notifications Table - PowerShell
# =========================================

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " Creating Notifications Table" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$sqlScriptPath = Join-Path $scriptPath "AddNotificationsTable.sql"

Write-Host "Script location: $sqlScriptPath" -ForegroundColor Yellow
Write-Host "Connecting to database..." -ForegroundColor Yellow
Write-Host ""

try {
    # Run the SQL script
    $result = sqlcmd -S "(localdb)\mssqllocaldb" -d TeamPro1 -i $sqlScriptPath 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "==========================================" -ForegroundColor Green
        Write-Host " SUCCESS! Table created successfully!" -ForegroundColor Green
        Write-Host "==========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host $result -ForegroundColor White
        Write-Host ""
        
        # Verify the table
        Write-Host "Verifying table creation..." -ForegroundColor Yellow
        Write-Host ""
        
        $verifyQuery = "SELECT 
            'Notifications' as TableName,
            CASE WHEN OBJECT_ID('Notifications') IS NOT NULL THEN 'EXISTS' ELSE 'NOT FOUND' END as Status
        FROM sys.objects WHERE object_id = OBJECT_ID('Notifications') OR 1=1;"
        
        $verifyResult = sqlcmd -S "(localdb)\mssqllocaldb" -d TeamPro1 -Q $verifyQuery
        Write-Host $verifyResult -ForegroundColor White
        
        # Count notifications
        Write-Host ""
        Write-Host "Checking notification count..." -ForegroundColor Yellow
        $countResult = sqlcmd -S "(localdb)\mssqllocaldb" -d TeamPro1 -Q "SELECT COUNT(*) as NotificationCount FROM Notifications;"
        Write-Host $countResult -ForegroundColor White
        
        Write-Host ""
        Write-Host "==========================================" -ForegroundColor Green
        Write-Host " Next Steps:" -ForegroundColor Green
        Write-Host "==========================================" -ForegroundColor Green
        Write-Host "1. Press F5 in Visual Studio to run the app" -ForegroundColor White
        Write-Host "2. Test the notification feature" -ForegroundColor White
        Write-Host "   - Login as Student A, send request to Student B" -ForegroundColor White
        Write-Host "   - Login as Student B, reject the request" -ForegroundColor White
        Write-Host "   - Login as Student A, go to Pool of Students" -ForegroundColor White
        Write-Host "   - POP-UP SHOULD APPEAR!" -ForegroundColor Yellow
        Write-Host ""
        
    } else {
        Write-Host "==========================================" -ForegroundColor Red
        Write-Host " ERROR! Something went wrong." -ForegroundColor Red
        Write-Host "==========================================" -ForegroundColor Red
        Write-Host ""
        Write-Host $result -ForegroundColor Red
        Write-Host ""
        Write-Host "Possible reasons:" -ForegroundColor Yellow
        Write-Host "1. SQL Server LocalDB is not running" -ForegroundColor White
        Write-Host "2. TeamPro1 database doesn't exist" -ForegroundColor White
        Write-Host "3. Connection string is wrong" -ForegroundColor White
        Write-Host ""
        Write-Host "Try opening SQL Server Management Studio and run the script manually." -ForegroundColor Yellow
        Write-Host ""
    }
}
catch {
    Write-Host "==========================================" -ForegroundColor Red
    Write-Host " EXCEPTION OCCURRED!" -ForegroundColor Red
    Write-Host "==========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure sqlcmd is installed and in your PATH." -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "Press any key to close..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
