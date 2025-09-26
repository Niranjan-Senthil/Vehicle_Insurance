using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.Repository.Repositories
{
    public interface IReportingVehicleRepository
    {
        IEnumerable<Vehicle> GetAllVehiclesWithDetails();
    }
}
