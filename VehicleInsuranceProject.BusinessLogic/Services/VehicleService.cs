using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models; // Correct namespace for models
using VehicleInsuranceProject.Repository.Repositories; // Correct namespace for repositories

namespace VehicleInsuranceProject.BusinessLogic.Services // Correct namespace for business logic
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _repository;

        public VehicleService(IVehicleRepository repository)
        {
            _repository = repository;
        }

        public void AddVehicle(Vehicle vehicle)
        {
            if (vehicle.CustomerId <= 0) // Changed to CustomerId
            {
                throw new ArgumentException("CustomerId is required.");
            }
            // Example validation
            if (string.IsNullOrWhiteSpace(vehicle.registrationNumber))
                throw new ArgumentException("Registration Number is required.");
            if (string.IsNullOrWhiteSpace(vehicle.make) || string.IsNullOrWhiteSpace(vehicle.model))
                throw new ArgumentException("Make and Model are required.");

            if (vehicle.yearOfManufacture < 1900 || vehicle.yearOfManufacture > DateTime.Now.Year ) // Allow next year for new models
                throw new ArgumentException("Invalid year of manufacture.");

            _repository.AddVehicle(vehicle);
            _repository.SaveChanges();
        }

        public IEnumerable<Vehicle> GetVehicleByCustomerId(int customerId)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("A valid CustomerId is required to retrieve vehicles.");
            }
            return _repository.GetVehicleByCustomerId(customerId);
        }

        public Vehicle? GetVehicleById(int vehicleId)
        {
            if (vehicleId <= 0)
            {
                throw new ArgumentException("A valid VehicleId is required.");
            }
            return _repository.GetVehicleById(vehicleId);
        }

        public void UpdateVehicle(Vehicle updatedVehicle)
        {
            if (updatedVehicle.vehicleId <= 0)
            {
                throw new ArgumentException("VehicleId is required for update.");
            }

            var existing = _repository.GetVehicleById(updatedVehicle.vehicleId);
            if (existing == null)
            {
                throw new Exception($"Vehicle with ID {updatedVehicle.vehicleId} not found.");
            }

            // Perform business-level validation for the updated fields
            if (string.IsNullOrWhiteSpace(updatedVehicle.registrationNumber))
                throw new ArgumentException("Registration Number cannot be empty.");
            if (string.IsNullOrWhiteSpace(updatedVehicle.make) || string.IsNullOrWhiteSpace(updatedVehicle.model))
                throw new ArgumentException("Make and Model are required.");
            if (updatedVehicle.yearOfManufacture < 1900 || updatedVehicle.yearOfManufacture > DateTime.Now.Year + 1)
                throw new ArgumentException("Invalid year of manufacture.");

            // Update only the allowed fields
            existing.registrationNumber = updatedVehicle.registrationNumber;
            existing.make = updatedVehicle.make;
            existing.model = updatedVehicle.model;
            existing.yearOfManufacture = updatedVehicle.yearOfManufacture;
            existing.vehicleType = updatedVehicle.vehicleType;

            _repository.UpdateVehicle(existing);
            _repository.SaveChanges();
        }
        public void DeleteVehicle(int vehicleId)
        {
            if (vehicleId <= 0)
            {
                throw new ArgumentException("A valid VehicleId is required to delete a vehicle.");
            }

            var vehicleToDelete = _repository.GetVehicleById(vehicleId);
            if (vehicleToDelete == null)
            {
                throw new Exception($"Vehicle with ID {vehicleId} not found for deletion.");
            }

            _repository.DeleteVehicle(vehicleToDelete);
            _repository.SaveChanges();
        }


        // NEW: For admin to get all vehicles of a specific customer (no IsActive filter)
        public IEnumerable<Vehicle> GetVehiclesForAdminByCustomerId(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("A valid CustomerId is required to retrieve vehicles for admin.");
            return _repository.GetVehicleByCustomerId(customerId); // Uses the existing repository method
        }

        // NEW: For admin to update vehicle details (explicitly for admin access)
        public void UpdateVehicleByAdmin(Vehicle updatedVehicle)
        {
            if (updatedVehicle.vehicleId <= 0)
            {
                throw new ArgumentException("VehicleId is required for update.");
            }

            var existing = _repository.GetVehicleById(updatedVehicle.vehicleId);
            if (existing == null)
            {
                throw new Exception($"Vehicle with ID {updatedVehicle.vehicleId} not found.");
            }

            // Perform validation
            if (string.IsNullOrWhiteSpace(updatedVehicle.registrationNumber))
                throw new ArgumentException("Registration Number cannot be empty.");
            if (string.IsNullOrWhiteSpace(updatedVehicle.make) || string.IsNullOrWhiteSpace(updatedVehicle.model))
                throw new ArgumentException("Make and Model are required.");
            if (updatedVehicle.yearOfManufacture < 1900 || updatedVehicle.yearOfManufacture > DateTime.Now.Year + 1)
                throw new ArgumentException("Invalid year of manufacture.");

            // Update all editable fields from the admin form (no IsActive field)
            existing.registrationNumber = updatedVehicle.registrationNumber;
            existing.make = updatedVehicle.make;
            existing.model = updatedVehicle.model;
            existing.yearOfManufacture = updatedVehicle.yearOfManufacture;
            existing.vehicleType = updatedVehicle.vehicleType;

            _repository.UpdateVehicle(existing); // Reuse repository update
            _repository.SaveChanges();
        }
    }
}