# Bảng Mô Tả Các Entities - Smarthome Project

## Catalog Module

### Product
**Mô tả:** Lưu trữ thông tin chi tiết về sản phẩm trong danh mục, bao gồm tên, giá cơ bản, tồn kho, thông số kỹ thuật, và liên kết với danh mục, thương hiệu, nhà cung cấp.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã sản phẩm (tự động tăng) |
| 2 | Name | string | - | - | Tên sản phẩm |
| 3 | Sku | Sku (ValueObject) | - | - | Mã SKU sản phẩm |
| 4 | BasePrice | Money (ValueObject) | - | - | Giá cơ bản |
| 5 | StockQuantity | int | - | - | Số lượng tồn kho tổng |
| 6 | FrozenStockQuantity | int | - | - | Số lượng đã đặt trước (frozen) |
| 7 | Description | string? | - | - | Mô tả sản phẩm |
| 8 | SpecsJson | string | - | - | Thông số kỹ thuật (JSON) |
| 9 | IsActive | bool | - | - | Trạng thái hoạt động |
| 10 | RequiresInstallation | bool | - | - | Cần lắp đặt |
| 11 | CategoryId | int | - | ✓ Category.Id | Mã danh mục |
| 12 | BrandId | int | - | ✓ Brand.Id | Mã thương hiệu |
| 13 | SupplierId | int? | - | ✓ Supplier.Id | Mã nhà cung cấp (nullable) |

### Category
**Mô tả:** Lưu trữ danh mục sản phẩm với cấu trúc phân cấp (hierarchy), cho phép danh mục cha-con và sắp xếp theo thứ tự.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã danh mục (tự động tăng) |
| 2 | Name | string | - | - | Tên danh mục |
| 3 | Description | string? | - | - | Mô tả danh mục |
| 4 | ParentId | int? | - | ✓ Category.Id | Mã danh mục cha (nullable) |
| 5 | SortOrder | int | - | - | Thứ tự sắp xếp |
| 6 | IsActive | bool | - | - | Trạng thái hoạt động |

### Brand
**Mô tả:** Lưu trữ thông tin về thương hiệu/hãng sản xuất sản phẩm, bao gồm tên, logo, website và mô tả.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã thương hiệu (tự động tăng) |
| 2 | Name | string | - | - | Tên thương hiệu |
| 3 | Description | string? | - | - | Mô tả thương hiệu |
| 4 | LogoUrl | string? | - | - | URL logo |
| 5 | Website | string? | - | - | Website thương hiệu |
| 6 | IsActive | bool | - | - | Trạng thái hoạt động |

### ProductVariant
**Mô tả:** Lưu trữ các biến thể của sản phẩm (size, màu sắc...) với giá riêng và tồn kho riêng cho từng biến thể.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã biến thể (tự động tăng) |
| 2 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 3 | Sku | Sku (ValueObject) | - | - | Mã SKU biến thể |
| 4 | Price | Money (ValueObject) | - | - | Giá biến thể |
| 5 | StockQuantity | int | - | - | Số lượng tồn kho |
| 6 | FrozenStockQuantity | int | - | - | Số lượng đã đặt trước |
| 7 | AttributesJson | string | - | - | Thuộc tính (size, color...) dạng JSON |
| 8 | IsActive | bool | - | - | Trạng thái hoạt động |

### ProductImage
**Mô tả:** Lưu trữ hình ảnh của sản phẩm với hỗ trợ hình ảnh chính (main) và thứ tự hiển thị.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã hình ảnh (tự động tăng) |
| 2 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 3 | Url | string | - | - | URL hình ảnh |
| 4 | AltText | string? | - | - | Text thay thế |
| 5 | IsMain | bool | - | - | Là hình ảnh chính |
| 6 | SortOrder | int | - | - | Thứ tự sắp xếp |

### ProductComment
**Mô tả:** Lưu trữ đánh giá/bình luận của khách hàng về sản phẩm với rating 1-5 sao, hỗ trợ trả lời lồng nhau và xác minh đã mua hàng.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã bình luận (tự động tăng) |
| 2 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 3 | UserId | int | - | ✓ ApplicationUser.Id | Mã người dùng |
| 4 | OrderId | int | - | ✓ Order.Id | Mã đơn hàng |
| 5 | Content | string | - | - | Nội dung bình luận |
| 6 | Rating | int | - | - | Đánh giá (1-5 sao) |
| 7 | IsApproved | bool | - | - | Đã duyệt |
| 8 | IsVerifiedPurchase | bool | - | - | Đã mua hàng |
| 9 | ParentCommentId | int? | - | ✓ ProductComment.Id | Mã bình luận cha (nullable) |

## Sales Module

