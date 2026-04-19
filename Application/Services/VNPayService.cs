using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderRepository _orderRepository;

        public VNPayService(IConfiguration configuration, IOrderRepository orderRepository)
        {
            _configuration = configuration;
            _orderRepository = orderRepository;
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

                if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret))
                {
                    return new VNPayResponse
                    {
                        Success = false,
                        Message = "VNPay configuration is missing"
                    };
                }

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return new VNPayResponse
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                var vnpayData = new Dictionary<string, string>
                {
                    { "vnp_Version", "2.1.0" },
                    { "vnp_Command", "pay" },
                    { "vnp_TmnCode", tmnCode },
                    { "vnp_Amount", ((long)(amount * 100)).ToString() },
                    { "vnp_CurrCode", "VND" },
                    { "vnp_TxnRef", DateTime.Now.Ticks.ToString() },
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
                    TransactionId = vnpayData["vnp_TxnRef"]
                };
            }
            catch (Exception ex)
            {
                return new VNPayResponse
                {
                    Success = false,
                    Message = ex.Message
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

                var responseCode = vnpayData["vnp_ResponseCode"];
                var transactionStatus = vnpayData["vnp_TransactionStatus"];

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
            return "127.0.0.1";
        }
    }
}
