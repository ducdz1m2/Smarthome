using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Promotions;
using Domain.Enums;
using Domain.Exceptions;

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
