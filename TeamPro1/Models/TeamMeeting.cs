using System.ComponentModel.DataAnnotations;

namespace TeamPro1.Models
{
    public class TeamMeeting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TeamId { get; set; }

        [Required]
        [Range(1, 100)]
        public int MeetingNumber { get; set; }

        [Required]
        public DateTime MeetingDate { get; set; } = DateTime.Now;

        // Project Completion Percentage after this meeting (0-100)
        [Range(0, 100)]
        public int CompletionPercentage { get; set; } = 0;

        // Legacy: file path (kept for backward compatibility with existing data)
        public string? ProofUploads { get; set; }

        // Proof image stored directly in the database
        public byte[]? ProofImageData { get; set; }

        // MIME type of the proof image (e.g., "image/jpeg")
        [StringLength(100)]
        public string? ProofContentType { get; set; }

        // Faculty Review/Feedback for this meeting
        public string? FacultyReview { get; set; }

        // Meeting Status: Scheduled, Completed, Cancelled
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        // Meeting notes/agenda
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastUpdated { get; set; }

        // Navigation property
        public virtual Team? Team { get; set; }
    }
}
