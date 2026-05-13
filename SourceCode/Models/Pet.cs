using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinaryClinicProject.Models
{
    public class Pet
    {
        // Primary key
        public int PetId { get; set; }

        // Pet information
        public string PetName { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public int Age { get; set; }

        // Foreign key
        public int OwnerId { get; set; }

        // Optional: Owner information (for JOIN queries)
        public string OwnerFirstName { get; set; }
        public string OwnerLastName { get; set; }
        public string OwnerPhone { get; set; }

        // Helper property
        public string OwnerFullName => $"{OwnerFirstName} {OwnerLastName}".Trim();
        public string PetInfo => $"{PetName} ({Species}) - Age: {Age}";
    }
}