### Order
**Mô tả:** Lưu trữ thông tin đơn hàng với hỗ trợ cả luồng giao hàng và lắp đặt, bao gồm thông tin người nhận, địa chỉ, phí vận chuyển, giảm giá và lịch sử trạng thái.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã đơn hàng (tự động tăng) |
| 2 | OrderNumber | string | - | - | Mã đơn hàng (format: ORDyyyyMMddXXXXXX) |
| 3 | Status | OrderStatus (Enum) | - | - | Trạng thái đơn hàng |
| 4 | UserId | int | - | ✓ ApplicationUser.Id | Mã người dùng |
| 5 | ReceiverName | string | - | - | Tên người nhận |
| 6 | ReceiverPhone | PhoneNumber (ValueObject) | - | - | Số điện thoại người nhận |
| 7 | ShippingAddress | Address (ValueObject) | - | - | Địa chỉ giao hàng |
| 8 | TotalAmount | Money (ValueObject) | - | - | Tổng tiền |
| 9 | ShippingFee | Money (ValueObject) | - | - | Phí vận chuyển |
| 10 | DiscountAmount | Money (ValueObject) | - | - | Số tiền giảm giá |
| 11 | PaymentMethod | PaymentMethod (Enum) | - | - | Phương thức thanh toán |
| 12 | ShippingMethod | ShippingMethod (Enum) | - | - | Phương thức vận chuyển |
| 13 | StatusHistoryJson | string | - | - | Lịch sử trạng thái (JSON) |
| 14 | CancelReason | string? | - | - | Lý do hủy |

### OrderItem
**Mô tả:** Lưu trữ từng mặt hàng trong đơn hàng với trạng thái giao/lắp đặt, phân bổ kho và liên kết lịch lắp đặt.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã item (tự động tăng) |
| 2 | OrderId | int | - | ✓ Order.Id | Mã đơn hàng |
| 3 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 4 | VariantId | int? | - | ✓ ProductVariant.Id | Mã biến thể (nullable) |
| 5 | Quantity | int | - | - | Số lượng |
| 6 | UnitPrice | Money (ValueObject) | - | - | Đơn giá |
| 7 | IsShipped | bool | - | - | Đã giao hàng |
| 8 | IsInstalled | bool | - | - | Đã lắp đặt |
| 9 | IsReserved | bool | - | - | Đã đặt trước |
| 10 | RequiresInstallation | bool | - | - | Cần lắp đặt |
| 11 | WarehouseId | int? | - | ✓ Warehouse.Id | Mã kho (nullable) |
| 12 | InstallationBookingId | int? | - | ✓ InstallationBooking.Id | Mã lịch lắp đặt (nullable) |

### CartItem
**Mô tả:** Lưu trữ sản phẩm trong giỏ hàng của người dùng với số lượng và thời gian thêm.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã item giỏ (tự động tăng) |
| 2 | UserId | int | - | ✓ ApplicationUser.Id | Mã người dùng |
| 3 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 4 | VariantId | int? | - | ✓ ProductVariant.Id | Mã biến thể (nullable) |
| 5 | Quantity | int | - | - | Số lượng |
| 6 | AddedAt | DateTime | - | - | Thời gian thêm |

### PaymentTransaction
**Mô tả:** Theo dõi giao dịch thanh toán của đơn hàng với trạng thái (Pending, Success, Failed, Refunded) và phản hồi từ gateway thanh toán.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã giao dịch (tự động tăng) |
| 2 | OrderId | int | - | ✓ Order.Id | Mã đơn hàng |
| 3 | Amount | Money (ValueObject) | - | - | Số tiền |
| 4 | Method | PaymentMethod (Enum) | - | - | Phương thức thanh toán |
| 5 | Status | PaymentTransactionStatus (Enum) | - | - | Trạng thái (Pending, Success, Failed, Refunded) |
| 6 | TransactionCode | string? | - | - | Mã giao dịch |
| 7 | GatewayResponse | string? | - | - | Phản hồi từ gateway |
| 8 | PaidAt | DateTime? | - | - | Thời gian thanh toán |

### ReturnOrder
**Mô tả:** Lưu trữ yêu cầu trả hàng với quy trình duyệt, nhận hàng, hoàn tiền và trạng thái (Pending, Approved, Received, Completed, Rejected).

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã trả hàng (tự động tăng) |
| 2 | OriginalOrderId | int | - | ✓ Order.Id | Mã đơn hàng gốc |
| 3 | ReturnType | ReturnType (Enum) | - | - | Loại trả hàng |
| 4 | Reason | string | - | - | Lý do trả |
| 5 | Status | ReturnOrderStatus (Enum) | - | - | Trạng thái (Pending, Approved, Received, Completed, Rejected) |
| 6 | RefundAmount | Money? | - | - | Số tiền hoàn lại |
| 7 | ApprovedAt | DateTime? | - | - | Thời gian duyệt |
| 8 | ReceivedAt | DateTime? | - | - | Thời gian nhận hàng |
| 9 | CompletedAt | DateTime? | - | - | Thời gian hoàn thành |

### ReturnOrderItem
**Mô tả:** Lưu trữ các mặt hàng trong yêu cầu trả hàng với số lượng và lý do trả.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã item trả (tự động tăng) |
| 2 | ReturnOrderId | int | - | ✓ ReturnOrder.Id | Mã trả hàng |
| 3 | OrderItemId | int | - | ✓ OrderItem.Id | Mã item đơn hàng |
| 4 | Quantity | int | - | - | Số lượng |
| 5 | Reason | string | - | - | Lý do trả |

