namespace Application.DTOs.Responses
{
    public class DashboardStatisticsResponse
    {
        public OverviewStatsResponse Overview { get; set; } = new OverviewStatsResponse();
        public SalesStatsResponse Sales { get; set; } = new SalesStatsResponse();
        public ProductStatsResponse Products { get; set; } = new ProductStatsResponse();
        public UserStatsResponse Users { get; set; } = new UserStatsResponse();
        public InventoryStatsResponse Inventory { get; set; } = new InventoryStatsResponse();
        public InstallationStatsResponse Installation { get; set; } = new InstallationStatsResponse();
        public WarrantyStatsResponse Warranty { get; set; } = new WarrantyStatsResponse();
        public PromotionStatsResponse Promotions { get; set; } = new PromotionStatsResponse();
        public ChartDataResponse Charts { get; set; } = new ChartDataResponse();
    }

    public class OverviewStatsResponse
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public int TotalBrands { get; set; }
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockProducts { get; set; }
    }

    public class SalesStatsResponse
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ReturnedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> OrdersByMonth { get; set; } = new Dictionary<string, int>();
        public List<TopProductResponse> TopSellingProducts { get; set; } = new List<TopProductResponse>();
        public List<PaymentMethodStatResponse> PaymentMethods { get; set; } = new List<PaymentMethodStatResponse>();
    }

    public class ProductStatsResponse
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalBrands { get; set; }
        public Dictionary<string, int> ProductsByCategory { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ProductsByBrand { get; set; } = new Dictionary<string, int>();
        public List<TopProductResponse> MostViewedProducts { get; set; } = new List<TopProductResponse>();
        public List<TopProductResponse> TopRatedProducts { get; set; } = new List<TopProductResponse>();
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
    }

    public class UserStatsResponse
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewUsersToday { get; set; }
        public int ActiveUsers { get; set; }
        public Dictionary<string, int> UsersByMonth { get; set; } = new Dictionary<string, int>();
        public List<TopCustomerResponse> TopCustomers { get; set; } = new List<TopCustomerResponse>();
    }

    public class InventoryStatsResponse
    {
        public int TotalWarehouses { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalStockEntries { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public Dictionary<string, int> StockByWarehouse { get; set; } = new Dictionary<string, int>();
        public List<RecentStockEntryResponse> RecentStockEntries { get; set; } = new List<RecentStockEntryResponse>();
    }

    public class InstallationStatsResponse
    {
        public int TotalTechnicians { get; set; }
        public int ActiveTechnicians { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal TotalInstallationRevenue { get; set; }
        public double AverageTechnicianRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<string, int> BookingsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> BookingsByMonth { get; set; } = new Dictionary<string, int>();
        public List<TopTechnicianResponse> TopTechnicians { get; set; } = new List<TopTechnicianResponse>();
    }

    public class WarrantyStatsResponse
    {
        public int TotalWarranties { get; set; }
        public int ActiveWarranties { get; set; }
        public int ExpiredWarranties { get; set; }
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public int CompletedClaims { get; set; }
        public Dictionary<string, int> ClaimsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> WarrantiesByMonth { get; set; } = new Dictionary<string, int>();
    }

    public class PromotionStatsResponse
    {
        public int TotalBanners { get; set; }
        public int ActiveBanners { get; set; }
        public int TotalCoupons { get; set; }
        public int ActiveCoupons { get; set; }
        public int TotalPromotions { get; set; }
        public int ActivePromotions { get; set; }
        public int UsedCouponsCount { get; set; }
        public decimal TotalDiscountAmount { get; set; }
    }

    public class ChartDataResponse
    {
        public List<ChartDataPointResponse> RevenueChart { get; set; } = new List<ChartDataPointResponse>();
        public List<ChartDataPointResponse> OrdersChart { get; set; } = new List<ChartDataPointResponse>();
        public List<ChartDataPointResponse> UsersChart { get; set; } = new List<ChartDataPointResponse>();
        public List<PieChartDataResponse> OrderStatusChart { get; set; } = new List<PieChartDataResponse>();
        public List<PieChartDataResponse> CategoryDistribution { get; set; } = new List<PieChartDataResponse>();
        public List<PieChartDataResponse> PaymentMethodChart { get; set; } = new List<PieChartDataResponse>();
    }

    public class ChartDataPointResponse
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public int Count { get; set; }
    }

    public class PieChartDataResponse
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class TopProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
        public int SoldCount { get; set; }
        public double? Rating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class PaymentMethodStatResponse
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
        public double Percentage { get; set; }
    }

    public class TopCustomerResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
    }

    public class RecentStockEntryResponse
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime EntryDate { get; set; }
    }

    public class TopTechnicianResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public decimal Rating { get; set; }
        public int CompletedJobs { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
