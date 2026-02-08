using Microsoft.AspNetCore.Mvc;
using TeamPro1.Data;
using TeamPro1.Models;

namespace TeamPro1.Controllers
{
    /// <summary>
    /// Controller for database seeding operations (Development/Testing only)
    /// </summary>
    public class SeedController : Controller
    {
        private readonly AppDbContext _context;

        public SeedController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Seed/Faculty
        // Navigate to this URL to manually seed faculty data
        [HttpGet]
        public async Task<IActionResult> Faculty()
        {
            try
            {
                await DbSeeder.SeedTestFacultyAsync(_context);
                
                var faculties = _context.Faculties.OrderBy(f => f.Id).ToList();
                
                ViewBag.Message = "Faculty data seeded successfully!";
                ViewBag.Faculties = faculties;
                
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error seeding data: {ex.Message}";
                return View();
            }
        }

        // GET: /Seed/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}
