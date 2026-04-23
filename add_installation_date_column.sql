-- Add missing InstallationDate column to Orders table
USE Smarthome;
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('Orders') AND name = 'InstallationDate'
)
BEGIN
    ALTER TABLE Orders
    ADD InstallationDate datetime2 NULL;
END
GO
