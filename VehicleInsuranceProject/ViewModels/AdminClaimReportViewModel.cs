using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.ViewModels
{
    public class AdminClaimReportViewModel
    {
        public int ClaimId { get; set; }
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehicleRegistrationNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public string ClaimReason { get; set; } = string.Empty;
        public DateTime ClaimDate { get; set; }
        public ClaimStatus ClaimStatus { get; set; }
    }
}
