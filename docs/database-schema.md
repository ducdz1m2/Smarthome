# Database Schema - Smarthome System

## Table Names and Relationships

### Identity Tables

| Table Name | Entity | Relationships |
|------------|--------|---------------|
| **AspNetUsers** | ApplicationUser | - One-to-many with Orders<br>- One-to-many with CartItems<br>- One-to-many with UserAddresses<br>- One-to-many with ProductComments<br>- One-to-many with ProductRatings<br>- One-to-many with TechnicianProfiles<br>- One-to-many with ChatMessages<br>- One-to-many with ChatParticipants<br>- One-to-many with Notifications |
| **AspNetRoles** | ApplicationRole | - Many-to-many with AspNetUsers (via AspNetUserRoles) |

### Catalog Tables

| Table Name | Entity | Relationships |
|------------|--------|---------------|
| **Products** | Product | - Many-to-one with Categories (CategoryId)<br>- Many-to-one with Brands (BrandId)<br>- Many-to-one with Suppliers (SupplierId, optional)<br>- One-to-many with ProductVariants<br>- One-to-many with ProductImages<br>- One-to-many with ProductComments<br>- One-to-many with OrderItems<br>- One-to-many with CartItems<br>- One-to-many with ProductWarehouses<br>- One-to-many with StockEntryDetails<br>- One-to-many with StockIssueDetails<br>- One-to-many with WarehouseTransferDetails<br>- One-to-many with ProductReservations<br>- One-to-many with Warranties<br>- One-to-many with WarrantyClaims<br>- One-to-many with ProductRatings<br>- One-to-many with PromotionProducts<br>- One-to-many with InstallationMaterials |
| **Categories** | Category | - Self-referencing: Many-to-one with Parent (ParentId)<br>- One-to-many with Children<br>- One-to-many with Products |
| **Brands** | Brand | - One-to-many with Products |
| **ProductVariants** | ProductVariant | - Many-to-one with Products (ProductId)<br>- One-to-many with OrderItems<br>- One-to-many with CartItems<br>- One-to-many with ProductWarehouses<br>- One-to-many with StockEntryDetails<br>- One-to-many with StockIssueDetails<br>- One-to-many with WarehouseTransferDetails<br>- One-to-many with ProductReservations<br>- One-to-many with Warranties<br>- One-to-many with WarrantyClaims<br>- One-to-many with InstallationMaterials |
| **ProductImages** | ProductImage | - Many-to-one with Products (ProductId) |
| **ProductComments** | ProductComment | - Many-to-one with Products (ProductId)<br>- Many-to-one with AspNetUsers (UserId) |

### Sales Tables

| Table Name | Entity | Relationships |
|------------|--------|---------------|
| **Orders** | Order | - Many-to-one with AspNetUsers (UserId)<br>- One-to-many with OrderItems (Cascade delete)<br>- One-to-many with OrderShipments (Cascade delete)<br>- One-to-one with PaymentTransactions (Cascade delete)<br>- One-to-many with InstallationBookings<br>- One-to-many with ReturnOrders<br>- One-to-many with Warranties |
| **OrderItems** | OrderItem | - Many-to-one with Orders (OrderId)<br>- Many-to-one with Products (ProductId)<br>- Many-to-one with ProductVariants (VariantId, optional)<br>- One-to-many with OrderWarehouseAllocations<br>- One-to-many with Warranties<br>- One-to-many with WarrantyClaims<br>- One-to-many with ReturnOrders<br>- One-to-one with InstallationBookings (InstallationBookingId) |
| **CartItems** | CartItem | - Many-to-one with AspNetUsers (UserId)<br>- Many-to-one with Products (ProductId)<br>- Many-to-one with ProductVariants (VariantId, optional) |
| **PaymentTransactions** | PaymentTransaction | - Many-to-one with Orders (OrderId, Cascade delete) |
| **OrderShipments** | OrderShipment | - Many-to-one with Orders (OrderId, Cascade delete) |
| **Warranties** | Warranty | - Many-to-one with Products (ProductId)<br>- Many-to-one with ProductVariants (VariantId, optional)<br>- Many-to-one with OrderItems (OrderItemId)<br>- Many-to-one with Orders<br>- One-to-many with WarrantyClaims (Cascade delete) |
| **WarrantyClaims** | WarrantyClaim | - Many-to-one with Warranties (WarrantyId, Cascade delete)<br>- Many-to-one with Products (ProductId)<br>- Many-to-one with ProductVariants (VariantId, optional)<br>- Many-to-one with OrderItems (OrderItemId) |
| **WarrantyRequests** | WarrantyRequest | - One-to-many with WarrantyRequestItems |
| **WarrantyRequestItems** | WarrantyRequestItem | - Many-to-one with WarrantyRequests |
| **ReturnOrders** | ReturnOrder | - Many-to-one with Orders (OrderId)<br>- Many-to-one with OrderItems (OrderItemId) |
| **ProductRatings** | ProductRating | - Many-to-one with Products (ProductId)<br>- Many-to-one with AspNetUsers (UserId) |
| **OrderWarehouseAllocations** | OrderWarehouseAllocation | - Many-to-one with OrderItems (OrderItemId)<br>- Many-to-one with Warehouses (WarehouseId) |

