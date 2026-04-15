using Domain.Entities.Catalog;
using Domain.Entities.Installation;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Events;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests;

/// <summary>
/// Tests for Mixed Order flow - đơn hàng có cả sản phẩm thường và cần lắp đặt
/// </summary>
public class MixedOrderFlowTests
{
    [Fact]
    public void Order_Co_Ca_San_Pham_Thuong_Va_Can_Lap_Dat_Phai_Phan_Loai_Dung()
    {
        // Arrange
        var order = CreateTestOrder();
        var normalProduct = Product.Create("Mouse", "MOU-001", Money.Vnd(200000), 1, 1, requiresInstallation: false);
        var installProduct = Product.Create("Smart Lock", "LOCK-001", Money.Vnd(2500000), 1, 1, requiresInstallation: true);

        // Act
        order.AddItem(normalProduct.Id, null, 2, Money.Vnd(200000), false);  // sản phẩm thường
        order.AddItem(installProduct.Id, null, 1, Money.Vnd(2500000), true);   // cần lắp đặt

        // Assert
        order.Items.Should().HaveCount(2);
        order.Items.Any(i => i.RequiresInstallation).Should().BeTrue();
        order.Items.Any(i => !i.RequiresInstallation).Should().BeTrue();
    }

    [Fact]
    public void Order_Co_Ca_2_Loai_Khi_Confirm_Phai_Vao_Trang_Thai_Confirmed()
    {
        // Arrange
        var order = CreateMixedOrder();

        // Act
        order.Confirm();

        // Assert - khi có cả 2 loại thì chờ xử lý cả 2 luồng
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Order_Co_Lap_Dat_Sau_Khi_Confirm_Co_The_Tao_InstallationBooking()
    {
        // Arrange
        var order = CreateMixedOrder();
        order.Confirm();

        // Act - tạo installation booking cho sản phẩm cần lắp đặt
        var installItem = order.Items.First(i => i.RequiresInstallation);
        
        // Lưu order trước để có ID
        var booking = InstallationBooking.Create(
            orderId: 1, // Giả định order đã lưu với Id = 1
            technicianId: 1,
            slotId: 1,
            scheduledDate: DateTime.UtcNow.AddDays(3));

        // Link booking to item
        installItem.AssignInstallation(booking.Id);

        // Assert
        booking.Should().NotBeNull();
        booking.OrderId.Should().Be(1);
        booking.Status.Should().Be(InstallationStatus.Assigned);
        installItem.InstallationBookingId.Should().Be(booking.Id);
    }

    [Fact]
    public void Order_Co_Nhieu_San_Pham_Can_Lap_Dat_Moi_Item_Co_Booking_Rieng()
    {
        // Arrange
        var order = CreateTestOrder();
        var installProduct1 = Product.Create("Camera", "CAM-001", Money.Vnd(3500000), 1, 1, requiresInstallation: true);
        var installProduct2 = Product.Create("Smart Lock", "LOCK-001", Money.Vnd(2500000), 1, 1, requiresInstallation: true);

        order.AddItem(installProduct1.Id, null, 1, Money.Vnd(3500000), true);
        order.AddItem(installProduct2.Id, null, 2, Money.Vnd(2500000), true);
        order.Confirm();

        // Act - tạo installation booking cho từng sản phẩm cần lắp đặt
        var installItems = order.Items.Where(i => i.RequiresInstallation).ToList();
        
        var booking1 = InstallationBooking.Create(
            orderId: 1,
            technicianId: 1,
            slotId: 1,
            scheduledDate: DateTime.UtcNow.AddDays(3));

        var booking2 = InstallationBooking.Create(
            orderId: 1,
            technicianId: 2,
            slotId: 2,
            scheduledDate: DateTime.UtcNow.AddDays(4));

        // Link bookings to items
        installItems[0].AssignInstallation(booking1.Id);
        installItems[1].AssignInstallation(booking2.Id);

        // Assert
        installItems.Should().HaveCount(2);
        installItems[0].InstallationBookingId.Should().Be(booking1.Id);
        installItems[1].InstallationBookingId.Should().Be(booking2.Id);
        booking1.Id.Should().NotBe(booking2.Id); // Each item has different booking
    }

    [Fact]
    public void Order_Mixed_Co_The_Bat_Dau_Lu_Song_Shipping()
    {
        // Arrange
        var order = CreateMixedOrder();
        order.Confirm();

        // Act
        order.StartShippingFlow();

        // Assert
        order.Status.Should().Be(OrderStatus.AwaitingPickup);
    }

    [Fact]
    public void Order_Mixed_Co_The_Bat_Dau_Lu_Song_Installation()
    {
        // Arrange
        var order = CreateMixedOrder();
        order.Confirm();

        // Act
        order.StartInstallationFlow();

        // Assert
        order.Status.Should().Be(OrderStatus.AwaitingSchedule);
    }

    [Fact]
    public void Order_Chi_Complete_Khi_Ca_2_Lu_Deu_Xong()
    {
        // Arrange
        var order = CreateMixedOrder();
        order.Confirm();
        order.StartShippingFlow();
        order.StartInstallationFlow();

        var normalItem = order.Items.First(i => !i.RequiresInstallation);
        var installItem = order.Items.First(i => i.RequiresInstallation);

        // Act 1 - Chỉ ship xong, chưa lắp
        normalItem.MarkAsShipped();
        order.MarkItemShipped(normalItem.Id);

        // Assert 1 - Chưa complete vì còn lắp đặt
        order.Status.Should().NotBe(OrderStatus.Completed);

        // Act 2 - Lắp xong
        installItem.MarkAsInstalled();
        order.MarkItemInstalled(installItem.Id);

        // Assert 2 - Giờ mới complete
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Order_Chi_Complete_Khi_Ca_2_Lu_Deu_Xong_Nguoc_Lai()
    {
        // Arrange
        var order = CreateMixedOrder();
        order.Confirm();
        order.StartShippingFlow();
        order.StartInstallationFlow();

        var normalItem = order.Items.First(i => !i.RequiresInstallation);
        var installItem = order.Items.First(i => i.RequiresInstallation);

        // Act 1 - Chỉ lắp xong, chưa ship
        installItem.MarkAsInstalled();
        order.MarkItemInstalled(installItem.Id);

        // Assert 1 - Chưa complete vì còn giao hàng
        order.Status.Should().NotBe(OrderStatus.Completed);

        // Act 2 - Ship xong
        normalItem.MarkAsShipped();
        order.MarkItemShipped(normalItem.Id);

        // Assert 2 - Giờ mới complete
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Order_Phan_Loai_San_Pham_Can_Giao_Hang_Va_Can_Lap()
    {
        // Arrange
        var order = CreateTestOrder();
        var normalProduct = Product.Create("USB Cable", "USB-001", Money.Vnd(150000), 1, 1, requiresInstallation: false);
        var installProduct = Product.Create("Camera", "CAM-001", Money.Vnd(3500000), 1, 1, requiresInstallation: true);

        order.AddItem(normalProduct.Id, null, 3, Money.Vnd(150000), false);
        order.AddItem(installProduct.Id, null, 2, Money.Vnd(3500000), true);
        order.Confirm();

        // Act - phân loại
        var shippingItems = order.Items.Where(i => !i.RequiresInstallation).ToList();
        var installItems = order.Items.Where(i => i.RequiresInstallation).ToList();

        // Assert
        shippingItems.Should().HaveCount(1);
        installItems.Should().HaveCount(1);
        shippingItems.First().Quantity.Should().Be(3);
        installItems.First().Quantity.Should().Be(2);
    }

    [Fact]
    public void Order_San_Pham_Can_Lap_Sau_Khi_Lap_Xong_Canh_Bao_Da_Giao()
    {
        // Arrange - đơn có cả 2 loại
        var order = CreateMixedOrder();
        order.Confirm();
        
        var installItem = order.Items.First(i => i.RequiresInstallation);
        
        // Act - tạo booking và hoàn thành lắp đặt
        var booking = InstallationBooking.Create(1, 1, 1, DateTime.UtcNow.AddDays(1)); // orderId = 1 (giả định đã lưu)
        booking.AssignTechnician(1, 1);
        booking.StartPreparation();
        booking.StartTravel();
        booking.StartInstallation();
        booking.Complete("Khách hàng ký nhận", 5, notes: "Hoàn thành tốt");

        // Assert - kiểm tra flow lắp đặt đã hoàn thành
        booking.Status.Should().Be(InstallationStatus.Completed);
        booking.CustomerRating.Should().Be(5);
    }

    [Fact]
    public void Order_Giao_Hang_Thuong_Complete_Sau_Do_Moi_Lap_Dat()
    {
        // Arrange
        var order = CreateMixedOrder();
        order.Confirm();
        
        var normalItem = order.Items.First(i => !i.RequiresInstallation);
        var installItem = order.Items.First(i => i.RequiresInstallation);

        // Verify items correctly identified
        normalItem.Should().NotBeNull();
        installItem.Should().NotBeNull();
        normalItem.RequiresInstallation.Should().BeFalse();
        installItem.RequiresInstallation.Should().BeTrue();

        // Act 1 - Giao hàng thường trước
        order.MarkItemShipped(normalItem.Id);

        // Assert 1
        normalItem.IsShipped.Should().BeTrue();
        order.Status.Should().NotBe(OrderStatus.Completed); // chưa hoàn thành vì còn lắp đặt

        // Act 2 - Hoàn thành lắp đặt
        order.MarkItemInstalled(installItem.Id);

        // Assert 2 - cả 2 đều xong thì có thể complete order
        installItem.IsInstalled.Should().BeTrue();
    }

    [Fact]
    public void Order_Tong_Tien_Co_Ca_2_Loai_San_Pham_Tinh_Dung()
    {
        // Arrange
        var order = CreateTestOrder();
        
        // Act
        order.AddItem(1, null, 2, Money.Vnd(200000), false);   // 400k
        order.AddItem(2, null, 1, Money.Vnd(2500000), true);  // 2.5M
        order.ApplyDiscount(Money.Vnd(100000));  // Giảm 100k

        // Assert - Total = 2.9M - 100k = 2.8M
        order.TotalAmount.Amount.Should().Be(2800000);
    }

    // Helper methods
    private static Order CreateTestOrder()
    {
        return Order.Create(
            userId: 1,
            receiverName: "Test User",
            receiverPhone: "0901234567",
            shippingAddress: Address.Create("123 Test", "Ward 1", "District 1", "HCMC", "Vietnam", "70000"));
    }

    private static Order CreateMixedOrder()
    {
        var order = CreateTestOrder();
        var normalProduct = Product.Create("Mouse", "MOU-001", Money.Vnd(200000), 1, 1, requiresInstallation: false);
        var installProduct = Product.Create("Smart Lock", "LOCK-001", Money.Vnd(2500000), 1, 1, requiresInstallation: true);

        var normalItem = order.AddItem(normalProduct.Id, null, 2, Money.Vnd(200000), false);
        var installItem = order.AddItem(installProduct.Id, null, 1, Money.Vnd(2500000), true);
        
        // Set unique Ids using reflection since EF Core hasn't assigned them yet
        var idProperty = typeof(OrderItem).GetProperty("Id");
        idProperty?.SetValue(normalItem, 1);
        idProperty?.SetValue(installItem, 2);
        
        return order;
    }
}
