using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleInsuranceProject.BusinessLogic.Services;
using VehicleInsuranceProject.ViewModels;

namespace VehicleInsuranceProject.Controllers
{
    [Authorize(Roles = "Customer")] // Only customers can access these reports
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICustomerService _customerService;

        public ReportController(IReportService reportService, UserManager<IdentityUser> userManager, ICustomerService customerService)
        {
            _reportService = reportService;
            _userManager = userManager;
            _customerService = customerService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "My Reports Dashboard";
            return View(); // Just return the view, no model needed for this dashboard
        }


        [HttpGet]
        public async Task<IActionResult> MyPolicies()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login", "Customer");
            }

            var customer = await _customerService.GetCustomerByIdAsync(currentUser.Id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer profile not found. Please complete your profile.";
                return RedirectToAction("UpdateProfile", "Customer");
            }

            ViewData["Title"] = "My Policy Report";
            // Get DTOs from service
            var dtoList = _reportService.GetCustomerPolicyReport(customer.CustomerId);

            // Map DTOs to ViewModels
            var viewModelList = dtoList.Select(dto => new CustomerPolicyReportViewModel
            {
                PolicyId = dto.PolicyId,
                PolicyNumber = dto.PolicyNumber,
                VehicleRegistrationNumber = dto.VehicleRegistrationNumber,
                VehicleMake = dto.VehicleMake,
                VehicleModel = dto.VehicleModel,
                VehicleYear = dto.VehicleYear,
                VehicleType = dto.VehicleType,
                CoverageAmount = dto.CoverageAmount,
                PremiumAmount = dto.PremiumAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                PolicyStatus = dto.PolicyStatus
            }).ToList();

            return View(viewModelList);
        }

        [HttpGet]
        public async Task<IActionResult> MyClaims()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Login", "Customer");
            }

            var customer = await _customerService.GetCustomerByIdAsync(currentUser.Id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer profile not found. Please complete your profile.";
                return RedirectToAction("UpdateProfile", "Customer");
            }

            ViewData["Title"] = "My Claim Report";
            // Get DTOs from service
            var dtoList = _reportService.GetCustomerClaimReport(customer.CustomerId);

            // Map DTOs to ViewModels
            var viewModelList = dtoList.Select(dto => new CustomerClaimReportViewModel
            {
                ClaimId = dto.ClaimId,
                PolicyId = dto.PolicyId,
                PolicyNumber = dto.PolicyNumber,
                VehicleRegistrationNumber = dto.VehicleRegistrationNumber,
                ClaimAmount = dto.ClaimAmount,
                ClaimReason = dto.ClaimReason,
                ClaimDate = dto.ClaimDate,
                ClaimStatus = dto.ClaimStatus
            }).ToList();

            return View(viewModelList);
        }
    }
}
