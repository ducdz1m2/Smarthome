using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Đăng ký AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Đăng ký các Services
            services.AddScoped<IProductVariantService, ProductVariantService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IBannerService, BannerService>();
            services.AddScoped<IInventoryService, InventoryService>();

            // Installation services
            services.AddScoped<IInstallationService, InstallationService>();
            services.AddScoped<ITechnicianProfileService, TechnicianProfileService>();
            services.AddScoped<IInstallationSlotService, InstallationSlotService>();

            // Warranty services
            services.AddScoped<IWarrantyService, WarrantyService>();

            // Identity services (User, Role, Auth combined)
            // Note: IIdentityService and IdentityService are registered in Web/Program.cs
            // to ensure proper dependency injection order with UserManager/RoleManager

            // Statistics services
            services.AddScoped<IStatisticsService, StatisticsService>();

            // User Address service
            services.AddScoped<IUserAddressService, UserAddressService>();

            return services;
        }
    }
}
