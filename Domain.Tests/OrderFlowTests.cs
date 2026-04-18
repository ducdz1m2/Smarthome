using Domain.Entities.Catalog;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests;

/// <summary>
/// Tests for Order flow - đặt hàng thường không cần lắp đặt
/// </summary>
public class OrderFlowTests
{
    [Fact]
    public void Order_Tao_Moi_Voi_Thong_Tin_Hop_Le_Phai_Thanh_Cong()
    {
        // Arrange
        var userId = 1;
        var receiverName = "Nguyen Van A";
        var receiverPhone = "0901234567";
        var address = Address.Create("123 Le Loi", "Phuong 1", "Quan 1", "TP.HCM", "Vietnam", "70000");

        // Act
        var order = Order.Create(userId, receiverName, receiverPhone, address);

        // Assert
        order.Should().NotBeNull();
        order.UserId.Should().Be(userId);
        order.ReceiverName.Should().Be(receiverName);
        order.Status.Should().Be(OrderStatus.Pending);
        order.TotalAmount.Should().Be(Money.Zero());
        order.Items.Should().BeEmpty();
        order.DomainEvents.Should().Contain(e => e is OrderCreatedEvent);
    }

    [Fact]
    public void Order_Them_San_Pham_Phai_Cap_Nhat_Tong_Tien()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct("LAP-001", 10000000);

        // Act
        order.AddItem(1, null, 2, Money.Vnd(10000000), false);

        // Assert
        order.Items.Should().HaveCount(1);
        order.TotalAmount.Amount.Should().Be(20000000);
    }

    [Fact]
    public void Order_Ap_Dung_Giam_Gia_Phai_Giam_Tong_Tien()
    {
        // Arrange
        var order = CreateTestOrder();
        order.AddItem(1, null, 5, Money.Vnd(200000), false); // 1,000,000

        // Act - apply 100k discount
        order.ApplyDiscount(Money.Vnd(100000));

        // Assert
        order.DiscountAmount.Amount.Should().Be(100000);
    }

    [Fact]
    public void Order_Xac_Nhan_Phai_Phat_Ra_OrderConfirmedEvent()
    {
        // Arrange
        var order = CreateTestOrder();
        order.AddItem(1, null, 1, Money.Vnd(100000), false);
        order.ClearDomainEvents(); // clear create event

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().BeOneOf(OrderStatus.AwaitingPickup, OrderStatus.AwaitingSchedule);
        order.DomainEvents.Should().Contain(e => e is OrderConfirmedEvent);
    }

    [Fact]
    public void Order_Item_Co_The_Danh_Dau_Da_Giao()
    {
        // Arrange
        var order = CreateTestOrder();
        order.AddItem(1, null, 1, Money.Vnd(100000), false);
        order.Confirm();

        // Act
        order.MarkItemShipped(order.Items.First().Id);

        // Assert
        order.Items.First().IsShipped.Should().BeTrue();
    }

    [Fact]
    public void Order_Hoan_Tat_Don_Hang_Phai_Cap_Nhat_Trang_Thai()
    {
        // Arrange
        var order = CreateTestOrder();
        var item = order.AddItem(1, null, 1, Money.Vnd(100000), false);
        order.Confirm();
        order.StartShipping();

        // Act - MarkItemShipped triggers UpdateOverallStatus which auto-completes order
        order.MarkItemShipped(item.Id);

        // Assert - order should be auto-completed when all items shipped
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Order_Thanh_Toan_Thanh_Cong_Phai_Ghi_Nhan_PaymentTransaction()
    {
        // Arrange
        var order = CreateTestOrder();
        order.AddItem(1, null, 1, Money.Vnd(1000000), false);

        // Act
        var payment = PaymentTransaction.Create(
            orderId: order.Id,
            amount: Money.Vnd(1000000),
            method: PaymentMethod.VNPay);

        // Assert
        payment.Should().NotBeNull();
        payment.Status.Should().Be(PaymentTransactionStatus.Pending);
        payment.Amount.Amount.Should().Be(1000000);
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

    private static Product CreateTestProduct(string sku, decimal price)
    {
        return Product.Create("Test Product", sku, 1, 1);
    }

    private static Product CreateTestProductWithStock(string sku, decimal price, int availableStock)
    {
        var product = Product.Create("Test Product", sku, 1, 1);
        product.AddVariant($"{sku}-VAR", Money.Vnd(price), new Dictionary<string, string>());
        product.AddStockToVariant($"{sku}-VAR", availableStock);
        return product;
    }
}
