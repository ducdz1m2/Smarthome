namespace Application.Services;

using Application.DTOs.ML;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Entities.Catalog;
using Domain.Entities.Identity;
using Domain.Enums;
using Domain.Repositories;

/// <summary>
/// Service để cung cấp tính năng gợi ý sản phẩm và tìm người dùng tương tự
/// </summary>
public class RecommendationService
{
    private readonly Application.Interfaces.Repositories.IUserBehaviorRepository _userBehaviorRepository;
    private readonly Application.Interfaces.Repositories.IProductRepository _productRepository;
    private readonly IUserSimilarityService _userSimilarityService;
    private readonly Domain.Repositories.IUserRepository _userRepository;

    public RecommendationService(
        Application.Interfaces.Repositories.IUserBehaviorRepository userBehaviorRepository,
        Application.Interfaces.Repositories.IProductRepository productRepository,
        IUserSimilarityService userSimilarityService,
        Domain.Repositories.IUserRepository userRepository)
    {
        _userBehaviorRepository = userBehaviorRepository;
        _productRepository = productRepository;
        _userSimilarityService = userSimilarityService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Lấy danh sách sản phẩm được gợi ý cho người dùng
    /// </summary>
    public async Task<IEnumerable<Product>> GetRecommendedProductsAsync(int userId, int topN = 10)
    {
        // Lấy danh sách tất cả sản phẩm
        var allProducts = await _productRepository.GetAllAsync();
        var activeProducts = allProducts.Where(p => p.IsActive).ToList();
        var productIds = activeProducts.Select(p => (uint)p.Id).ToList();

        // Lấy gợi ý từ ML.NET
        var recommendations = _userSimilarityService.RecommendProducts((uint)userId, productIds, topN);

        // Trả về danh sách Product thực tế
        var recommendedProductIds = recommendations.Select(r => (int)r.ProductId).ToList();
        var products = activeProducts.Where(p => recommendedProductIds.Contains(p.Id)).ToList();

        return products;
    }

    /// <summary>
    /// Lấy danh sách người dùng tương tự với người dùng hiện tại
    /// </summary>
    public async Task<IEnumerable<ApplicationUser>> GetSimilarUsersAsync(int userId, int topN = 10)
    {
        // Lấy danh sách tất cả người dùng
        var query = _userBehaviorRepository.Query();
        var allUsers = query.Select(b => b.UserId).Distinct().ToList();
        var candidateUserIds = allUsers.Where(id => id != userId).Select(id => (uint)id).ToList();

        // Tìm người dùng tương tự từ ML.NET
        var similarUsers = _userSimilarityService.FindSimilarUsers((uint)userId, candidateUserIds, topN);

        // Lấy thông tin ApplicationUser thực tế
        var similarUserIds = similarUsers.Select(s => (int)s.UserId).ToList();
        var users = new List<ApplicationUser>();

        foreach (var similarUserId in similarUserIds)
        {
            var user = await _userRepository.GetByIdAsync(similarUserId);
            if (user != null)
            {
                users.Add(user);
            }
        }

        return users;
    }

    /// <summary>
    /// Train lại model từ dữ liệu hành vi người dùng
    /// </summary>
    public async Task TrainModelAsync(DateTime? fromDate = null)
    {
        // Lấy dữ liệu training từ repository
        var behaviors = await _userBehaviorRepository.GetForTrainingAsync(fromDate);

        // Chuyển đổi sang DTO cho ML.NET
        var trainingData = behaviors.Select(b => new UserProductRatingDto
        {
            UserId = (uint)b.UserId,
            ProductId = (uint)b.ProductId,
            Rating = b.Rating
        }).ToList();

        // Train model
        _userSimilarityService.Train(trainingData);
    }

    /// <summary>
    /// Ghi log hành vi người dùng để phục vụ recommendation
    /// </summary>
    public async Task LogUserBehaviorAsync(int userId, int productId, BehaviorType behaviorType, float rating = 0)
    {
        var behavior = new UserBehavior
        {
            UserId = userId,
            ProductId = productId,
            BehaviorType = behaviorType,
            Timestamp = DateTime.UtcNow,
            Rating = rating
        };

        _userBehaviorRepository.Add(behavior);
        // Note: Cần gọi SaveChangesAsync từ UnitOfWork
    }

    /// <summary>
    /// Tính toán rating từ hành vi người dùng
    /// </summary>
    public float CalculateRatingFromBehavior(BehaviorType behaviorType)
    {
        return behaviorType switch
        {
            BehaviorType.View => 1.0f,
            BehaviorType.AddToCart => 2.0f,
            BehaviorType.Purchase => 5.0f,
            BehaviorType.Review => 4.0f,
            BehaviorType.Favorite => 3.0f,
            BehaviorType.Search => 1.5f,
            _ => 0.0f
        };
    }
}
