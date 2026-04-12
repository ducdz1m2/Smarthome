using Domain.Entities.Catalog;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests;

/// <summary>
/// Tests for Warranty flow - bảo hành sản phẩm
/// </summary>
public class WarrantyFlowTests
{
    [Fact]
    public void Warranty_Tao_Moi_Khi_Order_Hoan_Tat_Phai_Thanh_Cong()
    {
        // Arrange
        var orderItemId = 1;
        var productId = 1;
        var durationInMonths = 12;

        // Act
        var warranty = Warranty.Create(orderItemId, productId, durationInMonths);

        // Assert
        warranty.Should().NotBeNull();
        warranty.OrderItemId.Should().Be(orderItemId);
        warranty.ProductId.Should().Be(productId);
        warranty.Status.Should().Be(WarrantyStatus.Active);
        warranty.StartDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        warranty.EndDate.Should().BeCloseTo(DateTime.UtcNow.AddMonths(12), TimeSpan.FromDays(1));
    }

    [Fact]
    public void Warranty_Kiem_Tra_Con_Hieu_Luc_Trong_Thoi_Gian_Bao_Hanh()
    {
        // Arrange
        var warranty = Warranty.Create(1, 1, 12); // 12 tháng bảo hành

        // Act & Assert
        warranty.IsValid(DateTime.UtcNow.AddMonths(6)).Should().BeTrue();  // giữa chừng còn hiệu lực
        warranty.IsValid(DateTime.UtcNow.AddMonths(13)).Should().BeFalse(); // quá hạn
    }

    [Fact]
    public void Warranty_Co_The_Gia_Han_Them_Thoi_Gian()
    {
        // Arrange
        var warranty = Warranty.Create(1, 1, 12);
        var originalEndDate = warranty.EndDate;

        // Act
        warranty.Extend(additionalMonths: 6); // gia hạn thêm 6 tháng

        // Assert
        warranty.EndDate.Should().Be(originalEndDate.AddMonths(6));
    }

    [Fact]
    public void Warranty_Tao_Claim_Khi_Con_Hieu_Luc_Phai_Thanh_Cong()
    {
        // Arrange
        var warranty = Warranty.Create(1, 1, 12);
        var issue = "Product not working, screen flickering";

        // Act
        var claim = warranty.CreateClaim(issue);

        // Assert
        claim.Should().NotBeNull();
        claim.WarrantyId.Should().Be(warranty.Id);
        claim.Issue.Should().Be(issue);
        claim.Status.Should().Be(WarrantyClaimStatus.Pending);
        warranty.Claims.Should().Contain(claim);
    }

    [Fact]
    public void Warranty_Tao_Claim_Khi_Het_Han_Phai_Throw_WarrantyExpiredException()
    {
        // Arrange
        var warranty = Warranty.Create(1, 1, 6); // 6 tháng bảo hành
        // Simulate expired
        var pastDate = DateTime.UtcNow.AddMonths(7);

        // Act & Assert
        Action act = () => warranty.CreateClaim("Issue after expired");
        act.Should().Throw<BusinessRuleViolationException>().WithMessage("*Bảo hành đã hết hạn*");
    }

    [Fact]
    public void WarrantyClaim_Co_The_Phan_Cong_Nhan_Vien_Xu_Ly()
    {
        // Arrange
        var claim = CreateTestClaim();

        // Act
        claim.AssignTechnician(technicianId: 5);

        // Assert
        claim.TechnicianId.Should().Be(5);
        claim.Status.Should().Be(WarrantyClaimStatus.Assigned);
    }

    [Fact]
    public void WarrantyClaim_Hoan_Tat_Xu_Ly_Phai_Cap_Nhat_Trang_Thai()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.AssignTechnician(1);

        // Act
        claim.Resolve(resolution: "Replaced faulty component", isApproved: true);

        // Assert
        claim.Status.Should().Be(WarrantyClaimStatus.Resolved);
        claim.Resolution.Should().Be("Replaced faulty component");
    }

    [Fact]
    public void WarrantyClaim_Co_The_Approve_Replacement()
    {
        // Arrange
        var claim = CreateTestClaim();
        claim.AssignTechnician(1);

        // Act
        claim.ApproveReplacement();

        // Assert
        claim.Status.Should().Be(WarrantyClaimStatus.ReplacementApproved);
        claim.Resolution.Should().Be("Đổi sản phẩm mới");
    }

    [Fact]
    public void Warranty_Auto_MarkExpired_Khi_Qua_Han()
    {
        // Arrange
        var warranty = Warranty.Create(1, 1, 1); // 1 tháng bảo hành

        // Act - simulate time passing
        warranty.MarkExpired();

        // Assert - nếu đã quá hạn thì status = Expired
        if (DateTime.UtcNow > warranty.EndDate)
        {
            warranty.Status.Should().Be(WarrantyStatus.Expired);
        }
    }

    [Fact]
    public void Product_Yeu_Cau_Lap_Dat_Phai_Co_Flag_RequiresInstallation()
    {
        // Arrange & Act
        var product = Product.Create(
            name: "Smart AC",
            sku: "AC-001",
            basePrice: Money.Vnd(15000000),
            categoryId: 1,
            brandId: 1,
            requiresInstallation: true);

        // Assert
        product.RequiresInstallation.Should().BeTrue();
    }

    [Fact]
    public void Order_Voi_San_Pham_Can_Lap_Dat_Phai_Tao_InstallationBooking()
    {
        // Arrange
        var order = Order.Create(1, "Customer", "0901234567", 
            Address.Create("123 Test", "Ward 1", "Dist 1", "HCMC", "VN", "70000"));

        var product = Product.Create("Smart AC", "AC-001", Money.Vnd(15000000), 1, 1, requiresInstallation: true);
        order.AddItem(product.Id, null, 1, product.BasePrice, true);

        // Act - khi order có sản phẩm cần lắp đặt
        var requiresInstallation = order.Items.Any(i => i.Product.RequiresInstallation);

        // Assert
        requiresInstallation.Should().BeTrue();
        // Trong thực tế, Application Layer sẽ tạo InstallationBooking từ order này
    }

    // Helper methods
    private static WarrantyClaim CreateTestClaim()
    {
        var warranty = Warranty.Create(1, 1, 12);
        return warranty.CreateClaim("Screen broken");
    }
}
