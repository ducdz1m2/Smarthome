using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Entities.Inventory;
using Domain.Exceptions;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductWarehouseRepository _productWarehouseRepository;
        private readonly IWarehouseRepository _warehouseRepository;

        public ProductService(
            IProductRepository productRepository,
            IProductWarehouseRepository productWarehouseRepository,
            IWarehouseRepository warehouseRepository)
        {
            _productRepository = productRepository;
            _productWarehouseRepository = productWarehouseRepository;
            _warehouseRepository = warehouseRepository;
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
            
            // Get actual stock from ProductWarehouse
            var warehouseStocks = await _productWarehouseRepository.GetByProductAsync(id);
            var totalQuantity = warehouseStocks.Sum(pw => pw.Quantity);
            var totalReserved = warehouseStocks.Sum(pw => pw.ReservedQuantity);
            
            return MapToResponse(product, totalQuantity, totalReserved, warehouseStocks);
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
                request.CategoryId,
                request.BrandId,
                request.SupplierId,
                request.RequiresInstallation
            );

            if (!string.IsNullOrEmpty(request.Description))
                product.Update(request.Name, request.Description);

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

            // Nếu có tồn kho ban đầu, thêm vào kho mặc định
            if (request.StockQuantity > 0)
            {
                var warehouses = await _warehouseRepository.GetAllAsync();
                var defaultWarehouse = warehouses.FirstOrDefault();
                if (defaultWarehouse != null)
                {
                    var productWarehouse = ProductWarehouse.Create(product.Id, defaultWarehouse.Id, request.StockQuantity);
                    await _productWarehouseRepository.AddAsync(productWarehouse);
                    await _productWarehouseRepository.SaveChangesAsync();

                    // Đồng bộ Product.StockQuantity
                    product.SetStockQuantity(request.StockQuantity);
                    _productRepository.Update(product);
                    await _productRepository.SaveChangesAsync();
                }
            }

            return product.Id;
        }

        public async Task UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdForUpdateAsync(id);
            if (product == null)
                throw new DomainException("Không tìm thấy sản phẩm");

            product.Update(request.Name, request.Description);

            // Không cập nhật StockQuantity trực tiếp từ đây
            // Tồn kho phải được quản lý thông qua hệ thống kho (InventoryService)
            // để đảm bảo đồng nhất giữa ProductWarehouse và Product.StockQuantity

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

            // Xác định kho để nhập
            int warehouseId;
            if (request.WarehouseId.HasValue && request.WarehouseId.Value > 0)
            {
                warehouseId = request.WarehouseId.Value;
                var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
                if (warehouse == null)
                    throw new DomainException("Không tìm thấy kho");
            }
            else
            {
                // Nếu không chỉ định kho, lấy kho đầu tiên
                var warehouses = await _warehouseRepository.GetAllAsync();
                var firstWarehouse = warehouses.FirstOrDefault();
                if (firstWarehouse == null)
                    throw new DomainException("Chưa có kho nào. Vui lòng tạo kho trước khi nhập hàng.");
                warehouseId = firstWarehouse.Id;
            }

            // Cập nhật tồn kho trong ProductWarehouse
            var productWarehouse = await _productWarehouseRepository
                .GetByProductAndWarehouseAsync(id, warehouseId);

            if (productWarehouse == null)
            {
                productWarehouse = ProductWarehouse.Create(id, warehouseId, 0);
                await _productWarehouseRepository.AddAsync(productWarehouse);
            }

            productWarehouse.Receive(request.Quantity);
            _productWarehouseRepository.Update(productWarehouse);
            await _productWarehouseRepository.SaveChangesAsync();

            // Đồng bộ Product.StockQuantity từ tổng tồn kho các kho
            await SyncProductStockFromWarehouses(id);
        }

        /// <summary>
        /// Đồng bộ Product.StockQuantity từ tổng tồn kho của tất cả các kho
        /// </summary>
        private async Task SyncProductStockFromWarehouses(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return;

            var warehouseStocks = await _productWarehouseRepository.GetByProductAsync(productId);
            var totalStock = warehouseStocks.Sum(pw => pw.Quantity);

            product.SetStockQuantity(totalStock);
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return false;

            try
            {
                var newQuantity = product.StockQuantity + quantity;
                if (newQuantity < 0) return false;
                product.SetStockQuantity(newQuantity);
            }
            catch
            {
                return false;
            }

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        private ProductResponse MapToResponse(Product product)
        {
            return MapToResponse(product, product.StockQuantity, product.FrozenStockQuantity, new List<ProductWarehouse>());
        }

        private ProductResponse MapToResponse(Product product, int totalQuantity, int totalReserved, List<ProductWarehouse> warehouseStocks)
        {
            var activeVariants = product.Variants?.Where(v => v.IsActive).ToList() ?? new List<ProductVariant>();
            var minPrice = activeVariants.Any() ? activeVariants.Min(v => v.Price.Amount) : 0;
            var maxPrice = activeVariants.Any() ? activeVariants.Max(v => v.Price.Amount) : 0;

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku.Value,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                StockQuantity = totalQuantity,
                FrozenStockQuantity = totalReserved,
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
                Variants = product.Variants?.Select(v =>
                {
                    // Calculate stock for this variant from warehouse
                    var variantStocks = warehouseStocks.Where(pw => pw.VariantId == v.Id).ToList();
                    var variantQty = variantStocks.Sum(pw => pw.Quantity);
                    var variantReserved = variantStocks.Sum(pw => pw.ReservedQuantity);
                    
                    return new ProductVariantResponse
                    {
                        Id = v.Id,
                        Sku = v.Sku.Value,
                        Price = v.Price.Amount,
                        StockQuantity = variantQty,
                        WarrantyPeriod = v.WarrantyPeriod,
                        Attributes = v.GetAttributes(),
                        IsActive = v.IsActive
                    };
                }).ToList() ?? new List<ProductVariantResponse>(),
                Images = product.Images?.Select(i => new ProductImageResponse
                {
                    Id = i.Id,
                    Url = i.Url,
                    AltText = i.AltText,
                    IsMain = i.IsMain,
                    SortOrder = i.SortOrder
                }).ToList() ?? new List<ProductImageResponse>(),
                Comments = product.Comments?.Where(c => c.IsApproved)
                    .Select(c => new ProductCommentResponse
                    {
                        Id = c.Id,
                        ProductId = c.ProductId,
                        UserId = c.UserId,
                        UserName = $"User_{c.UserId}", // TODO: Get actual user name from user service
                        Content = c.Content,
                        Rating = c.Rating,
                        IsApproved = c.IsApproved,
                        IsVerifiedPurchase = c.IsVerifiedPurchase,
                        CreatedAt = c.CreatedAt
                    }).ToList() ?? new List<ProductCommentResponse>(),
                CreatedAt = product.CreatedAt
            };
        }

        private ProductListResponse MapToListResponse(Product product)
        {
            var activeVariants = product.Variants?.Where(v => v.IsActive).ToList() ?? new List<ProductVariant>();
            var minPrice = activeVariants.Any() ? activeVariants.Min(v => v.Price.Amount) : 0;
            var maxPrice = activeVariants.Any() ? activeVariants.Max(v => v.Price.Amount) : 0;

            return new ProductListResponse
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku.Value,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CategoryName = product.Category?.Name ?? "",
                BrandName = product.Brand?.Name ?? "",
                MainImageUrl = product.Images?.FirstOrDefault(i => i.IsMain)?.Url ?? product.Images?.FirstOrDefault()?.Url
            };
        }
    }
}
