# ?? **WHERE TO FIND THE FILES - Visual Guide**

```
?? C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts\

    ???????????????????????????????????????????????
    ?  ?? RUN_ME_TO_CREATE_TABLE.bat            ? ? DOUBLE-CLICK THIS! ?
    ?  ?? RUN_ME_TO_CREATE_TABLE.ps1             ? ? Or this (PowerShell)
    ?  ?? AddNotificationsTable.sql              ? ? Original script
    ?  ?? VerifyNotificationsSetup.sql           ? ? Verification
    ?  ?? HOW_TO_RUN.md                          ? ? Instructions
    ???????????????????????????????????????????????
```

---

## ?? **FASTEST METHOD (10 seconds):**

### **Step 1: Open File Explorer**
Press `Windows + E`

### **Step 2: Navigate to Scripts folder**
Copy this path and paste in address bar:
```
C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts
```

### **Step 3: Double-Click the BAT file**
```
?? RUN_ME_TO_CREATE_TABLE.bat
```

### **Step 4: Wait for Success**
You should see:
```
=========================================
 SUCCESS! Table created successfully!
=========================================
```

### **Step 5: Press Any Key**
Close the window

### **Step 6: Done!** ?
Go back to Visual Studio and press F5

---

## ?? **What the Script Does:**

```
Start
  ?
Connect to (localdb)\mssqllocaldb
  ?
Select TeamPro1 database
  ?
Run AddNotificationsTable.sql
  ?
Create Notifications table
  ?
Verify table exists
  ?
Show SUCCESS message
  ?
End
```

---

## ? **Expected Output:**

```cmd
=========================================
 Creating Notifications Table
=========================================

Connecting to database...

Notifications table created successfully!

=========================================
 SUCCESS! Table created successfully!
=========================================

Next steps:
1. Press F5 in Visual Studio to run the app
2. Test the notification feature

Press any key to verify the table was created...

Verifying table creation...

TableExists
-----------
591673878

NotificationCount
-----------------
0

If you see a number above, the table exists!

Press any key to close...
```

---

## ?? **Alternative: Visual Studio Terminal**

1. In Visual Studio, open **Terminal** (View ? Terminal)
2. Run this command:
```powershell
sqlcmd -S "(localdb)\mssqllocaldb" -d TeamPro1 -i "Scripts\AddNotificationsTable.sql"
```
3. Look for: ? "Notifications table created successfully!"

---

## ?? **Screenshot Guide:**

### **Step 1: File Explorer**
![image](https://via.placeholder.com/600x100/4CAF50/FFFFFF?text=Windows+%2B+E+to+open+File+Explorer)

### **Step 2: Navigate to Scripts**
![image](https://via.placeholder.com/600x100/2196F3/FFFFFF?text=Paste+path+in+address+bar)

### **Step 3: Double-Click BAT**
![image](https://via.placeholder.com/600x100/FF9800/FFFFFF?text=RUN_ME_TO_CREATE_TABLE.bat)

### **Step 4: See Success**
![image](https://via.placeholder.com/600x100/8BC34A/FFFFFF?text=SUCCESS!+Table+created)

---

## ?? **Troubleshooting:**

### **"'sqlcmd' is not recognized"**
**Solution:** Install SQL Server Command Line Tools
1. Download: https://aka.ms/sqlcmd
2. Install
3. Try again

### **"Cannot open database 'TeamPro1'"**
**Solution:** Database doesn't exist
1. Open SSMS
2. Create database: `CREATE DATABASE TeamPro1`
3. Try again

### **"Login failed"**
**Solution:** Wrong connection string
1. Check `appsettings.json`
2. Verify server name
3. Use Windows Authentication

---

## ?? **Quick Checklist:**

Before running the script:
- [ ] SQL Server LocalDB is installed
- [ ] TeamPro1 database exists
- [ ] Students table exists
- [ ] You have admin permissions

After running the script:
- [ ] No errors in console
- [ ] "SUCCESS!" message appeared
- [ ] Verification shows table exists
- [ ] Ready to test the feature!

---

## ?? **After Success:**

Your database now has:
```sql
???????????????????????????????????????
?  Notifications Table                ?
???????????????????????????????????????
?  ? Id (INT, Primary Key)            ?
?  ? StudentId (INT, Foreign Key)     ?
?  ? Message (NVARCHAR(500))          ?
?  ? Type (NVARCHAR(50))              ?
?  ? IsRead (BIT)                     ?
?  ? CreatedAt (DATETIME2)            ?
???????????????????????????????????????
```

---

## ?? **Next: Test the Feature!**

1. Press `F5` in Visual Studio
2. Login as Student A
3. Send request to Student B
4. Login as Student B
5. Reject the request
6. Login as Student A
7. Go to Pool of Students
8. **?? POP-UP APPEARS!**

---

**That's it! The script is ready to run!** ??

**Just double-click: `RUN_ME_TO_CREATE_TABLE.bat`** ?
