-- SQL Script để test fix cho vấn đề hiển thị categories không có sản phẩm
-- Script này kiểm tra logic lọc categories có sản phẩm

-- 1. Xem tất cả categories và số lượng sản phẩm của mỗi category
SELECT 
    c.Id,
    c.Name,
    c.IsActive,
    COUNT(p.Id) AS ProductCount,
    CASE 
        WHEN COUNT(p.Id) > 0 THEN 'Có sản phẩm'
        ELSE 'Không có sản phẩm'
    END AS HasProducts
FROM Categories c
LEFT JOIN Products p ON c.Id = p.CategoryId
GROUP BY c.Id, c.Name, c.IsActive
ORDER BY ProductCount DESC, c.SortOrder;

-- 2. Xem chỉ các categories có sản phẩm (đây là kết quả mà GetCategoriesWithProductsAsync() sẽ trả về)
SELECT 
    c.Id,
    c.Name,
    c.IsActive,
    COUNT(p.Id) AS ProductCount
FROM Categories c
INNER JOIN Products p ON c.Id = p.CategoryId
GROUP BY c.Id, c.Name, c.IsActive
HAVING COUNT(p.Id) > 0
ORDER BY c.SortOrder;

-- 3. Xem các categories KHÔNG có sản phẩm (những categories này sẽ KHÔNG được hiển thị sau fix)
SELECT 
    c.Id,
    c.Name,
    c.IsActive,
    COUNT(p.Id) AS ProductCount
FROM Categories c
LEFT JOIN Products p ON c.Id = p.CategoryId
GROUP BY c.Id, c.Name, c.IsActive
HAVING COUNT(p.Id) = 0 OR COUNT(p.Id) IS NULL
ORDER BY c.SortOrder;

-- 4. Kiểm tra số lượng categories có sản phẩm vs không có sản phẩm
SELECT 
    CASE 
        WHEN COUNT(p.Id) > 0 THEN 'Có sản phẩm'
        ELSE 'Không có sản phẩm'
    END AS CategoryStatus,
    COUNT(DISTINCT c.Id) AS CategoryCount
FROM Categories c
LEFT JOIN Products p ON c.Id = p.CategoryId
GROUP BY 
    CASE 
        WHEN COUNT(p.Id) > 0 THEN 'Có sản phẩm'
        ELSE 'Không có sản phẩm'
    END;

-- 5. Test scenario: Nếu muốn tạo một category không có sản phẩm để test
-- Uncomment dòng dưới để tạo test category
-- INSERT INTO Categories (Name, ParentId, SortOrder, Description, IsActive) 
-- VALUES ('Test Category No Products', NULL, 999, 'Category để test - không có sản phẩm', 1);

-- 6. Test scenario: Nếu muốn thêm sản phẩm vào category để test
-- Uncomment dòng dưới để thêm sản phẩm vào test category
-- DECLARE @CategoryId INT = (SELECT Id FROM Categories WHERE Name = 'Test Category No Products');
-- INSERT INTO Products (Sku, Name, Description, CategoryId, Price, IsActive) 
-- VALUES ('TEST-001', 'Test Product', 'Product để test', @CategoryId, 100000, 1);

-- 7. Cleanup test data sau khi test xong
-- Uncomment dòng dưới để xóa test data
-- DELETE FROM Products WHERE Sku = 'TEST-001';
-- DELETE FROM Categories WHERE Name = 'Test Category No Products';
