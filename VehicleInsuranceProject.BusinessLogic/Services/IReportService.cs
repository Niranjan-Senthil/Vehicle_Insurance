using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleInsuranceProject.BusinessLogic.DTOs.Reports;


namespace VehicleInsuranceProject.BusinessLogic.Services
{
    public interface IReportService
    {
        // Customer Reports
        IEnumerable<CustomerPolicyReportDto> GetCustomerPolicyReport(int customerId);
        IEnumerable<CustomerClaimReportDto> GetCustomerClaimReport(int customerId);

        // Admin Reports
        IEnumerable<AdminPolicyReportDto> GetAdminPolicyReport();
        IEnumerable<AdminClaimReportDto> GetAdminClaimReport();
        IEnumerable<AdminVehicleReportDto> GetAdminVehicleReport();
        IEnumerable<AdminCustomerReportDto> GetAdminCustomerReport();
    }
}
