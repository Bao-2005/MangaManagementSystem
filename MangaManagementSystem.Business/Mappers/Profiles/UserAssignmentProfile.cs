using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Users;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class UserAssignmentProfile : Profile
    {
        public UserAssignmentProfile()
        {
            CreateMap<UserAssignment, UserAssignmentResponse>()
                .ForMember(dest => dest.FromUserName,
                    opt => opt.MapFrom(src => src.FromUser.DisplayName))
                .ForMember(dest => dest.ToUserName,
                    opt => opt.MapFrom(src => src.ToUser.DisplayName));
        }
    }
}
