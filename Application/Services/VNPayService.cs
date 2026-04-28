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
                Console.WriteLine($"[VNPayService] CreatePaymentUrlAsync: OrderId={orderId}, Amount={amount}, OrderInfo={orderInfo}");
                
                var vnpayConfig = _configuration.GetSection("VNPay");
                var tmnCode = vnpayConfig["TmnCode"];
                var hashSecret = vnpayConfig["HashSecret"];
                var baseUrl = vnpayConfig["BaseUrl"];
                var returnUrl = vnpayConfig["ReturnUrl"];

                Console.WriteLine($"[VNPayService] Config: TmnCode={tmnCode}, BaseUrl={baseUrl}, ReturnUrl={returnUrl}");

                if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(returnUrl))
                {
                    Console.WriteLine("[VNPayService] Missing required configuration");
                    return new VNPayResponse
                    {
                        Success = false,
                        Message = "Cấu hình VNPay chưa được thiết lập. Vui lòng liên hệ quản trị viên."
                    };
                }

                if (tmnCode == "YOUR_TMN_CODE" || hashSecret == "YOUR_HASH_SECRET")
                {
                    Console.WriteLine("[VNPayService] Demo configuration detected");
                    return new VNPayResponse
                    {
                        Success = false,
                        Message = "Cấu hình VNPay đang ở chế độ demo. Vui lòng cập nhật thông tin VNPay thực tế trong appsettings.json"
                    };
                }

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    Console.WriteLine($"[VNPayService] Order not found: {orderId}");
                    return new VNPayResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn hàng"
                    };
                }

                Console.WriteLine($"[VNPayService] Order found: {orderId}, TotalAmount={order.TotalAmount}");

                // Use actual order ID instead of ticks for better tracking
                // vnp_OrderInfo must be Vietnamese without accents and no special characters
                var cleanOrderInfo = System.Text.RegularExpressions.Regex.Replace(orderInfo, "[^a-zA-Z0-9 ]", "").Replace(" ", "");
                
                var vnpayData = new Dictionary<string, string>
                {
                    { "vnp_Version", "2.1.0" },
                    { "vnp_Command", "pay" },
                    { "vnp_TmnCode", tmnCode },
                    { "vnp_Amount", ((long)(amount * 100)).ToString() },
                    { "vnp_CurrCode", "VND" },
                    { "vnp_TxnRef", orderId.ToString() },
                    { "vnp_OrderInfo", cleanOrderInfo },
                    { "vnp_OrderType", "billpayment" },
                    { "vnp_Locale", "vn" },
                    { "vnp_ReturnUrl", returnUrl },
                    { "vnp_IpAddr", GetIpAddress() },
                    { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
                };

                // VNPay requires URL encoding for hash data (like Python's urllib.parse.urlencode)
                var sortedData = vnpayData.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                var hashData = string.Join("&", sortedData.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
                var secureHash = HmacSHA512(hashSecret, hashData);

                Console.WriteLine($"[VNPayService] Hash computed: {secureHash}");

                // Build the actual payment URL with URL-encoded values
                var queryString = string.Join("&", sortedData.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
                var paymentUrl = $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";

                Console.WriteLine($"[VNPayService] Payment URL created: {paymentUrl}");

                return new VNPayResponse
                {
                    Success = true,
                    PaymentUrl = paymentUrl,
                    TransactionId = orderId.ToString()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VNPayService] Error: {ex.Message}");
                Console.WriteLine($"[VNPayService] Stack trace: {ex.StackTrace}");
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
                Console.WriteLine($"[VNPayService] ProcessIpnAsync: {string.Join(", ", vnpayData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
                
                var vnpayConfig = _configuration.GetSection("VNPay");
                var hashSecret = vnpayConfig["HashSecret"];

                if (string.IsNullOrEmpty(hashSecret))
                {
                    Console.WriteLine("[VNPayService] ProcessIpnAsync: Missing hash secret");
                    return false;
                }

                if (!vnpayData.ContainsKey("vnp_SecureHash"))
                {
                    Console.WriteLine("[VNPayService] ProcessIpnAsync: Missing vnp_SecureHash");
                    return false;
                }

                var receivedHash = vnpayData["vnp_SecureHash"];
                Console.WriteLine($"[VNPayService] ProcessIpnAsync: Received hash: {receivedHash}");
                
                // Exclude both vnp_SecureHash and vnp_SecureHashType from hash computation
                var vnpayDataWithoutHash = vnpayData
                    .Where(x => x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType")
                    .ToDictionary(x => x.Key, x => x.Value);

                // VNPay requires sorted keys with raw (non-encoded) values for hash verification
                var sortedData = vnpayDataWithoutHash.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                var hashData = string.Join("&", sortedData.Select(x => $"{x.Key}={x.Value}"));
                var calculatedHash = HmacSHA512(hashSecret, hashData);

                Console.WriteLine($"[VNPayService] ProcessIpnAsync: Calculated hash: {calculatedHash}");

                if (receivedHash != calculatedHash)
                {
                    Console.WriteLine("[VNPayService] ProcessIpnAsync: Hash mismatch - signature invalid");
                    return false;
                }

                var responseCode = vnpayData.GetValueOrDefault("vnp_ResponseCode");
                var transactionStatus = vnpayData.GetValueOrDefault("vnp_TransactionStatus");

                Console.WriteLine($"[VNPayService] ProcessIpnAsync: ResponseCode={responseCode}, TransactionStatus={transactionStatus}");

                if (responseCode == "00" && transactionStatus == "00")
                {
                    Console.WriteLine("[VNPayService] ProcessIpnAsync: Payment successful");
                    return true;
                }

                Console.WriteLine("[VNPayService] ProcessIpnAsync: Payment failed or pending");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VNPayService] ProcessIpnAsync Error: {ex.Message}");
                Console.WriteLine($"[VNPayService] ProcessIpnAsync Stack trace: {ex.StackTrace}");
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
                    hash.Append(theByte.ToString("x2")); // Lowercase hex
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
