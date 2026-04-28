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
            Console.WriteLine($"[VNPayReturnController] Return received: {string.Join(", ", vnpayData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
            
            var isValid = await _vnPayService.ProcessIpnAsync(vnpayData);

            if (!isValid)
            {
                Console.WriteLine("[VNPayReturnController] Invalid signature or transaction");
                return Redirect("/orders?payment=failed");
            }

            var responseCode = vnpayData["vnp_ResponseCode"];
            var transactionStatus = vnpayData["vnp_TransactionStatus"];

            Console.WriteLine($"[VNPayReturnController] ResponseCode={responseCode}, TransactionStatus={transactionStatus}");

            if (responseCode == "00" && transactionStatus == "00")
            {
                // Payment successful
                var orderId = int.Parse(vnpayData["vnp_TxnRef"]);
                var transactionNo = vnpayData.GetValueOrDefault("vnp_TransactionNo");
                
                Console.WriteLine($"[VNPayReturnController] Payment successful for order {orderId}, TransactionNo={transactionNo}");
                
                // Update order payment status
                try
                {
                    await _orderService.UpdatePaymentStatusAsync(orderId, "VNPay", transactionNo ?? "");
                    Console.WriteLine($"[VNPayReturnController] Payment status updated for order {orderId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VNPayReturnController] Error updating payment status: {ex.Message}");
                }
                
                return Redirect($"/orders/{orderId}?payment=success");
            }

            Console.WriteLine("[VNPayReturnController] Payment failed");
            return Redirect("/orders?payment=failed");
        }
    }
}
