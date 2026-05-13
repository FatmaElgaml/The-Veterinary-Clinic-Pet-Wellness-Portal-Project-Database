using System;

namespace VeterinaryClinicProject.Models
{
    public class ClinicalNote
    {
        public int NoteId { get; set; }
        public int VisitId { get; set; }
        public decimal? WeightKg { get; set; }
        public string Diagnosis { get; set; }
        public string TreatmentPlan { get; set; }
        public string GeneralObservations { get; set; }
        public DateTime? RecordedAt { get; set; }
    }
}