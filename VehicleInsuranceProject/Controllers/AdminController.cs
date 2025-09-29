using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleInsuranceProject.BusinessLogic.Services;
using VehicleInsuranceProject.Repository.Models;
using VehicleInsuranceProject.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace VehicleInsuranceProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICustomerService _customerService;
        private readonly IPolicyService _policyService;
        private readonly IVehicleService _vehicleService;
        private readonly ICoverageLevelService _coverageLevelService;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ICustomerService customerService,
            IPolicyService policyService,
            IVehicleService vehicleService,
            ICoverageLevelService coverageLevelService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _customerService = customerService;
            _policyService = policyService;
            _vehicleService = vehicleService;
            _coverageLevelService = coverageLevelService;
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Customers(string searchTerm)
        {
            ViewData["Title"] = "Manage Customers";
            IEnumerable<Customer> customers;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                ViewBag.SearchTerm = searchTerm;
                customers = await _customerService.SearchCustomersAsync(searchTerm);
            }
            else
            {
                customers = await _customerService.GetAllCustomersForAdminAsync();
            }

            return View("Customer/Index", customers);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerDetails(int id)
        {
            var customer = await _customerService.GetCustomerByCustomerIdAsync(id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return NotFound();
            }
            ViewData["Title"] = "Customer Details";
            return View("Customer/Details", customer);
        }

        [HttpGet]
        public async Task<IActionResult> EditCustomer(int id)
        {
            var customer = await _customerService.GetCustomerByCustomerIdAsync(id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found for editing.";
                return NotFound();
            }
            ViewData["Title"] = "Edit Customer Profile";
            return View("Customer/Edit", customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(Customer customer)
        {
            var existingCustomer = await _customerService.GetCustomerByCustomerIdAsync(customer.CustomerId);
            if (existingCustomer == null)
            {
                TempData["ErrorMessage"] = "Customer not found for update.";
                return NotFound();
            }
            customer.IsActive = existingCustomer.IsActive;

            if (ModelState.IsValid)
            {
                try
                {
                    await _customerService.UpdateCustomerAsync(customer);
                    TempData["SuccessMessage"] = "Customer profile updated successfully!";
                    return RedirectToAction(nameof(Customers));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (KeyNotFoundException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return NotFound();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                }
            }
            ViewData["Title"] = "Edit Customer Profile";
            return View("Customer/Edit", customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateCustomer(int id)
        {
            try
            {
                await _customerService.DeactivateCustomerAsync(id);
                TempData["SuccessMessage"] = "Customer account deactivated successfully!";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deactivating customer: {ex.Message}";
            }
            return RedirectToAction(nameof(Customers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateCustomer(int id)
        {
            try
            {
                await _customerService.ActivateCustomerAsync(id);
                TempData["SuccessMessage"] = "Customer account activated successfully!";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while activating customer: {ex.Message}";
            }
            return RedirectToAction(nameof(Customers));
        }

        // --- POLICY MANAGEMENT ACTIONS ---

        [HttpGet]
        public async Task<IActionResult> ViewPolicies(string searchString)
        {
            var allPolicies = await _policyService.GetAllPoliciesAsync();

            var policiesViewModel = allPolicies.Select(p => new AdminPolicyViewModel
            {
                PolicyId = p.policyId,
                PolicyNumber = p.policyNumber,
                CustomerName = p.Vehicle?.Customer?.Name ?? "N/A",
                VehicleRegistrationNumber = p.Vehicle?.registrationNumber ?? "N/A",
                VehicleType = p.Vehicle?.vehicleType ?? VehicleType.CAR,
                CoverageLevelName = p.CoverageLevel?.Name ?? "N/A",
                CoverageAmount = p.coverageAmount,
                PremiumAmount = p.premiumAmount,
                StartDate = p.startDate,
                EndDate = p.endDate,
                PolicyStatus = p.policyStatus
            }).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                policiesViewModel = policiesViewModel.Where(p =>
                    p.PolicyNumber.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.CustomerName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.VehicleRegistrationNumber.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.CurrentSearchString = searchString;

            return View("Customer/ViewPolicies",policiesViewModel.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivatePolicy(int policyId)
        {
            try
            {
                await _policyService.DeactivatePolicyAsync(policyId);
                TempData["SuccessMessage"] = "Policy successfully deactivated!";
                return RedirectToAction(nameof(ViewPolicies));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deactivating policy: {ex.Message}";
                return RedirectToAction(nameof(ViewPolicies));
            }
        }

        // GET: Admin/EditPolicy/{policyId} - Fetches policy for editing
        [HttpGet]
        public async Task<IActionResult> EditPolicy(int policyId)
        {
            var policy = await _policyService.GetPolicyWithVehicleAndCustomerAsync(policyId);
            if (policy == null)
            {
                TempData["ErrorMessage"] = "Policy not found for editing.";
                return NotFound();
            }

            // Ensure related objects are available for the view if needed (e.g., for dropdowns)
            int customerId = policy.Vehicle?.CustomerId ?? 0;
            var customerVehicles = _vehicleService.GetVehicleByCustomerId(customerId); // Used if vehicle selection was allowed
            ViewBag.Vehicles = new SelectList(customerVehicles, "vehicleId", "registrationNumber", policy.vehicleId);

            var coverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
            ViewBag.CoverageLevels = new SelectList(coverageLevels, "CoverageLevelId", "Name", policy.CoverageLevelId);

            return View("Customer/EditPolicies",policy); // Renders Views/Admin/EditPolicy.cshtml
        }

        // POST: Admin/EditPolicy - Handles submission of the edited policy form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPolicy(Policy policy, int CoverageLevelId) // Match parameter name to property for model binding
        {
            // Fetch the original policy from the database to retain unedited properties
            var existingPolicy = await _policyService.GetPolicyWithVehicleAndCustomerAsync(policy.policyId);
            if (existingPolicy == null)
            {
                TempData["ErrorMessage"] = "Policy not found for update.";
                return NotFound();
            }

            // Manually update properties that are allowed to be edited by Admin
            // PolicyNumber is generated and should not change
            // VehicleId should not change after policy creation
            existingPolicy.startDate = policy.startDate;
            existingPolicy.endDate = policy.endDate;
            existingPolicy.policyStatus = policy.policyStatus; // Admin can change status

            // Validate and set CoverageLevelId
            var selectedCoverageLevel = await _coverageLevelService.GetCoverageLevelByIdAsync(CoverageLevelId);
            if (selectedCoverageLevel == null)
            {
                ModelState.AddModelError("CoverageLevelId", "Selected coverage level not found.");
            }
            else
            {
                existingPolicy.CoverageLevelId = CoverageLevelId;
                // Recalculate premium and coverage based on the new coverage level
                var (calculatedPremium, calculatedCoverage) = _policyService.CalculatePremiumAndCoverage(existingPolicy.Vehicle!, selectedCoverageLevel);
                existingPolicy.premiumAmount = calculatedPremium;
                existingPolicy.coverageAmount = calculatedCoverage;
            }

            // Validate other model properties
            // We removed premiumAmount, coverageAmount, policyNumber from ModelState earlier
            // Need to ensure the updated existingPolicy is validated
            // You might need to manually trigger validation if you only update specific properties
            // or fetch the model from DB, update, and then re-validate.
            // For simplicity, we'll rely on the client-side validation for basic checks
            // and assume _policyService.UpdatePolicy handles server-side validation.

            if (ModelState.IsValid) // Check validity based on properties received and what was manually set
            {
                try
                {
                    _policyService.UpdatePolicy(existingPolicy); // Update the fetched entity
                    TempData["SuccessMessage"] = "Policy updated successfully!";
                    return RedirectToAction(nameof(ViewPolicies));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the policy. Error: " + ex.Message);
                    Console.WriteLine($"Error during Admin policy edit: {ex.Message}"); // Log for debugging
                }
            }

            // If ModelState is invalid or an error occurred, re-populate ViewBag and return view
            // Make sure to use the 'policy' object passed to the view, not 'existingPolicy'
            // as 'policy' holds the user's invalid input
            int customerIdForVehicles = existingPolicy.Vehicle?.CustomerId ?? 0;
            ViewBag.Vehicles = new SelectList(_vehicleService.GetVehicleByCustomerId(customerIdForVehicles), "vehicleId", "registrationNumber", policy.vehicleId);
            var allCoverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
            ViewBag.CoverageLevels = new SelectList(allCoverageLevels, "CoverageLevelId", "Name", CoverageLevelId); // Use CoverageLevelId from form
            return View("Customer/EditPolicies",policy); // Return the incoming model to preserve user input on error
        }

        [HttpGet]
        public async Task<IActionResult> RenewPolicy(int policyId)
        {
            var policy = await _policyService.GetPolicyWithVehicleAndCustomerAsync(policyId);
            if (policy == null)
            {
                TempData["ErrorMessage"] = "Policy not found for renewal.";
                return NotFound();
            }

            ViewBag.OriginalEndDate = policy.endDate;
            policy.startDate = DateTime.Today;
            policy.endDate = DateTime.Today.AddYears(1);

            ViewBag.VehicleRegistrationNumber = policy.Vehicle?.registrationNumber;

            var coverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
            ViewBag.CoverageLevels = new SelectList(coverageLevels, "CoverageLevelId", "Name", policy.CoverageLevelId);

            return View("Customer/Renew", policy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewPolicy(Policy policy, int coverageLevelId)
        {
            var existingPolicy = await _policyService.GetPolicyWithVehicleAndCustomerAsync(policy.policyId);
            if (existingPolicy == null)
            {
                TempData["ErrorMessage"] = "Policy not found for renewal.";
                return NotFound();
            }

            var selectedCoverageLevel = await _coverageLevelService.GetCoverageLevelByIdAsync(coverageLevelId);
            if (selectedCoverageLevel == null)
            {
                ModelState.AddModelError("CoverageLevelId", "Selected coverage level not found.");
                ViewBag.OriginalEndDate = existingPolicy.endDate;
                ViewBag.VehicleRegistrationNumber = existingPolicy.Vehicle?.registrationNumber;
                ViewBag.CoverageLevels = new SelectList(await _coverageLevelService.GetAllCoverageLevelsAsync(), "CoverageLevelId", "Name", coverageLevelId);
                return View("Customer/Renew", policy);
            }

            policy.CoverageLevelId = coverageLevelId;

            ModelState.Remove(nameof(policy.premiumAmount));
            ModelState.Remove(nameof(policy.coverageAmount));
            ModelState.Remove(nameof(policy.policyNumber));

            if (ModelState.IsValid)
            {
                try
                {
                    var (calculatedPremium, calculatedCoverage) = _policyService.CalculatePremiumAndCoverage(existingPolicy.Vehicle!, selectedCoverageLevel);

                    _policyService.RenewPolicy(policy.policyId, policy.endDate, calculatedPremium, calculatedCoverage);
                    TempData["SuccessMessage"] = "Policy renewed successfully!";
                    return RedirectToAction(nameof(ViewPolicies));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred during policy renewal. Error: " + ex.Message);
                    Console.WriteLine($"Error during Admin policy renewal: {ex.Message}");
                }
            }

            ViewBag.OriginalEndDate = existingPolicy.endDate;
            ViewBag.VehicleRegistrationNumber = existingPolicy.Vehicle?.registrationNumber;
            ViewBag.CoverageLevels = new SelectList(await _coverageLevelService.GetAllCoverageLevelsAsync(), "CoverageLevelId", "Name", coverageLevelId);
            return View("Customer/Renew", policy);
        }

        [HttpGet]
        public async Task<IActionResult> CalculateRenewalPolicyValues(int? policyId, int? coverageLevelId)
        {
            try
            {
                if (policyId == null || coverageLevelId == null || policyId <= 0 || coverageLevelId <= 0)
                {
                    return Json(new { success = false, message = "Invalid Policy ID or Coverage Level ID provided." });
                }

                var existingPolicy = await _policyService.GetPolicyWithVehicleAndCustomerAsync(policyId.Value);
                if (existingPolicy == null || existingPolicy.Vehicle == null)
                {
                    return Json(new { success = false, message = "Policy or associated vehicle not found." });
                }

                var coverageLevel = await _coverageLevelService.GetCoverageLevelByIdAsync(coverageLevelId.Value);
                if (coverageLevel == null)
                {
                    return Json(new { success = false, message = "Coverage level not found." });
                }

                var (premium, coverage) = _policyService.CalculatePremiumAndCoverage(existingPolicy.Vehicle, coverageLevel);
                return Json(new { success = true, premium = premium, coverage = coverage });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Admin CalculateRenewalPolicyValues: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> PolicyDetails(int policyId)
        {
            var policy = await _policyService.GetPolicyWithVehicleAndCustomerAsync(policyId);
            if (policy == null)
            {
                TempData["ErrorMessage"] = "Policy not found.";
                return NotFound();
            }
            return View("Customer/PolicyDetails", policy);
        }
        //Vehicle admin
        public async Task<IActionResult> Edit(int vehicleId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Customer");
            }

            var customer = await _customerService.GetCustomerByIdAsync(currentUser.Id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Could not find your customer profile.";
                return RedirectToAction("UpdateProfile", "Customer");
            }

            var vehicle = _vehicleService.GetVehicleById(vehicleId);
            if (vehicle == null)
            {
                TempData["ErrorMessage"] = "Vehicle not found.";
                return NotFound(); // HTTP 404
            }

            // SECURITY: Ensure the vehicle belongs to the logged-in customer
            if (vehicle.CustomerId != customer.CustomerId)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit this vehicle.";
                return Forbid(); // HTTP 403 - Forbidden
            }

            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Vehicle vehicle)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Customer");
            }

            var customer = await _customerService.GetCustomerByIdAsync(currentUser.Id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Could not find your customer profile.";
                return RedirectToAction("UpdateProfile", "Customer");
            }

            // Retrieve the existing vehicle to verify ownership before updating
            var existingVehicle = _vehicleService.GetVehicleById(vehicle.vehicleId);
            if (existingVehicle == null)
            {
                TempData["ErrorMessage"] = "Vehicle not found for update.";
                return NotFound();
            }

            // SECURITY: Crucial step - ensure the vehicle being updated belongs to the logged-in user
            if (existingVehicle.CustomerId != customer.CustomerId)
            {
                TempData["ErrorMessage"] = "You are not authorized to edit this vehicle.";
                return Forbid();
            }

            // SECURITY: Force the CustomerId to the original existing vehicle's CustomerId
            // This prevents a malicious user from changing the CustomerId via the form.
            vehicle.CustomerId = existingVehicle.CustomerId;

            if (ModelState.IsValid)
            {
                try
                {
                    _vehicleService.UpdateVehicle(vehicle);
                    TempData["SuccessMessage"] = "Vehicle updated successfully!";
                    return RedirectToAction("ViewVehicles"); // Redirect to the list of vehicles
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the vehicle. Please try again.");
                }
            }
            // If ModelState is not valid or an error occurred, return the view with the current model
            return View(vehicle);
        }
        [HttpGet]
        public async Task<IActionResult> CoverageLevels()
        {
            ViewData["Title"] = "Manage Coverage Levels";
            var coverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
            return View("Policy/CoverageLevels", coverageLevels); // Renders Views/Admin/CoverageLevels.cshtml
        }
        [HttpGet]
        public IActionResult CreateCoverageLevel()
        {
            ViewData["Title"] = "Add New Coverage Level";
            return View("Policy/CreateCoverageLevel"); // Renders Views/Admin/CreateCoverageLevel.cshtml
        }

        // NEW: POST action for adding a new Coverage Level
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoverageLevel(CoverageLevel coverageLevel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _coverageLevelService.AddCoverageLevelAsync(coverageLevel);
                    TempData["SuccessMessage"] = "Coverage Level added successfully!";
                    return RedirectToAction(nameof(CoverageLevels)); // Redirect to the list of coverage levels
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while adding the coverage level. Error: " + ex.Message);
                    Console.WriteLine($"Error during Admin Add Coverage Level: {ex.Message}"); // Log for debugging
                }
            }
            ViewData["Title"] = "Add New Coverage Level";
            return View("Policy/CreateCoverageLevel", coverageLevel); // Return the view with errors if model state is invalid
        }
        [HttpGet]
        public async Task<IActionResult> CoverageLevelDetails(int id)
        {
            ViewData["Title"] = "Coverage Level Details";
            var coverageLevel = await _coverageLevelService.GetCoverageLevelByIdAsync(id);

            if (coverageLevel == null)
            {
                TempData["ErrorMessage"] = "Coverage Level not found.";
                return NotFound();
            }

            return View("Policy/CoverageLevelDetails", coverageLevel);
        }
        // NEW: GET action for editing an existing Coverage Level
        [HttpGet]
        public async Task<IActionResult> EditCoverageLevel(int id)
        {
            ViewData["Title"] = "Edit Coverage Level";
            var coverageLevel = await _coverageLevelService.GetCoverageLevelByIdAsync(id);

            if (coverageLevel == null)
            {
                TempData["ErrorMessage"] = "Coverage Level not found for editing.";
                return NotFound();
            }

            return View("Policy/EditCoverageLevel", coverageLevel);
        }

        // NEW: POST action for submitting edits to a Coverage Level
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCoverageLevel(CoverageLevel coverageLevel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _coverageLevelService.UpdateCoverageLevelAsync(coverageLevel);
                    TempData["SuccessMessage"] = "Coverage Level updated successfully!";
                    return RedirectToAction(nameof(CoverageLevels)); // Redirect to the list view
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (KeyNotFoundException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return NotFound();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the coverage level. Error: " + ex.Message);
                    Console.WriteLine($"Error during Admin Edit Coverage Level: {ex.Message}"); // Log for debugging
                }
            }
            ViewData["Title"] = "Edit Coverage Level";
            return View("Policy/EditCoverageLevel", coverageLevel); // Return view with errors
        }
    }
}
