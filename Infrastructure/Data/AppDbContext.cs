using Domain.Entities.Catalog;
using Domain.Entities.Content;
using Domain.Entities.Identity;
using Domain.Entities.Installation;
using Domain.Entities.Inventory;
using Domain.Entities.Promotions;
using Domain.Entities.Sales;
using Domain.Entities.Shipping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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
        public DbSet<WarehouseTransfer> WarehouseTransfers => Set<WarehouseTransfer>();
        public DbSet<ProductReservation> ProductReservations => Set<ProductReservation>();

        // Sales
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderShipment> OrderShipments => Set<OrderShipment>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Warranty> Warranties => Set<Warranty>();
        public DbSet<WarrantyClaim> WarrantyClaims => Set<WarrantyClaim>();
        public DbSet<ReturnOrder> ReturnOrders => Set<ReturnOrder>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

        // Identity
        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<AppRole> Roles => Set<AppRole>();

        // Installation
        public DbSet<TechnicianProfile> TechnicianProfiles => Set<TechnicianProfile>();
        public DbSet<InstallationSlot> InstallationSlots => Set<InstallationSlot>();
        public DbSet<InstallationBooking> InstallationBookings => Set<InstallationBooking>();
        public DbSet<InstallationMaterial> InstallationMaterials => Set<InstallationMaterial>();

        // Promotions
        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<PromotionProduct> PromotionProducts => Set<PromotionProduct>();

        // Shipping
        public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
        public DbSet<ShippingRate> ShippingRates => Set<ShippingRate>();

        // Content
        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
