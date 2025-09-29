// VehicleInsuranceProject.BusinessLogic.Services/ClaimService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VehicleInsuranceProject.Repository.Models;
using VehicleInsuranceProject.Repository.Repositories;

namespace VehicleInsuranceProject.BusinessLogic.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IPolicyRepository _policyRepository; // To get policy details for validation

        public ClaimService(IClaimRepository claimRepository, IPolicyRepository policyRepository)
        {
            _claimRepository = claimRepository;
            _policyRepository = policyRepository;
        }

        public async Task<Claim> FileClaimAsync(Claim claim, List<IFormFile> images, string webRootPath)
        {
            if (claim.policyId <= 0)
            {
                throw new ArgumentException("Policy ID is required to file a claim.");
            }

            var policy = await _policyRepository.GetPolicyWithVehicleAndCustomerAsync(claim.policyId);
            if (policy == null)
            {
                throw new ArgumentException($"Policy with ID {claim.policyId} not found.");
            }

            // Validate claim amount against policy coverage
            if (claim.claimAmount > policy.coverageAmount)
            {
                throw new ArgumentException($"Claim amount (${claim.claimAmount:N2}) cannot exceed the policy's coverage amount (${policy.coverageAmount:N2}).");
            }

            // Validate policy status (e.g., only active policies can file claims)
            if (policy.policyStatus != PolicyStatus.ACTIVE)
            {
                throw new InvalidOperationException($"Claims can only be filed for active policies. Current policy status: {policy.policyStatus}");
            }

            // Handle image uploads
            List<string> uploadedImagePaths = new List<string>();
            if (images != null && images.Any())
            {
                string uploadsFolder = Path.Combine(webRootPath, "uploads", "claims");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }
                        // Store relative path or URL accessible from the web root
                        uploadedImagePaths.Add($"/uploads/claims/{uniqueFileName}");
                    }
                }
            }
            claim.ImagePaths = string.Join(",", uploadedImagePaths);

            claim.claimStatus = ClaimStatus.SUBMITTED; // Set initial status
            claim.claimDate = DateTime.Now; // Set claim date

            _claimRepository.AddClaim(claim);
            _claimRepository.SaveChanges();
            return claim;
        }


        public async Task UpdateClaimStatusAsync(int claimId, ClaimStatus newStatus)
        {
            var claim = _claimRepository.GetClaimById(claimId);
            if (claim == null)
            {
                throw new ArgumentException($"Claim with ID {claimId} not found.");
            }

            claim.claimStatus = newStatus;
            _claimRepository.UpdateClaim(claim);
            _claimRepository.SaveChanges();
            await Task.CompletedTask; // Since SaveChanges is sync, we just complete the task
        }

        public async Task<Claim?> GetClaimByIdAsync(int claimId)
        {
            return await Task.FromResult(_claimRepository.GetClaimById(claimId));
        }

        public async Task<IEnumerable<Claim>> GetClaimsByPolicyIdAsync(int policyId)
        {
            return await Task.FromResult(_claimRepository.GetClaimsByPolicyId(policyId));
        }

        public async Task<IEnumerable<Claim>> GetClaimsByCustomerIdAsync(int customerId)
        {
            return await _claimRepository.GetClaimsByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<Claim>> GetAllClaimsAsync()
        {
            return await _claimRepository.GetAllClaimsAsync();
        }
        public async Task ReapplyClaimAsync(int claimId)
        {
            var claim = _claimRepository.GetClaimById(claimId);
            if (claim == null)
            {
                throw new ArgumentException($"Claim with ID {claimId} not found.");
            }

            // Only allow reapplication if the claim was rejected
            if (claim.claimStatus != ClaimStatus.REJECTED)
            {
                throw new InvalidOperationException($"Claim ID {claimId} cannot be reapplied as its current status is {claim.claimStatus}. Only REJECTED claims can be reapplied.");
            }

            // Set status back to SUBMITTED and update date
            claim.claimStatus = ClaimStatus.SUBMITTED;
            claim.claimDate = DateTime.Now; // Update claim date to current re-submission date

            _claimRepository.UpdateClaim(claim);
            _claimRepository.SaveChanges();
            await Task.CompletedTask;
        }
    }
}