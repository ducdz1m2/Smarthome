using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderRepository _orderRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VNPayService(IConfiguration configuration, IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _orderRepository = orderRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<VNPayResponse> CreatePaymentUrlAsync(int orderId, decimal amount, string orderInfo)
        {
            try
            {
                var vnpayConfig = _configuration.GetSection("VNPay");
                var tmnCode = vnpayConfig["TmnCode"];
                var hashSecret = vnpayConfig["HashSecret"];
                var baseUrl = vnpayConfig["BaseUrl"];
                var returnUrl = vnpayConfig["ReturnUrl"];

                if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(returnUrl))
                {
                    return new VNPayResponse
                    {
                        Success = false,
                        Message = "Cấu hình VNPay chưa được thiết lập. Vui lòng liên hệ quản trị viên."
                    };
                }

                if (tmnCode == "YOUR_TMN_CODE" || hashSecret == "YOUR_HASH_SECRET")
                {
                    return new VNPayResponse
                    {
                        Success = false,
                        Message = "Cấu hình VNPay đang ở chế độ demo. Vui lòng cập nhật thông tin VNPay thực tế trong appsettings.json"
                    };
                }

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return new VNPayResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn hàng"
                    };
                }

                // Use actual order ID instead of ticks for better tracking
                var vnpayData = new Dictionary<string, string>
                {
                    { "vnp_Version", "2.1.0" },
                    { "vnp_Command", "pay" },
                    { "vnp_TmnCode", tmnCode },
                    { "vnp_Amount", ((long)(amount * 100)).ToString() },
                    { "vnp_CurrCode", "VND" },
                    { "vnp_TxnRef", orderId.ToString() },
                    { "vnp_OrderInfo", orderInfo },
                    { "vnp_OrderType", "billpayment" },
                    { "vnp_Locale", "vn" },
                    { "vnp_ReturnUrl", returnUrl },
                    { "vnp_IpAddr", GetIpAddress() },
                    { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
                };

                var sortedData = vnpayData.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                var queryString = string.Join("&", sortedData.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
                var secureHash = HmacSHA512(hashSecret, queryString);

                var paymentUrl = $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";

                return new VNPayResponse
                {
                    Success = true,
                    PaymentUrl = paymentUrl,
                    TransactionId = orderId.ToString()
                };
            }
            catch (Exception ex)
            {
                return new VNPayResponse
                {
                    Success = false,
                    Message = $"Lỗi khi tạo URL thanh toán: {ex.Message}"
                };
            }
        }

        public async Task<bool> ProcessIpnAsync(Dictionary<string, string> vnpayData)
        {
            try
            {
                var vnpayConfig = _configuration.GetSection("VNPay");
                var hashSecret = vnpayConfig["HashSecret"];

                if (string.IsNullOrEmpty(hashSecret))
                {
                    return false;
                }

                if (!vnpayData.ContainsKey("vnp_SecureHash"))
                {
                    return false;
                }

                var receivedHash = vnpayData["vnp_SecureHash"];
                var vnpayDataWithoutHash = vnpayData.Where(x => x.Key != "vnp_SecureHash")
                    .ToDictionary(x => x.Key, x => x.Value);

                var sortedData = vnpayDataWithoutHash.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                var queryString = string.Join("&", sortedData.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
                var calculatedHash = HmacSHA512(hashSecret, queryString);

                if (receivedHash != calculatedHash)
                {
                    return false;
                }

                var responseCode = vnpayData.GetValueOrDefault("vnp_ResponseCode");
                var transactionStatus = vnpayData.GetValueOrDefault("vnp_TransactionStatus");

                if (responseCode == "00" && transactionStatus == "00")
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        private string GetIpAddress()
        {
            try
            {
                var context = _httpContextAccessor?.HttpContext;
                if (context == null)
                {
                    return "127.0.0.1";
                }

                // Try to get IP from forwarded headers (for proxy/load balancer scenarios)
                var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedHeader))
                {
                    return forwardedHeader.Split(',')[0].Trim();
                }

                // Try to get IP from RemoteIpAddress
                var remoteIp = context.Connection.RemoteIpAddress;
                if (remoteIp != null)
                {
                    return remoteIp.ToString();
                }

                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }
}
