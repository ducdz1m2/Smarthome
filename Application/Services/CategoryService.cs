using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.DTOs.Responses.Catalog;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMemoryCache _cache;
        private const string CategoriesCacheKey = "AllCategories";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        public CategoryService(ICategoryRepository categoryRepository, IMemoryCache cache)
        {
            _categoryRepository = categoryRepository;
            _cache = cache;
        }

        public async Task<List<CategoryResponse>> GetAllAsync()
        {
            if (_cache.TryGetValue(CategoriesCacheKey, out List<CategoryResponse>? cachedCategories))
            {
                return cachedCategories ?? new List<CategoryResponse>();
            }

            var categories = await _categoryRepository.GetAllAsync();
            var result = categories.Select(MapToResponse).ToList();
            
            _cache.Set(CategoriesCacheKey, result, _cacheDuration);
            return result;
        }

        public async Task<CategoryResponse?> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdWithChildrenAsync(id);
            if (category == null) return null;
            return MapToResponse(category);
        }

        public async Task<List<CategoryTreeResponse>> GetTreeAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var rootCategories = categories.Where(c => c.ParentId == null).ToList();
            return rootCategories.Select(c => MapToTreeResponse(c, 0)).ToList();
        }

        public async Task<int> CreateAsync(CreateCategoryRequest request)
        {
            if (await _categoryRepository.ExistsAsync(request.Name))
                throw new DomainException("Tên danh mục đã tồn tại");

            var category = Category.Create(
                request.Name,
                request.ParentId,
                request.SortOrder,
                request.Description
            );

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();
            
            _cache.Remove(CategoriesCacheKey);
            return category.Id;
        }

        public async Task UpdateAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new DomainException("Không tìm thấy danh mục");

            if (await _categoryRepository.ExistsAsync(request.Name, id))
                throw new DomainException("Tên danh mục đã tồn tại");

            category.Update(request.Name, request.ParentId, request.SortOrder, request.Description);
            
            if (request.IsActive && !category.IsActive)
                category.Activate();
            else if (!request.IsActive && category.IsActive)
                category.Deactivate();

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();
            
            _cache.Remove(CategoriesCacheKey);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdWithChildrenAsync(id);
            if (category == null)
                throw new DomainException("Không tìm thấy danh mục");

            if (category.HasChildren())
                throw new DomainException("Không thể xóa danh mục có chứa danh mục con");

            if (category.HasProducts())
                throw new DomainException("Không thể xóa danh mục đã có sản phẩm");

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync();
            
            _cache.Remove(CategoriesCacheKey);
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            category.Activate();
            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();
            
            _cache.Remove(CategoriesCacheKey);
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            category.Deactivate();
            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();
            
            _cache.Remove(CategoriesCacheKey);
            return true;
        }

        private CategoryResponse MapToResponse(Category category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentId = category.ParentId,
                ParentName = category.Parent?.Name,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                ProductCount = category.Products?.Count ?? 0,
                Children = category.Children?.Select(MapToResponse).ToList() ?? new List<CategoryResponse>()
            };
        }

        private CategoryTreeResponse MapToTreeResponse(Category category, int level)
        {
            var result = new CategoryTreeResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentId = category.ParentId,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                ProductCount = category.Products?.Count ?? 0,
                Level = level,
                Children = category.Children?.Select(c => MapToTreeResponse(c, level + 1)).ToList() ?? new List<CategoryTreeResponse>()
            };
            return result;
        }
    }
}