### Inventory Tables

| Table Name | Entity | Relationships |
|------------|--------|---------------|
| **Warehouses** | Warehouse | - One-to-many with ProductWarehouses<br>- One-to-many with StockEntries<br>- One-to-many with StockIssues<br>- One-to-many with WarehouseTransfers (as FromWarehouse)<br>- One-to-many with WarehouseTransfers (as ToWarehouse)<br>- One-to-many with OrderWarehouseAllocations<br>- One-to-many with InstallationMaterials |
| **Suppliers** | Supplier | - One-to-many with StockEntries<br>- One-to-many with Products (optional) |
| **ProductWarehouses** | ProductWarehouse | - Many-to-one with Warehouses (WarehouseId)<br>- Many-to-one with Products (ProductId)<br>- Many-to-one with ProductVariants (VariantId, optional) |
| **StockEntries** | StockEntry | - Many-to-one with Suppliers (SupplierId, Restrict)<br>- Many-to-one with Warehouses (WarehouseId, Restrict)<br>- One-to-many with StockEntryDetails |
| **StockEntryDetails** | StockEntryDetail | - Many-to-one with StockEntries (StockEntryId)<br>- Many-to-one with Products (ProductId)<br>- Many-to-one with ProductVariants (VariantId, optional) |
| **StockIssues** | StockIssue | - Many-to-one with Warehouses (WarehouseId)<br>- One-to-many with StockIssueDetails |
| **StockIssueDetails** | StockIssueDetail | - Many-to-one with StockIssues (StockIssueId)<br>- Many-to-one with Products (ProductId)<br>- Many-to-one with ProductVariants (VariantId, optional) |
| **WarehouseTransfers** | WarehouseTransfer | - Many-to-one with Warehouses (FromWarehouseId)<br>- Many-to-one with Warehouses (ToWarehouseId)<br>- One-to-many with WarehouseTransferDetails |
| **WarehouseTransferDetails** | WarehouseTransferDetail | - Many-to-one with WarehouseTransfers (WarehouseTransferId)<br>- Many-to-one with Products (ProductId)<br>- Many-to-one with ProductVariants (VariantId, optional) |
| **ProductReservations** | ProductReservation | - Many-to-one with Products (ProductId)<br>- Many-to-one with ProductVariants (VariantId, optional)<br>- Many-to-one with OrderItems (OrderItemId) |

### Installation Tables

| Table Name | Entity | Relationships |
|------------|--------|---------------|
| **InstallationBookings** | InstallationBooking | - Many-to-one with Orders (OrderId)<br>- Many-to-one with TechnicianProfiles (TechnicianId, Restrict)<br>- Many-to-one with InstallationSlots (SlotId, optional, Restrict)<br>- One-to-many with InstallationMaterials (Cascade delete)<br>- One-to-many with TechnicianRatings<br>- One-to-many with OrderItems (via InstallationBookingId)<br>- One-to-many with ChatRooms (RelatedInstallationId) |
| **TechnicianProfiles** | TechnicianProfile | - Many-to-one with AspNetUsers (UserId, unique, optional)<br>- One-to-many with InstallationSlots (Cascade delete)<br>- One-to-many with InstallationBookings<br>- One-to-many with TechnicianRatings |
| **InstallationSlots** | InstallationSlot | - Many-to-one with TechnicianProfiles (TechnicianId, Cascade delete)<br>- One-to-one with InstallationBookings (optional) |
| **InstallationMaterials** | InstallationMaterial | - Many-to-one with InstallationBookings (BookingId, Cascade delete)<br>- Many-to-one with Products (ProductId)<br>- Many-to-one with ProductVariants (VariantId, optional)<br>- Many-to-one with Warehouses (WarehouseId, optional) |
| **TechnicianRatings** | TechnicianRating | - Many-to-one with TechnicianProfiles (TechnicianId)<br>- Many-to-one with InstallationBookings (InstallationBookingId) |

