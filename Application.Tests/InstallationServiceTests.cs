using Application.DTOs.Requests;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities.Installation;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests;

public class InstallationServiceTests
{
    private readonly Mock<IInstallationBookingRepository> _bookingRepositoryMock;
    private readonly Mock<ITechnicianProfileRepository> _technicianRepositoryMock;
    private readonly Mock<IInstallationSlotRepository> _slotRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IReturnOrderRepository> _returnOrderRepositoryMock;
    private readonly Mock<IWarrantyRequestRepository> _warrantyRequestRepositoryMock;
    private readonly InstallationService _installationService;

    public InstallationServiceTests()
    {
        _bookingRepositoryMock = new Mock<IInstallationBookingRepository>();
        _technicianRepositoryMock = new Mock<ITechnicianProfileRepository>();
        _slotRepositoryMock = new Mock<IInstallationSlotRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _returnOrderRepositoryMock = new Mock<IReturnOrderRepository>();
        _warrantyRequestRepositoryMock = new Mock<IWarrantyRequestRepository>();

        _installationService = new InstallationService(
            _bookingRepositoryMock.Object,
            _technicianRepositoryMock.Object,
            _slotRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _returnOrderRepositoryMock.Object,
            _warrantyRequestRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Bookings()
    {
        // Arrange
        var bookings = new List<InstallationBooking>
        {
            InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(2)),
            InstallationBooking.Create(2, 2, 1, DateTime.UtcNow.AddDays(3))
        };
        
        _bookingRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = await _installationService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Booking_When_Exists()
    {
        // Arrange
        var booking = InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(2));
        typeof(InstallationBooking).GetProperty("Id")?.SetValue(booking, 1);
        
        _bookingRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(booking);

        // Act
        var result = await _installationService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(1);
    }

    [Fact]
    public async Task GetByOrderIdAsync_Should_Return_Booking_When_Exists()
    {
        // Arrange
        var booking = InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(2));
        _bookingRepositoryMock.Setup(x => x.GetByOrderIdAsync(1)).ReturnsAsync(booking);

        // Act
        var result = await _installationService.GetByOrderIdAsync(1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Booking_And_Return_Id()
    {
        // Arrange
        var request = new CreateInstallationBookingRequest
        {
            OrderId = 1,
            TechnicianId = 1,
            SlotId = 1,
            ScheduledDate = DateTime.UtcNow.AddDays(2)
        };

        var technician = TechnicianProfile.Create(
            "John Tech", 
            PhoneNumber.Create("0901234567"), 
            "TECH001", 
            "HCMC", 
            new List<string> { "District 1", "District 2" },
            null, null, null, null, null);

        var slot = InstallationSlot.Create(1, DateTime.UtcNow.AddDays(2), TimeSpan.FromHours(9), TimeSpan.FromHours(11));
        typeof(InstallationSlot).GetProperty("Id")?.SetValue(slot, 1);

        _technicianRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(technician);
        _slotRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(slot);
        _bookingRepositoryMock.Setup(x => x.ExistsByOrderIdAsync(1)).ReturnsAsync(false);
        _bookingRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InstallationBooking>())).Callback<InstallationBooking>(b =>
        {
            typeof(InstallationBooking).GetProperty("Id")?.SetValue(b, 1);
        });
        _bookingRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _slotRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _installationService.CreateAsync(request);

        // Assert
        result.Should().Be(1);
        _bookingRepositoryMock.Verify(x => x.AddAsync(It.Is<InstallationBooking>(b => 
            b.OrderId == 1 && 
            b.TechnicianId == 1)), Times.Once);
    }

