# ?? PRODUCTION SECURITY AUDIT - Files to Review/Remove

## ? **ALREADY DELETED (SECURE)**
- ? `PasswordMigrationController.cs` - REMOVED ?
- ? `Views/PasswordMigration/Index.cshtml` - REMOVED ?

---

## ?? **HIGH PRIORITY - DELETE BEFORE PRODUCTION**

### 1. **SeedController.cs** ?? CRITICAL
**Location:** `TeamPro1/Controllers/SeedController.cs`

**Risk:** Allows anyone to access `/Seed/Faculty` and seed database with test data

**Action:** DELETE THIS FILE

```bash
# Delete command:
Remove-Item TeamPro1/Controllers/SeedController.cs
```

**Why it's dangerous:**
- Publicly accessible endpoint
- Can populate database with test accounts
- No authentication required
- Exposes test credentials

---

### 2. **Seed View** ?? CRITICAL
**Location:** `TeamPro1/Views/Seed/Faculty.cshtml`

**Action:** DELETE THIS FILE

```bash
# Delete command:
Remove-Item TeamPro1/Views/Seed/Faculty.cshtml
```

---

### 3. **Test SQL Scripts** ?? HIGH RISK
**Location:** `TeamPro1/Scripts/InsertTestFaculty.sql`

**Action:** DELETE THIS FILE (Contains plain-text test passwords!)

```bash
# Delete command:
Remove-Item TeamPro1/Scripts/InsertTestFaculty.sql
```

**Contents expose:**
- ? Plain-text passwords: `test123`, `password123`
- ? Test email addresses
- ? Direct SQL injection risk

---

## ?? **MEDIUM PRIORITY - SECURE/MODIFY**

### 4. **DbSeeder.cs** - MODIFY (Don't Delete)
**Location:** `TeamPro1/Data/DbSeeder.cs`

**Current Risk:**
- Runs automatically on app startup
- Seeds test faculty with known passwords

**Action:** MODIFY `Program.cs` to disable auto-seeding

**In `Program.cs`, change:**
```csharp
// ? INSECURE (currently):
await DbSeeder.SeedTestFacultyAsync(db);

// ? SECURE (change to):
// await DbSeeder.SeedTestFacultyAsync(db); // Disabled for production
```

**OR use environment check:**
```csharp
if (app.Environment.IsDevelopment())
{
    await DbSeeder.SeedTestFacultyAsync(db);
}
```

---

### 5. **Error Pages** - SECURE
**Location:** 
- `TeamPro1/Views/Shared/Error.cshtml`
- `TeamPro1/Views/Shared/Shared/Error.cshtml` (duplicate?)

**Risk:** Exposes detailed error messages in production

**Action:** CHECK `Program.cs` has proper error handling:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
```

**Also remove duplicate:** `Views/Shared/Shared/Error.cshtml`

---

## ?? **KEEP BUT SECURE - Configuration Files**

### 6. **appsettings.json** - SECURE
**Action:** Ensure no sensitive data in source control

**Check for:**
- ? Production connection strings
- ? API keys
- ? Passwords
- ? Secret tokens

**Recommended:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TeamPro1;..."
  }
}
```

**For Production, use:**
- Azure Key Vault
- Environment variables
- User Secrets (in development)

---

### 7. **appsettings.Development.json**
**Action:** Ensure this file is in `.gitignore`

**Check `.gitignore` contains:**
```
appsettings.Development.json
appsettings.Production.json
*.user
*.suo
```

---

## ?? **DOCUMENTATION FILES - KEEP (Safe)**

These files are **documentation only** and safe to keep:
- ? `Documentation/BCRYPT_*.md` - All BCrypt guides
- ? `Documentation/MIGRATION_*.md` - Migration guides  
- ? `Documentation/IMPLEMENTATION_COMPLETE.md`
- ? `Documentation/FIX_TEAMMEETINGS_TABLE.md`

**Note:** You can delete these if you want, but they pose no security risk.

---

## ??? **DATABASE MIGRATION FILES - KEEP**

**Location:** `TeamPro1/Migrations/*.cs`

**Action:** KEEP ALL MIGRATION FILES

**Why they're safe:**
- Required for Entity Framework database updates
- Don't contain sensitive data
- Necessary for production deployments

---

## ?? **PROGRAM.CS - SECURE**

**Current Status:** ? Already secure (but check auto-seeding)

**Required Changes:**
```csharp
// ? Remove or comment out:
await DbSeeder.SeedTestFacultyAsync(db);

// ? OR wrap in environment check:
if (app.Environment.IsDevelopment())
{
    await DbSeeder.SeedTestFacultyAsync(db);
}
```

---

## ?? **SECURITY CHECKLIST FOR PRODUCTION**

