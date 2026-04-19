using Application.DTOs.Responses;

namespace Application.Interfaces.Services
{
    public interface IVNPayService
    {
        Task<VNPayResponse> CreatePaymentUrlAsync(int orderId, decimal amount, string orderInfo);
        Task<bool> ProcessIpnAsync(Dictionary<string, string> vnpayData);
    }
}
