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
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var faculty = await _context.Faculties
                    .FirstOrDefaultAsync(f => f.Email == model.Email && f.Password == model.Password);

                if (faculty == null)
                {
                    ModelState.AddModelError("", "Invalid credentials!");
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
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Login error: {ex.Message}");
                return View(model);
            }
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
            faculty.Department = model.Department;

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

        // GET: Faculty/AllTeams - View all teams
        [HttpGet]
        public async Task<IActionResult> AllTeams()
        {
            var facultyId = HttpContext.Session.GetInt32("FacultyId");
            if (facultyId == null)
            {
                TempData["ErrorMessage"] = "Please login to view teams.";
                return RedirectToAction("Login");
            }

            var teams = await _context.Teams
                .Include(t => t.Student1)
                .Include(t => t.Student2)
                .OrderBy(t => t.TeamNumber)
                .ToListAsync();

            // Get project progress for each team
            var projectProgresses = await _context.ProjectProgresses
                .Include(pp => pp.AssignedFaculty)
                .ToListAsync();

            ViewBag.ProjectProgresses = projectProgresses;
            ViewBag.FacultyName = HttpContext.Session.GetString("FacultyName");
            ViewBag.CurrentFacultyId = facultyId.Value;

            return View(teams);
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
    }
}
