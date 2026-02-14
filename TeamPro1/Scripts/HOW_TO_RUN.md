# ?? ONE-CLICK DATABASE SETUP

## ? **Choose ONE of these methods:**

---

## **METHOD 1: Double-Click BAT File** (Easiest!)

1. Open File Explorer
2. Navigate to: `C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts\`
3. **Double-click:** `RUN_ME_TO_CREATE_TABLE.bat`
4. Wait for "SUCCESS!" message
5. Press any key to close

**That's it!** ?

---

## **METHOD 2: PowerShell** (If BAT doesn't work)

1. **Right-click** `RUN_ME_TO_CREATE_TABLE.ps1`
2. Select: **"Run with PowerShell"**
3. Wait for "SUCCESS!" message
4. Press any key to close

---

## **METHOD 3: Visual Studio Package Manager Console**

1. In Visual Studio, go to: `Tools ? NuGet Package Manager ? Package Manager Console`
2. Copy and paste this command:

```powershell
sqlcmd -S "(localdb)\mssqllocaldb" -d TeamPro1 -i "C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts\AddNotificationsTable.sql"
```

3. Press Enter
4. Look for: ? "Notifications table created successfully!"

---

## **METHOD 4: Command Prompt**

1. Press `Windows + R`
2. Type: `cmd` and press Enter
3. Copy and paste this command:

```cmd
sqlcmd -S "(localdb)\mssqllocaldb" -d TeamPro1 -i "C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts\AddNotificationsTable.sql"
```

4. Press Enter
5. Look for: ? "Notifications table created successfully!"

---

## **METHOD 5: SQL Server Management Studio** (Manual)

1. Open **SQL Server Management Studio**
2. Connect to: `(localdb)\mssqllocaldb`
3. Select database: **TeamPro1** (dropdown at top)
4. Press `Ctrl + O` (Open File)
5. Navigate to: `TeamPro1\Scripts\AddNotificationsTable.sql`
6. Press `F5` (Execute)
7. Look for: ? "Notifications table created successfully!"

---

## ? **Verification**

After running any method above, verify it worked:

```sql
-- Run in SSMS or sqlcmd
USE TeamPro1
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notifications'
```

**Expected:** 1 row showing Notifications table

---

## ?? **What to Do After Success**

1. **Press F5 in Visual Studio** to run the app
2. **Test the feature:**
   - Login as Student A ? Send request to Student B
   - Login as Student B ? Reject request
   - Login as Student A ? Go to Pool of Students
   - **?? POP-UP APPEARS!**

---

## ?? **If Nothing Works**

Try this emergency SQL script in SSMS:

```sql
USE TeamPro1
GO

-- Drop if exists (warning: deletes all notifications)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
    DROP TABLE Notifications
GO

-- Create table
CREATE TABLE [Notifications] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [StudentId] INT NOT NULL,
    [Message] NVARCHAR(500) NOT NULL,
    [Type] NVARCHAR(50) NOT NULL DEFAULT 'info',
    [IsRead] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Students_StudentId] FOREIGN KEY ([StudentId]) 
        REFERENCES [Students]([Id]) ON DELETE CASCADE
)
GO

-- Create index
CREATE INDEX [IX_Notifications_StudentId] ON [Notifications] ([StudentId])
GO

-- Verify
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notifications'
GO

PRINT 'SUCCESS! Notifications table created manually!'
GO
```

---

## ?? **Still Need Help?**

Check:
1. ? SQL Server LocalDB is running
2. ? TeamPro1 database exists
3. ? Students table exists (required for foreign key)
4. ? You have permissions to create tables

---

**TL;DR: Just double-click `RUN_ME_TO_CREATE_TABLE.bat` in the Scripts folder!** ??
