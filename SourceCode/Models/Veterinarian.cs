using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinaryClinicProject.Models
{
    public class Veterinarian
    {
        // Primary key
        public int VetId { get; set; }

        // Personal information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialty { get; set; }
        public string LicenseNumber { get; set; }
        public string Phone { get; set; }

        // Helper property
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string VetInfo => $"{FullName} - {Specialty ?? "No Specialty"}";

        // ============================================================
        // Clinic information (from JOIN with VET_CLINIC)
        // ============================================================
        public int? ClinicId { get; set; }
        public string ClinicName { get; set; }
        public string ClinicLocation { get; set; }
        public bool? IsPrimaryAtClinic { get; set; }
        public DateTime? JoinDate { get; set; }

        // Helper property for clinic assignment
        public string ClinicAssignment => IsPrimaryAtClinic == true ? $"{ClinicName} (Primary)" : ClinicName ?? "Not Assigned";
    }
}