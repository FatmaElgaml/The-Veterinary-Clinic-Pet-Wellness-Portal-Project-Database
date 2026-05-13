using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinaryClinicProject.Models
{
    public class Visit
    {
        // Primary key
        public int VisitId { get; set; }

        // Foreign keys
        public int PetId { get; set; }
        public int SlotId { get; set; }
        public int NoteId { get; set; }

        // Visit information
        public DateTime VisitDate { get; set; }
        public string VisitStatus { get; set; }  // Scheduled, Completed, Cancelled

        // ============================================================
        // Pet information (from JOIN)
        // ============================================================
        public string PetName { get; set; }
        public string Species { get; set; }

        // ============================================================
        // Owner information (from JOIN)
        // ============================================================
        public string OwnerFirstName { get; set; }
        public string OwnerLastName { get; set; }
        public string OwnerPhone { get; set; }

        public string OwnerFullName => $"{OwnerFirstName} {OwnerLastName}".Trim();

        // ============================================================
        // Veterinarian information (from JOIN)
        // ============================================================
        public string VetFirstName { get; set; }
        public string VetLastName { get; set; }
        public string VetSpecialty { get; set; }

        public string VetFullName => $"{VetFirstName} {VetLastName}".Trim();

        // ============================================================
        // Clinic information (from JOIN)
        // ============================================================
        public string ClinicName { get; set; }
        public string ClinicLocation { get; set; }

        // ============================================================
        // Clinical Note information (from JOIN)
        // ============================================================
        public string Diagnosis { get; set; }
        public string TreatmentPlan { get; set; }

        // ============================================================
        // Helper properties
        // ============================================================
        public string VisitInfo => $"{VisitDate:yyyy-MM-dd} - {PetName} ({VisitStatus})";
        public string DisplayDate => VisitDate.ToString("yyyy-MM-dd HH:mm");
    }
}