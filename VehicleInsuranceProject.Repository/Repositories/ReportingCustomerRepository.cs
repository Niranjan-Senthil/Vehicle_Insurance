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
    public class ReportingCustomerRepository : IReportingCustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportingCustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Customer> GetAllCustomersWithDetails()
        {
            // Eager load only IdentityUser and Vehicles.
            // Policy and Claim counts will be calculated in ReportService separately.
            return _context.Customers
                           .Include(c => c.IdentityUser) // For IdentityUser details like username/email
                           .Include(c => c.Vehicles) // Load vehicles for total count
                           .ToList(); // Execute synchronously
        }
    }
}
