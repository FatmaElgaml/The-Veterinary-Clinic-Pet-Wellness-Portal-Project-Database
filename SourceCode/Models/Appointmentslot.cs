using System;

namespace VeterinaryClinicProject.Models
{
    public class AppointmentSlot
    {
        public int SlotId { get; set; }
        public int VetClinicId { get; set; }   // FK → VET_CLINIC.ATTRIBUTE_70
        public int VisitId { get; set; }
        public DateTime SlotDateTime { get; set; }
        public int? DurationMinutes { get; set; }
        public string Status { get; set; }
    }
}