### Before Deployment:
- [ ] Delete `SeedController.cs`
- [ ] Delete `Views/Seed/Faculty.cshtml`
- [ ] Delete `Scripts/InsertTestFaculty.sql`
- [ ] Disable auto-seeding in `Program.cs`
- [ ] Remove duplicate `Views/Shared/Shared/Error.cshtml`
- [ ] Verify `appsettings.json` has no secrets
- [ ] Check `.gitignore` includes sensitive files
- [ ] Change all default admin passwords in database
- [ ] Enable HTTPS enforcement
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Test error pages don't reveal sensitive info

### Database Security:
- [ ] All passwords are BCrypt hashed ? (Done)
- [ ] Remove any test accounts from production database
- [ ] Change database connection string to production
- [ ] Use strong SQL Server authentication
- [ ] Enable database encryption (TDE)
- [ ] Regular database backups configured

### Application Security:
- [ ] HTTPS only (no HTTP)
- [ ] Secure cookies enabled
- [ ] CORS properly configured
- [ ] SQL injection prevention (using Entity Framework ?)
- [ ] XSS protection enabled
- [ ] CSRF tokens on all forms ? (Done)

---

## ?? **QUICK DELETE COMMANDS**

Run these commands in PowerShell to remove security-sensitive files:

```powershell
# Navigate to project root
cd C:\Users\shahi\Source\Repos\Acadamic1\TeamPro1

# Delete SeedController
Remove-Item .\Controllers\SeedController.cs -Force

# Delete Seed View
Remove-Item .\Views\Seed\Faculty.cshtml -Force

# Delete Test SQL Script
Remove-Item .\Scripts\InsertTestFaculty.sql -Force

# Delete duplicate Error view (if exists)
Remove-Item .\Views\Shared\Shared\Error.cshtml -Force -ErrorAction SilentlyContinue

# Verify deletions
Write-Host "? Security-sensitive files removed!"
```

---

## ?? **RISK ASSESSMENT SUMMARY**

| File/Component | Risk Level | Action | Status |
|----------------|------------|--------|--------|
| PasswordMigrationController | ?? CRITICAL | DELETE | ? Done |
| SeedController | ?? CRITICAL | DELETE | ?? TODO |
| Seed Views | ?? CRITICAL | DELETE | ?? TODO |
| InsertTestFaculty.sql | ?? HIGH | DELETE | ?? TODO |
| DbSeeder Auto-Run | ?? HIGH | DISABLE | ?? TODO |
| Error Pages | ?? MEDIUM | CHECK | ?? Review |
| Documentation | ?? LOW | OPTIONAL | ? Safe |
| Migrations | ?? SAFE | KEEP | ? Safe |

---

## ?? **RECOMMENDED IMMEDIATE ACTIONS**

### Priority 1 (Do Now):
```powershell
# 1. Delete SeedController
Remove-Item TeamPro1\Controllers\SeedController.cs

# 2. Delete Seed View
Remove-Item TeamPro1\Views\Seed\Faculty.cshtml

# 3. Delete Test SQL
Remove-Item TeamPro1\Scripts\InsertTestFaculty.sql
```

### Priority 2 (Before First Production Deploy):
1. Disable auto-seeding in `Program.cs`
2. Remove test accounts from database
3. Change all default passwords
4. Verify error handling is secure
5. Test with `ASPNETCORE_ENVIRONMENT=Production`

### Priority 3 (Production Hardening):
1. Enable Azure Key Vault for secrets
2. Configure Application Insights
3. Set up monitoring/alerts
4. Enable database encryption
5. Configure backup strategy

---

## ?? **TEST ACCOUNTS TO REMOVE FROM PRODUCTION DB**

**After deployment, run this SQL to remove test accounts:**

```sql
-- Remove test faculty accounts
DELETE FROM Faculties WHERE Email IN (
    'faculty@test.com',
    'john.doe@rgmcet.edu',
    'jane.smith@rgmcet.edu',
    'robert.brown@rgmcet.edu'
);

-- Remove test students (if any with @test.com)
DELETE FROM Students WHERE Email LIKE '%@test.com';

-- Verify no test accounts remain
SELECT * FROM Faculties WHERE Email LIKE '%test%';
SELECT * FROM Students WHERE Email LIKE '%test%';
SELECT * FROM Admins WHERE Email LIKE '%test%';
```

---

## ?? **SECURITY SUPPORT**

### If You Find More Security Issues:
1. Check for hardcoded passwords: `git grep -i "password.*=.*\""`
2. Search for API keys: `git grep -i "api.key"`
3. Look for connection strings: `git grep -i "connection.string"`

### Resources:
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Azure Security Best Practices](https://docs.microsoft.com/en-us/azure/security/)

---

## ? **AFTER CLEANUP CHECKLIST**

Once you've removed the files above:

```bash
# 1. Build to verify no errors
dotnet build

# 2. Run the application
dotnet run

# 3. Test that seed endpoints are gone
# Try accessing: https://localhost:5001/Seed/Faculty
# Should return 404 Not Found

# 4. Commit changes
git add .
git commit -m "Remove security-sensitive development files"
git push

# 5. Deploy to production
```

---

**?? Your application will be production-ready and secure once these files are removed!**
