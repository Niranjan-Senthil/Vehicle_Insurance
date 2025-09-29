using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.BusinessLogic.Services
{
    public interface IPolicyService
    {
        Policy CreatePolicy(Policy policy);
        void UpdatePolicy(Policy policy);
        void RenewPolicy(int policyId, DateTime newEndDate, decimal newPremiumAmount, decimal newCoverageAmount);
        void DeletePolicy(int policyId);
        Policy? GetPolicyById(int policyId); // Synchronous get for immediate use
        Task<Policy?> GetPolicyWithVehicleAndCustomerAsync(int policyId); // NEW: For admin details with eager loading
        IEnumerable<Policy> GetPoliciesByVehicleId(int vehicleId);
        IEnumerable<Policy> GetPoliciesByCustomerId(int customerId);
        Task<IEnumerable<Policy>> GetAllPoliciesAsync(); // Async signature
        Task DeactivatePolicyAsync(int policyId); // NEW: Method to deactivate policy

        string GenerateNewPolicyNumber();
        (decimal premium, decimal coverage) CalculatePremiumAndCoverage(Vehicle vehicle, CoverageLevel coverageLevel);
        Task<IEnumerable<Policy>> GetPoliciesByCustomerIdWithClaimsAsync(int customerId);
    }
}
