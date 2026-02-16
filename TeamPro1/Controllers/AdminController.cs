using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamPro1.Models;
using ClosedXML.Excel;

namespace TeamPro1.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Helper: Get current admin's department from session
        private string? GetAdminDepartment()
        {
            return HttpContext.Session.GetString("AdminDepartment");
        }

        // Helper: Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("IsAdmin") == "true"
                && !string.IsNullOrEmpty(GetAdminDepartment());
        }

        // Helper: Filter students by admin department
        private IQueryable<Student> DeptStudents()
        {
            var dept = GetAdminDepartment();
            return _context.Students.Where(s => s.Department == dept);
        }

        // Helper: Filter faculties by admin department
        private IQueryable<Faculty> DeptFaculties()
        {
            var dept = GetAdminDepartment();
            return _context.Faculties.Where(f => f.Department == dept);
        }

        // Helper: Filter teams by admin department (team belongs to dept if Student1 is in that dept)
        private IQueryable<Team> DeptTeams()
        {
            var dept = GetAdminDepartment();
            return _context.Teams
                .Include(t => t.Student1)
                .Include(t => t.Student2)
                .Where(t => t.Student1!.Department == dept);
        }

        // ===================== LOGIN =====================

        [HttpGet]
        public IActionResult Login()
        {
            if (IsAdminLoggedIn())
                return RedirectToAction("Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Email == model.Email);

                // Verify password using BCrypt
                if (admin == null || !BCrypt.Net.BCrypt.Verify(model.Password, admin.Password))
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                // Set session
                HttpContext.Session.SetInt32("AdminId", admin.Id);
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminName", admin.Name);
                HttpContext.Session.SetString("AdminEmail", admin.Email);
                HttpContext.Session.SetString("AdminDepartment", admin.Department);

                return RedirectToAction("Dashboard");
            }

            return View(model);
        }

        // ===================== DASHBOARD =====================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin to access the dashboard.";
                return RedirectToAction("Login");
            }

            var dept = GetAdminDepartment()!;
            var deptStudentIds = await DeptStudents().Select(s => s.Id).ToListAsync();
            var deptTeamIds = await DeptTeams().Select(t => t.Id).ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalStudents = await DeptStudents().CountAsync(),
                TotalFaculties = await DeptFaculties().CountAsync(),
                TotalEnrollments = deptTeamIds.Count,
                Department = dept
            };

            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminEmail = HttpContext.Session.GetString("AdminEmail");
            ViewBag.AdminDepartment = dept;
            ViewBag.TotalTeamRequests = await _context.TeamRequests
                .CountAsync(tr => tr.Status == "Pending"
                    && deptStudentIds.Contains(tr.SenderId));
            ViewBag.TotalProjectProgresses = await _context.ProjectProgresses
                .CountAsync(pp => deptTeamIds.Contains(pp.TeamId));
            ViewBag.CompletedProjects = await _context.ProjectProgresses
                .CountAsync(pp => pp.Status == "Completed" && deptTeamIds.Contains(pp.TeamId));

            return View(viewModel);
        }

        // ===================== MANAGE STUDENTS =====================

        [HttpGet]
        public async Task<IActionResult> ManageStudents()
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var students = await DeptStudents()
                .OrderBy(s => s.RegdNumber)
                .ToListAsync();

            ViewBag.AdminDepartment = GetAdminDepartment();
            return View(students);
        }

        [HttpGet]
        public IActionResult AddStudent()
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var student = new Student { Department = GetAdminDepartment()! };
            ViewBag.AdminDepartment = GetAdminDepartment();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(Student model)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            // Force department to admin's department
            model.Department = GetAdminDepartment()!;
            ViewBag.AdminDepartment = model.Department;

            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingEmail = await _context.Students
                    .FirstOrDefaultAsync(s => s.Email == model.Email);

                if (existingEmail != null)
                {
                    TempData["ErrorMessage"] = "Email already exists";
                    return RedirectToAction("ManageStudents");
                }

                // Check if registration number already exists
                var existingRegd = await _context.Students
                    .FirstOrDefaultAsync(s => s.RegdNumber == model.RegdNumber);

                if (existingRegd != null)
                {
                    TempData["ErrorMessage"] = "Registration number already exists";
                    return RedirectToAction("ManageStudents");
                }

                // Hash password using BCrypt before storing
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                model.Department = GetAdminDepartment() ?? "Computer Science";

                _context.Students.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Student added successfully";
                return RedirectToAction("ManageStudents");
            }

            return RedirectToAction("ManageStudents");
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null || student.Department != GetAdminDepartment())
            {
                TempData["ErrorMessage"] = "Student not found in your department.";
                return RedirectToAction("ManageStudents");
            }

            ViewBag.AdminDepartment = GetAdminDepartment();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(Student model)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            model.Department = GetAdminDepartment()!;
            ViewBag.AdminDepartment = model.Department;

            if (ModelState.IsValid)
            {
                var student = await _context.Students.FindAsync(model.Id);
                if (student == null || student.Department != GetAdminDepartment())
                {
                    TempData["ErrorMessage"] = "Student not found in your department.";
                    return RedirectToAction("ManageStudents");
                }

                // Update student details
                student.FullName = model.FullName;
                student.Email = model.Email;
                student.RegdNumber = model.RegdNumber;
                student.Year = model.Year;
                student.Semester = model.Semester;

                // Only update password if a new one is provided
                if (!string.IsNullOrEmpty(model.Password))
                {
                    student.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Student updated successfully";
                return RedirectToAction("ManageStudents");
            }

            return RedirectToAction("ManageStudents");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null || student.Department != GetAdminDepartment())
                    return Json(new { success = false, message = "Student not found in your department." });

                var isInTeam = await _context.Teams
                    .AnyAsync(t => t.Student1Id == id || t.Student2Id == id);
                if (isInTeam)
                    return Json(new { success = false, message = "Cannot delete student who is part of a team. Remove them from the team first." });

                var pendingRequests = await _context.TeamRequests
                    .Where(tr => tr.SenderId == id || tr.ReceiverId == id)
                    .ToListAsync();
                _context.TeamRequests.RemoveRange(pendingRequests);

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Student '{student.FullName}' deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting student: {ex.Message}" });
            }
        }

        // ===================== MANAGE FACULTY =====================

        [HttpGet]
        public async Task<IActionResult> ManageFaculty()
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var faculties = await DeptFaculties()
                .OrderBy(f => f.FullName)
                .ToListAsync();

            ViewBag.AdminDepartment = GetAdminDepartment();
            return View(faculties);
        }

        [HttpGet]
        public IActionResult AddFaculty()
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var faculty = new Faculty { Department = GetAdminDepartment()! };
            ViewBag.AdminDepartment = GetAdminDepartment();
            return View(faculty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFaculty(Faculty model)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            model.Department = GetAdminDepartment()!;
            ViewBag.AdminDepartment = model.Department;

            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingFaculty = await _context.Faculties
                    .FirstOrDefaultAsync(f => f.Email == model.Email);

                if (existingFaculty != null)
                {
                    TempData["ErrorMessage"] = "Faculty email already exists";
                    return RedirectToAction("ManageFaculty");
                }

                // Auto-generate FacultyId based on department
                var existingFacultyIds = await _context.Faculties
                    .Where(f => f.Department == model.Department)
                    .Select(f => f.FacultyId)
                    .ToListAsync();

                model.FacultyId = TeamPro1.Utilities.FacultyIdGenerator.GenerateNextFacultyId(
                    model.Department, 
                    existingFacultyIds
                );

                // Hash password using BCrypt before storing
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                model.Department = GetAdminDepartment() ?? "Computer Science";

                _context.Faculties.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Faculty added successfully! Faculty ID: {model.FacultyId}";
                return RedirectToAction("ManageFaculty");
            }

            return RedirectToAction("ManageFaculty");
        }

        [HttpGet]
        public async Task<IActionResult> EditFaculty(int id)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null || faculty.Department != GetAdminDepartment())
            {
                TempData["ErrorMessage"] = "Faculty not found in your department.";
                return RedirectToAction("ManageFaculty");
            }

            ViewBag.AdminDepartment = GetAdminDepartment();
            return View(faculty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFaculty(Faculty model)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            model.Department = GetAdminDepartment()!;
            ViewBag.AdminDepartment = model.Department;

            if (ModelState.IsValid)
            {
                var faculty = await _context.Faculties.FindAsync(model.Id);
                if (faculty == null || faculty.Department != GetAdminDepartment())
                {
                    TempData["ErrorMessage"] = "Faculty not found in your department.";
                    return RedirectToAction("ManageFaculty");
                }

                // Update faculty details
                faculty.FullName = model.FullName;
                faculty.Email = model.Email;

                // Only update password if a new one is provided
                if (!string.IsNullOrEmpty(model.Password))
                {
                    faculty.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Faculty updated successfully";
                return RedirectToAction("ManageFaculty");
            }

            return RedirectToAction("ManageFaculty");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFaculty(int id)
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                var faculty = await _context.Faculties.FindAsync(id);
                if (faculty == null || faculty.Department != GetAdminDepartment())
                    return Json(new { success = false, message = "Faculty not found in your department." });

                var isMentor = await _context.ProjectProgresses
                    .AnyAsync(pp => pp.AssignedFacultyId == id);
                if (isMentor)
                    return Json(new { success = false, message = "Cannot delete faculty who is assigned as mentor to teams. Remove mentor assignment first." });

                _context.Faculties.Remove(faculty);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Faculty '{faculty.FullName}' deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting faculty: {ex.Message}" });
            }
        }

        // ===================== MANAGE TEAMS =====================

        [HttpGet]
        public async Task<IActionResult> ManageTeams()
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var teams = await DeptTeams()
                .OrderBy(t => t.TeamNumber)
                .ToListAsync();

            var deptTeamIds = teams.Select(t => t.Id).ToList();

            var projectProgresses = await _context.ProjectProgresses
                .Include(pp => pp.AssignedFaculty)
                .Where(pp => deptTeamIds.Contains(pp.TeamId))
                .ToListAsync();

            var dept = GetAdminDepartment()!;
            var problemStatements = await _context.ProblemStatementBanks
                .Where(ps => ps.Department == dept)
                .OrderBy(ps => ps.Year).ThenBy(ps => ps.Statement)
                .ToListAsync();

            ViewBag.ProjectProgresses = projectProgresses;
            ViewBag.Faculties = await DeptFaculties().OrderBy(f => f.FullName).ToListAsync();
            ViewBag.ProblemStatements = problemStatements;
            ViewBag.AdminDepartment = dept;

            return View(teams);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                var team = await _context.Teams
                    .Include(t => t.Student1)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (team == null || team.Student1?.Department != GetAdminDepartment())
                    return Json(new { success = false, message = "Team not found in your department." });

                var projectProgress = await _context.ProjectProgresses
                    .Where(pp => pp.TeamId == id).ToListAsync();
                _context.ProjectProgresses.RemoveRange(projectProgress);

                var teamMeetings = await _context.TeamMeetings
                    .Where(tm => tm.TeamId == id).ToListAsync();
                _context.TeamMeetings.RemoveRange(teamMeetings);

                var teamRequests = await _context.TeamRequests
                    .Where(tr => (tr.SenderId == team.Student1Id && tr.ReceiverId == team.Student2Id) ||
                                 (tr.SenderId == team.Student2Id && tr.ReceiverId == team.Student1Id))
                    .ToListAsync();
                _context.TeamRequests.RemoveRange(teamRequests);

                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Team #{team.TeamNumber} deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting team: {ex.Message}" });
            }
        }

        // ===================== VIEW ALL PROGRESS =====================

        [HttpGet]
        public async Task<IActionResult> ViewAllProgress()
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var dept = GetAdminDepartment()!;
            var deptTeamIds = await DeptTeams().Select(t => t.Id).ToListAsync();

            var projectProgresses = await _context.ProjectProgresses
                .Include(pp => pp.Team)
                    .ThenInclude(t => t.Student1)
                .Include(pp => pp.Team)
                    .ThenInclude(t => t.Student2)
                .Include(pp => pp.AssignedFaculty)
                .Where(pp => deptTeamIds.Contains(pp.TeamId))
                .OrderBy(pp => pp.Team.TeamNumber)
                .ToListAsync();

            ViewBag.AdminDepartment = dept;
            return View(projectProgresses);
        }

        // ===================== TEAM DETAILS =====================

        [HttpGet]
        public async Task<IActionResult> TeamDetails(int id)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var team = await _context.Teams
                .Include(t => t.Student1)
                .Include(t => t.Student2)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null || team.Student1?.Department != GetAdminDepartment())
            {
                TempData["ErrorMessage"] = "Team not found in your department.";
                return RedirectToAction("ManageTeams");
            }

            var projectProgress = await _context.ProjectProgresses
                .Include(pp => pp.AssignedFaculty)
                .FirstOrDefaultAsync(pp => pp.TeamId == id);

            var teamMeetings = await _context.TeamMeetings
                .Where(tm => tm.TeamId == id)
                .OrderBy(tm => tm.MeetingNumber)
                .ToListAsync();

            // Only show same-department faculties for mentor assignment
            var faculties = await DeptFaculties().ToListAsync();

            ViewBag.Team = team;
            ViewBag.ProjectProgress = projectProgress;
            ViewBag.TeamMeetings = teamMeetings;
            ViewBag.Faculties = faculties;
            ViewBag.AdminDepartment = GetAdminDepartment();

            return View();
        }

        // POST: Admin/AssignFaculty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignFaculty(int teamId, int facultyId)
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                var team = await _context.Teams
                    .Include(t => t.Student1)
                    .FirstOrDefaultAsync(t => t.Id == teamId);

                if (team == null || team.Student1?.Department != GetAdminDepartment())
                    return Json(new { success = false, message = "Team not found in your department." });

                var faculty = await _context.Faculties.FindAsync(facultyId);
                if (faculty == null || faculty.Department != GetAdminDepartment())
                    return Json(new { success = false, message = "Faculty not found in your department." });

                var progress = await _context.ProjectProgresses
                    .FirstOrDefaultAsync(pp => pp.TeamId == teamId);

                if (progress == null)
                {
                    progress = new ProjectProgress
                    {
                        TeamId = teamId,
                        AssignedFacultyId = facultyId,
                        Status = "Mentor Assigned",
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };
                    _context.ProjectProgresses.Add(progress);
                }
                else
                {
                    progress.AssignedFacultyId = facultyId;
                    progress.LastUpdated = DateTime.Now;

                    if (!string.IsNullOrEmpty(progress.ProblemStatement))
                    {
                        progress.Status = "In Progress";
                    }
                    else if (progress.Status == "Pending" || progress.Status == "Not Started")
                    {
                        progress.Status = "Mentor Assigned";
                    }
                }

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = teamId,
                    Action = "Assigned Mentor (Admin)",
                    Details = $"Mentor: {faculty.FullName}",
                    PerformedByRole = "Admin",
                    PerformedByName = HttpContext.Session.GetString("AdminName") ?? "Admin",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Faculty '{faculty.FullName}' assigned as mentor to Team #{team.TeamNumber}!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error assigning faculty: {ex.Message}" });
            }
        }

        // POST: Admin/SetProblemStatement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetProblemStatement(int teamId, string problemStatement)
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                var team = await _context.Teams
                    .Include(t => t.Student1)
                    .FirstOrDefaultAsync(t => t.Id == teamId);

                if (team == null || team.Student1?.Department != GetAdminDepartment())
                    return Json(new { success = false, message = "Team not found in your department." });

                if (string.IsNullOrWhiteSpace(problemStatement))
                    return Json(new { success = false, message = "Problem statement cannot be empty." });

                var progress = await _context.ProjectProgresses
                    .FirstOrDefaultAsync(pp => pp.TeamId == teamId);

                if (progress == null)
                {
                    progress = new ProjectProgress
                    {
                        TeamId = teamId,
                        ProblemStatement = problemStatement.Trim(),
                        Status = "Problem Statement Assigned",
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };
                    _context.ProjectProgresses.Add(progress);
                }
                else
                {
                    progress.ProblemStatement = problemStatement.Trim();
                    progress.LastUpdated = DateTime.Now;

                    if (progress.AssignedFacultyId != null)
                    {
                        progress.Status = "In Progress";
                    }
                    else if (progress.Status == "Pending" || progress.Status == "Not Started")
                    {
                        progress.Status = "Problem Statement Assigned";
                    }
                }

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = teamId,
                    Action = "Set Problem Statement (Admin)",
                    Details = problemStatement.Trim().Length > 100 ? problemStatement.Trim().Substring(0, 100) + "..." : problemStatement.Trim(),
                    PerformedByRole = "Admin",
                    PerformedByName = HttpContext.Session.GetString("AdminName") ?? "Admin",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Problem statement updated for Team #{team.TeamNumber}!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error setting problem statement: {ex.Message}" });
            }
        }

        // ===================== LOGOUT =====================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Admin logged out successfully!";
            return RedirectToAction("Login");
        }

        // ===================== SCHEDULE TEAM FORMATION =====================

        [HttpGet]
        public async Task<IActionResult> ScheduleTeamFormation()
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var dept = GetAdminDepartment()!;

            // Get existing schedules for this department
            var schedules = await _context.TeamFormationSchedules
                .Where(s => s.Department == dept)
                .OrderBy(s => s.Year)
                .ToListAsync();

            // Ensure schedules exist for years 1-4
            var existingYears = schedules.Select(s => s.Year).ToHashSet();
            for (int year = 1; year <= 4; year++)
            {
                if (!existingYears.Contains(year))
                {
                    var newSchedule = new TeamFormationSchedule
                    {
                        Department = dept,
                        Year = year,
                        IsOpen = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.TeamFormationSchedules.Add(newSchedule);
                    schedules.Add(newSchedule);
                }
            }
            await _context.SaveChangesAsync();

            // Re-fetch to get IDs
            schedules = await _context.TeamFormationSchedules
                .Where(s => s.Department == dept)
                .OrderBy(s => s.Year)
                .ToListAsync();

            ViewBag.AdminDepartment = dept;
            return View(schedules);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTeamFormation(int year, bool isOpen)
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                var dept = GetAdminDepartment()!;

                var schedule = await _context.TeamFormationSchedules
                    .FirstOrDefaultAsync(s => s.Department == dept && s.Year == year);

                if (schedule == null)
                {
                    schedule = new TeamFormationSchedule
                    {
                        Department = dept,
                        Year = year,
                        IsOpen = isOpen,
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };
                    if (isOpen) schedule.OpenedAt = DateTime.Now;
                    else schedule.ClosedAt = DateTime.Now;
                    _context.TeamFormationSchedules.Add(schedule);
                }
                else
                {
                    schedule.IsOpen = isOpen;
                    schedule.LastUpdated = DateTime.Now;
                    if (isOpen) schedule.OpenedAt = DateTime.Now;
                    else schedule.ClosedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                var status = isOpen ? "opened" : "closed";
                return Json(new { success = true, message = $"Team formation for Year {year} has been {status}." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ===================== EXPORT STUDENTS =====================

        [HttpGet]
        public async Task<IActionResult> ExportStudentsExcel()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var students = await DeptStudents().OrderBy(s => s.RegdNumber).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Students");

            // Header row
            worksheet.Cell(1, 1).Value = "S.No";
            worksheet.Cell(1, 2).Value = "Full Name";
            worksheet.Cell(1, 3).Value = "Registration No";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Year";
            worksheet.Cell(1, 6).Value = "Semester";
            worksheet.Cell(1, 7).Value = "Department";

            // Style header
            var headerRange = worksheet.Range(1, 1, 1, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#6f42c1");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Data rows
            for (int i = 0; i < students.Count; i++)
            {
                var s = students[i];
                worksheet.Cell(i + 2, 1).Value = i + 1;
                worksheet.Cell(i + 2, 2).Value = s.FullName;
                worksheet.Cell(i + 2, 3).Value = s.RegdNumber;
                worksheet.Cell(i + 2, 4).Value = s.Email;
                worksheet.Cell(i + 2, 5).Value = s.Year;
                worksheet.Cell(i + 2, 6).Value = s.Semester;
                worksheet.Cell(i + 2, 7).Value = s.Department;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Students_{GetAdminDepartment()}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportFacultyExcel()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var faculties = await DeptFaculties().OrderBy(f => f.FacultyId).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Faculty");

            // Header row
            worksheet.Cell(1, 1).Value = "Faculty ID";
            worksheet.Cell(1, 2).Value = "Full Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Department";
            worksheet.Cell(1, 5).Value = "Joined Date";

            // Style header
            var headerRange = worksheet.Range(1, 1, 1, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#6f42c1");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Data rows
            for (int i = 0; i < faculties.Count; i++)
            {
                var f = faculties[i];
                worksheet.Cell(i + 2, 1).Value = f.FacultyId;
                worksheet.Cell(i + 2, 2).Value = f.FullName;
                worksheet.Cell(i + 2, 3).Value = f.Email;
                worksheet.Cell(i + 2, 4).Value = f.Department;
                worksheet.Cell(i + 2, 5).Value = f.CreatedAt.ToString("MMM dd, yyyy");
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Faculty_{GetAdminDepartment()}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public IActionResult DownloadStudentTemplate()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Student Template");

            worksheet.Cell(1, 1).Value = "FullName";
            worksheet.Cell(1, 2).Value = "RegdNumber";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Password";
            worksheet.Cell(1, 5).Value = "Year";
            worksheet.Cell(1, 6).Value = "Semester";

            // Style header
            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#6f42c1");
            headerRange.Style.Font.FontColor = XLColor.White;

            // Sample row
            worksheet.Cell(2, 1).Value = "John Doe";
            worksheet.Cell(2, 2).Value = "23091A32D1";
            worksheet.Cell(2, 3).Value = "john@rgmcet.edu.in";
            worksheet.Cell(2, 4).Value = "Pass@1234";
            worksheet.Cell(2, 5).Value = 3;
            worksheet.Cell(2, 6).Value = 1;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Student_Upload_Template.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUploadStudents(IFormFile excelFile)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid Excel file.";
                return RedirectToAction("ManageStudents");
            }

            if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only .xlsx files are supported.";
                return RedirectToAction("ManageStudents");
            }

            try
            {
                var dept = GetAdminDepartment()!;
                int addedCount = 0;
                var errors = new List<string>();

                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.First();
                var rows = worksheet.RowsUsed().Skip(1); // Skip header

                foreach (var row in rows)
                {
                    try
                    {
                        var fullName = row.Cell(1).GetString().Trim();
                        var regdNumber = row.Cell(2).GetString().Trim();
                        var email = row.Cell(3).GetString().Trim();
                        var password = row.Cell(4).GetString().Trim();
                        var yearStr = row.Cell(5).GetString().Trim();
                        var semStr = row.Cell(6).GetString().Trim();

                        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(regdNumber) || string.IsNullOrEmpty(email))
                        {
                            errors.Add($"Row {row.RowNumber()}: Missing required fields.");
                            continue;
                        }

                        if (await _context.Students.AnyAsync(s => s.Email == email || s.RegdNumber == regdNumber))
                        {
                            errors.Add($"Row {row.RowNumber()}: Student with email '{email}' or reg no '{regdNumber}' already exists.");
                            continue;
                        }

                        int.TryParse(yearStr, out int year);
                        int.TryParse(semStr, out int semester);

                        var student = new Student
                        {
                            FullName = fullName,
                            RegdNumber = regdNumber,
                            Email = email,
                            Password = BCrypt.Net.BCrypt.HashPassword(string.IsNullOrEmpty(password) ? "rgmcet@123" : password),
                            Year = year > 0 ? year : 1,
                            Semester = semester > 0 ? semester : 1,
                            Department = dept,
                            CreatedAt = DateTime.Now
                        };

                        _context.Students.Add(student);
                        addedCount++;
                    }
                    catch (Exception rowEx)
                    {
                        errors.Add($"Row {row.RowNumber()}: {rowEx.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully added {addedCount} student(s)!";
                if (errors.Count > 0)
                    TempData["ErrorDetails"] = string.Join(" | ", errors);

                return RedirectToAction("ManageStudents");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing file: {ex.Message}";
                return RedirectToAction("ManageStudents");
            }
        }

        // ===================== PROBLEM STATEMENT BANK =====================

        [HttpGet]
        public async Task<IActionResult> ManageProblemStatements()
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            var dept = GetAdminDepartment()!;
            var statements = await _context.ProblemStatementBanks
                .Where(ps => ps.Department == dept)
                .OrderBy(ps => ps.Year).ThenBy(ps => ps.Statement)
                .ToListAsync();

            ViewBag.AdminDepartment = dept;
            return View(statements);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProblemStatement(string statement, int year)
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                if (string.IsNullOrWhiteSpace(statement))
                    return Json(new { success = false, message = "Problem statement cannot be empty." });

                var dept = GetAdminDepartment()!;

                if (await _context.ProblemStatementBanks.AnyAsync(ps => ps.Statement == statement.Trim() && ps.Department == dept))
                    return Json(new { success = false, message = "This problem statement already exists." });

                var ps = new ProblemStatementBank
                {
                    Statement = statement.Trim(),
                    Year = year > 0 ? year : 3,
                    Department = dept,
                    CreatedAt = DateTime.Now
                };

                _context.ProblemStatementBanks.Add(ps);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Problem statement added successfully!", id = ps.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProblemStatement(int id, string statement, int year)
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                var ps = await _context.ProblemStatementBanks.FindAsync(id);
                if (ps == null || ps.Department != GetAdminDepartment())
                    return Json(new { success = false, message = "Problem statement not found." });

                if (string.IsNullOrWhiteSpace(statement))
                    return Json(new { success = false, message = "Problem statement cannot be empty." });

                ps.Statement = statement.Trim();
                ps.Year = year > 0 ? year : ps.Year;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Problem statement updated!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProblemStatement(int id)
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                var ps = await _context.ProblemStatementBanks.FindAsync(id);
                if (ps == null || ps.Department != GetAdminDepartment())
                    return Json(new { success = false, message = "Problem statement not found." });

                _context.ProblemStatementBanks.Remove(ps);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Problem statement deleted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult DownloadProblemStatementTemplate()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var dept = GetAdminDepartment()!;

            using var workbook = new XLWorkbook();

            // ===== INSTRUCTIONS SHEET =====
            var instrSheet = workbook.Worksheets.Add("INSTRUCTIONS - READ FIRST");

            // Title
            instrSheet.Cell(1, 1).Value = "PROBLEM STATEMENT UPLOAD TEMPLATE - INSTRUCTIONS";
            instrSheet.Range(1, 1, 1, 4).Merge();
            instrSheet.Cell(1, 1).Style.Font.Bold = true;
            instrSheet.Cell(1, 1).Style.Font.FontSize = 16;
            instrSheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            instrSheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#6f42c1");
            instrSheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Department info
            instrSheet.Cell(3, 1).Value = "YOUR DEPARTMENT:";
            instrSheet.Cell(3, 1).Style.Font.Bold = true;
            instrSheet.Cell(3, 1).Style.Font.FontColor = XLColor.FromHtml("#274060");
            instrSheet.Cell(3, 2).Value = dept;
            instrSheet.Cell(3, 2).Style.Font.Bold = true;
            instrSheet.Cell(3, 2).Style.Font.FontSize = 14;
            instrSheet.Cell(3, 2).Style.Font.FontColor = XLColor.FromHtml("#28a745");

            // Important notes
            var noteRow = 5;
            instrSheet.Cell(noteRow, 1).Value = "IMPORTANT NOTES:";
            instrSheet.Cell(noteRow, 1).Style.Font.Bold = true;
            instrSheet.Cell(noteRow, 1).Style.Font.FontColor = XLColor.Red;
            instrSheet.Cell(noteRow, 1).Style.Font.FontSize = 13;

            var notes = new[]
            {
                $"1. Only problem statements for YOUR department ({dept}) will be uploaded. Rows with a different department will be SKIPPED.",
                $"2. The Department column must contain exactly: {dept}  (it is pre-filled for you in the data sheet).",
                "3. YEAR FORMAT: Use NUMBERS only ? 1, 2, 3, or 4.",
                "   ? Do NOT use Roman numerals (I, II, III, IV).",
                "   ? Do NOT use text (First, Second, Third, Fourth).",
                "   ? Do NOT use 1st, 2nd, 3rd, 4th.",
                "   ? Correct examples: 1, 2, 3, 4",
                "4. The Statement column is REQUIRED. Empty rows will be skipped.",
                "5. Duplicate problem statements (already existing in the database) will be skipped.",
                "6. Do NOT modify the header row (Row 1) in the data sheet.",
                "7. Delete the sample rows before adding your own data, or add below them.",
                "8. Save the file as .xlsx format before uploading."
            };

            for (int i = 0; i < notes.Length; i++)
            {
                instrSheet.Cell(noteRow + 1 + i, 1).Value = notes[i];
                instrSheet.Range(noteRow + 1 + i, 1, noteRow + 1 + i, 4).Merge();

                if (notes[i].Contains("YEAR FORMAT"))
                {
                    instrSheet.Cell(noteRow + 1 + i, 1).Style.Font.Bold = true;
                    instrSheet.Cell(noteRow + 1 + i, 1).Style.Font.FontColor = XLColor.Red;
                }
                else if (notes[i].StartsWith("   ?"))
                {
                    instrSheet.Cell(noteRow + 1 + i, 1).Style.Font.FontColor = XLColor.Red;
                }
                else if (notes[i].StartsWith("   ?"))
                {
                    instrSheet.Cell(noteRow + 1 + i, 1).Style.Font.FontColor = XLColor.FromHtml("#28a745");
                    instrSheet.Cell(noteRow + 1 + i, 1).Style.Font.Bold = true;
                }
            }

            // Column descriptions
            var colDescRow = noteRow + notes.Length + 3;
            instrSheet.Cell(colDescRow, 1).Value = "COLUMN DESCRIPTIONS:";
            instrSheet.Cell(colDescRow, 1).Style.Font.Bold = true;
            instrSheet.Cell(colDescRow, 1).Style.Font.FontColor = XLColor.FromHtml("#274060");
            instrSheet.Cell(colDescRow, 1).Style.Font.FontSize = 13;

            var colHeaders = new[] { "Column", "Name", "Required?", "Description" };
            for (int c = 0; c < colHeaders.Length; c++)
            {
                instrSheet.Cell(colDescRow + 1, c + 1).Value = colHeaders[c];
                instrSheet.Cell(colDescRow + 1, c + 1).Style.Font.Bold = true;
                instrSheet.Cell(colDescRow + 1, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#274060");
                instrSheet.Cell(colDescRow + 1, c + 1).Style.Font.FontColor = XLColor.White;
            }

            instrSheet.Cell(colDescRow + 2, 1).Value = "A";
            instrSheet.Cell(colDescRow + 2, 2).Value = "Statement";
            instrSheet.Cell(colDescRow + 2, 3).Value = "YES";
            instrSheet.Cell(colDescRow + 2, 3).Style.Font.FontColor = XLColor.Red;
            instrSheet.Cell(colDescRow + 2, 3).Style.Font.Bold = true;
            instrSheet.Cell(colDescRow + 2, 4).Value = "The problem statement text. Cannot be empty.";

            instrSheet.Cell(colDescRow + 3, 1).Value = "B";
            instrSheet.Cell(colDescRow + 3, 2).Value = "Year";
            instrSheet.Cell(colDescRow + 3, 3).Value = "YES";
            instrSheet.Cell(colDescRow + 3, 3).Style.Font.FontColor = XLColor.Red;
            instrSheet.Cell(colDescRow + 3, 3).Style.Font.Bold = true;
            instrSheet.Cell(colDescRow + 3, 4).Value = "Use numbers: 1, 2, 3, or 4 ONLY. Default is 3 if invalid.";

            instrSheet.Cell(colDescRow + 4, 1).Value = "C";
            instrSheet.Cell(colDescRow + 4, 2).Value = "Department";
            instrSheet.Cell(colDescRow + 4, 3).Value = "YES";
            instrSheet.Cell(colDescRow + 4, 3).Style.Font.FontColor = XLColor.Red;
            instrSheet.Cell(colDescRow + 4, 3).Style.Font.Bold = true;
            instrSheet.Cell(colDescRow + 4, 4).Value = $"Must be exactly: {dept}. Rows with different department will be skipped.";

            instrSheet.Columns().AdjustToContents();
            instrSheet.Column(4).Width = 60;

            // ===== DATA SHEET =====
            var dataSheet = workbook.Worksheets.Add("Problem Statements Data");

            // Header row
            dataSheet.Cell(1, 1).Value = "Statement";
            dataSheet.Cell(1, 2).Value = "Year";
            dataSheet.Cell(1, 3).Value = "Department";

            var headerRange = dataSheet.Range(1, 1, 1, 3);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#6f42c1");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Sample rows with department pre-filled
            dataSheet.Cell(2, 1).Value = "Build a student management system using ASP.NET Core";
            dataSheet.Cell(2, 2).Value = 3;
            dataSheet.Cell(2, 3).Value = dept;

            dataSheet.Cell(3, 1).Value = "Design a machine learning model for sentiment analysis";
            dataSheet.Cell(3, 2).Value = 4;
            dataSheet.Cell(3, 3).Value = dept;

            dataSheet.Cell(4, 1).Value = "Develop a mobile app for campus navigation";
            dataSheet.Cell(4, 2).Value = 3;
            dataSheet.Cell(4, 3).Value = dept;

            // Style sample rows slightly different
            var sampleRange = dataSheet.Range(2, 1, 4, 3);
            sampleRange.Style.Font.FontColor = XLColor.Gray;

            // Info note row
            dataSheet.Cell(6, 1).Value = $"? DELETE the sample rows above (rows 2-4) and add your own data. Department must be: {dept}. Year must be a number (1, 2, 3, or 4).";
            dataSheet.Range(6, 1, 6, 3).Merge();
            dataSheet.Cell(6, 1).Style.Font.Bold = true;
            dataSheet.Cell(6, 1).Style.Font.FontColor = XLColor.Red;
            dataSheet.Cell(6, 1).Style.Font.FontSize = 10;

            dataSheet.Columns().AdjustToContents();
            dataSheet.Column(1).Width = 60;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ProblemStatement_Template_{dept}.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUploadProblemStatements(IFormFile excelFile)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login as admin.";
                return RedirectToAction("Login");
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid Excel file.";
                return RedirectToAction("ManageProblemStatements");
            }

            if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only .xlsx files are supported.";
                return RedirectToAction("ManageProblemStatements");
            }

            try
            {
                var dept = GetAdminDepartment()!;
                int addedCount = 0;
                int skippedDeptCount = 0;
                var errors = new List<string>();

                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);

                // Find the data sheet - prefer "Problem Statements Data", fallback to first sheet
                var worksheet = workbook.Worksheets.FirstOrDefault(ws =>
                    ws.Name.Contains("Data", StringComparison.OrdinalIgnoreCase)
                    || ws.Name.Contains("Problem Statement", StringComparison.OrdinalIgnoreCase))
                    ?? workbook.Worksheets.First();

                // Skip instructions-only sheets
                if (worksheet.Name.Contains("INSTRUCTION", StringComparison.OrdinalIgnoreCase) && workbook.Worksheets.Count > 1)
                {
                    worksheet = workbook.Worksheets.Skip(1).First();
                }

                var rows = worksheet.RowsUsed().Skip(1); // Skip header

                // Check if Department column exists (column 3)
                var hasDepCol = worksheet.Cell(1, 3).GetString().Trim().Equals("Department", StringComparison.OrdinalIgnoreCase);

                foreach (var row in rows)
                {
                    try
                    {
                        var statement = row.Cell(1).GetString().Trim();
                        var yearStr = row.Cell(2).GetString().Trim();

                        if (string.IsNullOrEmpty(statement))
                        {
                            errors.Add($"Row {row.RowNumber()}: Missing problem statement.");
                            continue;
                        }

                        // Skip note/warning rows
                        if (statement.StartsWith("Note:", StringComparison.OrdinalIgnoreCase)
                            || statement.StartsWith("?", StringComparison.OrdinalIgnoreCase)
                            || statement.StartsWith("WARNING", StringComparison.OrdinalIgnoreCase)
                            || statement.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Validate department if column exists
                        if (hasDepCol)
                        {
                            var rowDept = row.Cell(3).GetString().Trim();
                            if (!string.IsNullOrEmpty(rowDept) && !rowDept.Equals(dept, StringComparison.OrdinalIgnoreCase))
                            {
                                errors.Add($"Row {row.RowNumber()}: Department '{rowDept}' does not match your department '{dept}'. Skipped.");
                                skippedDeptCount++;
                                continue;
                            }
                        }

                        if (await _context.ProblemStatementBanks.AnyAsync(ps => ps.Statement == statement && ps.Department == dept))
                        {
                            errors.Add($"Row {row.RowNumber()}: '{(statement.Length > 50 ? statement.Substring(0, 50) + "..." : statement)}' already exists.");
                            continue;
                        }

                        int.TryParse(yearStr, out int year);

                        var ps = new ProblemStatementBank
                        {
                            Statement = statement,
                            Year = year > 0 ? year : 3,
                            Department = dept,
                            CreatedAt = DateTime.Now
                        };

                        _context.ProblemStatementBanks.Add(ps);
                        addedCount++;
                    }
                    catch (Exception rowEx)
                    {
                        errors.Add($"Row {row.RowNumber()}: {rowEx.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                var message = $"Successfully added {addedCount} problem statement(s) for {dept}!";
                if (skippedDeptCount > 0)
                    message += $" {skippedDeptCount} skipped due to department mismatch.";

                TempData["SuccessMessage"] = message;
                if (errors.Count > 0)
                    TempData["ErrorDetails"] = string.Join(" | ", errors);

                return RedirectToAction("ManageProblemStatements");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing file: {ex.Message}";
                return RedirectToAction("ManageProblemStatements");
            }
        }

        // ===================== DELETE ALL STUDENTS =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllStudents()
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                var dept = GetAdminDepartment()!;

                // Get all student IDs in this department
                var deptStudentIds = await DeptStudents().Select(s => s.Id).ToListAsync();

                if (!deptStudentIds.Any())
                    return Json(new { success = false, message = "No students found in your department." });

                // Get all team IDs that belong to this department's students
                var deptTeamIds = await _context.Teams
                    .Where(t => deptStudentIds.Contains(t.Student1Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                // 1. Delete team activity logs for these teams
                var activityLogs = await _context.TeamActivityLogs
                    .Where(al => deptTeamIds.Contains(al.TeamId))
                    .ToListAsync();
                _context.TeamActivityLogs.RemoveRange(activityLogs);

                // 2. Delete team meetings for these teams
                var teamMeetings = await _context.TeamMeetings
                    .Where(tm => deptTeamIds.Contains(tm.TeamId))
                    .ToListAsync();
                _context.TeamMeetings.RemoveRange(teamMeetings);

                // 3. Delete project progresses for these teams
                var projectProgresses = await _context.ProjectProgresses
                    .Where(pp => deptTeamIds.Contains(pp.TeamId))
                    .ToListAsync();
                _context.ProjectProgresses.RemoveRange(projectProgresses);

                // 4. Delete team requests involving these students
                var teamRequests = await _context.TeamRequests
                    .Where(tr => deptStudentIds.Contains(tr.SenderId) || deptStudentIds.Contains(tr.ReceiverId))
                    .ToListAsync();
                _context.TeamRequests.RemoveRange(teamRequests);

                // 5. Delete teams
                var teams = await _context.Teams
                    .Where(t => deptTeamIds.Contains(t.Id))
                    .ToListAsync();
                _context.Teams.RemoveRange(teams);

                // 6. Delete all students in this department
                var students = await DeptStudents().ToListAsync();
                _context.Students.RemoveRange(students);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Successfully deleted {students.Count} student(s), {teams.Count} team(s), and all related data from {dept}."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting students: {ex.Message}" });
            }
        }

        // ===================== DELETE ALL PROBLEM STATEMENTS =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllProblemStatements()
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login as admin." });

            try
            {
                var dept = GetAdminDepartment()!;

                var statements = await _context.ProblemStatementBanks
                    .Where(ps => ps.Department == dept)
                    .ToListAsync();

                if (!statements.Any())
                    return Json(new { success = false, message = "No problem statements found in your department." });

                _context.ProblemStatementBanks.RemoveRange(statements);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Successfully deleted {statements.Count} problem statement(s) from {dept}."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting problem statements: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult DownloadFacultyTemplate()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var dept = GetAdminDepartment()!;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Faculty Template");

            // Header row with Faculty ID included
            worksheet.Cell(1, 1).Value = "FacultyId";
            worksheet.Cell(1, 2).Value = "FullName";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Password";
            worksheet.Cell(1, 5).Value = "Department";

            // Style header
            var headerRange = worksheet.Range(1, 1, 1, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#6f42c1");
            headerRange.Style.Font.FontColor = XLColor.White;

            // Sample row with Faculty ID example
            var deptCode = TeamPro1.Utilities.FacultyIdGenerator.GetDepartmentCode(dept);
            worksheet.Cell(2, 1).Value = $"{deptCode}01 (AUTO)";
            worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
            
            worksheet.Cell(2, 2).Value = "Dr. John Doe";
            worksheet.Cell(2, 3).Value = "john.doe@rgmcet.edu.in";
            worksheet.Cell(2, 4).Value = "Pass@1234";
            worksheet.Cell(2, 5).Value = dept;

            // Make sample row gray to indicate it's just an example
            var sampleRange = worksheet.Range(2, 1, 2, 5);
            sampleRange.Style.Font.FontColor = XLColor.Gray;

            // Add instruction row
            worksheet.Cell(4, 1).Value = "NOTE:";
            worksheet.Cell(4, 1).Style.Font.Bold = true;
            worksheet.Cell(4, 1).Style.Font.FontColor = XLColor.Red;
            
            worksheet.Cell(4, 2).Value = $"Faculty ID is AUTO-GENERATED. Example for {dept}: {deptCode}01, {deptCode}02, {deptCode}03... You can leave FacultyId column empty.";
            worksheet.Range(4, 2, 4, 5).Merge();
            worksheet.Cell(4, 2).Style.Font.FontColor = XLColor.Red;
            worksheet.Cell(4, 2).Style.Font.Bold = true;

            worksheet.Columns().AdjustToContents();
            worksheet.Column(1).Width = 18;
            worksheet.Column(2).Width = 25;
            worksheet.Column(3).Width = 35;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Faculty_Upload_Template_{dept}.xlsx");
        }
    }
}