### Warranty
**Mô tả:** Lưu trữ thông tin bảo hành sản phẩm với thời hạn (StartDate, EndDate) và trạng thái (Active, Expired, Void).

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã bảo hành (tự động tăng) |
| 2 | OrderItemId | int | - | ✓ OrderItem.Id | Mã item đơn hàng |
| 3 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 4 | StartDate | DateTime | - | - | Ngày bắt đầu |
| 5 | EndDate | DateTime | - | - | Ngày kết thúc |
| 6 | Status | WarrantyStatus (Enum) | - | - | Trạng thái (Active, Expired, Void) |

### WarrantyClaim
**Mô tả:** Lưu trữ khiếu nại bảo hành với mô tả vấn đề, giải pháp, trạng thái xử lý và phân công kỹ thuật viên.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã khiếu nại (tự động tăng) |
| 2 | WarrantyId | int | - | ✓ Warranty.Id | Mã bảo hành |
| 3 | ClaimDate | DateTime | - | - | Ngày khiếu nại |
| 4 | Issue | string | - | - | Mô tả vấn đề |
| 5 | Resolution | string? | - | - | Giải pháp |
| 6 | Status | WarrantyClaimStatus (Enum) | - | - | Trạng thái (Pending, Assigned, InProgress, Resolved, Rejected, ReplacementApproved) |
| 7 | TechnicianId | int? | - | ✓ TechnicianProfile.Id | Mã kỹ thuật viên (nullable) |

### OrderShipment
**Mô tả:** Theo dõi quá trình vận chuyển đơn hàng với tracking number, đơn vị vận chuyển, trạng thái (PendingApproval, Approved, Delivered...) và thời gian.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã vận chuyển (tự động tăng) |
| 2 | OrderId | int | - | ✓ Order.Id | Mã đơn hàng |
| 3 | ShipperId | int? | - | - | Mã shipper (nullable) |
| 4 | Carrier | string | - | - | Đơn vị vận chuyển (GHN, GHTK, J&T...) |
| 5 | TrackingNumber | string | - | - | Mã tracking |
| 6 | Status | OrderShipmentStatus (Enum) | - | - | Trạng thái (PendingApproval, Approved, Rejected, Assigned, Accepted, PickedUp, InTransit, Delivered, Failed) |
| 7 | PickedUpAt | DateTime? | - | - | Thời gian lấy hàng |
| 8 | DeliveredAt | DateTime? | - | - | Thời gian giao hàng |
| 9 | Notes | string? | - | - | Ghi chú |
| 10 | ApprovedAt | DateTime? | - | - | Thời gian duyệt |
| 11 | ApprovedBy | int? | - | - | Người duyệt (nullable) |

### OrderWarehouseAllocation
**Mô tả:** Lưu trữ phân bổ kho cho từng item đơn hàng với số lượng phân bổ, trạng thái xác nhận và thời gian.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã phân bổ (tự động tăng) |
| 2 | OrderItemId | int | - | ✓ OrderItem.Id | Mã item đơn hàng |
| 3 | WarehouseId | int | - | ✓ Warehouse.Id | Mã kho |
| 4 | AllocatedQuantity | int | - | - | Số lượng phân bổ |
| 5 | IsConfirmed | bool | - | - | Đã xác nhận |
| 6 | ConfirmedAt | DateTime? | - | - | Thời gian xác nhận |

## Inventory Module

### Warehouse
**Mô tả:** Lưu trữ thông tin kho hàng với địa chỉ, số điện thoại, người quản lý và trạng thái hoạt động.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã kho (tự động tăng) |
| 2 | Name | string | - | - | Tên kho |
| 3 | Code | string | - | - | Mã kho |
| 4 | Address | Address (ValueObject) | - | - | Địa chỉ kho |
| 5 | Phone | PhoneNumber? | - | - | Số điện thoại |
| 6 | ManagerName | string? | - | - | Tên người quản lý |
| 7 | IsActive | bool | - | - | Trạng thái hoạt động |

### ProductWarehouse
**Mô tả:** Theo dõi tồn kho của sản phẩm theo từng kho với số lượng thực tế và số lượng đã đặt trước (reserved).

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã tồn kho (tự động tăng) |
| 2 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 3 | VariantId | int? | - | ✓ ProductVariant.Id | Mã biến thể (nullable) |
| 4 | WarehouseId | int | - | ✓ Warehouse.Id | Mã kho |
| 5 | Quantity | int | - | - | Số lượng |
| 6 | ReservedQuantity | int | - | - | Số lượng đã đặt trước |

### StockEntry
**Mô tả:** Lưu trữ phiếu nhập hàng từ nhà cung cấp với ngày nhập, tổng chi phí và trạng thái hoàn thành.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã phiếu nhập (tự động tăng) |
| 2 | EntryDate | DateTime | - | - | Ngày nhập |
| 3 | SupplierId | int | - | ✓ Supplier.Id | Mã nhà cung cấp |
| 4 | WarehouseId | int | - | ✓ Warehouse.Id | Mã kho |
| 5 | Note | string? | - | - | Ghi chú |
| 6 | TotalCost | decimal | - | - | Tổng chi phí |
| 7 | IsCompleted | bool | - | - | Đã hoàn thành |

