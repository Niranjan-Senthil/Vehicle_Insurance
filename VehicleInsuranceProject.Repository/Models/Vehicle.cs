using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema; // IMPORTANT: Add this for [ForeignKey]

namespace VehicleInsuranceProject.Repository.Models
{
    public class Vehicle
    {
        public int vehicleId { get; set; }
        public int CustomerId { get; set; } // Foreign Key to CustomerId
        public string registrationNumber { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public int yearOfManufacture { get; set; }
        public VehicleType vehicleType { get; set; }

        // NEW: Navigation property to the Customer who owns this vehicle
        // This is essential for EF Core to understand the relationship
        [ForeignKey("CustomerId")] // Explicitly links this property to the CustomerId foreign key
        public Customer? Customer { get; set; }
    }
}
