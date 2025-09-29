// VehicleInsuranceProject.ViewModels/CustomerLoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace VehicleInsuranceProject.ViewModels
{
    public class CustomerLoginViewModel
    {
        [Required(ErrorMessage = "Username or Email is required.")]
        [Display(Name = "Username / Email")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")] // ADD OR CONFIRM THIS LINE
        public bool RememberMe { get; set; }
    }
}