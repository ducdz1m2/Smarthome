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
/// Tests for Technician Booking flow - kỹ thuật viên nhận lịch, báo cáo
/// </summary>
public class TechnicianBookingFlowTests
{
    [Fact]
    public void Technician_Nhan_Lich_Lap_Dat_Phai_Cap_Nhat_Status_Assigned()
    {
        // Arrange
        var booking = InstallationBooking.Create(
            orderId: 1,
            technicianId: 1,
            slotId: 1,
            scheduledDate: DateTime.UtcNow.AddDays(2));

        // Act - phân công kỹ thuật viên
        booking.AssignTechnician(technicianId: 5, slotId: 10);

        // Assert
        booking.TechnicianId.Should().Be(5);
        booking.SlotId.Should().Be(10);
        booking.Status.Should().Be(InstallationStatus.TechnicianAssigned);
    }

    [Fact]
    public void Technician_Bat_Dau_Chuẩn_Bị_Vat_Tu_Phai_Set_MaterialsPrepared()
    {
        // Arrange
        var booking = CreateTestBookingWithTechnician();
        booking.AddMaterial(1, 5);  // 5 cuộn dây
        booking.AddMaterial(2, 10); // 10 ổ cắm

        // Act
        booking.StartPreparation();

        // Assert
        booking.Status.Should().Be(InstallationStatus.Preparing);
        booking.MaterialsPrepared.Should().BeTrue();
    }

    [Fact]
    public void Technician_Bat_Dau_Di_Chuyen_Phai_Ghi_Nhan_Thoi_Gian()
    {
        // Arrange
        var booking = CreateTestBookingWithTechnician();
        booking.StartPreparation();

        // Act
        booking.StartTravel();

        // Assert
        booking.Status.Should().Be(InstallationStatus.OnTheWay);
        booking.OnTheWayAt.Should().NotBeNull();
    }

