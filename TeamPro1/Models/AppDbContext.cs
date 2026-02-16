using Microsoft.EntityFrameworkCore;

namespace TeamPro1.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Student & Authentication
        public DbSet<Student> Students { get; set; }
        public DbSet<Faculty> Faculties { get; set; }

        // Admin
        public DbSet<Admin> Admins { get; set; }

        // Team Formation
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamRequest> TeamRequests { get; set; }

        // Project Progress Tracking
        public DbSet<ProjectProgress> ProjectProgresses { get; set; }
        public DbSet<TeamMeeting> TeamMeetings { get; set; }

        // Meeting Invitations
        public DbSet<MeetingInvitation> MeetingInvitations { get; set; }

        // Activity Logs
        public DbSet<TeamActivityLog> TeamActivityLogs { get; set; }

        // Problem Statement Bank
        public DbSet<ProblemStatementBank> ProblemStatementBanks { get; set; }

        // Team Formation Schedule
        public DbSet<TeamFormationSchedule> TeamFormationSchedules { get; set; }

        // Notifications
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Admin unique email per department
            modelBuilder.Entity<Admin>()
                .HasIndex(a => new { a.Email, a.Department })
                .IsUnique();

            // TeamFormationSchedule unique per department+year
            modelBuilder.Entity<TeamFormationSchedule>()
                .HasIndex(t => new { t.Department, t.Year })
                .IsUnique();

            // Configure Team relationships
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Student1)
                .WithMany()
                .HasForeignKey(t => t.Student1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Student2)
                .WithMany()
                .HasForeignKey(t => t.Student2Id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TeamRequest relationships
            modelBuilder.Entity<TeamRequest>()
                .HasOne(tr => tr.Sender)
                .WithMany()
                .HasForeignKey(tr => tr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeamRequest>()
                .HasOne(tr => tr.Receiver)
                .WithMany()
                .HasForeignKey(tr => tr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ProjectProgress relationships
            modelBuilder.Entity<ProjectProgress>()
                .HasOne(pp => pp.Team)
                .WithMany()
                .HasForeignKey(pp => pp.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectProgress>()
                .HasOne(pp => pp.AssignedFaculty)
                .WithMany()
                .HasForeignKey(pp => pp.AssignedFacultyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure TeamMeeting relationships
            modelBuilder.Entity<TeamMeeting>()
                .HasOne(tm => tm.Team)
                .WithMany()
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure MeetingInvitation relationships
            modelBuilder.Entity<MeetingInvitation>()
                .HasOne(mi => mi.Team)
                .WithMany()
                .HasForeignKey(mi => mi.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeetingInvitation>()
                .HasOne(mi => mi.Faculty)
                .WithMany()
                .HasForeignKey(mi => mi.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TeamActivityLog relationships
            modelBuilder.Entity<TeamActivityLog>()
                .HasOne(al => al.Team)
                .WithMany()
                .HasForeignKey(al => al.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Notification relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Student)
                .WithMany()
                .HasForeignKey(n => n.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ProblemStatementBank relationships
            modelBuilder.Entity<ProblemStatementBank>()
                .HasOne(ps => ps.AssignedToTeam)
                .WithMany()
                .HasForeignKey(ps => ps.AssignedToTeamId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProblemStatementBank>()
                .HasOne(ps => ps.AssignedByFaculty)
                .WithMany()
                .HasForeignKey(ps => ps.AssignedByFacultyId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

