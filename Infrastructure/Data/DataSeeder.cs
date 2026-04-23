using Domain.Entities.Identity;
using Domain.Entities.Catalog;
using Domain.Entities.Inventory;
using Domain.Entities.Promotions;
using Domain.Entities.Content;
using Domain.Entities.Sales;
using Domain.Entities.Installation;
using Domain.Entities.Shipping;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Domain.ValueObjects;

namespace Infrastructure.Data
{
    public static class DataSeeder
    {
        private static readonly Random random = new Random();
        private static readonly string[] vietnameseNames = new[]
        {
            "Nguyễn Văn A", "Trần Thị B", "Lê Văn C", "Phạm Thị D", "Hoàng Văn E",
            "Huỳnh Thị F", "Phan Văn G", "Vũ Thị H", "Võ Văn I", "Đặng Thị K",
            "Đỗ Văn L", "Ngô Thị M", "Đinh Văn N", "Bùi Thị O", "Dương Văn P",
            "Lý Thị Q", "Đào Văn R", "Đinh Thị S", "Trương Văn T", "Vũ Thị U",
            "Hoàng Văn V", "Nguyễn Thị W", "Trần Văn X", "Lê Thị Y", "Phạm Văn Z",
            "Nguyễn Thị An", "Trần Văn Bình", "Lê Thị Cường", "Phạm Văn Dũng", "Hoàng Thị Em",
            "Huỳnh Văn Phúc", "Phan Thị Giang", "Vũ Văn Hùng", "Võ Thị Iris", "Đặng Văn Nhật",
            "Đỗ Thị Oanh", "Ngô Văn Phúc", "Đinh Thị Quỳnh", "Bùi Văn Sang", "Dương Thị Thảo",
            "Lý Văn Tuấn", "Đào Thị Uyên", "Đinh Văn Vương", "Trương Thị Xuân", "Vũ Văn Yến",
            "Hoàng Thị Zara", "Nguyễn Văn Anh", "Trần Thị Bảo", "Lê Văn Cảnh", "Phạm Thị Duy",
            "Hoàng Văn Em", "Huỳnh Thị Phương", "Phan Văn Quân", "Vũ Thị Rạng", "Đặng Văn Sáng",
            "Đỗ Thị Tâm", "Ngô Văn Út", "Đinh Thị Vân", "Bùi Văn Xuân", "Dương Thị Yến",
            "Lý Văn Zalo", "Đào Thị Anh", "Đinh Văn Bình", "Trương Thị Cúc", "Vũ Văn Duy"
        };

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Admin User
            await SeedAdminUserAsync(userManager);

            // Seed Categories
            await SeedCategoriesAsync(context);

            // Seed Brands
            await SeedBrandsAsync(context);

            // Seed Warehouses
            await SeedWarehousesAsync(context);

            // Seed Suppliers
            await SeedSuppliersAsync(context);

            // Seed Shipping Zones & Rates
            await SeedShippingZonesAsync(context);
            Console.WriteLine("[Seed] ShippingZones done");

            // Seed Products
            await SeedProductsAsync(context);

            // Seed ProductWarehouses (số lượng sản phẩm trong các kho)
            await SeedProductWarehousesAsync(context);

            // Seed Coupons
            await SeedCouponsAsync(context);

            // Seed Banners
            await SeedBannersAsync(context);

            // Seed Customers
            await SeedCustomersAsync(userManager);

            // Seed Technicians
            await SeedTechniciansAsync(userManager, context);
            Console.WriteLine("[Seed] Technicians done");

            // Seed InstallationSlots
            await SeedInstallationSlotsAsync(context);
            Console.WriteLine("[Seed] InstallationSlots done");

            // Seed Orders (with various statuses)
            await SeedOrdersAsync(context, userManager);
            Console.WriteLine("[Seed] Orders done");

            // Seed ProductImages
            await SeedProductImagesAsync(context);
            Console.WriteLine("[Seed] ProductImages done");

            // Ensure SpecsJson for all products
            await EnsureProductSpecsAsync(context);
            Console.WriteLine("[Seed] ProductSpecs ensured");

            // Seed Promotions
            await SeedPromotionsAsync(context);
            Console.WriteLine("[Seed] Promotions done");

            // Seed StockEntries
            await SeedStockEntriesAsync(context);
            Console.WriteLine("[Seed] StockEntries done");

            // Seed InstallationBookings
            await SeedInstallationBookingsAsync(context);
            Console.WriteLine("[Seed] InstallationBookings done");

            // Seed InstallationMaterials
            await SeedInstallationMaterialsAsync(context);
            Console.WriteLine("[Seed] InstallationMaterials done");

            // Seed Warranties
            await SeedWarrantiesAsync(context);
            Console.WriteLine("[Seed] Warranties done");

            // Seed WarrantyClaims
            await SeedWarrantyClaimsAsync(context);
            Console.WriteLine("[Seed] WarrantyClaims done");

            // Seed WarrantyRequests
            await SeedWarrantyRequestsAsync(context);
            Console.WriteLine("[Seed] WarrantyRequests done");

            // Seed ReturnOrders
            await SeedReturnOrdersAsync(context);
            Console.WriteLine("[Seed] ReturnOrders done");

            // Seed ProductComments
            await SeedProductCommentsAsync(context, userManager);
            Console.WriteLine("[Seed] ProductComments done");

            // Seed TechnicianRatings
            await SeedTechnicianRatingsAsync(context, userManager);
            Console.WriteLine("[Seed] TechnicianRatings done");

            // Seed UserAddresses
            await SeedUserAddressesAsync(context, userManager);
            Console.WriteLine("[Seed] UserAddresses done");

            // Seed ProductRatings
            await SeedProductRatingsAsync(context);
            Console.WriteLine("[Seed] ProductRatings done");

            // Seed PaymentTransactions
            await SeedPaymentTransactionsAsync(context);
            Console.WriteLine("[Seed] PaymentTransactions done");
            Console.WriteLine("[Seed] ALL DONE");
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            string[] roles = { "Admin", "Customer", "Technician" };
            string[] descriptions = { "Quản trị viên - Toàn quyền hệ thống", "Khách hàng", "Kỹ thuật viên" };

