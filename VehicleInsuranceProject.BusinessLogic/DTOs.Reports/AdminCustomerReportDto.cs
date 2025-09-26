using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleInsuranceProject.BusinessLogic.DTOs.Reports
{
    public class AdminCustomerReportDto
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalVehicles { get; set; }
        public int ActivePolicies { get; set; }
        public int TotalClaims { get; set; }
    }
}
