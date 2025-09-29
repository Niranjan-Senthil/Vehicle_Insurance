using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleInsuranceProject.Repository.Data; // Correct namespace for DbContext
using VehicleInsuranceProject.Repository.Models; // Correct namespace for models

namespace VehicleInsuranceProject.Repository.Repositories // Correct namespace for repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _context; // Changed to ApplicationDbContext

        public VehicleRepository(ApplicationDbContext context) // Changed to ApplicationDbContext
        {
            _context = context;
        }

        public void AddVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public IEnumerable<Vehicle> GetVehicleByCustomerId(int customerId)
        {
            return _context.Vehicles.Where(v => v.CustomerId == customerId).ToList(); // Changed to CustomerId
        }

        public Vehicle? GetVehicleById(int vehicleId)
        {
            return _context.Vehicles.FirstOrDefault(v => v.vehicleId == vehicleId);
        }

        public void UpdateVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
        }
        public void DeleteVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Remove(vehicle);
        }

        // NEW: Implementation for admin to get all vehicles with customer data
        public IEnumerable<Vehicle> GetAllVehiclesIncludingCustomers()
        {
            return _context.Vehicles.Include(v => v.Customer).ToList();
        }


    }
}