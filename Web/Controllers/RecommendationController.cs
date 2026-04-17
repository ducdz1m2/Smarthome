using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Enums;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationController : ControllerBase
{
    private readonly RecommendationService _recommendationService;
    private readonly ILogger<RecommendationController> _logger;

    public RecommendationController(RecommendationService recommendationService, ILogger<RecommendationController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách sản phẩm được gợi ý cho người dùng hiện tại
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetRecommendedProducts([FromQuery] int topN = 10)
    {
        try
        {
            // Lấy userId từ claims
            if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
            {
                return Unauthorized();
            }

            var recommendedProducts = await _recommendationService.GetRecommendedProductsAsync(userId, topN);
            return Ok(recommendedProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy sản phẩm gợi ý");
            return StatusCode(500, "Có lỗi xảy ra khi lấy sản phẩm gợi ý");
        }
    }

    /// <summary>
    /// Lấy danh sách người dùng tương tự với người dùng hiện tại
    /// </summary>
    [HttpGet("similar-users")]
    public async Task<IActionResult> GetSimilarUsers([FromQuery] int topN = 10)
    {
        try
        {
            // Lấy userId từ claims
            if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
            {
                return Unauthorized();
            }

            var similarUsers = await _recommendationService.GetSimilarUsersAsync(userId, topN);
            return Ok(similarUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy người dùng tương tự");
            return StatusCode(500, "Có lỗi xảy ra khi lấy người dùng tương tự");
        }
    }

    /// <summary>
    /// Ghi log hành vi người dùng
    /// </summary>
    [HttpPost("log-behavior")]
    public async Task<IActionResult> LogUserBehavior([FromBody] LogBehaviorRequest request)
    {
        try
        {
            // Lấy userId từ claims
            if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
            {
                return Unauthorized();
            }

            var rating = _recommendationService.CalculateRatingFromBehavior(request.BehaviorType);
            await _recommendationService.LogUserBehaviorAsync(userId, request.ProductId, request.BehaviorType, rating);

            return Ok(new { message = "Đã ghi log hành vi thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi ghi log hành vi người dùng");
            return StatusCode(500, "Có lỗi xảy ra khi ghi log hành vi");
        }
    }

    /// <summary>
    /// Train lại model recommendation
    /// </summary>
    [HttpPost("train-model")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TrainModel([FromQuery] DateTime? fromDate = null)
    {
        try
        {
            await _recommendationService.TrainModelAsync(fromDate);
            return Ok(new { message = "Đã train model thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi train model");
            return StatusCode(500, "Có lỗi xảy ra khi train model");
        }
    }
}

public class LogBehaviorRequest
{
    public int ProductId { get; set; }
    public BehaviorType BehaviorType { get; set; }
}
