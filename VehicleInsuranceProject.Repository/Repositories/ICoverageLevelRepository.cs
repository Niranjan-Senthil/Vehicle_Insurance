using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.Repository.Repositories
{
    public interface ICoverageLevelRepository
    {
        Task<IEnumerable<CoverageLevel>> GetAllCoverageLevelsAsync();
        Task<CoverageLevel?> GetCoverageLevelByIdAsync(int id);
        Task AddCoverageLevelAsync(CoverageLevel coverageLevel);
        Task UpdateCoverageLevelAsync(CoverageLevel coverageLevel);
        Task DeleteCoverageLevelAsync(int id);
        Task SaveChangesAsync();
    }
}
