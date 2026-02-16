# ?? Meeting Notification System - Implementation Guide

## Overview

This feature allows faculty to send meeting invitations to teams, and students can accept or reject them.

---

## ?? Features Implemented

### For Faculty:
1. **Send Meeting Invites** - Schedule meetings with teams
2. **View Meeting Responses** - See which students accepted/rejected
3. **Cancel Meetings** - Cancel sent invitations
4. **Notification Icon** - Shows unread meeting invites count
5. **Meeting History** - View all past and upcoming meetings

### For Students:
1. **Receive Meeting Invites** - Get notifications when faculty sends invite
2. **Accept/Reject Invites** - Respond to meeting requests
3. **Meeting Calendar** - View all upcoming meetings
4. **Notification Badge** - Shows unread meeting invites
5. **Meeting Details** - View location, time, agenda

---

## ?? Database Structure

### MeetingInvitations Table

```sql
- Id (Primary Key)
- TeamId (Foreign Key ? Teams)
- FacultyId (Foreign Key ? Faculties)
- Title (Meeting title/subject)
- Description (Meeting agenda/details)
- MeetingDateTime (Date and time)
- Location (Meeting venue - online/offline)
- DurationMinutes (Meeting duration)
- Status (Pending/Accepted/Rejected/Cancelled/Completed)
- Student1ResponseId
- Student1Response (Accepted/Rejected/Pending)
- Student2ResponseId (nullable)
- Student2Response (Accepted/Rejected/Pending/null)
- CreatedAt
- UpdatedAt
```

### Key Fields:

- **Status** tracks the overall meeting status:
  - `Pending` - Waiting for student responses
  - `Accepted` - All students accepted
  - `Rejected` - At least one student rejected
  - `Cancelled` - Faculty cancelled
  - `Completed` - Meeting took place

- **Student Responses** track individual acceptance:
  - `Pending` - No response yet
  - `Accepted` - Student will attend
  - `Rejected` - Student cannot attend

---

## ??? Setup Instructions

### Step 1: Run Database Migration

**Option A: Using SQL Server Management Studio (Recommended)**

1. Open **SQL Server Management Studio (SSMS)** or **Azure Data Studio**
2. Connect to your database server
3. Open the script: `TeamPro1/Scripts/AddMeetingInvitationsTable.sql`
4. Execute the script (F5)
5. Verify the table was created:
   ```sql
   SELECT * FROM MeetingInvitations;
   ```

**Option B: Using Entity Framework Migrations**

```bash
# In Package Manager Console:
Add-Migration AddMeetingInvitations
Update-Database

# Or using .NET CLI:
dotnet ef migrations add AddMeetingInvitations --project TeamPro1
dotnet ef database update --project TeamPro1
```

---

## ?? UI Components to Add

### 1. Faculty Team Details Page - Add Meeting Invite Button

Location: `Views/Faculty/TeamDetails.cshtml`

Add this button next to "Activity Logs" button:

```html
<button type="button" class="glass-btn" 
        style="background: linear-gradient(135deg, #6f42c1, #8e44ad); color: white;"
        onclick="showMeetingInviteModal(@team.Id)">
    <i class="fas fa-calendar-plus"></i> Send Meeting Invite
</button>
```

### 2. Meeting Invite Modal (Faculty)

Add this modal at the end of `TeamDetails.cshtml`:

