namespace Web.Services;

using Application.DTOs.Responses;
using Domain.Enums;

public class RecommendationApiService
{
    private readonly HttpClient _httpClient;
    private readonly JwtTokenHandler _tokenHandler;

    public RecommendationApiService(HttpClient httpClient, JwtTokenHandler tokenHandler)
    {
        _httpClient = httpClient;
        _tokenHandler = tokenHandler;
    }

    public async Task LogUserBehaviorAsync(int productId, BehaviorType behaviorType)
    {
        try
        {
            var token = await _tokenHandler.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return; // Not logged in, don't log
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                ProductId = productId,
                BehaviorType = behaviorType.ToString()
            };

            var response = await _httpClient.PostAsJsonAsync("/api/recommendation/log-behavior", request);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Silently fail - logging behavior shouldn't break the app
        }
    }

    public async Task<List<ProductListResponse>> GetRecommendedProductsAsync(int topN = 10)
    {
        try
        {
            var token = await _tokenHandler.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new List<ProductListResponse>();
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"/api/recommendation/products?topN={topN}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductListResponse>>() ?? new List<ProductListResponse>();
            }
        }
        catch
        {
            // Silently fail
        }
        return new List<ProductListResponse>();
    }
}
