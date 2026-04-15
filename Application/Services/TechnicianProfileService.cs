using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Installation;
using Domain.Exceptions;

namespace Application.Services
{
    public class TechnicianProfileService : ITechnicianProfileService
    {
        private readonly ITechnicianProfileRepository _technicianRepository;
        private readonly IIdentityService _identityService;

        public TechnicianProfileService(ITechnicianProfileRepository technicianRepository, IIdentityService identityService)
        {
            _technicianRepository = technicianRepository;
            _identityService = identityService;
        }

        public async Task<List<TechnicianListResponse>> GetAllAsync()
        {
            var technicians = await _technicianRepository.GetAllAsync();
            return technicians.Select(MapToListResponse).ToList();
        }

        public async Task<TechnicianResponse?> GetByIdAsync(int id)
        {
            var technician = await _technicianRepository.GetByIdAsync(id);
            if (technician == null) return null;
            return MapToResponse(technician);
        }

        public async Task<TechnicianResponse?> GetByUserIdAsync(int userId)
        {
            var technician = await _technicianRepository.GetByUserIdAsync(userId);
            if (technician == null) return null;
            return MapToResponse(technician);
        }

        public async Task<(List<TechnicianListResponse> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, bool? isAvailable = null, string? search = null)
        {
            var (items, totalCount) = await _technicianRepository.GetPagedAsync(page, pageSize, isAvailable, search);
            return (items.Select(MapToListResponse).ToList(), totalCount);
        }

        public async Task<List<TechnicianResponse>> GetAvailableAsync()
        {
            var technicians = await _technicianRepository.GetAvailableAsync();
            return technicians.Select(MapToResponse).ToList();
        }

        public async Task<List<TechnicianResponse>> GetByDistrictAsync(string district)
        {
            var technicians = await _technicianRepository.GetByDistrictAsync(district);
            return technicians.Select(MapToResponse).ToList();
        }

        public async Task<List<TechnicianResponse>> GetByCityAsync(string city)
        {
            var technicians = await _technicianRepository.GetByCityAsync(city);
            return technicians.Select(MapToResponse).ToList();
        }

