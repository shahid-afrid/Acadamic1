# ?? Quick Start Guide - Meeting Notification System

## ? 5-Minute Setup

### Step 1: Run Database Migration (1 minute)

Open **SQL Server Management Studio** and run:

```sql
-- File: TeamPro1/Scripts/AddMeetingInvitationsTable.sql
-- Just execute the entire script in SSMS
```

Or use Entity Framework:

```bash
Add-Migration AddMeetingInvitations
Update-Database
```

---

### Step 2: Add Meeting Invite Button to Faculty Page (2 minutes)

**File**: `Views/Faculty/TeamDetails.cshtml`

Find the section with "Activity Logs" button and add:

```html
<button type="button" class="glass-btn" 
        style="background: linear-gradient(135deg, #6f42c1, #8e44ad);"
        onclick="showMeetingInviteModal(@team.Id)">
    <i class="fas fa-calendar-plus"></i> Send Meeting Invite
</button>
```

---

### Step 3: Add Meeting Invite Modal (2 minutes)

**File**: `Views/Faculty/TeamDetails.cshtml`

Copy the complete modal HTML from the documentation file at the end of the page (before `</body>`).

**Full modal code is in**: `Documentation/MEETING_NOTIFICATION_SYSTEM.md` (Section 2)

---

## ?? What You Get

### Faculty Features:
? **Send meeting invites** with title, agenda, date, location  
? **Track responses** - see who accepted/rejected  
? **Cancel meetings** if plans change  
? **View meeting history** - all past and upcoming meetings  

### Student Features:
? **Notification badge** - shows unread meeting invites  
? **Accept/Reject** - respond to invites with one click  
? **Meeting details** - see full agenda, location, time  
? **Meeting calendar** - view all upcoming meetings  

---

## ?? How It Works

### Workflow:

```
1. Faculty clicks "Send Meeting Invite" button
    ?
2. Fills form: Title, Agenda, Date/Time, Location
    ?
3. Clicks "Send Invitation"
    ?
4. Students receive notification (?? badge appears)
    ?
5. Students click notification bell ? see invite
    ?
6. Students click "Accept" or "Decline"
    ?
7. Faculty sees response status on Team Details page
```

---

## ?? UI Preview

### Faculty - Team Details Page

```
??????????????????????????????????????????????????????????
?  Team #1007 Details                                    ?
?  [Activity Logs]  [?? Send Meeting Invite]            ?
??????????????????????????????????????????????????????????
?  ?? Upcoming Meetings                                  ?
?  ??????????????????????????????????????????????????   ?
?  ? ? Project Review - Feb 15, 10:00 AM           ?   ?
?  ?    Responses: ? Student 1  ? Student 2         ?   ?
?  ?    [View] [Cancel]                              ?   ?
?  ??????????????????????????????????????????????????   ?
??????????????????????????????????????????????????????????
```

### Student - Notification Dropdown

```
??????????????????????????????????????????????
?  ?? Notifications                    [?]  ?
??????????????????????????????????????????????
?  ?? Meeting Invite from Dr. Prasad         ?
?     Project Progress Review                ?
?     Feb 15, 2025 at 10:00 AM              ?
?     [? Accept]  [? Decline]               ?
??????????????????????????????????????????????
```

---

## ?? API Endpoints

### Faculty:
- `POST /Faculty/SendMeetingInvite` - Send invite
- `GET /Faculty/GetMeetingInvitations` - View invites
- `POST /Faculty/CancelMeetingInvite` - Cancel invite

### Student:
- `GET /Student/GetMeetingInvitations` - View invites
- `POST /Student/RespondToMeetingInvite` - Accept/Reject

---

## ??? Files Created/Modified

### New Files:
1. `Models/MeetingInvitation.cs` ?
2. `Scripts/AddMeetingInvitationsTable.sql` ?
3. `Documentation/MEETING_NOTIFICATION_SYSTEM.md` ?
4. `Documentation/MEETING_NOTIFICATION_DESIGN_SUMMARY.md` ?

### Modified Files:
1. `Models/AppDbContext.cs` - Added `MeetingInvitations` DbSet ?
2. `Controllers/FacultyController.cs` - Added invite methods ?

### Files to Modify (UI):
1. `Views/Faculty/TeamDetails.cshtml` - Add button & modal ?
2. `Views/Student/MainDashboard.cshtml` - Add notification icon ?
3. Create `Views/Student/MeetingInvitations.cshtml` - New page ?

---

## ?? Database Schema

```sql
CREATE TABLE MeetingInvitations (
    Id INT PRIMARY KEY,
    TeamId INT NOT NULL,
    FacultyId INT NOT NULL,
    Title NVARCHAR(200),
    Description NVARCHAR(1000),
    MeetingDateTime DATETIME2,
    Location NVARCHAR(200),
    DurationMinutes INT,
    Status NVARCHAR(50),  -- Pending/Accepted/Rejected/Cancelled
    Student1ResponseId INT,
    Student1Response NVARCHAR(50),  -- Accepted/Rejected/Pending
    Student2ResponseId INT,
    Student2Response NVARCHAR(50),
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2
);
```

---

## ? Testing Checklist

After implementation, test these scenarios:

### Faculty:
- [ ] Can send meeting invite
- [ ] Modal opens and closes properly
- [ ] Form validation works
- [ ] Success message appears
- [ ] Can view sent invites
- [ ] Can cancel invites

### Student:
- [ ] Notification badge appears
- [ ] Can view meeting details
- [ ] Can accept invite
- [ ] Can reject invite
- [ ] Badge count updates
- [ ] Notification disappears after responding

### System:
- [ ] Database records created correctly
- [ ] Activity logs recorded
- [ ] Both students receive notifications
- [ ] Status updates properly

---

## ?? Common Issues

### Issue: "Table does not exist"
**Fix**: Run the SQL script first

### Issue: Button not appearing
**Fix**: Check if you added button in correct location

### Issue: Modal not showing
**Fix**: Verify JavaScript is included

### Issue: Notification badge not updating
**Fix**: Check if API endpoint is accessible

---

## ?? Full Documentation

For complete details, see:
- `Documentation/MEETING_NOTIFICATION_SYSTEM.md` - Full implementation guide
- `Documentation/MEETING_NOTIFICATION_DESIGN_SUMMARY.md` - Design overview

---

## ?? You're Done!

After completing these steps, your meeting notification system will be fully functional!

**Key Benefits:**
- ? Professional meeting scheduling
- ? Real-time notifications
- ? Easy accept/reject workflow
- ? Complete audit trail
- ? Mobile-friendly design

---

**Need Help?** Check the full documentation files for detailed examples and troubleshooting!
