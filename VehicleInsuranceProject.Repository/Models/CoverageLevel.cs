using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Add this for Column attribute

namespace VehicleInsuranceProject.Repository.Models
{
    public class CoverageLevel
    {
        [Key]
        public int CoverageLevelId { get; set; }

        [Required(ErrorMessage = "Coverage Level Name is required.")]
        [StringLength(50, ErrorMessage = "Coverage Level Name cannot exceed 50 characters.")]
        [Display(Name = "Coverage Name")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Premium Multiplier is required.")]
        [Range(0.1, 10.0, ErrorMessage = "Premium Multiplier must be between 0.1 and 10.0.")]
        [Column(TypeName = "decimal(18,2)")] // Fixes decimal warning
        [Display(Name = "Premium Multiplier")]
        public decimal PremiumMultiplier { get; set; } = 1.0m;

        [Required(ErrorMessage = "Coverage Multiplier is required.")]
        [Range(0.1, 10.0, ErrorMessage = "Coverage Multiplier must be between 0.1 and 10.0.")]
        [Column(TypeName = "decimal(18,2)")] // Fixes decimal warning
        [Display(Name = "Coverage Multiplier")]
        public decimal CoverageMultiplier { get; set; } = 1.0m;
    }
}