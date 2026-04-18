using Domain.Entities.Inventory;
using Domain.Exceptions;
using FluentAssertions;

namespace Domain.Tests;

public class StockIssueDetailTests
{
    [Fact]
    public void Create_ShouldReturnValidStockIssueDetail()
    {
        // Arrange
        var productId = 10;
        var quantity = 5;
        var variantId = 2;

        // Act
        var detail = StockIssueDetail.Create(productId, quantity, variantId);

        // Assert
        detail.Should().NotBeNull();
        detail.ProductId.Should().Be(productId);
        detail.Quantity.Should().Be(quantity);
        detail.VariantId.Should().Be(variantId);
    }

    [Fact]
    public void Create_WithoutVariantId_ShouldReturnValidStockIssueDetail()
    {
        // Arrange
        var productId = 10;
        var quantity = 5;

        // Act
        var detail = StockIssueDetail.Create(productId, quantity);

        // Assert
        detail.Should().NotBeNull();
        detail.ProductId.Should().Be(productId);
        detail.Quantity.Should().Be(quantity);
        detail.VariantId.Should().BeNull();
    }

    [Fact]
    public void Create_WithInvalidProductId_ShouldThrowException()
    {
        // Arrange
        var productId = 0;

        // Act
        Action act = () => StockIssueDetail.Create(productId, 5);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*ProductId không hợp lệ*");
    }

    [Fact]
    public void Create_WithNegativeProductId_ShouldThrowException()
    {
        // Arrange
        var productId = -1;

        // Act
        Action act = () => StockIssueDetail.Create(productId, 5);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*ProductId không hợp lệ*");
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var quantity = 0;

        // Act
        Action act = () => StockIssueDetail.Create(10, quantity);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*Quantity không hợp lệ*");
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var quantity = -1;

        // Act
        Action act = () => StockIssueDetail.Create(10, quantity);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*Quantity không hợp lệ*");
    }

    [Fact]
    public void Create_WithValidQuantity_ShouldSucceed()
    {
        // Arrange
        var quantity = 1;

        // Act
        var detail = StockIssueDetail.Create(10, quantity);

        // Assert
        detail.Should().NotBeNull();
        detail.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void Create_WithLargeQuantity_ShouldSucceed()
    {
        // Arrange
        var quantity = 1000;

        // Act
        var detail = StockIssueDetail.Create(10, quantity);

        // Assert
        detail.Should().NotBeNull();
        detail.Quantity.Should().Be(quantity);
    }
}
