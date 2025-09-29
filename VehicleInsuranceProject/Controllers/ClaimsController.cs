// VehicleInsuranceProject.Web.Controllers/ClaimsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleInsuranceProject.BusinessLogic.Services;
using VehicleInsuranceProject.Repository.Models;
using VehicleInsuranceProject.Web.ViewModels;
using System.Security.Claims; // For User.FindFirstValue
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleInsuranceProject.Web.Controllers
{
    [Authorize] // Ensure only authenticated users can access
    public class ClaimsController : Controller
    {
        private readonly IPolicyService _policyService;
        private readonly IClaimService _claimService;
        private readonly ICustomerService _customerService;
        private readonly IWebHostEnvironment _webHostEnvironment; // For image uploads

        public ClaimsController(IPolicyService policyService,
                                IClaimService claimService,
                                ICustomerService customerService,
                                IWebHostEnvironment webHostEnvironment)
        {
            _policyService = policyService;
            _claimService = claimService;
            _customerService = customerService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Claims/FileClaim/{policyId}
        [HttpGet]
        public async Task<IActionResult> FileClaim(int policyId)
        {
            if (policyId <= 0)
            {
                return BadRequest("Invalid policy ID.");
            }

            var policy = await _policyService.GetPolicyWithVehicleAndCustomerAsync(policyId);

            if (policy == null)
            {
                TempData["ErrorMessage"] = "Policy not found.";
                return RedirectToAction("ViewPolicies", "Customer"); // Or relevant customer policies page
            }

            // Ensure the policy belongs to the logged-in customer
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByIdAsync(userId!);

            if (customer == null || policy.Vehicle?.CustomerId != customer.CustomerId)
            {
                TempData["ErrorMessage"] = "You are not authorized to file a claim for this policy.";
                return RedirectToAction("ViewPolicies", "Customer");
            }

            var model = new ClaimViewModel
            {
                PolicyId = policy.policyId,
                PolicyNumber = policy.policyNumber,
                PolicyCoverageAmount = policy.coverageAmount
            };

            return View(model);
        }

        // POST: Claims/FileClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FileClaim(ClaimViewModel model)
        {
            // IMPORTANT: Remove PolicyCoverageAmount from ModelState validation as it's for display/comparison, not user input.
            ModelState.Remove(nameof(model.PolicyCoverageAmount));

            if (ModelState.IsValid)
            {
                var policy = await _policyService.GetPolicyWithVehicleAndCustomerAsync(model.PolicyId);
                if (policy == null)
                {
                    ModelState.AddModelError(string.Empty, "Policy not found.");
                    return View(model);
                }

                // Re-validate claim amount against policy coverage in case of client-side bypass
                if (model.ClaimAmount > policy.coverageAmount)
                {
                    ModelState.AddModelError(nameof(model.ClaimAmount), $"Claim amount cannot exceed policy coverage of ${policy.coverageAmount:N2}.");
                    return View(model);
                }

                // Validate policy status (e.g., only active policies can file claims)
                if (policy.policyStatus != PolicyStatus.ACTIVE)
                {
                    ModelState.AddModelError(string.Empty, $"Claims can only be filed for active policies. Current policy status: {policy.policyStatus}");
                    return View(model);
                }

                try
                {
                    var claim = new Repository.Models.Claim
                    {
                        policyId = model.PolicyId,
                        claimAmount = model.ClaimAmount,
                        claimReason = model.ClaimReason,
                        claimDate = DateTime.Now, // Set by service, but good to have here too
                        claimStatus = ClaimStatus.SUBMITTED // Set by service, but good to have here too
                    };

                    await _claimService.FileClaimAsync(claim, model.ClaimImages ?? new List<IFormFile>(), _webHostEnvironment.WebRootPath);

                    TempData["SuccessMessage"] = "Claim filed successfully and is under review!";
                    return RedirectToAction("CustomerClaims"); // Redirect to a page showing customer's claims
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    // Log the exception
                    ModelState.AddModelError(string.Empty, "An error occurred while filing the claim. Please try again.");
                }
            }

            // If ModelState is not valid or an error occurred, return the view with the model
            return View(model);
        }

        // GET: Claims/CustomerClaims (To view all claims by the logged-in customer)
        [HttpGet]
        public async Task<IActionResult> CustomerClaims()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

            var customer = await _customerService.GetCustomerByIdAsync(userId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer profile not found.";
                return RedirectToAction("Index", "Home");
            }

            var claims = await _claimService.GetClaimsByCustomerIdAsync(customer.CustomerId);
            return View(claims);
        }

        // GET: Claims/Details/{id} (To view details of a specific claim, including images)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction("CustomerClaims");
            }

            // Ensure the claim belongs to the logged-in customer for authorization
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByIdAsync(userId!);
            if (customer == null || claim.Policy?.Vehicle?.CustomerId != customer.CustomerId)
            {
                // This claim does not belong to the current user, or user/customer not found.
                // Could be an admin viewing, but for customer dashboard, redirect.
                if (!User.IsInRole("Admin")) // Allow admin to view any claim
                {
                    TempData["ErrorMessage"] = "You are not authorized to view this claim.";
                    return RedirectToAction("CustomerClaims");
                }
            }

            return View(claim);
        }

        // GET: Claims/AllClaims (For Admin to view all claims)
        [Authorize(Roles = "Admin")] // Only Admins can access this action
        [HttpGet]
        public async Task<IActionResult> AllClaims()
        {
            var claims = await _claimService.GetAllClaimsAsync();
            return View("~/Views/Admin/Customer/AllClaims.cshtml", claims);
        }

        // POST: Claims/UpdateClaimStatus (For Admin to update claim status)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClaimStatus(int claimId, ClaimStatus newStatus)
        {
            try
            {
                await _claimService.UpdateClaimStatusAsync(claimId, newStatus);
                TempData["SuccessMessage"] = $"Claim ID {claimId} status updated to {newStatus}.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating claim status.";
                // Log the exception
            }
            return RedirectToAction("AllClaims");
        }

    }
}