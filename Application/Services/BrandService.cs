using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Exceptions;

namespace Application.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;

        public BrandService(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<List<BrandResponse>> GetAllAsync()
        {
            var brands = await _brandRepository.GetAllAsync();
            return brands.Select(MapToResponse).ToList();
        }

        public async Task<BrandResponse?> GetByIdAsync(int id)
        {
            var brand = await _brandRepository.GetByIdWithProductsAsync(id);
            if (brand == null) return null;
            return MapToResponse(brand);
        }

        public async Task<int> CreateAsync(CreateBrandRequest request)
        {
            if (await _brandRepository.ExistsAsync(request.Name))
                throw new DomainException("Tên thương hiệu đã tồn tại");

            var brand = Brand.Create(
                request.Name,
                request.Description,
                request.LogoUrl,
                request.Website
            );

            await _brandRepository.AddAsync(brand);
            await _brandRepository.SaveChangesAsync();
            return brand.Id;
        }

        public async Task UpdateAsync(int id, UpdateBrandRequest request)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                throw new DomainException("Không tìm thấy thương hiệu");

            if (await _brandRepository.ExistsAsync(request.Name, id))
                throw new DomainException("Tên thương hiệu đã tồn tại");

            brand.Update(request.Name, request.Description, request.LogoUrl, request.Website);

            if (request.IsActive && !brand.IsActive)
                brand.Activate();
            else if (!request.IsActive && brand.IsActive)
                brand.Deactivate();

            _brandRepository.Update(brand);
            await _brandRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await _brandRepository.GetByIdWithProductsAsync(id);
            if (brand == null)
                throw new DomainException("Không tìm thấy thương hiệu");

            if (brand.Products?.Any() == true)
                throw new DomainException("Không thể xóa thương hiệu đã có sản phẩm");

            _brandRepository.Delete(brand);
            await _brandRepository.SaveChangesAsync();
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null) return false;

            brand.Activate();
            _brandRepository.Update(brand);
            await _brandRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null) return false;

            brand.Deactivate();
            _brandRepository.Update(brand);
            await _brandRepository.SaveChangesAsync();
            return true;
        }

        private BrandResponse MapToResponse(Brand brand)
        {
            return new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                Website = brand.Website,
                IsActive = brand.IsActive,
                ProductCount = brand.Products?.Count ?? 0
            };
        }
    }
}