    [Fact]
    public async Task AssignTechnicianAsync_Should_Assign_Technician_To_Booking()
    {
        // Arrange - Create booking with initial technician/slot
        var booking = InstallationBooking.Create(1, 2, 2, DateTime.UtcNow.AddDays(2));
        typeof(InstallationBooking).GetProperty("Id")?.SetValue(booking, 1);
        
        var technician = TechnicianProfile.Create(
            "John Tech", 
            PhoneNumber.Create("0901234567"), 
            "TECH001", 
            "HCMC", 
            new List<string> { "District 1" });
        typeof(TechnicianProfile).GetProperty("Id")?.SetValue(technician, 1);

        var newSlot = InstallationSlot.Create(1, DateTime.UtcNow.AddDays(2), TimeSpan.FromHours(14), TimeSpan.FromHours(16));
        typeof(InstallationSlot).GetProperty("Id")?.SetValue(newSlot, 1);

        _bookingRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(booking);
        _technicianRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(technician);
        _slotRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(newSlot);
        _bookingRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act - Reassign to technician 1 with slot 1
        await _installationService.AssignTechnicianAsync(1, 1, 1);

        // Assert
        booking.TechnicianId.Should().Be(1);
        booking.Status.Should().Be(InstallationStatus.TechnicianAssigned);
    }

    [Fact]
    public async Task CompleteAsync_Should_Complete_Booking()
    {
        // Arrange - Set up booking in proper state for completion
        var booking = InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(-1));
        typeof(InstallationBooking).GetProperty("Id")?.SetValue(booking, 1);

        // Use reflection to set status directly to Installing (to test Complete without full workflow)
        var statusProperty = typeof(InstallationBooking).GetProperty("Status");
        statusProperty?.SetValue(booking, InstallationStatus.Installing);

        // Set up order
        var order = Order.Create(
            1,
            "Customer Name",
            "0901234567",
            "Street",
            "Ward",
            "District",
            "City");
        typeof(Order).GetProperty("Id")?.SetValue(order, 1);

        _bookingRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(booking);
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _bookingRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _orderRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var request = new CompleteInstallationRequest
        {
            CustomerSignature = "customer-signature-123",
            CustomerRating = 5,
            Notes = "Great service!"
        };
        await _installationService.CompleteAsync(1, request);

