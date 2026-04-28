using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public PaymentController(IVNPayService vnPayService, IOrderRepository orderRepository, IOrderService orderService)
        {
            _vnPayService = vnPayService;
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        [HttpPost("vnpay/create-payment")]
        [Authorize]
        public async Task<IActionResult> CreateVNPayPayment([FromBody] VNPayPaymentRequest request)
        {
            Console.WriteLine($"[PaymentController] CreateVNPayPayment: OrderId={request.OrderId}, Amount={request.Amount}");
            var response = await _vnPayService.CreatePaymentUrlAsync(
                request.OrderId,
                request.Amount,
                request.OrderInfo
            );

            if (!response.Success)
            {
                Console.WriteLine($"[PaymentController] CreateVNPayPayment failed: {response.Message}");
                return BadRequest(new { message = response.Message });
            }

            Console.WriteLine($"[PaymentController] CreateVNPayPayment success: {response.PaymentUrl}");
            return Ok(response);
        }

        [HttpPost("vnpay/ipn")]
        [AllowAnonymous] // VNPay calls this from external server without authentication
        public async Task<IActionResult> VNPayIpn([FromForm] Dictionary<string, string> vnpayData)
        {
            Console.WriteLine($"[PaymentController] VNPayIpn received: {string.Join(", ", vnpayData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
            
            var isValid = await _vnPayService.ProcessIpnAsync(vnpayData);

            if (!isValid)
            {
                Console.WriteLine("[PaymentController] VNPayIpn: Invalid signature or transaction");
                return BadRequest(new { RspCode = "99", Message = "Invalid signature or transaction" });
            }

            var orderId = int.Parse(vnpayData["vnp_TxnRef"]);
            var responseCode = vnpayData.GetValueOrDefault("vnp_ResponseCode");
            var transactionStatus = vnpayData.GetValueOrDefault("vnp_TransactionStatus");
            var transactionNo = vnpayData.GetValueOrDefault("vnp_TransactionNo");

            Console.WriteLine($"[PaymentController] VNPayIpn: OrderId={orderId}, ResponseCode={responseCode}, TransactionStatus={transactionStatus}, TransactionNo={transactionNo}");

            if (responseCode == "00" && transactionStatus == "00")
            {
                // Payment successful - update order payment status
                try
                {
                    Console.WriteLine($"[PaymentController] Updating payment status for order {orderId}");
                    await _orderService.UpdatePaymentStatusAsync(orderId, "VNPay", transactionNo ?? "");
                    Console.WriteLine($"[PaymentController] Payment status updated successfully for order {orderId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PaymentController] Error updating payment status: {ex.Message}");
                    // Still return success to VNPay to avoid duplicate notifications
                }
            }

            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }
    }

    public class VNPayPaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
    }
}
