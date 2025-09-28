using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.ViewModels
{
    public class CustomerPolicyReportViewModel
    {
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string VehicleRegistrationNumber { get; set; } = string.Empty;
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public int VehicleYear { get; set; }
        public VehicleType VehicleType { get; set; }
        public decimal CoverageAmount { get; set; }
        public decimal PremiumAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PolicyStatus PolicyStatus { get; set; }
    }
}
