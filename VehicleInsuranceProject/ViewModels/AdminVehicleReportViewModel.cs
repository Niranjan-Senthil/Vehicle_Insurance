using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.ViewModels
{
    public class AdminVehicleReportViewModel
    {
        public int VehicleId { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int YearOfManufacture { get; set; }
        public VehicleType VehicleType { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int NumberOfPolicies { get; set; } // Count of policies for this vehicle
        public int NumberOfClaims { get; set; } // Count of claims for this vehicle
    }
}
