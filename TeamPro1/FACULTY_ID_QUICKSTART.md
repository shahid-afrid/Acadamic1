# ? FACULTY ID - QUICK START (2 Minutes)

## ? Pre-Check
- [x] Code files created
- [x] Project builds successfully
- [ ] **YOU NEED TO DO:** Run SQL script

---

## ?? DO THIS NOW (3 Steps)

### 1?? **Stop the App** (5 seconds)
```
Visual Studio ? Press Shift+F5
```

---

### 2?? **Run SQL Script** (1 minute)

**Open SSMS:**
- Connect to: `(localdb)\mssqllocaldb`
- Select database: **TeamPro1**
- Open file: `TeamPro1\Scripts\AddFacultyIdColumn.sql`
- Press **F5** to execute

**Expected Output:**
```
? FacultyId column added
? FacultyId values generated for existing faculty
? FacultyId column set to NOT NULL
? Unique index created on FacultyId
??? FacultyId column setup complete! ???
```

---

### 3?? **Run the App** (10 seconds)
```
Visual Studio ? Press F5
```

---

## ?? TEST IT (30 seconds)

1. **Login as Admin**: `admin.cseds@rgmcet.edu` / `admin123`
2. **Go to**: Manage Faculty
3. **? Verify**: You see "Faculty ID" column (e.g., 3201, 3202)
4. **? Test Add**: Click "Add Faculty" ? Fill details ? Submit
5. **? Check**: Success message shows Faculty ID (e.g., "Faculty ID: 3203")

---

## ?? SQL Script Locations

**Required (Run this):**
```
C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts\AddFacultyIdColumn.sql
```

**Optional (For verification):**
```
C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1\Scripts\VerifyFacultyIdSetup.sql
```

---

## ?? What You Get

| Before | After |
|--------|-------|
| Students: ID (1, 2, 3) | Students: RegdNumber (23091A32D4) |
| Faculty: ID (1, 2, 3) | Faculty: FacultyId (3201, 3202) |
| Admin: ID (1, 2, 3) | Admin: Email (no visible ID) |

---

## ?? Faculty ID Format

| Department | IDs |
|------------|-----|
| CSE(DS) | 3201, 3202, 3203... |
| CSE(AI&ML) | 3301, 3302, 3303... |
| Computer Science | 3101, 3102, 3103... |
| ECE | 4101, 4102, 4103... |
| EEE | 4201, 4202, 4203... |

---

## ?? If Something Goes Wrong

**Problem**: SQL script fails
**Fix**: 
1. Make sure you selected **TeamPro1** database (not master!)
2. Check if app is stopped

**Problem**: Build errors
**Fix**:
1. Clean Solution (Build ? Clean Solution)
2. Rebuild Solution (Build ? Rebuild Solution)

---

## ? Done!

Once the SQL script runs successfully, you're all set! The Faculty ID system will work automatically from now on.

**What happens automatically:**
- ? New faculty get IDs when added
- ? IDs display in all faculty pages
- ? Excel exports show Faculty IDs
- ? No manual ID entry needed

?? **You're ready to go!**
