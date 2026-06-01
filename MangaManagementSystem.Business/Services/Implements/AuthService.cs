using AutoMapper;
using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace MangaManagementSystem.Business.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IJwtTokenService jwtTokenService,
            IConfiguration configuration,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
            _mapper = mapper;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLower();
            var userName = request.UserName.Trim();

            var existed = await _userRepository.GetAll()
                .AnyAsync(x => x.Email.ToLower() == email || x.UserName == userName);

            if (existed)
                throw new Exception("Email hoặc username đã tồn tại.");

            var roleExists = await _roleRepository.GetAll()
                .AnyAsync(x => x.RoleId == request.RoleId);

            if (!roleExists)
                throw new Exception("Role không tồn tại.");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                UserName = userName,
                Email = email,
                DisplayName = request.DisplayName.Trim(),
                RoleId = request.RoleId,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangeAsync();

            var createdUser = await _userRepository.GetAll()
                .Include(x => x.Role)
                .FirstAsync(x => x.UserId == user.UserId);

            return _mapper.Map<AuthResponse>(createdUser);
        }

        public async Task<(AuthResponse User, string AccessToken, string RefreshToken)> LoginAsync(LoginRequest request)
        {
            var input = request.Email.Trim().ToLower();

            var user = await _userRepository.GetAll()
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.Email.ToLower() == input);


            if (user == null)
                throw new Exception("Tài khoản hoặc mật khẩu không đúng.");

            if (user.Status != "Active")
                throw new Exception("Tài khoản đang bị khóa hoặc không hoạt động.");

            var verifyResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

            if (verifyResult == PasswordVerificationResult.Failed)
                throw new Exception("Tài khoản hoặc mật khẩu không đúng.");

            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            user.RefreshTokenHash = _jwtTokenService.HashToken(refreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenDays"]!)
            );
            user.LastLoginAt = DateTime.UtcNow;

            await _userRepository.SaveChangeAsync();

            return (_mapper.Map<AuthResponse>(user), accessToken, refreshToken);
        }

        public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            var refreshTokenHash = _jwtTokenService.HashToken(refreshToken);

            var user = await _userRepository.GetAll()
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.RefreshTokenHash == refreshTokenHash &&
                    x.RefreshTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
                throw new Exception("Refresh token không hợp lệ hoặc đã hết hạn.");

            if (user.Status != "Active")
                throw new Exception("Tài khoản không hoạt động.");

            var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            user.RefreshTokenHash = _jwtTokenService.HashToken(newRefreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenDays"]!)
            );

            await _userRepository.SaveChangeAsync();

            return (newAccessToken, newRefreshToken);
        }

        public async Task LogoutAsync(Guid userId)
        {
            var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                return;

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;

            await _userRepository.SaveChangeAsync();
        }

        public async Task<AuthResponse> GetCurrentUserAsync(Guid userId)
        {
            var user = await _userRepository.GetAll()
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                throw new Exception("User không tồn tại.");

            return _mapper.Map<AuthResponse>(user);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                throw new Exception("User không tồn tại.");

            if (user.Status != "Active")
                throw new Exception("Tài khoản không hoạt động.");

            var verifyResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.CurrentPassword
            );

            if (verifyResult == PasswordVerificationResult.Failed)
                throw new Exception("Mật khẩu hiện tại không đúng.");

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

            // Invalidate refresh token — user must log in again with the new password
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;

            await _userRepository.SaveChangeAsync();
        }

    }
}
