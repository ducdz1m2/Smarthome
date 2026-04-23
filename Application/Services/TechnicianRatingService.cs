using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Installation;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services
{
    public class TechnicianRatingService : ITechnicianRatingService
    {
        private readonly ITechnicianRatingRepository _ratingRepository;
        private readonly ITechnicianProfileRepository _technicianRepository;
        private readonly IInstallationBookingRepository _bookingRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly Domain.Repositories.IUserRepository _userRepository;

        public TechnicianRatingService(
            ITechnicianRatingRepository ratingRepository,
            ITechnicianProfileRepository technicianRepository,
            IInstallationBookingRepository bookingRepository,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            IEmailService emailService,
            Domain.Repositories.IUserRepository userRepository)
        {
            _ratingRepository = ratingRepository;
            _technicianRepository = technicianRepository;
            _bookingRepository = bookingRepository;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _emailService = emailService;
            _userRepository = userRepository;
        }

        public async Task<List<TechnicianRatingResponse>> GetAllAsync()
        {
            var ratings = await _ratingRepository.GetAllAsync();
            return ratings.Select(MapToResponse).ToList();
        }

        public async Task<List<TechnicianRatingResponse>> GetByTechnicianAsync(int technicianId)
        {
            var ratings = await _ratingRepository.GetByTechnicianAsync(technicianId);
            return ratings.Select(MapToResponse).ToList();
        }

        public async Task<List<TechnicianRatingResponse>> GetByUserAsync(int userId)
        {
            var ratings = await _ratingRepository.GetByUserAsync(userId);
            return ratings.Select(MapToResponse).ToList();
        }

        public async Task<List<TechnicianRatingResponse>> GetByBookingAsync(int bookingId)
        {
            var ratings = await _ratingRepository.GetByBookingAsync(bookingId);
            return ratings.Select(MapToResponse).ToList();
        }

        public async Task<TechnicianRatingResponse?> GetByTechnicianAndBookingAsync(int technicianId, int bookingId)
        {
            var rating = await _ratingRepository.GetByTechnicianAndBookingAsync(technicianId, bookingId);
            return rating == null ? null : MapToResponse(rating);
        }

        public async Task<List<TechnicianRatingResponse>> GetPendingApprovalAsync()
        {
            var ratings = await _ratingRepository.GetPendingApprovalAsync();
            return ratings.Select(MapToResponse).ToList();
        }

        public async Task<TechnicianRatingResponse?> GetByIdAsync(int id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            return rating == null ? null : MapToResponse(rating);
        }

        public async Task<int> CountAsync()
        {
            return await _ratingRepository.CountAsync();
        }

        public async Task<int> CountPendingAsync()
        {
            return await _ratingRepository.CountPendingAsync();
        }

        public async Task<TechnicianRatingResponse> CreateAsync(CreateTechnicianRatingRequest request)
        {
            // Check if user has already rated this technician for this booking
            var existing = await _ratingRepository.GetByTechnicianAndBookingAsync(request.TechnicianId, request.BookingId);
            if (existing != null)
                throw new Exception("Bạn đã đánh giá kỹ thuật viên này cho lịch lắp đặt này");

            var rating = TechnicianRating.Create(
                request.TechnicianId,
                request.UserId,
                request.BookingId,
                request.Content,
                request.Rating,
                request.IsVerifiedService);

            await _ratingRepository.AddAsync(rating);
            await _ratingRepository.SaveChangesAsync();

            // Update installation booking's CustomerRating
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking != null)
            {
                booking.SetCustomerRating(request.Rating);
                _bookingRepository.Update(booking);
                await _bookingRepository.SaveChangesAsync();
            }

            // Update technician's rating if approved
            if (rating.IsApproved)
            {
                var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId);
                if (technician != null)
                {
                    technician.CompleteJob(request.Rating);
                    _technicianRepository.Update(technician);
                    await _technicianRepository.SaveChangesAsync();
                }
            }

            // Reload with navigation properties for mapping
            var created = await _ratingRepository.GetByIdAsync(rating.Id);
            return MapToResponse(created!);
        }

        public async Task<TechnicianRatingResponse> UpdateAsync(int id, CreateTechnicianRatingRequest request)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            if (rating == null)
                throw new Exception("Không tìm thấy đánh giá");

            // Store old rating for technician update
            var oldRating = rating.Rating;

            rating.UpdateContent(request.Content);
            rating.UpdateRating(request.Rating);

            _ratingRepository.Update(rating);
            await _ratingRepository.SaveChangesAsync();

            // Update installation booking's CustomerRating
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking != null)
            {
                booking.SetCustomerRating(request.Rating);
                _bookingRepository.Update(booking);
                await _bookingRepository.SaveChangesAsync();
            }

            // Update technician's rating if previously approved
            if (oldRating != request.Rating)
            {
                var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId);
                if (technician != null)
                {
                    // Recalculate rating based on all approved ratings
                    var allApprovedRatings = await _ratingRepository.GetByTechnicianAsync(request.TechnicianId);
                    var approvedRatings = allApprovedRatings.Where(r => r.IsApproved).ToList();

                    if (approvedRatings.Any())
                    {
                        var averageRating = approvedRatings.Average(r => r.Rating);
                        // Update technician rating directly
                        technician.GetType().GetProperty("Rating")?.SetValue(technician, averageRating);
                        _technicianRepository.Update(technician);
                        await _technicianRepository.SaveChangesAsync();
                    }
                }
            }

            // Reload with navigation properties for mapping
            var updated = await _ratingRepository.GetByIdAsync(id);
            return MapToResponse(updated!);
        }

        public async Task ApproveAsync(int id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            if (rating == null) return;

            rating.Approve();
            _ratingRepository.Update(rating);
            await _ratingRepository.SaveChangesAsync();

            // Update technician's rating
            var technician = await _technicianRepository.GetByIdAsync(rating.TechnicianId);
            if (technician != null)
            {
                technician.CompleteJob(rating.Rating);
                _technicianRepository.Update(technician);
                await _technicianRepository.SaveChangesAsync();
            }

            // Send notification to user
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = rating.UserId,
                UserType = UserType.Customer,
                Type = NotificationType.InstallationScheduled,
                Title = "Đánh giá kỹ thuật viên đã được duyệt",
                Message = "Đánh giá kỹ thuật viên của bạn đã được duyệt và hiển thị công khai.",
                ActionUrl = $"/installations/{rating.BookingId}",
                Icon = "check-circle",
                RelatedEntityId = rating.Id,
                RelatedEntityType = "TechnicianRating"
            });

            // Send email
            var user = await _userRepository.GetByIdAsync(rating.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendRatingApprovedEmailAsync(user.Email, "TechnicianRating", rating.Id);
            }
        }

        public async Task RejectAsync(int id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            if (rating == null) return;

            rating.Reject();
            _ratingRepository.Update(rating);
            await _ratingRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            if (rating == null) return;

            _ratingRepository.Delete(rating);
            await _ratingRepository.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            await _ratingRepository.DeleteByIdAsync(id);
        }

        private TechnicianRatingResponse MapToResponse(TechnicianRating rating)
        {
            return new TechnicianRatingResponse
            {
                Id = rating.Id,
                TechnicianId = rating.TechnicianId,
                TechnicianName = rating.Technician?.FullName ?? $"Kỹ thuật viên #{rating.TechnicianId}",
                UserId = rating.UserId,
                UserName = _currentUserService.FullName ?? $"User #{rating.UserId}",
                BookingId = rating.BookingId,
                OrderNumber = rating.Booking?.Order?.OrderNumber ?? $"Booking #{rating.BookingId}",
                Content = rating.Content,
                Rating = rating.Rating,
                IsApproved = rating.IsApproved,
                IsVerifiedService = rating.IsVerifiedService,
                CreatedAt = rating.CreatedAt
            };
        }
    }
}
