using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class EscalationProfile : Profile
    {
        public EscalationProfile()
        {
            CreateMap<Escalation, EscalationResponse>()
                .ForMember(dest => dest.CreatedBy,
                    opt => opt.MapFrom(src => src.Creator.DisplayName))
                .ForMember(dest => dest.ResolvedBy,
                    opt => opt.MapFrom(src => src.Resolver != null ? src.Resolver.DisplayName : null));
        }
    }
}
