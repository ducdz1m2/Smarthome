using MudBlazor;

namespace Web.Helpers
{
    public static class StatusHelper
    {
        // Order Status Mappings
        public static Color GetOrderStatusColor(string status)
        {
            return status switch
            {
                "Pending" => Color.Warning,
                "Confirmed" => Color.Info,
                "AwaitingPickup" => Color.Info,
                "Shipping" => Color.Primary,
                "Shipped" => Color.Primary,
                "Delivered" => Color.Success,
                "AwaitingSchedule" => Color.Info,
                "Scheduled" => Color.Secondary,
                "TechnicianAssigned" => Color.Tertiary,
                "Preparing" => Color.Primary,
                "Installing" => Color.Primary,
                "Testing" => Color.Info,
                "Completed" => Color.Success,
                "Cancelled" => Color.Error,
                "Refunded" => Color.Warning,
                "ReturnRequested" => Color.Warning,
                "InstallationFailed" => Color.Error,
                _ => Color.Default
            };
        }

        public static string GetOrderStatusText(string status)
        {
            return status switch
            {
                "Pending" => "Chờ xác nhận",
                "Confirmed" => "Đã xác nhận",
                "AwaitingPickup" => "Chờ lấy hàng",
                "Shipping" => "Đang giao",
                "Shipped" => "Đang giao",
                "Delivered" => "Đã giao",
                "AwaitingSchedule" => "Chờ lịch",
                "Scheduled" => "Đã đặt lịch",
                "TechnicianAssigned" => "Đã giao kỹ thuật",
                "Preparing" => "Đang chuẩn bị",
                "Installing" => "Đang lắp đặt",
                "Testing" => "Đang kiểm tra",
                "Completed" => "Hoàn thành",
                "Cancelled" => "Đã hủy",
                "Refunded" => "Đã hoàn tiền",
                "ReturnRequested" => "Yêu cầu trả hàng",
                "InstallationFailed" => "Lắp đặt thất bại",
                _ => status
            };
        }

        // Installation Booking Status Mappings
        public static Color GetInstallationStatusColor(string status)
        {
            return status switch
            {
                "Pending" => Color.Warning,
                "Assigned" => Color.Warning,
                "Rescheduled" => Color.Warning,
                "Confirmed" => Color.Secondary,
                "Preparing" => Color.Primary,
                "OnTheWay" => Color.Primary,
                "Installing" => Color.Primary,
                "Testing" => Color.Info,
                "Completed" => Color.Success,
                "Cancelled" => Color.Error,
                "Failed" => Color.Error,
                "AwaitingMaterial" => Color.Warning,
                _ => Color.Default
            };
        }

        public static string GetInstallationStatusText(string status)
        {
            return status switch
            {
                "Pending" => "Chờ xử lý",
                "Assigned" => "Chờ xác nhận",
                "Rescheduled" => "Đã đổi lịch",
                "Confirmed" => "Đã xác nhận",
                "Preparing" => "Đang chuẩn bị",
                "OnTheWay" => "Đang di chuyển",
                "Installing" => "Đang lắp đặt",
                "Testing" => "Đang kiểm tra",
                "Completed" => "Hoàn thành",
                "Cancelled" => "Đã hủy",
                "Failed" => "Thất bại",
                "AwaitingMaterial" => "Chờ vật tư",
                _ => status
            };
        }

        public static Color GetInstallationStatusColor(string status, bool isUninstall)
        {
            if (isUninstall)
            {
                return status switch
                {
                    "Pending" => Color.Warning,
                    "Assigned" => Color.Warning,
                    "Rescheduled" => Color.Warning,
                    "Confirmed" => Color.Secondary,
                    "Preparing" => Color.Primary,
                    "OnTheWay" => Color.Primary,
                    "Installing" => Color.Primary,
                    "Testing" => Color.Info,
                    "Completed" => Color.Success,
                    "Cancelled" => Color.Error,
                    "Failed" => Color.Error,
                    "AwaitingMaterial" => Color.Warning,
                    _ => Color.Default
                };
            }

            return GetInstallationStatusColor(status);
        }

        public static string GetInstallationStatusText(string status, bool isUninstall)
        {
            if (isUninstall)
            {
                return status switch
                {
                    "Pending" => "Chờ xử lý",
                    "Assigned" => "Chờ xác nhận",
                    "Rescheduled" => "Đã đổi lịch",
                    "Confirmed" => "Đã xác nhận",
                    "Preparing" => "Đang chuẩn bị",
                    "OnTheWay" => "Đang di chuyển",
                    "Installing" => "Đang lắp đặt",
                    "Testing" => "Đang kiểm tra",
                    "Completed" => "Hoàn thành",
                    "Cancelled" => "Đã hủy",
                    "Failed" => "Thất bại",
                    _ => status
                };
            }

            return GetInstallationStatusText(status);
        }

        // Return Order Status Mappings
        public static Color GetReturnOrderStatusColor(string status)
        {
            return status switch
            {
                "Pending" => Color.Warning,
                "Approved" => Color.Info,
                "Received" => Color.Primary,
                "Completed" => Color.Success,
                "Rejected" => Color.Error,
                _ => Color.Default
            };
        }

        public static string GetReturnOrderStatusText(string status)
        {
            return status switch
            {
                "Pending" => "Yêu cầu hoàn hàng",
                "Approved" => "Đã duyệt hoàn hàng",
                "Received" => "Đã nhận hàng hoàn",
                "Completed" => "Đã hoàn thành hoàn hàng",
                "Rejected" => "Đã từ chối hoàn hàng",
                _ => status
            };
        }

        // Warranty Request Status Mappings
        public static Color GetWarrantyStatusColor(string status)
        {
            return status switch
            {
                "Pending" => Color.Warning,
                "Approved" => Color.Success,
                "Rejected" => Color.Error,
                "InProgress" => Color.Info,
                "Completed" => Color.Primary,
                _ => Color.Default
            };
        }

        public static string GetWarrantyStatusText(string status)
        {
            return status switch
            {
                "Pending" => "Yêu cầu bảo hành",
                "Approved" => "Đã duyệt bảo hành",
                "Rejected" => "Đã từ chối bảo hành",
                "InProgress" => "Đang bảo hành",
                "Completed" => "Bảo hành hoàn thành",
                _ => status
            };
        }

        // Shipment Status Mappings
        public static Color GetShipmentStatusColor(string status)
        {
            return status switch
            {
                "Pending" => Color.Warning,
                "PickedUp" => Color.Info,
                "InTransit" => Color.Primary,
                "Delivered" => Color.Success,
                "Failed" => Color.Error,
                _ => Color.Default
            };
        }

        public static string GetShipmentStatusText(string status)
        {
            return status switch
            {
                "Pending" => "Chờ xử lý",
                "PickedUp" => "Đã lấy hàng",
                "InTransit" => "Đang vận chuyển",
                "Delivered" => "Đã giao",
                "Failed" => "Thất bại",
                _ => status
            };
        }
    }
}
