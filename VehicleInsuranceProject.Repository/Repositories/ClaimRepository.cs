// VehicleInsuranceProject.Repository.Repositories/ClaimRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleInsuranceProject.Repository.Data;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.Repository.Repositories
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly ApplicationDbContext _context;

        public ClaimRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddClaim(Claim claim)
        {
            _context.Claims.Add(claim);
        }

        public void UpdateClaim(Claim claim)
        {
            _context.Claims.Update(claim);
        }

        public void DeleteClaim(Claim claim)
        {
            _context.Claims.Remove(claim);
        }

        public Claim? GetClaimById(int claimId)
        {
            return _context.Claims
                           .Include(c => c.Policy!) // Eager load the associated Policy
                           .ThenInclude(p => p.Vehicle!) // Then load Vehicle from Policy
                           .ThenInclude(v => v.Customer) // Then load Customer from Vehicle
                           .FirstOrDefault(c => c.claimId == claimId);
        }

        public IEnumerable<Claim> GetClaimsByPolicyId(int policyId)
        {
            return _context.Claims
                           .Where(c => c.policyId == policyId)
                           .ToList();
        }

        public async Task<IEnumerable<Claim>> GetClaimsByCustomerIdAsync(int customerId)
        {
            return await _context.Claims
                                 .Include(c => c.Policy!)
                                 .ThenInclude(p => p.Vehicle)
                                 .Where(c => c.Policy != null && c.Policy.Vehicle != null && c.Policy.Vehicle.CustomerId == customerId)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetAllClaimsAsync()
        {
            return await _context.Claims
                                 .Include(c => c.Policy!)
                                 .ThenInclude(p => p.Vehicle!)
                                 .ThenInclude(v => v.Customer)
                                 .ToListAsync();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}