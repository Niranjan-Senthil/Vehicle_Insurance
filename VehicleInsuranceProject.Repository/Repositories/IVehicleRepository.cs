using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models; // Correct namespace for models

namespace VehicleInsuranceProject.Repository.Repositories // Correct namespace for repositories
{
    public interface IVehicleRepository
    {
        void AddVehicle(Vehicle vehicle);
        void SaveChanges();
        IEnumerable<Vehicle> GetVehicleByCustomerId(int customerId);
        Vehicle? GetVehicleById(int vehicleId); // Changed to nullable
        void UpdateVehicle(Vehicle vehicle);

        void DeleteVehicle(Vehicle vehicle);


        // NEW: For admin to get all vehicles (including associated customer)
        IEnumerable<Vehicle> GetAllVehiclesIncludingCustomers();
    }
}