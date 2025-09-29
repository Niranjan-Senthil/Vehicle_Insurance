using Microsoft.AspNetCore.Mvc;
using VehicleInsuranceProject.BusinessLogic.Services; // Correct namespace for services
using VehicleInsuranceProject.Repository.Models; // Correct namespace for models
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks; // Required for async/await

namespace VehicleInsuranceProject.Controllers // Ensure this namespace matches your UI project
{
    [Authorize(Roles = "Customer")] // Restrict access to authenticated customers
    public class VehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerService _customerService;
        private readonly UserManager<IdentityUser> _userManager;

        public VehicleController(IVehicleService vehicleService, ICustomerService customerService, UserManager<IdentityUser> userManager)
        {
            _vehicleService = vehicleService;
            _customerService = customerService;
            _userManager = userManager;
        }

        // Action for the main vehicle page (can redirect to specific customer's vehicles)
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Customer"); // Redirect to login if not authenticated
            }

            var customer = await _customerService.GetCustomerByIdAsync(currentUser.Id);
            if (customer == null)
            {
                // This scenario indicates an IdentityUser exists, but no corresponding Customer entity.
                // This might happen if registration process is incomplete or flawed.
                // Redirect to a page where the customer can complete their profile.
                TempData["ErrorMessage"] = "Please complete your customer profile before adding vehicles.";
                return RedirectToAction("UpdateProfile", "Customer"); // Assuming this action exists
            }

            // Pre-populate CustomerId in the model for the view
            return View(new Vehicle { CustomerId = customer.CustomerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Vehicle vehicle)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Customer");
            }

            var customer = await _customerService.GetCustomerByIdAsync(currentUser.Id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Could not find your customer profile. Please try again.";
                return RedirectToAction("UpdateProfile", "Customer");
            }

            // SECURITY: Crucial step - ensure the vehicle's CustomerId matches the logged-in user's CustomerId
            // This prevents a malicious user from tampering with the form to assign a vehicle to another customer.
            vehicle.CustomerId = customer.CustomerId;

            if (ModelState.IsValid)
            {
                try
                {
                    _vehicleService.AddVehicle(vehicle);
                    TempData["SuccessMessage"] = "Vehicle added successfully!"; // Use TempData for messages across redirects
                    return RedirectToAction("ViewVehicles"); // Redirect to the list of vehicles for the current customer
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while adding the vehicle. Please try again.");
                }
            }

            // If ModelState is not valid or an error occurred, return the view with the current model
            return View(vehicle);
        }

        [HttpGet]
        // Renamed from "View" to "ViewVehicles" for better clarity on action purpose
        public async Task<IActionResult> ViewVehicles()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Customer");
            }

            var customer = await _customerService.GetCustomerByIdAsync(currentUser.Id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Could not find your customer profile. Please complete it.";
                return RedirectToAction("UpdateProfile", "Customer");
            }

            // Get vehicles specifically for the logged-in customer
            var vehicles = _vehicleService.GetVehicleByCustomerId(customer.CustomerId);
            return View(vehicles);
        }

        [HttpGet]
       
        public IActionResult Success()
        {
            return View();
        }

        [HttpPost, ActionName("Delete")] // Use ActionName to keep the route cleaner if desired
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int vehicleId) // Renamed for clarity that it's the confirmed delete
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Customer");
            }

            var customer = await _customerService.GetCustomerByIdAsync(currentUser.Id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Could not find your customer profile. Please complete it.";
                return RedirectToAction("UpdateProfile", "Customer");
            }

            try
            {
                var vehicleToDelete = _vehicleService.GetVehicleById(vehicleId);

                if (vehicleToDelete == null)
                {
                    TempData["ErrorMessage"] = "Vehicle not found for deletion.";
                    return NotFound();
                }

                // SECURITY: Crucial - ensure the vehicle being deleted belongs to the logged-in user
                if (vehicleToDelete.CustomerId != customer.CustomerId)
                {
                    TempData["ErrorMessage"] = "You are not authorized to delete this vehicle.";
                    return Forbid();
                }

                _vehicleService.DeleteVehicle(vehicleId);
                TempData["SuccessMessage"] = "Vehicle successfully deleted!";
                return RedirectToAction("ViewVehicles");
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "This vehicle cannot be deleted as it has an active policy.";
            }

            // If there was an error, redirect back to the view vehicles page
            return RedirectToAction("ViewVehicles");
        }

        
       
    }
}