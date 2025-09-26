using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleInsuranceProject.BusinessLogic.DTOs.Reports;
using VehicleInsuranceProject.Repository.Models;
using VehicleInsuranceProject.Repository.Repositories;

namespace VehicleInsuranceProject.BusinessLogic.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportingPolicyRepository _reportingPolicyRepository;
        private readonly IReportingClaimRepository _reportingClaimRepository;
        private readonly IReportingVehicleRepository _reportingVehicleRepository;
        private readonly IReportingCustomerRepository _reportingCustomerRepository; // This is the new dedicated repo
        private readonly ICustomerService _customerService; // Injected to get customer data

        public ReportService(
            IReportingPolicyRepository reportingPolicyRepository,
            IReportingClaimRepository reportingClaimRepository,
            IReportingVehicleRepository reportingVehicleRepository,
            IReportingCustomerRepository reportingCustomerRepository, // Inject the dedicated reporting customer repo
            ICustomerService customerService) // Inject existing customer service for its async method
        {
            _reportingPolicyRepository = reportingPolicyRepository;
            _reportingClaimRepository = reportingClaimRepository;
            _reportingVehicleRepository = reportingVehicleRepository;
            _reportingCustomerRepository = reportingCustomerRepository;
            _customerService = customerService;
        }

        // Customer Reports

        public IEnumerable<CustomerPolicyReportDto> GetCustomerPolicyReport(int customerId)
        {
            var policies = _reportingPolicyRepository.GetPoliciesByCustomerIdWithDetails(customerId);

            return policies.Select(p => new CustomerPolicyReportDto // Map to DTO
            {
                PolicyId = p.policyId,
                PolicyNumber = p.policyNumber,
                VehicleRegistrationNumber = p.Vehicle?.registrationNumber ?? "N/A",
                VehicleMake = p.Vehicle?.make ?? "N/A",
                VehicleModel = p.Vehicle?.model ?? "N/A",
                VehicleYear = p.Vehicle?.yearOfManufacture ?? 0,
                VehicleType = p.Vehicle?.vehicleType ?? VehicleType.CAR, // Default if null
                CoverageAmount = p.coverageAmount,
                PremiumAmount = p.premiumAmount,
                StartDate = p.startDate,
                EndDate = p.endDate,
                PolicyStatus = p.policyStatus
            }).ToList();
        }

        public IEnumerable<CustomerClaimReportDto> GetCustomerClaimReport(int customerId)
        {
            var claims = _reportingClaimRepository.GetClaimsByCustomerIdWithDetails(customerId);

            return claims.Select(c => new CustomerClaimReportDto // Map to DTO
            {
                ClaimId = c.claimId,
                PolicyId = c.policyId,
                PolicyNumber = c.Policy?.policyNumber ?? "N/A",
                VehicleRegistrationNumber = c.Policy?.Vehicle?.registrationNumber ?? "N/A",
                ClaimAmount = c.claimAmount,
                ClaimReason = c.claimReason,
                ClaimDate = c.claimDate,
                ClaimStatus = c.claimStatus
            }).ToList();
        }

        // Admin Reports

        public IEnumerable<AdminPolicyReportDto> GetAdminPolicyReport()
        {
            var policies = _reportingPolicyRepository.GetAllPoliciesWithDetails();

            return policies.Select(p => new AdminPolicyReportDto // Map to DTO
            {
                PolicyId = p.policyId,
                PolicyNumber = p.policyNumber,
                VehicleId = p.vehicleId,
                VehicleRegistrationNumber = p.Vehicle?.registrationNumber ?? "N/A",
                VehicleMake = p.Vehicle?.make ?? "N/A",
                VehicleModel = p.Vehicle?.model ?? "N/A",
                VehicleYear = p.Vehicle?.yearOfManufacture ?? 0,
                VehicleType = p.Vehicle?.vehicleType ?? VehicleType.CAR,
                CustomerId = p.Vehicle?.Customer?.CustomerId ?? 0,
                CustomerName = p.Vehicle?.Customer?.Name ?? "N/A",
                CustomerEmail = p.Vehicle?.Customer?.Email ?? "N/A",
                CoverageAmount = p.coverageAmount,
                PremiumAmount = p.premiumAmount,
                StartDate = p.startDate,
                EndDate = p.endDate,
                PolicyStatus = p.policyStatus
            }).ToList();
        }

        public IEnumerable<AdminClaimReportDto> GetAdminClaimReport()
        {
            var claims = _reportingClaimRepository.GetAllClaimsWithDetails();

            return claims.Select(c => new AdminClaimReportDto // Map to DTO
            {
                ClaimId = c.claimId,
                PolicyId = c.policyId,
                PolicyNumber = c.Policy?.policyNumber ?? "N/A",
                VehicleId = c.Policy?.vehicleId ?? 0,
                VehicleRegistrationNumber = c.Policy?.Vehicle?.registrationNumber ?? "N/A",
                CustomerId = c.Policy?.Vehicle?.Customer?.CustomerId ?? 0,
                CustomerName = c.Policy?.Vehicle?.Customer?.Name ?? "N/A",
                CustomerEmail = c.Policy?.Vehicle?.Customer?.Email ?? "N/A",
                ClaimAmount = c.claimAmount,
                ClaimReason = c.claimReason,
                ClaimDate = c.claimDate,
                ClaimStatus = c.claimStatus
            }).ToList();
        }

        public IEnumerable<AdminVehicleReportDto> GetAdminVehicleReport()
        {
            var vehicles = _reportingVehicleRepository.GetAllVehiclesWithDetails();
            // Fetch all policies and claims to calculate counts in memory
            var allPolicies = _reportingPolicyRepository.GetAllPoliciesWithDetails().ToList();
            var allClaims = _reportingClaimRepository.GetAllClaimsWithDetails().ToList();

            return vehicles.Select(v => new AdminVehicleReportDto // Map to DTO
            {
                VehicleId = v.vehicleId,
                RegistrationNumber = v.registrationNumber,
                Make = v.make,
                Model = v.model,
                YearOfManufacture = v.yearOfManufacture,
                VehicleType = v.vehicleType,
                CustomerId = v.CustomerId,
                CustomerName = v.Customer?.Name ?? "N/A",
                CustomerEmail = v.Customer?.Email ?? "N/A",
                // Calculate counts by filtering the globally fetched lists
                NumberOfPolicies = allPolicies.Count(p => p.vehicleId == v.vehicleId),
                NumberOfClaims = allClaims.Count(c => c.Policy?.vehicleId == v.vehicleId)
            }).ToList();
        }

        public IEnumerable<AdminCustomerReportDto> GetAdminCustomerReport()
        {
            var customers = _reportingCustomerRepository.GetAllCustomersWithDetails(); // Get customer entities with loaded vehicles

            // Fetch all policies and claims to calculate counts in memory
            var allPolicies = _reportingPolicyRepository.GetAllPoliciesWithDetails().ToList();
            var allClaims = _reportingClaimRepository.GetAllClaimsWithDetails().ToList();

            return customers.Select(c => new AdminCustomerReportDto // Map to DTO
            {
                CustomerId = c.CustomerId,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone ?? "N/A",
                Address = c.Address ?? "N/A",
                IsActive = c.IsActive,
                TotalVehicles = c.Vehicles?.Count ?? 0, // Count directly from loaded Vehicles navigation property
                // Calculate counts by filtering the globally fetched lists by customer ID
                ActivePolicies = allPolicies.Count(p => p.Vehicle?.CustomerId == c.CustomerId && p.policyStatus == PolicyStatus.ACTIVE),
                TotalClaims = allClaims.Count(cl => cl.Policy?.Vehicle?.CustomerId == c.CustomerId)
            }).ToList();
        }
    }
}
