# Problem Statement Assignment Feature - Implementation Summary

## Overview
This feature allows faculty members to assign problem statements from a centralized bank to their assigned teams. Once a problem statement is assigned to a team by any faculty, it becomes unavailable to other faculty members, ensuring unique assignments across teams.

## Changes Made

### 1. **Model Updates**

#### ProblemStatementBank.cs
Added tracking fields for assignment status:
- `IsAssigned` - Boolean flag indicating if the problem statement has been assigned
- `AssignedToTeamId` - Foreign key to the Team that received this problem statement
- `AssignedByFacultyId` - Foreign key to the Faculty who assigned it
- `AssignedAt` - Timestamp of when it was assigned
- Navigation properties for `AssignedToTeam` and `AssignedByFaculty`

#### AppDbContext.cs
Added entity framework relationships for the new foreign keys with SetNull delete behavior.

### 2. **Controller Updates**

#### FacultyController.cs
Added two new actions:

**AssignProblemStatements (GET)**
- Retrieves all teams assigned to the logged-in faculty
- Gets all unassigned problem statements matching the faculty's department
- Passes data to the view for display

**AssignProblemStatementFromBank (POST)**
- Validates that the faculty is the mentor for the team
- Checks if the problem statement is still available
- Marks the problem statement as assigned
- Updates the project progress for the team
- Logs the activity in TeamActivityLogs

### 3. **View Updates**

#### Views/Faculty/AssignProblemStatements.cshtml (NEW)
A new interactive page featuring:
- **Left Panel**: Displays all teams assigned to the faculty
  - Shows team number, members, and current assignment status
  - Displays current problem statement if already assigned
  
- **Right Panel**: Lists all available problem statements
  - Shows problem statement text, year, department, and creation date
  - Each statement has a "Select to Assign" button

**Workflow:**
1. Faculty clicks "Select to Assign" on a problem statement
2. The selected statement is highlighted
3. Teams without assignments become clickable
4. Faculty clicks on a team to complete the assignment
5. Confirmation prompt appears
6. Assignment is saved and page refreshes

#### Views/Faculty/MainDashboard.cshtml
Added a new card:
- **Title**: "Assign Problem Statements"
- **Icon**: File/Document icon
- **Description**: "Assign problem statements from the bank to your assigned teams."
- **Button**: Links to the AssignProblemStatements action

### 4. **Database Migration**

**File**: `20260217000000_AddProblemStatementAssignmentTracking.cs`

Adds the following columns to `ProblemStatementBanks` table:
- `IsAssigned` (bit, default: false)
- `AssignedToTeamId` (int, nullable)
- `AssignedByFacultyId` (int, nullable)
- `AssignedAt` (datetime2, nullable)

Creates indexes and foreign key relationships.

## How to Apply These Changes

### Step 1: Stop the Running Application
The application must be stopped before running the migration.

### Step 2: Apply the Database Migration

**Option A - Using Package Manager Console in Visual Studio:**
```
Update-Database
```

**Option B - Using Command Line:**
```bash
cd TeamPro1
dotnet ef database update
```

### Step 3: Restart the Application

## How Faculty Use This Feature

1. **Access**: Faculty logs in and clicks "Assign Problem Statements" card on the main dashboard
2. **View Teams**: Left panel shows all teams assigned to them as mentor
3. **Browse Statements**: Right panel shows all available problem statements from the admin-uploaded bank
4. **Select**: Click "Select to Assign" on a problem statement
5. **Assign**: Click on a team without an existing assignment
6. **Confirm**: Confirm the assignment in the popup dialog
7. **Done**: The problem statement is assigned and marked as unavailable to other faculty

## Key Features

### Exclusivity
- Once assigned, a problem statement disappears from the available list for all other faculty
- This ensures each team gets a unique problem statement
- Teams can only have one problem statement at a time

### Department Filtering
- Faculty only see problem statements matching their department
- Ensures relevant problem statements for each department

### Assignment Tracking
- System tracks who assigned which problem statement
- Timestamp of assignment is recorded
- Activity is logged in TeamActivityLogs for audit trail

### Visual Feedback
- Teams already assigned show a green "Assigned" badge
- Teams pending assignment show an orange "Pending" badge
- Selected problem statements are highlighted
- Interactive click-to-assign workflow with visual indicators

## Testing Checklist

- [ ] Faculty can view their assigned teams
- [ ] Faculty can see available problem statements (only unassigned ones)
- [ ] Faculty can assign a problem statement to a team
- [ ] Assigned problem statement disappears from other faculty's available list
- [ ] Team shows the problem statement in Project Progress
- [ ] Activity is logged in TeamActivityLogs
- [ ] Teams with existing assignments cannot be reassigned (shows "Assigned" status)
- [ ] Only problem statements matching faculty department are shown
- [ ] Empty states display when no teams or problem statements are available

## Troubleshooting

### Migration Fails
If the migration fails, check:
1. Application is stopped
2. Database connection string is correct in appsettings.json
3. No other processes are accessing the database

### Problem Statements Not Showing
Check:
1. Admin has uploaded problem statements to the bank
2. Problem statements match the faculty's department
3. Problem statements are marked as unassigned in database

### Assignment Fails
Verify:
1. Faculty is logged in with a valid session
2. Faculty is actually assigned as mentor to the team
3. Problem statement is still available (not assigned by another faculty simultaneously)

## Future Enhancements (Optional)

- Allow faculty to reassign/change problem statements for their teams
- Add bulk assignment capability
- Filter problem statements by year
- Search functionality for problem statements
- Export assigned problem statements report
- Notification to students when problem statement is assigned

---

**Implementation Date**: February 17, 2025
**Developer**: S. Md. Shahid Afrid & G. Veena
**Guided By**: Dr. P. Penchala Prasad
