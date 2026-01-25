using System.ComponentModel.DataAnnotations;

namespace TeamPro1.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(50)]
        public string RegdNumber { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int Semester { get; set; }

        [MaxLength(100)]
        public string Department { get; set; } = "Computer Science";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}