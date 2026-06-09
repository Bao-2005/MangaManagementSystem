using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Tasks;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class AnnotationProfile : Profile
    {
        public AnnotationProfile()
        {
            CreateMap<Annotation, AnnotationResponse>()
                .ForMember(dest => dest.AuthorName,
                    opt => opt.MapFrom(src => src.Author.DisplayName))
                .ForMember(dest => dest.ChapterId,
                    opt => opt.MapFrom(src => src.Manuscript.ChapterId));
        }
    }
}
