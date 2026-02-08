using System.ComponentModel.DataAnnotations;

namespace TeamPro1.Models
{
    public class ProblemStatementBank
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Statement { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; } = 3;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
