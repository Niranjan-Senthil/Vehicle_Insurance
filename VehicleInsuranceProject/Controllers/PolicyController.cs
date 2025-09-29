using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleInsuranceProject.BusinessLogic.Services;
using VehicleInsuranceProject.Repository.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VehicleInsuranceProject.Controllers
{
    [Authorize(Roles = "Customer")]
    public class PolicyController : Controller
    {
        private readonly IPolicyService _policyService;
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerService _customerService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICoverageLevelService _coverageLevelService;

        public PolicyController(
            IPolicyService policyService,
            IVehicleService vehicleService,
            ICustomerService customerService,
            UserManager<IdentityUser> userManager,
            ICoverageLevelService coverageLevelService)
        {
            _policyService = policyService;
            _vehicleService = vehicleService;
            _customerService = customerService;
            _userManager = userManager;
            _coverageLevelService = coverageLevelService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewPolicies()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if user not found
            }

            var customer = await _customerService.GetCustomerByIdAsync(currentUser.Id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Could not find your customer profile. Please complete it.";
                return RedirectToAction("CompleteProfile", "Customer"); // Redirect to complete profile
            }

            // Fetch policies including their associated claims for the button logic in the view
            var policies = await _policyService.GetPoliciesByCustomerIdWithClaimsAsync(customer.CustomerId);

            // Logic to update policy status to EXPIRED if end date has passed
            foreach (var policy in policies)
            {
                if (policy.endDate < DateTime.Today && policy.policyStatus == PolicyStatus.ACTIVE)
                {
                    policy.policyStatus = PolicyStatus.EXPIRED;
                    try
                    {
                        // Update the policy status in the database
                        _policyService.UpdatePolicy(policy);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating policy status for policy ID {policy.policyId}: {ex.Message}");
                        // Log this error properly in a real application
                    }
                }
            }

            return View(policies);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
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

            var customerVehicles = _vehicleService.GetVehicleByCustomerId(customer.CustomerId);
            if (!customerVehicles.Any())
            {
                TempData["InfoMessage"] = "You need to add a vehicle before creating a policy.";
                return RedirectToAction("Add", "Vehicle");
            }

            ViewBag.Vehicles = new SelectList(customerVehicles, "vehicleId", "registrationNumber");

            var coverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
            ViewBag.CoverageLevels = new SelectList(coverageLevels, "CoverageLevelId", "Name");

            var newPolicy = new Policy
            {
                startDate = DateTime.Today,
                endDate = DateTime.Today.AddYears(1),
                policyStatus = PolicyStatus.ACTIVE,
                premiumAmount = 0.00m,
                coverageAmount = 0.00m
            };
            return View(newPolicy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Policy policy, int coverageLevelId)
        {
            policy.policyNumber = _policyService.GenerateNewPolicyNumber();
            ModelState.Remove(nameof(policy.policyNumber));

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

            var selectedVehicle = _vehicleService.GetVehicleById(policy.vehicleId);
            if (selectedVehicle == null || selectedVehicle.CustomerId != customer.CustomerId)
            {
                TempData["ErrorMessage"] = "Invalid vehicle selected or vehicle does not belong to you.";
                ViewBag.Vehicles = new SelectList(_vehicleService.GetVehicleByCustomerId(customer.CustomerId), "vehicleId", "registrationNumber", policy.vehicleId);
                var allCoverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
                ViewBag.CoverageLevels = new SelectList(allCoverageLevels, "CoverageLevelId", "Name", coverageLevelId);
                return View(policy);
            }

            var selectedCoverageLevel = await _coverageLevelService.GetCoverageLevelByIdAsync(coverageLevelId);
            if (selectedCoverageLevel == null)
            {
                ModelState.AddModelError("CoverageLevelId", "Selected coverage level not found.");
                ViewBag.Vehicles = new SelectList(_vehicleService.GetVehicleByCustomerId(customer.CustomerId), "vehicleId", "registrationNumber", policy.vehicleId);
                var allCoverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
                ViewBag.CoverageLevels = new SelectList(allCoverageLevels, "CoverageLevelId", "Name", coverageLevelId);
                return View(policy);
            }

            policy.CoverageLevelId = coverageLevelId;

            try
            {
                var (calculatedPremium, calculatedCoverage) = _policyService.CalculatePremiumAndCoverage(selectedVehicle, selectedCoverageLevel);
                policy.premiumAmount = calculatedPremium;
                policy.coverageAmount = calculatedCoverage;

                ModelState.Remove(nameof(policy.premiumAmount));
                ModelState.Remove(nameof(policy.coverageAmount));

            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Vehicles = new SelectList(_vehicleService.GetVehicleByCustomerId(customer.CustomerId), "vehicleId", "registrationNumber");
                var allCoverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
                ViewBag.CoverageLevels = new SelectList(allCoverageLevels, "CoverageLevelId", "Name", coverageLevelId);
                return View(policy);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _policyService.CreatePolicy(policy);
                    TempData["SuccessMessage"] = "Policy created successfully!";
                    return RedirectToAction("ViewPolicies");
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the policy. Please try again. Error: " + ex.Message);
                    Console.WriteLine($"Model Error: {ex.Message}");
                }
            }
            else
            {
                foreach (var modelStateEntry in ModelState.Values)
                {
                    foreach (var error in modelStateEntry.Errors)
                    {
                        Console.WriteLine($"Model Error: {error.ErrorMessage}");
                    }
                }
            }

            ViewBag.Vehicles = new SelectList(_vehicleService.GetVehicleByCustomerId(customer.CustomerId), "vehicleId", "registrationNumber", policy.vehicleId);
            var reloadedCoverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
            ViewBag.CoverageLevels = new SelectList(reloadedCoverageLevels, "CoverageLevelId", "Name", coverageLevelId);
            return View(policy);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int policyId)
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

            var policy = _policyService.GetPolicyById(policyId);
            if (policy == null)
            {
                TempData["ErrorMessage"] = "Policy not found.";
                return NotFound();
            }

            if (policy.Vehicle == null || policy.Vehicle.CustomerId != customer.CustomerId)
            {
                TempData["ErrorMessage"] = "You are not authorized to view this policy.";
                return Forbid();
            }

            return View(policy);
        }

        [HttpGet]
        public async Task<IActionResult> Renew(int policyId)
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

            var policy = _policyService.GetPolicyById(policyId);
            if (policy == null)
            {
                TempData["ErrorMessage"] = "Policy not found.";
                return NotFound();
            }

            if (policy.Vehicle == null || policy.Vehicle.CustomerId != customer.CustomerId)
            {
                TempData["ErrorMessage"] = "You are not authorized to renew this policy.";
                return Forbid();
            }
            ViewBag.OriginalEndDate = policy.endDate;

            policy.startDate = DateTime.Today;
            policy.endDate = DateTime.Today.AddYears(1);

            ViewBag.VehicleRegistrationNumber = policy.Vehicle?.registrationNumber;

            var coverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
            ViewBag.CoverageLevels = new SelectList(coverageLevels, "CoverageLevelId", "Name", policy.CoverageLevelId);

            return View(policy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(Policy policy, int coverageLevelId)
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

            var existingPolicy = _policyService.GetPolicyById(policy.policyId);
            if (existingPolicy == null)
            {
                TempData["ErrorMessage"] = "Policy not found for renewal.";
                return NotFound();
            }

            if (existingPolicy.Vehicle == null || existingPolicy.Vehicle.CustomerId != customer.CustomerId)
            {
                TempData["ErrorMessage"] = "You are not authorized to renew this policy.";
                return Forbid();
            }

            var selectedCoverageLevel = await _coverageLevelService.GetCoverageLevelByIdAsync(coverageLevelId);
            if (selectedCoverageLevel == null)
            {
                ModelState.AddModelError("CoverageLevelId", "Selected coverage level not found.");
                ViewBag.VehicleRegistrationNumber = existingPolicy.Vehicle?.registrationNumber;
                var allCoverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
                ViewBag.CoverageLevels = new SelectList(allCoverageLevels, "CoverageLevelId", "Name", coverageLevelId);
                return View(policy);
            }

            policy.CoverageLevelId = coverageLevelId;

            try
            {
                var (calculatedPremium, calculatedCoverage) = _policyService.CalculatePremiumAndCoverage(existingPolicy.Vehicle, selectedCoverageLevel);
                policy.premiumAmount = calculatedPremium;
                policy.coverageAmount = calculatedCoverage;

                ModelState.Remove(nameof(policy.premiumAmount));
                ModelState.Remove(nameof(policy.coverageAmount));

            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.VehicleRegistrationNumber = existingPolicy.Vehicle?.registrationNumber;
                var allCoverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
                ViewBag.CoverageLevels = new SelectList(allCoverageLevels, "CoverageLevelId", "Name", coverageLevelId);
                return View(policy);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _policyService.RenewPolicy(policy.policyId, policy.endDate, policy.premiumAmount, policy.coverageAmount);
                    TempData["SuccessMessage"] = "Policy renewed successfully!";
                    return RedirectToAction("ViewPolicies");
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred during policy renewal. Please try again. Error: " + ex.Message);
                    Console.WriteLine($"Model Error: {ex.Message}");
                }
            }
            ViewBag.VehicleRegistrationNumber = existingPolicy.Vehicle?.registrationNumber;
            var reloadedCoverageLevels = await _coverageLevelService.GetAllCoverageLevelsAsync();
            ViewBag.CoverageLevels = new SelectList(reloadedCoverageLevels, "CoverageLevelId", "Name", coverageLevelId);
            return View(policy);
        }

        // NEW AJAX endpoint for client-side calculation
        [HttpGet]
        public async Task<IActionResult> CalculatePolicyValues(int? vehicleId, int? coverageLevelId) // Changed to nullable ints
        {
            try
            {
                if (vehicleId == null || coverageLevelId == null || vehicleId <= 0 || coverageLevelId <= 0)
                {
                    return Json(new { success = false, message = "Invalid Vehicle ID or Coverage Level ID provided." });
                }

                var vehicle = _vehicleService.GetVehicleById(vehicleId.Value); // Use .Value for non-nullable int
                if (vehicle == null)
                {
                    return Json(new { success = false, message = "Vehicle not found." });
                }

                var coverageLevel = await _coverageLevelService.GetCoverageLevelByIdAsync(coverageLevelId.Value); // Use .Value
                if (coverageLevel == null)
                {
                    return Json(new { success = false, message = "Coverage level not found." });
                }

                var (premium, coverage) = _policyService.CalculatePremiumAndCoverage(vehicle, coverageLevel);
                return Json(new { success = true, premium = premium, coverage = coverage });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculatePolicyValues: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // NEW AJAX endpoint for client-side renewal calculation (similar to above but for existing policy context)
        [HttpGet]
        public async Task<IActionResult> CalculateRenewalPolicyValues(int? policyId, int? coverageLevelId) // Changed to nullable ints
        {
            try
            {
                if (policyId == null || coverageLevelId == null || policyId <= 0 || coverageLevelId <= 0)
                {
                    return Json(new { success = false, message = "Invalid Policy ID or Coverage Level ID provided." });
                }

                var existingPolicy = _policyService.GetPolicyById(policyId.Value);
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
                Console.WriteLine($"Error in CalculateRenewalPolicyValues: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Policy/Delete - Handles the deletion of a policy
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int policyId)
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

            try
            {
                var policyToDelete = _policyService.GetPolicyById(policyId);
                if (policyToDelete == null)
                {
                    TempData["ErrorMessage"] = "Policy not found for deletion.";
                    return NotFound();
                }

                if (policyToDelete.Vehicle == null || policyToDelete.Vehicle.CustomerId != customer.CustomerId)
                {
                    TempData["ErrorMessage"] = "You are not authorized to delete this policy.";
                    return Forbid();
                }

                _policyService.DeletePolicy(policyId);
                TempData["SuccessMessage"] = "Policy successfully deleted!";
                return RedirectToAction("ViewPolicies");
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the policy. Please try again.";
            }

            return RedirectToAction("ViewPolicies");
        }
    }
}
