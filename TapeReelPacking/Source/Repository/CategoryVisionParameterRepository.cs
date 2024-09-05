using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapeReelPacking.Source.Model;

namespace TapeReelPacking.Source.Repository
{
    public class CategoryVisionParameterRepository
    {
        private readonly DatabaseContext _context;

        public CategoryVisionParameterRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<CategoryVisionParameter> AddAsync(CategoryVisionParameter entity)
        {
            _context.categoryVisionParametersModel.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CategoryVisionParameter> GetByIdAsync(int camID, int areaID)
        {
            return await _context.categoryVisionParametersModel
                //.Include(c => c.categoryVisionParameter) // Include related RectanglesModel if you have a navigation property
                .FirstOrDefaultAsync(ac => ac.areaID == areaID && ac.cameraID == camID);
        }

        // Read (Get All)
        public async Task<IEnumerable<CategoryVisionParameter>> GetAllAsync()
        {
            return await _context.categoryVisionParametersModel
                //.Include(c => c.Rectangles) // Include related RectanglesModel if you have a navigation property
                .ToListAsync();
        }

        // Update
        public async Task UpdateAsync(CategoryVisionParameter entity)
        {
            _context.categoryVisionParametersModel.Update(entity);
            await _context.SaveChangesAsync();
        }

        // Delete
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.categoryVisionParametersModel.FindAsync(id);
            if (entity != null)
            {
                _context.categoryVisionParametersModel.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
