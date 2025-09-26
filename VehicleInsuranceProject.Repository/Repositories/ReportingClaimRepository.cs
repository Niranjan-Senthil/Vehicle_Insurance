using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleInsuranceProject.Repository.Data;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.Repository.Repositories
{
    public class ReportingClaimRepository : IReportingClaimRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportingClaimRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Claim> GetAllClaimsWithDetails()
        {
            // Eager load all necessary related data for admin reports
            return _context.Claims
                           .Include(c => c.Policy)
                               .ThenInclude(p => p.Vehicle)
                                   .ThenInclude(v => v.Customer) // Load customer for vehicle via policy
                           .ToList(); // Execute synchronously
        }

        public IEnumerable<Claim> GetClaimsByCustomerIdWithDetails(int customerId)
        {
            // This is complex as Claim is linked to Policy, then Policy to Vehicle, then Vehicle to Customer.
            // We need to filter claims by policies owned by the customer.
            return _context.Claims
                           .Include(c => c.Policy)
                               .ThenInclude(p => p.Vehicle)
                           .Where(c => c.Policy != null && c.Policy.Vehicle != null && c.Policy.Vehicle.CustomerId == customerId)
                           .ToList(); // Execute synchronously
        }
    }
}