```html
<!-- Meeting Invite Modal -->
<div id="meetingInviteModal" class="modal-overlay" style="display: none;">
    <div class="modal-content">
        <div class="modal-header" style="background: linear-gradient(135deg, #6f42c1, #8e44ad);">
            <h3><i class="fas fa-calendar-plus"></i> Send Meeting Invitation</h3>
            <button type="button" class="modal-close" onclick="hideMeetingInviteModal()">
                <i class="fas fa-times"></i>
            </button>
        </div>
        <form id="meetingInviteForm">
            @Html.AntiForgeryToken()
            <input type="hidden" id="meetingTeamId" name="teamId" />
            
            <div class="modal-body" style="padding: 30px;">
                <div class="form-group">
                    <label><i class="fas fa-heading"></i> Meeting Title *</label>
                    <input type="text" name="title" class="form-control" 
                           placeholder="e.g., Project Progress Review" required />
                </div>

                <div class="form-group">
                    <label><i class="fas fa-align-left"></i> Description / Agenda</label>
                    <textarea name="description" class="form-control" rows="3" 
                              placeholder="Meeting purpose and topics to discuss..."></textarea>
                </div>

                <div class="form-group">
                    <label><i class="fas fa-calendar-alt"></i> Meeting Date & Time *</label>
                    <input type="datetime-local" name="meetingDateTime" class="form-control" required />
                </div>

                <div class="form-group">
                    <label><i class="fas fa-map-marker-alt"></i> Location</label>
                    <input type="text" name="location" class="form-control" 
                           placeholder="e.g., Faculty Room / Online (Google Meet)" />
                </div>

                <div class="form-group">
                    <label><i class="fas fa-clock"></i> Duration (minutes)</label>
                    <input type="number" name="durationMinutes" class="form-control" 
                           value="60" min="15" max="480" />
                    <small class="form-text">Meeting duration in minutes (15 min to 8 hours)</small>
                </div>
            </div>

            <div class="modal-footer" style="padding: 20px 30px; border-top: 2px solid var(--slate);">
                <button type="button" class="btn-secondary" onclick="hideMeetingInviteModal()">
                    Cancel
                </button>
                <button type="submit" class="glass-btn" 
                        style="background: linear-gradient(135deg, #6f42c1, #8e44ad);">
                    <i class="fas fa-paper-plane"></i> Send Invitation
                </button>
            </div>
        </form>
    </div>
</div>
```

### 3. JavaScript for Meeting Invite Modal

Add this JavaScript at the end of the page:

```javascript
function showMeetingInviteModal(teamId) {
    document.getElementById('meetingTeamId').value = teamId;
    
    // Set default meeting date/time to tomorrow at 10:00 AM
    var tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    tomorrow.setHours(10, 0, 0, 0);
    var datetimeStr = tomorrow.toISOString().slice(0, 16);
    document.querySelector('input[name="meetingDateTime"]').value = datetimeStr;
    
    document.getElementById('meetingInviteModal').style.display = 'flex';
}

function hideMeetingInviteModal() {
    document.getElementById('meetingInviteModal').style.display = 'none';
    document.getElementById('meetingInviteForm').reset();
}

// Handle meeting invite form submission
document.getElementById('meetingInviteForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    var formData = new FormData(this);
    
    try {
        var response = await fetch('@Url.Action("SendMeetingInvite", "Faculty")', {
            method: 'POST',
            body: formData
        });
        
        var result = await response.json();
        
        if (result.success) {
            showAlert(result.message, true);
            hideMeetingInviteModal();
            // Optionally reload or update meeting list
        } else {
            showAlert(result.message, false);
        }
    } catch (error) {
        showAlert('Error sending meeting invitation. Please try again.', false);
    }
});

// Close modal on outside click
document.getElementById('meetingInviteModal')?.addEventListener('click', function(e) {
    if (e.target.id === 'meetingInviteModal') hideMeetingInviteModal();
});
```

### 4. Notification Icon with Badge (Faculty & Student)

Add to the navigation bar:

```html
<div class="notification-icon" style="position: relative; cursor: pointer;" 
     onclick="toggleNotificationDropdown()">
    <i class="fas fa-bell" style="font-size: 1.5em;"></i>
    <span id="notificationBadge" class="notification-badge" 
          style="display: none; position: absolute; top: -8px; right: -8px; background: red; color: white; border-radius: 50%; width: 20px; height: 20px; font-size: 0.7em; display: flex; align-items: center; justify-content: center;">
        0
    </span>
</div>
```

### 5. Notification Dropdown

```html
<div id="notificationDropdown" class="notification-dropdown" style="display: none; position: absolute; top: 60px; right: 20px; width: 350px; max-height: 400px; overflow-y: auto; background: white; border-radius: 12px; box-shadow: 0 8px 32px rgba(0,0,0,0.2); z-index: 10000;">
    <div style="padding: 15px; border-bottom: 2px solid #eee; background: linear-gradient(135deg, #6f42c1, #8e44ad); color: white; border-radius: 12px 12px 0 0;">
        <h4 style="margin: 0; display: flex; align-items: center; gap: 8px;">
            <i class="fas fa-bell"></i> Notifications
        </h4>
    </div>
    <div id="notificationList" style="padding: 10px;">
        <!-- Notifications will be loaded here -->
        <p style="text-align: center; color: #999; padding: 20px;">No new notifications</p>
    </div>
</div>
```