### StockEntryDetail
**Mô tả:** Lưu trữ chi tiết từng sản phẩm trong phiếu nhập với số lượng và đơn giá nhập.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã chi tiết (tự động tăng) |
| 2 | StockEntryId | int | - | ✓ StockEntry.Id | Mã phiếu nhập |
| 3 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 4 | VariantId | int? | - | ✓ ProductVariant.Id | Mã biến thể (nullable) |
| 5 | Quantity | int | - | - | Số lượng |
| 6 | UnitCost | Money (ValueObject) | - | - | Đơn giá nhập |

### Supplier
**Mô tả:** Lưu trữ thông tin nhà cung cấp với mã số thuế, địa chỉ, thông tin liên hệ, tài khoản ngân hàng và trạng thái hoạt động.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã nhà cung cấp (tự động tăng) |
| 2 | Name | string | - | - | Tên nhà cung cấp |
| 3 | TaxCode | string? | - | - | Mã số thuế |
| 4 | Address | Address? | - | - | Địa chỉ |
| 5 | ContactName | string? | - | - | Tên liên hệ |
| 6 | Phone | PhoneNumber? | - | - | Số điện thoại |
| 7 | Email | Email? | - | - | Email |
| 8 | BankAccount | string? | - | - | Số tài khoản ngân hàng |
| 9 | BankName | string? | - | - | Tên ngân hàng |
| 10 | IsActive | bool | - | - | Trạng thái hoạt động |

## Identity Module

### ApplicationUser
**Mô tả:** Lưu trữ thông tin người dùng hệ thống kế thừa từ IdentityUser, bao gồm họ tên, avatar, trạng thái hoạt động và lịch sử đăng nhập.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã người dùng (tự động tăng) |
| 2 | UserName | string | - | - | Tên đăng nhập (kế thừa từ IdentityUser) |
| 3 | Email | string | - | - | Email (kế thừa từ IdentityUser) |
| 4 | FullName | string | - | - | Họ tên đầy đủ |
| 5 | Avatar | string? | - | - | URL avatar |
| 6 | IsActive | bool | - | - | Trạng thái hoạt động |
| 7 | LastLoginAt | DateTime? | - | - | Lần đăng nhập cuối |
| 8 | CreatedAt | DateTime | - | - | Thời gian tạo |

### ApplicationRole
**Mô tả:** Lưu trữ vai trò người dùng kế thừa từ IdentityRole với mô tả và thời gian tạo.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã role (tự động tăng) |
| 2 | Name | string | - | - | Tên role (kế thừa từ IdentityRole) |
| 3 | Description | string? | - | - | Mô tả role |
| 4 | CreatedAt | DateTime | - | - | Thời gian tạo |

## Installation Module

### InstallationBooking
**Mô tả:** Lưu trữ lịch hẹn lắp đặt với kỹ thuật viên, khung giờ, trạng thái (Assigned, Preparing, OnTheWay, Installing, Completed), đánh giá khách hàng và hỗ trợ tháo lắp/bảo hành.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã lịch lắp đặt (tự động tăng) |
| 2 | OrderId | int | - | ✓ Order.Id | Mã đơn hàng |
| 3 | TechnicianId | int | - | ✓ TechnicianProfile.Id | Mã kỹ thuật viên |
| 4 | SlotId | int | - | ✓ InstallationSlot.Id | Mã khung giờ |
| 5 | Status | InstallationStatus (Enum) | - | - | Trạng thái (Pending, Assigned, Preparing, OnTheWay, Installing, Completed, Cancelled, Rescheduled) |
| 6 | ScheduledDate | DateTime | - | - | Ngày dự kiến |
| 7 | EstimatedDuration | TimeSpan | - | - | Thời gian dự kiến |
| 8 | MaterialsPrepared | bool | - | - | Đã chuẩn bị vật tư |
| 9 | OnTheWayAt | DateTime? | - | - | Thời gian bắt đầu di chuyển |
| 10 | StartedAt | DateTime? | - | - | Thời gian bắt đầu lắp |
| 11 | CompletedAt | DateTime? | - | - | Thời gian hoàn thành |
| 12 | CustomerRating | int? | - | - | Đánh giá khách hàng |
| 13 | CustomerSignature | string? | - | - | Chữ ký khách hàng |
| 14 | Notes | string? | - | - | Ghi chú |
| 15 | IsUninstall | bool | - | - | Là tháo lắp |
| 16 | IsWarranty | bool | - | - | Là bảo hành |

### InstallationSlot
**Mô tả:** Lưu trữ khung giờ làm việc của kỹ thuật viên với ngày, thời gian bắt đầu-kết thúc và trạng thái đặt.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã khung giờ (tự động tăng) |
| 2 | TechnicianId | int | - | ✓ TechnicianProfile.Id | Mã kỹ thuật viên |
| 3 | Date | DateTime | - | - | Ngày |
| 4 | StartTime | TimeSpan | - | - | Thời gian bắt đầu |
| 5 | EndTime | TimeSpan | - | - | Thời gian kết thúc |
| 6 | IsBooked | bool | - | - | Đã đặt |
| 7 | BookingId | int? | - | ✓ InstallationBooking.Id | Mã lịch lắp đặt (nullable) |

