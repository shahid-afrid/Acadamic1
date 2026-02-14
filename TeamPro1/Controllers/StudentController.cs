using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamPro1.Models;

namespace TeamPro1.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Student/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: Student/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Student/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Find student by email
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Email == model.Email);

                // Verify password using BCrypt
                if (student == null || !BCrypt.Net.BCrypt.Verify(model.Password, student.Password))
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                // Set session variables for authenticated user
                HttpContext.Session.SetString("StudentId", student.Id.ToString());
                HttpContext.Session.SetString("StudentName", student.FullName);
                HttpContext.Session.SetString("StudentEmail", student.Email);
                HttpContext.Session.SetString("StudentRegdNumber", student.RegdNumber);
                HttpContext.Session.SetString("StudentYear", student.Year.ToString());
                HttpContext.Session.SetString("StudentDepartment", student.Department);

                TempData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("MainDashboard");
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        // GET: Student/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Student/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(StudentRegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingStudent = await _context.Students
                    .FirstOrDefaultAsync(s => s.Email == model.Email);

                if (existingStudent != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered");
                    return View(model);
                }

                // Check if registration number already exists
                var existingRegd = await _context.Students
                    .FirstOrDefaultAsync(s => s.RegdNumber == model.RegdNumber);

                if (existingRegd != null)
                {
                    ModelState.AddModelError("RegdNumber", "This registration number is already registered");
                    return View(model);
                }

                // Parse Year and Semester strings to integers
                int year = model.Year.Contains("II") ? 2 : model.Year.Contains("III") ? 3 : 4;
                int semester = model.Semester.Contains("I") ? 1 : 2;

                // Hash password using BCrypt before storing
                var student = new Student
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password), // ? BCrypt Hash
                    RegdNumber = model.RegdNumber,
                    Year = year,
                    Semester = semester,
                    Department = model.Department
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Student/MainDashboard
        public async Task<IActionResult> MainDashboard()
        {
            // Check if student is logged in
            var studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                TempData["ErrorMessage"] = "Please login to access the dashboard.";
                return RedirectToAction("Login");
            }

            // Set ViewBag data from session
            ViewBag.StudentName = HttpContext.Session.GetString("StudentName");
            ViewBag.StudentRegdNumber = HttpContext.Session.GetString("StudentRegdNumber");
            ViewBag.StudentYear = HttpContext.Session.GetString("StudentYear");
            ViewBag.StudentDepartment = HttpContext.Session.GetString("StudentDepartment");

            // Check team formation schedule for this student's department and year
            var dept = HttpContext.Session.GetString("StudentDepartment");
            var yearStr = HttpContext.Session.GetString("StudentYear");
            int.TryParse(yearStr, out int year);

            var schedule = await _context.TeamFormationSchedules
                .FirstOrDefaultAsync(s => s.Department == dept && s.Year == year);

            if (schedule != null && schedule.IsOpen)
            {
                ViewBag.IsTeamFormationOpen = true;
                ViewBag.TeamFormationStatus = "Team member selection is currently available for your year.";
            }
            else
            {
                ViewBag.IsTeamFormationOpen = false;
                ViewBag.TeamFormationStatus = "Team member selection is currently closed for your year. Please wait for your admin to open it.";
            }

            return View();
        }

        // GET: Student/Dashboard (Profile view)
        public async Task<IActionResult> Dashboard()
        {
            // Check if student is logged in
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                TempData["ErrorMessage"] = "Please login to access your profile.";
                return RedirectToAction("Login");
            }

            try
            {
                // Parse student ID and fetch student from database
                if (!int.TryParse(studentIdString, out int studentId))
                {
                    TempData["ErrorMessage"] = "Invalid session data. Please login again.";
                    return RedirectToAction("Login");
                }

                var student = await _context.Students.FindAsync(studentId);
                
                if (student == null)
                {
                    TempData["ErrorMessage"] = "Student not found. Please login again.";
                    return RedirectToAction("Login");
                }

                return View(student);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                TempData["ErrorMessage"] = "An error occurred while loading your profile.";
                return RedirectToAction("MainDashboard");
            }
        }

        // GET: Student/SelectSubject
        public IActionResult SelectSubject()
        {
            // Check if student is logged in
            var studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                TempData["ErrorMessage"] = "Please login to access subject selection.";
                return RedirectToAction("Login");
            }

            // TODO: Implement subject selection logic
            TempData["InfoMessage"] = "Subject selection feature coming soon!";
            return RedirectToAction("MainDashboard");
        }

        // GET: Student/MySelectedSubjects
        public IActionResult MySelectedSubjects()
        {
            // Check if student is logged in
            var studentId = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentId))
            {
                TempData["ErrorMessage"] = "Please login to view your selected subjects.";
                return RedirectToAction("Login");
            }

            // TODO: Implement view selected subjects logic
            TempData["InfoMessage"] = "View selected subjects feature coming soon!";
            return RedirectToAction("MainDashboard");
        }

        // GET: Student/ChangePassword
        public async Task<IActionResult> ChangePassword()
        {
            // Check if student is logged in
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                TempData["ErrorMessage"] = "Please login to change your password.";
                return RedirectToAction("Login");
            }

            try
            {
                // Parse student ID and fetch student from database
                if (!int.TryParse(studentIdString, out int studentId))
                {
                    TempData["ErrorMessage"] = "Invalid session data. Please login again.";
                    return RedirectToAction("Login");
                }

                var student = await _context.Students.FindAsync(studentId);
                
                if (student == null)
                {
                    TempData["ErrorMessage"] = "Student not found. Please login again.";
                    return RedirectToAction("Login");
                }

                // Populate the model with student info
                var model = new ChangePasswordViewModel
                {
                    StudentId = studentId.ToString(),
                    StudentName = student.FullName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
                return RedirectToAction("Dashboard");
            }
        }

        // POST: Student/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                if (!int.TryParse(studentIdString, out int studentId))
                {
                    return RedirectToAction("Login");
                }

                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                {
                    return RedirectToAction("Login");
                }

                // Verify current password using BCrypt
                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, student.Password))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                    return View(model);
                }

                // Hash new password using BCrypt
                student.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Dashboard");
            }

            return View(model);
        }

        // GET: Student/PoolOfStudents
        public async Task<IActionResult> PoolOfStudents()
        {
            // Check if student is logged in
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                TempData["ErrorMessage"] = "Please login to view pool of students.";
                return RedirectToAction("Login");
            }

            try
            {
                // Parse student ID
                if (!int.TryParse(studentIdString, out int currentStudentId))
                {
                    TempData["ErrorMessage"] = "Invalid session data. Please login again.";
                    return RedirectToAction("Login");
                }

                // Get current student details
                var currentStudent = await _context.Students.FindAsync(currentStudentId);
                if (currentStudent == null)
                {
                    TempData["ErrorMessage"] = "Student not found. Please login again.";
                    return RedirectToAction("Login");
                }

                // Check if team formation is open for this student's department and year
                var schedule = await _context.TeamFormationSchedules
                    .FirstOrDefaultAsync(s => s.Department == currentStudent.Department && s.Year == currentStudent.Year);

                if (schedule == null || !schedule.IsOpen)
                {
                    TempData["ErrorMessage"] = "Team member selection is currently closed for your year. Please wait for your admin to open it.";
                    return RedirectToAction("MainDashboard");
                }

                // Get all students from same year and semester (excluding current student)
                var availableStudents = await _context.Students
                    .Where(s => s.Year == currentStudent.Year && 
                                s.Semester == currentStudent.Semester && 
                                s.Id != currentStudentId)
                    .OrderBy(s => s.RegdNumber)
                    .ToListAsync();

                // Get all team requests where current student is sender
                var sentRequests = await _context.TeamRequests
                    .Where(tr => tr.SenderId == currentStudentId && tr.Status == "Pending")
                    .ToListAsync();

                // Get all team requests where current student is receiver (INCOMING REQUESTS)
                var receivedRequests = await _context.TeamRequests
                    .Include(tr => tr.Sender)
                    .Where(tr => tr.ReceiverId == currentStudentId && tr.Status == "Pending")
                    .ToListAsync();

                // Get all teams to check if students are already in a team
                var existingTeams = await _context.Teams.ToListAsync();

                // Check if current student is already in a team
                var currentStudentInTeam = existingTeams.Any(t => 
                    t.Student1Id == currentStudentId || t.Student2Id == currentStudentId);

                // Create view model with student info and their status
                var studentPool = availableStudents.Select(s => new
                {
                    Student = s,
                    HasPendingRequest = sentRequests.Any(sr => sr.ReceiverId == s.Id),
                    IsInTeam = existingTeams.Any(t => t.Student1Id == s.Id || t.Student2Id == s.Id)
                }).ToList();

                ViewBag.CurrentStudentId = currentStudentId;
                ViewBag.CurrentStudentName = currentStudent.FullName;
                ViewBag.CurrentStudentRegdNumber = currentStudent.RegdNumber;
                ViewBag.CurrentStudentInTeam = currentStudentInTeam;
                ViewBag.HasPendingRequest = sentRequests.Any();
                ViewBag.ReceivedRequests = receivedRequests; // NEW: Pass received requests
                ViewBag.ReceivedRequestsCount = receivedRequests.Count; // NEW: Count for badge
                ViewBag.StudentPool = studentPool;

                return View();
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = "An error occurred while loading pool of students.";
                return RedirectToAction("MainDashboard");
            }
        }

        // POST: Student/SendTeamRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTeamRequest(int receiverId)
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                if (!int.TryParse(studentIdString, out int senderId))
                {
                    return Json(new { success = false, message = "Invalid session." });
                }

                // Check if sender already has a pending request
                var existingRequest = await _context.TeamRequests
                    .FirstOrDefaultAsync(tr => tr.SenderId == senderId && tr.Status == "Pending");

                if (existingRequest != null)
                {
                    return Json(new { success = false, message = "You already have a pending request." });
                }

                // Check if either student is already in a team
                var senderInTeam = await _context.Teams
                    .AnyAsync(t => t.Student1Id == senderId || t.Student2Id == senderId);
                var receiverInTeam = await _context.Teams
                    .AnyAsync(t => t.Student1Id == receiverId || t.Student2Id == receiverId);

                if (senderInTeam)
                {
                    return Json(new { success = false, message = "You are already in a team." });
                }

                if (receiverInTeam)
                {
                    return Json(new { success = false, message = "This student is already in a team." });
                }

                // Create new team request
                var teamRequest = new TeamRequest
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.TeamRequests.Add(teamRequest);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Team request sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while sending request." });
            }
        }

        // POST: Student/CancelTeamRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTeamRequest(int receiverId)
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                if (!int.TryParse(studentIdString, out int senderId))
                {
                    return Json(new { success = false, message = "Invalid session." });
                }

                // Find the pending request
                var request = await _context.TeamRequests
                    .FirstOrDefaultAsync(tr => tr.SenderId == senderId && 
                                              tr.ReceiverId == receiverId && 
                                              tr.Status == "Pending");

                if (request == null)
                {
                    return Json(new { success = false, message = "Request not found." });
                }

                // Remove the request
                _context.TeamRequests.Remove(request);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Request cancelled successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while canceling request." });
            }
        }

        // POST: Student/AcceptRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                if (!int.TryParse(studentIdString, out int currentStudentId))
                {
                    return Json(new { success = false, message = "Invalid session." });
                }

                // Find the request
                var request = await _context.TeamRequests
                    .FirstOrDefaultAsync(tr => tr.Id == requestId && 
                                              tr.ReceiverId == currentStudentId && 
                                              tr.Status == "Pending");

                if (request == null)
                {
                    return Json(new { success = false, message = "Request not found or already processed." });
                }

                // Check if either student is already in a team
                var senderInTeam = await _context.Teams
                    .AnyAsync(t => t.Student1Id == request.SenderId || t.Student2Id == request.SenderId);
                var receiverInTeam = await _context.Teams
                    .AnyAsync(t => t.Student1Id == currentStudentId || t.Student2Id == currentStudentId);

                if (senderInTeam)
                {
                    // Remove the request since sender is already in a team
                    _context.TeamRequests.Remove(request);
                    await _context.SaveChangesAsync();
                    return Json(new { success = false, message = "The sender is already in a team." });
                }

                if (receiverInTeam)
                {
                    return Json(new { success = false, message = "You are already in a team." });
                }

                // Get the next team number
                var maxTeamNumber = await _context.Teams.AnyAsync() 
                    ? await _context.Teams.MaxAsync(t => t.TeamNumber) 
                    : 0;
                var newTeamNumber = maxTeamNumber + 1;

                // Create the team
                var team = new Team
                {
                    TeamNumber = newTeamNumber,
                    Student1Id = request.SenderId,
                    Student2Id = currentStudentId,
                    CreatedAt = DateTime.Now
                };

                _context.Teams.Add(team);

                // Update request status
                request.Status = "Accepted";
                request.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Request accepted! Team {newTeamNumber} created successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while accepting request." });
            }
        }

        // POST: Student/RejectRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                if (!int.TryParse(studentIdString, out int currentStudentId))
                {
                    return Json(new { success = false, message = "Invalid session." });
                }

                // Find the request
                var request = await _context.TeamRequests
                    .FirstOrDefaultAsync(tr => tr.Id == requestId && 
                                              tr.ReceiverId == currentStudentId && 
                                              tr.Status == "Pending");

                if (request == null)
                {
                    return Json(new { success = false, message = "Request not found or already processed." });
                }

                // Update request status
                request.Status = "Rejected";
                request.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Request rejected successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while rejecting request." });
            }
        }

        // GET: Student/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logged out successfully!";
            return RedirectToAction("Login");
        }

        // Utility method to fix invalid semester data (Admin/Debug use only)
        // Access via: /Student/FixSemesterData
        public async Task<IActionResult> FixSemesterData()
        {
            try
            {
                // Get all students with invalid semester data (not 1 or 2)
                var studentsToFix = await _context.Students
                    .Where(s => s.Semester < 1 || s.Semester > 2)
                    .ToListAsync();

                if (studentsToFix.Count == 0)
                {
                    return Content("No students with invalid semester data found. All semesters are either 1 or 2.");
                }

                // Fix each student's semester
                foreach (var student in studentsToFix)
                {
                    // Convert semester 3,4,5,6,7,8 to 1 or 2
                    // Odd semesters (3,5,7) -> 1, Even semesters (4,6,8) -> 2
                    student.Semester = (student.Semester % 2 == 0) ? 2 : 1;
                }

                await _context.SaveChangesAsync();

                return Content($"Successfully fixed {studentsToFix.Count} student(s) with invalid semester data. All semesters now converted to 1 or 2.");
            }
            catch (Exception ex)
            {
                return Content($"Error fixing semester data: {ex.Message}");
            }
        }

        // GET: Student/StatusUpdate (Project Status)
        public async Task<IActionResult> StatusUpdate()
        {
            // Check if student is logged in
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                TempData["ErrorMessage"] = "Please login to view project status.";
                return RedirectToAction("Login");
            }

            try
            {
                // Parse student ID
                if (!int.TryParse(studentIdString, out int currentStudentId))
                {
                    TempData["ErrorMessage"] = "Invalid session data. Please login again.";
                    return RedirectToAction("Login");
                }

                // Get current student details
                var currentStudent = await _context.Students.FindAsync(currentStudentId);
                if (currentStudent == null)
                {
                    TempData["ErrorMessage"] = "Student not found. Please login again.";
                    return RedirectToAction("Login");
                }

                // Find the team that the current student is part of
                var team = await _context.Teams
                    .Include(t => t.Student1)
                    .Include(t => t.Student2)
                    .FirstOrDefaultAsync(t => t.Student1Id == currentStudentId || t.Student2Id == currentStudentId);

                if (team == null)
                {
                    ViewBag.HasTeam = false;
                    ViewBag.Message = "You are not part of any team yet. Please form a team first.";
                    ViewBag.CurrentStudentName = currentStudent.FullName;
                    ViewBag.CurrentStudentRegdNumber = currentStudent.RegdNumber;
                    return View();
                }

                // Get project progress for this team
                var projectProgress = await _context.ProjectProgresses
                    .Include(pp => pp.AssignedFaculty)
                    .FirstOrDefaultAsync(pp => pp.TeamId == team.Id);

                // If no project progress exists, create a default one
                if (projectProgress == null)
                {
                    projectProgress = new ProjectProgress
                    {
                        TeamId = team.Id,
                        CompletionPercentage = 0,
                        Status = "Not Started",
                        CreatedAt = DateTime.Now
                    };
                    _context.ProjectProgresses.Add(projectProgress);
                    await _context.SaveChangesAsync();
                }

                // Get all meetings for this team, ordered by meeting number
                // Handle case where TeamMeetings table might not exist yet
                List<TeamMeeting> teamMeetings = new List<TeamMeeting>();
                try
                {
                    teamMeetings = await _context.TeamMeetings
                        .Where(tm => tm.TeamId == team.Id)
                        .OrderBy(tm => tm.MeetingNumber)
                        .ToListAsync();
                }
                catch (Exception dbEx)
                {
                    // TeamMeetings table doesn't exist yet
                    if (dbEx.Message.Contains("Invalid object name 'TeamMeetings'") || 
                        dbEx.InnerException?.Message.Contains("Invalid object name 'TeamMeetings'") == true)
                    {
                        TempData["ErrorMessage"] = @"TeamMeetings table not found in database. 
                            Please run the SQL script: Scripts\AddTeamMeetingTable.sql to create the table first. 
                            Instructions: Open SQL Server Management Studio, connect to your database, and run the script.";
                        teamMeetings = new List<TeamMeeting>();
                    }
                    else
                    {
                        // Re-throw other database errors
                        throw;
                    }
                }

                ViewBag.HasTeam = true;
                ViewBag.CurrentStudentName = currentStudent.FullName;
                ViewBag.CurrentStudentRegdNumber = currentStudent.RegdNumber;
                ViewBag.Team = team;
                ViewBag.ProjectProgress = projectProgress;
                ViewBag.TeamMeetings = teamMeetings;

                return View();
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = $"An error occurred while loading project status: {ex.Message}";
                return RedirectToAction("MainDashboard");
            }
        }

        // POST: Student/AddMeeting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMeeting(int TeamId, int MeetingNumber, DateTime? MeetingDate, string? Notes, int? CompletionPercentage, IFormFile? ProofFile)
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                TempData["ErrorMessage"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            try
            {
                // Detailed validation with specific error messages
                if (TeamId <= 0)
                {
                    TempData["ErrorMessage"] = "Invalid Team ID. Please try again.";
                    return RedirectToAction("StatusUpdate");
                }

                if (MeetingNumber <= 0)
                {
                    TempData["ErrorMessage"] = "Meeting number must be greater than 0.";
                    return RedirectToAction("StatusUpdate");
                }

                if (!MeetingDate.HasValue)
                {
                    TempData["ErrorMessage"] = "Meeting date is required. Please select a date.";
                    return RedirectToAction("StatusUpdate");
                }

                if (MeetingDate.Value > DateTime.Now.AddDays(1))
                {
                    TempData["WarningMessage"] = "Meeting date is in the future. Are you sure this is correct?";
                    // Don't return - allow it but warn
                }

                if (!CompletionPercentage.HasValue)
                {
                    CompletionPercentage = 0;
                }

                if (CompletionPercentage < 0 || CompletionPercentage > 100)
                {
                    TempData["ErrorMessage"] = "Completion percentage must be between 0 and 100.";
                    return RedirectToAction("StatusUpdate");
                }

                // Verify student is part of this team
                if (!int.TryParse(studentIdString, out int currentStudentId))
                {
                    TempData["ErrorMessage"] = "Invalid session data. Please login again.";
                    return RedirectToAction("Login");
                }

                var team = await _context.Teams
                    .FirstOrDefaultAsync(t => t.Id == TeamId && 
                                            (t.Student1Id == currentStudentId || t.Student2Id == currentStudentId));

                if (team == null)
                {
                    TempData["ErrorMessage"] = "You are not authorized to add meetings for this team.";
                    return RedirectToAction("StatusUpdate");
                }

                // CRITICAL: Check if problem statement and mentor are assigned BEFORE allowing meeting creation
                var projectProgress = await _context.ProjectProgresses
                    .Include(pp => pp.AssignedFaculty)
                    .FirstOrDefaultAsync(pp => pp.TeamId == TeamId);

                if (projectProgress == null)
                {
                    TempData["ErrorMessage"] = "Cannot add meeting. Project progress record not found.";
                    return RedirectToAction("StatusUpdate");
                }

                if (string.IsNullOrEmpty(projectProgress.ProblemStatement))
                {
                    TempData["ErrorMessage"] = "Cannot add meeting. Problem statement must be assigned by faculty first.";
                    return RedirectToAction("StatusUpdate");
                }

                if (projectProgress.AssignedFacultyId == null)
                {
                    TempData["ErrorMessage"] = "Cannot add meeting. Mentor must be assigned by faculty first.";
                    return RedirectToAction("StatusUpdate");
                }

                // Check if meeting number already exists for this team
                var existingMeeting = await _context.TeamMeetings
                    .FirstOrDefaultAsync(tm => tm.TeamId == TeamId && tm.MeetingNumber == MeetingNumber);

                if (existingMeeting != null)
                {
                    TempData["ErrorMessage"] = $"Meeting #{MeetingNumber} already exists for your team. Please use a different meeting number.";
                    return RedirectToAction("StatusUpdate");
                }

                // Enforce auto-incremented meeting number
                var existingMeetings = await _context.TeamMeetings
                    .Where(tm => tm.TeamId == TeamId)
                    .OrderBy(tm => tm.MeetingNumber)
                    .ToListAsync();

                var expectedMeetingNumber = existingMeetings.Count > 0
                    ? existingMeetings.Max(m => m.MeetingNumber) + 1
                    : 1;

                if (MeetingNumber != expectedMeetingNumber)
                {
                    MeetingNumber = expectedMeetingNumber;
                }

                // Enforce that completion percentage cannot go backward
                if (existingMeetings.Count > 0)
                {
                    var lastCompletion = existingMeetings.Last().CompletionPercentage;
                    if (CompletionPercentage < lastCompletion)
                    {
                        TempData["ErrorMessage"] = $"Completion percentage cannot be less than {lastCompletion}% (previous meeting progress).";
                        return RedirectToAction("StatusUpdate");
                    }
                }

                // Handle proof file upload - store in database
                byte[]? proofImageData = null;
                string? proofContentType = null;
                if (ProofFile != null && ProofFile.Length > 0)
                {
                    // Validate file size (max 5MB)
                    if (ProofFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Proof file size must be less than 5MB.";
                        return RedirectToAction("StatusUpdate");
                    }

                    // Validate file type
                    var extension = Path.GetExtension(ProofFile.FileName).ToLower();
                    if (extension != ".jpg" && extension != ".jpeg")
                    {
                        TempData["ErrorMessage"] = "Only JPG/JPEG files are allowed for proof upload. Please select a valid image file.";
                        return RedirectToAction("StatusUpdate");
                    }

                    try
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await ProofFile.CopyToAsync(memoryStream);
                            proofImageData = memoryStream.ToArray();
                        }
                        proofContentType = ProofFile.ContentType;
                    }
                    catch (Exception fileEx)
                    {
                        TempData["ErrorMessage"] = $"Failed to process proof file: {fileEx.Message}";
                        return RedirectToAction("StatusUpdate");
                    }
                }

                // Create new meeting
                var meeting = new TeamMeeting
                {
                    TeamId = TeamId,
                    MeetingNumber = MeetingNumber,
                    MeetingDate = MeetingDate.Value,
                    Notes = Notes,
                    CompletionPercentage = CompletionPercentage.Value,
                    ProofImageData = proofImageData,
                    ProofContentType = proofContentType,
                    Status = "Completed",
                    CreatedAt = DateTime.Now
                };

                _context.TeamMeetings.Add(meeting);
                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = TeamId,
                    Action = $"Added Meeting #{MeetingNumber}",
                    Details = $"Date: {MeetingDate.Value:MMM dd, yyyy}, Completion: {CompletionPercentage}%{(ProofFile != null ? ", Proof uploaded" : "")}",
                    PerformedByRole = "Student",
                    PerformedByName = HttpContext.Session.GetString("StudentName") ?? "Unknown",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Meeting #{MeetingNumber} added successfully! Completion: {CompletionPercentage}%";
                return RedirectToAction("StatusUpdate");
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = $"Database error while adding meeting: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return RedirectToAction("StatusUpdate");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}. Please try again or contact support.";
                return RedirectToAction("StatusUpdate");
            }
        }

        // POST: Student/UpdateMeeting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMeeting(int MeetingId, int TeamId, int MeetingNumber, DateTime? MeetingDate, string? Notes, int? CompletionPercentage, IFormFile? ProofFile)
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                TempData["ErrorMessage"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            try
            {
                // Detailed validation
                if (MeetingId <= 0)
                {
                    TempData["ErrorMessage"] = "Invalid Meeting ID.";
                    return RedirectToAction("StatusUpdate");
                }

                if (!MeetingDate.HasValue)
                {
                    TempData["ErrorMessage"] = "Meeting date is required.";
                    return RedirectToAction("StatusUpdate");
                }

                if (!CompletionPercentage.HasValue)
                {
                    TempData["ErrorMessage"] = "Completion percentage is required.";
                    return RedirectToAction("StatusUpdate");
                }

                if (CompletionPercentage < 0 || CompletionPercentage > 100)
                {
                    TempData["ErrorMessage"] = "Completion percentage must be between 0 and 100.";
                    return RedirectToAction("StatusUpdate");
                }

                // Verify student is part of this team
                if (!int.TryParse(studentIdString, out int currentStudentId))
                {
                    TempData["ErrorMessage"] = "Invalid session data. Please login again.";
                    return RedirectToAction("Login");
                }

                var team = await _context.Teams
                    .FirstOrDefaultAsync(t => t.Id == TeamId && 
                                            (t.Student1Id == currentStudentId || t.Student2Id == currentStudentId));

                if (team == null)
                {
                    TempData["ErrorMessage"] = "You are not authorized to update meetings for this team.";
                    return RedirectToAction("StatusUpdate");
                }

                // Find the meeting to update
                var meeting = await _context.TeamMeetings
                    .FirstOrDefaultAsync(tm => tm.Id == MeetingId && tm.TeamId == TeamId);

                if (meeting == null)
                {
                    TempData["ErrorMessage"] = "Meeting not found.";
                    return RedirectToAction("StatusUpdate");
                }

                // Check if this is the latest meeting
                var latestMeetingNumber = await _context.TeamMeetings
                    .Where(tm => tm.TeamId == TeamId)
                    .MaxAsync(tm => tm.MeetingNumber);

                var isLatestMeeting = meeting.MeetingNumber == latestMeetingNumber;

                // If not the latest meeting, ignore any completion percentage change
                if (!isLatestMeeting)
                {
                    CompletionPercentage = meeting.CompletionPercentage;
                }

                // Verify problem statement and mentor are assigned
                var projectProgress = await _context.ProjectProgresses
                    .Include(pp => pp.AssignedFaculty)
                    .FirstOrDefaultAsync(pp => pp.TeamId == TeamId);

                if (projectProgress == null || string.IsNullOrEmpty(projectProgress.ProblemStatement) || projectProgress.AssignedFacultyId == null)
                {
                    TempData["ErrorMessage"] = "Cannot edit meeting until problem statement and mentor are assigned by faculty.";
                    return RedirectToAction("StatusUpdate");
                }

                // Handle proof file upload - store in database
                byte[]? proofImageData = meeting.ProofImageData; // Keep existing proof by default
                string? proofContentType = meeting.ProofContentType;
                if (ProofFile != null && ProofFile.Length > 0)
                {
                    // Validate file size
                    if (ProofFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Proof file size must be less than 5MB.";
                        return RedirectToAction("StatusUpdate");
                    }

                    // Validate file type
                    var extension = Path.GetExtension(ProofFile.FileName).ToLower();
                    if (extension != ".jpg" && extension != ".jpeg")
                    {
                        TempData["ErrorMessage"] = "Only JPG/JPEG files are allowed for proof upload.";
                        return RedirectToAction("StatusUpdate");
                    }

                    try
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await ProofFile.CopyToAsync(memoryStream);
                            proofImageData = memoryStream.ToArray();
                        }
                        proofContentType = ProofFile.ContentType;
                    }
                    catch (Exception fileEx)
                    {
                        TempData["ErrorMessage"] = $"Failed to process proof file: {fileEx.Message}";
                        return RedirectToAction("StatusUpdate");
                    }
                }

                // Update meeting details
                meeting.MeetingDate = MeetingDate.Value;
                meeting.Notes = Notes;
                meeting.CompletionPercentage = CompletionPercentage.Value;
                meeting.ProofImageData = proofImageData;
                meeting.ProofContentType = proofContentType;
                meeting.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                // Log activity
                _context.TeamActivityLogs.Add(new TeamActivityLog
                {
                    TeamId = TeamId,
                    Action = $"Updated Meeting #{MeetingNumber}",
                    Details = $"Date: {MeetingDate.Value:MMM dd, yyyy}, Completion: {CompletionPercentage}%{(ProofFile != null ? ", New proof uploaded" : "")}",
                    PerformedByRole = "Student",
                    PerformedByName = HttpContext.Session.GetString("StudentName") ?? "Unknown",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Meeting #{MeetingNumber} updated successfully! Completion: {CompletionPercentage}%";
                return RedirectToAction("StatusUpdate");
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = $"Database error while updating meeting: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return RedirectToAction("StatusUpdate");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}. Please try again or contact support.";
                return RedirectToAction("StatusUpdate");
            }
        }

        // GET: Student/GetProofImage/{id}
        public async Task<IActionResult> GetProofImage(int id)
        {
            var meeting = await _context.TeamMeetings
                .FirstOrDefaultAsync(tm => tm.Id == id);

            if (meeting == null)
            {
                return NotFound();
            }

            // Serve from database if available
            if (meeting.ProofImageData != null && meeting.ProofImageData.Length > 0)
            {
                return File(meeting.ProofImageData, meeting.ProofContentType ?? "image/jpeg");
            }

            // Fallback to legacy file path if ProofUploads has a value
            if (!string.IsNullOrEmpty(meeting.ProofUploads))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", meeting.ProofUploads.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    return File(fileBytes, "image/jpeg");
                }
            }

            return NotFound();
        }

        // GET: Student/GetTeamLogs/{teamId}
        public async Task<IActionResult> GetTeamLogs(int teamId)
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                return Json(new { success = false, message = "Please login first." });
            }

            if (!int.TryParse(studentIdString, out int currentStudentId))
            {
                return Json(new { success = false, message = "Invalid session." });
            }

            // Verify student is part of this team
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == teamId &&
                    (t.Student1Id == currentStudentId || t.Student2Id == currentStudentId));

            if (team == null)
            {
                return Json(new { success = false, message = "Unauthorized." });
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

        // POST: Student/BeIndividual
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BeIndividual()
        {
            var studentIdString = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdString))
            {
                return Json(new { success = false, message = "Please login first." });
            }

            try
            {
                if (!int.TryParse(studentIdString, out int currentStudentId))
                {
                    return Json(new { success = false, message = "Invalid session." });
                }

                // Check if student is already in a team
                var alreadyInTeam = await _context.Teams
                    .AnyAsync(t => t.Student1Id == currentStudentId || t.Student2Id == currentStudentId);

                if (alreadyInTeam)
                {
                    return Json(new { success = false, message = "You are already in a team." });
                }

                // Cancel any pending sent requests
                var pendingSentRequests = await _context.TeamRequests
                    .Where(tr => tr.SenderId == currentStudentId && tr.Status == "Pending")
                    .ToListAsync();

                if (pendingSentRequests.Any())
                {
                    _context.TeamRequests.RemoveRange(pendingSentRequests);
                }

                // Reject any pending received requests
                var pendingReceivedRequests = await _context.TeamRequests
                    .Where(tr => tr.ReceiverId == currentStudentId && tr.Status == "Pending")
                    .ToListAsync();

                foreach (var req in pendingReceivedRequests)
                {
                    req.Status = "Rejected";
                    req.UpdatedAt = DateTime.Now;
                }

                // Get the next team number
                var maxTeamNumber = await _context.Teams.AnyAsync()
                    ? await _context.Teams.MaxAsync(t => t.TeamNumber)
                    : 0;
                var newTeamNumber = maxTeamNumber + 1;

                // Create an individual team
                var team = new Team
                {
                    TeamNumber = newTeamNumber,
                    Student1Id = currentStudentId,
                    Student2Id = null,
                    IsIndividual = true,
                    CreatedAt = DateTime.Now
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"You are now registered as an individual team (Team {newTeamNumber})!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }
    }
}
