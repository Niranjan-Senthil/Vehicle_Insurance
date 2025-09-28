using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.Repository.Repositories
{
    public interface IPolicyRepository
    {
        void AddPolicy(Policy policy);
        void UpdatePolicy(Policy policy);
        void DeletePolicy(Policy policy);
        Policy? GetPolicyById(int policyId);
        IEnumerable<Policy> GetPoliciesByVehicleId(int vehicleId);
        IEnumerable<Policy> GetPoliciesByCustomerId(int customerId);
        Task<IEnumerable<Policy>> GetAllPoliciesAsync(); // Changed to async Task<IEnumerable<Policy>>
        Task<Policy?> GetPolicyWithVehicleAndCustomerAsync(int policyId); // NEW: For admin details
        void SaveChanges();
        Task<IEnumerable<Policy>> GetPoliciesByCustomerIdWithClaimsAsync(int customerId);
    }
}
