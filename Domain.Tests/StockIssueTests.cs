using Domain.Entities.Inventory;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;

namespace Domain.Tests;

public class StockIssueTests
{
    [Fact]
    public void Create_ShouldReturnValidStockIssue()
    {
        // Arrange
        var warehouseId = 1;
        var issueType = StockIssueType.Installation;
        var bookingId = 100;
        var issuedBy = 5;
        var note = "Test note";

        // Act
        var stockIssue = StockIssue.Create(warehouseId, issueType, bookingId, issuedBy, note);

        // Assert
        stockIssue.Should().NotBeNull();
        stockIssue.WarehouseId.Should().Be(warehouseId);
        stockIssue.IssueType.Should().Be(issueType);
        stockIssue.BookingId.Should().Be(bookingId);
        stockIssue.IssuedBy.Should().Be(issuedBy);
        stockIssue.Note.Should().Be(note);
        stockIssue.IssueDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithInvalidWarehouseId_ShouldThrowException()
    {
        // Arrange
        var warehouseId = 0;

        // Act
        Action act = () => StockIssue.Create(warehouseId, StockIssueType.Installation);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*WarehouseId không hợp lệ*");
    }

    [Fact]
    public void Create_WithNegativeWarehouseId_ShouldThrowException()
    {
        // Arrange
        var warehouseId = -1;

        // Act
        Action act = () => StockIssue.Create(warehouseId, StockIssueType.Installation);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*WarehouseId không hợp lệ*");
    }

    [Fact]
    public void Create_WithNullNote_ShouldTrimNote()
    {
        // Arrange
        var warehouseId = 1;
        var note = "  Test note with spaces  ";

        // Act
        var stockIssue = StockIssue.Create(warehouseId, StockIssueType.Installation, note: note);

        // Assert
        stockIssue.Note.Should().Be("Test note with spaces");
    }

    [Fact]
    public void AddItem_ShouldCreateStockIssueDetail()
    {
        // Arrange
        var stockIssue = StockIssue.Create(1, StockIssueType.Installation);
        var productId = 10;
        var quantity = 5;
        var variantId = 2;

        // Act
        var detail = stockIssue.AddItem(productId, quantity, variantId);

        // Assert
        detail.Should().NotBeNull();
        detail.ProductId.Should().Be(productId);
        detail.Quantity.Should().Be(quantity);
        detail.VariantId.Should().Be(variantId);
    }

    [Fact]
    public void AddItem_WithoutVariantId_ShouldCreateStockIssueDetail()
    {
        // Arrange
        var stockIssue = StockIssue.Create(1, StockIssueType.Installation);
        var productId = 10;
        var quantity = 5;

        // Act
        var detail = stockIssue.AddItem(productId, quantity);

        // Assert
        detail.Should().NotBeNull();
        detail.ProductId.Should().Be(productId);
        detail.Quantity.Should().Be(quantity);
        detail.VariantId.Should().BeNull();
    }

    [Fact]
    public void AddItem_MultipleItems_ShouldCreateMultipleDetails()
    {
        // Arrange
        var stockIssue = StockIssue.Create(1, StockIssueType.Installation);

        // Act
        var detail1 = stockIssue.AddItem(10, 5);
        var detail2 = stockIssue.AddItem(20, 3);
        var detail3 = stockIssue.AddItem(30, 2);

        // Assert
        detail1.Should().NotBeNull();
        detail2.Should().NotBeNull();
        detail3.Should().NotBeNull();
    }

    [Fact]
    public void CompleteWithItems_ShouldTriggerEvents()
    {
        // Arrange
        var stockIssue = StockIssue.Create(1, StockIssueType.Installation, bookingId: 100);
        var details = new List<StockIssueDetail>
        {
            stockIssue.AddItem(10, 5),
            stockIssue.AddItem(20, 3)
        };

        // Act
        stockIssue.CompleteWithItems(details);

        // Assert
        stockIssue.DomainEvents.Should().HaveCount(2);
        stockIssue.DomainEvents.Should().AllSatisfy(e =>
        {
            e.GetType().Name.Should().Be("StockDispatchedEvent");
        });
    }

    [Fact]
    public void CompleteWithItems_ShouldIncludeBookingIdInEvent()
    {
        // Arrange
        var bookingId = 100;
        var stockIssue = StockIssue.Create(1, StockIssueType.Installation, bookingId: bookingId);
        var details = new List<StockIssueDetail>
        {
            stockIssue.AddItem(10, 5)
        };

        // Act
        stockIssue.CompleteWithItems(details);

        // Assert
        var @event = stockIssue.DomainEvents.First();
        @event.GetType().GetProperty("BookingId")?.GetValue(@event).Should().Be(bookingId);
    }
}
