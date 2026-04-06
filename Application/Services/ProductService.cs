using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities.Catalog;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IMapper _map;

        public ProductService(IProductRepository repo, IMapper map)
        {
            _repo = repo;
            _map = map;
        }

        public async Task<int> CreateAsync(CreateProductRequest request)
        {
            var sku = Sku.Create(request.Sku);
            var price = Money.Create(request.Price);

            var product = Product.Create(
                request.Name,
                sku,
                price,
                request.CategoryId,
                request.BrandId,
                request.SupplierId,
                request.RequiresInstallation
                
            );

            await _repo.AddAsync(product);
            await _repo.SaveChangesAsync();

            return product.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null)
                throw new EntityNotFoundException("Product", id);

            await _repo.DeleteAsync(product);
            await _repo.SaveChangesAsync();
        }

        public async Task<List<ProductResponse>> GetAllAsync()
        {
            var products = await _repo.GetAllAsync();
            return _map.Map<List<ProductResponse>>(products);
        }

        public async Task<List<ProductResponse>> GetByCategoryAsync(int categoryId)
        {
            var products = await _repo.GetByCategoryAsync(categoryId);
            return _map.Map<List<ProductResponse>>(products);
        }

        public async Task<ProductResponse?> GetByIdAsync(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            return _map.Map<ProductResponse>(product);
        }

        public async Task<List<ProductResponse>> SearchAsync(string keyword, string? filters)
        {
            int? categoryId = null;
            if (!string.IsNullOrEmpty(filters))
            {
                // Simple parse, có thể dùng regex hoặc library
                var parts = filters.Split(';');
                foreach (var part in parts)
                {
                    if (part.StartsWith("category:"))
                        categoryId = int.Parse(part.Replace("category:", ""));
                }
            }

            var products = await _repo.SearchAsync(keyword, categoryId);
            return _map.Map<List<ProductResponse>>(products);
        }

        public async Task UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null)
                throw new EntityNotFoundException("Product", id);

            var price = Money.Create(request.Price);
            product.Update(request.Name, price, request.Description);
            // product.Activate/Deactivate nếu cần

            await _repo.SaveChangesAsync();
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await _repo.GetByIdAsync(productId);
            if (product == null)
                return false;

            if (quantity > 0)
            {
                product.AddStock(quantity);
            }
            else if (quantity < 0)
            {
                product.DeductStock(Math.Abs(quantity));
            }

            await _repo.SaveChangesAsync();
            return true;
        }

    }
}
