# ?? EXECUTION CHECKLIST - Notification Feature Setup

## ? Pre-Execution Verification

Your application is **BUILD SUCCESSFUL** ?

All code files are ready:
- ? `Notification.cs` - Model exists
- ? `AppDbContext.cs` - DbSet configured
- ? `StudentController.cs` - Logic implemented
- ? `PoolOfStudents.cshtml` - UI ready
- ? `AddNotificationsTable.sql` - Script ready

---

## ?? Step-by-Step Execution

### **STEP 1: Run Database Script** ?? 2 minutes

1. Open **SQL Server Management Studio (SSMS)**
   ```
   Start Menu ? Search "SQL Server Management Studio"
   ```

2. **Connect to database:**
   - Server: `(localdb)\mssqllocaldb`
   - Authentication: `Windows Authentication`
   - Click: **Connect**

3. **Open SQL script:**
   ```
   File ? Open ? File...
   Navigate to: C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts\AddNotificationsTable.sql
   ```

4. **Select TeamPro1 database:**
   - Look at toolbar (top of SSMS)
   - Find dropdown showing "master"
   - Change to: **TeamPro1** ?? IMPORTANT

5. **Execute script:**
   - Press `F5` OR click green **Execute** button
   - Wait for result

6. **Verify success:**
   - Look for message: `"Notifications table created successfully!"`
   - ? If you see this = SUCCESS!
   - ? If you see errors = Check you selected TeamPro1 database

---

### **STEP 2: Verify Database** ?? 1 minute

1. **Run verification script:**
   ```
   File ? Open ? File...
   Navigate to: C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts\VerifyNotificationsSetup.sql
   ```

2. **Execute:**
   - Press `F5`
   - Review the output

3. **Expected output:**
   ```
   ? Notifications table exists
   ? Students table exists
   ? TeamRequests table exists
   ? Teams table exists
   ? TeamFormationSchedules table exists
   
   ?? ALL CHECKS PASSED! ??
   ```

4. **If any ? appears:**
   - Read the error message
   - Follow suggested fix
   - Re-run verification

---

### **STEP 3: Stop Application** ?? 10 seconds

1. In **Visual Studio**:
   - Look for red square "Stop" button
   - Click it OR press `Shift + F5`
   - Wait for "Build stopped" message

---

### **STEP 4: Clean & Rebuild** ?? 30 seconds

1. **Clean Solution:**
   ```
   Menu: Build ? Clean Solution
   ```
   - Wait for: "Clean succeeded"

2. **Rebuild Solution:**
   ```
   Menu: Build ? Rebuild Solution
   ```
   - Wait for: "Rebuild succeeded"

3. **Check Error List:**
   ```
   Menu: View ? Error List
   ```
   - Should show: **0 Errors, 0 Warnings**
   - ? If 0 errors = SUCCESS!
   - ? If errors = Read error message, fix, rebuild

---

### **STEP 5: Run Application** ?? 20 seconds

1. **Start Debugging:**
   ```
   Press F5 OR Click green "Start" button
   ```

2. **Wait for browser to open:**
   - Should open: `https://localhost:5001`
   - Or: `http://localhost:5000`

3. **Check Output window:**
   ```
   Menu: View ? Output
   ```
   - Look for: "Now listening on: https://localhost:5001"
   - ? No errors = SUCCESS!

---

### **STEP 6: Test Notification Feature** ?? 5 minutes

#### **Part A: Login as Student A** (1 min)

1. Go to: `https://localhost:5001/Student/Login`
2. Login with first student:
   - Email: `[your student email]`
   - Password: `[your password]`
3. Click: **Login**
4. Should see: **Welcome, [Student Name]!**

---

#### **Part B: Send Team Request** (1 min)

1. From dashboard, click: **Pool of Students**
2. ?? **Check alert at top:**
   - ? Green "Team Formation Available" = Continue
   - ? Red "Team Formation Closed" = Ask admin to open it first
3. Find another student in the list (Student B)
4. Click: **Send Request** button
5. Should see: ? "Team request sent successfully!"
6. Student B should now show: "Request Sent" badge

---

#### **Part C: Logout** (10 seconds)

1. Click: **Logout** button (top right)
2. Confirm logout
3. Should return to login page

---

#### **Part D: Login as Student B** (1 min)

1. Login with second student:
   - Email: `[student B email]`
   - Password: `[student B password]`
2. Click: **Login**
3. Go to: **Pool of Students**

---

#### **Part E: Reject Request** (1 min)

1. Look for **Bell Icon** ?? (top right corner)
2. Should show: **Red badge with "1"**
3. Click the **Bell Icon**
4. Modal should open showing request from Student A
5. Click: **Reject** button
6. Confirm rejection
7. Should see: ? "Request rejected successfully!"
8. Request card should disappear
9. Close modal (click X or outside)

---

#### **Part F: Logout** (10 seconds)

1. Click: **Logout** button
2. Should return to login page

---

#### **Part G: Login as Student A Again** (30 seconds)

1. Login as Student A again
2. Click: **Login**
3. From dashboard, click: **Pool of Students**

