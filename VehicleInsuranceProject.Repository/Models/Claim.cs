// VehicleInsuranceProject.Repository.Models/Claim.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleInsuranceProject.Repository.Models
{
    public enum ClaimStatus
    {
        SUBMITTED,
        APPROVED,
        REJECTED
    }

    public class Claim
    {
        [Key]
        public int claimId { get; set; }

        [Required]
        public int policyId { get; set; } // Foreign Key to Policy

        [ForeignKey("policyId")]
        public virtual Policy? Policy { get; set; } // Navigation property

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, 9999999.99, ErrorMessage = "Claim amount must be a positive value.")]
        public decimal claimAmount { get; set; }

        [Required(ErrorMessage = "Claim reason is required.")]
        [StringLength(500, ErrorMessage = "Claim reason cannot exceed 500 characters.")]
        public string claimReason { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime claimDate { get; set; } = DateTime.Now; // Default to current date

        [Required]
        public ClaimStatus claimStatus { get; set; } = ClaimStatus.SUBMITTED; // Default status

        // Property to store comma-separated image paths
        public string? ImagePaths { get; set; }
    }
}