using Application.Interfaces.Repositories;
using Domain.Entities.Identity;
using Domain.Events;
using Infrastructure.Data;
using Infrastructure.Events;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
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

            // Đăng ký Domain Event Dispatcher
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Đăng ký Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Đăng ký các Repositories
            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
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
            services.AddScoped<Infrastructure.Repositories.Interfaces.IInstallationBookingRepository, InstallationBookingRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IInstallationBookingRepository, InstallationBookingRepository>();
            services.AddScoped<ITechnicianProfileRepository, TechnicianProfileRepository>();
            services.AddScoped<IInstallationSlotRepository, InstallationSlotRepository>();
            services.AddScoped<ITechnicianRatingRepository, TechnicianRatingRepository>();

            // Warranty repositories
            services.AddScoped<IWarrantyRepository, WarrantyRepository>();
            services.AddScoped<IWarrantyClaimRepository, WarrantyClaimRepository>();
            services.AddScoped<IWarrantyRequestRepository, WarrantyRequestRepository>();

            // User Address repository
            services.AddScoped<IUserAddressRepository, UserAddressRepository>();

            // User repository
            services.AddScoped<Domain.Repositories.IUserRepository, UserRepository>();

            // Return Order & Shipment repositories
            services.AddScoped<IReturnOrderRepository, ReturnOrderRepository>();
            services.AddScoped<IOrderShipmentRepository, OrderShipmentRepository>();
            services.AddScoped<IOrderWarehouseAllocationRepository, OrderWarehouseAllocationRepository>();

            // Product Comment repository
            services.AddScoped<IProductCommentRepository, ProductCommentRepository>();

            // Chat & Notification repositories
            services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            // ML/Recommendation
            services.AddScoped<IUserBehaviorRepository, UserBehaviorRepository>();
            services.AddScoped<Application.Interfaces.IUserSimilarityService, Infrastructure.Services.ML.UserSimilarityService>();
            services.AddScoped<Application.Services.RecommendationService>();

            // Shipping service
            services.AddScoped<Domain.Services.IShippingService, Services.ShippingService>();

            return services;
        }
    }
}
