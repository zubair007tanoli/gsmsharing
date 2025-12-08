using AutoMapper;
using GsmsharingV2.DTOs;
using GsmsharingV2.Interfaces;
using Microsoft.Extensions.Logging;

namespace GsmsharingV2.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IMapper mapper,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null) return null;

                return _mapper.Map<CategoryDto>(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<CategoryDto?> GetBySlugAsync(string slug)
        {
            try
            {
                var category = await _categoryRepository.GetBySlugAsync(slug);
                if (category == null) return null;

                return _mapper.Map<CategoryDto>(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by slug: {Slug}", slug);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<CategoryDto>>(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                throw;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetTopLevelCategoriesAsync();
                return _mapper.Map<IEnumerable<CategoryDto>>(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting root categories");
                throw;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(int parentId)
        {
            try
            {
                var allCategories = await _categoryRepository.GetAllAsync();
                var childCategories = allCategories.Where(c => c.ParentCategoryID == parentId);
                return _mapper.Map<IEnumerable<CategoryDto>>(childCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting child categories for parent: {ParentId}", parentId);
                throw;
            }
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto)
        {
            try
            {
                var category = _mapper.Map<Models.Category>(createCategoryDto);
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;
                category.IsActive = createCategoryDto.IsActive ?? true;

                var createdCategory = await _categoryRepository.CreateAsync(category);
                return _mapper.Map<CategoryDto>(createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        public async Task<CategoryDto> UpdateAsync(UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetByIdAsync(updateCategoryDto.CategoryID);
                if (existingCategory == null)
                {
                    throw new KeyNotFoundException($"Category with ID {updateCategoryDto.CategoryID} not found");
                }

                _mapper.Map(updateCategoryDto, existingCategory);
                existingCategory.UpdatedAt = DateTime.UtcNow;

                var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);
                return _mapper.Map<CategoryDto>(updatedCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", updateCategoryDto.CategoryID);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    throw new KeyNotFoundException($"Category with ID {id} not found");
                }

                await _categoryRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                throw;
            }
        }
    }
}

