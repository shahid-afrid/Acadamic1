using Microsoft.EntityFrameworkCore;

namespace TeamPro1.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        // public DbSet<Faculty> Faculties { get; set; }
    }
}
