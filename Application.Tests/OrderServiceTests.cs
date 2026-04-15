using Application.DTOs.Requests;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities.Catalog;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IInstallationService> _installationServiceMock;
    private readonly Mock<IInstallationSlotService> _installationSlotServiceMock;
    private readonly Mock<ITechnicianProfileService> _technicianProfileServiceMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _installationServiceMock = new Mock<IInstallationService>();
        _installationSlotServiceMock = new Mock<IInstallationSlotService>();
        _technicianProfileServiceMock = new Mock<ITechnicianProfileService>();
        _orderService = new OrderService(_orderRepositoryMock.Object, _productRepositoryMock.Object, _installationServiceMock.Object, _installationSlotServiceMock.Object, _technicianProfileServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Orders()
    {
        // Arrange
        var orders = new List<Order>
        {
            Order.Create(1, "Customer 1", "0901234567", Address.Create("123", "W1", "D1", "HCMC", "VN", "70000")),
            Order.Create(2, "Customer 2", "0901234568", Address.Create("456", "W2", "D2", "HCMC", "VN", "70000"))
        };
        _orderRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().ReceiverName.Should().Be("Customer 1");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Order_With_Details()
    {
        // Arrange
        var address = Address.Create("123 Street", "Ward 1", "District 1", "HCMC", "Vietnam", "70000");
        var order = Order.Create(1, "Test Customer", "0901234567", address);
        typeof(Order).GetProperty("Id")?.SetValue(order, 1);
        
        // Add an item to the order using reflection on the private Items collection
        var orderItem = OrderItem.Create(1, 1, null, 2, Money.Vnd(100000), false);
        typeof(OrderItem).GetProperty("OrderId")?.SetValue(orderItem, 1);
        
        // Add item to order through reflection since Items is private
        var itemsProperty = typeof(Order).GetProperty("Items");
        var items = itemsProperty?.GetValue(order) as ICollection<OrderItem>;
        items?.Add(orderItem);
        
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(order);

        // Act
        var result = await _orderService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.ReceiverName.Should().Be("Test Customer");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        _orderRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(999)).ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Order_With_Items()
    {
        // Arrange
        var orderItems = new List<CreateOrderItemRequest>
        {
            new CreateOrderItemRequest { ProductId = 1, Quantity = 2 }
        };

        var request = new CreateOrderRequest
        {
            UserId = 1,
            ReceiverName = "Test Customer",
            ReceiverPhone = "0901234567",
            ShippingStreet = "123 Street",
            ShippingWard = "Ward 1",
            ShippingDistrict = "District 1",
            ShippingCity = "HCMC",
            Items = orderItems
        };

        var product = Product.Create("Test Product", "SKU-001", 100000m, 1, 1);
        typeof(Product).GetProperty("Id")?.SetValue(product, 1);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
        _orderRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Order>())).Callback<Order>(o =>
        {
            typeof(Order).GetProperty("Id")?.SetValue(o, 1);
            typeof(Order).GetProperty("OrderNumber")?.SetValue(o, "ORD-001");
        });
        _orderRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.CreateAsync(request);

        // Assert
        result.Should().Be(1);
        _orderRepositoryMock.Verify(x => x.AddAsync(It.Is<Order>(o => 
            o.ReceiverName == "Test Customer" && 
            o.UserId == 1)), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Update_Order_Status()
    {
        // Arrange
        var address = Address.Create("123", "W1", "D1", "HCMC", "VN", "70000");
        var order = Order.Create(1, "Test Customer", "0901234567", address);
        typeof(Order).GetProperty("Id")?.SetValue(order, 1);
        
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var statusRequest = new UpdateOrderStatusRequest { Status = "Confirmed" };
        await _orderService.UpdateStatusAsync(1, statusRequest);

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task CancelAsync_Should_Cancel_Order()
    {
        // Arrange
        var address = Address.Create("123", "W1", "D1", "HCMC", "VN", "70000");
        var order = Order.Create(1, "Test Customer", "0901234567", address);
        typeof(Order).GetProperty("Id")?.SetValue(order, 1);
        
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _orderService.CancelAsync(1, "Customer request");

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_User_Orders()
    {
        // Arrange
        var address = Address.Create("123", "W1", "D1", "HCMC", "VN", "70000");
        var orders = new List<Order>
        {
            Order.Create(1, "Customer 1", "0901234567", address),
            Order.Create(1, "Customer 1", "0901234568", address)
        };
        
        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetByUserIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

}
