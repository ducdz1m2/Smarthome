using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Promotions;
using Domain.Exceptions;

namespace Application.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IProductRepository _productRepository;

        public PromotionService(IPromotionRepository promotionRepository, IProductRepository productRepository)
        {
            _promotionRepository = promotionRepository;
            _productRepository = productRepository;
        }

        public async Task<List<PromotionResponse>> GetAllAsync()
        {
            var promotions = await _promotionRepository.GetAllAsync();
            var tasks = promotions.Select(MapToResponseAsync);
            var responses = await Task.WhenAll(tasks);
            return responses.ToList();
        }

        public async Task<PromotionResponse?> GetByIdAsync(int id)
        {
            var promotion = await _promotionRepository.GetByIdWithProductsAsync(id);
            if (promotion == null) return null;
            return await MapToResponseAsync(promotion);
        }

        public async Task<int> CreateAsync(CreatePromotionRequest request)
        {
            if (await _promotionRepository.ExistsAsync(request.Name))
                throw new DomainException("Tên chương trình khuyến mãi đã tồn tại");

            var promotion = Promotion.Create(
                request.Name,
                request.DiscountPercent,
                request.StartDate,
                request.EndDate
            );

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                typeof(Promotion).GetProperty("Description")?.SetValue(promotion, request.Description.Trim());
            }

            if (request.MinOrderAmount.HasValue)
            {
                typeof(Promotion).GetProperty("MinOrderAmount")?.SetValue(promotion, request.MinOrderAmount.Value);
            }

            if (request.Priority != 0)
            {
                typeof(Promotion).GetProperty("Priority")?.SetValue(promotion, request.Priority);
            }

            // Save promotion first to get the Id
            await _promotionRepository.AddAsync(promotion);
            await _promotionRepository.SaveChangesAsync();

            // Now add products with the valid promotion Id
            foreach (var productId in request.ProductIds)
            {
                if (!await _productRepository.ExistsAsync(productId))
                    throw new DomainException($"Sản phẩm ID {productId} không tồn tại");

                decimal? customDiscount = null;
                if (request.CustomDiscounts.TryGetValue(productId, out var cd))
                    customDiscount = cd;

                promotion.AddProduct(productId, customDiscount.HasValue ? Domain.ValueObjects.Percentage.Create(customDiscount.Value) : null);
            }

            // Save again if there are products added
            if (request.ProductIds.Any())
            {
                await _promotionRepository.SaveChangesAsync();
            }

            return promotion.Id;
        }

        public async Task UpdateAsync(int id, UpdatePromotionRequest request)
        {
            var promotion = await _promotionRepository.GetByIdWithProductsAsync(id);
            if (promotion == null)
                throw new DomainException("Không tìm thấy chương trình khuyến mãi");

            if (await _promotionRepository.ExistsAsync(request.Name, id))
                throw new DomainException("Tên chương trình khuyến mãi đã tồn tại");

            // Update basic info using reflection since properties are private
            typeof(Promotion).GetProperty("Name")?.SetValue(promotion, request.Name.Trim());
            
            if (!string.IsNullOrWhiteSpace(request.Description))
                typeof(Promotion).GetProperty("Description")?.SetValue(promotion, request.Description.Trim());
            else
                typeof(Promotion).GetProperty("Description")?.SetValue(promotion, null);

            typeof(Promotion).GetProperty("DiscountPercent")?.SetValue(promotion, Domain.ValueObjects.Percentage.Create(request.DiscountPercent));
            typeof(Promotion).GetProperty("MinOrderAmount")?.SetValue(promotion, request.MinOrderAmount.HasValue ? Domain.ValueObjects.Money.Create(request.MinOrderAmount.Value) : null);
            typeof(Promotion).GetProperty("Priority")?.SetValue(promotion, request.Priority);
            typeof(Promotion).GetProperty("IsActive")?.SetValue(promotion, request.IsActive);

            // Update period
            promotion.UpdatePeriod(request.StartDate, request.EndDate);

            // Update products - clear existing and add new
            var existingProducts = promotion.PromotionProducts.ToList();
            foreach (var pp in existingProducts)
            {
                promotion.PromotionProducts.Remove(pp);
            }

            foreach (var productId in request.ProductIds)
            {
                if (!await _productRepository.ExistsAsync(productId))
                    throw new DomainException($"Sản phẩm ID {productId} không tồn tại");

                decimal? customDiscount = null;
                if (request.CustomDiscounts.TryGetValue(productId, out var cd))
                    customDiscount = cd;

                promotion.AddProduct(productId, customDiscount.HasValue ? Domain.ValueObjects.Percentage.Create(customDiscount.Value) : null);
            }

            _promotionRepository.Update(promotion);
            await _promotionRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var promotion = await _promotionRepository.GetByIdAsync(id);
            if (promotion == null)
                throw new DomainException("Không tìm thấy chương trình khuyến mãi");

            _promotionRepository.Delete(promotion);
            await _promotionRepository.SaveChangesAsync();
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var promotion = await _promotionRepository.GetByIdAsync(id);
            if (promotion == null) return false;

            typeof(Promotion).GetProperty("IsActive")?.SetValue(promotion, true);
            _promotionRepository.Update(promotion);
            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var promotion = await _promotionRepository.GetByIdAsync(id);
            if (promotion == null) return false;

            promotion.Deactivate();
            _promotionRepository.Update(promotion);
            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<PromotionResponse>> GetActiveAsync()
        {
            var promotions = await _promotionRepository.GetActiveAsync();
            var tasks = promotions.Select(MapToResponseAsync);
            var responses = await Task.WhenAll(tasks);
            return responses.ToList();
        }

        public async Task<List<PromotionResponse>> GetActiveForProductAsync(int productId)
        {
            var promotions = await _promotionRepository.GetActiveForProductAsync(productId);
            var tasks = promotions.Select(MapToResponseAsync);
            var responses = await Task.WhenAll(tasks);
            return responses.ToList();
        }

        public async Task<decimal> CalculateDiscountAsync(int promotionId, decimal originalPrice, int? productId = null)
        {
            var promotion = await _promotionRepository.GetByIdWithProductsAsync(promotionId);
            if (promotion == null)
                throw new DomainException("Không tìm thấy chương trình khuyến mãi");

            return promotion.CalculateDiscount(originalPrice, productId);
        }

        private async Task<PromotionResponse> MapToResponseAsync(Promotion promotion)
        {
            var response = new PromotionResponse
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Description = promotion.Description,
                DiscountPercent = promotion.DiscountPercent.Value,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                MinOrderAmount = promotion.MinOrderAmount?.Amount,
                IsActive = promotion.IsActive,
                Priority = promotion.Priority,
                CreatedAt = promotion.CreatedAt,
                Products = new List<PromotionProductDto>()
            };

            if (promotion.PromotionProducts?.Any() == true)
            {
                foreach (var pp in promotion.PromotionProducts)
                {
                    var product = await _productRepository.GetByIdAsync(pp.ProductId);
                    response.Products.Add(new PromotionProductDto
                    {
                        ProductId = pp.ProductId,
                        ProductName = product?.Name ?? $"Sản phẩm #{pp.ProductId}",
                        CustomDiscountPercent = pp.CustomDiscountPercent?.Value
                    });
                }
            }

            return response;
        }
    }
}
