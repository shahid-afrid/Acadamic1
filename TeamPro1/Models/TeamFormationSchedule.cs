using System.ComponentModel.DataAnnotations;

namespace TeamPro1.Models
{
    public class TeamFormationSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        public bool IsOpen { get; set; } = false;

        public DateTime? OpenedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastUpdated { get; set; }
    }
}
