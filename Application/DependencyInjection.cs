using Application.EventHandlers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Services;
using Domain.Events;
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

            // VNPay
            services.AddHttpContextAccessor();
            services.AddScoped<IVNPayService, VNPayService>();

            // Installation services
            services.AddScoped<IInstallationService, InstallationService>();
            services.AddScoped<ITechnicianProfileService, TechnicianProfileService>();
            services.AddScoped<IInstallationSlotService, InstallationSlotService>();
            services.AddScoped<ITechnicianRatingService, TechnicianRatingService>();

            // Warranty services
            services.AddScoped<IWarrantyService, WarrantyService>();
            services.AddScoped<IWarrantyRequestService, WarrantyRequestService>();

            // Return Order services
            services.AddScoped<IReturnOrderService, ReturnOrderService>();

            // Product Comment/Review services
            services.AddScoped<IProductCommentService, ProductCommentService>();

            // Shipment services
            services.AddScoped<IShipmentService, ShipmentService>();

            // Chat & Notification services
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();

            // Identity services (User, Role, Auth combined)
            // Note: IIdentityService and IdentityService are registered in Web/Program.cs
            // to ensure proper dependency injection order with UserManager/RoleManager

            // Statistics services
            services.AddScoped<IStatisticsService, StatisticsService>();

            // User Address service
            services.AddScoped<IUserAddressService, UserAddressService>();

            // Shipping service (registered in Infrastructure DI)
            // services.AddScoped<Domain.Services.IShippingService, Infrastructure.Services.ShippingService>();

            // Register Domain Event Handlers - Order Notifications
            services.AddScoped<IDomainEventHandler<OrderCreatedEvent>, OrderNotificationHandler>();
            services.AddScoped<IDomainEventHandler<OrderConfirmedEvent>, OrderNotificationHandler>();
            services.AddScoped<IDomainEventHandler<OrderShippedEvent>, OrderNotificationHandler>();
            services.AddScoped<IDomainEventHandler<OrderDeliveredEvent>, OrderNotificationHandler>();
            services.AddScoped<IDomainEventHandler<OrderCancelledEvent>, OrderNotificationHandler>();

            // Register Domain Event Handlers - Order Inventory Management
            services.AddScoped<IDomainEventHandler<OrderConfirmedEvent>, OrderInventoryHandler>();
            services.AddScoped<IDomainEventHandler<OrderCancelledEvent>, OrderInventoryHandler>();
            services.AddScoped<IDomainEventHandler<OrderShippingStartedEvent>, OrderInventoryHandler>();

            services.AddScoped<IDomainEventHandler<InstallationBookedEvent>, InstallationNotificationHandler>();
            services.AddScoped<IDomainEventHandler<InstallationAssignedEvent>, InstallationNotificationHandler>();
            services.AddScoped<IDomainEventHandler<InstallationCompletedEvent>, InstallationNotificationHandler>();
            services.AddScoped<IDomainEventHandler<WarrantyClaimCreatedEvent>, WarrantyNotificationHandler>();
            services.AddScoped<IDomainEventHandler<WarrantyClaimApprovedEvent>, WarrantyNotificationHandler>();
            services.AddScoped<IDomainEventHandler<WarrantyClaimResolvedEvent>, WarrantyNotificationHandler>();
            services.AddScoped<IDomainEventHandler<ChatMessageSentEvent>, ChatNotificationHandler>();
            services.AddScoped<IDomainEventHandler<ChatRoomCreatedEvent>, ChatNotificationHandler>();

            // Register Domain Event Handlers - Installation Status Sync
            services.AddScoped<IDomainEventHandler<InstallationBookingConfirmedEvent>, InstallationStatusSyncHandler>();
            services.AddScoped<IDomainEventHandler<InstallationCancelledEvent>, InstallationStatusSyncHandler>();
            services.AddScoped<IDomainEventHandler<InstallationCompletedEvent>, InstallationStatusSyncHandler>();

            return services;
        }
    }
}
