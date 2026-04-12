using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Entities.Identity;
using Domain.Entities.Installation;
using Domain.Entities.Sales;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IStockEntryRepository _stockEntryRepository;
        private readonly IInstallationBookingRepository _installationBookingRepository;
        private readonly ITechnicianProfileRepository _technicianProfileRepository;
        private readonly IWarrantyRepository _warrantyRepository;
        private readonly IWarrantyClaimRepository _warrantyClaimRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IBannerRepository _bannerRepository;

        public StatisticsService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            UserManager<ApplicationUser> userManager,
            IWarehouseRepository warehouseRepository,
            ISupplierRepository supplierRepository,
            IStockEntryRepository stockEntryRepository,
            IInstallationBookingRepository installationBookingRepository,
            ITechnicianProfileRepository technicianProfileRepository,
            IWarrantyRepository warrantyRepository,
            IWarrantyClaimRepository warrantyClaimRepository,
            ICouponRepository couponRepository,
            IPromotionRepository promotionRepository,
            IBannerRepository bannerRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _userManager = userManager;
            _warehouseRepository = warehouseRepository;
            _supplierRepository = supplierRepository;
            _stockEntryRepository = stockEntryRepository;
            _installationBookingRepository = installationBookingRepository;
            _technicianProfileRepository = technicianProfileRepository;
            _warrantyRepository = warrantyRepository;
            _warrantyClaimRepository = warrantyClaimRepository;
            _couponRepository = couponRepository;
            _promotionRepository = promotionRepository;
            _bannerRepository = bannerRepository;
        }

        public async Task<DashboardStatisticsResponse> GetDashboardStatisticsAsync()
        {
            var today = DateTime.Now.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            return new DashboardStatisticsResponse
            {
                Overview = await GetOverviewStatsAsync(today, firstDayOfMonth),
                Sales = await GetSalesStatisticsAsync(),
                Products = await GetProductStatisticsAsync(),
                Users = await GetUserStatisticsAsync(),
                Inventory = await GetInventoryStatisticsAsync(),
                Installation = await GetInstallationStatisticsAsync(),
                Warranty = await GetWarrantyStatisticsAsync(),
                Promotions = await GetPromotionStatisticsAsync(),
                Charts = await GetChartDataAsync()
            };
        }

        private async Task<OverviewStatsResponse> GetOverviewStatsAsync(DateTime today, DateTime firstDayOfMonth)
        {
            var orders = await _orderRepository.GetAllAsync();
            var totalOrders = orders.Count;
            var totalRevenue = orders
                .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered)
                .Sum(o => o.TotalAmount);

            var todayOrders = orders.Count(o => o.CreatedAt.Date == today);
            var todayRevenue = orders
                .Where(o => o.CreatedAt.Date == today && (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered))
                .Sum(o => o.TotalAmount);

            var products = await _productRepository.GetAllAsync();
            var lowStockThreshold = 10;
            var lowStockProducts = products.Count(p => p.StockQuantity > 0 && p.StockQuantity < lowStockThreshold && p.IsActive);

            return new OverviewStatsResponse
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalProducts = products.Count,
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalBrands = await _brandRepository.CountAsync(),
                TodayOrders = todayOrders,
                TodayRevenue = todayRevenue,
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                LowStockProducts = lowStockProducts
            };
        }

        public async Task<SalesStatsResponse> GetSalesStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var orders = await _orderRepository.GetAllAsync();
            
            if (fromDate.HasValue)
                orders = orders.Where(o => o.CreatedAt >= fromDate.Value).ToList();
            if (toDate.HasValue)
                orders = orders.Where(o => o.CreatedAt <= toDate.Value).ToList();

            var deliveredOrders = orders.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered).ToList();

            var ordersByStatus = orders
                .GroupBy(o => o.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Select(d => new { Year = d.Year, Month = d.Month, Key = $"{d.Month:00}/{d.Year}" })
                .Reverse()
                .ToList();

            var revenueByMonth = new Dictionary<string, decimal>();
            var ordersByMonth = new Dictionary<string, int>();

            foreach (var month in last6Months)
            {
                var monthOrders = orders.Where(o => o.CreatedAt.Year == month.Year && o.CreatedAt.Month == month.Month).ToList();
                ordersByMonth[month.Key] = monthOrders.Count;
                revenueByMonth[month.Key] = monthOrders
                    .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered)
                    .Sum(o => o.TotalAmount);
            }

            var paymentMethodsData = orders
                .GroupBy(o => o.PaymentMethod.ToString())
                .Select(g => new PaymentMethodStatResponse
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Revenue = g.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount)
                }).ToList();

            var totalRevenue = paymentMethodsData.Sum(p => p.Revenue);
            foreach (var method in paymentMethodsData)
            {
                method.Percentage = totalRevenue > 0 ? (double)(method.Revenue / totalRevenue * 100) : 0;
            }

            // Top selling products - simplified version
            var products = await _productRepository.GetAllAsync();
            var topProducts = products
                .Take(10)
                .Select(p => new TopProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.Images?.FirstOrDefault()?.Url,
                    Quantity = p.StockQuantity,
                    Revenue = p.BasePrice * 10, // Simplified
                    SoldCount = 10 // Simplified
                })
                .OrderByDescending(p => p.SoldCount)
                .ToList();

            return new SalesStatsResponse
            {
                TotalOrders = orders.Count,
                TotalRevenue = deliveredOrders.Sum(o => o.TotalAmount),
                AverageOrderValue = deliveredOrders.Any() ? deliveredOrders.Average(o => o.TotalAmount) : 0,
                CompletedOrders = deliveredOrders.Count,
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                ReturnedOrders = orders.Count(o => o.Status == OrderStatus.Refunded || o.Status == OrderStatus.ReturnRequested),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.AwaitingPickup),
                ShippingOrders = orders.Count(o => o.Status == OrderStatus.Shipping),
                DeliveredOrders = deliveredOrders.Count,
                OrdersByStatus = ordersByStatus,
                RevenueByMonth = revenueByMonth,
                OrdersByMonth = ordersByMonth,
                TopSellingProducts = topProducts,
                PaymentMethods = paymentMethodsData
            };
        }

        public async Task<ProductStatsResponse> GetProductStatisticsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();

            var productsByCategory = products
                .GroupBy(p => categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            var productsByBrand = products
                .GroupBy(p => brands.FirstOrDefault(b => b.Id == p.BrandId)?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            var lowStockThreshold = 10;

            // Top rated products
            var topRated = products
                .Where(p => p.Comments?.Any() == true)
                .OrderByDescending(p => p.Comments?.Average(c => (double?)c.Rating) ?? 0)
                .Take(10)
                .Select(p => new TopProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.Images?.FirstOrDefault()?.Url,
                    Rating = p.Comments?.Average(c => (double?)c.Rating) ?? 0,
                    ReviewCount = p.Comments?.Count ?? 0
                }).ToList();

            var totalReviews = products.Sum(p => p.Comments?.Count ?? 0);
            var averageRating = products.Where(p => p.Comments?.Any() == true).SelectMany(p => p.Comments ?? new List<ProductComment>()).Any()
                ? products.SelectMany(p => p.Comments ?? new List<ProductComment>()).Average(c => (double?)c.Rating) ?? 0
                : 0;

            return new ProductStatsResponse
            {
                TotalProducts = products.Count,
                ActiveProducts = products.Count(p => p.IsActive),
                InactiveProducts = products.Count(p => !p.IsActive),
                OutOfStockProducts = products.Count(p => p.StockQuantity == 0),
                LowStockProducts = products.Count(p => p.StockQuantity > 0 && p.StockQuantity < lowStockThreshold),
                TotalCategories = categories.Count,
                TotalBrands = brands.Count,
                ProductsByCategory = productsByCategory,
                ProductsByBrand = productsByBrand,
                TopRatedProducts = topRated,
                TotalReviews = totalReviews,
                AverageRating = Math.Round(averageRating, 1)
            };
        }

        public async Task<UserStatsResponse> GetUserStatisticsAsync()
        {
            var today = DateTime.Now.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            var users = await _userManager.Users.ToListAsync();
            var orders = await _orderRepository.GetAllAsync();

            var newUsersThisMonth = users.Count(u => u.CreatedAt >= firstDayOfMonth);
            var newUsersToday = users.Count(u => u.CreatedAt.Date == today);

            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Select(d => new { Year = d.Year, Month = d.Month, Key = $"{d.Month:00}/{d.Year}" })
                .Reverse()
                .ToList();

            var usersByMonth = new Dictionary<string, int>();
            foreach (var month in last6Months)
            {
                var count = users.Count(u => u.CreatedAt.Year == month.Year && u.CreatedAt.Month == month.Month);
                usersByMonth[month.Key] = count;
            }

            // Top customers
            var topCustomers = users
                .Select(u => new
                {
                    User = u,
                    UserOrders = orders.Where(o => o.UserId == u.Id).ToList()
                })
                .Where(x => x.UserOrders.Any())
                .Select(x => new TopCustomerResponse
                {
                    Id = x.User.Id,
                    FullName = x.User.FullName,
                    Email = x.User.Email ?? string.Empty,
                    Avatar = x.User.Avatar,
                    OrderCount = x.UserOrders.Count,
                    TotalSpent = x.UserOrders
                        .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered)
                        .Sum(o => o.TotalAmount),
                    LastOrderDate = x.UserOrders.Max(o => o.CreatedAt)
                })
                .Where(c => c.TotalSpent > 0)
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToList();

            var activeUsers = users.Count(u => orders.Any(o => o.UserId == u.Id && o.CreatedAt >= oneMonthAgo));

            return new UserStatsResponse
            {
                TotalUsers = users.Count,
                NewUsersThisMonth = newUsersThisMonth,
                NewUsersToday = newUsersToday,
                ActiveUsers = activeUsers,
                UsersByMonth = usersByMonth,
                TopCustomers = topCustomers
            };
        }

        public async Task<InventoryStatsResponse> GetInventoryStatisticsAsync()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            var suppliers = await _supplierRepository.GetAllAsync();
            var stockEntries = await _stockEntryRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();

            var firstWarehouseName = warehouses.FirstOrDefault()?.Name ?? "N/A";

            var stockByWarehouse = new Dictionary<string, int>();
            foreach (var warehouse in warehouses)
            {
                stockByWarehouse[warehouse.Name] = products.Sum(p => p.StockQuantity);
            }
            if (!stockByWarehouse.Any())
            {
                stockByWarehouse["Chưa có kho"] = products.Sum(p => p.StockQuantity);
            }

            var totalValue = products.Sum(p => p.StockQuantity * p.BasePrice);

            return new InventoryStatsResponse
            {
                TotalWarehouses = warehouses.Count,
                TotalSuppliers = suppliers.Count,
                TotalStockEntries = stockEntries.Count,
                TotalInventoryValue = totalValue,
                LowStockCount = products.Count(p => p.StockQuantity > 0 && p.StockQuantity < 10 && p.IsActive),
                OutOfStockCount = products.Count(p => p.StockQuantity == 0 && p.IsActive),
                StockByWarehouse = stockByWarehouse,
                RecentStockEntries = new List<RecentStockEntryResponse>() // Simplified
            };
        }

        public async Task<InstallationStatsResponse> GetInstallationStatisticsAsync()
        {
            var bookings = await _installationBookingRepository.GetAllAsync();
            var technicians = await _technicianProfileRepository.GetAllAsync();

            var bookingsByStatus = bookings
                .GroupBy(b => b.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Select(d => new { Year = d.Year, Month = d.Month, Key = $"{d.Month:00}/{d.Year}" })
                .Reverse()
                .ToList();

            var bookingsByMonth = new Dictionary<string, int>();
            foreach (var month in last6Months)
            {
                var count = bookings.Count(b => b.CreatedAt.Year == month.Year && b.CreatedAt.Month == month.Month);
                bookingsByMonth[month.Key] = count;
            }

            var completedBookings = bookings.Count(b => b.Status == InstallationStatus.Completed || b.Status == InstallationStatus.Testing);
            var pendingBookings = bookings.Count(b => b.Status == InstallationStatus.Pending || b.Status == InstallationStatus.Confirmed);
            var cancelledBookings = bookings.Count(b => b.Status == InstallationStatus.Cancelled || b.Status == InstallationStatus.Failed);

            var topTechnicians = technicians
                .OrderByDescending(t => t.CompletedJobs)
                .Take(10)
                .Select(t => new TopTechnicianResponse
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    PhoneNumber = t.PhoneNumber,
                    Rating = (decimal)t.Rating,
                    CompletedJobs = t.CompletedJobs,
                    TotalRevenue = 0 // Simplified - InstallationBooking không có Fee
                }).ToList();

            return new InstallationStatsResponse
            {
                TotalTechnicians = technicians.Count,
                ActiveTechnicians = technicians.Count(t => t.IsAvailable),
                TotalBookings = bookings.Count,
                PendingBookings = pendingBookings,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings,
                TotalInstallationRevenue = 0, // InstallationBooking không có Fee property
                AverageTechnicianRating = technicians.Any() ? (double)technicians.Average(t => t.Rating) : 0,
                TotalReviews = 0, // Simplified
                BookingsByStatus = bookingsByStatus,
                BookingsByMonth = bookingsByMonth,
                TopTechnicians = topTechnicians
            };
        }

        public async Task<WarrantyStatsResponse> GetWarrantyStatisticsAsync()
        {
            var warranties = await _warrantyRepository.GetAllAsync();
            var claims = await _warrantyClaimRepository.GetAllAsync();

            var claimsByStatus = claims
                .GroupBy(c => c.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Select(d => new { Year = d.Year, Month = d.Month, Key = $"{d.Month:00}/{d.Year}" })
                .Reverse()
                .ToList();

            var warrantiesByMonth = new Dictionary<string, int>();
            foreach (var month in last6Months)
            {
                var count = warranties.Count(w => w.CreatedAt.Year == month.Year && w.CreatedAt.Month == month.Month);
                warrantiesByMonth[month.Key] = count;
            }

            return new WarrantyStatsResponse
            {
                TotalWarranties = warranties.Count,
                ActiveWarranties = warranties.Count(w => w.Status == WarrantyStatus.Active),
                ExpiredWarranties = warranties.Count(w => w.Status == WarrantyStatus.Expired),
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => c.Status == WarrantyClaimStatus.Pending),
                ApprovedClaims = claims.Count(c => c.Status == WarrantyClaimStatus.Resolved || c.Status == WarrantyClaimStatus.ReplacementApproved),
                RejectedClaims = claims.Count(c => c.Status == WarrantyClaimStatus.Rejected),
                CompletedClaims = claims.Count(c => c.Status == WarrantyClaimStatus.Resolved || c.Status == WarrantyClaimStatus.ReplacementApproved),
                ClaimsByStatus = claimsByStatus,
                WarrantiesByMonth = warrantiesByMonth
            };
        }

        private async Task<PromotionStatsResponse> GetPromotionStatisticsAsync()
        {
            var coupons = await _couponRepository.GetAllAsync();
            var banners = await _bannerRepository.GetAllAsync();
            var promotions = await _promotionRepository.GetAllAsync();
            var now = DateTime.Now;

            return new PromotionStatsResponse
            {
                TotalBanners = banners.Count,
                ActiveBanners = banners.Count(b => b.IsActive),
                TotalCoupons = coupons.Count,
                ActiveCoupons = coupons.Count(c => c.IsActive && c.ExpiryDate >= now),
                TotalPromotions = promotions.Count,
                ActivePromotions = promotions.Count(p => p.StartDate <= now && p.EndDate >= now),
                UsedCouponsCount = coupons.Sum(c => c.UsedCount),
                TotalDiscountAmount = coupons.Sum(c => c.DiscountValue * c.UsedCount)
            };
        }

        private async Task<ChartDataResponse> GetChartDataAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();
            var categories = await _categoryRepository.GetAllAsync();

            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Select(d => new { Year = d.Year, Month = d.Month, Label = $"{d.Month:00}/{d.Year}" })
                .Reverse()
                .ToList();

            var revenueChart = new List<ChartDataPointResponse>();
            var ordersChart = new List<ChartDataPointResponse>();
            var usersChart = new List<ChartDataPointResponse>();

            foreach (var month in last6Months)
            {
                var monthOrders = orders.Where(o => o.CreatedAt.Year == month.Year && o.CreatedAt.Month == month.Month).ToList();
                var newUsers = users.Count(u => u.CreatedAt.Year == month.Year && u.CreatedAt.Month == month.Month);

                revenueChart.Add(new ChartDataPointResponse
                {
                    Label = month.Label,
                    Value = monthOrders.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount),
                    Count = monthOrders.Count
                });

                ordersChart.Add(new ChartDataPointResponse
                {
                    Label = month.Label,
                    Count = monthOrders.Count
                });

                usersChart.Add(new ChartDataPointResponse
                {
                    Label = month.Label,
                    Count = newUsers
                });
            }

            var orderStatusData = orders
                .GroupBy(o => o.Status.ToString())
                .Select(g => new PieChartDataResponse
                {
                    Label = g.Key,
                    Value = g.Count(),
                    Color = GetOrderStatusColor(g.Key)
                }).ToList();

            var products = await _productRepository.GetAllAsync();
            var categoryData = categories
                .Select(c => new PieChartDataResponse
                {
                    Label = c.Name,
                    Value = products.Count(p => p.CategoryId == c.Id),
                    Color = GetRandomColor(c.Id)
                })
                .Where(c => c.Value > 0)
                .ToList();

            var paymentData = orders
                .GroupBy(o => o.PaymentMethod.ToString())
                .Select(g => new PieChartDataResponse
                {
                    Label = g.Key,
                    Value = g.Count(),
                    Color = GetRandomColor(g.Key.GetHashCode())
                }).ToList();

            return new ChartDataResponse
            {
                RevenueChart = revenueChart,
                OrdersChart = ordersChart,
                UsersChart = usersChart,
                OrderStatusChart = orderStatusData,
                CategoryDistribution = categoryData,
                PaymentMethodChart = paymentData
            };
        }

        private string GetOrderStatusColor(string status)
        {
            return status switch
            {
                "Pending" => "#f59e0b",
                "Confirmed" => "#3b82f6",
                "AwaitingPickup" => "#8b5cf6",
                "Shipping" => "#06b6d4",
                "Delivered" => "#10b981",
                "Completed" => "#10b981",
                "Cancelled" => "#ef4444",
                "Refunded" => "#f97316",
                "ReturnRequested" => "#f97316",
                _ => "#6b7280"
            };
        }

        private string GetRandomColor(int seed)
        {
            var colors = new[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#06b6d4", "#f97316", "#84cc16", "#ec4899", "#6366f1" };
            return colors[Math.Abs(seed) % colors.Length];
        }
    }
}
