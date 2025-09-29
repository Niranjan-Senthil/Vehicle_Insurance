using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models;
using VehicleInsuranceProject.Repository.Repositories;
using Microsoft.EntityFrameworkCore;

namespace VehicleInsuranceProject.BusinessLogic.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IVehicleService _vehicleService;
        private readonly ICoverageLevelService _coverageLevelService;
        // Removed: private readonly IVehicleTypeConfigService _vehicleTypeConfigService;

        public PolicyService(
            IPolicyRepository policyRepository,
            IVehicleService vehicleService,
            ICoverageLevelService coverageLevelService)
        // Removed: IVehicleTypeConfigService vehicleTypeConfigService)
        {
            _policyRepository = policyRepository;
            _vehicleService = vehicleService;
            _coverageLevelService = coverageLevelService;
            // Removed: _vehicleTypeConfigService = vehicleTypeConfigService;
        }

        public Policy CreatePolicy(Policy policy)
        {
            if (policy.vehicleId <= 0)
            {
                throw new ArgumentException("Vehicle ID is required to create a policy.");
            }
            if (policy.startDate >= policy.endDate)
            {
                throw new ArgumentException("Policy end date must be after start date.");
            }
            if (policy.premiumAmount <= 0 || policy.coverageAmount <= 0)
            {
                throw new ArgumentException("Premium and Coverage amounts must be positive.");
            }
            if (!policy.CoverageLevelId.HasValue || policy.CoverageLevelId.Value <= 0)
            {
                throw new ArgumentException("Coverage Level is required to create a policy.");
            }

            policy.policyNumber = GeneratePolicyNumber();
            policy.policyStatus = PolicyStatus.ACTIVE;

            _policyRepository.AddPolicy(policy);
            _policyRepository.SaveChanges();
            return policy;
        }

        public void UpdatePolicy(Policy updatedPolicy)
        {
            if (updatedPolicy.policyId <= 0)
            {
                throw new ArgumentException("Policy ID is required for update.");
            }

            var existingPolicy = _policyRepository.GetPolicyById(updatedPolicy.policyId);
            if (existingPolicy == null)
            {
                throw new Exception($"Policy with ID {updatedPolicy.policyId} not found.");
            }

            existingPolicy.coverageAmount = updatedPolicy.coverageAmount;
            existingPolicy.premiumAmount = updatedPolicy.premiumAmount;
            existingPolicy.startDate = updatedPolicy.startDate;
            existingPolicy.endDate = updatedPolicy.endDate;
            existingPolicy.policyStatus = updatedPolicy.policyStatus;
            existingPolicy.CoverageLevelId = updatedPolicy.CoverageLevelId;

            if (existingPolicy.endDate < DateTime.Today && existingPolicy.policyStatus == PolicyStatus.ACTIVE)
            {
                existingPolicy.policyStatus = PolicyStatus.EXPIRED;
            }

            _policyRepository.UpdatePolicy(existingPolicy);
            _policyRepository.SaveChanges();
        }

        public void RenewPolicy(int policyId, DateTime newEndDate, decimal newPremiumAmount, decimal newCoverageAmount)
        {
            var policyToRenew = _policyRepository.GetPolicyById(policyId);
            if (policyToRenew == null)
            {
                throw new Exception($"Policy with ID {policyId} not found for renewal.");
            }

            if (newEndDate <= DateTime.Today)
            {
                throw new ArgumentException("New end date for renewal must be in the future.");
            }
            if (newPremiumAmount <= 0 || newCoverageAmount <= 0)
            {
                throw new ArgumentException("New premium and coverage amounts must be positive for renewal.");
            }

            policyToRenew.startDate = DateTime.Today;
            policyToRenew.endDate = newEndDate;
            policyToRenew.premiumAmount = newPremiumAmount;
            policyToRenew.coverageAmount = newCoverageAmount;
            policyToRenew.policyStatus = PolicyStatus.ACTIVE;

            _policyRepository.UpdatePolicy(policyToRenew);
            _policyRepository.SaveChanges();
        }

        public void DeletePolicy(int policyId)
        {
            if (policyId <= 0)
            {
                throw new ArgumentException("A valid PolicyId is required to delete a policy.");
            }

            var policyToDelete = _policyRepository.GetPolicyById(policyId);
            if (policyToDelete == null)
            {
                throw new Exception($"Policy with ID {policyId} not found for deletion.");
            }

            _policyRepository.DeletePolicy(policyToDelete);
            _policyRepository.SaveChanges();
        }

        public Policy? GetPolicyById(int policyId)
        {
            if (policyId <= 0)
            {
                throw new ArgumentException("A valid PolicyId is required.");
            }
            return _policyRepository.GetPolicyById(policyId);
        }

        public async Task<Policy?> GetPolicyWithVehicleAndCustomerAsync(int policyId)
        {
            if (policyId <= 0)
            {
                throw new ArgumentException("A valid PolicyId is required.");
            }
            return await _policyRepository.GetPolicyWithVehicleAndCustomerAsync(policyId);
        }


        public IEnumerable<Policy> GetPoliciesByVehicleId(int vehicleId)
        {
            if (vehicleId <= 0)
            {
                throw new ArgumentException("A valid VehicleId is required to retrieve policies.");
            }
            return _policyRepository.GetPoliciesByVehicleId(vehicleId);
        }

        public IEnumerable<Policy> GetPoliciesByCustomerId(int customerId)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("A valid CustomerId is required to retrieve policies.");
            }
            return _policyRepository.GetPoliciesByCustomerId(customerId);
        }

        public async Task<IEnumerable<Policy>> GetAllPoliciesAsync()
        {
            return await _policyRepository.GetAllPoliciesAsync();
        }

        public async Task DeactivatePolicyAsync(int policyId)
        {
            var policy = await Task.Run(() => _policyRepository.GetPolicyById(policyId));
            if (policy == null)
            {
                throw new Exception($"Policy with ID {policyId} not found for deactivation.");
            }
            if (policy.policyStatus == PolicyStatus.EXPIRED)
            {
                throw new InvalidOperationException($"Policy with ID {policyId} is already expired.");
            }

            policy.policyStatus = PolicyStatus.EXPIRED;
            policy.endDate = DateTime.Today;

            _policyRepository.UpdatePolicy(policy);
            _policyRepository.SaveChanges();
        }

        public (decimal premium, decimal coverage) CalculatePremiumAndCoverage(Vehicle vehicle, CoverageLevel coverageLevel)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle), "Vehicle cannot be null for premium calculation.");
            }
            if (coverageLevel == null)
            {
                throw new ArgumentNullException(nameof(coverageLevel), "Coverage Level cannot be null for premium calculation.");
            }

            decimal basePremium = 0;
            decimal baseCoverage = 0;

            // Base premium and coverage based on vehicle type (hardcoded values, as requested)
            switch (vehicle.vehicleType)
            {
                case VehicleType.CAR:
                    basePremium = 500m;
                    baseCoverage = 50000m;
                    break;
                case VehicleType.BIKE:
                    basePremium = 200m;
                    baseCoverage = 20000m;
                    break;
                case VehicleType.TRUCK:
                    basePremium = 800m;
                    baseCoverage = 100000m;
                    break;
                case VehicleType.JEEP:
                    basePremium = 600m;
                    baseCoverage = 75000m;
                    break;
                default:
                    basePremium = 400m;
                    baseCoverage = 40000m;
                    break;
            }

            // Adjust premium and coverage based on year of manufacture
            int currentYear = DateTime.Now.Year;
            int age = currentYear - vehicle.yearOfManufacture;

            if (age > 10)
            {
                basePremium *= 1.1m;
                baseCoverage *= 0.8m;
            }
            else if (age < 3)
            {
                basePremium *= 1.05m;
                baseCoverage *= 1.2m;
            }

            // Apply multipliers from the dynamic CoverageLevel object
            basePremium *= coverageLevel.PremiumMultiplier;
            baseCoverage *= coverageLevel.CoverageMultiplier;

            // Ensure amounts are not negative
            basePremium = Math.Max(1, basePremium);
            baseCoverage = Math.Max(1, baseCoverage);

            // Round to 2 decimal places
            return (Math.Round(basePremium, 2), Math.Round(baseCoverage, 2));
        }

        private string GeneratePolicyNumber()
        {
            return $"POL-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        public string GenerateNewPolicyNumber()
        {
            return $"POL-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
        public async Task<IEnumerable<Policy>> GetPoliciesByCustomerIdWithClaimsAsync(int customerId)
        {
            return await _policyRepository.GetPoliciesByCustomerIdWithClaimsAsync(customerId);
        }
    }
}
