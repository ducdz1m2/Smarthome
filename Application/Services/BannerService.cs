using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Content;
using Domain.Exceptions;

namespace Application.Services
{
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _bannerRepository;

        public BannerService(IBannerRepository bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        public async Task<List<BannerResponse>> GetAllAsync()
        {
            var banners = await _bannerRepository.GetAllAsync();
            return banners.Select(MapToResponse).ToList();
        }

        public async Task<BannerResponse?> GetByIdAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null) return null;
            return MapToResponse(banner);
        }

        public async Task<List<BannerResponse>> GetByPositionAsync(string position)
        {
            var banners = await _bannerRepository.GetByPositionAsync(position);
            return banners.Select(MapToResponse).ToList();
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
        }

        public async Task DeleteAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                throw new DomainException("Không tìm thấy banner");

            _bannerRepository.Delete(banner);
            await _bannerRepository.SaveChangesAsync();
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null) return false;

            banner.Activate();
            _bannerRepository.Update(banner);
            await _bannerRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null) return false;

            banner.Deactivate();
            _bannerRepository.Update(banner);
            await _bannerRepository.SaveChangesAsync();
            return true;
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
