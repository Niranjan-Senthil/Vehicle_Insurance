using VehicleInsuranceProject.Repository.Models;
using System.ComponentModel.DataAnnotations;

namespace VehicleInsuranceProject.ViewModels
{
    public class AdminPolicyViewModel
    {
        public int PolicyId { get; set; }
        [Display(Name = "Policy Number")]
        public string PolicyNumber { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Display(Name = "Vehicle Reg. No.")]
        public string VehicleRegistrationNumber { get; set; }

        [Display(Name = "Vehicle Type")]
        public VehicleType VehicleType { get; set; }

        [Display(Name = "Coverage Level")]
        public string CoverageLevelName { get; set; }

        [Display(Name = "Coverage Amount")]
        public decimal CoverageAmount { get; set; }

        [Display(Name = "Premium Amount")]
        public decimal PremiumAmount { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Status")]
        public PolicyStatus PolicyStatus { get; set; }
    }
}
