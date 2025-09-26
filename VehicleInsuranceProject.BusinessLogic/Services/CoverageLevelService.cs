using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleInsuranceProject.Repository.Models;
using VehicleInsuranceProject.Repository.Repositories;

namespace VehicleInsuranceProject.BusinessLogic.Services
{
    public class CoverageLevelService : ICoverageLevelService
    {
        private readonly ICoverageLevelRepository _coverageLevelRepository;

        public CoverageLevelService(ICoverageLevelRepository coverageLevelRepository)
        {
            _coverageLevelRepository = coverageLevelRepository;
        }

        public async Task<IEnumerable<CoverageLevel>> GetAllCoverageLevelsAsync()
        {
            return await _coverageLevelRepository.GetAllCoverageLevelsAsync();
        }

        public async Task<CoverageLevel?> GetCoverageLevelByIdAsync(int id)
        {
            return await _coverageLevelRepository.GetCoverageLevelByIdAsync(id);
        }

        public async Task AddCoverageLevelAsync(CoverageLevel coverageLevel)
        {
            await _coverageLevelRepository.AddCoverageLevelAsync(coverageLevel);
        }

        public async Task UpdateCoverageLevelAsync(CoverageLevel coverageLevel)
        {
            await _coverageLevelRepository.UpdateCoverageLevelAsync(coverageLevel);
        }

        public async Task DeleteCoverageLevelAsync(int id)
        {
            await _coverageLevelRepository.DeleteCoverageLevelAsync(id);
        }
    }
}
