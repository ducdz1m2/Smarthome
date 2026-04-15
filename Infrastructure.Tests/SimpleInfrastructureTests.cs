using Domain.Entities.Catalog;
using Domain.Entities.Sales;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests;

/// <summary>
/// Simple Infrastructure tests using InMemory database
/// </summary>
public class SimpleInfrastructureTests : IDisposable
{
    private readonly AppDbContext _context;

    public SimpleInfrastructureTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void DbContext_Should_Initialize_With_All_DbSets()
    {
        // Assert
        _context.Products.Should().NotBeNull();
        _context.Orders.Should().NotBeNull();
        _context.OrderItems.Should().NotBeNull();
        _context.Categories.Should().NotBeNull();
        _context.Brands.Should().NotBeNull();
        _context.Warehouses.Should().NotBeNull();
        _context.InstallationBookings.Should().NotBeNull();
        _context.Warranties.Should().NotBeNull();
    }

    [Fact]
    public async Task Product_Should_Save_And_Retrieve()
    {
        // Arrange
        var product = Product.Create(
            "Test Product",
            "TEST-001",
            Money.Vnd(1000000),
            1,
            1);

        // Act
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Products.FindAsync(product.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Product");
        saved.Sku.Value.Should().Be("TEST-001");
        saved.BasePrice.Amount.Should().Be(1000000);
    }

    [Fact]
    public async Task Order_Should_Save_With_Address_ValueObject()
    {
        // Arrange
        var address = Address.Create(
            "123 Street", "Ward 1", "District 1",
            "HCMC", "Vietnam", "70000");

        var order = Order.Create(
            1, "Test Customer", "0901234567", address);

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Orders.FindAsync(order.Id);
        saved.Should().NotBeNull();
        saved!.ReceiverName.Should().Be("Test Customer");
        saved.ShippingAddressStreet.Should().Be("123 Street");
        saved.ShippingAddressCity.Should().Be("HCMC");
    }

    [Fact]
    public async Task Order_With_Items_Should_Cascade_Save()
    {
        // Arrange
        var order = Order.Create(1, "Test", "0901234567",
            Address.Create("123", "W1", "D1", "HCMC", "VN", "70000"));
        order.AddItem(1, null, 2, Money.Vnd(100000), false);
        order.AddItem(2, null, 1, Money.Vnd(500000), true);

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        saved.Should().NotBeNull();
        saved!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task OrderItems_Should_Cascade_Delete()
    {
        // Arrange
        var order = Order.Create(1, "Test", "0901234567",
            Address.Create("123", "W1", "D1", "HCMC", "VN", "70000"));
        order.AddItem(1, null, 2, Money.Vnd(100000), false);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var itemCountBefore = await _context.OrderItems.CountAsync();

        // Act
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        // Assert
        var itemCountAfter = await _context.OrderItems.CountAsync();
        itemCountAfter.Should().Be(itemCountBefore - 1);
    }

    [Fact]
    public async Task Product_Can_Be_Updated()
    {
        // Arrange
        var product = Product.Create("Original", "UPDATE-001", Money.Vnd(100000), 1, 1);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        product.UpdateInfo("Updated Name", product.Description, Money.Vnd(200000));
        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        // Clear tracking
        _context.ChangeTracker.Clear();

        // Assert
        var saved = await _context.Products.FindAsync(product.Id);
        saved!.Name.Should().Be("Updated Name");
        saved.BasePrice.Amount.Should().Be(200000);
    }

    [Fact]
    public async Task InstallationBooking_Should_Save_And_Retrieve()
    {
        // Arrange
        var booking = Domain.Entities.Installation.InstallationBooking.Create(
            1, 1, 1, DateTime.UtcNow.AddDays(2));

        // Act
        _context.InstallationBookings.Add(booking);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.InstallationBookings.FindAsync(booking.Id);
        saved.Should().NotBeNull();
        saved!.OrderId.Should().Be(1);
        saved.Status.Should().Be(Domain.Enums.InstallationStatus.Assigned);
    }

    [Fact]
    public async Task Multiple_Operations_In_Single_Context_Should_Work()
    {
        // Arrange
        var product = Product.Create("Product", "MULTI-001", Money.Vnd(100000), 1, 1);
        var order = Order.Create(1, "Customer", "0901234567",
            Address.Create("123", "W1", "D1", "HCMC", "VN", "70000"));
        order.AddItem(1, null, 2, Money.Vnd(50000), false);

        // Act
        _context.Products.Add(product);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var products = await _context.Products.ToListAsync();
        var orders = await _context.Orders.Include(o => o.Items).ToListAsync();

        products.Should().HaveCount(1);
        orders.Should().HaveCount(1);
        orders.First().Items.Should().HaveCount(1);
    }
}
