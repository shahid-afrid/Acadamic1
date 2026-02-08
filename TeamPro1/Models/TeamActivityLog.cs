using System.ComponentModel.DataAnnotations;

namespace TeamPro1.Models
{
    public class TeamActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TeamId { get; set; }

        [Required]
        [StringLength(200)]
        public string Action { get; set; } = string.Empty;

        public string? Details { get; set; }

        [Required]
        [StringLength(50)]
        public string PerformedByRole { get; set; } = string.Empty; // "Student", "Faculty", "Admin"

        [Required]
        [StringLength(150)]
        public string PerformedByName { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Team? Team { get; set; }
    }
}
