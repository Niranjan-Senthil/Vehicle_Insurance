namespace VehicleInsuranceProject.ViewModels
{
    public class AdminCustomerReportViewModel
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } // Confirmed from your Customer.cs
        public int TotalVehicles { get; set; }
        public int ActivePolicies { get; set; } // Count of active policies
        public int TotalClaims { get; set; } // Count of all claims
    }
}
