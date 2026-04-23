-- Add CouponCode column to Orders table
USE Smarthome;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'CouponCode')
BEGIN
    ALTER TABLE dbo.Orders
    ADD CouponCode NVARCHAR(MAX) NULL;
    PRINT 'CouponCode column added successfully.';
END
ELSE
BEGIN
    PRINT 'CouponCode column already exists.';
END
GO
