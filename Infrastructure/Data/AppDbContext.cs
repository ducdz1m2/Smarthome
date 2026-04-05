using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Catalog
        public DbSet<Domain.Entities.Catalog.Product> Products => Set<Domain.Entities.Catalog.Product>();
        public DbSet<Domain.Entities.Catalog.ProductVariant> ProductVariants => Set<Domain.Entities.Catalog.ProductVariant>();
        public DbSet<Domain.Entities.Catalog.ProductImage> ProductImages => Set<Domain.Entities.Catalog.ProductImage>();
        public DbSet<Domain.Entities.Catalog.ProductComment> ProductComments => Set<Domain.Entities.Catalog.ProductComment>();
        public DbSet<Domain.Entities.Catalog.Category> Categories => Set<Domain.Entities.Catalog.Category>();
        public DbSet<Domain.Entities.Catalog.Brand> Brands => Set<Domain.Entities.Catalog.Brand>();

        // Inventory
        public DbSet<Domain.Entities.Inventory.Warehouse> Warehouses => Set<Domain.Entities.Inventory.Warehouse>();
        public DbSet<Domain.Entities.Inventory.Supplier> Suppliers => Set<Domain.Entities.Inventory.Supplier>();
        public DbSet<Domain.Entities.Inventory.ProductWarehouse> ProductWarehouses => Set<Domain.Entities.Inventory.ProductWarehouse>();
        public DbSet<Domain.Entities.Inventory.StockEntry> StockEntries => Set<Domain.Entities.Inventory.StockEntry>();
        public DbSet<Domain.Entities.Inventory.StockEntryDetail> StockEntryDetails => Set<Domain.Entities.Inventory.StockEntryDetail>();
        public DbSet<Domain.Entities.Inventory.WarehouseTransfer> WarehouseTransfers => Set<Domain.Entities.Inventory.WarehouseTransfer>();
        public DbSet<Domain.Entities.Inventory.ProductReservation> ProductReservations => Set<Domain.Entities.Inventory.ProductReservation>();

        // Sales
        public DbSet<Domain.Entities.Sales.Order> Orders => Set<Domain.Entities.Sales.Order>();
        public DbSet<Domain.Entities.Sales.OrderItem> OrderItems => Set<Domain.Entities.Sales.OrderItem>();
        public DbSet<Domain.Entities.Sales.OrderShipment> OrderShipments => Set<Domain.Entities.Sales.OrderShipment>();
        public DbSet<Domain.Entities.Sales.CartItem> CartItems => Set<Domain.Entities.Sales.CartItem>();
        public DbSet<Domain.Entities.Sales.Warranty> Warranties => Set<Domain.Entities.Sales.Warranty>();
        public DbSet<Domain.Entities.Sales.WarrantyClaim> WarrantyClaims => Set<Domain.Entities.Sales.WarrantyClaim>();
        public DbSet<Domain.Entities.Sales.ReturnOrder> ReturnOrders => Set<Domain.Entities.Sales.ReturnOrder>();
        public DbSet<Domain.Entities.Sales.PaymentTransaction> PaymentTransactions => Set<Domain.Entities.Sales.PaymentTransaction>();

        // Identity
        public DbSet<Domain.Entities.Identity.AppUser> Users => Set<Domain.Entities.Identity.AppUser>();
        public DbSet<Domain.Entities.Identity.AppRole> Roles => Set<Domain.Entities.Identity.AppRole>();

        // Installation
        public DbSet<Domain.Entities.Installation.TechnicianProfile> TechnicianProfiles => Set<Domain.Entities.Installation.TechnicianProfile>();
        public DbSet<Domain.Entities.Installation.InstallationSlot> InstallationSlots => Set<Domain.Entities.Installation.InstallationSlot>();
        public DbSet<Domain.Entities.Installation.InstallationBooking> InstallationBookings => Set<Domain.Entities.Installation.InstallationBooking>();
        public DbSet<Domain.Entities.Installation.InstallationMaterial> InstallationMaterials => Set<Domain.Entities.Installation.InstallationMaterial>();

        // Promotions
        public DbSet<Domain.Entities.Promotions.Coupon> Coupons => Set<Domain.Entities.Promotions.Coupon>();
        public DbSet<Domain.Entities.Promotions.Promotion> Promotions => Set<Domain.Entities.Promotions.Promotion>();
        public DbSet<Domain.Entities.Promotions.PromotionProduct> PromotionProducts => Set<Domain.Entities.Promotions.PromotionProduct>();

        // Shipping
        public DbSet<Domain.Entities.Shipping.ShippingZone> ShippingZones => Set<Domain.Entities.Shipping.ShippingZone>();
        public DbSet<Domain.Entities.Shipping.ShippingRate> ShippingRates => Set<Domain.Entities.Shipping.ShippingRate>();

        // Content
        public DbSet<Domain.Entities.Content.UserAddress> UserAddresses => Set<Domain.Entities.Content.UserAddress>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
