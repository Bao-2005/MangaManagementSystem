using AutoMapper;
using MangaManagementSystem.Business.DTOs.Requests.Auth;
using MangaManagementSystem.Business.DTOs.Responses.Auth;
using MangaManagementSystem.Business.DTOs.Responses.Users;
using MangaManagementSystem.Business.Services.Interfaces.Auth;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
//
namespace MangaManagementSystem.Business.Services.Implements.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<DataAccess.Entities.Models.Role> _roleRepository;
        private readonly IRepository<UserAssignment> _userAssignmentRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly string _supabaseUrl;

        public AuthService(
            IRepository<User> userRepository,
            IRepository<DataAccess.Entities.Models.Role> roleRepository,
            IRepository<UserAssignment> userAssignmentRepository,
            IJwtTokenService jwtTokenService,
            IConfiguration configuration,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userAssignmentRepository = userAssignmentRepository;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
            _mapper = mapper;
            _passwordHasher = new PasswordHasher<User>();
            _supabaseUrl = (configuration["Supabase:Url"] ?? string.Empty).TrimEnd('/');
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLower();
            var userName = request.UserName.Trim();

            var existed = await _userRepository.GetAll()
                .AnyAsync(x => x.Email.ToLower() == email || x.UserName == userName);

            if (existed)
                throw new InvalidOperationException("Email hoặc username đã tồn tại.");

            var role = await _roleRepository.GetAll()
                .FirstOrDefaultAsync(x => x.RoleId == request.RoleId);

            if (role == null)
                throw new KeyNotFoundException("Role không tồn tại.");

            var isMangaka = string.Equals(role.RoleName, UserRole.Mangaka.ToString(), StringComparison.OrdinalIgnoreCase);

            var user = new User
            {
                UserName = userName,
                Email = email,
                DisplayName = request.DisplayName.Trim(),
                RoleId = role.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            UserAssignment? userAssignment = null;

            if (isMangaka && !request.AssignedFromUserId.HasValue)
                throw new ArgumentException("Tantou Editor là bắt buộc khi tạo tài khoản Mangaka.");

            if (request.AssignedFromUserId.HasValue)
            {
                var assignedFromUser = await _userRepository.GetAll()
                    .Include(x => x.Role)
                    .FirstOrDefaultAsync(x => x.UserId == request.AssignedFromUserId.Value);

                if (assignedFromUser == null)
                    throw new KeyNotFoundException("Người dùng được gán không tồn tại.");

                if (isMangaka && !string.Equals(assignedFromUser.Role.RoleName, UserRole.TantouEditor.ToString(), StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Người dùng được chọn không phải Tantou Editor.");

                if (assignedFromUser.DeletedAt != null)
                    throw new InvalidOperationException("Người dùng được gán đang bị khóa hoặc không hoạt động.");

                userAssignment = new UserAssignment
                {
                    FromUserId = assignedFromUser.UserId,
                    ToUser = user,
                    AssignedAt = DateTime.UtcNow,
                    UnassignedAt = null
                };
            }

            await _userRepository.AddAsync(user);

            if (userAssignment != null)
                await _userAssignmentRepository.AddAsync(userAssignment);

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
                throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không đúng.");

            if (user.DeletedAt != null)
                throw new UnauthorizedAccessException("Tài khoản đang bị khóa hoặc không hoạt động.");

            var verifyResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

            if (verifyResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không đúng.");

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
                throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");

            if (user.DeletedAt != null)
                throw new UnauthorizedAccessException("Tài khoản không hoạt động.");

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

        public async Task<UserProfileResponse> GetCurrentUserAsync(Guid userId)
        {
            var user = await _userRepository.GetAll()
                .Include(x => x.Role)
                .Include(x => x.AvatarFileAsset)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                throw new KeyNotFoundException("User không tồn tại.");

            return MapProfile(user, null, _supabaseUrl);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                throw new KeyNotFoundException("User không tồn tại.");

            if (user.DeletedAt != null)
                throw new UnauthorizedAccessException("Tài khoản không hoạt động.");

            var verifyResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.CurrentPassword
            );

            if (verifyResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng.");

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

            // Invalidate refresh token — user must log in again with the new password
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;

            await _userRepository.SaveChangeAsync();
        }
        private static UserProfileResponse MapProfile(
            User user,
            AssignedEditorProjection? assignedEditor,
            string supabaseUrl)
        {
            return new UserProfileResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                RoleName = user.Role.RoleName,
                AssignedEditorId = assignedEditor == null ? null : assignedEditor.FromUserId,
                AssignedEditorName = assignedEditor == null ? null : assignedEditor.DisplayName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                DeletedAt = user.DeletedAt,
                AvatarFileAssetId = user.AvatarFileAssetId,
                AvatarUrl = BuildAvatarUrl(user.AvatarFileAsset, supabaseUrl),
            };
        }
        private sealed class AssignedEditorProjection
        {
            public Guid FromUserId { get; set; }
            public string DisplayName { get; set; } = null!;
        }
        private static string? BuildAvatarUrl(FileAsset? avatarFileAsset, string supabaseUrl)
        {
            if (avatarFileAsset is null || avatarFileAsset.DeletedAt != null || string.IsNullOrEmpty(supabaseUrl))
                return null;

            return $"{supabaseUrl}/storage/v1/object/public/{avatarFileAsset.BucketName}/{avatarFileAsset.ObjectPath}";
        }
    }
}
