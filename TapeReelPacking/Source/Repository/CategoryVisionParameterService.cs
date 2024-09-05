using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapeReelPacking.Source.Model;

namespace TapeReelPacking.Source.Repository
{
    public class CategoryVisionParameterService
    {
        private readonly CategoryVisionParameterRepository _repository;

        public CategoryVisionParameterService(CategoryVisionParameterRepository repository)
        {
            _repository = repository;
        }

        public async Task<CategoryVisionParameter> CreateCategoryTeachParameter(CategoryVisionParameter entity)
        {
            return await _repository.AddAsync(entity);
        }

        public async Task<CategoryVisionParameter> GetCategoryTeachParameterById(int cameraID, int areaID)
        {
            return await _repository.GetByIdAsync(cameraID, areaID);
        }

        public async Task<IEnumerable<CategoryVisionParameter>> GetAllCategoryTeachParameters()
        {
            return await _repository.GetAllAsync();
        }

        public async Task UpdateCategoryTeachParameter(CategoryVisionParameter entity)
        {
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteCategoryTeachParameter(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
