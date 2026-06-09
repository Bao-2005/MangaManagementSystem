using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Auth;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, AuthResponse>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));
        }
    }
}
