# Quick Reference: Meeting Notification System

## ?? Files Created/Modified

### Created Files
1. **`Views/Shared/_NotificationBell.cshtml`**
   - Reusable notification bell component
   - Dropdown with invitations list
   - Accept/Decline functionality

2. **`Views/Student/MeetingInvitations.cshtml`**
   - Full page for managing meeting invitations
   - Filter tabs (All/Pending/Accepted/Declined)
   - Card-based UI with detailed information

3. **`Documentation/NOTIFICATION_SYSTEM_IMPLEMENTATION.md`**
   - Complete implementation guide
   - Features and benefits documentation

### Modified Files
1. **`Controllers/StudentController.cs`**
   - Added `GetMeetingInvitations()` - GET API
   - Added `RespondToMeetingInvite()` - POST API
   - Added `MeetingInvitations()` - Page view

2. **`Views/Student/Dashboard.cshtml`**
   - Added notification bell partial

3. **`Views/Student/MainDashboard.cshtml`**
   - Added notification bell partial

4. **`Views/Student/PoolOfStudents.cshtml`**
   - Added notification bell partial
   - Adjusted team requests bell position

5. **`Views/Student/StatusUpdate.cshtml`**
   - Added notification bell partial

---

## ?? API Endpoints

### GET `/Student/GetMeetingInvitations`
**Purpose**: Get all meeting invitations for student's team

**Returns**:
```json
{
  "success": true,
  "invitations": [
    {
      "Id": 1,
      "Title": "Project Review",
      "Description": "Discuss progress",
      "MeetingDateTime": "Jan 25, 2025 02:00 PM",
      "Location": "Room 301",
      "DurationMinutes": 30,
      "Status": "Pending",
      "FacultyName": "Dr. Smith",
      "MyResponse": "Pending",
      "CanRespond": true
    }
  ]
}
```

### POST `/Student/RespondToMeetingInvite`
**Purpose**: Accept or reject a meeting invitation

**Parameters**:
- `invitationId` (int) - Meeting invitation ID
- `response` (string) - "Accepted" or "Rejected"

**Returns**:
```json
{
  "success": true,
  "message": "You have accepted the meeting invitation."
}
```

### GET `/Student/MeetingInvitations`
**Purpose**: View full page with all meeting invitations

**Returns**: Razor view with all invitations

---

## ?? CSS Classes Reference

### Notification Bell
```css
.notification-container       /* Main container */
.notification-bell-icon      /* Bell icon */
.notification-badge          /* Badge with count */
.notification-dropdown       /* Dropdown panel */
```

### Invitation Cards
```css
.invitation-card             /* Card container */
.invitation-card.pending     /* Yellow border */
.invitation-card.accepted    /* Green border */
.invitation-card.rejected    /* Red border */
```

### Action Buttons
```css
.notification-btn           /* Base button */
.notification-btn-accept    /* Green accept button */
.notification-btn-reject    /* Red reject button */
```

---

## ?? JavaScript Functions

### Notification Bell Component
```javascript
toggleNotifications()        // Open/close dropdown
closeNotifications()         // Close dropdown
loadNotifications()          // Fetch and display invitations
updateNotificationBadge()    // Update badge count
respondToInvite(id, response) // Accept/Decline invitation
```

### Meeting Invitations Page
```javascript
loadMeetingInvitations()     // Load all invitations
displayInvitations()         // Render invitations
filterInvitations(filter)    // Filter by status
respondToInvitation(id, response) // Handle response
```

---

## ?? Usage Examples

### Add Notification Bell to Any Page
```html
@Html.Partial("_NotificationBell")
```

### Check If Student Has Pending Invitations
```csharp
var invitations = await _context.MeetingInvitations
    .Include(mi => mi.Team)
    .Where(mi => mi.TeamId == teamId && 
                 mi.Status == "Pending")
    .ToListAsync();

int pendingCount = invitations.Count;
```

