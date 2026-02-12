using Microsoft.EntityFrameworkCore;
using TeamPro1.Models;

namespace TeamPro1.Data
{
    /// <summary>
    /// Database seeder for adding test data
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// Seeds test faculty members into the database
        /// </summary>
        public static async Task SeedTestFacultyAsync(AppDbContext context)
        {
            // Check if any faculty exists
            if (await context.Faculties.AnyAsync())
            {
                Console.WriteLine("Faculty data already exists. Skipping seed.");
            }
            else
            {
                var testFaculties = new List<Faculty>
                {
                    new Faculty
                    {
                        FullName = "Dr. Test Faculty",
                        Email = "faculty@test.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("test123"), // Using BCrypt for password hashing
                        Department = "Computer Science",
                        CreatedAt = DateTime.Now
                    },
                    new Faculty
                    {
                        FullName = "Dr. John Doe",
                        Email = "john.doe@rgmcet.edu",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        Department = "CSE(DS)",
                        CreatedAt = DateTime.Now
                    },
                    new Faculty
                    {
                        FullName = "Dr. Jane Smith",
                        Email = "jane.smith@rgmcet.edu",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        Department = "CSE(AI&ML)",
                        CreatedAt = DateTime.Now
                    },
                    new Faculty
                    {
                        FullName = "Prof. Robert Brown",
                        Email = "robert.brown@rgmcet.edu",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        Department = "Computer Science",
                        CreatedAt = DateTime.Now
                    },
                    new Faculty
                    {
                        FullName = "Dr. P. Penchala Prasad",
                        Email = "prasad@rgmcet.edu",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        Department = "CSE(DS)",
                        CreatedAt = DateTime.Now
                    }
                };

                await context.Faculties.AddRangeAsync(testFaculties);
                await context.SaveChangesAsync();

                Console.WriteLine($"Successfully seeded {testFaculties.Count} test faculty members!");
                Console.WriteLine("\nTest Faculty Credentials:");
                Console.WriteLine("========================================");
                foreach (var faculty in testFaculties)
                {
                    Console.WriteLine($"Email: {faculty.Email} | Password: {faculty.Password}");
                }
                Console.WriteLine("========================================");
            }

            // Seed admins
            await SeedAdminsAsync(context);
        }

        /// <summary>
        /// Seeds department-wise admins into the Admins table
        /// </summary>
        public static async Task SeedAdminsAsync(AppDbContext context)
        {
            // Check if any admins exist
            if (await context.Admins.AnyAsync())
            {
                Console.WriteLine("Admin data already exists. Skipping seed.");
                return;
            }

            var admins = new List<Admin>
            {
                new Admin { Name = "CSE(DS) Admin",    Email = "admin.cseds@rgmcet.edu",   Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Department = "CSE(DS)",          CreatedAt = DateTime.Now },
                new Admin { Name = "CSE(AI&ML) Admin", Email = "admin.cseaiml@rgmcet.edu", Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Department = "CSE(AI&ML)",       CreatedAt = DateTime.Now },
                new Admin { Name = "CSE Admin",        Email = "admin.cse@rgmcet.edu",     Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Department = "Computer Science", CreatedAt = DateTime.Now },
                new Admin { Name = "ECE Admin",        Email = "admin.ece@rgmcet.edu",     Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Department = "ECE",              CreatedAt = DateTime.Now },
                new Admin { Name = "EEE Admin",        Email = "admin.eee@rgmcet.edu",     Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Department = "EEE",              CreatedAt = DateTime.Now },
                new Admin { Name = "Mechanical Admin", Email = "admin.mech@rgmcet.edu",    Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Department = "Mechanical",       CreatedAt = DateTime.Now },
                new Admin { Name = "Civil Admin",      Email = "admin.civil@rgmcet.edu",   Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Department = "Civil",            CreatedAt = DateTime.Now },
            };

            await context.Admins.AddRangeAsync(admins);
            await context.SaveChangesAsync();

            Console.WriteLine($"Successfully seeded {admins.Count} department admins!");
            Console.WriteLine("\nAdmin Credentials:");
            Console.WriteLine("========================================");
            foreach (var admin in admins)
            {
                Console.WriteLine($"Dept: {admin.Department,-20} | Email: {admin.Email,-30} | Password: {admin.Password}");
            }
            Console.WriteLine("========================================");
        }

        /// <summary>
        /// Seeds all test data including faculty, students, teams, etc.
        /// </summary>
        public static async Task SeedAllTestDataAsync(AppDbContext context)
        {
            // Seed faculty first
            await SeedTestFacultyAsync(context);

            // You can add more seed methods here for students, teams, etc.
            Console.WriteLine("All test data seeded successfully!");
        }
    }
}
