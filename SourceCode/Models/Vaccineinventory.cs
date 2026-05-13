using System;

namespace VeterinaryClinicProject.Models
{
    public class VaccineInventory
    {
        public int InventoryId { get; set; }
        public int ClinicId { get; set; }
        public string VaccineInventoryType { get; set; }
        public string BatchNumber { get; set; }
        public string SupplierName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? QuantityAvailable { get; set; }
        public int? ReorderThreshold { get; set; }
    }
}