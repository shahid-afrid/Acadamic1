using System.ComponentModel.DataAnnotations;

namespace TeamPro1.Models
{
    public class StudentRegistrationModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s.]+$", ErrorMessage = "Name can only contain letters, spaces, and dots")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Registration number is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Registration number must be exactly 10 characters")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Registration number must be alphanumeric and uppercase")]
        public string RegdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        public string Year { get; set; } = string.Empty;

        [Required(ErrorMessage = "Semester is required")]
        public string Semester { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@rgmcet\.edu\.in$", ErrorMessage = "Must use college email ending with @rgmcet.edu.in")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).+$", ErrorMessage = "Password must contain at least 1 uppercase letter, 1 digit, and 1 special character")]
        public string Password { get; set; } = string.Empty;
    }
}
