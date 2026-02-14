# ?? Setup Guide: Notification Pop-up Feature

## ? Step-by-Step Instructions

### **Step 1: Run the Database Script** ??

1. **Open SQL Server Management Studio (SSMS)**
   - Press `Windows + S` and search for "SQL Server Management Studio"
   - Or find it in your Start Menu

2. **Connect to Your Database**
   - Server name: `(localdb)\mssqllocaldb`
   - Authentication: Windows Authentication
   - Click **Connect**

3. **Open the SQL Script**
   - In SSMS, go to: `File ? Open ? File...`
   - Navigate to: `C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts\AddNotificationsTable.sql`
   - Click **Open**

4. **Select the Correct Database**
   - In the toolbar, find the dropdown that says "master"
   - Change it to: **TeamPro1**
   - ?? This is important! Make sure you're using the TeamPro1 database

5. **Execute the Script**
   - Press `F5` or click the **Execute** button
   - You should see: ? **"Notifications table created successfully!"**

---

### **Step 2: Verify the Table Was Created** ??

Run this query in SSMS to verify:

```sql
-- Check if Notifications table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'Notifications'

-- View the table structure
EXEC sp_help 'Notifications'

-- Check if it's empty (should return 0 rows initially)
SELECT COUNT(*) as NotificationCount FROM Notifications
```

**Expected Results:**
- First query: Shows 1 row with table info
- Second query: Shows table structure with columns
- Third query: Returns 0 (no notifications yet)

---

### **Step 3: Stop and Rebuild Your Application** ??

1. **Stop the Application (if running)**
   - In Visual Studio, click the **Stop** button (red square)
   - Or press `Shift + F5`

2. **Clean the Solution**
   ```
   Build ? Clean Solution
   ```
   Wait for "Clean succeeded" message

3. **Rebuild the Solution**
   ```
   Build ? Rebuild Solution
   ```
   Wait for "Rebuild succeeded" message

4. **Check for Errors**
   - Look at the **Error List** window (View ? Error List)
   - Should show: **0 Errors, 0 Warnings**

---

### **Step 4: Run the Application** ??

1. **Start Debugging**
   - Press `F5` or click **Start Debugging**
   - Wait for browser to open

2. **Check Console Output**
   - Look for: "Now listening on: https://localhost:5001"
   - No errors should appear

---

### **Step 5: Test the Notification Feature** ??

#### **Scenario: Student A sends request, Student B rejects it**

1. **Create Two Test Students (if not exists)**
   ```sql
   -- Run in SSMS
   -- Check existing students
   SELECT Id, FullName, Email, RegdNumber FROM Students
   ```

2. **Login as Student A**
   - Go to: `https://localhost:5001/Student/Login`
   - Login with Student A credentials
   - Example: `student1@rgmcet.edu` / password

3. **Go to Pool of Students**
   - Click **Pool of Students** from dashboard
   - ?? Make sure "Team Formation Available" alert shows (green)
   - If closed, ask admin to open it

4. **Send Team Request to Student B**
   - Find Student B in the list
   - Click **Send Request** button
   - Should see: ? "Team request sent successfully!"

5. **Logout and Login as Student B**
   - Click **Logout** button
   - Login with Student B credentials
   - Example: `student2@rgmcet.edu` / password

6. **Check Notifications**
   - Go to **Pool of Students**
   - Click the **Bell Icon** (??) in top-right
   - Should see 1 notification: Request from Student A

7. **Reject the Request**
   - In the notification modal, click **Reject**
   - Confirm the rejection
   - Should see: ? "Request rejected successfully!"
   - The request card should disappear

8. **Logout and Login as Student A Again**
   - Click **Logout**
   - Login as Student A

9. **See the Pop-up Notification** ??
   - Go to **Pool of Students**
   - **Wait 500ms (half a second)**
   - **POP-UP SHOULD APPEAR!** ??
   - Message: "Student B Name (RegdNumber) has rejected your team request."
   - Background: Red/danger theme
   - Time: "Just now" or "5 minutes ago"

10. **Close the Pop-up**
    - Click **OK** button
    - Or click the **X** button
    - Or click outside the pop-up

11. **Verify It Won't Appear Again**
    - Refresh the page
    - Pop-up should NOT appear (already marked as read)

---

### **Step 6: Verify in Database** ??

Run this query in SSMS:

```sql
-- Check notifications created
SELECT 
    n.Id,
    s.FullName as StudentName,
    s.RegdNumber,
    n.Message,
    n.Type,
    n.IsRead,
    n.CreatedAt
FROM Notifications n
INNER JOIN Students s ON n.StudentId = s.Id
ORDER BY n.CreatedAt DESC

-- Should show:
-- 1 notification for Student A
-- IsRead = 1 (true) after viewing
-- Type = 'danger'
-- Message contains "rejected your team request"
```

---

## ?? Quick Test Commands

### **PowerShell Commands (Run in Package Manager Console)**

