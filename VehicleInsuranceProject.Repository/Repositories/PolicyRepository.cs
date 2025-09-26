using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Required for .Include() and .ThenInclude()
using VehicleInsuranceProject.Repository.Data;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.Repository.Repositories
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly ApplicationDbContext _context;

        public PolicyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddPolicy(Policy policy)
        {
            _context.Policies.Add(policy);
        }

        public void UpdatePolicy(Policy policy)
        {
            _context.Policies.Update(policy);
        }

        public void DeletePolicy(Policy policy)
        {
            _context.Policies.Remove(policy);
        }

        public Policy? GetPolicyById(int policyId)
        {
            return _context.Policies
                           .Include(p => p.Vehicle) // Eager load the associated Vehicle
                           .Include(p => p.CoverageLevel) // Eager load the CoverageLevel
                           .FirstOrDefault(p => p.policyId == policyId);
        }

        public IEnumerable<Policy> GetPoliciesByVehicleId(int vehicleId)
        {
            return _context.Policies
                           .Where(p => p.vehicleId == vehicleId)
                           .Include(p => p.Vehicle)
                           .Include(p => p.CoverageLevel)
                           .ToList();
        }

        public IEnumerable<Policy> GetPoliciesByCustomerId(int customerId)
        {
            return _context.Policies
                           .Include(p => p.Vehicle!)
                           .ThenInclude(v => v.Customer)
                           .Include(p => p.CoverageLevel)
                           .Where(p => p.Vehicle != null && p.Vehicle.CustomerId == customerId)
                           .ToList();
        }

        // Changed to async: Get all policies, including the associated Vehicle, its Customer, and CoverageLevel
        public async Task<IEnumerable<Policy>> GetAllPoliciesAsync() // Changed signature
        {
            return await _context.Policies
                                 .Include(p => p.Vehicle!)
                                 .ThenInclude(v => v.Customer)
                                 .Include(p => p.CoverageLevel)
                                 .ToListAsync(); // Use ToListAsync() for async operation
        }

        // NEW: Get a single policy with related Vehicle and Customer details
        public async Task<Policy?> GetPolicyWithVehicleAndCustomerAsync(int policyId)
        {
            return await _context.Policies
                                 .Include(p => p.Vehicle!)
                                 .ThenInclude(v => v.Customer)
                                 .Include(p => p.CoverageLevel) // Also include CoverageLevel
                                 .FirstOrDefaultAsync(p => p.policyId == policyId);
        }
        public async Task<IEnumerable<Policy>> GetPoliciesByCustomerIdWithClaimsAsync(int customerId)
        {
            return await _context.Policies
                                 .Include(p => p.Vehicle!)
                                 .ThenInclude(v => v.Customer)
                                 .Include(p => p.CoverageLevel)
                                 .Include(p => p.Claims) // Eager load claims
                                 .Where(p => p.Vehicle != null && p.Vehicle.CustomerId == customerId)
                                 .ToListAsync();
        }
        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
