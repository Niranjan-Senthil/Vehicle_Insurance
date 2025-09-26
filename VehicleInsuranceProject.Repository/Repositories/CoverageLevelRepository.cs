using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleInsuranceProject.Repository.Data;
using VehicleInsuranceProject.Repository.Models;

namespace VehicleInsuranceProject.Repository.Repositories
{
    public class CoverageLevelRepository : ICoverageLevelRepository
    {
        private readonly ApplicationDbContext _context;

        public CoverageLevelRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CoverageLevel>> GetAllCoverageLevelsAsync()
        {
            return await _context.CoverageLevels.ToListAsync();
        }

        public async Task<CoverageLevel?> GetCoverageLevelByIdAsync(int id)
        {
            return await _context.CoverageLevels.FindAsync(id);
        }

        public async Task AddCoverageLevelAsync(CoverageLevel coverageLevel)
        {
            _context.CoverageLevels.Add(coverageLevel);
            await SaveChangesAsync();
        }

        public async Task UpdateCoverageLevelAsync(CoverageLevel coverageLevel)
        {
            _context.CoverageLevels.Update(coverageLevel);
            await SaveChangesAsync();
        }

        public async Task DeleteCoverageLevelAsync(int id)
        {
            var coverageLevel = await _context.CoverageLevels.FindAsync(id);
            if (coverageLevel != null)
            {
                _context.CoverageLevels.Remove(coverageLevel);
                await SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