        // Assert
        booking.Status.Should().Be(InstallationStatus.Completed);
        booking.CustomerRating.Should().Be(5);
    }

    [Fact]
    public async Task GetByTechnicianAsync_Should_Return_Technician_Bookings()
    {
        // Arrange
        var bookings = new List<InstallationBooking>
        {
            InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(2)),
            InstallationBooking.Create(2, 1, 1, DateTime.UtcNow.AddDays(3))
        };
        
        _bookingRepositoryMock.Setup(x => x.GetByTechnicianIdAsync(1)).ReturnsAsync(bookings);

        // Act
        var result = await _installationService.GetByTechnicianAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CompleteAsync_WhenUninstallBooking_Should_UpdateReturnOrderStatus()
    {
        // Arrange - Set up uninstall booking
        var booking = InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(-1));
        typeof(InstallationBooking).GetProperty("Id")?.SetValue(booking, 1);
        booking.SetIsUninstall(true);

        var statusProperty = typeof(InstallationBooking).GetProperty("Status");
        statusProperty?.SetValue(booking, InstallationStatus.Installing);

        // Set up return order - set status to Approved
        var returnOrder = ReturnOrder.Create(1, ReturnType.Exchange, "Product defect");
        typeof(ReturnOrder).GetProperty("Id")?.SetValue(returnOrder, 1);
        var returnOrderStatusProperty = typeof(ReturnOrder).GetProperty("Status");
        returnOrderStatusProperty?.SetValue(returnOrder, ReturnOrderStatus.Approved);

        // Set up order
        var order = Order.Create(
            1,
            "Customer Name",
            "0901234567",
            "Street",
            "Ward",
            "District",
            "City");
        typeof(Order).GetProperty("Id")?.SetValue(order, 1);

        _bookingRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(booking);
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _returnOrderRepositoryMock.Setup(x => x.GetByOrderIdAsync(1)).ReturnsAsync(new List<ReturnOrder> { returnOrder });
        _returnOrderRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var request = new CompleteInstallationRequest
        {
            CustomerSignature = "customer-signature-123",
            CustomerRating = 5,
            Notes = "Uninstall completed"
        };
        await _installationService.CompleteAsync(1, request);

        // Assert
        booking.Status.Should().Be(InstallationStatus.Completed);
        _returnOrderRepositoryMock.Verify(x => x.Update(It.IsAny<ReturnOrder>()), Times.AtLeastOnce);
        _returnOrderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.AtLeast(2));
    }

    [Fact]
    public async Task CompleteAsync_WhenRegularBooking_Should_NotUpdateReturnOrder()
    {
        // Arrange - Set up regular installation booking (not uninstall)
        var booking = InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(-1));
        typeof(InstallationBooking).GetProperty("Id")?.SetValue(booking, 1);
        booking.SetIsUninstall(false);
        
        var statusProperty = typeof(InstallationBooking).GetProperty("Status");
        statusProperty?.SetValue(booking, InstallationStatus.Installing);

        // Set up order
        var order = Order.Create(
            1, 
            "Customer Name", 
            "0901234567", 
            "Street", 
            "Ward", 
            "District", 
            "City");
        typeof(Order).GetProperty("Id")?.SetValue(order, 1);

        _bookingRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(booking);
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var request = new CompleteInstallationRequest
        {
            CustomerSignature = "customer-signature-123",
            CustomerRating = 5,
            Notes = "Installation completed"
        };
        await _installationService.CompleteAsync(1, request);

        // Assert
        booking.Status.Should().Be(InstallationStatus.Completed);
        _returnOrderRepositoryMock.Verify(x => x.Update(It.IsAny<ReturnOrder>()), Times.Never);
    }

    [Fact]
    public async Task CompleteAsync_WhenWarrantyBooking_Should_UpdateWarrantyRequestStatusToCompleted()
    {
        // Arrange - Set up warranty booking
        var booking = InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(-1));
        typeof(InstallationBooking).GetProperty("Id")?.SetValue(booking, 1);
        booking.SetIsWarranty(true);

        var statusProperty = typeof(InstallationBooking).GetProperty("Status");
        statusProperty?.SetValue(booking, InstallationStatus.Installing);

        // Set up warranty request
        var warrantyRequest = Domain.Entities.Sales.WarrantyRequest.Create(1, Domain.Enums.WarrantyType.Repair, "Test warranty");
        typeof(Domain.Entities.Sales.WarrantyRequest).GetProperty("Id")?.SetValue(warrantyRequest, 1);
        typeof(Domain.Entities.Sales.WarrantyRequest).GetProperty("Status")?.SetValue(warrantyRequest, Domain.Enums.WarrantyRequestStatus.Approved);

        // Set up order
        var order = Order.Create(
            1,
            "Customer Name",
            "0901234567",
            "Street",
            "Ward",
            "District",
            "City");
        typeof(Order).GetProperty("Id")?.SetValue(order, 1);

        _bookingRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(booking);
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _warrantyRequestRepositoryMock.Setup(x => x.GetByOrderIdAsync(1)).ReturnsAsync(new List<Domain.Entities.Sales.WarrantyRequest> { warrantyRequest });
        _warrantyRequestRepositoryMock.Setup(x => x.Update(It.IsAny<Domain.Entities.Sales.WarrantyRequest>())).Verifiable();
        _warrantyRequestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _bookingRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var request = new CompleteInstallationRequest
        {
            CustomerSignature = "customer-signature-123",
            CustomerRating = 5,
            Notes = "Warranty completed"
        };
        await _installationService.CompleteAsync(1, request);

        // Assert
        booking.Status.Should().Be(InstallationStatus.Completed);
        warrantyRequest.Status.Should().Be(Domain.Enums.WarrantyRequestStatus.Completed);
        warrantyRequest.StartedAt.Should().NotBeNull();
        warrantyRequest.CompletedAt.Should().NotBeNull();
        warrantyRequest.TechnicianNotes.Should().Be("Warranty completed");
        _warrantyRequestRepositoryMock.Verify(x => x.Update(It.IsAny<Domain.Entities.Sales.WarrantyRequest>()), Times.Once);
        _warrantyRequestRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WhenWarrantyBooking_And_WarrantyRequestInProgress_Should_UpdateToCompleted()
    {
        // Arrange - Set up warranty booking
        var booking = InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(-1));
        typeof(InstallationBooking).GetProperty("Id")?.SetValue(booking, 1);
        booking.SetIsWarranty(true);

        var statusProperty = typeof(InstallationBooking).GetProperty("Status");
        statusProperty?.SetValue(booking, InstallationStatus.Installing);

        // Set up warranty request already in InProgress status
        var warrantyRequest = Domain.Entities.Sales.WarrantyRequest.Create(1, Domain.Enums.WarrantyType.Repair, "Test warranty");
        typeof(Domain.Entities.Sales.WarrantyRequest).GetProperty("Id")?.SetValue(warrantyRequest, 1);
        typeof(Domain.Entities.Sales.WarrantyRequest).GetProperty("Status")?.SetValue(warrantyRequest, Domain.Enums.WarrantyRequestStatus.InProgress);

        // Set up order
        var order = Order.Create(
            1,
            "Customer Name",
            "0901234567",
            "Street",
            "Ward",
            "District",
            "City");
        typeof(Order).GetProperty("Id")?.SetValue(order, 1);

        _bookingRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(booking);
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _warrantyRequestRepositoryMock.Setup(x => x.GetByOrderIdAsync(1)).ReturnsAsync(new List<Domain.Entities.Sales.WarrantyRequest> { warrantyRequest });
        _warrantyRequestRepositoryMock.Setup(x => x.Update(It.IsAny<Domain.Entities.Sales.WarrantyRequest>())).Verifiable();
        _warrantyRequestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _bookingRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var request = new CompleteInstallationRequest
        {
            CustomerSignature = "customer-signature-123",
            CustomerRating = 5,
            Notes = "Warranty completed"
        };
        await _installationService.CompleteAsync(1, request);

        // Assert
        booking.Status.Should().Be(InstallationStatus.Completed);
        warrantyRequest.Status.Should().Be(Domain.Enums.WarrantyRequestStatus.Completed);
        warrantyRequest.CompletedAt.Should().NotBeNull();
        warrantyRequest.TechnicianNotes.Should().Be("Warranty completed");
        _warrantyRequestRepositoryMock.Verify(x => x.Update(It.IsAny<Domain.Entities.Sales.WarrantyRequest>()), Times.Once);
        _warrantyRequestRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WhenWarrantyBooking_And_WarrantyRequestCompleted_Should_NotUpdate()
    {
        // Arrange - Set up warranty booking
        var booking = InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(-1));
        typeof(InstallationBooking).GetProperty("Id")?.SetValue(booking, 1);
        booking.SetIsWarranty(true);

        var statusProperty = typeof(InstallationBooking).GetProperty("Status");
        statusProperty?.SetValue(booking, InstallationStatus.Installing);

        // Set up warranty request already in Completed status
        var warrantyRequest = Domain.Entities.Sales.WarrantyRequest.Create(1, Domain.Enums.WarrantyType.Repair, "Test warranty");
        typeof(Domain.Entities.Sales.WarrantyRequest).GetProperty("Id")?.SetValue(warrantyRequest, 1);
        typeof(Domain.Entities.Sales.WarrantyRequest).GetProperty("Status")?.SetValue(warrantyRequest, Domain.Enums.WarrantyRequestStatus.Completed);

        // Set up order
        var order = Order.Create(
            1,
            "Customer Name",
            "0901234567",
            "Street",
            "Ward",
            "District",
            "City");
        typeof(Order).GetProperty("Id")?.SetValue(order, 1);

        _bookingRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(booking);
        _orderRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        _warrantyRequestRepositoryMock.Setup(x => x.GetByOrderIdAsync(1)).ReturnsAsync(new List<Domain.Entities.Sales.WarrantyRequest> { warrantyRequest });
        _bookingRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var request = new CompleteInstallationRequest
        {
            CustomerSignature = "customer-signature-123",
            CustomerRating = 5,
            Notes = "Warranty completed"
        };
        await _installationService.CompleteAsync(1, request);

        // Assert
        booking.Status.Should().Be(InstallationStatus.Completed);
        warrantyRequest.Status.Should().Be(Domain.Enums.WarrantyRequestStatus.Completed);
        _warrantyRequestRepositoryMock.Verify(x => x.Update(It.IsAny<Domain.Entities.Sales.WarrantyRequest>()), Times.Never);
        _warrantyRequestRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}
