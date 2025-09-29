// VehicleInsuranceProject.BusinessLogic.Services/ICustomerService.cs
using Microsoft.AspNetCore.Identity;
using VehicleInsuranceProject.Repository.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VehicleInsuranceProject.BusinessLogic.Services
{
    public interface ICustomerService
    {
        Task<IdentityResult> CreateCustomerAsync(Customer customer, string password);
        Task<Customer> GetCustomerByIdAsync(string identityUserId);
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(int customerId);
        Task<IEnumerable<Customer>> GetAllCustomersAsync();

        // THIS METHOD IS STILL NEEDED FOR THE UPDATEPROFILE SCENARIO
        Task AddCustomerProfileAsync(Customer customer);


        // Methods For Admin 
        Task<Customer?> GetCustomerByCustomerIdAsync(int customerId);

        // NEW: Methods for administrative activate/deactivate
        Task DeactivateCustomerAsync(int customerId);
        Task ActivateCustomerAsync(int customerId);

        // NEW: Method for listing all customers (including inactive) for admin
        Task<IEnumerable<Customer>> GetAllCustomersForAdminAsync();

        // NEW: Method for searching customers
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
      
    }
}