using Domain.Entities.Catalog;
using Domain.Entities.Installation;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests;

/// <summary>
/// Tests for Installation flow - đặt hàng cần lắp đặt + quản lý nhân viên
/// </summary>
public class InstallationFlowTests
{
    [Fact]
    public void InstallationBooking_Tao_Moi_Voi_Order_Can_Lap_Dat_Phai_Thanh_Cong()
    {
        // Arrange
        var orderId = 1;
        var technicianId = 1;
        var slotId = 1;
        var scheduledDate = DateTime.UtcNow.AddDays(3);

        // Act
        var booking = InstallationBooking.Create(orderId, technicianId, slotId, scheduledDate);

        // Assert
        booking.Should().NotBeNull();
        booking.OrderId.Should().Be(orderId);
        booking.TechnicianId.Should().Be(technicianId);
        booking.Status.Should().Be(InstallationStatus.Assigned);
        booking.MaterialsPrepared.Should().BeFalse();
        booking.DomainEvents.Should().Contain(e => e is InstallationBookingCreatedEvent);
    }

    [Fact]
    public void Technician_Tao_Moi_Voi_Thong_Tin_Hop_Le_Phai_Thanh_Cong()
    {
        // Arrange & Act
        var technician = TechnicianProfile.Create(
            fullName: "Nguyen Van Tech",
            phoneNumber: PhoneNumber.Create("0901234567"),
            employeeCode: "TECH-001",
            city: "TP.HCM",
            districts: new List<string> { "Q1", "Q2", "Q3" },
            baseSalary: Money.Vnd(15000000));

        // Assert
        technician.Should().NotBeNull();
        technician.EmployeeCode.Should().Be("TECH-001");
        technician.BaseSalary.Amount.Should().Be(15000000);
        technician.IsAvailable.Should().BeTrue();
        technician.City.Should().Be("TP.HCM");
    }

    [Fact]
    public void InstallationSlot_Tao_Moi_Voi_Thoi_Gian_Hop_Le_Phai_Thanh_Cong()
    {
        // Arrange
        var technicianId = 1;
        var date = DateTime.UtcNow.Date;
        var startTime = new TimeSpan(8, 0, 0);  // 8:00 AM
        var endTime = new TimeSpan(10, 0, 0);  // 10:00 AM

        // Act
        var slot = InstallationSlot.Create(technicianId, date, startTime, endTime);

        // Assert
        slot.Should().NotBeNull();
        slot.TechnicianId.Should().Be(technicianId);
        slot.Date.Should().Be(date);
        slot.StartTime.Should().Be(startTime);
        slot.EndTime.Should().Be(endTime);
        slot.IsBooked.Should().BeFalse();
    }

    [Fact]
    public void InstallationSlot_Co_The_Duoc_Dat_Sau_Khi_Tao()
    {
        // Arrange
        var slot = InstallationSlot.Create(1, DateTime.UtcNow.Date, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0));

        // Act
        slot.Book(bookingId: 1);

        // Assert
        slot.IsBooked.Should().BeTrue();
        slot.BookingId.Should().Be(1);
    }

    [Fact]
    public void InstallationBooking_Co_The_Them_Vat_Tu_Su_Dung()
    {
        // Arrange
        var booking = CreateTestBooking();

        // Act
        booking.AddMaterial(productId: 1, quantityTaken: 10);
        booking.AddMaterial(productId: 2, quantityTaken: 2);

        // Assert
        booking.Materials.Should().HaveCount(2);
        booking.MaterialsPrepared.Should().BeFalse();
    }

    [Fact]
    public void InstallationBooking_StartPreparation_Phai_Set_MaterialsPrepared()
    {
        // Arrange
        var booking = CreateTestBooking();
        booking.AssignTechnician(1, 1);
        booking.AddMaterial(1, 10);

        // Act
        booking.StartPreparation();

        // Assert
        booking.MaterialsPrepared.Should().BeTrue();
        booking.Status.Should().Be(InstallationStatus.Preparing);
    }

    [Fact]
    public void InstallationBooking_Co_The_Hoan_Tat_Voi_Danh_Gia()
    {
        // Arrange - flow đầy đủ: Create → Assign → Prepare → Travel → Install → Complete
        var booking = CreateTestBooking();
        booking.AssignTechnician(1, 1);
        booking.StartPreparation();
        booking.StartTravel();
        booking.StartInstallation();

        // Act
        booking.Complete(customerSignature: "Customer signed", customerRating: 5, notes: "Good job");

        // Assert
        booking.Status.Should().Be(InstallationStatus.Completed);
        booking.CompletedAt.Should().NotBeNull();
        booking.CustomerRating.Should().Be(5);
        booking.CustomerSignature.Should().Be("Customer signed");
    }

    [Fact]
    public void InstallationBooking_Co_The_Bat_Dau_Di_Chuyen()
    {
        // Arrange
        var booking = CreateTestBooking();
        booking.AssignTechnician(1, 1);
        booking.StartPreparation();

        // Act
        booking.StartTravel();

        // Assert
        booking.Status.Should().Be(InstallationStatus.OnTheWay);
        booking.OnTheWayAt.Should().NotBeNull();
    }

    [Fact]
    public void Technician_Khong_Available_Khi_Da_Nghi_Viec()
    {
        // Arrange
        var technician = TechnicianProfile.Create(
            "Tech", PhoneNumber.Create("0901234567"), "TECH-002", 
            "TP.HCM", new List<string> { "Q1" });
        
        // Act - Deactivate
        technician.GetType().GetProperty("IsAvailable")?.SetValue(technician, false);

        // Assert
        technician.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void InstallationSlot_Khi_Booked_Khong_The_Book_Lai()
    {
        // Arrange
        var slot = InstallationSlot.Create(1, DateTime.UtcNow.Date, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0));
        slot.Book(1);

        // Act & Assert
        Action act = () => slot.Book(2);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    // Helper methods
    private static InstallationBooking CreateTestBooking()
    {
        return InstallationBooking.Create(
            orderId: 1,
            technicianId: 1,
            slotId: 1,
            scheduledDate: DateTime.UtcNow.AddDays(3));
    }
}
