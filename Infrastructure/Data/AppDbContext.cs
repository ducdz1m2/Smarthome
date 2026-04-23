using Domain.Abstractions;
using Domain.Entities;
using Domain.Entities.Catalog;
using Domain.Entities.Communication;
using Domain.Entities.Content;
using Domain.Entities.Identity;
using Domain.Entities.Installation;
using Domain.Entities.Inventory;
using Domain.Entities.Promotions;
using Domain.Entities.Sales;
using Domain.Entities.Shipping;
using Domain.Events;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        private readonly IDomainEventDispatcher? _domainEventDispatcher;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public AppDbContext(DbContextOptions<AppDbContext> options, IDomainEventDispatcher domainEventDispatcher) : base(options)
        {
            _domainEventDispatcher = domainEventDispatcher;
        }

        // Catalog
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductComment> ProductComments => Set<ProductComment>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();

        // Inventory
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<ProductWarehouse> ProductWarehouses => Set<ProductWarehouse>();
        public DbSet<StockEntry> StockEntries => Set<StockEntry>();
        public DbSet<StockEntryDetail> StockEntryDetails => Set<StockEntryDetail>();
        public DbSet<StockIssue> StockIssues => Set<StockIssue>();
        public DbSet<StockIssueDetail> StockIssueDetails => Set<StockIssueDetail>();
        public DbSet<WarehouseTransfer> WarehouseTransfers => Set<WarehouseTransfer>();
        public DbSet<ProductReservation> ProductReservations => Set<ProductReservation>();

        // Sales
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderShipment> OrderShipments => Set<OrderShipment>();
        public DbSet<OrderWarehouseAllocation> OrderWarehouseAllocations => Set<OrderWarehouseAllocation>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Warranty> Warranties => Set<Warranty>();
        public DbSet<WarrantyClaim> WarrantyClaims => Set<WarrantyClaim>();
        public DbSet<WarrantyRequest> WarrantyRequests => Set<WarrantyRequest>();
        public DbSet<WarrantyRequestItem> WarrantyRequestItems => Set<WarrantyRequestItem>();
        public DbSet<ProductRating> ProductRatings => Set<ProductRating>();
        public DbSet<ReturnOrder> ReturnOrders => Set<ReturnOrder>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

        // Installation
        public DbSet<TechnicianProfile> TechnicianProfiles => Set<TechnicianProfile>();
        public DbSet<InstallationSlot> InstallationSlots => Set<InstallationSlot>();
        public DbSet<InstallationBooking> InstallationBookings => Set<InstallationBooking>();
        public DbSet<InstallationMaterial> InstallationMaterials => Set<InstallationMaterial>();
        public DbSet<TechnicianRating> TechnicianRatings => Set<TechnicianRating>();

        // Promotions
        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<PromotionProduct> PromotionProducts => Set<PromotionProduct>();

        // Shipping
        public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
        public DbSet<ShippingRate> ShippingRates => Set<ShippingRate>();

        // Content
        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
        public DbSet<Banner> Banners => Set<Banner>();

        // Communication
        public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();

        // ML/Recommendation
        // Communication
        public DbSet<Domain.Entities.Communication.ChatRoom> ChatRooms => Set<Domain.Entities.Communication.ChatRoom>();
        public DbSet<Domain.Entities.Communication.ChatMessage> ChatMessages => Set<Domain.Entities.Communication.ChatMessage>();
        public DbSet<Domain.Entities.Communication.ChatParticipant> ChatParticipants => Set<Domain.Entities.Communication.ChatParticipant>();
        public DbSet<Domain.Entities.Communication.ChatAttachment> ChatAttachments => Set<Domain.Entities.Communication.ChatAttachment>();
        public DbSet<Domain.Entities.Communication.Notification> Notifications => Set<Domain.Entities.Communication.Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Ignore DomainEvent - it's not an entity to be persisted
            modelBuilder.Ignore<Domain.Events.DomainEvent>();
            
            // Configure WarehouseTransfer to avoid cascade delete cycles
            modelBuilder.Entity<WarehouseTransfer>()
                .HasOne(wt => wt.FromWarehouse)
                .WithMany()
                .HasForeignKey(wt => wt.FromWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<WarehouseTransfer>()
                .HasOne(wt => wt.ToWarehouse)
                .WithMany()
                .HasForeignKey(wt => wt.ToWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch domain events before saving
            await DispatchDomainEventsAsync();

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            DispatchDomainEventsAsync().GetAwaiter().GetResult();
            return base.SaveChanges();
        }

        private async Task DispatchDomainEventsAsync()
        {
            if (_domainEventDispatcher == null)
                return;

            // Get all entities that have domain events
            var entities = ChangeTracker
                .Entries<AggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity);

            // Get all domain events
            var domainEvents = entities
                .SelectMany(e => e.DomainEvents)
                .Cast<DomainEvent>()
                .ToList();

            // Clear domain events from entities
            foreach (var entity in entities)
            {
                entity.ClearDomainEvents();
            }

            // Dispatch events
            foreach (var domainEvent in domainEvents)
            {
                await _domainEventDispatcher.DispatchAsync(domainEvent);
            }
        }
    }
}
