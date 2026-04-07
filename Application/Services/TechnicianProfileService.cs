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

        public TechnicianProfileService(ITechnicianProfileRepository technicianRepository)
        {
            _technicianRepository = technicianRepository;
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

        public async Task<int> CreateAsync(CreateTechnicianProfileRequest request)
        {
            // Check if employee code already exists
            if (await _technicianRepository.ExistsByEmployeeCodeAsync(request.EmployeeCode))
                throw new DomainException("Mã nhân viên đã tồn tại");

            /* TODO: Create User account for technician
             * This requires IUserRepository and user creation logic
             * For now, create technician without user link (UserId = null)
             * The user can be linked later via LinkToUser method
             */

            // Create Technician Profile
            var technician = TechnicianProfile.Create(
                request.FullName,
                request.PhoneNumber,
                request.EmployeeCode,
                request.City,
                request.Districts,
                request.Email,
                request.IdentityCard,
                request.Address,
                request.DateOfBirth,
                request.BaseSalary
            );
            
            // Note: For now, UserId remains null. To complete user creation:
            // 1. Create IUserRepository interface
            // 2. Implement UserRepository 
            // 3. Inject IUserRepository into this service
            // 4. Create AppUser with request.Username and request.Password
            // 5. Link technician to user: technician.LinkToUser(user.Id);

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
                technician.UpdateInfo(
                    request.FullName ?? technician.FullName,
                    request.PhoneNumber ?? technician.PhoneNumber,
                    request.Email ?? technician.Email,
                    request.IdentityCard ?? technician.IdentityCard,
                    request.Address ?? technician.Address,
                    request.DateOfBirth ?? technician.DateOfBirth
                );
            }

            // Update work info
            if (request.BaseSalary.HasValue || request.City != null || request.Districts != null)
            {
                technician.UpdateWorkInfo(
                    request.BaseSalary ?? technician.BaseSalary,
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
                PhoneNumber = technician.PhoneNumber,
                Email = technician.Email,
                IdentityCard = technician.IdentityCard,
                Address = technician.Address,
                DateOfBirth = technician.DateOfBirth,
                EmployeeCode = technician.EmployeeCode,
                HireDate = technician.HireDate,
                BaseSalary = technician.BaseSalary,
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
                PhoneNumber = technician.PhoneNumber,
                Email = technician.Email,
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
