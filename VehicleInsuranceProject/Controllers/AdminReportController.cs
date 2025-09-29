using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleInsuranceProject.BusinessLogic.Services;
using VehicleInsuranceProject.ViewModels;

namespace VehicleInsuranceProject.Controllers
{
    [Authorize(Roles = "Admin")] // Only administrators can access these reports
    public class AdminReportController : Controller
    {
        private readonly IReportService _reportService;

        public AdminReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Admin Reports Dashboard";
            return View(); // Just return the view, no model needed for this dashboard
        }

        [HttpGet]
        public IActionResult AllPolicies()
        {
            ViewData["Title"] = "All Policies Report (Admin)";
            // Get DTOs from service
            var dtoList = _reportService.GetAdminPolicyReport();

            // Map DTOs to ViewModels
            var viewModelList = dtoList.Select(dto => new AdminPolicyReportViewModel
            {
                PolicyId = dto.PolicyId,
                PolicyNumber = dto.PolicyNumber,
                VehicleId = dto.VehicleId,
                VehicleRegistrationNumber = dto.VehicleRegistrationNumber,
                VehicleMake = dto.VehicleMake,
                VehicleModel = dto.VehicleModel,
                VehicleYear = dto.VehicleYear,
                VehicleType = dto.VehicleType,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                CoverageAmount = dto.CoverageAmount,
                PremiumAmount = dto.PremiumAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                PolicyStatus = dto.PolicyStatus
            }).ToList();

            return View(viewModelList);
        }

        [HttpGet]
        public IActionResult AllClaims()
        {
            ViewData["Title"] = "All Claims Report (Admin)";
            // Get DTOs from service
            var dtoList = _reportService.GetAdminClaimReport();

            // Map DTOs to ViewModels
            var viewModelList = dtoList.Select(dto => new AdminClaimReportViewModel
            {
                ClaimId = dto.ClaimId,
                PolicyId = dto.PolicyId,
                PolicyNumber = dto.PolicyNumber,
                VehicleId = dto.VehicleId,
                VehicleRegistrationNumber = dto.VehicleRegistrationNumber,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                ClaimAmount = dto.ClaimAmount,
                ClaimReason = dto.ClaimReason,
                ClaimDate = dto.ClaimDate,
                ClaimStatus = dto.ClaimStatus
            }).ToList();

            return View(viewModelList);
        }

        [HttpGet]
        public IActionResult AllVehicles()
        {
            ViewData["Title"] = "All Vehicles Report (Admin)";
            // Get DTOs from service
            var dtoList = _reportService.GetAdminVehicleReport();

            // Map DTOs to ViewModels
            var viewModelList = dtoList.Select(dto => new AdminVehicleReportViewModel
            {
                VehicleId = dto.VehicleId,
                RegistrationNumber = dto.RegistrationNumber,
                Make = dto.Make,
                Model = dto.Model,
                YearOfManufacture = dto.YearOfManufacture,
                VehicleType = dto.VehicleType,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                NumberOfPolicies = dto.NumberOfPolicies,
                NumberOfClaims = dto.NumberOfClaims
            }).ToList();

            return View(viewModelList);
        }

        [HttpGet]
        public IActionResult AllCustomers()
        {
            ViewData["Title"] = "All Customers Report (Admin)";
            // Get DTOs from service
            var dtoList = _reportService.GetAdminCustomerReport();

            // Map DTOs to ViewModels
            var viewModelList = dtoList.Select(dto => new AdminCustomerReportViewModel
            {
                //New comment
                CustomerId = dto.CustomerId,
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                IsActive = dto.IsActive,
                TotalVehicles = dto.TotalVehicles,
                ActivePolicies = dto.ActivePolicies,
                TotalClaims = dto.TotalClaims
            }).ToList();

            return View(viewModelList);
        }
    }
}
