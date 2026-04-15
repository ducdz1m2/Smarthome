using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Exceptions;

namespace Application.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;

        public ProductVariantService(
            IProductVariantRepository variantRepository,
            IProductRepository productRepository)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
        }

        public async Task<List<ProductVariantListResponse>> GetByProductIdAsync(int productId)
        {
            var variants = await _variantRepository.GetByProductIdAsync(productId);
            return variants.Select(MapToListResponse).ToList();
        }

        public async Task<ProductVariantDetailResponse?> GetByIdAsync(int id)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null) return null;
            return MapToDetailResponse(variant);
        }

        public async Task<ProductVariantDetailResponse?> GetBySkuAsync(string sku)
        {
            var variant = await _variantRepository.GetBySkuAsync(sku);
            if (variant == null) return null;
            return MapToDetailResponse(variant);
        }

        public async Task<int> CreateAsync(CreateProductVariantRequest request)
        {
            // Verify product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                throw new DomainException("Không tìm thấy sản phẩm");

            // Check SKU uniqueness
            if (await _variantRepository.ExistsAsync(request.Sku))
                throw new DomainException("SKU đã tồn tại");

            // Check product SKU doesn't conflict
            if (await _productRepository.ExistsAsync(request.Sku))
                throw new DomainException("SKU đã tồn tại ở sản phẩm chính");

            var variant = ProductVariant.Create(
                request.ProductId,
                request.Sku,
                request.Price,
                request.StockQuantity,
                request.Attributes
            );

            await _variantRepository.AddAsync(variant);
            await _variantRepository.SaveChangesAsync();

            return variant.Id;
        }

        public async Task UpdateAsync(int id, UpdateProductVariantRequest request)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null)
                throw new DomainException("Không tìm thấy phân loại sản phẩm");

            // Check SKU uniqueness if changed
            if (variant.Sku.Value != request.Sku.ToUpper())
            {
                if (await _variantRepository.ExistsAsync(request.Sku, id))
                    throw new DomainException("SKU đã tồn tại");

                if (await _productRepository.ExistsAsync(request.Sku))
                    throw new DomainException("SKU đã tồn tại ở sản phẩm chính");
            }

            // Update using reflection since the properties are private
            typeof(ProductVariant).GetProperty("Sku")?.SetValue(variant, request.Sku.Trim().ToUpper());
            typeof(ProductVariant).GetProperty("Price")?.SetValue(variant, request.Price);
            typeof(ProductVariant).GetProperty("StockQuantity")?.SetValue(variant, request.StockQuantity);
            
            variant.UpdateAttributes(request.Attributes);

            if (request.IsActive && !variant.IsActive)
                variant.Activate();
            else if (!request.IsActive && variant.IsActive)
                variant.Deactivate();

            _variantRepository.Update(variant);
            await _variantRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null)
                throw new DomainException("Không tìm thấy phân loại sản phẩm");

            _variantRepository.Delete(variant);
            await _variantRepository.SaveChangesAsync();
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null) return false;

            variant.Activate();
            _variantRepository.Update(variant);
            await _variantRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null) return false;

            variant.Deactivate();
            _variantRepository.Update(variant);
            await _variantRepository.SaveChangesAsync();
            return true;
        }

        public async Task AddStockAsync(int id, int quantity)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null)
                throw new DomainException("Không tìm thấy phân loại sản phẩm");

            variant.AddStock(quantity);
            _variantRepository.Update(variant);
            await _variantRepository.SaveChangesAsync();
        }

        private ProductVariantListResponse MapToListResponse(ProductVariant variant)
        {
            return new ProductVariantListResponse
            {
                Id = variant.Id,
                Sku = variant.Sku.Value,
                Price = variant.Price.Amount,
                StockQuantity = variant.StockQuantity,
                Attributes = variant.GetAttributes(),
                IsActive = variant.IsActive
            };
        }

        private ProductVariantDetailResponse MapToDetailResponse(ProductVariant variant)
        {
            return new ProductVariantDetailResponse
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                ProductName = variant.Product?.Name ?? "",
                Sku = variant.Sku.Value,
                Price = variant.Price.Amount,
                StockQuantity = variant.StockQuantity,
                Attributes = variant.GetAttributes(),
                IsActive = variant.IsActive,
                CreatedAt = variant.CreatedAt
            };
        }
    }
}
