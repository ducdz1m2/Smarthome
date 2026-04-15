using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Services;
using Domain.Entities.Identity;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        #region User Management

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserResponse>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(MapToUserResponse(user, roles));
            }

            return result;
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapToUserResponse(user, roles);
        }

        public async Task<UserResponse?> GetUserByNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapToUserResponse(user, roles);
        }

        public async Task<(IdentityResult Result, int UserId)> CreateUserAsync(CreateUserRequest request)
        {
            if (await UserExistsAsync(request.UserName))
                return (IdentityResult.Failed(new IdentityError { Description = "Tên đăng nhập đã tồn tại" }), 0);

            if (await EmailExistsAsync(request.Email))
                return (IdentityResult.Failed(new IdentityError { Description = "Email đã được sử dụng" }), 0);

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email.ToLower(),
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return (result, 0);

            // Assign roles
            foreach (var roleName in request.Roles)
            {
                // Create role if it doesn't exist
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    };
                    await _roleManager.CreateAsync(role);
                }
                await _userManager.AddToRoleAsync(user, roleName);
            }

            return (result, user.Id);
        }

        public async Task<IdentityResult> UpdateUserAsync(UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

            user.Email = request.Email.ToLower();
            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.Avatar = request.Avatar;
            user.IsActive = request.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return result;

            // Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(request.Roles).ToList();
            var rolesToAdd = request.Roles.Except(currentRoles).ToList();

            if (rolesToRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

            foreach (var roleName in rolesToAdd)
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityResult> ActivateUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

            user.IsActive = true;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeactivateUserAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

            user.IsActive = false;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(int id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IdentityResult> AssignRoleAsync(int userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

            if (!await _roleManager.RoleExistsAsync(roleName))
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy vai trò" });

            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> RemoveRoleAsync(int userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

            return await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public async Task<bool> UserExistsAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName) != null;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        #endregion

        #region Role Management

        public async Task<List<RoleResponse>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return roles.Select(MapToRoleResponse).ToList();
        }

        public async Task<RoleResponse?> GetRoleByIdAsync(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return null;

            return MapToRoleResponse(role);
        }

        public async Task<RoleResponse?> GetRoleByNameAsync(string name)
        {
            var role = await _roleManager.FindByNameAsync(name);
            if (role == null) return null;

            return MapToRoleResponse(role);
        }

        public async Task<IdentityResult> CreateRoleAsync(string name, string? description = null)
        {
            var role = new ApplicationRole
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            return await _roleManager.CreateAsync(role);
        }

        public async Task<IdentityResult> UpdateRoleAsync(int id, string name, string? description = null)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy vai trò" });

            role.Name = name;
            role.Description = description;

            return await _roleManager.UpdateAsync(role);
        }

        public async Task<IdentityResult> DeleteRoleAsync(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy vai trò" });

            return await _roleManager.DeleteAsync(role);
        }

        public async Task<bool> RoleExistsAsync(string name)
        {
            return await _roleManager.RoleExistsAsync(name);
        }

        #endregion

        #region Authentication

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
                throw new DomainException("Tên đăng nhập hoặc mật khẩu không đúng");

            if (!user.IsActive)
                throw new DomainException("Tài khoản đã bị vô hiệu hóa");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                throw new DomainException("Tên đăng nhập hoặc mật khẩu không đúng");

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            return GenerateAuthResponse(user, roles);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate
            if (request.Password != request.ConfirmPassword)
                throw new DomainException("Mật khẩu xác nhận không khớp");

            if (request.Password.Length < 6)
                throw new DomainException("Mật khẩu phải có ít nhất 6 ký tự");

            // Check existing
            if (await UserExistsAsync(request.UserName))
                throw new DomainException("Tên đăng nhập đã tồn tại");

            if (await EmailExistsAsync(request.Email))
                throw new DomainException("Email đã được sử dụng");

            // Create user
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email.ToLower(),
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new DomainException(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Assign default role (Customer)
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = "Customer",
                    Description = "Khách hàng",
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _userManager.AddToRoleAsync(user, "Customer");

            var roles = await _userManager.GetRolesAsync(user);
            return GenerateAuthResponse(user, roles);
        }

        public async Task<IdentityResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<UserResponse?> GetCurrentUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapToUserResponse(user, roles);
        }

        #endregion

        #region Private Methods

        private AuthResponse GenerateAuthResponse(ApplicationUser user, IList<string> roles)
        {
            var token = GenerateJwtToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                User = MapToUserResponse(user, roles)
            };
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKey12345678901234567890");
            var issuer = _configuration["Jwt:Issuer"] ?? "SmarthomeApp";
            var audience = _configuration["Jwt:Audience"] ?? "SmarthomeClient";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.GivenName, user.FullName)
            };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private static UserResponse MapToUserResponse(ApplicationUser user, IList<string> roles)
        {
            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                Roles = roles.ToList()
            };
        }

        private static RoleResponse MapToRoleResponse(ApplicationRole role)
        {
            return new RoleResponse
            {
                Id = role.Id,
                Name = role.Name ?? "",
                Description = role.Description
            };
        }

        #endregion
    }
}
