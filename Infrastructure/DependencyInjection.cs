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
            // Cấu hình DbContext for Blazor Server (scoped per circuit to avoid concurrency issues)
            // Enable sensitive data logging and detailed errors for debugging
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }, ServiceLifetime.Scoped);

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
            services.AddScoped<Application.Interfaces.Repositories.IProductVariantRepository, ProductVariantRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IProductRepository, ProductRepository>();
            services.AddScoped<Application.Interfaces.Repositories.ICategoryRepository, CategoryRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IBrandRepository, BrandRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IOrderRepository, OrderRepository>();
            services.AddScoped<Application.Interfaces.Repositories.ISupplierRepository, SupplierRepository>();
            services.AddScoped<Application.Interfaces.Repositories.ICouponRepository, CouponRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IPromotionRepository, PromotionRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IBannerRepository, BannerRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IStockEntryRepository, StockEntryRepository>();
            services.AddScoped<Domain.Repositories.IStockIssueRepository, StockIssueRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IProductWarehouseRepository, ProductWarehouseRepository>();
            services.AddScoped<Domain.Repositories.IWarehouseTransferRepository, WarehouseTransferRepository>();

            // Installation repositories
            services.AddScoped<Infrastructure.Repositories.Interfaces.IInstallationBookingRepository, InstallationBookingRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IInstallationBookingRepository, InstallationBookingRepository>();
            services.AddScoped<Application.Interfaces.Repositories.ITechnicianProfileRepository, TechnicianProfileRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IInstallationSlotRepository, InstallationSlotRepository>();
            services.AddScoped<Application.Interfaces.Repositories.ITechnicianRatingRepository, TechnicianRatingRepository>();
            services.AddScoped<Domain.Repositories.IInstallationMaterialRepository, InstallationMaterialRepository>();

            // Warranty repositories
            services.AddScoped<Application.Interfaces.Repositories.IWarrantyRepository, WarrantyRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IWarrantyClaimRepository, WarrantyClaimRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IWarrantyRequestRepository, WarrantyRequestRepository>();

            // User Address repository
            services.AddScoped<Application.Interfaces.Repositories.IUserAddressRepository, UserAddressRepository>();

            // User repository
            services.AddScoped<Domain.Repositories.IUserRepository, UserRepository>();

            // Return Order & Shipment repositories
            services.AddScoped<Application.Interfaces.Repositories.IReturnOrderRepository, ReturnOrderRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IOrderShipmentRepository, OrderShipmentRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IOrderWarehouseAllocationRepository, OrderWarehouseAllocationRepository>();

            // Product Comment repository
            services.AddScoped<Application.Interfaces.Repositories.IProductCommentRepository, ProductCommentRepository>();

            // Chat & Notification repositories
            services.AddScoped<Application.Interfaces.Repositories.IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<Application.Interfaces.Repositories.IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<Application.Interfaces.Repositories.INotificationRepository, NotificationRepository>();

            // Push Notification repository
            services.AddScoped<Domain.Repositories.IPushSubscriptionRepository, PushSubscriptionRepository>();

            // Shipping service
            services.AddScoped<Domain.Services.IShippingService, Services.ShippingService>();

            return services;
        }
    }
}