            for (int i = 0; i < roles.Length; i++)
            {
                if (!await roleManager.RoleExistsAsync(roles[i]))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roles[i],
                        Description = descriptions[i],
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminUserName = "admin";
            const string adminEmail = "admin@smarthome.com";
            const string adminPassword = "admin123";

            var existingAdmin = await userManager.FindByNameAsync(adminUserName);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    FullName = "Administrator",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedCategoriesAsync(AppDbContext context)
        {
            if (!context.Categories.Any())
            {
                var refrigerator = Category.Create("Tủ lạnh", null, 0, "Tủ lạnh");
                var airConditioner = Category.Create("Máy lạnh", null, 1, "Máy lạnh");
                var washingMachine = Category.Create("Máy giặt", null, 2, "Máy giặt");
                var dryer = Category.Create("Máy sấy", null, 3, "Máy sấy");
                var tv = Category.Create("Tivi", null, 4, "Tivi");
                var audio = Category.Create("Âm thanh", null, 5, "Âm thanh");
                var riceCooker = Category.Create("Nồi cơm", null, 6, "Nồi cơm");
                var inductionCooker = Category.Create("Bếp từ", null, 7, "Bếp từ");
                var oven = Category.Create("Lò nướng", null, 8, "Lò nướng");
                var hood = Category.Create("Máy hút mùi", null, 9, "Máy hút mùi");
                var waterHeater = Category.Create("Máy nước nóng", null, 10, "Máy nước nóng");
                var robot = Category.Create("Robot hút bụi", null, 11, "Robot hút bụi");
                var airPurifier = Category.Create("Máy lọc không khí", null, 12, "Máy lọc không khí");
                var smartLight = Category.Create("Đèn thông minh", null, 13, "Đèn thông minh");
                var camera = Category.Create("Camera an ninh", null, 14, "Camera an ninh");
                var smartLock = Category.Create("Khóa thông minh", null, 15, "Khóa thông minh");
                var doorbell = Category.Create("Chuông cửa", null, 16, "Chuông cửa");
                var smartPlug = Category.Create("Ổ cắm thông minh", null, 17, "Ổ cắm thông minh");
                var smartSwitch = Category.Create("Công tắc thông minh", null, 18, "Công tắc thông minh");
                var waterFilter = Category.Create("Máy lọc nước", null, 19, "Máy lọc nước");
                var fan = Category.Create("Quạt điện", null, 20, "Quạt điện");
                var coffeeMaker = Category.Create("Máy pha cà phê", null, 21, "Máy pha cà phê");
                var juicer = Category.Create("Máy ép", null, 22, "Máy ép");
                var vacuum = Category.Create("Máy hút bụi", null, 23, "Máy hút bụi");
                var dishwasher = Category.Create("Máy rửa bát", null, 24, "Máy rửa bát");
                var microwave = Category.Create("Lò vi sóng", null, 25, "Lò vi sóng");
                var blender = Category.Create("Máy xay", null, 26, "Máy xay");
                var kettle = Category.Create("Ấm đun nước", null, 27, "Ấm đun nước");
                var iron = Category.Create("Bàn là", null, 28, "Bàn là");

                var categories = new List<Category> { refrigerator, airConditioner, washingMachine, dryer, tv, audio, riceCooker, inductionCooker, oven, hood, waterHeater, robot, airPurifier, smartLight, camera, smartLock, doorbell, smartPlug, smartSwitch, waterFilter, fan, coffeeMaker, juicer, vacuum, dishwasher, microwave, blender, kettle, iron };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // Thêm subcategories
                var subCategories = new[]
                {
                    Category.Create("Tủ lạnh Side-by-Side", refrigerator.Id, 0, "Tủ lạnh lớn"),
                    Category.Create("Tủ lạnh Inverter", refrigerator.Id, 1, "Tủ lạnh Inverter"),
                    Category.Create("Máy lạnh Inverter", airConditioner.Id, 0, "Máy lạnh Inverter"),
                    Category.Create("Máy giặt cửa trước", washingMachine.Id, 0, "Máy giặt cửa trước"),
                    Category.Create("Tivi OLED", tv.Id, 0, "Tivi OLED"),
                    Category.Create("Loa Soundbar", audio.Id, 0, "Loa Soundbar"),
                    Category.Create("Đèn LED", smartLight.Id, 0, "Đèn LED"),
                    Category.Create("Camera WiFi", camera.Id, 0, "Camera WiFi"),
                    Category.Create("Khóa vân tay", smartLock.Id, 0, "Khóa vân tay"),
                    Category.Create("Chuông WiFi", doorbell.Id, 0, "Chuông WiFi"),
                    Category.Create("Quạt đứng", fan.Id, 0, "Quạt đứng"),
                    Category.Create("Quạt treo", fan.Id, 1, "Quạt treo")
                };

                await context.Categories.AddRangeAsync(subCategories);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedBrandsAsync(AppDbContext context)
        {
            if (!context.Brands.Any())
            {
                var brands = new[]
                {
                    Brand.Create("Samsung", "Hàn Quốc - Điện tử, điện lạnh hàng đầu thế giới"),
                    Brand.Create("LG", "Hàn Quốc - Điện tử, gia dụng chất lượng cao"),
                    Brand.Create("Sony", "Nhật Bản - Tivi, âm thanh, máy ảnh"),
                    Brand.Create("Panasonic", "Nhật Bản - Điện lạnh, gia dụng"),
                    Brand.Create("Toshiba", "Nhật Bản - Tủ lạnh, máy giặt, điều hòa"),
                    Brand.Create("Sharp", "Nhật Bản - Tủ lạnh, máy lạnh, TV"),
                    Brand.Create("Electrolux", "Thụy Điển - Máy giặt, máy sấy, tủ lạnh"),
                    Brand.Create("Dyson", "Anh - Robot hút bụi, máy sấy"),
                    Brand.Create("Midea", "Trung Quốc - Điều hòa, máy giặt, nồi cơm"),
                    Brand.Create("Aqua", "Nhật Bản - Tủ lạnh, máy giặt"),
                    Brand.Create("Daikin", "Nhật Bản - Máy lạnh chuyên nghiệp"),
                    Brand.Create("Bosch", "Đức - Máy giặt, máy rửa bát"),
                    Brand.Create("Philips", "Hà Lan - Tivi, âm thanh, thiết bị gia dụng"),
                    Brand.Create("Xiaomi", "Trung Quốc - Robot hút bụi, thiết bị thông minh"),
                    Brand.Create("TCL", "Trung Quốc - Tivi, máy lạnh")
                };

                await context.Brands.AddRangeAsync(brands);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedWarehousesAsync(AppDbContext context)
        {
            if (!context.Warehouses.Any())
            {
                var warehouses = new[]
                {
                    // Hà Nội
                    Warehouse.Create("Kho chính Hà Nội", "WH-HN-001", 
                        Address.Create("123 Nguyễn Trãi", "Thanh Xuân", "Thanh Xuân", "Hà Nội"),
                        PhoneNumber.Create("0901234567"), "Nguyễn Văn A"),
                    Warehouse.Create("Kho Hà Nội - Cầu Giấy", "WH-HN-002", 
                        Address.Create("456 Đường Láng", "Quan Hoa", "Cầu Giấy", "Hà Nội"),
                        PhoneNumber.Create("0901234568"), "Trần Thị B"),
                    Warehouse.Create("Kho Hà Nội - Hoàng Mai", "WH-HN-003", 
                        Address.Create("789 Giải Phóng", "Tam Hiệp", "Hoàng Mai", "Hà Nội"),
                        PhoneNumber.Create("0901234569"), "Lê Văn C"),
                    
                    // TP.HCM
                    Warehouse.Create("Kho chính TP.HCM", "WH-HCM-001", 
                        Address.Create("456 Nguyễn Văn Linh", "Bình Chánh", "Bình Chánh", "TP.HCM"),
                        PhoneNumber.Create("0907654321"), "Trần Văn D"),
                    Warehouse.Create("Kho TP.HCM - Quận 1", "WH-HCM-002", 
                        Address.Create("123 Lê Lợi", "Bến Nghé", "Quận 1", "TP.HCM"),
                        PhoneNumber.Create("0907654322"), "Phạm Thị E"),
                    Warehouse.Create("Kho TP.HCM - Quận 7", "WH-HCM-003", 
                        Address.Create("789 Huỳnh Tấn Phát", "Tân Tạo", "Quận 7", "TP.HCM"),
                        PhoneNumber.Create("0907654323"), "Hoàng Văn F"),
                    
                    // Đà Nẵng
                    Warehouse.Create("Kho Đà Nẵng", "WH-DN-001", 
                        Address.Create("789 Võ Nguyên Giáp", "Hải Châu", "Hải Châu", "Đà Nẵng"),
                        PhoneNumber.Create("0905432167"), "Lê Văn G"),
                    
                    // Hải Phòng
                    Warehouse.Create("Kho Hải Phòng", "WH-HP-001", 
                        Address.Create("123 Lê Thánh Tông", "Đằng Giang", "Hải An", "Hải Phòng"),
                        PhoneNumber.Create("0905432168"), "Phạm Thị H"),
                    
                    // Cần Thơ
                    Warehouse.Create("Kho Cần Thơ", "WH-CT-001", 
                        Address.Create("456 Võ Văn Kiệt", "Cái Khế", "Ninh Kiều", "Cần Thơ"),
                        PhoneNumber.Create("0905432169"), "Huỳnh Văn I")
                };

                await context.Warehouses.AddRangeAsync(warehouses);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedSuppliersAsync(AppDbContext context)
        {
            if (!context.Suppliers.Any())
            {
                var suppliers = new[]
                {
                    Supplier.Create("Công ty TNHH Samsung Việt Nam", null,
                        Address.Create("Khu công nghệ cao Hòa Lạc", "Thạch Thất", "Thạch Thất", "Hà Nội"),
                        "Nguyễn Văn D", PhoneNumber.Create("0912345678"), Email.Create("samsung@example.com")),
                    Supplier.Create("Công ty TNHH LG Electronics Việt Nam", null,
                        Address.Create("Khu công nghiệp Tràng Bàng", "Tràng Bàng", "Tràng Bàng", "Hà Nội"),
                        "Trần Văn E", PhoneNumber.Create("0923456789"), Email.Create("lg@example.com"))
                };

                await context.Suppliers.AddRangeAsync(suppliers);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedProductWarehousesAsync(AppDbContext context)
        {
            if (!context.ProductWarehouses.Any())
            {
                var products = await context.Products.Include(p => p.Variants).ToListAsync();
                var warehouses = await context.Warehouses.ToListAsync();

                var productWarehouses = new List<ProductWarehouse>();
                var variantStockTotals = new Dictionary<int, int>();
                var productStockTotals = new Dictionary<int, int>();

                // Phân phối sản phẩm vào các kho với số lượng thực tế cho từng variant
                foreach (var product in products)
                {
                    if (!product.Variants.Any())
                    {
                        // Nếu sản phẩm không có variant, tạo ProductWarehouse cho product (VariantId = null)
                        // Phân phối ngẫu nhiên vào 2-4 kho
                        var warehouseCount = random.Next(2, 5);
                        var selectedWarehouses = warehouses.OrderBy(x => random.Next()).Take(warehouseCount).ToList();

                        var totalStock = 0;

                        foreach (var warehouse in selectedWarehouses)
                        {
                            // HN warehouses get more stock (20-50), others get 10-30
                            var quantity = warehouse.Address.City.Contains("Hà Nội") 
                                ? random.Next(20, 51) 
                                : random.Next(10, 31);

                            var productWarehouse = ProductWarehouse.Create(product.Id, null, warehouse.Id, quantity);
                            productWarehouses.Add(productWarehouse);
                            totalStock += quantity;
                        }

                        productStockTotals[product.Id] = totalStock;
                    }
                    else
                    {
                        // Nếu sản phẩm có variants, phân phối cho từng variant
                        foreach (var variant in product.Variants)
                        {
                            // Phân phối ngẫu nhiên vào 2-4 kho
                            var warehouseCount = random.Next(2, 5);
                            var selectedWarehouses = warehouses.OrderBy(x => random.Next()).Take(warehouseCount).ToList();

                            var totalStock = 0;

                            foreach (var warehouse in selectedWarehouses)
                            {
                                // HN warehouses get more stock (15-40), others get 5-25
                                var quantity = warehouse.Address.City.Contains("Hà Nội") 
                                    ? random.Next(15, 41) 
                                    : random.Next(5, 26);

                                var productWarehouse = ProductWarehouse.Create(product.Id, variant.Id, warehouse.Id, quantity);
                                productWarehouses.Add(productWarehouse);
                                totalStock += quantity;
                            }

                            variantStockTotals[variant.Id] = totalStock;
                        }

                        // Tổng stock của product = tổng stock của tất cả variants
                        productStockTotals[product.Id] = variantStockTotals
                            .Where(kvp => product.Variants.Any(v => v.Id == kvp.Key))
                            .Sum(kvp => kvp.Value);
                    }
                }

                await context.ProductWarehouses.AddRangeAsync(productWarehouses);
                await context.SaveChangesAsync();

                // Đồng bộ ProductVariant.StockQuantity từ tổng tồn kho của variant
                foreach (var product in products)
                {
                    foreach (var variant in product.Variants)
                    {
                        if (variantStockTotals.TryGetValue(variant.Id, out var totalStock) && totalStock > 0)
                        {
                            variant.AddStock(totalStock);
                        }
                    }
                }

                await context.SaveChangesAsync();

                // Đồng bộ Product.StockQuantity từ tổng StockQuantity của các variants
                foreach (var product in products)
                {
                    if (productStockTotals.TryGetValue(product.Id, out var totalStock))
                    {
                        product.SetStockQuantity(totalStock);
                    }
                }

                await context.SaveChangesAsync();
            }
        }

        private static List<(string Sku, int Price, Dictionary<string, string> Attributes)> GetProductVariants(string baseSku, int basePrice, string categoryName)
        {
            var variants = new List<(string, int, Dictionary<string, string>)>();
            
            switch (categoryName)
            {
                case "Tủ lạnh":
                    variants.Add(($"{baseSku}-TRANG", basePrice, new Dictionary<string, string> { { "Màu", "Trắng" } }));
                    variants.Add(($"{baseSku}-XAM", basePrice + 500000, new Dictionary<string, string> { { "Màu", "Xám" } }));
                    variants.Add(($"{baseSku}-DEN", basePrice + 500000, new Dictionary<string, string> { { "Màu", "Đen" } }));
                    break;
                    
                case "Máy lạnh":
                    variants.Add(($"{baseSku}-1HP", basePrice, new Dictionary<string, string> { { "Công suất", "1HP" } }));
                    variants.Add(($"{baseSku}-15HP", (int)(basePrice * 1.3), new Dictionary<string, string> { { "Công suất", "1.5HP" } }));
                    variants.Add(($"{baseSku}-2HP", (int)(basePrice * 1.8), new Dictionary<string, string> { { "Công suất", "2HP" } }));
                    break;
                    
                case "Máy giặt":
                    variants.Add(($"{baseSku}-7KG", basePrice, new Dictionary<string, string> { { "Khối lượng", "7kg" } }));
                    variants.Add(($"{baseSku}-8KG", (int)(basePrice * 1.1), new Dictionary<string, string> { { "Khối lượng", "8kg" } }));
                    variants.Add(($"{baseSku}-9KG", (int)(basePrice * 1.2), new Dictionary<string, string> { { "Khối lượng", "9kg" } }));
                    break;
                    
                case "Tivi":
                    variants.Add(($"{baseSku}-43", basePrice, new Dictionary<string, string> { { "Kích thước", "43\"" } }));
                    variants.Add(($"{baseSku}-55", (int)(basePrice * 1.8), new Dictionary<string, string> { { "Kích thước", "55\"" } }));
                    variants.Add(($"{baseSku}-65", (int)(basePrice * 2.5), new Dictionary<string, string> { { "Kích thước", "65\"" } }));
                    break;
                    
                case "Âm thanh":
                    variants.Add(($"{baseSku}-DEN", basePrice, new Dictionary<string, string> { { "Màu", "Đen" } }));
                    variants.Add(($"{baseSku}-TRANG", basePrice + 300000, new Dictionary<string, string> { { "Màu", "Trắng" } }));
                    break;
                    
                case "Nồi cơm":
                    variants.Add(($"{baseSku}-10L", basePrice, new Dictionary<string, string> { { "Dung tích", "1.0L" } }));
                    variants.Add(($"{baseSku}-18L", (int)(basePrice * 1.2), new Dictionary<string, string> { { "Dung tích", "1.8L" } }));
                    break;
                    
                case "Bếp từ":
                    variants.Add(($"{baseSku}-DON", basePrice, new Dictionary<string, string> { { "Loại", "Đơn" } }));
                    variants.Add(($"{baseSku}-DOI", (int)(basePrice * 1.3), new Dictionary<string, string> { { "Loại", "Đôi" } }));
                    break;
                    
                default:
                    // Default single variant
                    variants.Add(($"{baseSku}-DEFAULT", basePrice, new Dictionary<string, string>()));
                    break;
            }
            
            return variants;
        }

        private static Dictionary<string, string> GetProductAttributes(string categoryName, string productName)
        {
            var attrs = new Dictionary<string, string>();
            switch (categoryName)
            {
                case "Tủ lạnh":
                    var tlMatch = System.Text.RegularExpressions.Regex.Match(productName, @"(\d+)L");
                    var capacity = tlMatch.Success ? $"{tlMatch.Groups[1].Value} lít" : "N/A";
                    attrs["Dung tích tổng"] = capacity;
                    attrs["Dung tích làm lạnh"] = tlMatch.Success ? $"{int.Parse(tlMatch.Groups[1].Value) * 0.7:F0} lít" : "N/A";
                    attrs["Dung tích cấp đông"] = tlMatch.Success ? $"{int.Parse(tlMatch.Groups[1].Value) * 0.3:F0} lít" : "N/A";
                    attrs["Công nghệ làm lạnh"] = productName.Contains("Inverter") ? "Inverter tiết kiệm điện" : "Làm lạnh trực tiếp";
                    attrs["Kiểu tủ"] = productName.Contains("Side-by-Side") ? "Side-by-Side (2 cửa)"
                        : productName.Contains("Door-in-Door") ? "Door-in-Door"
                        : "Ngăn đá trên";
                    attrs["Số cửa"] = productName.Contains("Side-by-Side") ? "4 cửa" : "2 cửa";
                    attrs["Khử mùi & Kháng khuẩn"] = productName.Contains("Plasmacluster") ? "Plasmacluster Ion" : "Kháng khuẩn Nano Ag+";
                    attrs["Công nghệ làm đá"] = "Làm đá tự động";
                    attrs["Lấy nước ngoài"] = "Có";
                    attrs["Kích thước (R x S x C)"] = productName.Contains("Side-by-Side") ? "912 x 1789 x 718 mm" : "703 x 1789 x 718 mm";
                    attrs["Trọng lượng"] = productName.Contains("Side-by-Side") ? "135 kg" : "85 kg";
                    attrs["Tiêu thụ điện năng"] = productName.Contains("Inverter") ? "1.2 kWh/ngày" : "1.8 kWh/ngày";
                    attrs["Độ ồn"] = "38 dB";
                    attrs["Màu sắc"] = "Xám Bạc / Đen / Trắng";
                    attrs["Bảo hành"] = "24 tháng (bộ máy), 10 năm (block máy)";
                    break;

                case "Máy lạnh":
                    var mlMatch = System.Text.RegularExpressions.Regex.Match(productName, @"([\d.]+)HP");
                    var hp = mlMatch.Success ? double.Parse(mlMatch.Groups[1].Value) : 1.0;
                    attrs["Công suất làm lạnh"] = mlMatch.Success ? $"{mlMatch.Groups[1].Value} HP" : "N/A";
                    attrs["Diện tích phòng"] = hp <= 1.0 ? "Dưới 15m²" : hp <= 1.5 ? "15 - 20m²" : hp <= 2.0 ? "20 - 30m²" : "30 - 40m²";
                    attrs["Công nghệ máy nén"] = "Inverter tiết kiệm điện năng";
                    attrs["Loại Gas lạnh"] = "R32 (thân thiện môi trường)";
                    attrs["Điều hướng gió"] = "4 chiều (lên xuống, trái phải tự động)";
                    attrs["Chế độ làm lạnh nhanh"] = "Có";
                    attrs["Chế độ ngủ"] = "Có";
                    attrs["Chế độ tự làm sạch"] = "Có";
                    attrs["Lọc không khí"] = "Bộ lọc bụi mịn PM2.5";
                    attrs["Độ ồn"] = hp <= 1.5 ? "19 dB" : "24 dB";
                    attrs["Tiêu thụ điện năng"] = $"{hp * 0.8:F1} kWh/giờ";
                    attrs["Kích thước (R x S x C)"] = hp <= 1.5 ? "820 x 280 x 699 mm" : "1067 x 300 x 895 mm";
                    attrs["Trọng lượng"] = hp <= 1.5 ? "9 kg" : "13 kg";
                    attrs["Bảo hành"] = "36 tháng (bộ máy), 5 năm (block máy)";
                    break;

                case "Máy giặt":
                    var mgMatch = System.Text.RegularExpressions.Regex.Match(productName, @"([\d.]+)kg");
                    var kg = mgMatch.Success ? double.Parse(mgMatch.Groups[1].Value) : 8.0;
                    attrs["Khối lượng giặt"] = mgMatch.Success ? $"{mgMatch.Groups[1].Value} kg" : "N/A";
                    attrs["Loại cửa"] = productName.Contains("cửa trước") ? "Cửa trước (Lồng ngang)" : "Cửa trên (Lồng đứng)";
                    attrs["Công nghệ motor"] = "Inverter Direct Drive";
                    attrs["Công nghệ giặt"] = "Giặt hơi nước Steam, Giặt AI";
                    attrs["Tốc độ vắt tối đa"] = "1400 vòng/phút";
                    attrs["Số chương trình giặt"] = "14 chương trình";
                    attrs["Chương trình đặc biệt"] = "Giặt nhanh 15 phút, Giặt em bé, Giặt dị ứng";
                    attrs["Khóa trẻ em"] = "Có";
                    attrs["Hẹn giờ giặt"] = "Có (đến 19 giờ)";
                    attrs["Tự vệ sinh lồng giặt"] = "Có";
                    attrs["Độ ồn giặt"] = "54 dB";
                    attrs["Độ ồn vắt"] = "74 dB";
                    attrs["Tiêu thụ điện năng"] = $"{kg * 0.1:F1} kWh/chu trình";
                    attrs["Tiêu thụ nước"] = $"{kg * 10:F0} lít/chu trình";
                    attrs["Kích thước (R x S x C)"] = productName.Contains("cửa trước") ? "600 x 600 x 850 mm" : "560 x 590 x 960 mm";
                    attrs["Trọng lượng"] = "70 kg";
                    attrs["Màu sắc"] = "Trắng / Bạc";
                    attrs["Bảo hành"] = "24 tháng (bộ máy), 10 năm (motor)";
                    break;

                case "Máy sấy":
                    var msMatch = System.Text.RegularExpressions.Regex.Match(productName, @"(\d+)kg");
                    attrs["Khối lượng sấy"] = msMatch.Success ? $"{msMatch.Groups[1].Value} kg" : "N/A";
                    attrs["Công nghệ sấy"] = productName.Contains("Heat Pump") ? "Bơm nhiệt (Heat Pump) - Tiết kiệm điện" : "Sấy thông hơi";
                    attrs["Nhiệt độ sấy"] = "Từ 30°C - 70°C";
                    attrs["Số chương trình sấy"] = "12 chương trình";
                    attrs["Chế độ đặc biệt"] = "Sấy nhanh 40 phút, Chống nhăn, Sấy đồ len, Sấy đồ em bé";
                    attrs["Cảm biến độ ẩm"] = "Có (tự động ngắt khi khô)";
                    attrs["Bộ lọc xơ vải"] = "Có (dễ tháo lắp)";
                    attrs["Độ ồn"] = productName.Contains("Heat Pump") ? "62 dB" : "68 dB";
                    attrs["Tiêu thụ điện năng"] = productName.Contains("Heat Pump") ? "1.5 kWh/chu trình" : "2.8 kWh/chu trình";
                    attrs["Kích thước (R x S x C)"] = "600 x 600 x 850 mm";
                    attrs["Trọng lượng"] = "52 kg";
                    attrs["Màu sắc"] = "Trắng / Bạc";
                    attrs["Bảo hành"] = "24 tháng (bộ máy), 10 năm (motor)";
                    break;

                case "Tivi":
                    var tvSizeMatch = System.Text.RegularExpressions.Regex.Match(productName, @"(\d+)""");
                    var inch = tvSizeMatch.Success ? int.Parse(tvSizeMatch.Groups[1].Value) : 43;
                    attrs["Kích thước màn hình"] = tvSizeMatch.Success ? $"{tvSizeMatch.Groups[1].Value} inch" : "N/A";
                    attrs["Độ phân giải"] = "4K Ultra HD (3840 x 2160)";
                    attrs["Công nghệ màn hình"] = productName.Contains("OLED") ? "OLED (Organic Light Emitting Diode)"
                        : productName.Contains("QLED") ? "QLED (Quantum Dot LED)"
                        : "LED Crystal UHD";
                    attrs["Tần số quét"] = "120Hz (Motion Rate 240)";
                    attrs["Hệ điều hành"] = productName.Contains("Samsung") ? "Tizen OS" : productName.Contains("LG") ? "webOS" : productName.Contains("Sony") ? "Google TV" : "Android TV";
                    attrs["Chip xử lý"] = productName.Contains("Samsung") ? "Crystal Processor 4K" : productName.Contains("LG") ? "α9 Gen5 AI Processor" : productName.Contains("Sony") ? "Cognitive Processor XR" : "Quad Core";
                    attrs["Cổng kết nối"] = "4 x HDMI 2.1, 2 x USB 2.0, 1 x Optical, 1 x LAN, Wi-Fi 6, Bluetooth 5.2";
                    attrs["HDMI eARC"] = "Có";
                    attrs["HDR"] = "HDR10, HDR10+, HLG, Dolby Vision";
                    attrs["Loa"] = $"{inch * 0.5}W - {inch * 0.8}W (Dolby Atmos / Object Tracking Sound)";
                    attrs["Công nghệ âm thanh"] = "Dolby Atmos, DTS:X";
                    attrs["Chế độ hình ảnh"] = "Phim, Thể thao, Game, HDR+";
                    attrs["Gaming"] = "VRR, ALLM, 4K@120Hz";
                    attrs["Độ dày"] = productName.Contains("OLED") ? "4.7 mm" : "25 mm";
                    attrs["Chân đế"] = "Có thể tháo lắp";
                    attrs["Bảo hành"] = "24 tháng";
                    break;

                case "Âm thanh":
                    attrs["Loại sản phẩm"] = productName.Contains("Soundbar") ? "Loa thanh (Soundbar)" : "Loa Bluetooth";
                    attrs["Tổng công suất"] = productName.Contains("Soundbar") ? "300W - 500W" : "30W - 50W";
                    attrs["Số kênh âm thanh"] = productName.Contains("Soundbar") ? "5.1.2 kênh" : "2.0 kênh";
                    attrs["Công nghệ âm thanh"] = "Dolby Atmos, DTS:X, DTS Virtual:X";
                    attrs["Kết nối không dây"] = "Bluetooth 5.0, Wi-Fi, Chromecast built-in, AirPlay 2";
                    attrs["Kết nối có dây"] = "HDMI eARC, Optical, USB, AUX 3.5mm";
                    attrs["Loa subwoofer"] = productName.Contains("Soundbar") ? "Có (không dây)" : "Không";
                    attrs["Chế độ âm thanh"] = "Phim, Nhạc, Thể thao, Game, Night Mode";
                    attrs["EQ tùy chỉnh"] = "Có";
                    attrs["Điều khiển"] = "Remote, App điện thoại, Giọng nói";
                    attrs["Kích thước (R x S x C)"] = productName.Contains("Soundbar") ? "1060 x 58 x 90 mm" : "200 x 80 x 70 mm";
                    attrs["Trọng lượng"] = productName.Contains("Soundbar") ? "6.5 kg" : "0.8 kg";
                    attrs["Màu sắc"] = "Đen";
                    attrs["Pin"] = productName.Contains("Soundbar") ? "Không (cắm điện)" : " upto 12 giờ";
                    attrs["Bảo hành"] = "12 tháng";
                    break;

                case "Nồi cơm":
                    var ncMatch = System.Text.RegularExpressions.Regex.Match(productName, @"([\d.]+)L");
                    var liters = ncMatch.Success ? double.Parse(ncMatch.Groups[1].Value) : 1.8;
                    attrs["Dung tích"] = ncMatch.Success ? $"{ncMatch.Groups[1].Value} lít" : "N/A";
                    attrs["Số người ăn"] = liters >= 1.8 ? "8 - 10 người" : liters >= 1.0 ? "4 - 6 người" : "2 - 4 người";
                    attrs["Lòng nồi"] = "Hợp kim nhôm phủ chống dính cao cấp";
                    attrs["Độ dày lòng nồi"] = "2.5 mm";
                    attrs["Công nghệ nấu"] = "Nấu cao tần (IH) hoặc Nấu 3D nhiệt đối lưu";
                    attrs["Công suất"] = liters >= 1.8 ? "1200W" : "700W";
                    attrs["Điện áp"] = "220V - 50Hz";
                    attrs["Số chế độ nấu"] = "8 chế độ";
                    attrs["Chế độ nấu"] = "Cơm trắng, Cơm nâu, Cơm trộn, Hầm, Làm bánh, Nấu cháo";
                    attrs["Hẹn giờ nấu"] = "Có (đến 24 giờ)";
                    attrs["Giữ ấm"] = "12 - 24 tiếng";
                    attrs["Màn hình hiển thị"] = "LED";
                    attrs["Nắp nồi"] = "Nắp gài có thể tháo rời";
                    attrs["Kích thước (R x S x C)"] = $"{240 + liters * 30} x {240 + liters * 30} x {200 + liters * 20} mm";
                    attrs["Trọng lượng"] = $"{3 + liters} kg";
                    attrs["Màu sắc"] = "Đỏ / Trắng / Đen";
                    attrs["Bảo hành"] = "12 tháng";
                    break;

                case "Bếp từ":
                    attrs["Loại bếp"] = productName.Contains("ba") ? "Bếp từ 3 vùng nấu" : "Bếp từ đôi (2 vùng nấu)";
                    attrs["Mặt bếp"] = "Kính Schott Ceran cao cấp (Đức) - Chống trầy xước";
                    attrs["Công suất tổng"] = productName.Contains("ba") ? "7200W" : "4800W";
                    attrs["Công suất mỗi vùng"] = productName.Contains("ba") ? "1800W - 2200W" : "2000W - 2800W";
                    attrs["Chế độ Booster"] = "Có (tăng công suất lên 50% trong 10 phút)";
                    attrs["Điều khiển cảm ứng"] = "Cảm ứng trượt + Touch";
                    attrs["Hiển thị nhiệt độ"] = "Có";
                    attrs["Công nghệ IH"] = "Bản mạch inverter tiết kiệm điện";
                    attrs["Kích thước lắp đặt"] = productName.Contains("ba") ? "730 x 430 mm" : "700 x 400 mm";
                    attrs["Kích thước mặt kính"] = productName.Contains("ba") ? "760 x 460 mm" : "730 x 420 mm";
                    attrs["Chiều cao cắt đá"] = "50 mm";
                    attrs["Tiện ích an toàn"] = "Khóa trẻ em, Tự ngắt khi quá nhiệt, Tự ngắt khi không có nồi, Cảnh báo nhiệt dư";
                    attrs["Bộ hẹn giờ"] = "Có (đến 99 phút cho từng vùng)";
                    attrs["Độ ồn"] = "Không có (chỉ tiếng quạt tản nhiệt)";
                    attrs["Tiêu thụ điện"] = productName.Contains("ba") ? "7.2 kWh/giờ (tối đa)" : "4.8 kWh/giờ (tối đa)";
                    attrs["Màu sắc"] = "Đen";
                    attrs["Bảo hành"] = "24 tháng";
                    break;

                case "Lò nướng":
                    var lnMatch = System.Text.RegularExpressions.Regex.Match(productName, @"(\d+)L");
                    var ovenLiters = lnMatch.Success ? int.Parse(lnMatch.Groups[1].Value) : 28;
                    attrs["Dung tích"] = lnMatch.Success ? $"{lnMatch.Groups[1].Value} lít" : "N/A";
                    attrs["Loại lò"] = productName.Contains("vi sóng") ? "Lò vi sóng có nướng" : "Lò nướng điện đối lưu";
                    attrs["Số chức năng"] = "10 chức năng";
                    attrs["Chức năng"] = "Nướng, Nướng đối lưu, Nướng giòn, Hâm nóng, Rã đông, Nướng BBQ, Làm bánh";
                    attrs["Công suất"] = productName.Contains("vi sóng") ? "1000W" : "2000W";
                    attrs["Dải nhiệt độ"] = "50°C - 250°C";
                    attrs["Quạt đối lưu"] = "Có";
                    attrs["Đèn chiếu sáng"] = "Có (LED)";
                    attrs["Bộ hẹn giờ"] = "Có (đến 120 phút)";
                    attrs["Cửa kính"] = "Kính 2 lớp cách nhiệt";
                    attrs["Khay nướng"] = "Có (khay tráng men)";
                    attrs["Vỉ nướng"] = "Có";
                    attrs["Kích thước (R x S x C)"] = productName.Contains("vi sóng") ? "510 x 440 x 310 mm" : "595 x 595 x 595 mm";
                    attrs["Trọng lượng"] = productName.Contains("vi sóng") ? "17 kg" : "35 kg";
                    attrs["Màu sắc"] = "Đen / Bạc";
                    break;

                case "Máy hút mùi":
                    attrs["Loại máy"] = "Máy hút mùi kính cong / áp tường";
                    attrs["Công suất hút"] = "700 - 1000 m³/h";
                    attrs["Độ ồn"] = "≤ 58 dB";
                    attrs["Số tốc độ"] = "3 tốc độ";
                    attrs["Chế độ hút"] = "Hút trực tiếp ra ngoài / Hút tuần hoàn (than hoạt tính)";
                    attrs["Bộ lọc"] = "Lưới lọc nhôm 5 lớp + Than hoạt tính";
                    attrs["Đèn chiếu sáng"] = "LED 2 x 1.5W";
                    attrs["Điều khiển"] = "Cảm ứng + Remote";
                    attrs["Hẹn giờ tắt"] = "Có (đến 15 phút)";
                    attrs["Bộ lọc mỡ"] = "Dễ tháo lắp, rửa bằng máy rửa bát";
                    attrs["Kích thước (R x S x C)"] = "700 x 500 mm";
                    attrs["Khoét đá"] = "695 x 495 mm";
                    attrs["Chiều cao tối thiểu"] = "650 mm";
                    attrs["Màu sắc"] = "Đen / Bạc";
                    attrs["Bảo hành"] = "24 tháng";
                    break;

                case "Máy nước nóng":
                    attrs["Loại máy"] = "Trực tiếp (có bơm trợ lực)";
                    attrs["Công suất làm nóng"] = "4500W";
                    attrs["Dung tích bình chứa"] = "Không có (nước nóng tức thì)";
                    attrs["Nhiệt độ nước nóng"] = "35°C - 60°C (có thể điều chỉnh)";
                    attrs["Vòi sen"] = "Vòi sen 5 chế độ phun";
                    attrs["Đầu sen"] = "Đầu sen 3 chức năng";
                    attrs["Cảm biến nhiệt"] = "Có (tự động điều chỉnh nhiệt độ)";
                    attrs["Cầu dao chống giật"] = "ELCB (tự ngắt trong 0.1 giây)";
                    attrs["Van an toàn"] = "Có (xả áp khi quá nhiệt)";
                    attrs["Công nghệ tiết kiệm điện"] = "ECO";
                    attrs["Kích thước (R x S x C)"] = "245 x 420 x 85 mm";
                    attrs["Màu sắc"] = "Trắng / Bạc";
                    attrs["Bảo hành"] = "18 tháng (bộ máy), 5 năm (bình nóng)";
                    break;

                case "Robot hút bụi":
                    attrs["Lực hút"] = "3000Pa - 5000Pa";
                    attrs["Thời gian hoạt động"] = "120 - 180 phút";
                    attrs["Thời gian sạc"] = "4 - 6 giờ";
                    attrs["Dung tích hộp bụi"] = "400ml";
                    attrs["Dung tích bình chứa nước"] = "180ml";
                    attrs["Chức năng lau nhà"] = "Có (lau ướt)";
                    attrs["Cảm biến"] = "Lidar, ToF, Cảm biến rơi, Cảm biến bụi";
                    attrs["Vẽ bản đồ"] = "Có (SLAM)";
                    attrs["Phân vùng"] = "Có (tạo vùng cấm, vùng ưu tiên)";
                    attrs["Tự động sạc"] = "Có (tự quay về dock khi pin yếu)";
                    attrs["Tiếp tục sau sạc"] = "Có";
                    attrs["Kết nối"] = "Wi-Fi 2.4GHz, App Mi Home / Samsung SmartThings";
                    attrs["Điều khiển giọng nói"] = "Google Assistant, Alexa";
                    attrs["Kích thước (R x S x C)"] = "350 x 350 x 97 mm";
                    attrs["Trọng lượng"] = "3.5 kg";
                    attrs["Bảo hành"] = "12 tháng";
                    break;

                case "Máy lọc không khí":
                    attrs["Diện tích lọc"] = "30m² - 60m²";
                    attrs["Lưu lượng khí"] = "300 - 600 m³/h";
                    attrs["Bộ lọc"] = "Bộ lọc 3 lớp: Pre-filter, HEPA H13, Than hoạt tính";
                    attrs["Khử mùi"] = "Có (Than hoạt tính)";
                    attrs["Khử khuẩn"] = "Ion âm";
                    attrs["Cảm biến bụi mịn"] = "PM2.5 (hiển thị thời gian thực)";
                    attrs["Cảm biến khí"] = "VOC (Volatile Organic Compounds)";
                    attrs["Độ ồn"] = "20dB - 55dB";
                    attrs["Chế độ hoạt động"] = "Tự động, Ngủ, Turbo, Yên tĩnh";
                    attrs["Đèn báo chất lượng không khí"] = "Màu (Đỏ/Vàng/Xanh)";
                    attrs["Hẹn giờ"] = "Có (1 - 12 giờ)";
                    attrs["Kết nối"] = "Wi-Fi, App điện thoại";
                    attrs["Kích thước (R x S x C)"] = "240 x 240 x 520 mm";
                    attrs["Trọng lượng"] = "5.5 kg";
                    attrs["Màu sắc"] = "Trắng";
                    attrs["Bảo hành"] = "12 tháng (bộ máy), 2 năm (bộ lọc)";
                    break;
            }
            return attrs;
        }

        private static async Task SeedProductsAsync(AppDbContext context)
        {
            if (!context.Products.Any())
            {
                var categories = await context.Categories.ToListAsync();
                var brands = await context.Brands.ToListAsync();

                var products = new List<Product>();

                // Danh sách sản phẩm thực tế
                var productData = new List<(string Name, string Category, string Brand, int Price, bool RequiresInstallation)>
                {
                    // Tủ lạnh
                    ("Samsung Inverter 450L RB46A", "Tủ lạnh", "Samsung", 18900000, true),
                    ("LG Inverter 520L GR-B257", "Tủ lạnh", "LG", 21900000, true),
                    ("Panasonic Inverter 519L GR-R570", "Tủ lạnh", "Panasonic", 19900000, true),
                    
                    // Máy lạnh
                    ("Samsung Inverter 1HP AR12TY", "Máy lạnh", "Samsung", 8900000, true),
                    ("Samsung Inverter 2HP AR24TY", "Máy lạnh", "Samsung", 15900000, true),
                    ("LG Inverter 1.5HP V18APV", "Máy lạnh", "LG", 11500000, true),
                    ("Daikin Inverter 2HP FTKA50", "Máy lạnh", "Daikin", 18900000, true),
                    
                    // Máy giặt
                    ("Samsung Inverter 8kg WA80H", "Máy giặt", "Samsung", 8900000, true),
                    ("LG Inverter 9kg T2308V", "Máy giặt", "LG", 9500000, true),
                    ("Bosch Inverter 8kg WAK241", "Máy giặt", "Bosch", 13500000, true),
                    
                    // Tivi
                    ("Samsung 43\" 4K Smart UA43CU", "Tivi", "Samsung", 12900000, false),
                    ("Samsung 55\" 4K QLED QA55Q7", "Tivi", "Samsung", 24900000, false),
                    ("LG 55\" 4K OLED 55C3", "Tivi", "LG", 28900000, false),
                    ("Sony 55\" 4K OLED XR-55A8", "Tivi", "Sony", 32900000, false),
                    
                    // Âm thanh
                    ("Samsung Soundbar HW-T65", "Âm thanh", "Samsung", 5900000, false),
                    ("LG Soundbar S95QR", "Âm thanh", "LG", 15900000, false),
                    
                    // Robot hút bụi
                    ("Dyson V15 Detect Robot", "Robot hút bụi", "Dyson", 19900000, false),
                    ("Samsung Jet Bot Robot", "Robot hút bụi", "Samsung", 8900000, false),
                    ("Xiaomi Robot Vacuum S10", "Robot hút bụi", "Xiaomi", 5900000, false),
                    
                    // Máy lọc không khí
                    ("Samsung AX60R5080WD", "Máy lọc không khí", "Samsung", 7900000, false),
                    ("LG PuriCare 360", "Máy lọc không khí", "LG", 8900000, false),
                    
                    // Bếp từ
                    ("Samsung Bếp từ đôi NZ64H", "Bếp từ", "Samsung", 3900000, true),
                    ("Bosch Bếp từ ba PFS061", "Bếp từ", "Bosch", 8900000, true),
                    
                    // Máy nước nóng
                    ("Samsung Máy nước nóng DQE-50", "Máy nước nóng", "Samsung", 2900000, true),
                    ("Aqua Máy nước nóng A-50", "Máy nước nóng", "Aqua", 2600000, true),
                    
                    // Máy hút mùi
                    ("Samsung Máy hút mùi CF-300", "Máy hút mùi", "Samsung", 2500000, true),
                    ("Bosch Máy hút mùi DHI965", "Máy hút mùi", "Bosch", 8900000, true),
                    
                    // Lò nướng
                    ("Bosch Lò nướng 71L HBG67", "Lò nướng", "Bosch", 15900000, true),
                    
                    // Nồi cơm
                    ("Samsung Nồi cơm 1.8L ECJ-HC", "Nồi cơm", "Samsung", 1490000, false),
                    ("Panasonic Nồi cơm 1.8L SR-DF18", "Nồi cơm", "Panasonic", 1290000, false)
                };

                // Dùng HashSet để tránh trùng SKU
                var usedSkus = new HashSet<string>();

                foreach (var (name, categoryName, brandName, price, requiresInstallation) in productData)
                {
                    var category = categories.FirstOrDefault(c => c.Name == categoryName);
                    var brand = brands.FirstOrDefault(b => b.Name == brandName);

                    if (category != null && brand != null)
                    {
                        var brandPrefix = new string(brand.Name.Where(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')).Take(3).ToArray()).ToUpper();
                        var categoryPrefix = new string(category.Name.Where(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')).Take(3).ToArray()).ToUpper();
                        string sku;
                        do { sku = $"{brandPrefix}-{categoryPrefix}-{random.Next(1000, 9999)}"; }
                        while (usedSkus.Contains(sku));
                        usedSkus.Add(sku);

                        var product = Product.Create(name, sku, category.Id, brand.Id, null, requiresInstallation);

                        // Cập nhật SpecsJson cho Product
                        var attributes = GetProductAttributes(categoryName, name);
                        product.UpdateSpecs(attributes);

                        // Thêm variants với phân loại ngắn gọn
                        var variants = GetProductVariants(sku, price, categoryName);
                        foreach (var (vSku, vPrice, vAttrs) in variants)
                        {
                            product.AddVariant(vSku, Money.Vnd(vPrice), vAttrs);
                        }

                        products.Add(product);
                    }
                }

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedCouponsAsync(AppDbContext context)
        {
            if (!context.Coupons.Any())
            {
                var coupons = new[]
                {
                    Coupon.Create("WELCOME10", DiscountType.Percentage, Money.Vnd(10), DateTime.UtcNow.AddMonths(6), 1000, Money.Vnd(500000), Money.Vnd(500000)),
                    Coupon.Create("SUMMER20", DiscountType.Percentage, Money.Vnd(20), DateTime.UtcNow.AddMonths(3), 500, Money.Vnd(2000000), Money.Vnd(1000000)),
                    Coupon.Create("FREESHIP", DiscountType.FixedAmount, Money.Vnd(50000), DateTime.UtcNow.AddMonths(12), 2000, Money.Vnd(300000), Money.Vnd(50000))
                };

                await context.Coupons.AddRangeAsync(coupons);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedBannersAsync(AppDbContext context)
        {
            if (!context.Banners.Any())
            {
                var banners = new[]
                {
                    Banner.Create("Khuyến mãi mùa hè", WebsiteUrl.Create("https://localhost:5001/images/banner1.jpg"), 
                        "Giảm đến 40% cho sản phẩm điện lạnh", WebsiteUrl.Create("https://localhost:5001/products"), "HomeTop", 1),
                    Banner.Create("Sản phẩm mới", WebsiteUrl.Create("https://localhost:5001/images/banner2.jpg"), 
                        "Khám phá các sản phẩm mới nhất", WebsiteUrl.Create("https://localhost:5001/products?new=true"), "HomeMiddle", 2),
                    Banner.Create("Miễn phí lắp đặt", WebsiteUrl.Create("https://localhost:5001/images/banner3.jpg"), 
                        "Miễn phí lắp đặt cho đơn hàng trên 5 triệu", WebsiteUrl.Create("https://localhost:5001/products"), "HomeBottom", 3)
                };

                await context.Banners.AddRangeAsync(banners);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedCustomersAsync(UserManager<ApplicationUser> userManager)
        {
            // Tạo 100 customers để có đủ dữ liệu train model
            var vietnameseNames = new[]
            {
                "Nguyễn Văn A", "Trần Thị B", "Lê Văn C", "Phạm Thị D", "Hoàng Văn E",
                "Huỳnh Thị F", "Phan Văn G", "Vũ Thị H", "Võ Văn I", "Đặng Thị K",
                "Đỗ Văn L", "Ngô Thị M", "Đinh Văn N", "Bùi Thị O", "Dương Văn P",
                "Lý Thị Q", "Đào Văn R", "Đinh Thị S", "Trương Văn T", "Vũ Thị U",
                "Hoàng Văn V", "Nguyễn Thị W", "Trần Văn X", "Lê Thị Y", "Phạm Văn Z",
                "Nguyễn Thị An", "Trần Văn Bình", "Lê Thị Cường", "Phạm Văn Dũng", "Hoàng Thị Em",
                "Huỳnh Văn Phúc", "Phan Thị Giang", "Vũ Văn Hùng", "Võ Thị Iris", "Đặng Văn Nhật",
                "Đỗ Thị Oanh", "Ngô Văn Phúc", "Đinh Thị Quỳnh", "Bùi Văn Sang", "Dương Thị Thảo",
                "Lý Văn Tuấn", "Đào Thị Uyên", "Đinh Văn Vương", "Trương Thị Xuân", "Vũ Văn Yến",
                "Hoàng Thị Zara", "Nguyễn Văn Anh", "Trần Thị Bảo", "Lê Văn Cảnh", "Phạm Thị Duy",
                "Hoàng Văn Em", "Huỳnh Thị Phương", "Phan Văn Quân", "Vũ Thị Rạng", "Đặng Văn Sáng",
                "Đỗ Thị Tâm", "Ngô Văn Út", "Đinh Thị Vân", "Bùi Văn Xuân", "Dương Thị Yến",
                "Lý Văn Zalo", "Đào Thị Anh", "Đinh Văn Bình", "Trương Thị Cúc", "Vũ Văn Duy"
            };

            for (int i = 1; i <= 100; i++)
            {
                var nameIndex = (i - 1) % vietnameseNames.Length;
                var userName = $"customer{i}";
                var email = $"customer{i}@example.com";
                var password = "Customer123!";
                var fullName = vietnameseNames[nameIndex] + $" {i}";

                var existingUser = await userManager.FindByNameAsync(userName);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = email,
                        FullName = fullName,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 730)) // 2 năm trước đến hiện tại
                    };

                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Customer");
                    }
                }
            }
        }

        private static async Task SeedOrdersAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!context.Orders.Any())
            {
                var products = await context.Products.Include(p => p.Variants).ToListAsync();
                var customers = await userManager.GetUsersInRoleAsync("Customer");
                var customerList = customers.ToList();

                var cities = new[] { "Hà Nội", "TP.HCM", "Đà Nẵng", "Hải Phòng", "Cần Thơ", "Nha Trang", "Huế", "Quảng Ninh" };
                var districts = new[] { "Quận 1", "Quận 2", "Quận 3", "Quận Thanh Xuân", "Quận Cầu Giấy", "Quận Hai Bà Trưng", "Quận Ba Đình", "Quận Hoàn Kiếm" };

                // Giảm xuống còn 10 đơn hàng tổng cộng
                var totalOrders = 10;
                var baseDate = DateTime.UtcNow.AddDays(-365);

                for (int i = 0; i < totalOrders; i++)
                {
                    var customer = customerList[random.Next(customerList.Count)];
                    var orderDate = baseDate.AddDays(random.Next(0, 365));
                    var selectedProducts = products.OrderBy(x => random.Next()).Take(random.Next(1, 4)).ToList();

                    var city = cities[random.Next(cities.Length)];
                    var district = districts[random.Next(districts.Length)];

                    var order = Order.Create(
                        customer.Id,
                        customer.FullName ?? $"Customer {customer.Id}",
                        $"09{random.Next(10000000, 99999999)}",
                        Address.Create($"{random.Next(1, 200)} Đường {vietnameseNames[random.Next(vietnameseNames.Length)]}",
                            $"Phường {random.Next(1, 20)}", district, city),
                        random.Next(0, 50000),
                        orderDate
                    );

                    // 30% dùng VNPay
                    if (random.Next(100) < 30)
                    {
                        order.SetPaymentMethod(PaymentMethod.VNPay);
                    }

                    foreach (var product in selectedProducts)
                    {
                        var variant = product.Variants.FirstOrDefault();
                        var unitPrice = variant != null ? variant.Price : Money.Vnd(random.Next(1000000, 50000000));
                        order.AddItem(product.Id, variant?.Id, random.Next(1, 3), unitPrice, product.RequiresInstallation);
                    }

                    var hasInstall = selectedProducts.Any(p => p.RequiresInstallation);
                    var hasShip = selectedProducts.Any(p => !p.RequiresInstallation);
                    order.Confirm(hasInstall, hasShip);

                    // Phân bổ trạng thái đa dạng
                    var statusRoll = random.Next(100);

                    if (hasShip && !hasInstall)
                    {
                        if (statusRoll < 60) // Tăng tỷ lệ hoàn thành
                        {
                            order.StartShipping();
                            order.MarkDelivered(customer.Id);
                        }
                        else if (statusRoll < 80)
                        {
                            order.StartShipping();
                        }
                        else if (statusRoll < 90)
                        {
                            order.Cancel("Khách hàng hủy đơn", customer.Id);
                        }
                    }
                    else if (hasInstall && !hasShip)
                    {
                        // Đơn chỉ có lắp đặt sẽ được xử lý trong SeedInstallationBookingsAsync
                        if (statusRoll < 10)
                        {
                            order.Cancel("Khách hàng hủy đơn", customer.Id);
                        }
                    }
                    else if (hasInstall && hasShip)
                    {
                        if (statusRoll < 40) // 40% đã giao hàng, chờ lắp đặt
                        {
                            order.StartShipping();
                            order.MarkDelivered(customer.Id);
                        }
                        else if (statusRoll < 10)
                        {
                            order.Cancel("Khách hàng hủy đơn", customer.Id);
                        }
                    }

                    context.Orders.Add(order);
                }
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTechniciansAsync(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            if (!context.TechnicianProfiles.Any())
            {
                var cities = new[] { "Hà Nội", "TP.HCM", "Đà Nẵng" };
                var districtsMap = new Dictionary<string, List<string>>
                {
                    ["Hà Nội"] = new List<string> { "Ba Đình", "Hoàn Kiếm", "Đống Đa", "Hai Bà Trưng", "Cầu Giấy", "Thanh Xuân" },
                    ["TP.HCM"] = new List<string> { "Quận 1", "Quận 2", "Quận 3", "Quận 7", "Quận 10", "Bình Thạnh" },
                    ["Đà Nẵng"] = new List<string> { "Hải Châu", "Thanh Khê", "Sơn Trà", "Ngũ Hành Sơn" }
                };

                var skills = new[] { "Lắp máy lạnh", "Lắp tủ lạnh", "Lắp máy giặt", "Lắp tivi", "Sửa chữa điện lạnh" };

                for (int i = 1; i <= 20; i++)
                {
                    var userName = $"technician{i}";
                    var email = $"technician{i}@smarthome.com";
                    var password = "Technician123!";
                    var fullName = vietnameseNames[random.Next(vietnameseNames.Length)] + $" (KTV{i})";
                    var city = cities[random.Next(cities.Length)];

                    var existingUser = await userManager.FindByNameAsync(userName);
                    ApplicationUser user;
                    if (existingUser == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = userName,
                            Email = email,
                            FullName = fullName,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(180, 730))
                        };

                        var result = await userManager.CreateAsync(user, password);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "Technician");
                        }
                    }
                    else
                    {
                        user = existingUser;
                    }

                    var techProfile = TechnicianProfile.Create(
                        fullName,
                        PhoneNumber.Create($"09{random.Next(10000000, 99999999)}"),
                        $"KTV{i:D4}",
                        city,
                        districtsMap[city].OrderBy(x => random.Next()).Take(random.Next(2, 5)).ToList(),
                        Email.Create(email),
                        baseSalary: Money.Vnd(random.Next(8000000, 15000000))
                    );

                    techProfile.LinkToUser(user.Id);

                    // Add random skills
                    var techSkills = skills.OrderBy(x => random.Next()).Take(random.Next(2, 4)).ToList();
                    foreach (var skill in techSkills)
                    {
                        techProfile.AddSkill(skill);
                    }

                    // Random rating and completed jobs
                    var completedJobs = random.Next(10, 100);
                    for (int j = 0; j < completedJobs; j++)
                    {
                        techProfile.CompleteJob(random.Next(3, 6));
                    }

                    await context.TechnicianProfiles.AddAsync(techProfile);
                }

                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedInstallationSlotsAsync(AppDbContext context)
        {
            if (!context.InstallationSlots.Any())
            {
                var technicians = await context.TechnicianProfiles.ToListAsync();

                // Tạo slots cho 30 ngày tới
                for (int day = 0; day < 30; day++)
                {
                    var date = DateTime.Today.AddDays(day);
                    var daySlots = new List<InstallationSlot>();
                    foreach (var tech in technicians)
                    {
                        // Mỗi kỹ thuật viên có 4 slots/ngày: 8-10, 10-12, 14-16, 16-18
                        var timeSlots = new[]
                        {
                            (new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
                            (new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)),
                            (new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0)),
                            (new TimeSpan(16, 0, 0), new TimeSpan(18, 0, 0))
                        };

                        foreach (var (start, end) in timeSlots)
                        {
                            daySlots.Add(InstallationSlot.Create(tech.Id, date, start, end));
                        }
                    }
                    await context.InstallationSlots.AddRangeAsync(daySlots);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedProductImagesAsync(AppDbContext context)
        {
            if (!context.ProductImages.Any())
            {
                var products = await context.Products.Include(p => p.Images).ToListAsync();
                var images = new List<ProductImage>();

                foreach (var product in products)
                {
                    if (!product.Images.Any())
                    {
                        // Thêm 1-3 ảnh cho mỗi sản phẩm
                        var imageCount = random.Next(1, 4);
                        for (int i = 0; i < imageCount; i++)
                        {
                            var image = ProductImage.Create(
                                product.Id,
                                $"https://via.placeholder.com/600x400?text={Uri.EscapeDataString(product.Name)}",
                                product.Name,
                                i == 0, // First image is main
                                i
                            );
                            images.Add(image);
                        }
                    }
                }

                await context.ProductImages.AddRangeAsync(images);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPromotionsAsync(AppDbContext context)
        {
            if (!context.Promotions.Any())
            {
                var products = await context.Products.ToListAsync();
                var promotions = new List<Promotion>();

                // Khuyến mãi hiện tại
                var promo1 = Promotion.Create(
                    "Giảm giá mùa hè 2026",
                    Percentage.Create(15),
                    DateTime.UtcNow.AddDays(-10),
                    DateTime.UtcNow.AddDays(20),
                    Money.Vnd(5000000)
                );
                promotions.Add(promo1);

                // Khuyến mãi sắp tới
                var promo2 = Promotion.Create(
                    "Black Friday 2026",
                    Percentage.Create(30),
                    DateTime.UtcNow.AddDays(30),
                    DateTime.UtcNow.AddDays(35),
                    Money.Vnd(10000000)
                );
                promotions.Add(promo2);

                // Khuyến mãi đã hết hạn
                var promo3 = Promotion.Create(
                    "Tết Nguyên Đán 2026",
                    Percentage.Create(20),
                    DateTime.UtcNow.AddDays(-60),
                    DateTime.UtcNow.AddDays(-30),
                    Money.Vnd(3000000)
                );
                promo3.Deactivate();
                promotions.Add(promo3);

                await context.Promotions.AddRangeAsync(promotions);
                await context.SaveChangesAsync();

                // Thêm sản phẩm vào promotion
                var promo1Products = products.OrderBy(x => random.Next()).Take(20).ToList();
                foreach (var product in promo1Products)
                {
                    var pp = PromotionProduct.Create(promo1.Id, product.Id, (Percentage?)null);
                    await context.Set<PromotionProduct>().AddAsync(pp);
                }

                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedStockEntriesAsync(AppDbContext context)
        {
            if (!context.StockEntries.Any())
            {
                var suppliers = await context.Suppliers.ToListAsync();
                var warehouses = await context.Warehouses.ToListAsync();
                var products = await context.Products.Include(p => p.Variants).ToListAsync();

                // Tạo 10 phiếu nhập kho
                for (int i = 0; i < 10; i++)
                {
                    var supplier = suppliers[random.Next(suppliers.Count)];
                    var warehouse = warehouses[random.Next(warehouses.Count)];

                    var entry = StockEntry.Create(supplier.Id, warehouse.Id, $"Nhập hàng lần {i + 1}");

                    // Thêm 5-10 sản phẩm vào phiếu
                    var entryProducts = products.OrderBy(x => random.Next()).Take(random.Next(5, 11)).ToList();
                    foreach (var product in entryProducts)
                    {
                        var variant = product.Variants.FirstOrDefault();
                        entry.AddItem(
                            product.Id,
                            random.Next(10, 50),
                            random.Next(1000000, 20000000),
                            variant?.Id
                        );
                    }

                    entry.Complete();
                    await context.StockEntries.AddAsync(entry);
                }

                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedInstallationBookingsAsync(AppDbContext context)
        {
            if (!context.InstallationBookings.Any())
            {
                var orders = await context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.Items.Any(i => i.RequiresInstallation))
                    .ToListAsync();

                var technicians = await context.TechnicianProfiles.ToListAsync();
                var slots = await context.InstallationSlots.Where(s => !s.IsBooked).ToListAsync();

                var bookings = new List<InstallationBooking>();
                var slotIndex = 0;

                foreach (var order in orders.Take(50)) // Chỉ tạo booking cho 50 đơn đầu
                {
                    if (slotIndex >= slots.Count) break;

                    var tech = technicians[random.Next(technicians.Count)];
                    var slot = slots[slotIndex++];

                    // Combine slot Date with StartTime to create proper ScheduledDate with time component
                    var scheduledDate = slot.Date.Add(slot.StartTime);
                    var booking = InstallationBooking.Create(order.Id, tech.Id, slot.Id, scheduledDate);
                    slot.Book(booking.Id);

                    // Random status
                    var statusRoll = random.Next(100);
                    if (statusRoll < 60) // 60% completed
                    {
                        booking.Accept();
                        booking.PrepareMaterials();
                        booking.StartTravel();
                        booking.StartInstallation();
                        booking.Complete("Signed", random.Next(4, 6));
                        
                        // Cập nhật trạng thái cho Order
                        foreach (var item in order.Items.Where(i => i.RequiresInstallation))
                        {
                            order.MarkItemInstalled(item.Id);
                        }
                    }
                    else if (statusRoll < 80) // 20% in progress
                    {
                        booking.Accept();
                        if (random.Next(2) == 0)
                        {
                            booking.PrepareMaterials();
                            if (random.Next(2) == 0)
                            {
                                booking.StartTravel();
                            }
                        }
                    }
                    // 20% còn lại ở trạng thái Assigned

                    bookings.Add(booking);
                }

                await context.InstallationBookings.AddRangeAsync(bookings);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedWarrantiesAsync(AppDbContext context)
        {
            if (!context.Warranties.Any())
            {
                var completedOrders = await context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.Status == OrderStatus.Completed)
                    .ToListAsync();

                var warranties = new List<Warranty>();

                foreach (var order in completedOrders.Take(100))
                {
                    foreach (var item in order.Items)
                    {
                        var warranty = Warranty.Create(
                            item.ProductId,
                            item.VariantId,
                            item.Id,
                            random.Next(12, 37) // 12-36 tháng
                        );
                        warranties.Add(warranty);
                    }
                }

                await context.Warranties.AddRangeAsync(warranties);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedWarrantyClaimsAsync(AppDbContext context)
        {
            if (!context.WarrantyClaims.Any())
            {
                var warranties = await context.Warranties.ToListAsync();
                var claims = new List<WarrantyClaim>();

                // Tạo claim cho 20% warranties
                foreach (var warranty in warranties.OrderBy(x => random.Next()).Take(warranties.Count / 5))
                {
                    var issues = new[]
                    {
                        "Không hoạt động",
                        "Kêu to bất thường",
                        "Rò rỉ nước",
                        "Không làm lạnh",
                        "Màn hình bị lỗi",
                        "Không khởi động được"
                    };

                    var claim = WarrantyClaim.Create(
                        warranty.Id,
                        warranty.ProductId,
                        warranty.VariantId,
                        warranty.OrderItemId,
                        issues[random.Next(issues.Length)]
                    );

                    // Random status
                    var statusRoll = random.Next(100);
                    if (statusRoll < 70) // 70% resolved
                    {
                        claim.Resolve("Đã sửa chữa thành công", true);
                    }
                    else if (statusRoll < 85) // 15% in progress
                    {
                        // Keep as Pending
                    }
                    else // 15% rejected
                    {
                        claim.Resolve("Không thuộc phạm vi bảo hành", false);
                    }

                    claims.Add(claim);
                }

                await context.WarrantyClaims.AddRangeAsync(claims);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedWarrantyRequestsAsync(AppDbContext context)
        {
            if (!context.WarrantyRequests.Any())
            {
                var warranties = await context.Warranties.ToListAsync();
                var requests = new List<WarrantyRequest>();

                // Tạo request cho 15% warranties
                foreach (var warranty in warranties.OrderBy(x => random.Next()).Take(warranties.Count / 7))
                {
                    var descriptions = new[]
                    {
                        "Yêu cầu kiểm tra và bảo dưỡng định kỳ",
                        "Sản phẩm hoạt động không ổn định",
                        "Cần thay thế linh kiện",
                        "Yêu cầu sửa chữa tại nhà"
                    };

                    // Get order item to find order id
                    var orderItem = context.OrderItems.FirstOrDefault(oi => oi.Id == warranty.OrderItemId);
                    if (orderItem == null || orderItem.OrderId == 0)
                    {
                        continue; // Skip if order item not found or has invalid order id
                    }

                    var request = WarrantyRequest.Create(
                        warranty.Id,
                        warranty.ProductId,
                        warranty.VariantId,
                        warranty.OrderItemId,
                        orderItem.OrderId,
                        WarrantyType.Repair,
                        descriptions[random.Next(descriptions.Length)]
                    );

                    // Random status
                    var statusRoll = random.Next(100);
                    if (statusRoll < 60) // 60% completed
                    {
                        request.Approve();
                        request.Start();
                        request.Complete("Đã hoàn thành bảo hành");
                    }
                    else if (statusRoll < 80) // 20% approved/in progress
                    {
                        request.Approve();
                        if (random.Next(2) == 0)
                        {
                            request.Start();
                        }
                    }
                    else if (statusRoll < 90) // 10% rejected
                    {
                        request.Reject("Không đủ điều kiện bảo hành");
                    }
                    // 10% còn lại ở trạng thái Pending

                    requests.Add(request);
                }

                await context.WarrantyRequests.AddRangeAsync(requests);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedReturnOrdersAsync(AppDbContext context)
        {
            if (!context.ReturnOrders.Any())
            {
                var completedOrders = await context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered)
                    .ToListAsync();

                var returnOrders = new List<ReturnOrder>();

                // Tạo return cho 5% đơn hàng
                foreach (var order in completedOrders.OrderBy(x => random.Next()).Take(completedOrders.Count / 20))
                {
                    var reasons = new[]
                    {
                        "Sản phẩm không đúng mô tả",
                        "Sản phẩm bị lỗi",
                        "Không còn nhu cầu sử dụng",
                        "Nhận được sản phẩm bị hư hỏng"
                    };

                    var returnOrder = ReturnOrder.Create(
                        order.Id,
                        random.Next(2) == 0 ? ReturnType.Refund : ReturnType.Exchange,
                        random.Next(2) == 0 ? ReturnMethod.Shipping : ReturnMethod.Technician,
                        reasons[random.Next(reasons.Length)]
                    );

                    // Thêm items
                    var returnItems = order.Items.OrderBy(x => random.Next()).Take(random.Next(1, order.Items.Count + 1)).ToList();
                    foreach (var item in returnItems)
                    {
                        returnOrder.AddItem(
                            item.Id,
                            item.ProductId,
                            item.VariantId,
                            random.Next(1, item.Quantity + 1),
                            "Lỗi sản phẩm",
                            random.Next(3) == 0 // 33% damaged
                        );
                    }

                    // Random status
                    var statusRoll = random.Next(100);
                    if (statusRoll < 60) // 60% completed
                    {
                        returnOrder.Approve(Money.Vnd(random.Next(1000000, 10000000)));
                        returnOrder.MarkReceived();
                        returnOrder.Complete();
                    }
                    else if (statusRoll < 80) // 20% approved/received
                    {
                        returnOrder.Approve(Money.Vnd(random.Next(1000000, 10000000)));
                        if (random.Next(2) == 0)
                        {
                            returnOrder.MarkReceived();
                        }
                    }
                    // 20% còn lại ở trạng thái Pending

                    returnOrders.Add(returnOrder);
                }

                await context.ReturnOrders.AddRangeAsync(returnOrders);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedProductCommentsAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!context.ProductComments.Any())
            {
                var products = await context.Products.ToListAsync();
                var customers = await userManager.GetUsersInRoleAsync("Customer");
                var customerList = customers.ToList();
                var completedOrders = await context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.Status == OrderStatus.Completed)
                    .ToListAsync();

                var comments = new List<ProductComment>();

                // Tạo 200 comments
                for (int i = 0; i < 200; i++)
                {
                    var product = products[random.Next(products.Count)];
                    var customer = customerList[random.Next(customerList.Count)];
                    var order = completedOrders.FirstOrDefault(o => o.UserId == customer.Id && o.Items.Any(item => item.ProductId == product.Id));

                    if (order == null) continue;

                    var commentTexts = new[]
                    {
                        "Sản phẩm rất tốt, đáng tiền",
                        "Chất lượng ổn, giao hàng nhanh",
                        "Sản phẩm đúng mô tả, hài lòng",
                        "Rất hài lòng với sản phẩm này",
                        "Tạm ổn, giá hơi cao",
                        "Sản phẩm chất lượng, sẽ mua lại",
                        "Giao hàng nhanh, đóng gói cẩn thận",
                        "Sản phẩm tốt nhưng hơi ồn",
                        "Đáng đồng tiền bát gạo",
                        "Chất lượng tốt, giá cả hợp lý"
                    };

                    var comment = ProductComment.Create(
                        product.Id,
                        customer.Id,
                        order.Id,
                        commentTexts[random.Next(commentTexts.Length)],
                        random.Next(3, 6), // 3-5 sao
                        true // verified purchase
                    );

                    // 80% được approve
                    if (random.Next(100) < 80)
                    {
                        comment.Approve();
                    }

                    comments.Add(comment);
                }

                await context.ProductComments.AddRangeAsync(comments);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTechnicianRatingsAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!context.TechnicianRatings.Any())
            {
                var completedBookings = await context.InstallationBookings
                    .Include(b => b.Order)
                    .Where(b => b.Status == InstallationStatus.Completed)
                    .ToListAsync();

                var customers = await userManager.GetUsersInRoleAsync("Customer");
                var customerList = customers.ToList();

                var ratings = new List<TechnicianRating>();

                foreach (var booking in completedBookings.Take(100))
                {
                    var customer = customerList.FirstOrDefault(c => c.Id == booking.Order.UserId);
                    if (customer == null) continue;

                    var ratingTexts = new[]
                    {
                        "Kỹ thuật viên nhiệt tình, lắp đặt nhanh gọn",
                        "Rất hài lòng với dịch vụ",
                        "Chuyên nghiệp, tận tâm",
                        "Lắp đặt cẩn thận, sạch sẽ",
                        "Kỹ thuật viên giỏi, nhiệt tình tư vấn",
                        "Dịch vụ tốt, sẽ giới thiệu cho bạn bè",
                        "Lắp đặt nhanh, kiểm tra kỹ",
                        "Rất chuyên nghiệp",
                        "Hài lòng với dịch vụ lắp đặt",
                        "Kỹ thuật viên thân thiện, nhiệt tình"
                    };

                    var rating = TechnicianRating.Create(
                        booking.TechnicianId,
                        customer.Id,
                        booking.Id,
                        ratingTexts[random.Next(ratingTexts.Length)],
                        random.Next(4, 6), // 4-5 sao
                        true // verified service
                    );

                    // 90% được approve
                    if (random.Next(100) < 90)
                    {
                        rating.Approve();
                    }

                    ratings.Add(rating);
                }

                await context.TechnicianRatings.AddRangeAsync(ratings);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedUserAddressesAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!context.Set<UserAddress>().Any())
            {
                var customers = await userManager.GetUsersInRoleAsync("Customer");
                var addresses = new List<UserAddress>();

                var cities = new[] { "Hà Nội", "TP.HCM", "Đà Nẵng", "Hải Phòng", "Cần Thơ" };
                var districts = new[] { "Quận 1", "Quận 2", "Ba Đình", "Hoàn Kiếm", "Thanh Xuân", "Cầu Giấy" };

                foreach (var customer in customers.Take(50))
                {
                    // Mỗi customer có 1-2 địa chỉ
                    var addressCount = random.Next(1, 3);
                    for (int i = 0; i < addressCount; i++)
                    {
                        var address = UserAddress.Create(
                            customer.Id,
                            i == 0 ? "Nhà riêng" : "Văn phòng",
                            customer.FullName ?? "Customer",
                            PhoneNumber.Create($"09{random.Next(10000000, 99999999)}"),
                            Address.Create(
                                $"{random.Next(1, 200)} Đường {vietnameseNames[random.Next(vietnameseNames.Length)]}",
                                $"Phường {random.Next(1, 20)}",
                                districts[random.Next(districts.Length)],
                                cities[random.Next(cities.Length)]
                            ),
                            i == 0
                        );
                        addresses.Add(address);
                    }
                }

                await context.Set<UserAddress>().AddRangeAsync(addresses);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPaymentTransactionsAsync(AppDbContext context)
        {
            if (!context.PaymentTransactions.Any())
            {
                var orders = await context.Orders.Where(o => o.PaymentMethod == PaymentMethod.VNPay).ToListAsync();
                var transactions = new List<PaymentTransaction>();

                foreach (var order in orders.Take(50))
                {
                    var transaction = PaymentTransaction.Create(
                        order.Id,
                        order.TotalAmount,
                        PaymentMethod.VNPay
                    );

                    // 90% success
                    if (random.Next(100) < 90)
                    {
                        transaction.MarkSuccess($"TXN{random.Next(100000, 999999)}");
                    }
                    else
                    {
                        transaction.MarkFailed("Giao dịch thất bại");
                    }

                    transactions.Add(transaction);
                }

                await context.PaymentTransactions.AddRangeAsync(transactions);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedShippingZonesAsync(AppDbContext context)
        {
            if (!context.Set<ShippingZone>().Any())
            {
                var zones = new List<ShippingZone>();

                var zone1 = ShippingZone.Create("Nội thành Hà Nội", "Các quận nội thành Hà Nội");
                var zone2 = ShippingZone.Create("Nội thành TP.HCM", "Các quận nội thành TP.HCM");
                var zone3 = ShippingZone.Create("Tỉnh/Thành khác", "Các tỉnh thành khác trên toàn quốc");

                zones.Add(zone1);
                zones.Add(zone2);
                zones.Add(zone3);

                await context.Set<ShippingZone>().AddRangeAsync(zones);
                await context.SaveChangesAsync();

                // Add rates
                var rates = new List<ShippingRate>();
                
                // Zone 1
                rates.Add(ShippingRate.Create(zone1.Id, 0, 5, 20000));
                rates.Add(ShippingRate.Create(zone1.Id, 5, 20, 50000));
                rates.Add(ShippingRate.Create(zone1.Id, 20, 100, 100000));

                // Zone 2
                rates.Add(ShippingRate.Create(zone2.Id, 0, 5, 25000));
                rates.Add(ShippingRate.Create(zone2.Id, 5, 20, 60000));
                rates.Add(ShippingRate.Create(zone2.Id, 20, 100, 120000));

                // Zone 3
                rates.Add(ShippingRate.Create(zone3.Id, 0, 5, 40000));
                rates.Add(ShippingRate.Create(zone3.Id, 5, 20, 100000));
                rates.Add(ShippingRate.Create(zone3.Id, 20, 100, 250000));

                await context.Set<ShippingRate>().AddRangeAsync(rates);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedInstallationMaterialsAsync(AppDbContext context)
        {
            if (!context.Set<InstallationMaterial>().Any())
            {
                var bookings = await context.InstallationBookings
                    .Where(b => b.Status == InstallationStatus.Completed || b.Status == InstallationStatus.Installing)
                    .ToListAsync();

                var products = await context.Products.ToListAsync();
                var warehouses = await context.Warehouses.ToListAsync();

                var materials = new List<InstallationMaterial>();

                foreach (var booking in bookings)
                {
                    // Mỗi booking dùng 1-3 loại vật tư
                    var materialCount = random.Next(1, 4);
                    for (int i = 0; i < materialCount; i++)
                    {
                        var product = products[random.Next(products.Count)];
                        var warehouse = warehouses[random.Next(warehouses.Count)];
                        
                        var material = InstallationMaterial.Create(
                            booking.Id,
                            product.Id,
                            random.Next(1, 11), // Taken 1-10
                            warehouse.Id
                        );

                        if (booking.Status == InstallationStatus.Completed)
                        {
                            material.RecordUsage(random.Next(1, material.QuantityTaken + 1));
                        }

                        materials.Add(material);
                    }
                }

                await context.Set<InstallationMaterial>().AddRangeAsync(materials);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedProductRatingsAsync(AppDbContext context)
        {
            if (!context.Set<ProductRating>().Any())
            {
                var completedOrders = await context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.Status == OrderStatus.Completed)
                    .ToListAsync();

                var ratings = new List<ProductRating>();

                foreach (var order in completedOrders)
                {
                    foreach (var item in order.Items)
                    {
                        // 70% chance of having a rating
                        if (random.Next(100) < 70)
                        {
                            var commentTexts = new[]
                            {
                                "Sản phẩm tuyệt vời!",
                                "Rất hài lòng với chất lượng",
                                "Giao hàng hơi chậm nhưng sản phẩm tốt",
                                "Đóng gói cẩn thận, sản phẩm đẹp",
                                "Sẽ tiếp tục ủng hộ shop",
                                "Chất lượng đúng như mô tả",
                                "Giá cả hợp lý",
                                "Nhân viên hỗ trợ nhiệt tình"
                            };

                            var rating = ProductRating.Create(
                                item.ProductId,
                                item.VariantId,
                                item.Id,
                                order.UserId,
                                random.Next(4, 6), // 4-5 stars
                                commentTexts[random.Next(commentTexts.Length)]
                            );
                            ratings.Add(rating);
                        }
                    }
                }

                await context.Set<ProductRating>().AddRangeAsync(ratings);
                await context.SaveChangesAsync();
            }
        }

        private static async Task EnsureProductSpecsAsync(AppDbContext context)
        {
            var products = await context.Products
                .Include(p => p.Category)
                .Where(p => string.IsNullOrEmpty(p.SpecsJson) || p.SpecsJson == "{}")
                .ToListAsync();

            if (products.Any())
            {
                foreach (var product in products)
                {
                    var attributes = GetProductAttributes(product.Category.Name, product.Name);
                    product.UpdateSpecs(attributes);
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
