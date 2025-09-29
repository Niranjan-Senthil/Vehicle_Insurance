// VehicleInsuranceProject.Web/ViewModels/ClaimViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace VehicleInsuranceProject.Web.ViewModels
{
    public class ClaimViewModel
    {
        public int PolicyId { get; set; }

        [Display(Name = "Policy Number")]
        public string? PolicyNumber { get; set; } // To display pre-filled

        [Display(Name = "Policy Coverage Amount")]
        public decimal PolicyCoverageAmount { get; set; } // To validate against

        [Required(ErrorMessage = "Claim amount is required.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Claim amount must be a positive value.")]
        [Display(Name = "Claim Amount")]
        public decimal ClaimAmount { get; set; }

        [Required(ErrorMessage = "Reason for claim is required.")]
        [StringLength(500, ErrorMessage = "Claim reason cannot exceed 500 characters.")]
        [Display(Name = "Reason for Claim")]
        [DataType(DataType.MultilineText)]
        public string ClaimReason { get; set; } = string.Empty;

        [Display(Name = "Upload Images (optional)")]
        // Use List<IFormFile> to allow multiple file uploads
        public List<IFormFile>? ClaimImages { get; set; }
    }
}