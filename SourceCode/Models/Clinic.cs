using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinaryClinicProject.Models
{
    public class Clinic
    {
        // Primary key
        public int ClinicId { get; set; }

        // Clinic information
        public string ClinicName { get; set; }
        public string Location { get; set; }
        public bool HasEmergencyFacility { get; set; }
        public string Phone { get; set; }

        // Helper properties
        public string ClinicInfo => $"{ClinicName} - {Location ?? "Location not specified"}";
        public string EmergencyStatus => HasEmergencyFacility ? "Yes" : "No";

        // ============================================================
        // Veterinarian information (from JOIN with VET_CLINIC)
        // ============================================================
        public int? VetId { get; set; }
        public string VetFirstName { get; set; }
        public string VetLastName { get; set; }
        public string VetSpecialty { get; set; }
        public bool? IsPrimaryVet { get; set; }

        public string VetFullName => $"{VetFirstName} {VetLastName}".Trim();

        // ============================================================
        // Statistics (from aggregation queries)
        // ============================================================
        public int VetCount { get; set; }
        public int VisitCount { get; set; }
        public int TotalVaccinations { get; set; }

        // ============================================================
        // Helper properties for statistics
        // ============================================================
        public string StatisticsInfo => $"Vets: {VetCount}, Visits: {VisitCount}, Vaccinations: {TotalVaccinations}";
    }
}