```powershell
# Check if application builds
dotnet build

# Run application
dotnet run

# Check database connection
# (Already in appsettings.json)
```

### **SQL Quick Checks**

```sql
-- 1. Verify Notifications table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notifications'

-- 2. Check all notifications
SELECT * FROM Notifications ORDER BY CreatedAt DESC

-- 3. Check unread notifications for a specific student
SELECT * FROM Notifications WHERE StudentId = 1 AND IsRead = 0

-- 4. Manually mark notification as unread (for testing)
UPDATE Notifications SET IsRead = 0 WHERE Id = 1

-- 5. Delete all notifications (for testing)
-- DELETE FROM Notifications
```

---

## ?? Troubleshooting

### **Problem 1: Pop-up doesn't appear**

? **Solutions:**
```javascript
// Open browser console (F12) and check for errors
// Should see no errors related to notifications

// Manually check if notifications exist
console.log('Notifications:', @Html.Raw(System.Text.Json.JsonSerializer.Serialize(ViewBag.UnreadNotifications ?? new List<object>())));
```

**Check in SSMS:**
```sql
-- Are there unread notifications for this student?
SELECT * FROM Notifications WHERE StudentId = YOUR_STUDENT_ID AND IsRead = 0
```

---

### **Problem 2: Database table not created**

? **Solutions:**
1. Make sure you selected **TeamPro1** database (not master)
2. Check for SQL errors in SSMS
3. Run this to drop and recreate:
```sql
-- Drop table if exists (WARNING: Deletes all notifications)
DROP TABLE IF EXISTS Notifications

-- Then run the AddNotificationsTable.sql script again
```

---

### **Problem 3: Build errors**

? **Solutions:**
```powershell
# Clean and rebuild
dotnet clean
dotnet build

# Check for missing packages
dotnet restore
```

**Check Error List in Visual Studio:**
- Should show 0 errors
- If errors about `Notifications`, rebuild again

---

### **Problem 4: Application won't start**

? **Check:**
1. Port 5001 is not already in use
2. No other Visual Studio instances running
3. Database connection string is correct in `appsettings.json`

```json
// Should look like:
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TeamPro1;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

---

## ?? Checklist

Before testing, make sure:

- [ ] ? SQL script executed successfully in SSMS
- [ ] ? TeamPro1 database selected (not master)
- [ ] ? Notifications table created (verified with query)
- [ ] ? Application builds without errors (0 errors)
- [ ] ? Application runs on https://localhost:5001
- [ ] ? Two test students exist in database
- [ ] ? Team formation is OPEN for test students' year
- [ ] ? No existing team requests between test students

---

## ?? Success Criteria

You'll know it's working when:

1. ? Student A sends request to Student B
2. ? Student B sees notification bell with badge (1)
3. ? Student B opens modal and rejects request
4. ? Request disappears from modal
5. ? Student A logs back in
6. ? **POP-UP APPEARS AUTOMATICALLY** on Pool of Students page
7. ? Pop-up shows rejection message with red theme
8. ? Clicking OK closes pop-up
9. ? Refreshing page does NOT show pop-up again (marked as read)
10. ? Database shows notification with IsRead = 1

---

## ?? Still Having Issues?

### **Check Browser Console (F12)**
1. Press `F12` in browser
2. Go to **Console** tab
3. Look for JavaScript errors
4. Should see: No errors
5. Can manually trigger pop-up:
```javascript
// In browser console, test the function
showNotificationPopup([{
    Id: 1,
    Message: "Test notification",
    Type: "danger",
    CreatedAt: new Date().toISOString()
}]);
```

### **Check Network Tab**
1. Press `F12` ? **Network** tab
2. Reload the page
3. Look for `PoolOfStudents` request
4. Check Response Preview
5. Should see `UnreadNotifications` in ViewBag

---

## ?? Security Notes

? **Already Implemented:**
- CSRF protection (anti-forgery tokens)
- SQL injection protection (Entity Framework)
- XSS protection (HTML encoding)
- Authorization (student can only see own notifications)

---

## ?? Files Modified

All these files are already updated and ready:

1. ? `TeamPro1\Models\Notification.cs` - Model
2. ? `TeamPro1\Models\AppDbContext.cs` - DbSet + Configuration
3. ? `TeamPro1\Controllers\StudentController.cs` - Backend logic
4. ? `TeamPro1\Views\Student\PoolOfStudents.cshtml` - Frontend UI
5. ? `TeamPro1\Scripts\AddNotificationsTable.sql` - Database script

**Nothing else needs to be changed!**

---

## ? Quick Start (TL;DR)

```bash
# 1. Run SQL script in SSMS (TeamPro1 database)
# 2. Rebuild solution in Visual Studio
# 3. Press F5 to run
# 4. Login as Student A ? Send request to Student B
# 5. Login as Student B ? Reject request
# 6. Login as Student A ? Go to Pool of Students
# 7. POP-UP SHOULD APPEAR! ??
```

---

**Good luck! ?? The feature is ready to test!**
