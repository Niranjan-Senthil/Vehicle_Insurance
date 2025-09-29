using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models; // Correct namespace for models

namespace VehicleInsuranceProject.BusinessLogic.Services // Correct namespace for business logic
{
    public interface IVehicleService
    {
        void AddVehicle(Vehicle vehicle);
        IEnumerable<Vehicle> GetVehicleByCustomerId(int customerId);
        Vehicle GetVehicleById(int vehicleId);
        void UpdateVehicle(Vehicle vehicle);

        void DeleteVehicle(int vehicleId);

        // NEW: For admin to get all vehicles of a specific customer (no IsActive filter)
        IEnumerable<Vehicle> GetVehiclesForAdminByCustomerId(int customerId);

        // NEW: For admin to update vehicle details (explicitly for admin)
        void UpdateVehicleByAdmin(Vehicle vehicle);
    }
}