### Promotion Tables

| Table Name | Entity | Relationships |
|------------|--------|---------------|
| **Promotions** | Promotion | - One-to-many with PromotionProducts (Cascade delete) |
| **PromotionProducts** | PromotionProduct | - Many-to-one with Promotions (PromotionId, Cascade delete)<br>- Many-to-one with Products (ProductId) |
| **Coupons** | Coupon | - No direct foreign key relationships |

### Shipping Tables

| Table Name | Entity | Relationships |
|------------|--------|---------------|
| **ShippingZones** | ShippingZone | - One-to-many with ShippingRates |
| **ShippingRates** | ShippingRate | - Many-to-one with ShippingZones (ShippingZoneId) |

### Content Tables

| Table Name | Entity | Relationships |
|------------|--------|---------------|
| **Banners** | Banner | - No direct foreign key relationships |
| **UserAddresses** | UserAddress | - Many-to-one with AspNetUsers (UserId) |

### Communication Tables

| Table Name | Entity | Relationships |
|------------|--------|---------------|
| **ChatRooms** | ChatRoom | - One-to-many with ChatMessages<br>- One-to-many with ChatParticipants<br>- Many-to-one with Orders (RelatedOrderId, optional)<br>- Many-to-one with InstallationBookings (RelatedInstallationId, optional)<br>- Many-to-one with WarrantyClaims (RelatedWarrantyClaimId, optional) |
| **ChatMessages** | ChatMessage | - Many-to-one with ChatRooms (ChatRoomId)<br>- Many-to-one with AspNetUsers (UserId)<br>- One-to-one with ChatAttachments |
| **ChatParticipants** | ChatParticipant | - Many-to-one with ChatRooms (ChatRoomId)<br>- Many-to-one with AspNetUsers (UserId) |
| **ChatAttachments** | ChatAttachment | - Many-to-one with ChatMessages (ChatMessageId) |
| **Notifications** | Notification | - Many-to-one with AspNetUsers (UserId)<br>- Many-to-one with various entities (RelatedEntityId, RelatedEntityType) |

## Delete Behaviors

- **Cascade**: When parent is deleted, all related children are automatically deleted
- **Restrict**: Parent cannot be deleted if it has related children
- **SetNull**: Foreign key is set to null when parent is deleted (rarely used)

## Indexes

### Unique Indexes
- Products.Sku
- Categories.ParentId (with self-reference)
- Orders.OrderNumber
- Warehouses.Code
- TechnicianProfiles.EmployeeCode
- TechnicianProfiles.UserId (with filter)
- CartItems.UserId, ProductId, VariantId (composite)
- Warranties.ProductId, VariantId, OrderItemId (composite)

### Common Indexes
- Products.CategoryId, BrandId, IsActive
- Orders.UserId, Status, CreatedAt
- OrderItems.OrderId, ProductId
- InstallationBookings.OrderId, TechnicianId, SlotId, ScheduledDate
- StockEntries.SupplierId, WarehouseId, EntryDate
- Warranties.ProductId, OrderItemId, EndDate

## Value Objects (Stored as Columns)

The following value objects are stored as columns in their parent tables:

### Address (Stored as separate columns)
- Street, Ward, District, City, Country, PostalCode
- Used in: Orders (ShippingAddress), Warehouses (Address), UserAddresses (Address), TechnicianProfiles (Address)

### Money (Stored as decimal)
- Amount
- Used in: ProductVariant.Price, Order.TotalAmount, Order.ShippingFee, Order.DiscountAmount, etc.

### PhoneNumber (Stored as string)
- Value
- Used in: Orders.ReceiverPhone, Warehouses.Phone, UserAddresses.ReceiverPhone, TechnicianProfiles.PhoneNumber

### Email (Stored as string)
- Value
- Used in: Supplier.Email, TechnicianProfile.Email

### Sku (Stored as string)
- Value
- Used in: Products.Sku, ProductVariant.Sku

### Percentage (Stored as decimal)
- Value
- Used in: Promotion.DiscountPercent, PromotionProduct.CustomDiscount
