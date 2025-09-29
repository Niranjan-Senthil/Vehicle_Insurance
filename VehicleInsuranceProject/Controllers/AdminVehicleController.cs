using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleInsuranceProject.BusinessLogic.Services;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.Controllers
{
    [Authorize(Roles = "Admin")] // Only accessible by administrators
    public class AdminVehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerService _customerService; // To get customer details for context

        public AdminVehicleController(IVehicleService vehicleService, ICustomerService customerService)
        {
            _vehicleService = vehicleService;
            _customerService = customerService;
        }

        // GET: AdminVehicle/ListCustomerVehicles/{customerId}
        // Lists all vehicles for a specific customer (no IsActive filter as per request)
        [HttpGet]
        public IActionResult ListCustomerVehicles(int customerId)
        {
            // Assuming GetCustomerByCustomerId is synchronous if you don't want async
            var customer = _customerService.GetCustomerByCustomerIdAsync(customerId).Result; // .Result to synchronously wait for async
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return NotFound();
            }

            ViewData["Title"] = $"Vehicles for {customer.Name} (ID: {customer.CustomerId})";
            ViewBag.CustomerId = customerId; // Pass CustomerId to the view for links
            ViewBag.CustomerName = customer.Name; // Pass Customer Name to the view

            // Assuming GetVehiclesForAdminByCustomerId is synchronous
            var vehicles = _vehicleService.GetVehiclesForAdminByCustomerId(customerId);
            return View(vehicles); // Will look for Views/AdminVehicle/ListCustomerVehicles.cshtml
        }

        // GET: AdminVehicle/EditVehicle/{vehicleId}
        // Displays the form to edit a specific vehicle (admin-level)
        [HttpGet]
        public IActionResult EditVehicle(int vehicleId)
        {

            // Assuming GetVehicleById is synchronous
            var vehicle = _vehicleService.GetVehicleById(vehicleId);
            var customer = _customerService.GetCustomerByCustomerIdAsync(vehicle.CustomerId).Result;
            if (vehicle == null)
            {
                TempData["ErrorMessage"] = "Vehicle not found for editing.";
                return NotFound();
            }

            ViewData["Title"] = $"Edit Vehicle: {vehicle.registrationNumber}";
            ViewBag.CustomerId = vehicle.CustomerId; // Pass CustomerId for "Back to List" link
            ViewBag.CustomerName = customer?.Name; // Pass Customer Name for display

            // Populate VehicleType dropdown
            ViewBag.VehicleTypes = new SelectList(Enum.GetValues(typeof(VehicleType)));

            return View(vehicle); // Will look for Views/AdminVehicle/EditVehicle.cshtml
        }

        // POST: AdminVehicle/EditVehicle
        // Handles the submission of the edited vehicle form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditVehicle(Vehicle vehicle)
        {
            // Re-fetch customer info in case ModelState is invalid and we need to return the view
            // Assuming GetCustomerByCustomerId is synchronous if you don't want async
            var customer = _customerService.GetCustomerByCustomerIdAsync(vehicle.CustomerId).Result; // .Result to synchronously wait for async
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Associated customer not found.";
                return BadRequest();
            }
            ViewBag.CustomerId = customer.CustomerId;
            ViewBag.CustomerName = customer.Name;
            ViewBag.VehicleTypes = new SelectList(Enum.GetValues(typeof(VehicleType)), vehicle.vehicleType); // Keep selection

            if (ModelState.IsValid)
            {
                try
                {
                    _vehicleService.UpdateVehicleByAdmin(vehicle); // Call admin-specific update
                    TempData["SuccessMessage"] = "Vehicle updated successfully by Admin!";
                    return RedirectToAction(nameof(ListCustomerVehicles), new { customerId = vehicle.CustomerId });
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An unexpected error occurred while updating the vehicle: {ex.Message}");
                }
            }
            ViewData["Title"] = $"Edit Vehicle: {vehicle.registrationNumber}";
            return View(vehicle);
        }

        // POST: AdminVehicle/DeleteVehicle/{vehicleId} (Admin can delete any vehicle)
        [HttpPost, ActionName("DeleteVehicle")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteVehicleConfirmed(int vehicleId, int customerId)
        {
            try
            {
                _vehicleService.DeleteVehicle(vehicleId); // Call common delete
                TempData["SuccessMessage"] = "Vehicle deleted successfully by Admin!";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex) // Catches if vehicle has active policies (from PolicyService check)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred while deleting vehicle: {ex.Message}";
            }
            return RedirectToAction(nameof(ListCustomerVehicles), new { customerId = customerId });
        }
    }
}
