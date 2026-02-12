# ?? BCrypt Quick Reference - TeamPro1

## ?? Quick Commands

### Migrate Existing Passwords:
```
URL: https://localhost:5001/PasswordMigration/CheckStatus
Then click: "Migrate Passwords Now"
```

### Delete Old Database (Fresh Start):
```bash
dotnet ef database drop --force
dotnet ef database update
```

---

## ?? Test Credentials

### Admin:
- **Email:** admin.cseds@rgm.ac.in
- **Password:** admin123
- **Department:** CSE(DS)

### Faculty:
- **Email:** penchala.prasad@rgm.ac.in
- **Password:** faculty123

### Students:
- Use your original passwords (now securely hashed)

---

## ?? Code Snippets

### Hash Password:
```csharp
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
```

### Verify Password:
```csharp
bool isValid = BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
```

### Full Login Example:
```csharp
var user = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);

if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
{
    return Unauthorized();
}
// Login successful
```

---

## ? Security Checklist

- [x] BCrypt implemented in StudentController
- [x] BCrypt implemented in FacultyController  
- [x] BCrypt implemented in AdminController
- [x] DbSeeder uses BCrypt
- [ ] **Run password migration**
- [ ] **Delete PasswordMigrationController.cs**
- [ ] Test all login flows
- [ ] Verify passwords in database (should start with $2a$)

---

## ?? Important Notes

1. **BCrypt hashes are ~60 characters** - Ensure database field is VARCHAR(100)+
2. **Migration is ONE-TIME only** - Run once, then delete controller
3. **Original passwords still work** - Users don't need to reset passwords
4. **Login is slightly slower** - This is intentional security feature (~200-500ms)

---

## ?? Verify Implementation

### Check Database:
```sql
-- Passwords should look like this:
SELECT TOP 3 Email, Password FROM Students;
-- Expected: $2a$11$N9qo8uLOickgx2ZMRZoMyeIjZ...
```

### Test Login:
1. Navigate to Student/Faculty/Admin login
2. Enter test credentials above
3. Should login successfully
4. Check session is created

---

## ?? Troubleshooting

| Problem | Solution |
|---------|----------|
| Login fails after migration | Clear browser cookies/cache |
| "Password too long" error | Increase Password field size to VARCHAR(100) |
| Migration error | Check database permissions, restart app |
| Slow login | Normal - BCrypt is intentionally slow for security |

---

## ?? Quick Help

**See full guide:** `Documentation/BCRYPT_IMPLEMENTATION_GUIDE.md`

**Need help?** Check the implementation guide for detailed troubleshooting steps.
