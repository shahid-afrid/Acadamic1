# ? FACULTY ID SETUP - COMPLETE GUIDE

## ?? Overview
This implements a hierarchical ID system for Faculty:
- **Format**: DepartmentCode + Sequential Number
- **Example**: CSE(DS) faculty get IDs: 3201, 3202, 3203...

---

## ??? Department Code Mapping

| Department | Admin Code | Faculty ID Range |
|------------|------------|------------------|
| CSE(DS) | 32 | 3201 - 3299 |
| CSE(AI&ML) | 33 | 3301 - 3399 |
| Computer Science | 31 | 3101 - 3199 |
| ECE | 41 | 4101 - 4199 |
| EEE | 42 | 4201 - 4299 |
| Mechanical | 51 | 5101 - 5199 |
| Civil | 61 | 6101 - 6199 |
| IT | 34 | 3401 - 3499 |

---

## ?? SETUP STEPS (5 Minutes)

### Step 1: Stop the Application ??
```
Press Shift + F5 in Visual Studio
OR
Click the red "Stop" button
```

---

### Step 2: Run SQL Script ??

**Open SQL Server Management Studio (SSMS):**

1. **Connect to Database:**
   - Server: `(localdb)\mssqllocaldb`
   - Authentication: `Windows Authentication`
   - Click **Connect**

2. **Select Database:**
   - In the dropdown at top, select: **TeamPro1**

3. **Open Script:**
   - File ? Open ? File...
   - Navigate to: `C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts\AddFacultyIdColumn.sql`
   - Click Open

4. **Execute:**
   - Press `F5` OR click green "Execute" button
   - Wait for completion (should take ~5 seconds)

5. **Verify Output:**
   Look for these messages:
   ```
   ? FacultyId column added
   ? FacultyId values generated for existing faculty
   ? FacultyId column set to NOT NULL
   ? Unique index created on FacultyId
   ??? FacultyId column setup complete! ???
   ```

6. **Check Faculty Assignments:**
   You should see a table showing all faculty with their new IDs, like:
   ```
   FacultyId | FullName           | Department
   3201      | Dr. John Doe       | CSE(DS)
   3202      | Dr. Jane Smith     | CSE(DS)
   3301      | Prof. Robert Brown | CSE(AI&ML)
   ```

---

### Step 3: Build the Project ??

**In Visual Studio:**

1. **Clean Solution:**
   ```
   Menu: Build ? Clean Solution
   Wait for: "Clean succeeded"
   ```

2. **Rebuild Solution:**
   ```
   Menu: Build ? Rebuild Solution
   Wait for: "Rebuild succeeded"
   ```

3. **Check for Errors:**
   ```
   Menu: View ? Error List
   Should show: 0 Errors, 0 Warnings
   ```

---

### Step 4: Run & Test ??

1. **Start Application:**
   - Press `F5`
   - Wait for browser to open

2. **Test as Admin:**
   - Login as Admin (e.g., `admin.cseds@rgmcet.edu` / `admin123`)
   - Go to: **Manage Faculty**
   - **Verify**: You see "Faculty ID" column (e.g., 3201, 3202)
   - **Add New Faculty**: Click "Add Faculty"
     - Fill in details
     - Submit
     - **Verify**: Success message shows the auto-generated Faculty ID
     - **Example**: "Faculty added successfully! Faculty ID: 3203"

3. **Test as Faculty:**
   - Logout from Admin
   - Login as Faculty
   - Go to: **Profile**
   - **Verify**: You see your Faculty ID prominently displayed

4. **Test Excel Export:**
   - As Admin, go to: **Manage Faculty**
   - Click: **Export Excel**
   - **Verify**: Excel file shows "Faculty ID" column instead of "S.No"

---

## ? Files Modified

### 1. **Model**
- ? `TeamPro1/Models/Faculty.cs` - Added `FacultyId` field

### 2. **Utility**
- ? `TeamPro1/Utilities/FacultyIdGenerator.cs` - Auto-generation logic

### 3. **Controller**
- ? `TeamPro1/Controllers/AdminController.cs`
  - `AddFaculty()` - Auto-generates FacultyId
  - `ExportFacultyExcel()` - Shows FacultyId in export

### 4. **Views**
- ? `TeamPro1/Views/Admin/ManageFaculty.cshtml` - Shows FacultyId column
- ? `TeamPro1/Views/Faculty/Profile.cshtml` - Displays FacultyId

### 5. **Database Script**
- ? `TeamPro1/Scripts/AddFacultyIdColumn.sql` - Adds column & generates IDs

---

## ?? Verification Checklist

- [ ] SQL script executed successfully
- [ ] Project builds with 0 errors
- [ ] Manage Faculty page shows "Faculty ID" column
- [ ] Adding new faculty auto-generates ID
- [ ] Faculty Profile shows Faculty ID
- [ ] Excel export includes Faculty ID
- [ ] All faculty have IDs matching their department code

---

## ?? Sample Faculty IDs by Department

### CSE(DS) Department (Code: 32)
```
3201 - Dr. P. Penchala Prasad
3202 - Dr. Faculty Name 2
3203 - Dr. Faculty Name 3
```

### CSE(AI&ML) Department (Code: 33)
```
3301 - First Faculty
3302 - Second Faculty
```

### Computer Science Department (Code: 31)
```
3101 - First Faculty
3102 - Second Faculty
```

---

## ?? Troubleshooting

### Problem: "FacultyId column already exists"
**Solution**: The column was added previously. Just verify existing faculty have IDs:
```sql
SELECT FacultyId, FullName, Department FROM Faculties ORDER BY FacultyId;
```

### Problem: "Build failed"
**Solution**: 
1. Make sure app is stopped
2. Clean solution
3. Rebuild solution

### Problem: "Faculty ID not showing in UI"
**Solution**: 
1. Refresh browser (Ctrl + F5)
2. Clear browser cache
3. Restart application

### Problem: "Null FacultyId for new faculty"
**Solution**: 
1. Check if SQL script was run
2. Verify `Faculty.cs` has `FacultyId` property
3. Rebuild solution

---

## ?? Success Indicators

? **Database**: All faculty have unique FacultyIds
? **UI**: Faculty ID visible in all faculty pages
? **Auto-Generation**: New faculty get IDs automatically
? **Export**: Excel shows Faculty ID column
? **Format**: IDs match pattern (e.g., 3201, 3202 for CSE(DS))

---

## ?? Notes

- **Registration Number** is for **Students** only (e.g., 23091A32D4)
- **Faculty ID** is for **Faculty** only (e.g., 3201)
- **Admin** uses email for login (no special ID displayed to users)
- IDs are **auto-generated** - admins don't need to enter them manually
- **Sequential numbering** within each department (01, 02, 03...)

---

## ?? You're Done!

Faculty ID system is now fully implemented. Each faculty member has a unique, department-based ID that's automatically generated when they're added to the system.

**Next Steps:**
- ? S.No removed from student tables
- ? Faculty ID implemented
- ?? Students use Registration Number as their identifier

All three user types now have proper, meaningful IDs! ??
