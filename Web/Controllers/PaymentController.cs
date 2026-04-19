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

        public PaymentController(IVNPayService vnPayService, IOrderRepository orderRepository)
        {
            _vnPayService = vnPayService;
            _orderRepository = orderRepository;
        }

        [HttpPost("vnpay/create-payment")]
        public async Task<IActionResult> CreateVNPayPayment([FromBody] VNPayPaymentRequest request)
        {
            var response = await _vnPayService.CreatePaymentUrlAsync(
                request.OrderId,
                request.Amount,
                request.OrderInfo
            );

            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }

        [HttpPost("vnpay/ipn")]
        public async Task<IActionResult> VNPayIpn([FromForm] Dictionary<string, string> vnpayData)
        {
            var isValid = await _vnPayService.ProcessIpnAsync(vnpayData);

            if (!isValid)
            {
                return BadRequest(new { RspCode = "99", Message = "Invalid signature or transaction" });
            }

            var orderId = int.Parse(vnpayData["vnp_TxnRef"]);
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order != null)
            {
                // Update order payment status
                // This will be handled by the order service
                // For now, just return success
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
