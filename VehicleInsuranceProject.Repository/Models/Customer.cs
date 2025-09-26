using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace VehicleInsuranceProject.Repository.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        // Foreign Key to AspNetUsers.Id
        [Required(ErrorMessage = "Identity User ID is required.")]
        public string IdentityUserId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number.")]
        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters.")]
        public string? Phone { get; set; } // Nullable if not always required

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        public string? Address { get; set; } // Nullable if not always required

        // NEW: Field to track if the customer account is active/inactive
        public bool IsActive { get; set; } = true; // Default to active for new customers

        // Navigation property (optional)
        // Ensure you load this with .Include() in queries if you use it in views/logic.
        public IdentityUser? IdentityUser { get; set; } // Made nullable to avoid issues if not eager loaded

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>(); // Assuming this is correct
    }
}
