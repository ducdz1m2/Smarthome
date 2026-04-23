-- Thêm cột OrderId vào bảng WarrantyRequest
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio

USE SmarthomeDb;
GO

-- Kiểm tra xem cột đã tồn tại chưa
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('WarrantyRequest') AND name = 'OrderId'
)
BEGIN
    -- Thêm cột OrderId với giá trị mặc định là 0
    ALTER TABLE WarrantyRequest
    ADD OrderId int NOT NULL DEFAULT 0;

    PRINT 'Đã thêm cột OrderId vào bảng WarrantyRequest thành công';
END
ELSE
BEGIN
    PRINT 'Cột OrderId đã tồn tại trong bảng WarrantyRequest';
END
GO

-- Cập nhật OrderId cho các bản ghi hiện có dựa trên OrderItemId
UPDATE wr
SET wr.OrderId = oi.OrderId
FROM WarrantyRequest wr
INNER JOIN OrderItem oi ON wr.OrderItemId = oi.Id
WHERE wr.OrderId = 0;

PRINT 'Đã cập nhật OrderId cho các bản ghi hiện có';
GO
