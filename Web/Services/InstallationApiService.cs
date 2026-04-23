using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Web.Services
{
    /// <summary>
    /// Service for calling Installation API endpoints from Blazor components
    /// </summary>
    public class InstallationApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JwtTokenHandler _tokenHandler;
        private readonly NavigationManager _navigationManager;

        public InstallationApiService(HttpClient httpClient, JwtTokenHandler tokenHandler, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _tokenHandler = tokenHandler;
            _navigationManager = navigationManager;
            
            // Ensure BaseAddress is set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_navigationManager.BaseUri);
            }
        }

        private async Task AddAuthTokenAsync()
        {
            try
            {
                var token = await _tokenHandler.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch
            {
                // Ignore token errors - will get 401 if auth is required
            }
        }

        /// <summary>
        /// Get warehouses with available stock for products
        /// </summary>
        public async Task<List<WarehouseStockForTechnicianResponse>> GetWarehousesForProductsAsync(
            List<int> productIds,
            string? city = null,
            string? district = null)
        {
            await AddAuthTokenAsync();
            var queryString = string.Join("&", productIds.Select(id => $"productIds={id}"));
            if (!string.IsNullOrWhiteSpace(city))
                queryString += $"&city={Uri.EscapeDataString(city)}";
            if (!string.IsNullOrWhiteSpace(district))
                queryString += $"&district={Uri.EscapeDataString(district)}";
            var response = await _httpClient.GetAsync($"api/installation/warehouses/for-products?{queryString}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<WarehouseStockForTechnicianResponse>>() ?? new();
        }

        /// <summary>
        /// Get product stock across all warehouses
        /// </summary>
        public async Task<List<ProductStockForTechnician>> GetProductStockAsync(int productId)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.GetAsync($"api/installation/stock/{productId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ProductStockForTechnician>>() ?? new();
        }

        /// <summary>
        /// Prepare materials from warehouse for a booking
        /// </summary>
        public async Task PrepareMaterialsAsync(int bookingId, PrepareMaterialsRequest request)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/prepare-materials", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Return unused materials to warehouse
        /// </summary>
        public async Task ReturnMaterialsAsync(int bookingId, List<MaterialReturnInfo> returns)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/return-materials", returns);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Add a single material to booking
        /// </summary>
        public async Task AddMaterialAsync(int bookingId, AddInstallationMaterialRequest request)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/materials", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Accept booking
        /// </summary>
        public async Task AcceptBookingAsync(int bookingId, int technicianId)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.PostAsync($"api/installation/{bookingId}/accept?technicianId={technicianId}", null);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Reject booking
        /// </summary>
        public async Task RejectBookingAsync(int bookingId, int technicianId, RejectBookingRequest request)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/reject?technicianId={technicianId}", request);
            response.EnsureSuccessStatusCode();
        }

        public async Task ReportOutOfStockAsync(int bookingId, int technicianId)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.PostAsync($"api/installation/{bookingId}/report-out-of-stock?technicianId={technicianId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task ResetFromAwaitingMaterialAsync(int bookingId, DateTime? newScheduledDate = null)
        {
            await AddAuthTokenAsync();
            var request = new ResetFromAwaitingMaterialRequest { NewScheduledDate = newScheduledDate };
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/reset-from-awaiting-material", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Mark booking as failed
        /// </summary>
        public async Task FailBookingAsync(int bookingId, string reason)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/fail", new { Reason = reason });
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Start travel
        /// </summary>
        public async Task StartTravelAsync(int bookingId)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.PostAsync($"api/installation/{bookingId}/start-travel", null);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Start installation
        /// </summary>
        public async Task StartInstallationAsync(int bookingId)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.PostAsync($"api/installation/{bookingId}/start-installation", null);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Complete installation
        /// </summary>
        public async Task CompleteAsync(int bookingId, CompleteInstallationRequest request)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/complete", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Get booking by id
        /// </summary>
        public async Task<InstallationBookingResponse?> GetByIdAsync(int id)
        {
            await AddAuthTokenAsync();
            var response = await _httpClient.GetAsync($"api/installation/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<InstallationBookingResponse>();
        }
    }
}