---

## ?? Student Controller Methods

Add these methods to `StudentController.cs`:

```csharp
// GET: Student/GetMeetingInvitations
public async Task<IActionResult> GetMeetingInvitations()
{
    var studentIdString = HttpContext.Session.GetString("StudentId");
    if (string.IsNullOrEmpty(studentIdString))
    {
        return Json(new { success = false, message = "Please login first." });
    }

    try
    {
        if (!int.TryParse(studentIdString, out int studentId))
        {
            return Json(new { success = false, message = "Invalid session." });
        }

        // Find team where student is a member
        var team = await _context.Teams
            .FirstOrDefaultAsync(t => t.Student1Id == studentId || t.Student2Id == studentId);

        if (team == null)
        {
            return Json(new { success = true, invitations = new List<object>() });
        }

        // Get all meeting invitations for this team
        var invitations = await _context.MeetingInvitations
            .Include(mi => mi.Faculty)
            .Where(mi => mi.TeamId == team.Id && mi.Status != "Cancelled")
            .OrderByDescending(mi => mi.MeetingDateTime)
            .Select(mi => new
            {
                mi.Id,
                mi.Title,
                mi.Description,
                MeetingDateTime = mi.MeetingDateTime.ToString("MMM dd, yyyy hh:mm tt"),
                mi.Location,
                mi.DurationMinutes,
                mi.Status,
                FacultyName = mi.Faculty.FullName,
                MyResponse = mi.Student1ResponseId == studentId ? mi.Student1Response : mi.Student2Response,
                CanRespond = (mi.Student1ResponseId == studentId && mi.Student1Response == "Pending") ||
                             (mi.Student2ResponseId == studentId && mi.Student2Response == "Pending")
            })
            .ToListAsync();

        return Json(new { success = true, invitations });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = $"Error: {ex.Message}" });
    }
}

// POST: Student/RespondToMeetingInvite
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RespondToMeetingInvite(int invitationId, string response)
{
    var studentIdString = HttpContext.Session.GetString("StudentId");
    if (string.IsNullOrEmpty(studentIdString))
    {
        return Json(new { success = false, message = "Please login first." });
    }

    try
    {
        if (!int.TryParse(studentIdString, out int studentId))
        {
            return Json(new { success = false, message = "Invalid session." });
        }

        if (response != "Accepted" && response != "Rejected")
        {
            return Json(new { success = false, message = "Invalid response." });
        }

        var invitation = await _context.MeetingInvitations
            .Include(mi => mi.Team)
                .ThenInclude(t => t.Student1)
            .Include(mi => mi.Team)
                .ThenInclude(t => t.Student2)
            .FirstOrDefaultAsync(mi => mi.Id == invitationId);

        if (invitation == null)
        {
            return Json(new { success = false, message = "Meeting invitation not found." });
        }

        // Update student's response
        if (invitation.Student1ResponseId == studentId)
        {
            invitation.Student1Response = response;
        }
        else if (invitation.Student2ResponseId == studentId)
        {
            invitation.Student2Response = response;
        }
        else
        {
            return Json(new { success = false, message = "You are not part of this team." });
        }

        // Update overall status
        var student1Accepted = invitation.Student1Response == "Accepted";
        var student2Accepted = invitation.Student2ResponseId.HasValue ? 
                               invitation.Student2Response == "Accepted" : true;
        
        var student1Rejected = invitation.Student1Response == "Rejected";
        var student2Rejected = invitation.Student2ResponseId.HasValue && 
                               invitation.Student2Response == "Rejected";

        if (student1Accepted && student2Accepted)
        {
            invitation.Status = "Accepted";
        }
        else if (student1Rejected || student2Rejected)
        {
            invitation.Status = "Rejected";
        }

        invitation.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        // Log activity
        var studentName = HttpContext.Session.GetString("StudentName") ?? "Student";
        _context.TeamActivityLogs.Add(new TeamActivityLog
        {
            TeamId = invitation.TeamId,
            Action = $"{response} Meeting Invitation",
            Details = $"{invitation.Title} - {studentName}",
            PerformedByRole = "Student",
            PerformedByName = studentName,
            Timestamp = DateTime.Now
        });
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = $"You have {response.ToLower()} the meeting invitation." });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = $"Error: {ex.Message}" });
    }
}
```

