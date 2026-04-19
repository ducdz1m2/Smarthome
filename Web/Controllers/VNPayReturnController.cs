using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [AllowAnonymous]
    public class VNPayReturnController : Controller
    {
        private readonly IVNPayService _vnPayService;
        private readonly IOrderService _orderService;

        public VNPayReturnController(IVNPayService vnPayService, IOrderService orderService)
        {
            _vnPayService = vnPayService;
            _orderService = orderService;
        }

        [HttpGet("payment/vnpay/return")]
        public async Task<IActionResult> Return([FromQuery] Dictionary<string, string> vnpayData)
        {
            var isValid = await _vnPayService.ProcessIpnAsync(vnpayData);

            if (!isValid)
            {
                return Redirect("/orders?payment=failed");
            }

            var responseCode = vnpayData["vnp_ResponseCode"];
            var transactionStatus = vnpayData["vnp_TransactionStatus"];

            if (responseCode == "00" && transactionStatus == "00")
            {
                // Payment successful
                var orderId = int.Parse(vnpayData["vnp_TxnRef"]);
                // Update order payment status
                await _orderService.UpdatePaymentStatusAsync(orderId, "VNPay", vnpayData["vnp_TransactionNo"]);
                
                return Redirect($"/orders/{orderId}?payment=success");
            }

            return Redirect("/orders?payment=failed");
        }
    }
}
