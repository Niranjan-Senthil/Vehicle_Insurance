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
    public class ReportingPolicyRepository : IReportingPolicyRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportingPolicyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Policy> GetAllPoliciesWithDetails()
        {
            // Eager load all necessary related data for admin reports
            return _context.Policies
                           .Include(p => p.Vehicle)
                               .ThenInclude(v => v.Customer) // Load customer for vehicle
                           .ToList(); // Execute synchronously
        }

        public IEnumerable<Policy> GetPoliciesByCustomerIdWithDetails(int customerId)
        {
            // Eager load necessary related data for customer-specific reports
            return _context.Policies
                           .Include(p => p.Vehicle)
                           .Where(p => p.Vehicle != null && p.Vehicle.CustomerId == customerId)
                           .ToList(); // Execute synchronously
        }
    }
}