---

## ?? Student UI Components

### Meeting Invitations Page

Create a new view `Views/Student/MeetingInvitations.cshtml`:

```html
@{
    ViewData["Title"] = "Meeting Invitations";
}

<div class="page-header">
    <h1><i class="fas fa-calendar-check"></i> Meeting Invitations</h1>
</div>

<div class="invitations-container">
    <div class="section-title">
        <i class="fas fa-inbox"></i> Pending Invitations
    </div>
    
    <div id="invitationsList">
        <!-- Invitations will be loaded here -->
        <div class="loading">
            <i class="fas fa-spinner fa-spin"></i> Loading invitations...
        </div>
    </div>
</div>

<script>
async function loadMeetingInvitations() {
    try {
        var response = await fetch('@Url.Action("GetMeetingInvitations", "Student")');
        var result = await response.json();
        
        var listDiv = document.getElementById('invitationsList');
        
        if (result.success && result.invitations.length > 0) {
            var html = '';
            result.invitations.forEach(function(invite) {
                html += '<div class="invitation-card ' + (invite.MyResponse !== 'Pending' ? 'responded' : '') + '">';
                html += '  <div class="invitation-header">';
                html += '    <h3>' + invite.Title + '</h3>';
                html += '    <span class="status-badge status-' + invite.MyResponse.toLowerCase() + '">' + invite.MyResponse + '</span>';
                html += '  </div>';
                html += '  <div class="invitation-body">';
                html += '    <p><i class="fas fa-user-tie"></i> <strong>From:</strong> ' + invite.FacultyName + '</p>';
                html += '    <p><i class="fas fa-calendar"></i> <strong>When:</strong> ' + invite.MeetingDateTime + '</p>';
                html += '    <p><i class="fas fa-map-marker-alt"></i> <strong>Where:</strong> ' + (invite.Location || 'To be decided') + '</p>';
                html += '    <p><i class="fas fa-clock"></i> <strong>Duration:</strong> ' + invite.DurationMinutes + ' minutes</p>';
                if (invite.Description) {
                    html += '    <p><i class="fas fa-align-left"></i> <strong>Agenda:</strong> ' + invite.Description + '</p>';
                }
                html += '  </div>';
                
                if (invite.CanRespond) {
                    html += '  <div class="invitation-actions">';
                    html += '    <button class="btn-accept" onclick="respondToInvite(' + invite.Id + ', \'Accepted\')">';
                    html += '      <i class="fas fa-check"></i> Accept';
                    html += '    </button>';
                    html += '    <button class="btn-reject" onclick="respondToInvite(' + invite.Id + ', \'Rejected\')">';
                    html += '      <i class="fas fa-times"></i> Decline';
                    html += '    </button>';
                    html += '  </div>';
                }
                
                html += '</div>';
            });
            listDiv.innerHTML = html;
        } else {
            listDiv.innerHTML = '<div class="no-invitations"><i class="fas fa-inbox"></i><p>No meeting invitations yet</p></div>';
        }
    } catch (error) {
        document.getElementById('invitationsList').innerHTML = '<div class="error"><i class="fas fa-exclamation-circle"></i><p>Error loading invitations</p></div>';
    }
}

async function respondToInvite(invitationId, response) {
    try {
        var formData = new FormData();
        formData.append('invitationId', invitationId);
        formData.append('response', response);
        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) formData.append('__RequestVerificationToken', tokenInput.value);
        
        var result = await fetch('@Url.Action("RespondToMeetingInvite", "Student")', {
            method: 'POST',
            body: formData
        });
        
        var data = await result.json();
        
        if (data.success) {
            alert(data.message);
            loadMeetingInvitations(); // Reload list
        } else {
            alert(data.message);
        }
    } catch (error) {
        alert('Error responding to invitation.');
    }
}

// Load invitations on page load
loadMeetingInvitations();
</script>

@Html.AntiForgeryToken()
```

