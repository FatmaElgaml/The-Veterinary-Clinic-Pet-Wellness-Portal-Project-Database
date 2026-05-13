using System;

namespace VeterinaryClinicProject.Models
{
    public class Reminder
    {
        public int ReminderId { get; set; }
        public int OwnerId { get; set; }
        public int VaccinationId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Channel { get; set; }
        public string ReminderStatus { get; set; }
        public DateTime? SentAt { get; set; }
    }
}