using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.BusinessLogic.Services
{
    public interface ICoverageLevelService
    {
        Task<IEnumerable<CoverageLevel>> GetAllCoverageLevelsAsync();
        Task<CoverageLevel?> GetCoverageLevelByIdAsync(int id);
        Task AddCoverageLevelAsync(CoverageLevel coverageLevel);
        Task UpdateCoverageLevelAsync(CoverageLevel coverageLevel);
        Task DeleteCoverageLevelAsync(int id);
    }
}
