using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Identity;
using Domain.Exceptions;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<List<UserResponse>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var result = new List<UserResponse>();

            foreach (var user in users)
            {
                var userWithRoles = await _userRepository.GetByIdWithRolesAsync(user.Id);
                result.Add(MapToResponse(userWithRoles!));
            }

            return result;
        }

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(id);
            if (user == null) return null;
            return MapToResponse(user);
        }

        public async Task<UserResponse?> GetByUserNameAsync(string userName)
        {
            var user = await _userRepository.GetByUserNameAsync(userName);
            if (user == null) return null;
            var userWithRoles = await _userRepository.GetByIdWithRolesAsync(user.Id);
            return MapToResponse(userWithRoles!);
        }

        public async Task<int> CreateAsync(CreateUserRequest request)
        {
            if (await _userRepository.ExistsAsync(request.UserName))
                throw new DomainException("Tên đăng nhập đã tồn tại");

            if (await _userRepository.ExistsEmailAsync(request.Email))
                throw new DomainException("Email đã được sử dụng");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = AppUser.Create(
                request.UserName,
                request.Email,
                request.FullName,
                passwordHash,
                "bcrypt",
                request.PhoneNumber
            );

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Reload user to properly track it, then assign roles
            var savedUser = await _userRepository.GetByIdWithRolesAsync(user.Id);
            foreach (var roleName in request.Roles)
            {
                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role != null && savedUser != null)
                {
                    savedUser.AssignRole(role);
                }
            }

            // Save again if roles were added
            if (request.Roles.Any())
            {
                await _userRepository.SaveChangesAsync();
            }

            return user.Id;
        }

        public async Task UpdateAsync(UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(request.Id);
            if (user == null)
                throw new DomainException("Không tìm thấy người dùng");

            // Update basic info using reflection (private setters)
            typeof(AppUser).GetProperty("Email")?.SetValue(user, request.Email.ToLower());
            typeof(AppUser).GetProperty("FullName")?.SetValue(user, request.FullName);
            typeof(AppUser).GetProperty("PhoneNumber")?.SetValue(user, request.PhoneNumber);
            typeof(AppUser).GetProperty("Avatar")?.SetValue(user, request.Avatar);

            if (request.IsActive)
                user.Activate();
            else
                user.Deactivate();

            // Update roles
            var currentRoles = user.Roles.Select(r => r.Name).ToList();
            var rolesToRemove = currentRoles.Except(request.Roles).ToList();
            var rolesToAdd = request.Roles.Except(currentRoles).ToList();

            foreach (var roleName in rolesToRemove)
            {
                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role != null)
                    user.RemoveRole(role);
            }

            foreach (var roleName in rolesToAdd)
            {
                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role != null)
                    user.AssignRole(role);
            }

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new DomainException("Không tìm thấy người dùng");

            _userRepository.Delete(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task ActivateAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new DomainException("Không tìm thấy người dùng");

            user.Activate();
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new DomainException("Không tìm thấy người dùng");

            user.Deactivate();
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task ResetPasswordAsync(int id, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new DomainException("Không tìm thấy người dùng");

            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            typeof(AppUser).GetProperty("PasswordHash")?.SetValue(user, newPasswordHash);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task AssignRoleAsync(int userId, string roleName)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            if (user == null)
                throw new DomainException("Không tìm thấy người dùng");

            var role = await _roleRepository.GetByNameAsync(roleName);
            if (role == null)
                throw new DomainException("Không tìm thấy vai trò");

            user.AssignRole(role);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task RemoveRoleAsync(int userId, string roleName)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            if (user == null)
                throw new DomainException("Không tìm thấy người dùng");

            var role = await _roleRepository.GetByNameAsync(roleName);
            if (role == null)
                throw new DomainException("Không tìm thấy vai trò");

            user.RemoveRole(role);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        private static UserResponse MapToResponse(AppUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                Roles = user.Roles.Select(r => r.Name).ToList()
            };
        }
    }
}
