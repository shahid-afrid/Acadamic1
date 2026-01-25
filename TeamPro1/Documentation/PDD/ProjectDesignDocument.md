1. Project Title
Team Formation Portal for Academic Projects

2. Project Overview
The Team Formation Portal for Academic Projects is a college academic web application that helps students form teams for academic projects and allows faculty and HOD to track project progress and manage assignments.

This system supports three user roles:

Student

Faculty

HOD (Admin-level role)

The application provides:

Secure login system for all roles

Student team request and team formation flow

Faculty review and progress tracking

HOD management of students, faculty, and project assignment

3. Problem Statement
In many colleges, team formation and project tracking is done manually, which causes:

confusion in team selection

duplicate team entries

lack of transparency

difficulty in tracking project progress and reviews

This portal automates the process and maintains accurate records for team formation, faculty assignment, and project monitoring.

4. Objectives
Provide a digital platform for students to form teams smoothly

Allow students to send/accept/reject team requests

Auto-generate team numbers based on team formation order

Enable faculty to monitor project completion percentage and upload proofs

Enable HOD to manage student/faculty records and assign faculty/problem statements

5. Scope of the Project
5.1 Student Module Scope
Login

View dashboard cards:

Student Profile

Pool of Students

Grouped Students Pool

Status Update

Send team request to one student (same year + semester)

Receive request notifications via bell icon

Accept/Reject requests

View formed team details

5.2 Faculty Module Scope
Login

View dashboard cards:

Faculty Profile

Project Status

View project teams assigned to faculty in table format

Update completion percentage (editable)

Provide review/suggestions (only faculty can update)

View proofs uploaded by students

5.3 HOD Module Scope
Login

View dashboard statistics:

Total Students

Total Faculty

Total Teams

Total Assigned Projects

Manage Students (Add/Edit/Delete/Bulk Add)

Manage Faculty (Add/Edit/Delete)

Manage Project Assignment:

View pool students and grouped pool by Year + Semester filter

Manage Assignment:

Assign faculty to teams

Assign problem statement to teams

Faculty-wise tracking view:

Select faculty and see all their assigned teams with progress table

6. System Users and Roles
6.1 Student
Permissions:

Login

View own profile

Send request to another student

Accept/Reject requests

View team info

Upload proofs for project progress

View completion % and faculty suggestions (read-only for suggestions)

6.2 Faculty
Permissions:

Login

View assigned teams

Update completion %

View proofs uploaded by student

Provide review/suggestions

6.3 HOD
Permissions:

Login

CRUD Students and Faculty

Assign faculty and problem statement to teams

View overall statistics and progress

7. Functional Requirements
7.1 Common Requirements (All Roles)
Role-based login

Change password option

Secure session handling

Logout

7.2 Student Module Functional Requirements
7.2.1 Student Login
Student logs in using email/registration number + password

After login, redirect to Student Dashboard

7.2.2 Student Dashboard UI (4 Cards)
Student Profile

Pool of Students

Grouped Students Pool

Status Update

7.2.3 Student Profile
Fields:

Name

Age

Department

Year

Semester

Registration Number
Actions:

Change Password

7.2.4 Pool of Students (Same Year + Semester)
Display list of roll numbers eligible for team selection

Student can click a roll number and see options:

Send Request

Cancel Request

Rules:

Student can send request to only one teammate at a time

Request allowed only if both are not already in a team

7.2.5 Notification Bell (Request Handling)
Bell icon shows pending requests

Requested student can:

Accept Request

Reject Request

7.2.6 Grouped Students Pool
Shows confirmed teams

Displays:

Team Number

Student 1 name + regd

Student 2 name + regd

Team Number Logic:

Team number assigned based on order of team formation

First team formed gets Team No. 1, next gets Team No. 2, etc.

7.2.7 Status Update (Project Status for Students)
Student can view project progress table (only their team)
Fields:

Team Number

Mate regd number + name (2 students)

Project Completion %

Proof uploads

Faculty review/suggestions

Student Permissions:

Can upload proofs

Can update completion % (optional based on requirement)

Can view faculty suggestions (read-only)

7.3 Faculty Module Functional Requirements
7.3.1 Faculty Login
Faculty logs in with email + password

Redirect to Faculty Dashboard

7.3.2 Faculty Dashboard UI (2 Cards)
Faculty Profile

Project Status

7.3.3 Faculty Profile
Fields:

Name

Department

Email
Actions:

