using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapeReelPacking.Source.Model;

namespace TapeReelPacking.Source.Repository
{
    public class CategoryTeachParameterService
    {
        private readonly CategoryTeachParameterRepository _repository;

        public CategoryTeachParameterService(CategoryTeachParameterRepository repository)
        {
            _repository = repository;
        }

        public async Task<CategoryTeachParameter> CreateCategoryTeachParameter(CategoryTeachParameter entity)
        {
            return await _repository.AddAsync(entity);
        }

        public async Task<CategoryTeachParameter> GetCategoryTeachParameterById(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<CategoryTeachParameter>> GetAllCategoryTeachParameters()
        {
            return await _repository.GetAllAsync();
        }

        public async Task UpdateCategoryTeachParameter(CategoryTeachParameter entity)
        {
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteCategoryTeachParameter(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