---

## ?? Notification Badge Implementation

### JavaScript to Update Badge Count

```javascript
async function updateNotificationBadge() {
    try {
        var response = await fetch('@Url.Action("GetMeetingInvitations", "Student")');
        var result = await response.json();
        
        if (result.success) {
            var pendingCount = result.invitations.filter(i => i.CanRespond).length;
            
            var badge = document.getElementById('notificationBadge');
            if (pendingCount > 0) {
                badge.textContent = pendingCount;
                badge.style.display = 'flex';
            } else {
                badge.style.display = 'none';
            }
        }
    } catch (error) {
        console.error('Error updating notification badge:', error);
    }
}

// Update badge every 30 seconds
setInterval(updateNotificationBadge, 30000);

// Initial load
updateNotificationBadge();
```

---

## ?? Usage Workflow

### For Faculty:

1. Navigate to **Team Details** page
2. Click **"Send Meeting Invite"** button
3. Fill in meeting details:
   - Title
   - Description/Agenda
   - Date & Time
   - Location
   - Duration
4. Click **"Send Invitation"**
5. Students receive notifications
6. Faculty can view responses in the **Meeting Invitations** section

### For Students:

1. Receive notification bell badge
2. Click notification icon
3. See meeting invite details
4. Click **Accept** or **Decline**
5. Faculty sees the response
6. If both students accept, meeting is confirmed

---

## ?? Security Features

1. **Session Validation** - Checks user authentication
2. **Team Verification** - Ensures student is part of the team
3. **Faculty Authorization** - Only assigned mentors can send invites
4. **CSRF Protection** - Anti-forgery tokens on all forms
5. **Input Validation** - Validates all meeting details
6. **Date Validation** - Ensures meeting is in the future

---

## ?? Benefits

### For Faculty:
- ? Easy scheduling
- ? Track student availability
- ? Centralized meeting management
- ? Automatic notifications

### For Students:
- ? Never miss a meeting
- ? Respond at convenience
- ? Clear meeting agenda
- ? Calendar integration

### For Admin:
- ? Monitor mentor-student interactions
- ? Track meeting frequency
- ? Generate reports

---

## ??? Customization Options

1. **Email Integration**: Send email notifications in addition to in-app
2. **Calendar Export**: Allow exporting to Google Calendar/Outlook
3. **Recurring Meetings**: Support for weekly/monthly meetings
4. **Meeting Reminders**: Send reminders before meeting time
5. **Video Call Integration**: Embed Google Meet/Zoom links
6. **Attendance Tracking**: Mark attendance after meeting
7. **Meeting Notes**: Allow students to add notes after meeting

---

## ?? Sample Data

```sql
-- Sample meeting invitation
INSERT INTO MeetingInvitations (TeamId, FacultyId, Title, Description, MeetingDateTime, Location, DurationMinutes, Status, Student1ResponseId, Student1Response, Student2ResponseId, Student2Response, CreatedAt)
VALUES (1, 1, 'Project Progress Review', 'Discuss current progress and next milestones', '2025-02-15 10:00:00', 'Faculty Room 301', 60, 'Pending', 1, 'Pending', 2, 'Pending', GETDATE());
```

---

## ? Testing Checklist

- [ ] Table created successfully
- [ ] Faculty can send meeting invites
- [ ] Students receive notifications
- [ ] Students can accept/reject invites
- [ ] Badge count updates correctly
- [ ] Faculty can see responses
- [ ] Faculty can cancel invites
- [ ] Meeting history displays correctly
- [ ] Notifications work for individual teams
- [ ] Activity logs record meeting actions

---

## ?? Troubleshooting

### Issue: "Table 'MeetingInvitations' does not exist"
**Solution:** Run the SQL script `AddMeetingInvitationsTable.sql`

### Issue: Notifications not showing
**Solution:** Check if notifications are enabled in the database

### Issue: Badge not updating
**Solution:** Verify JavaScript is loading and API endpoint is accessible

---

## ?? Next Steps

1. Run the database migration
2. Add UI components to Faculty TeamDetails page
3. Create Student Meeting Invitations page
4. Test the complete flow
5. Deploy to production

---

**? The meeting notification system is now ready for implementation!**

Just run the migration and add the UI components to start using it.
