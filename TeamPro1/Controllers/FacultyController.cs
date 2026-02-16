using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamPro1.Models;

namespace TeamPro1.Controllers
{
    /// <summary>
    /// Controller for managing faculty-related operations
    /// </summary>
    public class FacultyController : Controller
    {
        private readonly AppDbContext _context;

        public FacultyController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Faculty/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Faculty/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(FacultyLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var faculty = await _context.Faculties
                    .FirstOrDefaultAsync(f => f.Email == model.Email);

                // Verify password using BCrypt
                if (faculty == null || !BCrypt.Net.BCrypt.Verify(model.Password, faculty.Password))
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                // Clear any existing session
                HttpContext.Session.Clear();

                // Store faculty ID and info in session
                HttpContext.Session.SetInt32("FacultyId", faculty.Id);
                HttpContext.Session.SetString("FacultyName", faculty.FullName);
                HttpContext.Session.SetString("FacultyDepartment", faculty.Department);

                // Force session to be saved immediately
                await HttpContext.Session.CommitAsync();

                TempData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("MainDashboard");
            }

            return View(model);
        }

        // GET: Faculty/MainDashboard
        [HttpGet]
        public IActionResult MainDashboard()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");

            if (facultyId == null)
            {
                TempData["ErrorMessage"] = "Please login to access the dashboard.";
                return RedirectToAction("Login");
            }

            ViewBag.FacultyId = facultyId;
            ViewBag.FacultyName = HttpContext.Session.GetString("FacultyName");
            ViewBag.FacultyDepartment = HttpContext.Session.GetString("FacultyDepartment");

