using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Exceptions;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<ProductListResponse>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToListResponse).ToList();
        }

        public async Task<ProductResponse?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(id);
            if (product == null) return null;
            return MapToResponse(product);
        }

        public async Task<ProductResponse?> GetBySkuAsync(string sku)
        {
            var product = await _productRepository.GetBySkuAsync(sku);
            if (product == null) return null;
            return MapToResponse(product);
        }

        public async Task<(List<ProductListResponse> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? search = null, int? categoryId = null, int? brandId = null, bool? isActive = null)
        {
            var (items, totalCount) = await _productRepository.GetPagedAsync(page, pageSize, search, categoryId, brandId, isActive);
            return (items.Select(MapToListResponse).ToList(), totalCount);
        }

        public async Task<List<ProductListResponse>> GetByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetByCategoryAsync(categoryId);
            return products.Select(MapToListResponse).ToList();
        }

        public async Task<List<ProductListResponse>> SearchAsync(string keyword, string? filters)
        {
            int? categoryId = null;
            if (!string.IsNullOrEmpty(filters))
            {
                var parts = filters.Split(',');
                foreach (var part in parts)
                {
                    if (part.StartsWith("category:"))
                    {
                        if (int.TryParse(part.Substring(9), out var catId))
                            categoryId = catId;
                    }
                }
            }
            var products = await _productRepository.SearchAsync(keyword, categoryId);
            return products.Select(MapToListResponse).ToList();
        }

        public async Task<int> CreateAsync(CreateProductRequest request)
        {
            if (await _productRepository.ExistsAsync(request.Sku))
                throw new DomainException("SKU đã tồn tại");

            var product = Product.Create(
                request.Name,
                request.Sku,
                request.BasePrice,
                request.CategoryId,
                request.BrandId,
                request.SupplierId,
                request.RequiresInstallation
            );

            if (request.StockQuantity > 0)
                product.AddStock(request.StockQuantity);

            if (!string.IsNullOrEmpty(request.Description))
                product.Update(request.Name, request.BasePrice, request.Description);

            if (request.Specs != null && request.Specs.Any())
                product.UpdateSpecs(request.Specs);

            if (request.ImageUrls != null && request.ImageUrls.Any())
            {
                for (int i = 0; i < request.ImageUrls.Count; i++)
                {
                    product.AddImage(request.ImageUrls[i], i == 0, i);
                }
            }

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
            return product.Id;
        }

        public async Task UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdForUpdateAsync(id);
            if (product == null)
                throw new DomainException("Không tìm thấy sản phẩm");

            product.Update(request.Name, request.BasePrice, request.Description);
            
            // Update additional fields
            if (product.StockQuantity != request.StockQuantity)
                product.SetStockQuantity(request.StockQuantity);
            
            if (product.RequiresInstallation != request.RequiresInstallation)
                product.SetRequiresInstallation(request.RequiresInstallation);
            
            if (request.SupplierId.HasValue && product.SupplierId != request.SupplierId)
                product.ChangeSupplier(request.SupplierId.Value);
            else if (!request.SupplierId.HasValue && product.SupplierId.HasValue)
                product.RemoveSupplier();
            
            if (request.Specs != null)
                product.UpdateSpecs(request.Specs);

            product.MoveToCategory(request.CategoryId);
            product.ChangeBrand(request.BrandId);

            if (request.IsActive && !product.IsActive)
                product.Activate();
            else if (!request.IsActive && product.IsActive)
                product.Deactivate();

            // Handle images - remove old ones and add new ones
            if (request.ImageUrls != null)
            {
                // Explicitly remove old images from EF tracking
                foreach (var oldImage in product.Images.ToList())
                {
                    product.Images.Remove(oldImage);
                }
                
                for (int i = 0; i < request.ImageUrls.Count; i++)
                {
                    product.AddImage(request.ImageUrls[i], i == 0, i);
                }
            }

            await _productRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new DomainException("Không tìm thấy sản phẩm");

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            product.Activate();
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            product.Deactivate();
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task AddStockAsync(int id, AddStockRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new DomainException("Không tìm thấy sản phẩm");

            product.AddStock(request.Quantity);
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return false;

            if (quantity > 0)
                product.AddStock(quantity);
            else if (quantity < 0)
            {
                try
                {
                    product.DeductStock(Math.Abs(quantity));
                }
                catch
                {
                    return false;
                }
            }

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        private ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                BasePrice = product.BasePrice,
                StockQuantity = product.StockQuantity,
                FrozenStockQuantity = product.FrozenStockQuantity,
                Description = product.Description,
                Specs = product.GetSpecs(),
                IsActive = product.IsActive,
                RequiresInstallation = product.RequiresInstallation,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "",
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? "",
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name,
                Variants = product.Variants?.Select(v => new ProductVariantResponse
                {
                    Id = v.Id,
                    Sku = v.Sku,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Attributes = v.GetAttributes(),
                    IsActive = v.IsActive
                }).ToList() ?? new List<ProductVariantResponse>(),
                Images = product.Images?.Select(i => new ProductImageResponse
                {
                    Id = i.Id,
                    Url = i.Url,
                    AltText = i.AltText,
                    IsMain = i.IsMain,
                    SortOrder = i.SortOrder
                }).ToList() ?? new List<ProductImageResponse>(),
                CreatedAt = product.CreatedAt
            };
        }

        private ProductListResponse MapToListResponse(Product product)
        {
            return new ProductListResponse
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                BasePrice = product.BasePrice,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CategoryName = product.Category?.Name ?? "",
                BrandName = product.Brand?.Name ?? "",
                MainImageUrl = product.Images?.FirstOrDefault(i => i.IsMain)?.Url ?? product.Images?.FirstOrDefault()?.Url
            };
        }
    }
}
