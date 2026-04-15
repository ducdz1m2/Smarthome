using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Entities.Sales;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace Application.Tests;

public class WarrantyRequestServiceTests
{
    private readonly Mock<IWarrantyRequestRepository> _warrantyRequestRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInstallationService> _installationServiceMock;
    private readonly Mock<IInstallationSlotService> _installationSlotServiceMock;
    private readonly Mock<ITechnicianProfileService> _technicianProfileServiceMock;
    private readonly WarrantyRequestService _warrantyRequestService;

    public WarrantyRequestServiceTests()
    {
        _warrantyRequestRepositoryMock = new Mock<IWarrantyRequestRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _installationServiceMock = new Mock<IInstallationService>();
        _installationSlotServiceMock = new Mock<IInstallationSlotService>();
        _technicianProfileServiceMock = new Mock<ITechnicianProfileService>();

        _warrantyRequestService = new WarrantyRequestService(
            _warrantyRequestRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _installationServiceMock.Object,
            _installationSlotServiceMock.Object,
            _technicianProfileServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_WarrantyRequests()
    {
        // Arrange
        var warrantyRequests = new List<WarrantyRequest>
        {
            WarrantyRequest.Create(1, WarrantyType.Repair, "Product defect"),
            WarrantyRequest.Create(2, WarrantyType.Replace, "Product broken")
        };

        _warrantyRequestRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(warrantyRequests);

        // Act
        var result = await _warrantyRequestService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_WarrantyRequest_When_Exists()
    {
        // Arrange
        var warrantyRequest = WarrantyRequest.Create(1, WarrantyType.Repair, "Product defect");
        typeof(WarrantyRequest).GetProperty("Id")?.SetValue(warrantyRequest, 1);

        _warrantyRequestRepositoryMock.Setup(x => x.GetByIdWithItemsAsync(1)).ReturnsAsync(warrantyRequest);

        // Act
        var result = await _warrantyRequestService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_WarrantyRequest()
    {
        // Arrange
        var order = Domain.Entities.Sales.Order.Create(
            1,
            "Customer Name",
            "0901234567",
            "Street",
            "Ward",
            "District",
            "City");
        typeof(Domain.Entities.Sales.Order).GetProperty("Id")?.SetValue(order, 1);

        var request = new CreateWarrantyRequestRequest
        {
            OrderId = 1,
            WarrantyType = WarrantyType.Repair,
            Description = "Product defect",
            Items = new List<WarrantyRequestItemDto>
            {
                new WarrantyRequestItemDto { OrderItemId = 1, Quantity = 1, Description = "Item 1" }
            }
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _warrantyRequestRepositoryMock.Setup(x => x.ExistsPendingWarrantyForOrderAsync(1)).ReturnsAsync(false);
        _warrantyRequestRepositoryMock.Setup(x => x.AddAsync(It.IsAny<WarrantyRequest>())).Callback<WarrantyRequest>(w =>
        {
            typeof(WarrantyRequest).GetProperty("Id")?.SetValue(w, 1);
        });
        _warrantyRequestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _warrantyRequestService.CreateAsync(request);

        // Assert
        result.Should().Be(1);
        _warrantyRequestRepositoryMock.Verify(x => x.AddAsync(It.IsAny<WarrantyRequest>()), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_Should_Approve_WarrantyRequest_And_Create_Booking()
    {
        // Arrange
        var warrantyRequest = WarrantyRequest.Create(1, WarrantyType.Repair, "Product defect");
        typeof(WarrantyRequest).GetProperty("Id")?.SetValue(warrantyRequest, 1);

        var order = Domain.Entities.Sales.Order.Create(
            1,
            "Customer Name",
            "0901234567",
            "Street",
            "Ward",
            "District",
            "City");
        typeof(Domain.Entities.Sales.Order).GetProperty("Id")?.SetValue(order, 1);

        var technician = new TechnicianResponse { Id = 1 };
        var slot = new InstallationSlotResponse { Id = 1, TechnicianId = 1, Date = DateTime.UtcNow.AddDays(1) };

        _warrantyRequestRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(warrantyRequest);
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _technicianProfileServiceMock.Setup(x => x.GetAvailableAsync()).ReturnsAsync(new List<TechnicianResponse> { technician });
        _installationSlotServiceMock.Setup(x => x.GetAvailableSlotsAsync(1, It.IsAny<DateTime>())).ReturnsAsync(new List<InstallationSlotResponse> { slot });
        _installationServiceMock.Setup(x => x.CreateAsync(It.IsAny<CreateInstallationBookingRequest>())).ReturnsAsync(1);
        _warrantyRequestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _warrantyRequestService.ApproveAsync(1);

        // Assert
        warrantyRequest.Status.Should().Be(WarrantyRequestStatus.Approved);
        _installationServiceMock.Verify(x => x.CreateAsync(It.IsAny<CreateInstallationBookingRequest>()), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_Should_Create_Booking_With_IsWarranty_True()
    {
        // Arrange
        var warrantyRequest = WarrantyRequest.Create(1, WarrantyType.Repair, "Product defect");
        typeof(WarrantyRequest).GetProperty("Id")?.SetValue(warrantyRequest, 1);

        var order = Domain.Entities.Sales.Order.Create(
            1,
            "Customer Name",
            "0901234567",
            "Street",
            "Ward",
            "District",
            "City");
        typeof(Domain.Entities.Sales.Order).GetProperty("Id")?.SetValue(order, 1);

        var technician = new TechnicianResponse { Id = 1 };
        var slot = new InstallationSlotResponse { Id = 1, TechnicianId = 1, Date = DateTime.UtcNow.AddDays(1) };

        _warrantyRequestRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(warrantyRequest);
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _technicianProfileServiceMock.Setup(x => x.GetAvailableAsync()).ReturnsAsync(new List<TechnicianResponse> { technician });
        _installationSlotServiceMock.Setup(x => x.GetAvailableSlotsAsync(1, It.IsAny<DateTime>())).ReturnsAsync(new List<InstallationSlotResponse> { slot });
        _installationServiceMock.Setup(x => x.CreateAsync(It.IsAny<CreateInstallationBookingRequest>())).ReturnsAsync(1);
        _warrantyRequestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _warrantyRequestService.ApproveAsync(1);

        // Assert
        _installationServiceMock.Verify(x => x.CreateAsync(It.Is<CreateInstallationBookingRequest>(r => r.IsWarranty == true)), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_Should_Reject_WarrantyRequest()
    {
        // Arrange
        var warrantyRequest = WarrantyRequest.Create(1, WarrantyType.Repair, "Product defect");
        typeof(WarrantyRequest).GetProperty("Id")?.SetValue(warrantyRequest, 1);

        _warrantyRequestRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(warrantyRequest);
        _warrantyRequestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _warrantyRequestService.RejectAsync(1, "Not approved");

        // Assert
        warrantyRequest.Status.Should().Be(WarrantyRequestStatus.Rejected);
    }

    [Fact]
    public async Task StartAsync_Should_Start_WarrantyRequest()
    {
        // Arrange
        var warrantyRequest = WarrantyRequest.Create(1, WarrantyType.Repair, "Product defect");
        typeof(WarrantyRequest).GetProperty("Id")?.SetValue(warrantyRequest, 1);
        warrantyRequest.Approve();

        _warrantyRequestRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(warrantyRequest);
        _warrantyRequestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _warrantyRequestService.StartAsync(1);

        // Assert
        warrantyRequest.Status.Should().Be(WarrantyRequestStatus.InProgress);
    }

    [Fact]
    public async Task CompleteAsync_Should_Complete_WarrantyRequest()
    {
        // Arrange
        var warrantyRequest = WarrantyRequest.Create(1, WarrantyType.Repair, "Product defect");
        typeof(WarrantyRequest).GetProperty("Id")?.SetValue(warrantyRequest, 1);
        warrantyRequest.Approve();
        warrantyRequest.Start();

        _warrantyRequestRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(warrantyRequest);
        _warrantyRequestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _warrantyRequestService.CompleteAsync(1, "Completed successfully");

        // Assert
        warrantyRequest.Status.Should().Be(WarrantyRequestStatus.Completed);
    }
}
