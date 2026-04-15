using Application.DTOs.Requests;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities.Promotions;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests;

public class CouponServiceTests
{
    private readonly Mock<ICouponRepository> _couponRepositoryMock;
    private readonly CouponService _couponService;

    public CouponServiceTests()
    {
        _couponRepositoryMock = new Mock<ICouponRepository>();
        _couponService = new CouponService(_couponRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Coupons()
    {
        // Arrange
        var coupons = new List<Coupon>
        {
            Coupon.Create("SAVE10", DiscountType.Percentage, Money.Vnd(10000), DateTime.UtcNow.AddDays(30)),
            Coupon.Create("SAVE20", DiscountType.FixedAmount, Money.Vnd(20000), DateTime.UtcNow.AddDays(30))
        };
        
        _couponRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(coupons);

        // Act
        var result = await _couponService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Code.Should().Be("SAVE10");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Coupon_When_Exists()
    {
        // Arrange
        var coupon = Coupon.Create("SAVE10", DiscountType.Percentage, Money.Vnd(10000), DateTime.UtcNow.AddDays(30));
        typeof(Coupon).GetProperty("Id")?.SetValue(coupon, 1);
        
        _couponRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(coupon);

        // Act
        var result = await _couponService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("SAVE10");
        result.DiscountValue.Should().Be(10000m);
    }

    [Fact]
    public async Task GetByCodeAsync_Should_Return_Coupon_When_Exists()
    {
        // Arrange
        var coupon = Coupon.Create("SAVE10", DiscountType.Percentage, Money.Vnd(10000), DateTime.UtcNow.AddDays(30));
        _couponRepositoryMock.Setup(x => x.GetByCodeAsync("SAVE10")).ReturnsAsync(coupon);

        // Act
        var result = await _couponService.GetByCodeAsync("SAVE10");

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("SAVE10");
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Coupon_And_Return_Id()
    {
        // Arrange
        var request = new CreateCouponRequest
        {
            Code = "NEWCODE",
            DiscountType = "FixedAmount",
            DiscountValue = 50000,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            UsageLimit = 100
        };

        _couponRepositoryMock.Setup(x => x.ExistsAsync("NEWCODE")).ReturnsAsync(false);
        _couponRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Coupon>())).Callback<Coupon>(c =>
        {
            typeof(Coupon).GetProperty("Id")?.SetValue(c, 1);
        });
        _couponRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _couponService.CreateAsync(request);

        // Assert
        result.Should().Be(1);
        _couponRepositoryMock.Verify(x => x.AddAsync(It.Is<Coupon>(c => 
            c.Code == "NEWCODE" && 
            c.DiscountValue.Amount == 50000m)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Code_Already_Exists()
    {
        // Arrange
        var request = new CreateCouponRequest
        {
            Code = "EXISTING",
            DiscountType = "Fixed",
            DiscountValue = 50000,
            ExpiryDate = DateTime.UtcNow.AddDays(30)
        };

        _couponRepositoryMock.Setup(x => x.ExistsAsync("EXISTING")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _couponService.CreateAsync(request));
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Coupon()
    {
        // Arrange
        var coupon = Coupon.Create("SAVE10", DiscountType.Percentage, Money.Vnd(10000), DateTime.UtcNow.AddDays(30));
        typeof(Coupon).GetProperty("Id")?.SetValue(coupon, 1);
        
        _couponRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(coupon);
        _couponRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _couponService.DeleteAsync(1);

        // Assert
        _couponRepositoryMock.Verify(x => x.Delete(coupon), Times.Once);
    }

}
