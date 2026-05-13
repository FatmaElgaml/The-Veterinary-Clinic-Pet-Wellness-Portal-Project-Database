using System;

namespace VeterinaryClinicProject.Models
{
    public class Owner
    {
        public int OwnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string BillingAddress { get; set; }
        public string EmergencyContact { get; set; }
    }
}