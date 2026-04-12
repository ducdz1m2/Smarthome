using Domain.Abstractions;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Catalog;

/// <summary>
/// ProductImage entity - represents a product image.
/// </summary>
public class ProductImage : Entity
    {
        public int ProductId { get; private set; }
        public string Url { get; private set; } = string.Empty;
        public string? AltText { get; private set; }
        public bool IsMain { get; private set; } = false;
        public int SortOrder { get; private set; } = 0;

        // Navigation
        public virtual Product Product { get; private set; } = null!;

        private ProductImage() { } // EF Core

        public static ProductImage Create(int productId, WebsiteUrl url, string? altText = null, bool isMain = false, int sortOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(url?.Value))
                throw new ValidationException(nameof(url), "URL hình ảnh không được trống");

            return new ProductImage
            {
                ProductId = productId,
                Url = url.Value,
                AltText = altText?.Trim(),
                IsMain = isMain,
                SortOrder = sortOrder
            };
        }

        // Legacy overload for backward compatibility
        public static ProductImage Create(int productId, string url, string? altText = null, bool isMain = false, int sortOrder = 0)
        {
            var websiteUrl = WebsiteUrl.Create(url);
            if (websiteUrl == null)
                throw new ArgumentException("URL không được để trống", nameof(url));
            return Create(productId, websiteUrl, altText, isMain, sortOrder);
        }

        public void SetAsMain()
        {
            IsMain = true;
        }

        public void SetAsSecondary()
        {
            IsMain = false;
        }

        public void UpdateSortOrder(int newOrder)
        {
            SortOrder = newOrder;
        }

        public void UpdateAltText(string? altText)
        {
            AltText = altText?.Trim();
        }
    }