    [Fact]
    public void Technician_Bat_Dau_Lap_Dat_Phai_Ghi_Nhan_Thoi_Gian_Bat_Dau()
    {
        // Arrange
        var booking = CreateTestBookingWithFullFlowToTravel();

        // Act
        booking.StartInstallation();

        // Assert
        booking.Status.Should().Be(InstallationStatus.Installing);
        booking.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public void Technician_Hoan_Tat_Lap_Dat_Phai_Co_Chu_Ky_Khach_Hang()
    {
        // Arrange
        var booking = CreateTestBookingWithFullFlowToInstalling();

        // Act
        booking.Complete(
            customerSignature: "Nguyễn Văn A - Đã hoàn thành lắp đặt",
            customerRating: 5,
            notes: "Khách hàng hài lòng, thiết bị hoạt động tốt");

        // Assert
        booking.Status.Should().Be(InstallationStatus.Completed);
        booking.CompletedAt.Should().NotBeNull();
        booking.CustomerSignature.Should().Be("Nguyễn Văn A - Đã hoàn thành lắp đặt");
        booking.CustomerRating.Should().Be(5);
        booking.DomainEvents.Should().Contain(e => e is InstallationCompletedEvent);
    }

    [Fact]
    public void Technician_Khong_The_Di_Chuyen_Neu_Chua_Chuẩn_Bị_Xong()
    {
        // Arrange - chưa chuẩn bị
        var booking = CreateTestBookingWithTechnician();

        // Act & Assert
        Action act = () => booking.StartTravel();
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Technician_Khong_The_Lap_Neu_Chua_Den_Noi()
    {
        // Arrange - đã phân công, chuẩn bị xong nhưng chưa di chuyển
        var booking = CreateTestBookingWithTechnician();
        booking.StartPreparation();

        // Act & Assert
        Action act = () => booking.StartInstallation();
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Technician_Khong_The_Hoan_Tat_Neu_Chua_Lap_Xong()
    {
        // Arrange - đang di chuyển, chưa lắp
        var booking = CreateTestBookingWithFullFlowToTravel();

        // Act & Assert
        Action act = () => booking.Complete("test", 5);
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Technician_Bao_Cao_Danh_Gia_Thap_Khi_Khach_Khong_Hai_Long()
    {
        // Arrange
        var booking = CreateTestBookingWithFullFlowToInstalling();

        // Act - khách không hài lòng, đánh giá 2 sao
        booking.Complete(
            customerSignature: "Nguyễn Văn B",
            customerRating: 2,
            notes: "Thời gian lắp đặt lâu hơn dự kiến");

        // Assert
        booking.CustomerRating.Should().Be(2);
        booking.Status.Should().Be(InstallationStatus.Completed);
    }

    [Fact]
    public void Technician_Co_The_Them_Nhieu_Vat_Tu_Trong_Qua_Trinh_Chuẩn_Bị()
    {
        // Arrange
        var booking = CreateTestBookingWithTechnician();

        // Act
        booking.AddMaterial(1, 5);
        booking.AddMaterial(2, 3);
        booking.AddMaterial(3, 10);

        // Assert
        booking.Materials.Should().HaveCount(3);
    }

    [Fact]
    public void Slot_Lap_Dat_Co_The_Duoc_Tao_Va_Booked()
    {
        // Arrange
        var slot = InstallationSlot.Create(
            technicianId: 5, // Id > 0
            date: DateTime.UtcNow.AddDays(3).Date,
            startTime: new TimeSpan(9, 0, 0),
            endTime: new TimeSpan(11, 0, 0));

        // Act
        slot.Book(bookingId: 1);

        // Assert
        slot.IsBooked.Should().BeTrue();
        slot.BookingId.Should().Be(1);
    }

    [Fact]
    public void Order_Sau_Khi_Lap_Xong_Co_The_Hoan_Tat_Toan_Bo()
    {
        // Arrange
        var order = CreateTestOrderWithInstallProduct();
        order.Confirm();

        var installItem = order.Items.First(i => i.RequiresInstallation);
        var booking = InstallationBooking.Create(order.Id, 1, 1, DateTime.UtcNow.AddDays(1));
        
        // Flow: Assign → Prepare → Travel → Install → Complete
        booking.AssignTechnician(1, 1);
        booking.StartPreparation();
        booking.StartTravel();
        booking.StartInstallation();
        booking.Complete("Khách hàng ký nhận", 5);

        // Act - cập nhật order item
        order.MarkItemInstalled(installItem.Id);

        // Assert
        installItem.IsInstalled.Should().BeTrue();
    }

    // Helper methods
    private static InstallationBooking CreateTestBooking()
    {
        return InstallationBooking.Create(
            orderId: 1,
            technicianId: 1,
            slotId: 1,
            scheduledDate: DateTime.UtcNow.AddDays(2));
    }

    private static InstallationBooking CreateTestBookingWithTechnician()
    {
        var booking = CreateTestBooking();
        booking.AssignTechnician(1, 1);
        return booking;
    }

    private static InstallationBooking CreateTestBookingWithFullFlowToTravel()
    {
        var booking = CreateTestBookingWithTechnician();
        booking.StartPreparation();
        booking.StartTravel();
        return booking;
    }

    private static InstallationBooking CreateTestBookingWithFullFlowToInstalling()
    {
        var booking = CreateTestBookingWithFullFlowToTravel();
        booking.StartInstallation();
        return booking;
    }

    private static TechnicianProfile CreateTestTechnician()
    {
        return TechnicianProfile.Create(
            fullName: "Kỹ thuật viên A",
            phoneNumber: PhoneNumber.Create("0901234567"),
            employeeCode: "TECH-001",
            city: "TP.HCM",
            districts: new List<string> { "Q1", "Q2", "Q3" });
    }

    private static Order CreateTestOrderWithInstallProduct()
    {
        var order = Order.Create(1, "Test User", "0901234567",
            Address.Create("123 Test", "Ward 1", "District 1", "HCMC", "Vietnam", "70000"));

        var product = Product.Create("Smart Lock", "LOCK-001", Money.Vnd(2500000), 1, 1, requiresInstallation: true);
        order.AddItem(product.Id, null, 1, Money.Vnd(2500000), true);

        return order;
    }
}
