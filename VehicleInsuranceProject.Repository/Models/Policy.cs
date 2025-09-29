// VehicleInsuranceProject.Repository.Models/Policy.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleInsuranceProject.Repository.Models
{
    public enum PolicyStatus
    {
        ACTIVE,
        EXPIRED,
        CANCELLED
    }

    public class Policy
    {
        [Key]
        public int policyId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Policy number cannot exceed 50 characters.")]
        public string policyNumber { get; set; } = string.Empty;

        [Required]
        public DateTime PolicyDate { get; set; } // Date policy was created

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime startDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime endDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, 10000000.00, ErrorMessage = "Premium amount must be positive.")]
        public decimal premiumAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, 100000000.00, ErrorMessage = "Coverage amount must be positive.")]
        public decimal coverageAmount { get; set; }

        [Required]
        public PolicyStatus policyStatus { get; set; } = PolicyStatus.ACTIVE;

        // Foreign Key to Vehicle
        [Required]
        public int vehicleId { get; set; }

        [ForeignKey("vehicleId")]
        public virtual Vehicle? Vehicle { get; set; } // Navigation property

        // Foreign Key to CoverageLevel (nullable)
        public int? CoverageLevelId { get; set; }

        [ForeignKey("CoverageLevelId")]
        public virtual CoverageLevel? CoverageLevel { get; set; } // Navigation property

        // NEW: Navigation property for Claims associated with this policy
        public ICollection<Claim>? Claims { get; set; }
    }
}