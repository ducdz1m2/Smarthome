namespace Domain.Entities.Content;

using Domain.Abstractions;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// Banner entity - represents a promotional banner.
/// </summary>
public class Banner : Entity
    {
        public string Title { get; private set; } = string.Empty;
        public string? Subtitle { get; private set; }
        public WebsiteUrl ImageUrl { get; private set; } = null!;
        public WebsiteUrl? LinkUrl { get; private set; }
        public string Position { get; private set; } = "HomeTop"; // HomeTop, HomeMiddle, ProductPage, etc.
        public int SortOrder { get; private set; } = 0;
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public bool IsActive { get; private set; } = true;
        public int ClickCount { get; private set; } = 0;

        private Banner() { }

        public static Banner Create(string title, WebsiteUrl imageUrl, string? subtitle = null, WebsiteUrl? linkUrl = null,
            string position = "HomeTop", int sortOrder = 0, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException(nameof(title), "Tiêu đề không được trống");

            if (string.IsNullOrWhiteSpace(imageUrl?.Value))
                throw new ValidationException(nameof(imageUrl), "URL hình ảnh không được trống");

            return new Banner
            {
                Title = title.Trim(),
                Subtitle = subtitle?.Trim(),
                ImageUrl = imageUrl,
                LinkUrl = linkUrl,
                Position = position.Trim(),
                SortOrder = sortOrder,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                ClickCount = 0
            };
        }

        public void Update(string title, string? subtitle, WebsiteUrl? linkUrl, string position, int sortOrder,
            DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException(nameof(title), "Tiêu đề không được trống");

            Title = title.Trim();
            Subtitle = subtitle?.Trim();
            LinkUrl = linkUrl;
            Position = position.Trim();
            SortOrder = sortOrder;
            StartDate = startDate;
            EndDate = endDate;
        }

        public void UpdateImage(WebsiteUrl imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl?.Value))
                throw new ValidationException(nameof(imageUrl), "URL hình ảnh không được trống");

            ImageUrl = imageUrl;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void IncrementClick()
        {
            ClickCount++;
        }

        public bool IsVisible()
        {
            if (!IsActive) return false;
            var now = DateTime.UtcNow;
            if (StartDate.HasValue && now < StartDate.Value) return false;
            if (EndDate.HasValue && now > EndDate.Value) return false;
            return true;
        }
    }