### TechnicianProfile
**Mô tả:** Lưu trữ hồ sơ kỹ thuật viên với thông tin cá nhân, mã nhân viên, lương, khu vực phục vụ, kỹ năng, đánh giá và thống kê công việc.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã kỹ thuật viên (tự động tăng) |
| 2 | UserId | int? | - | ✓ ApplicationUser.Id | Mã user (nullable) |
| 3 | FullName | string | - | - | Họ tên |
| 4 | PhoneNumber | PhoneNumber (ValueObject) | - | - | Số điện thoại |
| 5 | Email | Email? | - | - | Email |
| 6 | IdentityCard | string? | - | - | CCCD |
| 7 | Address | Address? | - | - | Địa chỉ |
| 8 | DateOfBirth | DateTime? | - | - | Ngày sinh |
| 9 | EmployeeCode | string | - | - | Mã nhân viên |
| 10 | HireDate | DateTime | - | - | Ngày tuyển dụng |
| 11 | BaseSalary | Money (ValueObject) | - | - | Lương cơ bản |
| 12 | City | string | - | - | Thành phố phục vụ |
| 13 | Districts | string | - | - | Các quận (JSON array) |
| 14 | SkillsJson | string | - | - | Kỹ năng (JSON array) |
| 15 | IsAvailable | bool | - | - | Có sẵn sàng |
| 16 | Rating | double | - | - | Đánh giá trung bình |
| 17 | CompletedJobs | int | - | - | Số công việc hoàn thành |
| 18 | CancelledJobs | int | - | - | Số công việc hủy |

### InstallationMaterial
**Mô tả:** Lưu trữ vật tư sử dụng trong quá trình lắp đặt với sản phẩm và số lượng lấy.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã vật tư (tự động tăng) |
| 2 | InstallationBookingId | int | - | ✓ InstallationBooking.Id | Mã lịch lắp đặt |
| 3 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 4 | QuantityTaken | int | - | - | Số lượng lấy |

## Communication Module

### ChatRoom
**Mô tả:** Lưu trữ phòng chat với loại (OneToOne, Support), liên kết với đơn hàng/lắp đặt/bảo hành, trạng thái hoạt động và thời gian đóng.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã phòng chat (tự động tăng) |
| 2 | Title | string | - | - | Tiêu đề phòng |
| 3 | Type | ChatRoomType (Enum) | - | - | Loại phòng (OneToOne, Support) |
| 4 | RelatedOrderId | int? | - | ✓ Order.Id | Mã đơn hàng liên quan (nullable) |
| 5 | RelatedInstallationId | int? | - | ✓ InstallationBooking.Id | Mã lắp đặt liên quan (nullable) |
| 6 | RelatedWarrantyClaimId | int? | - | ✓ WarrantyClaim.Id | Mã khiếu nại liên quan (nullable) |
| 7 | CreatedAt | DateTime | - | - | Thời gian tạo |
| 8 | ClosedAt | DateTime? | - | - | Thời gian đóng |
| 9 | IsActive | bool | - | - | Đang hoạt động |

### ChatMessage
**Mô tả:** Lưu trữ tin nhắn trong phòng chat với hỗ trợ sửa/xóa, loại người gửi (Customer, Admin, Technician) và thời gian.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã tin nhắn (tự động tăng) |
| 2 | ChatRoomId | int | - | ✓ ChatRoom.Id | Mã phòng chat |
| 3 | SenderId | int | - | ✓ ApplicationUser.Id | Mã người gửi |
| 4 | SenderType | UserType (Enum) | - | - | Loại người gửi (Customer, Admin, Technician) |
| 5 | Content | string | - | - | Nội dung |
| 6 | SentAt | DateTime | - | - | Thời gian gửi |
| 7 | EditedAt | DateTime? | - | - | Thời gian sửa |
| 8 | IsDeleted | bool | - | - | Đã xóa |
| 9 | DeletedAt | DateTime? | - | - | Thời gian xóa |
| 10 | DeletedBy | string? | - | - | Người xóa |

### ChatParticipant
**Mô tả:** Lưu trữ người tham gia phòng chat với loại người dùng, trạng thái chặn, hoạt động lần cuối và số tin chưa đọc.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã tham gia (tự động tăng) |
| 2 | ChatRoomId | int | - | ✓ ChatRoom.Id | Mã phòng chat |
| 3 | UserId | int | - | ✓ ApplicationUser.Id | Mã người dùng |
| 4 | UserType | UserType (Enum) | - | - | Loại người dùng |
| 5 | IsBlocked | bool | - | - | Đã chặn |
| 6 | BlockedReason | string? | - | - | Lý do chặn |
| 7 | LastActivityAt | DateTime? | - | - | Hoạt động lần cuối |
| 8 | UnreadCount | int | - | - | Số tin chưa đọc |

