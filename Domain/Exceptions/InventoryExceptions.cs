namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when there is insufficient stock for an order.
/// </summary>
public class InsufficientStockException : DomainException
{
    public int ProductId { get; }
    public int WarehouseId { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException(int productId, int requested, int available)
        : base("InsufficientStock", $"Not enough stock for product {productId}. Requested: {requested}, Available: {available}")
    {
        ProductId = productId;
        RequestedQuantity = requested;
        AvailableQuantity = available;
        WarehouseId = 0;
    }

    public InsufficientStockException(int productId, int warehouseId, int requested, int available)
        : base("InsufficientStock", $"Not enough stock for product {productId} in warehouse {warehouseId}. Requested: {requested}, Available: {available}")
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        RequestedQuantity = requested;
        AvailableQuantity = available;
    }
}

/// <summary>
/// Exception thrown when a SKU is invalid or duplicated.
/// </summary>
public class InvalidSkuException : DomainException
{
    public string Sku { get; }

    public InvalidSkuException(string sku, string reason)
        : base("InvalidSku", $"SKU '{sku}' is invalid: {reason}")
    {
        Sku = sku;
    }
}

/// <summary>
/// Exception thrown when a duplicate SKU is detected.
/// </summary>
public class DuplicateSkuException : DomainException
{
    public string Sku { get; }

    public DuplicateSkuException(string sku)
        : base("DuplicateSku", $"SKU '{sku}' already exists.")
    {
        Sku = sku;
    }
}

/// <summary>
/// Exception thrown when warehouse capacity is exceeded.
/// </summary>
public class WarehouseCapacityExceededException : DomainException
{
    public int WarehouseId { get; }

    public WarehouseCapacityExceededException(int warehouseId)
        : base("WarehouseCapacityExceeded", $"Warehouse {warehouseId} has reached capacity.")
    {
        WarehouseId = warehouseId;
    }
}

/// <summary>
/// Exception thrown when a product is deactivated.
/// </summary>
public class ProductDeactivatedException : DomainException
{
    public int ProductId { get; }

    public ProductDeactivatedException(int productId)
        : base("ProductDeactivated", $"Product {productId} is deactivated and cannot be ordered.")
    {
        ProductId = productId;
    }
}

/// <summary>
/// Exception thrown when an invalid quantity is specified.
/// </summary>
public class InvalidQuantityException : DomainException
{
    public int Quantity { get; }
    public string Operation { get; }

    public InvalidQuantityException(int quantity, string operation)
        : base("InvalidQuantity", $"Quantity {quantity} is invalid for operation '{operation}'.")
    {
        Quantity = quantity;
        Operation = operation;
    }
}
