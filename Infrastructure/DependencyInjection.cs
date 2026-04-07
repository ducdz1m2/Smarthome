using Application.Interfaces.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Cấu hình DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký các Repositories
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IBannerRepository, BannerRepository>();
            services.AddScoped<IStockEntryRepository, StockEntryRepository>();
            services.AddScoped<IProductWarehouseRepository, ProductWarehouseRepository>();

            // Installation repositories
            services.AddScoped<IInstallationBookingRepository, InstallationBookingRepository>();
            services.AddScoped<ITechnicianProfileRepository, TechnicianProfileRepository>();
            services.AddScoped<IInstallationSlotRepository, InstallationSlotRepository>();

            return services;
        }
    }
}
