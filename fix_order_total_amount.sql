-- Fix TotalAmount for existing orders
-- TotalAmount should be: SubTotal + ShippingFee - DiscountAmount

UPDATE Orders
SET TotalAmount = (
    (SELECT COALESCE(SUM(UnitPrice * Quantity), 0) FROM OrderItems WHERE OrderItems.OrderId = Orders.Id)
    + ShippingFee
    - DiscountAmount
)
WHERE TotalAmount <> (
    (SELECT COALESCE(SUM(UnitPrice * Quantity), 0) FROM OrderItems WHERE OrderItems.OrderId = Orders.Id)
    + ShippingFee
    - DiscountAmount
);
