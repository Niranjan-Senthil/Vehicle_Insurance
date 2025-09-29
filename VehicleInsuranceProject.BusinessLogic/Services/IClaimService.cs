// VehicleInsuranceProject.BusinessLogic.Services/IClaimService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // For IFormFile
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.BusinessLogic.Services
{
    public interface IClaimService
    {
        Task<Claim> FileClaimAsync(Claim claim, List<IFormFile> images, string webRootPath);
        Task UpdateClaimStatusAsync(int claimId, ClaimStatus newStatus);
        Task<Claim?> GetClaimByIdAsync(int claimId);
        Task<IEnumerable<Claim>> GetClaimsByPolicyIdAsync(int policyId);
        Task<IEnumerable<Claim>> GetClaimsByCustomerIdAsync(int customerId);
        Task<IEnumerable<Claim>> GetAllClaimsAsync(); // For admin

    }
}