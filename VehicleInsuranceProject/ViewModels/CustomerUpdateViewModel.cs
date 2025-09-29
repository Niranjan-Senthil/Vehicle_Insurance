using System.ComponentModel.DataAnnotations;

namespace VehicleInsuranceProject.ViewModels
{
    public class CustomerUpdateViewModel
    {
        public int CustomerId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }
    }
}