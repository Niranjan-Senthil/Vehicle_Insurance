// VehicleInsuranceProject.Repository.Repositories/IClaimRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.Repository.Repositories
{
    public interface IClaimRepository
    {
        void AddClaim(Claim claim);
        void UpdateClaim(Claim claim);
        void DeleteClaim(Claim claim);
        Claim? GetClaimById(int claimId);
        IEnumerable<Claim> GetClaimsByPolicyId(int policyId);
        Task<IEnumerable<Claim>> GetClaimsByCustomerIdAsync(int customerId); // NEW: Get claims by customer ID
        Task<IEnumerable<Claim>> GetAllClaimsAsync(); // NEW: Get all claims for admin
        void SaveChanges();
    }
}