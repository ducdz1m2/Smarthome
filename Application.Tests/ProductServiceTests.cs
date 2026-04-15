using Application.DTOs.Requests;
using Application.Interfaces.Repositories;
using Application.Services;
using Domain.Entities.Catalog;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IProductWarehouseRepository> _productWarehouseRepositoryMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _productWarehouseRepositoryMock = new Mock<IProductWarehouseRepository>();
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _productService = new ProductService(_productRepositoryMock.Object, _productWarehouseRepositoryMock.Object, _warehouseRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Products()
    {
        // Arrange
        var products = new List<Product>
        {
            Product.Create("Product 1", "SKU-001", 100000m, 1, 1),
            Product.Create("Product 2", "SKU-002", 200000m, 1, 1)
        };
        _productRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Product 1");
        result.First().Sku.Should().Be("SKU-001");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Product_When_Exists()
    {
        // Arrange
        var product = Product.Create("Test Product", "SKU-001", 100000m, 1, 1);
        typeof(Product).GetProperty("Id")?.SetValue(product, 1);
        
        _productRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _productService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Product");
        result.Sku.Should().Be("SKU-001");
        result.BasePrice.Should().Be(100000m);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        _productRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySkuAsync_Should_Return_Product_When_Exists()
    {
        // Arrange
        var product = Product.Create("Test Product", "SKU-001", 100000m, 1, 1);
        _productRepositoryMock.Setup(x => x.GetBySkuAsync("SKU-001")).ReturnsAsync(product);

        // Act
        var result = await _productService.GetBySkuAsync("SKU-001");

        // Assert
        result.Should().NotBeNull();
        result!.Sku.Should().Be("SKU-001");
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Product_And_Return_Id()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            Sku = "SKU-NEW",
            BasePrice = 150000m,
            CategoryId = 1,
            BrandId = 1,
            StockQuantity = 10
        };

        _productRepositoryMock.Setup(x => x.ExistsAsync("SKU-NEW", null)).ReturnsAsync(false);
        _productRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>())).Callback<Product>(p => 
        {
            typeof(Product).GetProperty("Id")?.SetValue(p, 1);
        });
        _productRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _productService.CreateAsync(request);

        // Assert
        result.Should().Be(1);
        _productRepositoryMock.Verify(x => x.AddAsync(It.Is<Product>(p => 
            p.Name == "New Product" && 
            p.Sku.Value == "SKU-NEW" && 
            p.BasePrice.Amount == 150000m)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Sku_Already_Exists()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            Sku = "SKU-EXISTING",
            BasePrice = 150000m,
            CategoryId = 1,
            BrandId = 1
        };

        _productRepositoryMock.Setup(x => x.ExistsAsync("SKU-EXISTING", null)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _productService.CreateAsync(request));
    }

    [Fact]
    public async Task GetPagedAsync_Should_Return_Paged_Results()
    {
        // Arrange
        var products = Enumerable.Range(1, 10)
            .Select(i => Product.Create($"Product {i}", $"SKU-{i:D3}", i * 10000m, 1, 1))
            .ToList();
        
        _productRepositoryMock.Setup(x => x.GetPagedAsync(1, 5, null, null, null, null))
            .ReturnsAsync((products.Take(5).ToList(), 10));

        // Act
        var (items, totalCount) = await _productService.GetPagedAsync(1, 5);

        // Assert
        items.Should().HaveCount(5);
        totalCount.Should().Be(10);
    }

    [Fact]
    public async Task UpdateStockAsync_Should_Increase_Stock_When_Positive_Quantity()
    {
        // Arrange
        var product = Product.Create("Test Product", "SKU-001", 100000m, 1, 1);
        typeof(Product).GetProperty("Id")?.SetValue(product, 1);
        typeof(Product).GetProperty("StockQuantity")?.SetValue(product, 10);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
        _productRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _productService.UpdateStockAsync(1, 5);

        // Assert
        result.Should().BeTrue();
        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Product()
    {
        // Arrange
        var product = Product.Create("Test Product", "SKU-001", 100000m, 1, 1);
        typeof(Product).GetProperty("Id")?.SetValue(product, 1);
        product.Activate();
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
        _productRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _productService.DeleteAsync(1);

        // Assert
        _productRepositoryMock.Verify(x => x.Delete(product), Times.Once);
    }
}