            return View();
        }

        // GET: Faculty/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                TempData["ErrorMessage"] = "Please login to access the dashboard.";
                return RedirectToAction("Login");
            }

            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(f => f.Id == facultyId.Value);

            if (faculty == null)
            {
                TempData["ErrorMessage"] = "Faculty not found. Please login again.";
                return RedirectToAction("Login");
            }

            return View(faculty);
        }

        // GET: Faculty/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                TempData["ErrorMessage"] = "Please login to view your profile.";
                return RedirectToAction("Login");
            }

            var faculty = await _context.Faculties.FindAsync(facultyId.Value);
            if (faculty == null)
            {
                TempData["ErrorMessage"] = "Faculty not found. Please login again.";
                return RedirectToAction("Login");
            }

            return View(faculty);
        }

        // GET: Faculty/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                TempData["ErrorMessage"] = "Please login to edit your profile.";
                return RedirectToAction("Login");
            }

            var faculty = await _context.Faculties.FindAsync(facultyId.Value);
            if (faculty == null)
            {
                TempData["ErrorMessage"] = "Faculty not found. Please login again.";
                return RedirectToAction("Login");
            }

            var viewModel = new FacultyEditProfileViewModel
            {
                FacultyId = faculty.Id,
                FullName = faculty.FullName,
                Email = faculty.Email,
                Department = faculty.Department
            };

            return View(viewModel);
        }

        // POST: Faculty/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(FacultyEditProfileViewModel model)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                TempData["ErrorMessage"] = "Please login to edit your profile.";
                return RedirectToAction("Login");
            }

            // Ensure the model ID matches the logged-in faculty's ID
            if (model.FacultyId != facultyId.Value)
            {
                return BadRequest();
            }

            // Custom validation for password change
            if (!string.IsNullOrEmpty(model.CurrentPassword) || !string.IsNullOrEmpty(model.NewPassword) || !string.IsNullOrEmpty(model.ConfirmPassword))
            {
                // If any password field is filled, all three must be filled
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is required to change password");
                }
                if (string.IsNullOrEmpty(model.NewPassword))
                {
                    ModelState.AddModelError("NewPassword", "New password is required");
                }
                if (string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    ModelState.AddModelError("ConfirmPassword", "Confirm password is required");
                }
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "New password and confirmation do not match");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var faculty = await _context.Faculties.FindAsync(facultyId.Value);
            if (faculty == null)
            {
                TempData["ErrorMessage"] = "Faculty not found. Please login again.";
                return RedirectToAction("Login");
            }

            // Update profile fields
            faculty.FullName = model.FullName;
            faculty.Email = model.Email;
            // Department is not updatable by faculty - keep the original value

            bool passwordChanged = false;

            // Handle password change if requested
            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                // Verify current password
                if (faculty.Password != model.CurrentPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                    return View(model);
                }

                // Update to new password
                faculty.Password = model.NewPassword;
                passwordChanged = true;
            }

            await _context.SaveChangesAsync();

            // Update session with new info
            HttpContext.Session.SetString("FacultyName", faculty.FullName);
            HttpContext.Session.SetString("FacultyDepartment", faculty.Department);

            TempData["SuccessMessage"] = passwordChanged
                ? "Profile and password updated successfully!"
                : "Profile updated successfully!";

            return RedirectToAction("Profile");
        }

        // GET: Faculty/AssignedTeams - View teams assigned to this faculty
        [HttpGet]
        public async Task<IActionResult> AssignedTeams()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                TempData["ErrorMessage"] = "Please login to view assigned teams.";
                return RedirectToAction("Login");
            }

            var assignedTeams = await _context.ProjectProgresses
                .Include(pp => pp.Team)
                    .ThenInclude(t => t.Student1)
                .Include(pp => pp.Team)
                    .ThenInclude(t => t.Student2)
                .Where(pp => pp.AssignedFacultyId == facultyId.Value)
                .ToListAsync();

            ViewBag.FacultyName = HttpContext.Session.GetString("FacultyName");
            return View(assignedTeams);
        }

        // GET: Faculty/TeamDetails/{id}
        [HttpGet]
        public async Task<IActionResult> TeamDetails(int id)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                TempData["ErrorMessage"] = "Please login to view team details.";
                return RedirectToAction("Login");
            }

            var team = await _context.Teams
                .Include(t => t.Student1)
                .Include(t => t.Student2)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                TempData["ErrorMessage"] = "Team not found.";
                return RedirectToAction("AllTeams");
            }

            var projectProgress = await _context.ProjectProgresses
                .Include(pp => pp.AssignedFaculty)
                .FirstOrDefaultAsync(pp => pp.TeamId == id);

            var teamMeetings = await _context.TeamMeetings
                .Where(tm => tm.TeamId == id)
                .OrderBy(tm => tm.MeetingNumber)
                .ToListAsync();

            // Auto-sync: update CompletionPercentage from the latest meeting
            if (projectProgress != null && teamMeetings.Count > 0)
            {
                var latestMeeting = teamMeetings.Last();
                var needsSave = false;

                if (projectProgress.CompletionPercentage != latestMeeting.CompletionPercentage)
                {
                    projectProgress.CompletionPercentage = latestMeeting.CompletionPercentage;
                    projectProgress.LastUpdated = DateTime.Now;
                    needsSave = true;
                }

                // Auto-set status to "In Progress" if both mentor and problem statement are assigned and status is still Pending/Not Started
                if (projectProgress.AssignedFacultyId != null
                    && !string.IsNullOrEmpty(projectProgress.ProblemStatement)
                    && (projectProgress.Status == "Pending" || projectProgress.Status == "Not Started"
                        || projectProgress.Status == "Mentor Assigned" || projectProgress.Status == "Problem Statement Assigned"))
                {
                    projectProgress.Status = "In Progress";
                    needsSave = true;
                }

                // Auto-set Completed if progress is 100%
                if (projectProgress.CompletionPercentage >= 100 && projectProgress.Status != "Completed")
                {
                    projectProgress.Status = "Completed";
                    needsSave = true;
                }

                if (needsSave)
                {
                    await _context.SaveChangesAsync();
                }
            }
            // Also auto-set status even if no meetings yet
            else if (projectProgress != null)
            {
                var needsSave = false;
                if (projectProgress.AssignedFacultyId != null
                    && !string.IsNullOrEmpty(projectProgress.ProblemStatement)
                    && (projectProgress.Status == "Pending" || projectProgress.Status == "Not Started"
                        || projectProgress.Status == "Mentor Assigned" || projectProgress.Status == "Problem Statement Assigned"))
                {
                    projectProgress.Status = "In Progress";
                    projectProgress.LastUpdated = DateTime.Now;
                    needsSave = true;
                }

                if (needsSave)
                {
                    await _context.SaveChangesAsync();
                }
            }

            // Get all faculties for assignment dropdown
            var faculties = await _context.Faculties.ToListAsync();

            ViewBag.Team = team;
            ViewBag.ProjectProgress = projectProgress;
            ViewBag.TeamMeetings = teamMeetings;
            ViewBag.Faculties = faculties;
            ViewBag.CurrentFacultyId = facultyId.Value;

            return View();
        }

        // POST: Faculty/AssignProblemStatement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProblemStatement(int teamId, string problemStatement)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                var projectProgress = await _context.ProjectProgresses
                    .FirstOrDefaultAsync(pp => pp.TeamId == teamId);

                if (projectProgress == null)
                {
                    projectProgress = new ProjectProgress
                    {
                        TeamId = teamId,
                        ProblemStatement = problemStatement,
                        Status = "Problem Statement Assigned",
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };
                    _context.ProjectProgresses.Add(projectProgress);
                }
                else
                {
                    projectProgress.ProblemStatement = problemStatement;
                    projectProgress.LastUpdated = DateTime.Now;

                    if (projectProgress.AssignedFacultyId != null && !string.IsNullOrEmpty(problemStatement))
                    {
                        projectProgress.Status = "In Progress";
                    }
                    else
                    {
                        projectProgress.Status = "Problem Statement Assigned";
                    }
                }

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = teamId,
                    Action = "Assigned Problem Statement",
                    Details = problemStatement?.Length > 100 ? problemStatement.Substring(0, 100) + "..." : problemStatement,
                    PerformedByRole = "Faculty",
                    PerformedByName = HttpContext.Session.GetString("FacultyName") ?? "Unknown",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Problem statement assigned successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Faculty/AssignMentor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMentor(int teamId, int mentorId)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                var mentor = await _context.Faculties.FindAsync(mentorId);

                var projectProgress = await _context.ProjectProgresses
                    .FirstOrDefaultAsync(pp => pp.TeamId == teamId);

                if (projectProgress == null)
                {
                    projectProgress = new ProjectProgress
                    {
                        TeamId = teamId,
                        AssignedFacultyId = mentorId,
                        Status = "Mentor Assigned",
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };
                    _context.ProjectProgresses.Add(projectProgress);
                }
                else
                {
                    projectProgress.AssignedFacultyId = mentorId;
                    projectProgress.LastUpdated = DateTime.Now;

                    if (!string.IsNullOrEmpty(projectProgress.ProblemStatement))
                    {
                        projectProgress.Status = "In Progress";
                    }
                    else
                    {
                        projectProgress.Status = "Mentor Assigned";
                    }
                }

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = teamId,
                    Action = "Assigned Mentor",
                    Details = $"Mentor: {mentor?.FullName ?? "Unknown"}",
                    PerformedByRole = "Faculty",
                    PerformedByName = HttpContext.Session.GetString("FacultyName") ?? "Unknown",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Mentor assigned successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Faculty/UpdateTeamProgress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTeamProgress(int teamId, int completionPercentage, string status)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                var projectProgress = await _context.ProjectProgresses
                    .FirstOrDefaultAsync(pp => pp.TeamId == teamId);

                if (projectProgress == null)
                {
                    return Json(new { success = false, message = "Project progress not found." });
                }

                projectProgress.CompletionPercentage = completionPercentage;
                projectProgress.Status = status;
                projectProgress.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = teamId,
                    Action = "Updated Team Progress",
                    Details = $"Completion: {completionPercentage}%, Status: {status}",
                    PerformedByRole = "Faculty",
                    PerformedByName = HttpContext.Session.GetString("FacultyName") ?? "Unknown",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Progress updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Faculty/AddFacultyReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFacultyReview(int meetingId, string? facultyReview)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                var meeting = await _context.TeamMeetings.FindAsync(meetingId);
                if (meeting == null)
                {
                    return Json(new { success = false, message = "Meeting not found." });
                }

                meeting.FacultyReview = facultyReview;
                meeting.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = meeting.TeamId,
                    Action = $"Added Review for Meeting #{meeting.MeetingNumber}",
                    Details = facultyReview?.Length > 100 ? facultyReview.Substring(0, 100) + "..." : facultyReview,
                    PerformedByRole = "Faculty",
                    PerformedByName = HttpContext.Session.GetString("FacultyName") ?? "Unknown",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Faculty review saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: Faculty/GetTeamLogs/{teamId}
        public async Task<IActionResult> GetTeamLogs(int teamId)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            var logs = await _context.TeamActivityLogs
                .Where(l => l.TeamId == teamId)
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new
                {
                    l.Action,
                    l.Details,
                    l.PerformedByRole,
                    l.PerformedByName,
                    Timestamp = l.Timestamp.ToString("MMM dd, yyyy hh:mm tt")
                })
                .ToListAsync();

            return Json(new { success = true, logs });
        }

        // GET: Faculty/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logged out successfully!";
            return RedirectToAction("Login");
        }

        // ===================== PROBLEM STATEMENT ASSIGNMENT =====================

        // GET: Faculty/AssignProblemStatements
        [HttpGet]
        public async Task<IActionResult> AssignProblemStatements()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                TempData["ErrorMessage"] = "Please login to assign problem statements.";
                return RedirectToAction("Login");
            }

            var faculty = await _context.Faculties.FindAsync(facultyId.Value);
            if (faculty == null)
            {
                TempData["ErrorMessage"] = "Faculty not found. Please login again.";
                return RedirectToAction("Login");
            }

            // Get all teams assigned to this faculty with their problem statements
            var assignedTeamsWithProgress = await _context.ProjectProgresses
                .Include(pp => pp.Team)
                    .ThenInclude(t => t.Student1)
                .Include(pp => pp.Team)
                    .ThenInclude(t => t.Student2)
                .Where(pp => pp.AssignedFacultyId == facultyId.Value)
                .ToListAsync();

            // Get available problem statements (unassigned ones matching department and year)
            var availableProblemStatements = await _context.ProblemStatementBanks
                .Where(ps => !ps.IsAssigned && ps.Department == faculty.Department && ps.Year == 3)
                .OrderByDescending(ps => ps.CreatedAt)
                .ToListAsync();

            ViewBag.FacultyName = HttpContext.Session.GetString("FacultyName");
            ViewBag.AssignedTeams = assignedTeamsWithProgress;
            ViewBag.AvailableProblemStatements = availableProblemStatements;

            return View();
        }

        // POST: Faculty/AssignProblemStatementFromBank
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProblemStatementFromBank(int teamId, int problemStatementId)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                // Verify that the team is assigned to this faculty
                var projectProgress = await _context.ProjectProgresses
                    .FirstOrDefaultAsync(pp => pp.TeamId == teamId && pp.AssignedFacultyId == facultyId.Value);

                if (projectProgress == null)
                {
                    return Json(new { success = false, message = "You are not assigned as mentor to this team." });
                }

                // Get the problem statement from bank
                var problemStatement = await _context.ProblemStatementBanks
                    .FirstOrDefaultAsync(ps => ps.Id == problemStatementId && !ps.IsAssigned);

                if (problemStatement == null)
                {
                    return Json(new { success = false, message = "This problem statement is no longer available." });
                }

                // Assign the problem statement to the team
                problemStatement.IsAssigned = true;
                problemStatement.AssignedToTeamId = teamId;
                problemStatement.AssignedByFacultyId = facultyId.Value;
                problemStatement.AssignedAt = DateTime.Now;

                // Update project progress
                projectProgress.ProblemStatement = problemStatement.Statement;
                projectProgress.LastUpdated = DateTime.Now;

                if (projectProgress.AssignedFacultyId != null && !string.IsNullOrEmpty(problemStatement.Statement))
                {
                    projectProgress.Status = "In Progress";
                }
                else
                {
                    projectProgress.Status = "Problem Statement Assigned";
                }

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = teamId,
                    Action = "Assigned Problem Statement from Bank",
                    Details = problemStatement.Statement.Length > 100 ? problemStatement.Statement.Substring(0, 100) + "..." : problemStatement.Statement,
                    PerformedByRole = "Faculty",
                    PerformedByName = HttpContext.Session.GetString("FacultyName") ?? "Unknown",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Problem statement assigned successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ===================== FACULTY NOTIFICATIONS =====================

        // GET: Faculty/GetFacultyNotifications - Get notifications for faculty about student activities
        public async Task<IActionResult> GetFacultyNotifications()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                // Get all teams assigned to this faculty
                var assignedTeamIds = await _context.ProjectProgresses
                    .Where(pp => pp.AssignedFacultyId == facultyId.Value)
                    .Select(pp => pp.TeamId)
                    .ToListAsync();

                if (!assignedTeamIds.Any())
                {
                    return Json(new { success = true, notifications = new List<object>() });
                }

                // Get recent team activity logs (last 7 days) related to student responses and meeting progress
                var cutoffDate = DateTime.Now.AddDays(-7);
                var recentActivities = await _context.TeamActivityLogs
                    .Where(log => assignedTeamIds.Contains(log.TeamId)
                        && log.Timestamp >= cutoffDate
                        && log.PerformedByRole == "Student"
                        && (log.Action.Contains("Meeting Invitation")
                            || log.Action.Contains("Attended")
                            || log.Action.Contains("Added Meeting")
                            || log.Action.Contains("Updated Meeting")))
                    .OrderByDescending(log => log.Timestamp)
                    .Take(50)
                    .ToListAsync();

                // Get team details for each activity
                var notifications = new List<object>();
                foreach (var activity in recentActivities)
                {
                    var team = await _context.Teams
                        .Include(t => t.Student1)
                        .Include(t => t.Student2)
                        .FirstOrDefaultAsync(t => t.Id == activity.TeamId);

                    if (team == null) continue;

                    var notificationType = "";
                    var status = "";
                    var title = "";
                    var message = "";

                    if (activity.Action.Contains("Accepted Meeting Invitation"))
                    {
                        notificationType = "accepted";
                        status = "Accepted";
                        title = "Meeting Accepted";
                        message = $"{activity.PerformedByName} accepted a meeting invitation.";
                    }
                    else if (activity.Action.Contains("Rejected Meeting Invitation"))
                    {
                        notificationType = "rejected";
                        status = "Rejected";
                        title = "Meeting Rejected";
                        message = $"{activity.PerformedByName} rejected a meeting invitation.";
                    }
                    else if (activity.Action.Contains("Attended Scheduled Meeting"))
                    {
                        notificationType = "attended";
                        status = "Attended";
                        title = "Meeting Attended";
                        message = $"{activity.PerformedByName} marked a meeting as attended.";
                    }
                    else if (activity.Action.Contains("Added Meeting"))
                    {
                        notificationType = "added_progress";
                        status = "Added";
                        title = "Meeting Added to Progress";
                        message = $"{activity.PerformedByName} added a new meeting to project progress.";
                    }
                    else if (activity.Action.Contains("Updated Meeting"))
                    {
                        notificationType = "added_progress";
                        status = "Updated";
                        title = "Meeting Updated";
                        message = $"{activity.PerformedByName} updated a meeting record.";
                    }
                    else
                    {
                        continue; // Skip other activity types
                    }

                    notifications.Add(new
                    {
                        id = activity.Id,
                        type = notificationType,
                        title,
                        message,
                        details = activity.Details,
                        teamNumber = team.TeamNumber,
                        studentName = activity.PerformedByName,
                        status,
                        timestamp = activity.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss"),
                        isRead = false // Faculty notifications are always shown as new
                    });
                }

                return Json(new { success = true, notifications });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Faculty/MarkFacultyNotificationsAsRead - Mark notifications as viewed (optional, for future use)
        [HttpPost]
        public async Task<IActionResult> MarkFacultyNotificationsAsRead()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                // Currently, faculty notifications are activity log based and don't have a read/unread state
                // This endpoint is a placeholder for future enhancement
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ===================== MEETING INVITATIONS =====================

        // GET: Faculty/ScheduledMeetings - View all scheduled meetings across all teams
        [HttpGet]
        public async Task<IActionResult> ScheduledMeetings()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                TempData["ErrorMessage"] = "Please login to view scheduled meetings.";
                return RedirectToAction("Login");
            }

            // Get all teams assigned to this faculty for the schedule meeting dropdown
            var assignedTeams = await _context.ProjectProgresses
                .Include(pp => pp.Team)
                    .ThenInclude(t => t.Student1)
                .Include(pp => pp.Team)
                    .ThenInclude(t => t.Student2)
                .Where(pp => pp.AssignedFacultyId == facultyId.Value)
                .Select(pp => pp.Team)
                .ToListAsync();

            ViewBag.FacultyName = HttpContext.Session.GetString("FacultyName");
            ViewBag.AssignedTeams = assignedTeams;

            return View();
        }

        // GET: Faculty/GetAllMeetingInvitations - Get all meeting invitations for this faculty across all teams
        public async Task<IActionResult> GetAllMeetingInvitations()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                var cutoff24Hours = DateTime.Now.AddHours(-24);

                // Clean up stale invitations
                var staleInvitations = await _context.MeetingInvitations
                    .Where(mi => mi.FacultyId == facultyId.Value
                        && (
                            mi.Status == "AddedToProgress"
                            || mi.Status == "Completed"
                            || (mi.Status == "Rejected" && mi.UpdatedAt.HasValue && mi.UpdatedAt.Value < cutoff24Hours)
                            || (mi.Status == "Cancelled" && mi.UpdatedAt.HasValue && mi.UpdatedAt.Value < cutoff24Hours)
                        ))
                    .ToListAsync();

                if (staleInvitations.Count > 0)
                {
                    _context.MeetingInvitations.RemoveRange(staleInvitations);
                    await _context.SaveChangesAsync();
                }

                var invitations = await _context.MeetingInvitations
                    .Include(mi => mi.Team)
                        .ThenInclude(t => t.Student1)
                    .Include(mi => mi.Team)
                        .ThenInclude(t => t.Student2)
                    .Where(mi => mi.FacultyId == facultyId.Value)
                    .OrderByDescending(mi => mi.MeetingDateTime)
                    .Select(mi => new
                    {
                        mi.Id,
                        mi.Title,
                        mi.Description,
                        MeetingDateTime = mi.MeetingDateTime.ToString("MMM dd, yyyy hh:mm tt"),
                        MeetingDateTimeRaw = mi.MeetingDateTime.ToString("yyyy-MM-ddTHH:mm"),
                        mi.Location,
                        mi.DurationMinutes,
                        mi.Status,
                        mi.TeamId,
                        TeamNumber = mi.Team.TeamNumber,
                        Student1Response = mi.Student1Response ?? "Pending",
                        Student2Response = mi.Student2Response ?? (mi.Student2ResponseId.HasValue ? "Pending" : "N/A"),
                        Student1Name = mi.Team.Student1.FullName,
                        Student2Name = mi.Team.Student2 != null ? mi.Team.Student2.FullName : null,
                        CreatedAt = mi.CreatedAt.ToString("MMM dd, yyyy hh:mm tt")
                    })
                    .ToListAsync();

                return Json(new { success = true, invitations });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Faculty/SendMeetingInvite
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMeetingInvite(int teamId, string title, string description, DateTime meetingDateTime, string location, int durationMinutes)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    return Json(new { success = false, message = "Meeting title is required." });
                }

                if (meetingDateTime <= DateTime.Now)
                {
                    return Json(new { success = false, message = "Meeting date/time must be in the future." });
                }

                var team = await _context.Teams
                    .Include(t => t.Student1)
                    .Include(t => t.Student2)
                    .FirstOrDefaultAsync(t => t.Id == teamId);

                if (team == null)
                {
                    return Json(new { success = false, message = "Team not found." });
                }

                // Create meeting invitation
                var invitation = new MeetingInvitation
                {
                    TeamId = teamId,
                    FacultyId = facultyId.Value,
                    Title = title.Trim(),
                    Description = description?.Trim(),
                    MeetingDateTime = meetingDateTime,
                    Location = location?.Trim(),
                    DurationMinutes = durationMinutes > 0 ? durationMinutes : 60,
                    Status = "Pending",
                    Student1ResponseId = team.Student1Id,
                    Student1Response = "Pending",
                    Student2ResponseId = team.Student2Id,
                    Student2Response = team.Student2Id.HasValue ? "Pending" : null,
                    CreatedAt = DateTime.Now
                };

                _context.MeetingInvitations.Add(invitation);

                // Create notifications for students
                var facultyName = HttpContext.Session.GetString("FacultyName") ?? "Your mentor";

                // Notification for Student 1
                _context.Notifications.Add(new Notification
                {
                    StudentId = team.Student1Id,
                    Message = $"?? Meeting invite from {facultyName}: {title} on {meetingDateTime:MMM dd, yyyy hh:mm tt}",
                    Type = "info",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                // Notification for Student 2 (if exists)
                if (team.Student2Id.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        StudentId = team.Student2Id.Value,
                        Message = $"?? Meeting invite from {facultyName}: {title} on {meetingDateTime:MMM dd, yyyy hh:mm tt}",
                        Type = "info",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = teamId,
                    Action = "Sent Meeting Invitation",
                    Details = $"{title} - {meetingDateTime:MMM dd, yyyy hh:mm tt}",
                    PerformedByRole = "Faculty",
                    PerformedByName = facultyName,
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Meeting invitation sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: Faculty/GetMeetingInvitations
        public async Task<IActionResult> GetMeetingInvitations(int teamId)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                var cutoff24Hours = DateTime.Now.AddHours(-24);

                // Permanently delete stale invitations to free up space
                // Activity logs are preserved in TeamActivityLogs table
                var staleInvitations = await _context.MeetingInvitations
                    .Where(mi => mi.TeamId == teamId
                        && (
                            // Delete AddedToProgress and Completed immediately
                            mi.Status == "AddedToProgress"
                            || mi.Status == "Completed"
                            // Delete Rejected after 24 hours
                            || (mi.Status == "Rejected" && mi.UpdatedAt.HasValue && mi.UpdatedAt.Value < cutoff24Hours)
                            // Delete Cancelled after 24 hours
                            || (mi.Status == "Cancelled" && mi.UpdatedAt.HasValue && mi.UpdatedAt.Value < cutoff24Hours)
                        ))
                    .ToListAsync();

                if (staleInvitations.Count > 0)
                {
                    _context.MeetingInvitations.RemoveRange(staleInvitations);
                    await _context.SaveChangesAsync();
                }

                // Now fetch remaining active invitations
                var invitations = await _context.MeetingInvitations
                    .Include(mi => mi.Team)
                        .ThenInclude(t => t.Student1)
                    .Include(mi => mi.Team)
                        .ThenInclude(t => t.Student2)
                    .Include(mi => mi.Faculty)
                    .Where(mi => mi.TeamId == teamId)
                    .OrderByDescending(mi => mi.MeetingDateTime)
                    .Select(mi => new
                    {
                        mi.Id,
                        mi.Title,
                        mi.Description,
                        MeetingDateTime = mi.MeetingDateTime.ToString("MMM dd, yyyy hh:mm tt"),
                        MeetingDateTimeRaw = mi.MeetingDateTime.ToString("yyyy-MM-ddTHH:mm"),
                        mi.Location,
                        mi.DurationMinutes,
                        mi.Status,
                        Student1Response = mi.Student1Response ?? "Pending",
                        Student2Response = mi.Student2Response ?? (mi.Student2ResponseId.HasValue ? "Pending" : "N/A"),
                        Student1Name = mi.Team.Student1.FullName,
                        Student2Name = mi.Team.Student2 != null ? mi.Team.Student2.FullName : null,
                        CreatedAt = mi.CreatedAt.ToString("MMM dd, yyyy hh:mm tt")
                    })
                    .ToListAsync();

                return Json(new { success = true, invitations });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Faculty/CancelMeetingInvite
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelMeetingInvite(int invitationId)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                var invitation = await _context.MeetingInvitations
                    .FirstOrDefaultAsync(mi => mi.Id == invitationId && mi.FacultyId == facultyId.Value);

                if (invitation == null)
                {
                    return Json(new { success = false, message = "Meeting invitation not found." });
                }

                invitation.Status = "Cancelled";
                invitation.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = invitation.TeamId,
                    Action = "Cancelled Meeting Invitation",
                    Details = invitation.Title,
                    PerformedByRole = "Faculty",
                    PerformedByName = HttpContext.Session.GetString("FacultyName") ?? "Faculty",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Meeting invitation cancelled." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Faculty/EditMeetingInvite
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMeetingInvite(int invitationId, string title, string description, DateTime meetingDateTime, string location, int durationMinutes)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                var invitation = await _context.MeetingInvitations
                    .Include(mi => mi.Team)
                    .FirstOrDefaultAsync(mi => mi.Id == invitationId && mi.FacultyId == facultyId.Value);

                if (invitation == null)
                {
                    return Json(new { success = false, message = "Meeting invitation not found." });
                }

                // Update invitation details
                invitation.Title = title;
                invitation.Description = description;
                invitation.MeetingDateTime = meetingDateTime;
                invitation.Location = location;
                invitation.DurationMinutes = durationMinutes > 0 ? durationMinutes : invitation.DurationMinutes;
                invitation.Status = "Pending";
                invitation.Student1Response = "Pending";
                if (invitation.Student2ResponseId.HasValue)
                {
                    invitation.Student2Response = "Pending";
                }
                invitation.UpdatedAt = DateTime.Now;

                var facultyName = HttpContext.Session.GetString("FacultyName") ?? "Your mentor";

                // Notify students about the update
                if (invitation.Student1ResponseId.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        StudentId = invitation.Student1ResponseId.Value,
                        Message = $"?? Meeting updated by {facultyName}: \"{title}\" - now on {meetingDateTime:MMM dd, yyyy hh:mm tt}. Please respond again.",
                        Type = "warning",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }

                if (invitation.Student2ResponseId.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        StudentId = invitation.Student2ResponseId.Value,
                        Message = $"?? Meeting updated by {facultyName}: \"{title}\" - now on {meetingDateTime:MMM dd, yyyy hh:mm tt}. Please respond again.",
                        Type = "warning",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = invitation.TeamId,
                    Action = "Updated Meeting Invitation",
                    Details = title,
                    PerformedByRole = "Faculty",
                    PerformedByName = facultyName,
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Meeting invitation updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Faculty/DeleteMeetingInvite
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMeetingInvite(int invitationId)
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                var invitation = await _context.MeetingInvitations
                    .Include(mi => mi.Team)
                    .FirstOrDefaultAsync(mi => mi.Id == invitationId && mi.FacultyId == facultyId.Value);

                if (invitation == null)
                {
                    return Json(new { success = false, message = "Meeting invitation not found." });
                }

                var invTitle = invitation.Title;
                var invTeamId = invitation.TeamId;
                var facultyName = HttpContext.Session.GetString("FacultyName") ?? "Your mentor";

                // Notify students about the deletion
                if (invitation.Student1ResponseId.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        StudentId = invitation.Student1ResponseId.Value,
                        Message = $"??? Meeting \"{invTitle}\" has been deleted by {facultyName}.",
                        Type = "danger",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }

                if (invitation.Student2ResponseId.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        StudentId = invitation.Student2ResponseId.Value,
                        Message = $"??? Meeting \"{invTitle}\" has been deleted by {facultyName}.",
                        Type = "danger",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }

                _context.MeetingInvitations.Remove(invitation);
                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = invTeamId,
                    Action = "Deleted Meeting Invitation",
                    Details = invTitle,
                    PerformedByRole = "Faculty",
                    PerformedByName = facultyName,
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Meeting invitation deleted permanently." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
