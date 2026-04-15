using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Entities.Inventory;
using Domain.Exceptions;

namespace Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductWarehouseRepository _productWarehouseRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IStockEntryRepository _stockEntryRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductVariantService _productVariantService;

        public InventoryService(
            IProductRepository productRepository,
            IProductWarehouseRepository productWarehouseRepository,
            IWarehouseRepository warehouseRepository,
            ISupplierRepository supplierRepository,
            IStockEntryRepository stockEntryRepository,
            ICategoryRepository categoryRepository,
            IProductVariantRepository productVariantRepository,
            IProductVariantService productVariantService)
        {
            _productRepository = productRepository;
            _productWarehouseRepository = productWarehouseRepository;
            _warehouseRepository = warehouseRepository;
            _supplierRepository = supplierRepository;
            _stockEntryRepository = stockEntryRepository;
            _categoryRepository = categoryRepository;
            _productVariantRepository = productVariantRepository;
            _productVariantService = productVariantService;
        }

        #region Product Inventory

        public async Task<List<ProductInventoryResponse>> GetProductInventoryAsync(InventoryFilterRequest? filter = null)
        {
            var products = await _productRepository.GetAllAsync();

            if (filter?.CategoryId.HasValue == true)
                products = products.Where(p => p.CategoryId == filter.CategoryId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(filter?.SearchTerm))
                products = products.Where(p =>
                    p.Name.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Sku.Value.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

            var productIds = products.Select(p => p.Id).ToList();
            var warehouseStocks = productIds.Any()
                ? await _productWarehouseRepository.GetByProductsAsync(productIds)
                : new List<ProductWarehouse>();

            // Load variants for all products
            var allVariants = productIds.Any()
                ? await _productVariantRepository.GetByProductIdsAsync(productIds)
                : new List<ProductVariant>();

            var result = new List<ProductInventoryResponse>();

            foreach (var product in products)
            {
                var productStocks = warehouseStocks.Where(pw => pw.ProductId == product.Id).ToList();
                var totalQty = productStocks.Sum(pw => pw.Quantity);
                var totalReserved = productStocks.Sum(pw => pw.ReservedQuantity);

                // Load variants for this product
                var productVariants = allVariants.Where(v => v.ProductId == product.Id).ToList();
                var variantResponses = new List<ProductVariantInventoryResponse>();

                foreach (var variant in productVariants)
                {
                    // Get warehouse stocks for this variant
                    var variantStocks = productStocks.Where(pw => pw.VariantId == variant.Id).ToList();

                    variantResponses.Add(new ProductVariantInventoryResponse
                    {
                        VariantId = variant.Id,
                        Sku = variant.Sku.Value,
                        Price = variant.Price.Amount,
                        StockQuantity = variant.StockQuantity,
                        Attributes = variant.GetAttributes(),
                        IsActive = variant.IsActive,
                        WarehouseStocks = variantStocks.Select(pw => new WarehouseStockDetailResponse
                        {
                            WarehouseId = pw.WarehouseId,
                            WarehouseName = pw.Warehouse?.Name ?? "",
                            WarehouseCode = pw.Warehouse?.Code ?? "",
                            Quantity = pw.Quantity,
                            ReservedQuantity = pw.ReservedQuantity
                        }).ToList()
                    });
                }

                var response = new ProductInventoryResponse
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Sku = product.Sku.Value,
                    BasePrice = product.BasePrice.Amount,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name ?? "",
                    BrandId = product.BrandId,
                    BrandName = product.Brand?.Name ?? "",
                    TotalQuantity = totalQty,
                    TotalReserved = totalReserved,
                    IsLowStock = (totalQty - totalReserved) <= 10 && (totalQty - totalReserved) > 0,
                    MainImageUrl = "",
                    WarehouseStocks = productStocks.Select(pw => new WarehouseStockDetailResponse
                    {
                        WarehouseId = pw.WarehouseId,
                        WarehouseName = pw.Warehouse?.Name ?? "",
                        WarehouseCode = pw.Warehouse?.Code ?? "",
                        Quantity = pw.Quantity,
                        ReservedQuantity = pw.ReservedQuantity
                    }).ToList(),
                    Variants = variantResponses
                };

                if (filter?.WarehouseId.HasValue == true)
                {
                    var warehouseStock = productStocks.FirstOrDefault(pw => pw.WarehouseId == filter.WarehouseId.Value);
                    if (warehouseStock == null) continue;
                    
                    response.TotalQuantity = warehouseStock.Quantity;
                    response.TotalReserved = warehouseStock.ReservedQuantity;
                    response.WarehouseStocks = response.WarehouseStocks
                        .Where(ws => ws.WarehouseId == filter.WarehouseId.Value)
                        .ToList();
                }

                if (filter?.LowStockOnly == true && !response.IsLowStock && response.AvailableStock > 0)
                    continue;

                result.Add(response);
            }

            return result;
        }

        public async Task<ProductInventoryResponse?> GetProductInventoryByIdAsync(int productId, int? warehouseId = null)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return null;

            List<ProductWarehouse> warehouseStocks;
            if (warehouseId.HasValue)
            {
                var singleStock = await _productWarehouseRepository.GetByProductAndWarehouseAsync(productId, warehouseId.Value);
                warehouseStocks = singleStock != null ? new List<ProductWarehouse> { singleStock } : new List<ProductWarehouse>();
            }
            else
            {
                warehouseStocks = await _productWarehouseRepository.GetByProductAsync(productId);
            }

            var totalQty = warehouseStocks.Sum(pw => pw.Quantity);
            var totalReserved = warehouseStocks.Sum(pw => pw.ReservedQuantity);

            return new ProductInventoryResponse
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Sku = product.Sku.Value,
                BasePrice = product.BasePrice.Amount,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "",
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? "",
                TotalQuantity = totalQty,
                TotalReserved = totalReserved,
                IsLowStock = (totalQty - totalReserved) <= 10 && (totalQty - totalReserved) > 0,
                MainImageUrl = "",
                WarehouseStocks = warehouseStocks.Select(pw => new WarehouseStockDetailResponse
                {
                    WarehouseId = pw.WarehouseId,
                    WarehouseName = pw.Warehouse?.Name ?? "",
                    WarehouseCode = pw.Warehouse?.Code ?? "",
                    Quantity = pw.Quantity,
                    ReservedQuantity = pw.ReservedQuantity
                }).ToList()
            };
        }

        public async Task<List<ProductInventoryResponse>> GetLowStockProductsAsync(int? warehouseId = null, int threshold = 10)
        {
            var allProducts = await GetProductInventoryAsync(new InventoryFilterRequest { WarehouseId = warehouseId });
            return allProducts.Where(p => p.AvailableStock > 0 && p.AvailableStock <= threshold)
                .OrderBy(p => p.AvailableStock).ToList();
        }

        public async Task<List<ProductInventoryResponse>> GetOutOfStockProductsAsync(int? warehouseId = null)
        {
            var allProducts = await GetProductInventoryAsync(new InventoryFilterRequest { WarehouseId = warehouseId });
            return allProducts.Where(p => p.AvailableStock == 0).ToList();
        }

        #endregion

        #region Category Inventory Summary

        public async Task<List<CategoryInventorySummaryResponse>> GetInventoryByCategoryAsync(int? parentCategoryId = null)
        {
            var allCategories = await _categoryRepository.GetAllAsync();
            var categories = parentCategoryId.HasValue 
                ? allCategories.Where(c => c.ParentId == parentCategoryId.Value).ToList()
                : allCategories.Where(c => c.ParentId == null).ToList();

            var allProducts = await _productRepository.GetAllAsync();
            var allWarehouses = await _warehouseRepository.GetAllAsync();

            var result = new List<CategoryInventorySummaryResponse>();

            foreach (var category in categories)
            {
                var categoryProducts = allProducts.Where(p => p.CategoryId == category.Id).ToList();
                var categoryProductIds = categoryProducts.Select(p => p.Id).ToList();
                
                var categoryStocks = categoryProductIds.Any() 
                    ? await _productWarehouseRepository.GetByProductsAsync(categoryProductIds)
                    : new List<ProductWarehouse>();

                var summary = new CategoryInventorySummaryResponse
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    CategoryPath = category.Parent?.Name != null ? $"{category.Parent.Name} > {category.Name}" : category.Name,
                    ParentCategoryId = category.ParentId ?? 0,
                    ParentCategoryName = category.Parent?.Name,
                    TotalProducts = categoryProducts.Count,
                    TotalStockQuantity = categoryStocks.Sum(pw => pw.Quantity),
                    TotalStockValue = categoryProducts.Sum(p => 
                        categoryStocks.Where(pw => pw.ProductId == p.Id).Sum(pw => pw.Quantity) * p.BasePrice.Amount),
                    LowStockProducts = categoryProducts.Count(p => 
                    {
                        var stock = categoryStocks.Where(pw => pw.ProductId == p.Id).Sum(pw => pw.Quantity - pw.ReservedQuantity);
                        return stock > 0 && stock <= 10;
                    }),
                    OutOfStockProducts = categoryProducts.Count(p => 
                        categoryStocks.Where(pw => pw.ProductId == p.Id).Sum(pw => pw.Quantity - pw.ReservedQuantity) == 0)
                };

                summary.WarehouseBreakdown = allWarehouses.Select(w => new WarehouseCategoryStockResponse
                {
                    WarehouseId = w.Id,
                    WarehouseName = w.Name,
                    ProductCount = categoryProducts.Count(p => categoryStocks.Any(pw => pw.WarehouseId == w.Id && pw.ProductId == p.Id && pw.Quantity > 0)),
                    TotalQuantity = categoryStocks.Where(pw => pw.WarehouseId == w.Id).Sum(pw => pw.Quantity)
                }).Where(wb => wb.TotalQuantity > 0).ToList();

                result.Add(summary);
            }

            return result.OrderByDescending(r => r.TotalStockValue).ToList();
        }

        public async Task<CategoryInventorySummaryResponse?> GetCategoryInventorySummaryAsync(int categoryId, int? warehouseId = null)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null) return null;

            var allCategories = await _categoryRepository.GetAllAsync();
            var childCategoryIds = allCategories
                .Where(c => c.ParentId == categoryId || c.Id == categoryId)
                .Select(c => c.Id).ToList();

            var allProducts = await _productRepository.GetAllAsync();
            var products = allProducts.Where(p => childCategoryIds.Contains(p.CategoryId)).ToList();
            var productIds = products.Select(p => p.Id).ToList();

            var stocks = productIds.Any()
                ? await _productWarehouseRepository.GetByProductsAsync(productIds)
                : new List<ProductWarehouse>();

            if (warehouseId.HasValue)
                stocks = stocks.Where(s => s.WarehouseId == warehouseId.Value).ToList();

            var warehouses = await _warehouseRepository.GetAllAsync();

            return new CategoryInventorySummaryResponse
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategoryPath = category.Parent?.Name != null ? $"{category.Parent.Name} > {category.Name}" : category.Name,
                ParentCategoryId = category.ParentId ?? 0,
                ParentCategoryName = category.Parent?.Name,
                TotalProducts = products.Count,
                TotalStockQuantity = stocks.Sum(s => s.Quantity),
                TotalStockValue = products.Sum(p => stocks.Where(s => s.ProductId == p.Id).Sum(s => s.Quantity) * p.BasePrice.Amount),
                LowStockProducts = products.Count(p =>
                {
                    var productStock = stocks.Where(s => s.ProductId == p.Id).Sum(s => s.Quantity - s.ReservedQuantity);
                    return productStock > 0 && productStock <= 10;
                }),
                OutOfStockProducts = products.Count(p =>
                    stocks.Where(s => s.ProductId == p.Id).Sum(s => s.Quantity - s.ReservedQuantity) == 0),
                WarehouseBreakdown = stocks.GroupBy(s => s.WarehouseId).Select(g => new WarehouseCategoryStockResponse
                {
                    WarehouseId = g.Key,
                    WarehouseName = warehouses.FirstOrDefault(w => w.Id == g.Key)?.Name ?? "Unknown",
                    ProductCount = g.Select(s => s.ProductId).Distinct().Count(),
                    TotalQuantity = g.Sum(s => s.Quantity)
                }).ToList()
            };
        }

        #endregion

        #region Warehouse Inventory Summary

        public async Task<List<WarehouseInventorySummaryResponse>> GetInventoryByWarehouseAsync()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            var allProducts = await _productRepository.GetAllAsync();
            var allStocks = await _productWarehouseRepository.GetAllAsync();
            var result = new List<WarehouseInventorySummaryResponse>();

            foreach (var warehouse in warehouses)
            {
                var warehouseStocks = allStocks.Where(pw => pw.WarehouseId == warehouse.Id).ToList();
                var productIds = warehouseStocks.Select(pw => pw.ProductId).ToList();
                var products = allProducts.Where(p => productIds.Contains(p.Id)).ToList();

                var totalQty = warehouseStocks.Sum(pw => pw.Quantity);
                var totalReserved = warehouseStocks.Sum(pw => pw.ReservedQuantity);

                var summary = new WarehouseInventorySummaryResponse
                {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name,
                    WarehouseCode = warehouse.Code,
                    IsActive = warehouse.IsActive,
                    TotalProducts = warehouseStocks.Count(pw => pw.Quantity > 0),
                    TotalQuantity = totalQty,
                    TotalReserved = totalReserved,
                    TotalStockValue = products.Sum(p => 
                        warehouseStocks.Where(pw => pw.ProductId == p.Id).Sum(pw => pw.Quantity) * p.BasePrice.Amount),
                    LowStockCount = warehouseStocks.Count(pw => pw.IsLowStock()),
                    CategoryBreakdown = products
                        .GroupBy(p => p.CategoryId)
                        .Select(g => new CategoryStockInWarehouseResponse
                        {
                            CategoryId = g.Key,
                            CategoryName = g.First().Category?.Name ?? "Unknown",
                            ProductCount = g.Count(),
                            TotalQuantity = warehouseStocks.Where(pw => g.Select(p => p.Id).Contains(pw.ProductId)).Sum(pw => pw.Quantity)
                        })
                        .OrderByDescending(cb => cb.TotalQuantity)
                        .ToList()
                };

                result.Add(summary);
            }

            return result;
        }

        public async Task<WarehouseInventorySummaryResponse?> GetWarehouseInventorySummaryAsync(int warehouseId)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null) return null;

            var warehouseStocks = await _productWarehouseRepository.GetByWarehouseAsync(warehouseId);
            var productIds = warehouseStocks.Select(pw => pw.ProductId).ToList();
            
            var allProducts = await _productRepository.GetAllAsync();
            var products = allProducts.Where(p => productIds.Contains(p.Id)).ToList();

            return new WarehouseInventorySummaryResponse
            {
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                WarehouseCode = warehouse.Code,
                IsActive = warehouse.IsActive,
                TotalProducts = warehouseStocks.Count(pw => pw.Quantity > 0),
                TotalQuantity = warehouseStocks.Sum(pw => pw.Quantity),
                TotalReserved = warehouseStocks.Sum(pw => pw.ReservedQuantity),
                TotalStockValue = products.Sum(p => 
                    warehouseStocks.Where(pw => pw.ProductId == p.Id).Sum(pw => pw.Quantity) * p.BasePrice.Amount),
                LowStockCount = warehouseStocks.Count(pw => pw.IsLowStock()),
                CategoryBreakdown = products
                    .GroupBy(p => p.CategoryId)
                    .Select(g => new CategoryStockInWarehouseResponse
                    {
                        CategoryId = g.Key,
                        CategoryName = g.First().Category?.Name ?? "Unknown",
                        ProductCount = g.Count(),
                        TotalQuantity = warehouseStocks.Where(pw => g.Select(p => p.Id).Contains(pw.ProductId)).Sum(pw => pw.Quantity)
                    })
                    .OrderByDescending(cb => cb.TotalQuantity)
                    .ToList()
            };
        }

        #endregion

        #region Stock Entry Management

        public async Task<List<StockEntryListItemResponse>> GetStockEntriesAsync(int? warehouseId = null, int? supplierId = null, bool? isCompleted = null)
        {
            var entries = await _stockEntryRepository.GetFilteredAsync(warehouseId, supplierId, isCompleted);

            return entries.Select(se => new StockEntryListItemResponse
            {
                Id = se.Id,
                EntryDate = se.EntryDate,
                SupplierName = se.Supplier?.Name ?? "",
                WarehouseName = se.Warehouse?.Name ?? "",
                TotalCost = se.TotalCost,
                IsCompleted = se.IsCompleted,
                ItemCount = se.Details?.Sum(d => d.Quantity) ?? 0,
                CreatedAt = se.CreatedAt
            }).ToList();
        }

        public async Task<StockEntryDetailListResponse?> GetStockEntryByIdAsync(int id)
        {
            var entry = await _stockEntryRepository.GetByIdWithDetailsAsync(id);
            if (entry == null) return null;

            return new StockEntryDetailListResponse
            {
                Id = entry.Id,
                EntryDate = entry.EntryDate,
                SupplierId = entry.SupplierId,
                SupplierName = entry.Supplier?.Name ?? "",
                WarehouseId = entry.WarehouseId,
                WarehouseName = entry.Warehouse?.Name ?? "",
                Note = entry.Note,
                TotalCost = entry.TotalCost,
                IsCompleted = entry.IsCompleted,
                CreatedAt = entry.CreatedAt,
                Details = entry.Details?.Select(d => new StockEntryDetailItemResponse
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? "",
                    Sku = d.Product?.Sku.Value ?? "",
                    Quantity = d.Quantity,
                    UnitCost = d.UnitCost.Amount,
                    CategoryName = d.Product?.Category?.Name
                }).ToList() ?? new List<StockEntryDetailItemResponse>()
            };
        }

        public async Task<int> CreateStockEntryAsync(CreateStockEntryRequest request)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
            if (warehouse == null)
                throw new DomainException("Không tìm thấy kho");

            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId);
            if (supplier == null)
                throw new DomainException("Không tìm thấy nhà cung cấp");

            var entry = StockEntry.Create(request.SupplierId, request.WarehouseId, request.Note);

            foreach (var detail in request.Details)
            {
                var product = await _productRepository.GetByIdAsync(detail.ProductId);
                if (product == null)
                    throw new DomainException($"Không tìm thấy sản phẩm ID: {detail.ProductId}");

                entry.AddItem(detail.ProductId, detail.Quantity, detail.UnitCost, variantId: detail.VariantId);
            }

            await _stockEntryRepository.AddAsync(entry);
            await _stockEntryRepository.SaveChangesAsync();

            return entry.Id;
        }

        public async Task CompleteStockEntryAsync(int stockEntryId)
        {
            var entry = await _stockEntryRepository.GetByIdWithDetailsAsync(stockEntryId);
            if (entry == null)
                throw new DomainException("Không tìm thấy phiếu nhập kho");

            if (entry.IsCompleted)
                throw new DomainException("Phiếu nhập kho đã được hoàn thành");

            foreach (var detail in entry.Details)
            {
                var productWarehouse = await _productWarehouseRepository
                    .GetByProductVariantAndWarehouseAsync(detail.ProductId, detail.VariantId, entry.WarehouseId);

                if (productWarehouse == null)
                {
                    productWarehouse = ProductWarehouse.Create(detail.ProductId, detail.VariantId, entry.WarehouseId, 0);
                    await _productWarehouseRepository.AddAsync(productWarehouse);
                }

                productWarehouse.Receive(detail.Quantity);

                if (productWarehouse.Id > 0)
                {
                    _productWarehouseRepository.Update(productWarehouse);
                }
            }

            entry.Complete();
            // Không cần gọi Update vì entry đã được tracked từ GetByIdWithDetailsAsync
            await _stockEntryRepository.SaveChangesAsync();

            // Đồng bộ Product.StockQuantity cho tất cả sản phẩm trong phiếu nhập
            foreach (var detail in entry.Details)
            {
                await SyncProductStockFromWarehouses(detail.ProductId);
            }
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
            var totalReserved = warehouseStocks.Sum(pw => pw.ReservedQuantity);

            product.SetStockQuantity(totalStock);
            // FrozenStockQuantity cũng cần được đồng bộ từ ReservedQuantity
            // Note: Product không có method SetFrozenStockQuantity, nên cần cập nhật trực tiếp qua EF
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            // Đồng bộ ProductVariant.StockQuantity cho tất cả variants của sản phẩm
            var variants = await _productVariantRepository.GetByProductIdAsync(productId);
            foreach (var variant in variants)
            {
                // Tính tổng stock cho variant từ tất cả các kho
                var variantStocks = warehouseStocks.Where(pw => pw.VariantId == variant.Id).ToList();
                var variantTotalStock = variantStocks.Sum(pw => pw.Quantity);

                // Cập nhật StockQuantity của variant
                await _productVariantService.UpdateStockQuantityAsync(variant.Id, variantTotalStock);
            }
        }

        public async Task CancelStockEntryAsync(int stockEntryId)
        {
            var entry = await _stockEntryRepository.GetByIdWithDetailsAsync(stockEntryId);
            if (entry == null)
                throw new DomainException("Không tìm thấy phiếu nhập kho");

            if (entry.IsCompleted)
                throw new DomainException("Không thể hủy phiếu đã hoàn thành");

            _stockEntryRepository.Delete(entry);
            await _stockEntryRepository.SaveChangesAsync();
        }

        #endregion

        #region Stock Adjustments & Transfers

        public async Task AdjustStockAsync(AdjustStockRequest request)
        {
            var productWarehouse = await _productWarehouseRepository
                .GetByProductAndWarehouseAsync(request.ProductId, request.WarehouseId);

            if (productWarehouse == null)
            {
                if (request.Adjustment > 0)
                {
                    productWarehouse = ProductWarehouse.Create(request.ProductId, request.WarehouseId, request.Adjustment);
                    await _productWarehouseRepository.AddAsync(productWarehouse);
                }
                else
                {
                    throw new DomainException("Không tìm thấy tồn kho sản phẩm");
                }
            }
            else
            {
                if (request.Adjustment > 0)
                {
                    productWarehouse.Receive(request.Adjustment);
                }
                else if (request.Adjustment < 0)
                {
                    var qtyToRemove = Math.Abs(request.Adjustment);
                    if (productWarehouse.GetAvailableStock() < qtyToRemove)
                        throw new DomainException("Không đủ tồn kho để điều chỉnh");

                    productWarehouse.Dispatch(qtyToRemove);
                }
                _productWarehouseRepository.Update(productWarehouse);
            }

            await _productWarehouseRepository.SaveChangesAsync();

            // Đồng bộ Product.StockQuantity sau khi điều chỉnh
            await SyncProductStockFromWarehouses(request.ProductId);
        }

        public async Task TransferStockAsync(TransferStockRequest request)
        {
            if (request.FromWarehouseId == request.ToWarehouseId)
                throw new DomainException("Kho nguồn và kho đích không thể giống nhau");

            var fromWarehouse = await _warehouseRepository.GetByIdAsync(request.FromWarehouseId);
            var toWarehouse = await _warehouseRepository.GetByIdAsync(request.ToWarehouseId);

            if (fromWarehouse == null || toWarehouse == null)
                throw new DomainException("Không tìm thấy kho");

            var sourceStock = await _productWarehouseRepository
                .GetByProductAndWarehouseAsync(request.ProductId, request.FromWarehouseId);

            if (sourceStock == null || sourceStock.GetAvailableStock() < request.Quantity)
                throw new DomainException("Không đủ tồn kho để chuyển");

            var destStock = await _productWarehouseRepository
                .GetByProductAndWarehouseAsync(request.ProductId, request.ToWarehouseId);

            if (destStock == null)
            {
                destStock = ProductWarehouse.Create(request.ProductId, request.ToWarehouseId, 0);
                await _productWarehouseRepository.AddAsync(destStock);
            }

            sourceStock.Dispatch(request.Quantity);
            destStock.Receive(request.Quantity);

            _productWarehouseRepository.Update(sourceStock);
            _productWarehouseRepository.Update(destStock);

            await _productWarehouseRepository.SaveChangesAsync();

            // Đồng bộ Product.StockQuantity sau khi chuyển kho
            // Tổng tồn kho không đổi khi chuyển kho, nhưng vẫn đồng bộ để đảm bảo
            await SyncProductStockFromWarehouses(request.ProductId);
        }

        #endregion

        #region Reports

        public async Task<InventoryReportResponse> GetInventoryReportAsync()
        {
            var allProducts = await _productRepository.GetAllAsync();
            var allStocks = await _productWarehouseRepository.GetAllAsync();

            var totalQty = allStocks.Sum(s => s.Quantity);
            var totalReserved = allStocks.Sum(s => s.ReservedQuantity);

            decimal totalValue = 0;
            foreach (var product in allProducts)
            {
                var productStocks = allStocks.Where(s => s.ProductId == product.Id).Sum(s => s.Quantity);
                totalValue += productStocks * product.BasePrice.Amount;
            }

            var lowStockCount = allStocks.Count(s => s.IsLowStock());
            var outOfStockCount = allProducts.Count(p => 
                !allStocks.Any(s => s.ProductId == p.Id && s.Quantity > 0));

            return new InventoryReportResponse
            {
                ReportDate = DateTime.UtcNow,
                TotalProducts = allProducts.Count,
                TotalStockQuantity = totalQty,
                TotalStockValue = totalValue,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStockCount,
                CategorySummaries = await GetInventoryByCategoryAsync(),
                WarehouseSummaries = await GetInventoryByWarehouseAsync()
            };
        }

        public async Task<List<StockMovementResponse>> GetStockMovementsAsync(int? productId = null, int? warehouseId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var entries = await _stockEntryRepository.GetFilteredAsync(warehouseId, null, true);

            var movements = new List<StockMovementResponse>();
            var allWarehouses = await _warehouseRepository.GetAllAsync();
            var allProducts = await _productRepository.GetAllAsync();

            foreach (var entry in entries.Where(e => !fromDate.HasValue || e.EntryDate >= fromDate.Value)
                                         .Where(e => !toDate.HasValue || e.EntryDate <= toDate.Value))
            {
                foreach (var detail in entry.Details.Where(d => !productId.HasValue || d.ProductId == productId.Value))
                {
                    movements.Add(new StockMovementResponse
                    {
                        Id = entry.Id * 1000 + detail.Id,
                        MovementDate = entry.EntryDate,
                        MovementType = "IN",
                        ProductId = detail.ProductId,
                        ProductName = allProducts.FirstOrDefault(p => p.Id == detail.ProductId)?.Name ?? "",
                        Sku = allProducts.FirstOrDefault(p => p.Id == detail.ProductId)?.Sku.Value ?? "",
                        WarehouseId = entry.WarehouseId,
                        WarehouseName = allWarehouses.FirstOrDefault(w => w.Id == entry.WarehouseId)?.Name ?? "",
                        Quantity = detail.Quantity,
                        UnitCost = detail.UnitCost.Amount,
                        ReferenceNumber = $"NK-{entry.Id:D5}",
                        Note = entry.Note
                    });
                }
            }

            return movements.OrderByDescending(m => m.MovementDate).ToList();
        }

        #endregion

        #region Order Stock Management

        public async Task ReserveStockForOrderAsync(int productId, int quantity, int orderId, int? warehouseId = null)
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            if (!warehouses.Any())
                throw new DomainException("Chưa có kho nào trong hệ thống");

            int targetWarehouseId;
            if (warehouseId.HasValue)
            {
                targetWarehouseId = warehouseId.Value;
            }
            else
            {
                // Tìm kho có đủ tồn kho
                var allStocks = await _productWarehouseRepository.GetByProductAsync(productId);
                var availableWarehouse = allStocks
                    .Where(s => s.GetAvailableStock() >= quantity)
                    .OrderByDescending(s => s.GetAvailableStock())
                    .FirstOrDefault();

                if (availableWarehouse == null)
                    throw new InsufficientStockException(productId, 0, quantity,
                        allStocks.Sum(s => s.GetAvailableStock()));

                targetWarehouseId = availableWarehouse.WarehouseId;
            }

            var productWarehouse = await _productWarehouseRepository
                .GetByProductAndWarehouseAsync(productId, targetWarehouseId);

            if (productWarehouse == null || productWarehouse.GetAvailableStock() < quantity)
                throw new InsufficientStockException(productId, targetWarehouseId, quantity,
                    productWarehouse?.GetAvailableStock() ?? 0);

            // Reserve stock in ProductWarehouse
            productWarehouse.Reserve(quantity);
            _productWarehouseRepository.Update(productWarehouse);
            await _productWarehouseRepository.SaveChangesAsync();

            // Đồng bộ Product.StockQuantity và FrozenStockQuantity
            await SyncProductStockFromWarehouses(productId);
        }

        public async Task ReleaseStockForOrderAsync(int productId, int quantity, int orderId, int? warehouseId = null)
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            if (!warehouses.Any()) return; // No warehouses, nothing to release

            int targetWarehouseId;
            if (warehouseId.HasValue)
            {
                targetWarehouseId = warehouseId.Value;
            }
            else
            {
                // Find warehouse with reserved stock for this product
                var allStocks = await _productWarehouseRepository.GetByProductAsync(productId);
                var warehouseWithReserve = allStocks
                    .Where(s => s.ReservedQuantity > 0)
                    .OrderByDescending(s => s.ReservedQuantity)
                    .FirstOrDefault();

                if (warehouseWithReserve == null) return; // No reserved stock to release
                targetWarehouseId = warehouseWithReserve.WarehouseId;
            }

            var productWarehouse = await _productWarehouseRepository
                .GetByProductAndWarehouseAsync(productId, targetWarehouseId);

            if (productWarehouse == null) return;

            // Release reserved stock
            productWarehouse.Release(quantity);
            _productWarehouseRepository.Update(productWarehouse);
            await _productWarehouseRepository.SaveChangesAsync();

            // Đồng bộ Product.StockQuantity và FrozenStockQuantity
            await SyncProductStockFromWarehouses(productId);
        }

        public async Task DeductStockForOrderAsync(int productId, int quantity, int orderId, int? warehouseId = null)
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            if (!warehouses.Any())
                throw new DomainException("Chưa có kho nào trong hệ thống");

            int targetWarehouseId;
            if (warehouseId.HasValue)
            {
                targetWarehouseId = warehouseId.Value;
            }
            else
            {
                // Tìm kho có đủ tồn kho đã được giữ chỗ
                var allStocks = await _productWarehouseRepository.GetByProductAsync(productId);
                var warehouseWithReserve = allStocks
                    .Where(s => s.ReservedQuantity >= quantity)
                    .OrderByDescending(s => s.ReservedQuantity)
                    .FirstOrDefault();

                if (warehouseWithReserve == null)
                {
                    // Nếu không có reserved, tìm kho có đủ available stock
                    var availableWarehouse = allStocks
                        .Where(s => s.GetAvailableStock() >= quantity)
                        .OrderByDescending(s => s.GetAvailableStock())
                        .FirstOrDefault();

                    if (availableWarehouse == null)
                        throw new InsufficientStockException(productId, 0, quantity,
                            allStocks.Sum(s => s.GetAvailableStock()));

                    targetWarehouseId = availableWarehouse.WarehouseId;
                }
                else
                {
                    targetWarehouseId = warehouseWithReserve.WarehouseId;
                }
            }

            var productWarehouse = await _productWarehouseRepository
                .GetByProductAndWarehouseAsync(productId, targetWarehouseId);

            if (productWarehouse == null)
                throw new DomainException($"Không tìm thấy tồn kho sản phẩm {productId} trong kho {targetWarehouseId}");

            // Dispatch (deduct) stock from warehouse
            // Note: Dispatch will reduce both Quantity and ReservedQuantity
            productWarehouse.Dispatch(quantity);
            _productWarehouseRepository.Update(productWarehouse);
            await _productWarehouseRepository.SaveChangesAsync();

            // Đồng bộ Product.StockQuantity
            await SyncProductStockFromWarehouses(productId);
        }

        #endregion
    }
}