### Send Meeting Invitation (Faculty)
```csharp
var invitation = new MeetingInvitation
{
    TeamId = teamId,
    FacultyId = facultyId,
    Title = "Project Review",
    Description = "Discuss your progress",
    MeetingDateTime = DateTime.Parse("2025-01-25 14:00"),
    Location = "Room 301",
    DurationMinutes = 30,
    Status = "Pending",
    Student1ResponseId = student1.Id,
    Student1Response = "Pending",
    Student2ResponseId = student2?.Id,
    Student2Response = student2 != null ? "Pending" : null
};

_context.MeetingInvitations.Add(invitation);
await _context.SaveChangesAsync();
```

---

## ?? Troubleshooting

### Notification Badge Not Showing
- Check if student has a team
- Verify meeting invitations exist in database
- Check browser console for JavaScript errors
- Ensure session has StudentId

### Can't Accept/Decline Invitation
- Verify anti-forgery token is present
- Check if student is part of the team
- Ensure invitation status is not "Cancelled"
- Check browser network tab for API errors

### Dropdown Not Opening
- Check for JavaScript conflicts
- Verify jQuery is loaded
- Check CSS z-index values
- Ensure no conflicting click handlers

### Badge Count Wrong
- Clear browser cache
- Check database for correct statuses
- Verify auto-refresh interval (30 seconds)
- Check console for fetch errors

---

## ?? Mobile Optimization

### Responsive Breakpoints
```css
/* Desktop: > 768px */
- Fixed position bell
- 380px dropdown width
- Multi-column grids

/* Mobile: ? 768px */
- Adjusted bell position
- Full-width dropdown
- Single-column cards
```

### Touch Targets
- Minimum 44x44px for buttons
- Increased padding on mobile
- Larger tap areas for actions

---

## ?? Color Scheme

```css
:root {
    --royal-blue: #274060;      /* Primary */
    --warm-gold: #FFC857;       /* Accent */
    --success-green: #28a745;   /* Accept */
    --error-red: #dc3545;       /* Decline */
    --warning-orange: #ff9800;  /* Pending */
}
```

---

## ?? Database Schema

### MeetingInvitations Table
```sql
Id INT PRIMARY KEY
TeamId INT FOREIGN KEY
FacultyId INT FOREIGN KEY
Title NVARCHAR(200)
Description NVARCHAR(MAX)
MeetingDateTime DATETIME
Location NVARCHAR(200)
DurationMinutes INT
Status NVARCHAR(50)
Student1ResponseId INT
Student1Response NVARCHAR(50)
Student2ResponseId INT
Student2Response NVARCHAR(50)
CreatedAt DATETIME
UpdatedAt DATETIME
```

---

## ?? Performance Tips

1. **Caching**: Badge count cached for 30 seconds
2. **Lazy Loading**: Invitations loaded on demand
3. **Pagination**: Consider for many invitations
4. **Indexing**: Database indexes on TeamId, Status
5. **Minification**: Minify CSS/JS for production

---

## ?? Security Checklist

- ? Anti-CSRF tokens on all POST requests
- ? Session validation for student identity
- ? Team membership verification
- ? SQL injection prevention (EF Core)
- ? XSS protection (HTML encoding)
- ? Authorization checks before responses

---

## ?? Quick Start

1. **Build the project**: `Ctrl + Shift + B`
2. **Run the application**: `F5`
3. **Login as student**
4. **Check notification bell** in top-right corner
5. **Faculty creates invitation** (from faculty dashboard)
6. **Badge appears** with count
7. **Click bell** to see details
8. **Accept or Decline** invitation
9. **View full page** via "View All Meetings" link

---

## ?? Support

For issues or questions, check:
- Documentation files in `/Documentation/`
- Code comments in modified files
- Build output window for errors
- Browser console for JavaScript errors

---

**Last Updated**: January 2025
**Version**: 1.0
**Status**: ? Production Ready
