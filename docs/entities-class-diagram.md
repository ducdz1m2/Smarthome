# Sơ Đồ Class Entities - Smarthome Project

```mermaid
classDiagram
    %% BASE CLASSES
    class Entity {
        +int Id
        <<abstract>>
    }
    
    class AggregateRoot {
        +AddDomainEvent(event)
        +GetDomainEvents()
        <<abstract>>
    }
    
    AggregateRoot --|> Entity

    %% CATALOG MODULE
    class Product {
        +int Id
        +string Name
        +Sku Sku
        +Money BasePrice
        +int StockQuantity
        +int FrozenStockQuantity
        +string? Description
        +string SpecsJson
        +bool IsActive
        +bool RequiresInstallation
        +int CategoryId
        +int BrandId
        +int? SupplierId
        +Create(name, sku, basePrice, categoryId, brandId, supplierId)
        +Update(name, basePrice, description)
        +AddVariant(sku, price, attributes)
        +AddImage(url, isMain, sortOrder)
    }
    
    class Category {
        +int Id
        +string Name
        +string? Description
        +int? ParentId
        +int SortOrder
        +bool IsActive
        +Create(name, parentId, sortOrder, description)
        +MoveTo(newParentId)
        +AddChild(name)
    }
    
    class Brand {
        +int Id
        +string Name
        +string? Description
        +string? LogoUrl
        +string? Website
        +bool IsActive
        +Create(name, description, logoUrl, website)
    }
    
    class ProductVariant {
        +int Id
        +int ProductId
        +Sku Sku
        +Money Price
        +int StockQuantity
        +int FrozenStockQuantity
        +string AttributesJson
        +bool IsActive
        +AddStock(quantity)
        +ReserveStock(quantity)
        +DeductStock(quantity)
    }
    
    class ProductImage {
        +int Id
        +int ProductId
        +string Url
        +string? AltText
        +bool IsMain
        +int SortOrder
        +SetAsMain()
        +UpdateSortOrder(newOrder)
    }
    
    class ProductComment {
        +int Id
        +int ProductId
        +int UserId
        +int OrderId
        +string Content
        +int Rating
        +bool IsApproved
        +bool IsVerifiedPurchase
        +int? ParentCommentId
        +Approve()
        +Reject()
        +UpdateContent(newContent)
    }

    Product --|> AggregateRoot
    Category --|> Entity
    Brand --|> Entity
    ProductVariant --|> Entity
    ProductImage --|> Entity
    ProductComment --|> Entity

    Product "1" --> "*" ProductVariant
    Product "1" --> "*" ProductImage
    Product "1" --> "*" ProductComment
    Category "1" --> "*" Category : Parent
    Category "1" --> "*" Product
    Brand "1" --> "*" Product
    ProductComment "1" --> "0..*" ProductComment : Parent

    %% SALES MODULE
    class Order {
        +int Id
        +string OrderNumber
        +OrderStatus Status
        +int UserId
        +string ReceiverName
        +PhoneNumber ReceiverPhone
        +Address ShippingAddress
        +Money TotalAmount
        +Money ShippingFee
        +Money DiscountAmount
        +PaymentMethod PaymentMethod
        +ShippingMethod ShippingMethod
        +string StatusHistoryJson
        +string? CancelReason
        +AddItem(productId, variantId, quantity, unitPrice)
        +Confirm()
        +ApplyShippingFee(fee)
        +ApplyDiscount(discount)
        +Complete()
        +Cancel(reason, cancelledByUserId)
    }
    
    class OrderItem {
        +int Id
        +int OrderId
        +int ProductId
        +int? VariantId
        +int Quantity
        +Money UnitPrice
        +bool IsShipped
        +bool IsInstalled
        +bool IsReserved
        +bool RequiresInstallation
        +int? WarehouseId
        +int? InstallationBookingId
        +MarkAsShipped()
        +MarkAsInstalled()
        +AssignInstallation(bookingId)
        +Reserve()
    }
    
    class CartItem {
        +int Id
        +int UserId
        +int ProductId
        +int? VariantId
        +int Quantity
        +DateTime AddedAt
        +UpdateQuantity(newQuantity)
    }
    
    class PaymentTransaction {
        +int Id
        +int OrderId
        +Money Amount
        +PaymentMethod Method
        +PaymentTransactionStatus Status
        +string? TransactionCode
        +string? GatewayResponse
        +DateTime? PaidAt
        +MarkSuccess(transactionCode, gatewayResponse)
        +MarkFailed(reason)
        +MarkRefunded(refundTransactionCode)
    }
    
    class ReturnOrder {
        +int Id
        +int OriginalOrderId
        +ReturnType ReturnType
        +string Reason
        +ReturnOrderStatus Status
        +Money? RefundAmount
        +DateTime? ApprovedAt
        +DateTime? ReceivedAt
        +DateTime? CompletedAt
        +AddItem(orderItemId, quantity, itemReason)
        +Approve(refundAmount)
        +MarkReceived()
        +Complete()
    }
    
    class ReturnOrderItem {
        +int Id
        +int ReturnOrderId
        +int OrderItemId
        +int Quantity
        +string Reason
    }
    
    class Warranty {
        +int Id
        +int OrderItemId
        +int ProductId
        +DateTime StartDate
        +DateTime EndDate
        +WarrantyStatus Status
        +IsValid(date)
        +Extend(additionalMonths)
        +CreateClaim(issue)
    }
    
    class WarrantyClaim {
        +int Id
        +int WarrantyId
        +DateTime ClaimDate
        +string Issue
        +string? Resolution
        +WarrantyClaimStatus Status
        +int? TechnicianId
        +AssignTechnician(technicianId)
        +Resolve(resolution, isApproved)
    }
    
    class OrderShipment {
        +int Id
        +int OrderId
        +int? ShipperId
        +string Carrier
        +string TrackingNumber
        +OrderShipmentStatus Status
        +DateTime? PickedUpAt
        +DateTime? DeliveredAt
        +string? Notes
        +DateTime? ApprovedAt
        +int? ApprovedBy
        +Approve(approvedBy)
        +AssignShipper(shipperId)
        +MarkPickedUp()
        +MarkDelivered()
    }
    
    class OrderWarehouseAllocation {
        +int Id
        +int OrderItemId
        +int WarehouseId
        +int AllocatedQuantity
        +bool IsConfirmed
        +DateTime? ConfirmedAt
        +Confirm()
    }

    Order --|> AggregateRoot
    ReturnOrder --|> AggregateRoot
    Warranty --|> AggregateRoot
    OrderItem --|> Entity
    CartItem --|> Entity
    PaymentTransaction --|> Entity
    ReturnOrderItem --|> Entity
    WarrantyClaim --|> Entity
    OrderShipment --|> Entity
    OrderWarehouseAllocation --|> Entity

    Order "1" --> "*" OrderItem
    Order "1" --> "*" OrderShipment
    OrderItem "1" --> "*" OrderWarehouseAllocation
    ReturnOrder "1" --> "*" ReturnOrderItem
    Warranty "1" --> "*" WarrantyClaim

    %% INVENTORY MODULE
    class Warehouse {
        +int Id
        +string Name
        +string Code
        +Address Address
        +PhoneNumber? Phone
        +string? ManagerName
        +bool IsActive
        +Create(name, code, address, phone, managerName)
        +Update(name, address, phone, managerName)
    }
    
    class ProductWarehouse {
        +int Id
        +int ProductId
        +int? VariantId
        +int WarehouseId
        +int Quantity
        +int ReservedQuantity
        +Receive(quantity)
        +Dispatch(quantity)
        +Reserve(quantity)
        +Release(quantity)
    }
    
    class StockEntry {
        +int Id
        +DateTime EntryDate
        +int SupplierId
        +int WarehouseId
        +string? Note
        +decimal TotalCost
        +bool IsCompleted
        +AddItem(productId, quantity, unitCost, variantId)
        +Complete()
    }
    
    class StockEntryDetail {
        +int Id
        +int StockEntryId
        +int ProductId
        +int? VariantId
        +int Quantity
        +Money UnitCost
        +GetTotalCost()
    }
    
    class Supplier {
        +int Id
        +string Name
        +string? TaxCode
        +Address? Address
        +string? ContactName
        +PhoneNumber? Phone
        +Email? Email
        +string? BankAccount
        +string? BankName
        +bool IsActive
        +Create(name, taxCode, address, contactName, phone, email)
        +UpdateBankInfo(bankAccount, bankName)
    }

    Warehouse --|> AggregateRoot
    StockEntry --|> AggregateRoot
    Supplier --|> AggregateRoot
    ProductWarehouse --|> Entity
    StockEntryDetail --|> Entity

    Warehouse "1" --> "*" ProductWarehouse
    Supplier "1" --> "*" StockEntry
    Warehouse "1" --> "*" StockEntry
    StockEntry "1" --> "*" StockEntryDetail

    %% IDENTITY MODULE
    class ApplicationUser {
        +int Id
        +string UserName
        +string Email
        +string FullName
        +string? Avatar
        +bool IsActive
        +DateTime? LastLoginAt
        +DateTime CreatedAt
    }
    
    class ApplicationRole {
        +int Id
        +string Name
        +string? Description
        +DateTime CreatedAt
    }

    ApplicationUser --|> "IdentityUser<int>"
    ApplicationRole --|> "IdentityRole<int>"

    %% INSTALLATION MODULE
    class InstallationBooking {
        +int Id
        +int OrderId
        +int TechnicianId
        +int SlotId
        +InstallationStatus Status
        +DateTime ScheduledDate
        +TimeSpan EstimatedDuration
        +bool MaterialsPrepared
        +DateTime? OnTheWayAt
        +DateTime? StartedAt
        +DateTime? CompletedAt
        +int? CustomerRating
        +string? CustomerSignature
        +string? Notes
        +bool IsUninstall
        +bool IsWarranty
        +Accept()
        +StartTravel()
        +StartInstallation()
        +Complete(customerSignature, customerRating)
        +Reschedule(newSlotId, newDate)
    }
    
    class InstallationSlot {
        +int Id
        +int TechnicianId
        +DateTime Date
        +TimeSpan StartTime
        +TimeSpan EndTime
        +bool IsBooked
        +int? BookingId
        +Book(bookingId)
        +Release()
    }
    
    class TechnicianProfile {
        +int Id
        +int? UserId
        +string FullName
        +PhoneNumber PhoneNumber
        +Email? Email
        +string? IdentityCard
        +Address? Address
        +DateTime? DateOfBirth
        +string EmployeeCode
        +DateTime HireDate
        +Money BaseSalary
        +string City
        +string Districts
        +string SkillsJson
        +bool IsAvailable
        +double Rating
        +int CompletedJobs
        +int CancelledJobs
        +AddSkill(skill)
        +CanHandle(district, requiredSkill)
        +CompleteJob(customerRating)
    }
    
    class InstallationMaterial {
        +int Id
        +int InstallationBookingId
        +int ProductId
        +int QuantityTaken
    }

    InstallationBooking --|> AggregateRoot
    TechnicianProfile --|> AggregateRoot
    InstallationSlot --|> Entity
    InstallationMaterial --|> Entity

    TechnicianProfile "1" --> "*" InstallationSlot
    TechnicianProfile "1" --> "*" InstallationBooking
    InstallationSlot "1" --> "0..1" InstallationBooking
    InstallationBooking "1" --> "*" InstallationMaterial

    %% COMMUNICATION MODULE
    class ChatRoom {
        +int Id
        +string Title
        +ChatRoomType Type
        +int? RelatedOrderId
        +int? RelatedInstallationId
        +int? RelatedWarrantyClaimId
        +DateTime CreatedAt
        +DateTime? ClosedAt
        +bool IsActive
        +AddMessage(senderId, senderType, content, attachments)
        +Join(userId, userType)
        +Leave(userId)
        +AssignTechnician(technicianId)
        +Close()
    }
    
    class ChatMessage {
        +int Id
        +int ChatRoomId
        +int SenderId
        +UserType SenderType
        +string Content
        +DateTime SentAt
        +DateTime? EditedAt
        +bool IsDeleted
        +DateTime? DeletedAt
        +string? DeletedBy
        +Edit(newContent)
        +Delete(deletedBy)
    }
    
    class ChatParticipant {
        +int Id
        +int ChatRoomId
        +int UserId
        +UserType UserType
        +bool IsBlocked
        +string? BlockedReason
        +DateTime? LastActivityAt
        +int UnreadCount
        +MarkAsRead()
        +Block(reason)
        +Leave()
    }
    
    class ChatAttachment {
        +int Id
        +int ChatMessageId
        +string FileName
        +string FileUrl
        +string? FileType
        +long? FileSize
        +DateTime UploadedAt
    }
    
    class Notification {
        +int Id
        +int UserId
        +UserType UserType
        +NotificationType Type
        +string Title
        +string Message
        +string? ActionUrl
        +string? Icon
        +bool IsRead
        +DateTime? ReadAt
        +bool IsSent
        +DateTime? SentAt
        +DateTime CreatedAt
        +string? SendError
        +int? RelatedEntityId
        +string? RelatedEntityType
        +MarkAsRead()
        +MarkAsSent()
        +MarkSendFailed(error)
    }

    ChatRoom --|> AggregateRoot
    ChatMessage --|> Entity
    ChatParticipant --|> Entity
    ChatAttachment --|> Entity
    Notification --|> Entity

    ChatRoom "1" --> "*" ChatMessage
    ChatRoom "1" --> "*" ChatParticipant
    ChatMessage "1" --> "*" ChatAttachment

    %% CONTENT MODULE
    class Banner {
        +int Id
        +string Title
        +string? Subtitle
        +WebsiteUrl ImageUrl
        +WebsiteUrl? LinkUrl
        +string Position
        +int SortOrder
        +DateTime? StartDate
        +DateTime? EndDate
        +bool IsActive
        +int ClickCount
        +Update(title, subtitle, linkUrl, position, sortOrder, startDate, endDate)
        +IncrementClick()
        +IsVisible()
    }
    
    class UserAddress {
        +int Id
        +int UserId
        +string Label
        +string ReceiverName
        +PhoneNumber ReceiverPhone
        +Address Address
        +bool IsDefault
        +Update(label, receiverName, receiverPhone, address)
        +SetAsDefault()
    }

    Banner --|> Entity
    UserAddress --|> AggregateRoot

    %% PROMOTIONS MODULE
    class Coupon {
        +int Id
        +string Code
        +DiscountType DiscountType
        +Money DiscountValue
        +Money? MinOrderAmount
        +Money? MaxDiscountAmount
        +DateTime ExpiryDate
        +int MaxUsage
        +int UsedCount
        +bool IsActive
        +IsValid(orderAmount)
        +CalculateDiscount(orderAmount)
        +IncrementUsage()
    }
    
    class Promotion {
        +int Id
        +string Name
        +string? Description
        +Percentage DiscountPercent
        +DateTime StartDate
        +DateTime EndDate
        +Money? MinOrderAmount
        +bool IsActive
        +int Priority
        +AddProduct(productId, customDiscount)
        +CalculateDiscount(originalPrice, productId)
        +IsActiveNow()
    }
    
    class PromotionProduct {
        +int Id
        +int PromotionId
        +int ProductId
        +Percentage? CustomDiscount
        +GetEffectiveDiscount(defaultDiscount)
    }

    Coupon --|> AggregateRoot
    Promotion --|> AggregateRoot
    PromotionProduct --|> Entity

    Promotion "1" --> "*" PromotionProduct

    %% ANALYTICS MODULE
    class UserBehavior {
        +int Id
        +int UserId
        +int ProductId
        +BehaviorType BehaviorType
        +DateTime Timestamp
        +float Rating
        +string? AdditionalData
    }

    UserBehavior --|> Entity

    %% CROSS-MODULE RELATIONSHIPS
    Product "0..1" --> "0..*" Supplier
    Order "1" --> "0..1" ApplicationUser
    OrderItem "1" --> "0..1" Product
    CartItem "1" --> "0..1" Product
    ProductComment "1" --> ApplicationUser
    InstallationBooking "1" --> "0..1" Order
    Notification "1" --> ApplicationUser
    ChatMessage "1" --> ApplicationUser
    UserAddress "1" --> ApplicationUser
    UserBehavior "1" --> ApplicationUser
    UserBehavior "1" --> Product
    TechnicianProfile "0..1" --> ApplicationUser
```

## Lưu ý
- Sơ đồ này sử dụng MermaidJS, có thể render trực tiếp trong GitHub, GitLab, VS Code với extension Mermaid
- Để xem sơ đồ, hãy mở file này trong editor hỗ trợ Mermaid hoặc sử dụng [Mermaid Live Editor](https://mermaid.live/)
- Shipping module (ShippingZone, ShippingRate) không được bao gồm vì không được sử dụng trong production
