using Application.DTOs.Requests;
using Application.DTOs.Responses;
using System.Net.Http.Json;

namespace Web.Services
{
    /// <summary>
    /// Service for calling Installation API endpoints from Blazor components
    /// </summary>
    public class InstallationApiService
    {
        private readonly HttpClient _httpClient;

        public InstallationApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get warehouses with available stock for products
        /// </summary>
        public async Task<List<WarehouseStockForTechnicianResponse>> GetWarehousesForProductsAsync(List<int> productIds)
        {
            var queryString = string.Join("&", productIds.Select(id => $"productIds={id}"));
            var response = await _httpClient.GetAsync($"api/installation/warehouses/for-products?{queryString}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<WarehouseStockForTechnicianResponse>>() ?? new();
        }

        /// <summary>
        /// Get product stock across all warehouses
        /// </summary>
        public async Task<List<ProductStockForTechnician>> GetProductStockAsync(int productId)
        {
            var response = await _httpClient.GetAsync($"api/installation/stock/{productId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ProductStockForTechnician>>() ?? new();
        }

        /// <summary>
        /// Prepare materials from warehouse for a booking
        /// </summary>
        public async Task PrepareMaterialsAsync(int bookingId, PrepareMaterialsRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/prepare-materials", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Return unused materials to warehouse
        /// </summary>
        public async Task ReturnMaterialsAsync(int bookingId, List<MaterialReturnInfo> returns)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/return-materials", returns);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Add a single material to booking
        /// </summary>
        public async Task AddMaterialAsync(int bookingId, AddInstallationMaterialRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/materials", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Accept booking
        /// </summary>
        public async Task AcceptBookingAsync(int bookingId, int technicianId)
        {
            var response = await _httpClient.PostAsync($"api/installation/{bookingId}/accept?technicianId={technicianId}", null);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Reject booking
        /// </summary>
        public async Task RejectBookingAsync(int bookingId, int technicianId, RejectBookingRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/reject?technicianId={technicianId}", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Start travel
        /// </summary>
        public async Task StartTravelAsync(int bookingId)
        {
            var response = await _httpClient.PostAsync($"api/installation/{bookingId}/start-travel", null);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Start installation
        /// </summary>
        public async Task StartInstallationAsync(int bookingId)
        {
            var response = await _httpClient.PostAsync($"api/installation/{bookingId}/start-installation", null);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Complete installation
        /// </summary>
        public async Task CompleteAsync(int bookingId, CompleteInstallationRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/installation/{bookingId}/complete", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Get booking by id
        /// </summary>
        public async Task<InstallationBookingResponse?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/installation/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<InstallationBookingResponse>();
        }
    }
}
