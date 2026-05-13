using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinaryClinicProject.Models
{
    public class Vaccination
    {
        // Primary key
        public int VaccinationId { get; set; }

        // Foreign keys
        public int VisitId { get; set; }
        public int InventoryId { get; set; }

        // Vaccination information
        public string VaccineType { get; set; }
        public DateTime AdministeredDate { get; set; }
        public DateTime? NextBoosterDue { get; set; }

        // ============================================================
        // Pet information (from JOIN)
        // ============================================================
        public int PetId { get; set; }
        public string PetName { get; set; }
        public string Species { get; set; }

        // ============================================================
        // Owner information (from JOIN)
        // ============================================================
        public string OwnerFirstName { get; set; }
        public string OwnerLastName { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerEmail { get; set; }

        public string OwnerFullName => $"{OwnerFirstName} {OwnerLastName}".Trim();

        // ============================================================
        // Visit information (from JOIN)
        // ============================================================
        public DateTime VisitDate { get; set; }
        public string VisitStatus { get; set; }

        // ============================================================
        // Clinic information (from JOIN)
        // ============================================================
        public string ClinicName { get; set; }
        public string ClinicLocation { get; set; }

        // ============================================================
        // Inventory information (from JOIN)
        // ============================================================
        public string BatchNumber { get; set; }
        public string SupplierName { get; set; }

        // ============================================================
        // Helper properties
        // ============================================================
        public string VaccinationInfo => $"{VaccineType} - {AdministeredDate:yyyy-MM-dd}";
        public bool IsBoosterDue => NextBoosterDue.HasValue && NextBoosterDue.Value <= DateTime.Today;
        public bool IsBoosterExpiring => NextBoosterDue.HasValue && NextBoosterDue.Value <= DateTime.Today.AddDays(30);
        public string BoosterStatus
        {
            get
            {
                if (!NextBoosterDue.HasValue) return "Not Required";
                if (NextBoosterDue.Value < DateTime.Today) return "Overdue";
                if (NextBoosterDue.Value <= DateTime.Today.AddDays(30)) return "Due Soon";
                return "Up to Date";
            }
        }
    }
}