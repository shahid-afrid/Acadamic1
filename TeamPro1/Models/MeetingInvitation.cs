using System.ComponentModel.DataAnnotations;

namespace TeamPro1.Models
{
    /// <summary>
    /// Model for meeting invitations sent by faculty to students
    /// </summary>
    public class MeetingInvitation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TeamId { get; set; }

        [Required]
        public int FacultyId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime MeetingDateTime { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        // Meeting duration in minutes
        [Range(15, 480)] // 15 minutes to 8 hours
        public int DurationMinutes { get; set; } = 60;

        // Status: Pending, Accepted, Rejected, Cancelled
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        // Student responses
        public int? Student1ResponseId { get; set; }
        public string? Student1Response { get; set; } // Accepted/Rejected/Pending

        public int? Student2ResponseId { get; set; }
        public string? Student2Response { get; set; } // Accepted/Rejected/Pending

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Team? Team { get; set; }
        public virtual Faculty? Faculty { get; set; }
    }
}