### ChatAttachment
**Mô tả:** Lưu trữ file đính kèm của tin nhắn với tên file, URL, loại file, kích thước và thời gian upload.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã đính kèm (tự động tăng) |
| 2 | ChatMessageId | int | - | ✓ ChatMessage.Id | Mã tin nhắn |
| 3 | FileName | string | - | - | Tên file |
| 4 | FileUrl | string | - | - | URL file |
| 5 | FileType | string? | - | - | Loại file |
| 6 | FileSize | long? | - | - | Kích thước file |
| 7 | UploadedAt | DateTime | - | - | Thời gian upload |

### Notification
**Mô tả:** Lưu trữ thông báo hệ thống với loại thông báo, tiêu đề, nội dung, URL hành động, trạng thái đọc/gửi và liên kết với entity liên quan.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã thông báo (tự động tăng) |
| 2 | UserId | int | - | ✓ ApplicationUser.Id | Mã người dùng |
| 3 | UserType | UserType (Enum) | - | - | Loại người dùng |
| 4 | Type | NotificationType (Enum) | - | - | Loại thông báo |
| 5 | Title | string | - | - | Tiêu đề |
| 6 | Message | string | - | - | Nội dung |
| 7 | ActionUrl | string? | - | - | URL hành động |
| 8 | Icon | string? | - | - | Icon |
| 9 | IsRead | bool | - | - | Đã đọc |
| 10 | ReadAt | DateTime? | - | - | Thời gian đọc |
| 11 | IsSent | bool | - | - | Đã gửi |
| 12 | SentAt | DateTime? | - | - | Thời gian gửi |
| 13 | CreatedAt | DateTime | - | - | Thời gian tạo |
| 14 | SendError | string? | - | - | Lỗi gửi |
| 15 | RelatedEntityId | int? | - | - | Mã entity liên quan (nullable) |
| 16 | RelatedEntityType | string? | - | - | Loại entity liên quan (nullable) |

## Content Module

### Banner
**Mô tả:** Lưu trữ banner quảng cáo với hình ảnh, link, vị trí hiển thị (HomeTop, HomeMiddle...), thời gian hoạt động và số lần click.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã banner (tự động tăng) |
| 2 | Title | string | - | - | Tiêu đề |
| 3 | Subtitle | string? | - | - | Phụ đề |
| 4 | ImageUrl | WebsiteUrl (ValueObject) | - | - | URL hình ảnh |
| 5 | LinkUrl | WebsiteUrl? | - | - | URL link |
| 6 | Position | string | - | - | Vị trí (HomeTop, HomeMiddle, ProductPage...) |
| 7 | SortOrder | int | - | - | Thứ tự sắp xếp |
| 8 | StartDate | DateTime? | - | - | Ngày bắt đầu (nullable) |
| 9 | EndDate | DateTime? | - | - | Ngày kết thúc (nullable) |
| 10 | IsActive | bool | - | - | Đang hoạt động |
| 11 | ClickCount | int | - | - | Số lần click |

### UserAddress
**Mô tả:** Lưu trữ địa chỉ đã lưu của người dùng với nhãn (Nhà, Công ty...), thông tin người nhận và địa chỉ mặc định.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã địa chỉ (tự động tăng) |
| 2 | UserId | int | - | ✓ ApplicationUser.Id | Mã người dùng |
| 3 | Label | string | - | - | Nhãn địa chỉ (Nhà, Công ty...) |
| 4 | ReceiverName | string | - | - | Tên người nhận |
| 5 | ReceiverPhone | PhoneNumber (ValueObject) | - | - | Số điện thoại người nhận |
| 6 | Address | Address (ValueObject) | - | - | Địa chỉ |
| 7 | IsDefault | bool | - | - | Là địa chỉ mặc định |

## Promotions Module

### Coupon
**Mô tả:** Lưu trữ mã giảm giá với loại giảm giá (cố định hoặc phần trăm), giá trị giảm, điều kiện tối thiểu, giới hạn sử dụng và ngày hết hạn.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã coupon (tự động tăng) |
| 2 | Code | string | - | - | Mã coupon |
| 3 | DiscountType | DiscountType (Enum) | - | - | Loại giảm giá (FixedAmount, Percentage) |
| 4 | DiscountValue | Money (ValueObject) | - | - | Giá trị giảm giá |
| 5 | MinOrderAmount | Money? | - | - | Giá trị đơn hàng tối thiểu (nullable) |
| 6 | MaxDiscountAmount | Money? | - | - | Giảm giá tối đa (nullable) |
| 7 | ExpiryDate | DateTime | - | - | Ngày hết hạn |
| 8 | MaxUsage | int | - | - | Số lần sử dụng tối đa |
| 9 | UsedCount | int | - | - | Số lần đã dùng |
| 10 | IsActive | bool | - | - | Đang hoạt động |

### Promotion
**Mô tả:** Lưu trữ chương trình khuyến mãi với phần trăm giảm giá, thời gian hoạt động, điều kiện tối thiểu và độ ưu tiên áp dụng.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã khuyến mãi (tự động tăng) |
| 2 | Name | string | - | - | Tên chương trình |
| 3 | Description | string? | - | - | Mô tả |
| 4 | DiscountPercent | Percentage (ValueObject) | - | - | Phần trăm giảm giá |
| 5 | StartDate | DateTime | - | - | Ngày bắt đầu |
| 6 | EndDate | DateTime | - | - | Ngày kết thúc |
| 7 | MinOrderAmount | Money? | - | - | Giá trị đơn hàng tối thiểu (nullable) |
| 8 | IsActive | bool | - | - | Đang hoạt động |
| 9 | Priority | int | - | - | Độ ưu tiên |