---

#### **Part H: SEE THE MAGIC!** ? (30 seconds)

1. **Page loads...**
2. **Wait 500ms (half a second)...**
3. **?? POP-UP APPEARS! ??**

**Expected Pop-up:**
- **Title:** "Rejection Notice" (with red ? icon)
- **Background:** Red theme
- **Message:** "[Student B Name] (RegdNumber) has rejected your team request."
- **Time:** "Just now" or "X minutes ago"
- **Buttons:** OK button + X button

4. **Click OK or X to close**
5. **Refresh the page (F5)**
6. **Pop-up should NOT appear again** (already marked as read)

---

### **STEP 7: Verify in Database** ?? 1 minute

1. **Go back to SSMS**
2. **Run this query:**
   ```sql
   -- Check notifications
   SELECT 
       n.Id,
       s.FullName as StudentName,
       s.RegdNumber,
       n.Message,
       n.Type,
       CASE WHEN n.IsRead = 1 THEN 'Read' ELSE 'Unread' END as Status,
       n.CreatedAt
   FROM Notifications n
   INNER JOIN Students s ON n.StudentId = s.Id
   ORDER BY n.CreatedAt DESC
   ```

3. **Expected result:**
   - Should show 1 notification
   - StudentName = Student A's name
   - Message = "...rejected your team request"
   - Type = "danger"
   - **Status = "Read"** ? (Important!)
   - CreatedAt = Recent timestamp

---

## ? Success Criteria

You'll know everything works when:

| # | Check | Status |
|---|-------|--------|
| 1 | Notifications table created in database | ? |
| 2 | Application builds without errors (0 errors) | ? |
| 3 | Application runs on localhost | ? |
| 4 | Student A can send request to Student B | ? |
| 5 | Student B sees bell icon with badge (1) | ? |
| 6 | Student B can open notification modal | ? |
| 7 | Student B can reject request | ? |
| 8 | Request disappears after rejection | ? |
| 9 | Student A sees pop-up notification automatically | ? |
| 10 | Pop-up shows rejection message with red theme | ? |
| 11 | Clicking OK closes pop-up | ? |
| 12 | Refreshing page does NOT show pop-up again | ? |
| 13 | Database shows notification with IsRead = 1 | ? |

**If all 13 checks pass = ?? SUCCESS! ??**

---

## ?? Quick Troubleshooting

### **Pop-up doesn't appear?**

**Check 1: Browser Console**
```
Press F12 ? Console tab
Look for errors (red text)
```

**Check 2: Database**
```sql
-- Are there unread notifications?
SELECT * FROM Notifications 
WHERE StudentId = [Your Student A ID] 
AND IsRead = 0
```
- If YES = Check browser console for JavaScript errors
- If NO = Notification wasn't created or already marked as read

**Check 3: ViewBag**
- Add breakpoint in `PoolOfStudents` action
- Check if `ViewBag.UnreadNotifications` has data

---

### **Build errors?**

**Solution:**
```powershell
# In Package Manager Console
dotnet clean
dotnet restore
dotnet build
```

---

### **Table doesn't exist?**

**Solution:**
```sql
-- Make sure you're in TeamPro1 database
USE TeamPro1
GO

-- Re-run the script
-- (Copy from AddNotificationsTable.sql)
```

---

### **Team Formation closed?**

**Solution:**
1. Login as Admin
2. Go to: Schedule Team Formation
3. Open schedule for test students' year
4. Try again

---

## ?? Need Help?

### **Common Issues:**

1. **"Cannot open database TeamPro1"**
   - Solution: Check connection string in `appsettings.json`
   - Make sure database exists

2. **"Port 5001 is already in use"**
   - Solution: Stop all Visual Studio instances
   - Or change port in `launchSettings.json`

3. **"Notifications table already exists"**
   - This is OK! It means table was created before
   - Continue to next step

4. **"No students found"**
   - Solution: Create test students first
   - Or use admin panel to add students

---

## ?? Time Estimate

**Total Time: ~10 minutes**

- Step 1 (SQL): 2 min
- Step 2 (Verify): 1 min
- Step 3-5 (Rebuild): 1 min
- Step 6 (Test): 5 min
- Step 7 (Verify): 1 min

---

## ?? Reference Documents

All documentation is in: `TeamPro1\Documentation\`

1. **SETUP_GUIDE_NOTIFICATIONS.md** - Detailed setup guide
2. **NOTIFICATION_POPUP_IMPLEMENTATION.md** - Technical details
3. **This file** - Quick checklist

---

## ?? Ready to Start?

**Current Status:**
- ? Code is ready (Build successful)
- ? Scripts are ready
- ? Database table needs to be created (YOU DO THIS)
- ? Feature needs testing (YOU DO THIS)

**Start with STEP 1 above!** ??

---

**Good luck! You've got this! ??**

If you see the pop-up notification, **take a screenshot!** ??

---

_Last Updated: January 2025_
_Feature: Rejection Notification Pop-up_
_Status: Ready for Deployment_ ?
