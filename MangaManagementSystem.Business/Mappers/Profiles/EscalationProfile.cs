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
                // Guid identity fields — map directly from entity
                .ForMember(dest => dest.CreatedBy,
                    opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.ResolvedBy,
                    opt => opt.MapFrom(src => src.ResolvedBy))
                // Human-readable display names from navigation properties
                .ForMember(dest => dest.CreatorName,
                    opt => opt.MapFrom(src => src.Creator != null ? src.Creator.DisplayName : string.Empty))
                .ForMember(dest => dest.ResolverName,
                    opt => opt.MapFrom(src => src.Resolver != null ? src.Resolver.DisplayName : null));
        }
    }
}