### PromotionProduct
**Mô tả:** Lưu trữ sản phẩm trong chương trình khuyến mãi với tùy chọn giảm giá tùy chỉnh cho từng sản phẩm.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã (tự động tăng) |
| 2 | PromotionId | int | - | ✓ Promotion.Id | Mã khuyến mãi |
| 3 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 4 | CustomDiscount | Percentage? | - | - | Giảm giá tùy chỉnh (nullable) |

## Analytics Module

### UserBehavior
**Mô tả:** Lưu trữ hành vi người dùng (View, Click, AddToCart, Purchase, Rating) cho hệ thống gợi ý ML.NET với đánh giá và dữ liệu thêm.

| STT | Tên trường | Kiểu dữ liệu | Khoá chính | Khoá ngoại | Mô tả |
|-----|------------|--------------|------------|------------|--------|
| 1 | Id | int | ✓ | - | Mã hành vi (tự động tăng) |
| 2 | UserId | int | - | ✓ ApplicationUser.Id | Mã người dùng |
| 3 | ProductId | int | - | ✓ Product.Id | Mã sản phẩm |
| 4 | BehaviorType | BehaviorType (Enum) | - | - | Loại hành vi (View, Click, AddToCart, Purchase, Rating) |
| 5 | Timestamp | DateTime | - | - | Thời gian |
| 6 | Rating | float | - | - | Đánh giá (0-5) cho collaborative filtering |
| 7 | AdditionalData | string? | - | - | Dữ liệu thêm (JSON) |

---

## Quan Hệ Giữa Các Entities

### Quan Hệ Trong Cùng Module (Intra-Module)

#### Catalog Module
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| Category | Category | 1:N (Parent-Children) | Một danh mục có thể có nhiều danh mục con (ParentId) |
| Category | Product | 1:N | Một danh mục có thể có nhiều sản phẩm |
| Brand | Product | 1:N | Một thương hiệu có thể có nhiều sản phẩm |
| Supplier | Product | 1:N | Một nhà cung cấp có thể cung cấp nhiều sản phẩm |
| Product | ProductVariant | 1:N | Một sản phẩm có thể có nhiều biến thể |
| Product | ProductImage | 1:N | Một sản phẩm có thể có nhiều hình ảnh |
| Product | ProductComment | 1:N | Một sản phẩm có thể có nhiều bình luận |
| ProductComment | ProductComment | 1:N (Parent-Child) | Một bình luận có thể có nhiều câu trả lời (ParentCommentId) |

#### Sales Module
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| Order | OrderItem | 1:N | Một đơn hàng có thể có nhiều mặt hàng |
| Order | OrderShipment | 1:N | Một đơn hàng có thể có nhiều lô vận chuyển |
| OrderItem | OrderWarehouseAllocation | 1:N | Một item có thể được phân bổ từ nhiều kho |
| ReturnOrder | ReturnOrderItem | 1:N | Một yêu cầu trả hàng có thể có nhiều item |
| Warranty | WarrantyClaim | 1:N | Một bảo hành có thể có nhiều khiếu nại |

#### Inventory Module
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| Warehouse | ProductWarehouse | 1:N | Một kho có thể lưu nhiều sản phẩm |
| StockEntry | StockEntryDetail | 1:N | Một phiếu nhập có thể có nhiều chi tiết sản phẩm |
| Supplier | StockEntry | 1:N | Một nhà cung cấp có thể có nhiều phiếu nhập |

#### Installation Module
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| TechnicianProfile | InstallationSlot | 1:N | Một kỹ thuật viên có thể có nhiều khung giờ |
| TechnicianProfile | InstallationBooking | 1:N | Một kỹ thuật viên có thể có nhiều lịch hẹn |
| InstallationSlot | InstallationBooking | 1:1 (Optional) | Một khung giờ có thể được gán cho một lịch hẹn (BookingId) |
| InstallationBooking | InstallationMaterial | 1:N | Một lịch lắp đặt có thể sử dụng nhiều vật tư |

#### Communication Module
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| ChatRoom | ChatMessage | 1:N | Một phòng chat có thể có nhiều tin nhắn |
| ChatRoom | ChatParticipant | 1:N | Một phòng chat có thể có nhiều người tham gia |
| ChatMessage | ChatAttachment | 1:N | Một tin nhắn có thể có nhiều file đính kèm |

#### Promotions Module
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| Promotion | PromotionProduct | 1:N | Một chương trình khuyến mãi có thể áp dụng cho nhiều sản phẩm |

### Quan Hệ Giữa Các Module Khác Nhau (Cross-Module)

#### Product ↔ Sales
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| Product | OrderItem | N:1 | Một sản phẩm có thể có trong nhiều order item |
| ProductVariant | OrderItem | N:1 | Một biến thể có thể có trong nhiều order item |
| Product | CartItem | N:1 | Một sản phẩm có thể có trong nhiều cart item |
| ProductVariant | CartItem | N:1 | Một biến thể có thể có trong nhiều cart item |
| Product | ProductComment | N:1 | Một sản phẩm có thể có nhiều bình luận |

