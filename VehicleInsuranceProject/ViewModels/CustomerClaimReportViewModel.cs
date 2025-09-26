using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.ViewModels
{
    public class CustomerClaimReportViewModel
    {
        public int ClaimId { get; set; }
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string VehicleRegistrationNumber { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public string ClaimReason { get; set; } = string.Empty;
        public DateTime ClaimDate { get; set; }
        public ClaimStatus ClaimStatus { get; set; }
    }
}
