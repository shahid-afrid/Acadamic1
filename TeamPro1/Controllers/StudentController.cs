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

                if (student == null)
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View(model);
                }

                // In a real application, you should hash passwords
                // For now, we'll do a direct comparison
                if (student.Password != model.Password)
                {
                    ModelState.AddModelError("", "Invalid email or password.");
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if email already exists
                var existingStudent = await _context.Students
                    .FirstOrDefaultAsync(s => s.Email == model.Email);

                if (existingStudent != null)
                {
                    ModelState.AddModelError("Email", "A student with this email already exists.");
                    return View(model);
                }

                // Check if registration number already exists
                var existingRegdNumber = await _context.Students
                    .FirstOrDefaultAsync(s => s.RegdNumber == model.RegdNumber);

                if (existingRegdNumber != null)
                {
                    ModelState.AddModelError("RegdNumber", "A student with this registration number already exists.");
                    return View(model);
                }

                // Convert year string to number (e.g., "II Year" -> 2)
                int yearNumber = model.Year switch
                {
                    "II Year" => 2,
                    "III Year" => 3,
                    "IV Year" => 4,
                    _ => 2
                };

                // Convert semester string to number (e.g., "I Semester" -> 1)
                int semesterNumber = model.Semester switch
                {
                    "I Semester" => 1,
                    "II Semester" => 2,
                    _ => 1
                };

                // Create new student
                var student = new Student
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password, // In a real app, hash this password
                    RegdNumber = model.RegdNumber,
                    Year = yearNumber,
                    Semester = semesterNumber,
                    Department = model.Department ?? "Computer Science",
                    CreatedAt = DateTime.Now
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app, use proper logging)
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // GET: Student/MainDashboard
        public IActionResult MainDashboard()
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

            // Set schedule availability (you can implement actual logic later)
            ViewBag.IsSelectionAvailable = true; // Default to true for now
            ViewBag.ScheduleStatus = "Faculty selection is currently available for all students.";

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
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Student/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TODO: Implement change password logic
            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("MainDashboard");
        }

        // GET: Student/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logged out successfully!";
            return RedirectToAction("Login");
        }
    }
}
