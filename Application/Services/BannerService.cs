using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Content;
using Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services
{
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMemoryCache _cache;
        private const string BannersCacheKey = "AllBanners";
        private const string BannersByPositionCacheKey = "BannersByPosition_";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);

        public BannerService(IBannerRepository bannerRepository, IMemoryCache cache)
        {
            _bannerRepository = bannerRepository;
            _cache = cache;
        }

        public async Task<List<BannerResponse>> GetAllAsync()
        {
            if (_cache.TryGetValue(BannersCacheKey, out List<BannerResponse>? cachedBanners))
            {
                return cachedBanners ?? new List<BannerResponse>();
            }

            var banners = await _bannerRepository.GetAllAsync();
            var result = banners.Select(MapToResponse).ToList();
            
            _cache.Set(BannersCacheKey, result, _cacheDuration);
            return result;
        }

        public async Task<List<BannerResponse>> GetByPositionAsync(string position)
        {
            var cacheKey = BannersByPositionCacheKey + position;
            
            if (_cache.TryGetValue(cacheKey, out List<BannerResponse>? cachedBanners))
            {
                return cachedBanners ?? new List<BannerResponse>();
            }

            var banners = await _bannerRepository.GetByPositionAsync(position);
            var result = banners.Select(MapToResponse).ToList();
            
            _cache.Set(cacheKey, result, _cacheDuration);
            return result;
        }

        public async Task<BannerResponse?> GetByIdAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null) return null;
            return MapToResponse(banner);
        }

        public async Task<int> CreateAsync(CreateBannerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ImageUrl))
                throw new DomainException("ImageUrl is required");

            var banner = Banner.Create(
                request.Title,
                Domain.ValueObjects.WebsiteUrl.Create(request.ImageUrl)!,
                request.Subtitle,
                request.LinkUrl != null ? Domain.ValueObjects.WebsiteUrl.Create(request.LinkUrl) : null,
                request.Position,
                request.SortOrder,
                request.StartDate,
                request.EndDate
            );

            await _bannerRepository.AddAsync(banner);
            await _bannerRepository.SaveChangesAsync();
            
            InvalidateCache();
            return banner.Id;
        }

        public async Task UpdateAsync(int id, UpdateBannerRequest request)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                throw new DomainException("Không tìm thấy banner");

            banner.Update(
                request.Title,
                request.Subtitle,
                request.LinkUrl != null ? Domain.ValueObjects.WebsiteUrl.Create(request.LinkUrl) : null,
                request.Position,
                request.SortOrder,
                request.StartDate,
                request.EndDate
            );

            if (request.IsActive && !banner.IsActive)
                banner.Activate();
            else if (!request.IsActive && banner.IsActive)
                banner.Deactivate();

            _bannerRepository.Update(banner);
            await _bannerRepository.SaveChangesAsync();
            
            InvalidateCache();
        }

        public async Task DeleteAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                throw new DomainException("Không tìm thấy banner");

            _bannerRepository.Delete(banner);
            await _bannerRepository.SaveChangesAsync();
            
            InvalidateCache();
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null) return false;

            banner.Activate();
            _bannerRepository.Update(banner);
            await _bannerRepository.SaveChangesAsync();
            
            InvalidateCache();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null) return false;

            banner.Deactivate();
            _bannerRepository.Update(banner);
            await _bannerRepository.SaveChangesAsync();
            
            InvalidateCache();
            return true;
        }

        private void InvalidateCache()
        {
            _cache.Remove(BannersCacheKey);
            // Remove all position-based cache entries
            var positions = new[] { "HomeTop", "HomeMiddle", "HomeBottom", "ProductPage" };
            foreach (var position in positions)
            {
                _cache.Remove(BannersByPositionCacheKey + position);
            }
        }

        private BannerResponse MapToResponse(Banner banner)
        {
            return new BannerResponse
            {
                Id = banner.Id,
                Title = banner.Title,
                Subtitle = banner.Subtitle,
                ImageUrl = banner.ImageUrl.Value,
                LinkUrl = banner.LinkUrl?.Value,
                Position = banner.Position,
                SortOrder = banner.SortOrder,
                StartDate = banner.StartDate,
                EndDate = banner.EndDate,
                IsActive = banner.IsActive,
                ClickCount = banner.ClickCount,
                IsVisible = banner.IsVisible(),
                CreatedAt = banner.CreatedAt
            };
        }
    }
}
