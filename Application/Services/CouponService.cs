using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.DTOs.Results;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Promotions;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _couponRepository;

        public CouponService(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        public async Task<List<CouponResponse>> GetAllAsync()
        {
            var coupons = await _couponRepository.GetAllAsync();
            return coupons.Select(MapToResponse).ToList();
        }

        public async Task<CouponResponse?> GetByIdAsync(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null) return null;
            return MapToResponse(coupon);
        }

        public async Task<CouponResponse?> GetByCodeAsync(string code)
        {
            var coupon = await _couponRepository.GetByCodeAsync(code);
            if (coupon == null) return null;
            return MapToResponse(coupon);
        }

        public async Task<int> CreateAsync(CreateCouponRequest request)
        {
            if (await _couponRepository.ExistsAsync(request.Code))
                throw new DomainException("Mã coupon đã tồn tại");

            if (!Enum.TryParse<DiscountType>(request.DiscountType, true, out var discountType))
                throw new DomainException("Loại giảm giá không hợp lệ");

            var coupon = Coupon.Create(
                request.Code,
                discountType,
                Domain.ValueObjects.Money.Vnd(request.DiscountValue),
                request.ExpiryDate,
                request.UsageLimit ?? 100
            );

            if (request.MinOrderAmount.HasValue || request.MaxDiscountAmount.HasValue)
            {
                coupon.SetConstraints(
                    request.MinOrderAmount.HasValue ? Domain.ValueObjects.Money.Vnd(request.MinOrderAmount.Value) : null,
                    request.MaxDiscountAmount.HasValue ? Domain.ValueObjects.Money.Vnd(request.MaxDiscountAmount.Value) : null
                );
            }

            await _couponRepository.AddAsync(coupon);
            await _couponRepository.SaveChangesAsync();
            return coupon.Id;
        }

        public async Task UpdateAsync(int id, UpdateCouponRequest request)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                throw new DomainException("Không tìm thấy coupon");

            // Coupon entity is immutable for most properties, only allow deactivate/reactivate
            if (request.IsActive && !coupon.IsActive)
            {
                // Reactivate - but need to check if expired
                if (DateTime.UtcNow > coupon.ExpiryDate)
                    throw new DomainException("Không thể kích hoạt coupon đã hết hạn");
            }
            else if (!request.IsActive && coupon.IsActive)
            {
                coupon.Deactivate();
            }

            _couponRepository.Update(coupon);
            await _couponRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                throw new DomainException("Không tìm thấy coupon");

            _couponRepository.Delete(coupon);
            await _couponRepository.SaveChangesAsync();
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null) return false;

            if (DateTime.UtcNow > coupon.ExpiryDate)
                throw new DomainException("Coupon đã hết hạn");

            // Cannot reactivate through this method, entity doesn't have Activate method
            _couponRepository.Update(coupon);
            await _couponRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null) return false;

            coupon.Deactivate();
            _couponRepository.Update(coupon);
            await _couponRepository.SaveChangesAsync();
            return true;
        }

        public async Task<ApplyCouponResult> ValidateAndApplyCouponAsync(string couponCode, decimal orderAmount)
        {
            Console.WriteLine($"[CouponService] ValidateAndApplyCouponAsync called with CouponCode: {couponCode}, OrderAmount: {orderAmount}");

            if (string.IsNullOrWhiteSpace(couponCode))
            {
                Console.WriteLine($"[CouponService] Coupon code is empty or null");
                return new ApplyCouponResult
                {
                    IsValid = false,
                    ErrorMessage = "Mã coupon không được để trống",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
            }

            var coupon = await _couponRepository.GetByCodeAsync(couponCode.ToUpper());
            if (coupon == null)
            {
                Console.WriteLine($"[CouponService] Coupon not found: {couponCode}");
                return new ApplyCouponResult
                {
                    IsValid = false,
                    ErrorMessage = "Mã coupon không tồn tại",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
            }

            Console.WriteLine($"[CouponService] Coupon found: {coupon.Code}, IsActive: {coupon.IsActive}, ExpiryDate: {coupon.ExpiryDate}");

            var orderAmountMoney = Money.Vnd(orderAmount);

            if (!coupon.IsValid(orderAmountMoney))
            {
                Console.WriteLine($"[CouponService] Coupon validation failed");
                if (!coupon.IsActive)
                {
                    return new ApplyCouponResult
                    {
                        IsValid = false,
                        ErrorMessage = "Mã coupon đã bị vô hiệu hóa",
                        DiscountAmount = 0,
                        FinalAmount = orderAmount
                    };
                }
                if (DateTime.UtcNow > coupon.ExpiryDate)
                {
                    return new ApplyCouponResult
                    {
                        IsValid = false,
                        ErrorMessage = "Mã coupon đã hết hạn",
                        DiscountAmount = 0,
                        FinalAmount = orderAmount
                    };
                }
                if (coupon.UsedCount >= coupon.MaxUsage)
                {
                    return new ApplyCouponResult
                    {
                        IsValid = false,
                        ErrorMessage = "Mã coupon đã hết lượt sử dụng",
                        DiscountAmount = 0,
                        FinalAmount = orderAmount
                    };
                }
                if (coupon.MinOrderAmount != null && orderAmountMoney.IsLessThan(coupon.MinOrderAmount))
                {
                    return new ApplyCouponResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Đơn hàng tối thiểu phải là {coupon.MinOrderAmount.Amount:N0}đ",
                        DiscountAmount = 0,
                        FinalAmount = orderAmount
                    };
                }
            }

            try
            {
                var discountAmount = coupon.CalculateDiscount(orderAmountMoney);
                Console.WriteLine($"[CouponService] Discount calculated: {discountAmount.Amount}");

                var finalAmount = orderAmountMoney.Subtract(discountAmount);
                if (finalAmount.IsLessThan(Money.Zero()))
                {
                    finalAmount = Money.Zero();
                }

                Console.WriteLine($"[CouponService] Final amount: {finalAmount.Amount}");

                return new ApplyCouponResult
                {
                    IsValid = true,
                    ErrorMessage = null,
                    DiscountAmount = discountAmount.Amount,
                    FinalAmount = finalAmount.Amount
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CouponService] Error calculating discount: {ex.Message}");
                return new ApplyCouponResult
                {
                    IsValid = false,
                    ErrorMessage = $"Lỗi khi tính giảm giá: {ex.Message}",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
            }
        }

        private CouponResponse MapToResponse(Coupon coupon)
        {
            return new CouponResponse
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountType = coupon.DiscountType.ToString(),
                DiscountValue = coupon.DiscountValue.Amount,
                MinOrderAmount = coupon.MinOrderAmount?.Amount,
                MaxDiscountAmount = coupon.MaxDiscountAmount?.Amount,
                ExpiryDate = coupon.ExpiryDate,
                UsageLimit = coupon.MaxUsage,
                UsageCount = coupon.UsedCount,
                IsActive = coupon.IsActive
            };
        }
    }
}
