using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Manuscripts;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class ManuscriptProfile : Profile
    {
        public ManuscriptProfile()
        {
            CreateMap<Manuscript, ManuscriptResponse>()
                .ForMember(dest => dest.ReviewedBy,
                    opt => opt.MapFrom(src => src.Reviewer != null ? src.Reviewer.DisplayName : null));
        }
    }
}
