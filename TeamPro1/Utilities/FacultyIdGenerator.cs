using System.Collections.Generic;

namespace TeamPro1.Utilities
{
    /// <summary>
    /// Helper class to generate department-based Faculty IDs
    /// Format: DepartmentCode + Sequential Number
    /// Example: CSE(DS) ? Admin code: 32 ? Faculty IDs: 3201, 3202, 3203...
    /// </summary>
    public static class FacultyIdGenerator
    {
        // Department code mapping (matches Admin IDs)
        private static readonly Dictionary<string, string> DepartmentCodes = new()
        {
            { "CSE(DS)", "32" },
            { "CSE(AI&ML)", "33" },
            { "Computer Science", "31" },
            { "ECE", "41" },
            { "EEE", "42" },
            { "Mechanical", "51" },
            { "Civil", "61" },
            { "IT", "34" },
            { "CSE(AIML)", "33" }, // Alternative name
            { "CSE", "31" } // Alternative name
        };

        /// <summary>
        /// Generate next Faculty ID for a department
        /// </summary>
        /// <param name="department">Department name</param>
        /// <param name="existingFacultyIds">List of existing faculty IDs in the department</param>
        /// <returns>Next faculty ID (e.g., "3201")</returns>
        public static string GenerateNextFacultyId(string department, List<string> existingFacultyIds)
        {
            // Get department code
            if (!DepartmentCodes.TryGetValue(department, out string? deptCode))
            {
                // Default to "99" for unknown departments
                deptCode = "99";
            }

            // Find the highest sequence number
            int maxSequence = 0;

            foreach (var existingId in existingFacultyIds)
            {
                if (existingId.StartsWith(deptCode) && existingId.Length >= deptCode.Length + 1)
                {
                    // Extract sequence number (last digits after dept code)
                    string sequenceStr = existingId.Substring(deptCode.Length);
                    if (int.TryParse(sequenceStr, out int sequence))
                    {
                        maxSequence = Math.Max(maxSequence, sequence);
                    }
                }
            }

            // Generate next ID
            int nextSequence = maxSequence + 1;
            string nextFacultyId = $"{deptCode}{nextSequence:D2}"; // Pad with 2 digits: 01, 02, 03...

            return nextFacultyId;
        }

        /// <summary>
        /// Get department code for a department name
        /// </summary>
        public static string GetDepartmentCode(string department)
        {
            return DepartmentCodes.TryGetValue(department, out string? code) ? code : "99";
        }

        /// <summary>
        /// Validate if a Faculty ID matches the department format
        /// </summary>
        public static bool IsValidFacultyId(string facultyId, string department)
        {
            if (string.IsNullOrEmpty(facultyId)) return false;

            string deptCode = GetDepartmentCode(department);
            return facultyId.StartsWith(deptCode) && facultyId.Length >= deptCode.Length + 1;
        }
    }
}
