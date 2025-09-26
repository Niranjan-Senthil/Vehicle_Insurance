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
    public class ReportingVehicleRepository : IReportingVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportingVehicleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Vehicle> GetAllVehiclesWithDetails()
        {
            // Include Customer, and then count policies/claims via navigation properties if available,
            // or perform separate counts in the service layer if direct navigation isn't clean.
            // For simplicity here, we'll fetch Customer and let service count policies/claims.
            return _context.Vehicles
                           .Include(v => v.Customer)
                           .ToList(); // Execute synchronously
        }
    }
}