        public async Task<int> CreateAsync(CreateTechnicianProfileRequest request)
        {
            // Check if employee code already exists
            if (await _technicianRepository.ExistsByEmployeeCodeAsync(request.EmployeeCode))
                throw new DomainException("Mã nhân viên đã tồn tại");

            Console.WriteLine($"[TechnicianProfileService] Creating user account for technician: {request.Username}");

            // Create User account for technician
            var createUserRequest = new CreateUserRequest
            {
                UserName = request.Username,
                Password = request.Password,
                Email = request.Email ?? string.Empty,
                FullName = request.FullName,
                Roles = new List<string> { "Technician" }
            };

            var (result, userId) = await _identityService.CreateUserAsync(createUserRequest);
            if (!result.Succeeded)
            {
                Console.WriteLine($"[TechnicianProfileService] User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new DomainException($"Không thể tạo tài khoản: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            Console.WriteLine($"[TechnicianProfileService] User created successfully with ID: {userId}");

            // Create Technician Profile
            // For address, use a simple format: street only (no ward/district validation)
            var address = !string.IsNullOrWhiteSpace(request.Address)
                ? Domain.ValueObjects.Address.Create(request.Address, null, "N/A", "N/A")
                : null;

            var technician = TechnicianProfile.Create(
                request.FullName,
                Domain.ValueObjects.PhoneNumber.Create(request.PhoneNumber),
                request.EmployeeCode,
                request.City,
                request.Districts,
                request.Email != null ? Domain.ValueObjects.Email.Create(request.Email) : null,
                request.IdentityCard,
                address,
                request.DateOfBirth,
                request.BaseSalary > 0 ? Domain.ValueObjects.Money.Vnd(request.BaseSalary) : null
            );

            // Link technician to user
            technician.LinkToUser(userId);

            // Add skills if provided
            foreach (var skill in request.Skills)
            {
                technician.AddSkill(skill);
            }

            await _technicianRepository.AddAsync(technician);
            await _technicianRepository.SaveChangesAsync();

            return technician.Id;
        }

        public async Task UpdateAsync(int id, UpdateTechnicianProfileRequest request)
        {
            var technician = await _technicianRepository.GetByIdAsync(id);
            if (technician == null)
                throw new DomainException("Không tìm thấy kỹ thuật viên");

            // Update personal info
            if (request.FullName != null || request.PhoneNumber != null)
            {
                var address = request.Address != null
                    ? Domain.ValueObjects.Address.Create(request.Address, null, "N/A", "N/A")
                    : technician.Address;

                technician.UpdateInfo(
                    request.FullName ?? technician.FullName,
                    request.PhoneNumber != null ? Domain.ValueObjects.PhoneNumber.Create(request.PhoneNumber) : technician.PhoneNumber,
                    request.Email != null ? Domain.ValueObjects.Email.Create(request.Email) : technician.Email,
                    request.IdentityCard ?? technician.IdentityCard,
                    address,
                    request.DateOfBirth ?? technician.DateOfBirth
                );
            }

            // Update work info
            if (request.BaseSalary.HasValue || request.City != null || request.Districts != null)
            {
                technician.UpdateWorkInfo(
                    request.BaseSalary.HasValue ? Domain.ValueObjects.Money.Vnd(request.BaseSalary.Value) : technician.BaseSalary,
                    request.City ?? technician.City,
                    request.Districts ?? technician.GetDistricts()
                );
            }

            if (request.IsAvailable.HasValue)
            {
                technician.SetAvailable(request.IsAvailable.Value);
            }

            _technicianRepository.Update(technician);
            await _technicianRepository.SaveChangesAsync();
        }

        public async Task AddSkillAsync(int id, AddTechnicianSkillRequest request)
        {
            var technician = await _technicianRepository.GetByIdAsync(id);
            if (technician == null)
                throw new DomainException("Không tìm thấy kỹ thuật viên");

            technician.AddSkill(request.Skill);
            await _technicianRepository.SaveChangesAsync();
        }

        public async Task SetAvailableAsync(int id, bool available)
        {
            var technician = await _technicianRepository.GetByIdAsync(id);
            if (technician == null)
                throw new DomainException("Không tìm thấy kỹ thuật viên");

            technician.SetAvailable(available);
            await _technicianRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var technician = await _technicianRepository.GetByIdAsync(id);
            if (technician == null)
                throw new DomainException("Không tìm thấy kỹ thuật viên");

            _technicianRepository.Delete(technician);
            await _technicianRepository.SaveChangesAsync();
        }

        private TechnicianResponse MapToResponse(TechnicianProfile technician)
        {
            return new TechnicianResponse
            {
                Id = technician.Id,
                UserId = technician.UserId,
                FullName = technician.FullName,
                PhoneNumber = technician.PhoneNumber.ToString(),
                Email = technician.Email?.ToString(),
                IdentityCard = technician.IdentityCard,
                Address = technician.Address?.ToString(),
                DateOfBirth = technician.DateOfBirth,
                EmployeeCode = technician.EmployeeCode,
                HireDate = technician.HireDate,
                BaseSalary = technician.BaseSalary.Amount,
                City = technician.City,
                Districts = technician.GetDistricts(),
                Skills = technician.GetSkills(),
                IsAvailable = technician.IsAvailable,
                Rating = technician.Rating,
                CompletedJobs = technician.CompletedJobs,
                CancelledJobs = technician.CancelledJobs
            };
        }

        private TechnicianListResponse MapToListResponse(TechnicianProfile technician)
        {
            return new TechnicianListResponse
            {
                Id = technician.Id,
                EmployeeCode = technician.EmployeeCode,
                FullName = technician.FullName,
                PhoneNumber = technician.PhoneNumber.ToString(),
                Email = technician.Email?.ToString(),
                City = technician.City,
                Districts = technician.GetDistricts(),
                Rating = technician.Rating,
                CompletedJobs = technician.CompletedJobs,
                CancelledJobs = technician.CancelledJobs,
                IsAvailable = technician.IsAvailable
            };
        }
    }
}