Change Password

7.3.4 Project Status Table (Faculty View)
Faculty should see all teams assigned to them:

Table Columns:

Team Number

Student 1 Regd No + Name

Student 2 Regd No + Name

Project Completion % (editable by faculty)

Proofs (view/download)

Review/Suggestions (editable only by faculty)

7.4 HOD Module Functional Requirements
7.4.1 HOD Login
HOD logs in with credentials

Redirect to HOD Dashboard

7.4.2 HOD Dashboard (Statistics + Cards)
Statistics:

Total Students

Total Faculty

Total Teams

Cards:

Manage Students

Manage Faculty

Manage Project Assignment

Manage Assignment

Faculty-wise Tracking

7.4.3 Manage Students
Features:

Add Student

Edit Student

Delete Student

Bulk Upload Students (Excel)

Display format:

Table view (same as TutorLive project)

7.4.4 Manage Faculty
Features:

Add Faculty

Edit Faculty

Delete Faculty

Table view

7.4.5 Manage Project Assignment
HOD can:

Filter by Year and Semester

View:

Single pool students (not grouped)

Grouped pool students (already in teams)

7.4.6 Manage Assignment
HOD can view teams and assign:

Faculty (dropdown)

Problem Statement (dropdown)

Table Fields:

Team Number

Student 1 Regd + Name

Student 2 Regd + Name

Assigned Faculty (dropdown)

Assigned Problem Statement (dropdown)

7.4.7 Faculty-wise Tracking
HOD selects a faculty from dropdown

System shows same project status table that faculty sees

8. Non-Functional Requirements
User-friendly interface (dashboard cards, tables)

Responsive UI (mobile + desktop)

Secure authentication & password hashing

Fast database queries (filtering year/semester/team)

Real-time notifications (optional wow factor using SignalR)

9. Technology Stack
9.1 Frontend
Razor Views (MVC)

Bootstrap 5

JavaScript + jQuery

DataTables.js (optional for tables)

9.2 Backend
ASP.NET Core MVC (.NET 8 LTS)

C#

9.3 Database
SQL Server (LocalDB / SQL Server Express)

Entity Framework Core (EF Core 8.x)

9.4 Security
BCrypt.Net-Next (Password hashing)

Sessions for authentication

9.5 Real-time Feature (Wow Factor)
SignalR (for instant request notifications & live updates)

10. Database Design (Tables)
10.1 Students
Fields:

StudentId (PK)

FullName

RegdNumber

Email

PasswordHash

Department

Year

Semester

IsInTeam (boolean)

10.2 Faculties
Fields:

FacultyId (PK)

Name

Email

PasswordHash

Department

10.3 HODs / Admins
Fields:

HodId (PK)

Name

Email

PasswordHash

Department

10.4 TeamRequests
Fields:

RequestId (PK)

FromStudentId (FK)

ToStudentId (FK)

Status (Pending / Accepted / Rejected / Cancelled)

RequestedAt

10.5 Teams
Fields:

TeamId (PK)

TeamNumber (Unique)

Student1Id (FK)

Student2Id (FK)

CreatedAt

10.6 ProblemStatements
Fields:

ProblemId (PK)

Title

Description

Department

Year

Semester

10.7 TeamAssignments
Fields:

AssignmentId (PK)

TeamId (FK)

FacultyId (FK)

ProblemId (FK)

AssignedAt

10.8 ProjectStatus
Fields:

StatusId (PK)

TeamId (FK)

CompletionPercentage

ProofFilePath

FacultySuggestions

LastUpdatedAt

11. System Flow (High Level)
Step 1: Open Website
Homepage shows:

Student Login

Faculty Login

HOD Login

Step 2: Student Team Formation
Student logs in

Selects partner from pool

Sends request

Partner accepts

Team is created and team number assigned

Step 3: Faculty Monitoring
Faculty logs in

Views assigned teams

Updates completion %

Adds suggestions

Step 4: HOD Management
HOD logs in

Adds students/faculty

Assigns faculty and problem statement to teams

Tracks faculty-wise status

12. Future Enhancements
Email notifications on request

Team size 3 or 4 members support

Upload multiple proofs with timeline

Auto report generation (PDF/Excel)

Admin approval for final team confirmation

13. Conclusion
The Team Formation Portal for Academic Projects provides an efficient and professional solution for student team formation, faculty monitoring, and HOD administration. It reduces manual work, avoids confusion, and improves project tracking transparency.