using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IStatisticsService
    {
        Task<DashboardStatisticsResponse> GetDashboardStatisticsAsync();
        Task<SalesStatsResponse> GetSalesStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ProductStatsResponse> GetProductStatisticsAsync();
        Task<UserStatsResponse> GetUserStatisticsAsync();
        Task<InventoryStatsResponse> GetInventoryStatisticsAsync();
        Task<InstallationStatsResponse> GetInstallationStatisticsAsync();
        Task<WarrantyStatsResponse> GetWarrantyStatisticsAsync();
    }
}
