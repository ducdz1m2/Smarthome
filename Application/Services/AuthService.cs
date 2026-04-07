using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Identity;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUserNameAsync(request.UserName);
            if (user == null)
                throw new DomainException("Tên đăng nhập hoặc mật khẩu không đúng");

            if (!user.IsActive)
                throw new DomainException("Tài khoản đã bị vô hiệu hóa");

            // Verify password with BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new DomainException("Tên đăng nhập hoặc mật khẩu không đúng");

            // Mark login
            user.MarkLogin();
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            // Get user with roles
            var userWithRoles = await _userRepository.GetByIdWithRolesAsync(user.Id);

            return GenerateAuthResponse(userWithRoles!);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate
            if (request.Password != request.ConfirmPassword)
                throw new DomainException("Mật khẩu xác nhận không khớp");

            if (request.Password.Length < 6)
                throw new DomainException("Mật khẩu phải có ít nhất 6 ký tự");

            // Check existing
            if (await _userRepository.ExistsAsync(request.UserName))
                throw new DomainException("Tên đăng nhập đã tồn tại");

            if (await _userRepository.ExistsEmailAsync(request.Email))
                throw new DomainException("Email đã được sử dụng");

            // Hash password with BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            string passwordSalt = "bcrypt"; // BCrypt includes salt in the hash

            // Create user
            var user = AppUser.Create(
                request.UserName,
                request.Email,
                request.FullName,
                passwordHash,
                passwordSalt,
                request.PhoneNumber
            );

            // Assign default role (Customer)
            var customerRole = await _roleRepository.GetByNameAsync("Customer");
            if (customerRole == null)
            {
                // Create Customer role if not exists
                customerRole = AppRole.Create("Customer", "Khách hàng");
                await _roleRepository.AddAsync(customerRole);
                await _roleRepository.SaveChangesAsync();
            }
            user.AssignRole(customerRole);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Get user with roles
            var userWithRoles = await _userRepository.GetByIdWithRolesAsync(user.Id);

            return GenerateAuthResponse(userWithRoles!);
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
        {
            // Validate refresh token logic here
            // For simplicity, we'll just return null for now
            return null;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new DomainException("Không tìm thấy người dùng");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                throw new DomainException("Mật khẩu hiện tại không đúng");

            // Hash new password
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Update password using reflection (since properties are private)
            typeof(AppUser).GetProperty("PasswordHash")?.SetValue(user, newPasswordHash);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public Task LogoutAsync(int userId)
        {
            // In a real implementation, you might invalidate the token here
            // For JWT, this is typically handled by token expiration or a blacklist
            return Task.CompletedTask;
        }

        public async Task<UserResponse?> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            if (user == null) return null;

            return MapToUserResponse(user);
        }

        private AuthResponse GenerateAuthResponse(AppUser user)
        {
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                User = MapToUserResponse(user)
            };
        }

        private string GenerateJwtToken(AppUser user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKey12345678901234567890");
            var issuer = _configuration["Jwt:Issuer"] ?? "SmarthomeApp";
            var audience = _configuration["Jwt:Audience"] ?? "SmarthomeClient";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FullName)
            };

            // Add roles as claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
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

        private static UserResponse MapToUserResponse(AppUser user)
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