#### Order ↔ User
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| ApplicationUser | Order | 1:N | Một người dùng có thể có nhiều đơn hàng |
| ApplicationUser | CartItem | 1:N | Một người dùng có thể có nhiều item trong giỏ |
| ApplicationUser | ProductComment | 1:N | Một người dùng có thể có nhiều bình luận |
| ApplicationUser | UserAddress | 1:N | Một người dùng có thể có nhiều địa chỉ |
| ApplicationUser | Notification | 1:N | Một người dùng có thể nhận nhiều thông báo |
| ApplicationUser | ChatMessage | 1:N | Một người dùng có thể gửi nhiều tin nhắn |

#### Order ↔ Installation
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| Order | InstallationBooking | 1:N (Optional) | Một đơn hàng có thể có nhiều lịch lắp đặt |
| OrderItem | InstallationBooking | 1:1 (Optional) | Một item có thể được gán một lịch lắp đặt |

#### Order ↔ Inventory
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| OrderItem | Warehouse | N:1 (Through Allocation) | Một item có thể được phân bổ từ một kho |
| OrderItem | Warehouse | N:M (Through Allocation) | Một item có thể được phân bổ từ nhiều kho (OrderWarehouseAllocation) |
| Product | ProductWarehouse | N:M | Một sản phẩm có thể tồn ở nhiều kho |
| ProductVariant | ProductWarehouse | N:M | Một biến thể có thể tồn ở nhiều kho |

#### Order ↔ Warranty
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| OrderItem | Warranty | 1:1 (Optional) | Một item có thể có một bảo hành |
| Warranty | WarrantyClaim | 1:N | Một bảo hành có thể có nhiều khiếu nại |
| TechnicianProfile | WarrantyClaim | N:1 | Một kỹ thuật viên có thể xử lý nhiều khiếu nại |

#### Communication ↔ Business Entities
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| ChatRoom | Order | N:1 (Optional) | Một phòng chat có thể liên kết với một đơn hàng (RelatedOrderId) |
| ChatRoom | InstallationBooking | N:1 (Optional) | Một phòng chat có thể liên kết với một lịch lắp đặt (RelatedInstallationId) |
| ChatRoom | WarrantyClaim | N:1 (Optional) | Một phòng chat có thể liên kết với một khiếu nại (RelatedWarrantyClaimId) |
| ApplicationUser | ChatParticipant | 1:N | Một người dùng có thể tham gia nhiều phòng chat |
| ApplicationUser | ChatMessage | 1:N | Một người dùng có thể gửi nhiều tin nhắn |

#### User ↔ Technician
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| ApplicationUser | TechnicianProfile | 1:1 (Optional) | Một user có thể liên kết với một hồ sơ kỹ thuật viên (UserId) |

#### Analytics
| Entity A | Entity B | Loại quan hệ | Mô tả |
|----------|----------|--------------|-------|
| ApplicationUser | UserBehavior | 1:N | Một người dùng có thể có nhiều hành vi |
| Product | UserBehavior | 1:N | Một sản phẩm có thể có nhiều hành vi từ nhiều người dùng |

### Tóm Tắt Các Quan Hệ Chính

**Core Business Flow:**
```
User (ApplicationUser) → Order (1:N)
Order → OrderItem (1:N)
OrderItem → Product (N:1)
OrderItem → ProductVariant (N:1)
OrderItem → InstallationBooking (1:1 Optional)
OrderItem → WarehouseAllocation (1:N)
```

**Inventory Flow:**
```
Product → ProductWarehouse (N:M)
ProductWarehouse → Warehouse (N:1)
Warehouse → StockEntry (1:N)
StockEntry → StockEntryDetail (1:N)
StockEntry → Supplier (N:1)
```

**Installation Flow:**
```
TechnicianProfile → InstallationSlot (1:N)
InstallationSlot → InstallationBooking (1:1 Optional)
InstallationBooking → Order (N:1)
InstallationBooking → InstallationMaterial (1:N)
```

**Communication Flow:**
```
ChatRoom → ChatMessage (1:N)
ChatRoom → ChatParticipant (1:N)
ChatRoom → Order/Installation/WarrantyClaim (N:1 Optional)
ApplicationUser → ChatMessage (1:N)
```

---

## Lưu ý quan trọng:
- **Shipping module (ShippingZone, ShippingRate)**: Entities tồn tại nhưng **không được sử dụng** trong production. Hệ thống sử dụng `IShippingService` với logic đơn giản (hardcode phí ship 30.000đ, miễn phí cho đơn >= 500.000đ).
- **Value Objects**: Các trường kiểu `Money`, `PhoneNumber`, `Address`, `Email`, `Sku`, `Weight`, `Percentage` là Value Objects, không phải entities riêng biệt.
- **Enums**: Các trường kiểu Enum như `OrderStatus`, `PaymentMethod`, `InstallationStatus`... được định nghĩa trong `Domain/Enums/`.
